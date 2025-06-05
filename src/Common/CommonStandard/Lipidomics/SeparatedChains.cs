using CompMs.Common.DataStructure;
using CompMs.Common.Utility;
using MessagePack; // Added
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompMs.Common.Lipidomics
{
    [MessagePackObject] // Added
    public class SeparatedChains : ITotalChain
    {
        // Backing field for Chains property, used for serialization
        [Key(0)] // Added
        public ChainInformation[] InnerChains { get; } // Renamed from _chains for clarity, used by MessagePack

        [IgnoreMember] // Added - derived from InnerChains
        private readonly ChainInformation[] _decided;
        [IgnoreMember] // Added - derived from InnerChains
        private readonly ChainInformation[] _undecided;

        // Existing constructors used by application logic
        public SeparatedChains(IChain[] chains, LipidDescription description) {
            if (chains.Length == 0) {
                throw new ArgumentException(nameof(chains));
            }
            InnerChains = chains.Select(c => new ChainInformation(c)).ToArray();
            _decided = Array.Empty<ChainInformation>();
            _undecided = InnerChains;
            if (chains.All(c => c.DoubleBond.UnDecidedCount == 0)) {
                description |= LipidDescription.DoubleBondPosition;
            }
            Description = description;
        }

        public SeparatedChains((IChain, int)[] chains, LipidDescription description) {
            if (chains.Length == 0) {
                throw new ArgumentException(nameof(chains));
            }
            InnerChains = chains.Select(c => new ChainInformation(c.Item1, c.Item2)).ToArray();
            _decided = InnerChains.Where(c => c.Position >= 0).ToArray();
            _undecided = InnerChains.Where(c => c.Position < 0).ToArray();
            if (chains.All(c => c.Item1.DoubleBond.UnDecidedCount == 0)) {
                description |= LipidDescription.DoubleBondPosition;
            }
            Description = description;
        }

        // Constructor for MessagePack deserialization
        [SerializationConstructor] // Added
        public SeparatedChains(ChainInformation[] innerChains, LipidDescription description) {
            InnerChains = innerChains ?? throw new ArgumentNullException(nameof(innerChains));
            _decided = InnerChains.Where(c => c.Position >= 0).ToArray();
            _undecided = InnerChains.Where(c => c.Position < 0).ToArray();
            Description = description;
        }


        IChain ITotalChain.GetChainByPosition(int position) {
            return InnerChains.FirstOrDefault(c => c.Position == position)?.Chain;
        }

        public IChain[] GetDeterminedChains() {
            return InnerChains.Select(c => c.Chain).ToArray();
        }

        [IgnoreMember] // Added
        public int ChainCount => InnerChains.Length;
        [IgnoreMember] // Added
        public int AcylChainCount => InnerChains.Count(c => c.Chain is AcylChain);
        [IgnoreMember] // Added
        public int AlkylChainCount => InnerChains.Count(c => c.Chain is AlkylChain);
        [IgnoreMember] // Added
        public int SphingoChainCount => InnerChains.Count(c => c.Chain is SphingoChain);
        [IgnoreMember] // Added
        public int CarbonCount => InnerChains.Sum(c => c.Chain.CarbonCount);
        [IgnoreMember] // Added
        public int DoubleBondCount => InnerChains.Sum(c => c.Chain.DoubleBondCount);
        [IgnoreMember] // Added
        public int OxidizedCount => InnerChains.Sum(c => c.Chain.OxidizedCount);
        [Key(1)] // Added
        public LipidDescription Description { get; }
        [IgnoreMember] // Added
        public double Mass => InnerChains.Sum(c => c.Chain.Mass);

        IEnumerable<ITotalChain> ITotalChain.GetCandidateSets(ITotalChainVariationGenerator totalChainGenerator) {
            var gc = new GenerateChain(_chains.Length, _decided);
            var indetermined = _undecided.Select(c => c.Chain).ToArray();
            if (indetermined.Length > 0) {
                return SearchCollection.Permutations(indetermined).Select(cs => new PositionLevelChains(gc.Apply(cs)));
            }
            ITotalChain pc = new PositionLevelChains(_decided.Select(c => c.Chain).ToArray());
            return pc.GetCandidateSets(totalChainGenerator);
        }

        public override string ToString() {
            var box = new ChainInformation[InnerChains.Length];
            foreach (var c in _decided) {
                box[c.Position - 1] = c;
            }
            var idx = 0;
            foreach (var c in _undecided) {
                while (idx < box.Length && box[idx] != null) {
                    ++idx;
                }
                if (idx < box.Length) {
                    box[idx++] = c;
                }
            }
            return string.Concat(box.Select(c => c.Chain.ToString() + (c.Position < 0 ? "_" : "/"))).TrimEnd('_', '/');
        }

        bool ITotalChain.Includes(ITotalChain chains) {
            if (chains.ChainCount != ChainCount) {
                return false;
            }
            if (!(chains is SeparatedChains sChains)) {
                return false;
            }
            if (_decided.Length > sChains._decided.Length) { // sChains._decided would be null if SeparatedChains not changed to use InnerChains
                return false;
            }
            var used = new HashSet<ChainInformation>();
            foreach (var d in _decided) {
                // Need to access sChains.InnerChains and filter for decided ones if _decided is private
                var sChainsDecided = sChains.InnerChains.Where(sc => sc.Position >=0);
                if (sChainsDecided.FirstOrDefault(d2 => d2.Position == d.Position) is ChainInformation ci && d.Chain.Includes(ci.Chain)) {
                    used.Add(ci);
                }
                else {
                    return false;
                }
            }
            var canUse = new HashSet<ChainInformation>(sChains.InnerChains.Except(used));
            foreach (var d in _undecided) {
                if (canUse.All(d2 => !d.Chain.Includes(d2.Chain))) {
                    return false;
                }
            }
            return true;
        }

        public virtual bool Equals(ITotalChain other) {
            if (other.ChainCount != ChainCount) {
                return false;
            }
            if (!(other is SeparatedChains sChains)) {
                return false;
            }
            if (_decided.Length > sChains._decided.Length) {
                return false;
            }
            var used = new HashSet<ChainInformation>();
            foreach (var d in _decided) {
                if (sChains._decided.FirstOrDefault(d2 => d2.Position == d.Position) is ChainInformation ci && d.Chain.Equals(ci.Chain)) {
                    used.Add(ci);
                }
                else {
                    return false;
                }
            }
            var canUse = new HashSet<ChainInformation>(sChains._chains.Except(used));
            foreach (var d in _undecided) {
                if (canUse.All(d2 => !d.Chain.Equals(d2.Chain))) {
                    return false;
                }
            }
            return true;
        }

        public virtual TResult Accept<TResult>(IAcyclicVisitor visitor, IAcyclicDecomposer<TResult> decomposer) {
            if (decomposer is IDecomposer<TResult, SeparatedChains> decomposer_) {
                return decomposer_.Decompose(visitor, this);
            }
            return default;
        }

        [MessagePackObject] // Added
        public class ChainInformation { // Made public
            [Key(0)] // Added
            public IChain Chain { get; }
            [Key(1)] // Added
            public int Position { get; }

            [SerializationConstructor] // Added
            public ChainInformation(IChain chain, int position) {
                Chain = chain;
                Position = position;
            }

            // This constructor is fine for application logic, MP will use the attributed one.
            public ChainInformation(IChain chain) : this(chain, -1) {
                
            }
        }

        // GenerateChain is a private helper class, does not need MP attributes unless it's part of serialized state, which it isn't.
        class GenerateChain {
            private readonly IChain[] _box;
            private readonly List<int> _remains;

            public GenerateChain(int length, IEnumerable<ChainInformation> determined) {
                _box = new IChain[length + 1];
                _remains = Enumerable.Range(1, length).ToList();
                foreach (var d in determined) {
                    _box[d.Position] = d.Chain;
                    _remains.Remove(d.Position);
                }
            }

            public IChain[] Apply(IEnumerable<IChain> chains) {
                var result = _box.ToArray();
                foreach (var (i, c) in _remains.Zip(chains, (i, c) => (i, c))) {
                    result[i] = c;
                }
                return result;
            }
        }
    }
}
