using CompMs.Common.Enum;
using CompMs.Common.Parser;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Concurrent;

namespace CompMs.Common.DataObj.Property
{
    /// <summary>
    /// This is the storage of adduct ion information.
    /// </summary>
    [MessagePackObject]
    [MessagePackFormatter(typeof(AdductIonFormatter))]
    public class AdductIon
    {
        /// <summary>
        /// Initializes a new instance of the AdductIon class.
        /// <para>
        /// This constructor is preserved for use with MessagePack for C#, and direct usage is deprecated. If used as a default value,
        /// consider using the AdductIon.Default property. If you know the AdductIonName, consider using the AdductIon.GetAdductIon method.
        /// </para>
        /// </summary>
        [SerializationConstructor]
        [Obsolete("Use GetAdductIon static method.")]
        public AdductIon() { }

        [Key(0)]
        public double AdductIonAccurateMass { get; set; }

        public double ConvertToMz(double exactMass) {
            double precursorMz = (exactMass * AdductIonXmer + AdductIonAccurateMass) / ChargeNumber;
            if (IonMode == IonMode.Positive) {
                precursorMz -= 0.0005485799 * ChargeNumber;
            }
            else {
                precursorMz += 0.0005485799 * ChargeNumber;
            }
            return precursorMz;
        }

        public double ConvertToExactMass(double mz) {
            double monoIsotopicMass = (mz * ChargeNumber - AdductIonAccurateMass) / AdductIonXmer;
            if (IonMode == IonMode.Positive) {
                monoIsotopicMass += 0.0005485799 * ChargeNumber;
            }
            else {
                monoIsotopicMass -= 0.0005485799 * ChargeNumber;
            }
            return monoIsotopicMass;
        }

        [Key(1)]
        public int AdductIonXmer { get; set; }
        [Key(2)]
        public string AdductIonName { get; set; } = string.Empty;
        [Key(3)]
        public int ChargeNumber { get; set; }
        [Key(4)]
        public IonMode IonMode { get; set; }
        [Key(5)]
        public bool FormatCheck { get; set; }
        [Key(6)]
        public double M1Intensity { get; set; }
        [Key(7)]
        public double M2Intensity { get; set; }
        [Key(8)]
        public bool IsRadical { get; set; }
        [Key(9)]
        public bool IsIncluded { get; set; } // used for applications

        [IgnoreMember]
        public bool HasAdduct => !string.IsNullOrEmpty(AdductIonName);
        [IgnoreMember]
        public bool IsFA => AdductIonName == "[M+HCOO]-" || AdductIonName == "[M+FA-H]-";
        [IgnoreMember]
        public bool IsHac => AdductIonName == "[M+CH3COO]-" || AdductIonName == "[M+Hac-H]-";
        [IgnoreMember]
        public bool IsHco3 => AdductIonName == "[M+HCO3]-";

        public override string ToString() {
            return AdductIonName;
        }

        /// <summary>
        /// This method returns the AdductIon class variable from the adduct string.
        /// </summary>
        /// <param name="adductName">Add the formula string such as "C6H12O6"</param>
        /// <returns>AdductIon</returns>
        public static AdductIon GetAdductIon(string? adductName) {
            return ADDUCT_IONS.GetOrAdd(adductName ?? string.Empty);
        }

        private static AdductIon GetAdductIonCore(string adductName) {
#pragma warning disable CS0618 // Type or member is obsolete
            AdductIon adduct = new AdductIon() { AdductIonName = adductName };
#pragma warning restore CS0618 // Type or member is obsolete

            if (!AdductIonParser.IonTypeFormatChecker(adductName)) {
                adduct.FormatCheck = false;
                return adduct;
            }

            var chargeNum = AdductIonParser.GetChargeNumber(adductName);
            if (chargeNum == -1) {
                adduct.FormatCheck = false;
                return adduct;
            }

            var adductIonXmer = AdductIonParser.GetAdductIonXmer(adductName);
            var ionType = AdductIonParser.GetIonType(adductName);
            var isRadical = AdductIonParser.GetRadicalInfo(adductName);

            (var accurateMass, var m1Intensity, var m2Intensity) = AdductIonParser.CalculateAccurateMassAndIsotopeRatio(adduct.AdductIonName);
            adduct.AdductIonAccurateMass += accurateMass;
            adduct.M1Intensity += m1Intensity;
            adduct.M2Intensity += m2Intensity;

            adduct.AdductIonXmer = adductIonXmer;
            adduct.ChargeNumber = chargeNum;
            adduct.FormatCheck = true;
            adduct.IonMode = ionType;
            adduct.IsRadical = isRadical;

            return adduct;
        }

        public static AdductIon GetStandardAdductIon(int charge, IonMode ionMode) {
            switch (ionMode) {
                case IonMode.Positive:
                    if (charge >= 2)
                        return GetAdductIon($"[M+{charge}H]{charge}+");
                    else
                        return GetAdductIon("[M+H]+");
                case IonMode.Negative:
                    if (charge >= 2)
                        return GetAdductIon($"[M-{charge}H]{charge}-");
                    else
                        return GetAdductIon("[M-H]-");
                default:
                    return Default;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public static readonly AdductIon Default = new AdductIon();
#pragma warning restore CS0618 // Type or member is obsolete

        private static readonly AdductIons ADDUCT_IONS = new AdductIons();

        class AdductIons {
            private readonly ConcurrentDictionary<string, AdductIon> _dictionary;
            public AdductIons() {
                _dictionary = new ConcurrentDictionary<string, AdductIon>();
                _dictionary.TryAdd(Default.AdductIonName, Default);
                var hac = GetAdductIonCore("[M+CH3COO]-");
                _dictionary.TryAdd("[M+CH3COO]-", hac);
                _dictionary.TryAdd("[M+Hac-H]-", hac);
                var fa = GetAdductIonCore("[M+HCOO]-");
                _dictionary.TryAdd("[M+HCOO]-", fa);
                _dictionary.TryAdd("[M+FA-H]-", fa);
            }

            public AdductIon GetOrAdd(string adduct) {
                return _dictionary.GetOrAdd(adduct, GetAdductIonCore);
            }
        }

        class AdductIonFormatter : IMessagePackFormatter<AdductIon>
        {
            private const int ArrayLength = 10; // Corresponds to [Key(0)] through [Key(9)]

            public void Serialize(ref MessagePackWriter writer, AdductIon value, MessagePackSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNil();
                    return;
                }

                writer.WriteArrayHeader(ArrayLength);
                writer.Write(value.AdductIonAccurateMass); // Key 0
                writer.Write(value.AdductIonXmer);        // Key 1
                writer.Write(value.AdductIonName);        // Key 2
                writer.Write(value.ChargeNumber);         // Key 3
                options.Resolver.GetFormatterWithVerify<IonMode>().Serialize(ref writer, value.IonMode, options); // Key 4
                writer.Write(value.FormatCheck);          // Key 5
                writer.Write(value.M1Intensity);          // Key 6
                writer.Write(value.M2Intensity);          // Key 7
                writer.Write(value.IsRadical);            // Key 8
                writer.Write(value.IsIncluded);           // Key 9
            }

            public AdductIon Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            {
                if (reader.TryReadNil())
                {
                    return AdductIon.Default;
                }

                var count = reader.ReadArrayHeader();
                // The custom serializer (via DynamicObjectResolver) always wrote 10 fields.
                // The old custom deserializer was trying to be flexible but could misinterpret.
                // This new version expects the 10 fields consistently.
                if (count != ArrayLength)
                {
                     throw new MessagePackSerializationException($"Invalid AdductIon array length: {count}. Expected {ArrayLength}.");
                }

                // Read all 10 fields to advance the reader correctly
                var adductAccurateMass = reader.ReadDouble();     // Key 0
                var adductIonXmer = reader.ReadInt32();              // Key 1
                var adductName = reader.ReadString();             // Key 2
                var chargeNumber = reader.ReadInt32();            // Key 3
                var ionMode = options.Resolver.GetFormatterWithVerify<IonMode>().Deserialize(ref reader, options); // Key 4
                var formatCheck = reader.ReadBoolean();           // Key 5
                var m1Intensity = reader.ReadDouble();            // Key 6
                var m2Intensity = reader.ReadDouble();            // Key 7
                var isRadical = reader.ReadBoolean();             // Key 8
                var isIncluded = reader.ReadBoolean();            // Key 9

                // Core logic from original custom formatter: use GetAdductIon, then set specific fields.
                AdductIon adduct = AdductIon.GetAdductIon(adductName);

                adduct.M1Intensity = m1Intensity;
                adduct.M2Intensity = m2Intensity;
                adduct.IsIncluded = isIncluded; // Original was |=. If this specific OR logic is critical, it should be retained.
                                                // For now, direct assignment for simplicity post-GetAdductIon.

                // The other deserialized fields (adductAccurateMass, adductIonXmer, chargeNumber, ionMode, formatCheck, isRadical)
                // are read from the stream to ensure the reader is advanced correctly.
                // However, they are NOT used to overwrite the properties of the 'adduct' object returned by 'GetAdductIon',
                // because 'GetAdductIon' is considered authoritative for these base properties.
                // This matches the selective update behavior of the original custom deserializer.

                return adduct;
            }
        }
    }


    /// <summary>
    /// this is used in LC-MS project
    /// the queries are used in PosNegAmalgamator.cs of MsdialLcmsProcess assembly
    /// </summary>
    [MessagePackObject]
    public class AdductDiff {
        public AdductDiff(AdductIon posAdduct, AdductIon negAdduct) : this(posAdduct, negAdduct, posAdduct.AdductIonAccurateMass - negAdduct.AdductIonAccurateMass) {

        }

        [SerializationConstructor]
        public AdductDiff(AdductIon posAdduct, AdductIon negAdduct, double diff) {
            PosAdduct = posAdduct;
            NegAdduct = negAdduct;
            Diff = diff;
        }

        #region // properties
        [Key(0)]
        public AdductIon PosAdduct { get; }
        [Key(1)]
        public AdductIon NegAdduct { get; }
        [Key(2)]
        public double Diff { get; } // pos - neg
        #endregion
    }
}
