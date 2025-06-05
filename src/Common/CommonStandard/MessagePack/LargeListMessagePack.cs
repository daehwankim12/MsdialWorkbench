using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using MessagePack.LZ4;
using System.Buffers;

namespace CompMs.Common.MessagePack {
    public static class LargeListMessagePack
    {
        public static int MaxUncompressedChunkSize = 1 * 1024 * 1024; // 1MB chunks for uncompressed data
        public const sbyte ExtensionTypeCode = 99;

        private static readonly MessagePackSerializerOptions DefaultOptions =
            MessagePackSerializerOptions.Standard
                .WithResolver(StandardResolverAllowPrivate.Instance);

        public static void Serialize<T>(Stream stream, IReadOnlyList<T> value, MessagePackSerializerOptions options = null)
        {
            options = options ?? DefaultOptions;
            var formatter = options.Resolver.GetFormatterWithVerify<T>();

            if (value == null)
            {
                var nilWriter = new MessagePackWriter(stream);
                nilWriter.WriteNil();
                nilWriter.Flush();
                return;
            }

            var chunkItemsBuffer = new ArrayBufferWriter<byte>();
            var chunkDataWriter = new MessagePackWriter(chunkItemsBuffer);
            int itemsInCurrentChunk = 0;
            int totalItems = value.Count;

            for (int i = 0; i < totalItems; i++)
            {
                // Serialize item to the current chunk's buffer (indirectly, via chunkDataWriter)
                // The original code serialized item by item checking offset.
                // Here, we'll serialize to ArrayBufferWriter and check its size.
                // This is still slightly different as we serialize before checking size.
                // A more faithful approach would be to try serializing and if it exceeds, then finalize previous.
                // For now, serialize then check.

                // To know if adding this item exceeds MaxUncompressedChunkSize, we'd ideally serialize it to a temporary buffer first.
                // Or, serialize it and if chunkItemsBuffer.WrittenCount > MaxUncompressedChunkSize, then this item starts the *next* chunk.
                // This simplified version just adds and then checks if the loop should finalize.

                if (itemsInCurrentChunk == 0) { // About to add first item to a new conceptual chunk
                    //chunkDataWriter = new MessagePackWriter(chunkItemsBuffer); // Start new writer for this chunk
                }

                // Serialize item (this will be part of the current chunk being built)
                // We need to write array header before items for this chunk.
                // This logic needs to be outside the item loop, framing a list of items.

                // Corrected approach: Collect items for a chunk, then serialize that collection.
                // This loop should collect items, and the actual serialization happens when a chunk is full or all items processed.
            }

            // Revised loop structure for serialization
            int currentItemIndex = 0;
            while (currentItemIndex < totalItems)
            {
                chunkItemsBuffer.Clear(); // Reset for new chunk data
                // chunkDataWriter needs to be reset or re-associated with the cleared chunkItemsBuffer
                // For ArrayBufferWriter, new MessagePackWriter(chunkItemsBuffer) effectively does this.
                chunkDataWriter = new MessagePackWriter(chunkItemsBuffer);

                List<T> chunkSpecificItems = new List<T>();
                long currentChunkUncompressedSize = 0;

                // Start array for the current chunk
                // We don't know the count yet. This is a problem for forward-only writer.
                // Option 1: Buffer all items for the chunk, then write header and items. (Chosen)
                // Option 2: Write indefinite array header if supported, then items, then end array. (Less common for this)

                int itemsCollectedForThisChunk = 0;
                while (currentItemIndex < totalItems) {
                    // Estimate size if item is added. This is hard without actual serialization.
                    // Original code: serialize one by one to a large buffer and check offset.
                    // New approach: Collect items, serialize them all into chunkItemsBuffer, then check size.
                    // If too large, need to re-chunk, which is inefficient.
                    // Compromise: Collect a reasonable number of items or until a heuristic size is met.

                    chunkSpecificItems.Add(value[currentItemIndex]);
                    currentItemIndex++;
                    itemsCollectedForThisChunk++;

                    // Heuristic: check estimated size or item count to break chunk
                    // This is where original's `OffsetCutoff` was used against actual serialized offset.
                    // Let's use MaxUncompressedChunkSize against estimated or actual serialized size of chunkSpecificItems.
                    // For simplicity, if after adding an item, the *next* item might overflow, finalize.
                    // This still requires serializing chunkSpecificItems to know its actual size.

                    // Serialize current collected items to check size
                    var tempSizeCheckBuffer = new ArrayBufferWriter<byte>();
                    var tempSizeCheckWriter = new MessagePackWriter(tempSizeCheckBuffer);
                    tempSizeCheckWriter.WriteArrayHeader(chunkSpecificItems.Count);
                    foreach(var item in chunkSpecificItems) {
                        formatter.Serialize(ref tempSizeCheckWriter, item, options);
                    }
                    tempSizeCheckWriter.Flush();
                    currentChunkUncompressedSize = tempSizeCheckBuffer.WrittenCount;

                    if (currentChunkUncompressedSize >= MaxUncompressedChunkSize || currentItemIndex == totalItems) {
                        break; // Finalize this chunk
                    }
                }

                // Now, chunkSpecificItems contains items for the current chunk. Serialize them.
                chunkItemsBuffer.Clear();
                chunkDataWriter = new MessagePackWriter(chunkItemsBuffer);
                chunkDataWriter.WriteArrayHeader(chunkSpecificItems.Count);
                foreach (var item in chunkSpecificItems)
                {
                    formatter.Serialize(ref chunkDataWriter, item, options);
                }
                chunkDataWriter.Flush();
                var uncompressedChunkMemory = chunkItemsBuffer.WrittenMemory;

                if (uncompressedChunkMemory.Length == 0) continue; // Should not happen if chunkSpecificItems has items

                byte[] compressedChunkLz4 = LZ4Codec.Encode(uncompressedChunkMemory.ToArray());

                var mainStreamWriter = new MessagePackWriter(stream);

                // Write original uncompressed length as a fixed 4-byte big-endian integer.
                byte[] originalLengthBytes = BitConverter.GetBytes(uncompressedChunkMemory.Length);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(originalLengthBytes); // Ensure Big Endian
                }

                mainStreamWriter.WriteExtensionFormatHeader(new ExtensionHeader(ExtensionTypeCode, (uint)(originalLengthBytes.Length + compressedChunkLz4.Length)));
                // Manually write the 4-byte original length to the stream.
                // MessagePackWriter does not have a WriteFixedInt32BigEndian directly.
                // So, we write it as raw bytes to the underlying stream.
                mainStreamWriter.Flush(); // Flush writer before writing directly to stream to ensure order
                stream.Write(originalLengthBytes, 0, originalLengthBytes.Length);

                // Write compressed data
                mainStreamWriter.WriteRaw(compressedChunkLz4);
                mainStreamWriter.Flush();
            }
        }

        public static List<T> Deserialize<T>(Stream stream, MessagePackSerializerOptions options = null)
        {
            options = options ?? DefaultOptions;
            var resultList = new List<T>();

            while (stream.Position < stream.Length) // Loop as long as there's data
            {
                var chunkItems = DeserializeEachChunk<T>(stream, options);
                if (chunkItems == null) // Should ideally not happen unless stream ends unexpectedly with partial data
                {
                    break;
                }
                resultList.AddRange(chunkItems);
            }
            return resultList;
        }

        private static List<T> DeserializeEachChunk<T>(Stream stream, MessagePackSerializerOptions options)
        {
            MessagePackReader reader;

            try
            {
                // Check for NIL indicating end of list elements or an empty list
                var peekBuffer = new byte[1];
                if (stream.Read(peekBuffer, 0, 1) == 0) return null; // End of stream
                stream.Position -= 1; // Rewind

                reader = new MessagePackReader(stream);
                if (reader.TryReadNil()) return null;
            }
            catch (EndOfStreamException) { return null; }


            ExtensionHeader extHeader = reader.ReadExtensionFormatHeader();
            if (extHeader.TypeCode != ExtensionTypeCode)
            {
                throw new MessagePackSerializationException($"Invalid Extension TypeCode. Expected {ExtensionTypeCode}, got {extHeader.TypeCode}");
            }

            // Read original uncompressed length (fixed 4-byte big-endian integer)
            byte[] originalLengthBytes = new byte[4];
            int readLenBytes = stream.Read(originalLengthBytes, 0, 4); // Read directly from stream
            if (readLenBytes != 4)
            {
                throw new EndOfStreamException("Could not read original length bytes for the chunk.");
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(originalLengthBytes); // Ensure Big Endian before ToInt32
            }
            int originalUncompressedLength = BitConverter.ToInt32(originalLengthBytes, 0);

            // Calculate compressed data length
            uint compressedDataLength = extHeader.Length - 4; // 4 bytes for the original length
            if (extHeader.Length < 4) {
                 throw new MessagePackSerializationException("Extension header length is smaller than the fixed original length size.");
            }

            byte[] compressedBytes = new byte[compressedDataLength];
            int bytesRead = stream.Read(compressedBytes, 0, compressedBytes.Length); // Read directly from stream
            if (bytesRead != compressedDataLength)
            {
                throw new EndOfStreamException("Could not read all compressed bytes for the chunk.");
            }

            byte[] decompressedBytes = LZ4Codec.Decode(compressedBytes, 0, compressedBytes.Length, originalUncompressedLength);

            var itemReader = new MessagePackReader(new ReadOnlySequence<byte>(decompressedBytes));
            return DeserializeList<T>(ref itemReader, options);
        }


        public static IEnumerable<List<T>> DeserializeIncremental<T>(Stream stream, MessagePackSerializerOptions options = null)
        {
            options = options ?? DefaultOptions;
            while (stream.Position < stream.Length) // Check if there's more data
            {
                var chunk = DeserializeEachChunk<T>(stream, options);
                if (chunk == null) yield break; // Normal end or error
                yield return chunk;
            }
        }

        static void AddList<T>(List<T> original, List<T> tmp) // Unchanged
        {
            if (tmp == null) return;
            foreach (var t in tmp)
            {
                original.Add(t);
            }
        }

        static List<T> DeserializeList<T>(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            var formatter = options.Resolver.GetFormatterWithVerify<T>();
            int len = reader.ReadArrayHeader();
            var list = new List<T>(len);
            for (int i = 0; i < len; i++)
            {
                list.Add(formatter.Deserialize(ref reader, options));
            }
            return list;
        }

        public static T DeserializeAt<T>(Stream stream, int index, MessagePackSerializerOptions options = null) {
            options = options ?? DefaultOptions;
            int currentIndex = 0;
            foreach (var chunk in DeserializeIncremental<T>(stream, options)) {
                if (index < currentIndex + chunk.Count) {
                    return chunk[index - currentIndex];
                }
                currentIndex += chunk.Count;
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        // TryDeserializeAtOrSkip functionality is implicitly handled by iterating chunks
        // in DeserializeAt, or would require a more complex method signature
        // to return skipArraySize if not found in current chunk.
        // For now, not providing a direct TryDeserializeAtOrSkip replacement
        // that matches the old signature with 'out int skipArraySize'.
        // The DeserializeAt provides the core "find at index" capability.
    }
}
