using CompMs.Common.DataStructure;
using MessagePack; // Added
using System.Collections.Generic;
using System.Linq;

namespace CompMs.Common.Lipidomics
{
    [MessagePackObject] // Added
    public class PositionLevelChains : SeparatedChains, ITotalChain
    {
        // This field duplicates data from base.InnerChains but is used by this class's GetChainByPosition.
        // It needs to be repopulated upon deserialization.
        [IgnoreMember] // Added - This will be repopulated by the [SerializationConstructor]
        private readonly IChain[] _chains;

        // Existing constructor for application logic
        public PositionLevelChains(params IChain[] chains)
            // Assuming ChainInformation.Position should be 1-indexed if GetChainByPosition uses position - 1
            : base(chains.Select((c, i) => ValueTuple.Create(c, i + 1)).ToArray(), LipidDescription.Class | LipidDescription.Chain | LipidDescription.SnPosition) {
            _chains = chains;
        }

        // Constructor for MessagePack deserialization
        [SerializationConstructor] // Added
        public PositionLevelChains(ChainInformation[] innerChains, LipidDescription description) : base(innerChains, description) {
            // Repopulate _chains based on the deserialized InnerChains from the base class.
            // Ensure correct ordering if InnerChains are not guaranteed to be sorted by Position.
            // And that Position is 1-indexed as expected by GetChainByPosition.
            var chainList = new List<IChain>();
            if (innerChains != null) {
                foreach (var chainInfo in innerChains.OrderBy(ci => ci.Position)) {
                    // Assuming Position is 1-based and continuous for PositionLevelChains
                    // Fill potential gaps with null if positions are not continuous, though less likely for PositionLevelChains
                    while(chainList.Count < chainInfo.Position -1 && chainInfo.Position > 0) {
                        chainList.Add(null); // Or handle error if non-continuous positions are not allowed
                    }
                    if (chainInfo.Position > 0) {
                         if (chainList.Count == chainInfo.Position -1) {
                            chainList.Add(chainInfo.Chain);
                         } else {
                             // Handle error or sparse positions if necessary
                         }
                    } else {
                        // Handle chains with undetermined positions if they can exist in PositionLevelChains
                        // For now, assume all chains in PositionLevelChains have determined, positive positions.
                    }
                }
            }
            _chains = chainList.ToArray();
            // A simpler way if InnerChains is guaranteed to be dense and ordered by position:
            // _chains = innerChains?.Select(ci => ci.Chain).ToArray() ?? System.Array.Empty<IChain>();
            // The current base constructor for PositionLevelChains (params IChain[] chains) implies a dense array.
            // So, the simpler way is likely correct if innerChains reflects that.
            // The (IChain, int)[] constructor for SeparatedChains takes the position as is.
            // The chains.Select((c, i) => ValueTuple.Create(c, i + 1)) ensures positions are 1, 2, 3...
            // So, sorting by position and selecting chain should be fine.
            // _chains = innerChains?.OrderBy(ci => ci.Position).Select(ci => ci.Chain).ToArray() ?? System.Array.Empty<IChain>();
        }

        IChain ITotalChain.GetChainByPosition(int position) {
            if (position > _chains.Length) {
                return null;
            }
            return _chains[position - 1];
        }

        IEnumerable<ITotalChain> ITotalChain.GetCandidateSets(ITotalChainVariationGenerator totalChainGenerator) {
            return totalChainGenerator.Product(this);
        }
        private readonly ITotalChainVariationGenerator totalChainGenerator;
        public override string ToString() {
            return string.Join("/", GetDeterminedChains().Select(c => c.ToString()));
        }

        bool ITotalChain.Includes(ITotalChain chains) {
            return chains.ChainCount == ChainCount && chains is PositionLevelChains pChains
                && Enumerable.Range(0, ChainCount).All(i => GetDeterminedChains()[i].Includes(pChains.GetDeterminedChains()[i]));
        }

        public override bool Equals(ITotalChain other) {
            return other is PositionLevelChains pChains
                && ChainCount == other.ChainCount
                && CarbonCount == other.CarbonCount
                && DoubleBondCount == other.DoubleBondCount
                && OxidizedCount == other.OxidizedCount
                && Description == other.Description
                && GetDeterminedChains().Zip(pChains.GetDeterminedChains(), (a, b) => a.Equals(b)).All(p => p);
        }

        public override TResult Accept<TResult>(IAcyclicVisitor visitor, IAcyclicDecomposer<TResult> decomposer) {
            if (decomposer is IDecomposer<TResult, PositionLevelChains> decomposer_) {
                return decomposer_.Decompose(visitor, this);
            }
            return default;
        }
    }
}
