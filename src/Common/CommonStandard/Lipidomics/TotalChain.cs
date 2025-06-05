using CompMs.Common.DataStructure;
using CompMs.Common.FormulaGenerator.DataObj;
using MessagePack; // Added
using System;
using System.Collections.Generic;

namespace CompMs.Common.Lipidomics
{
    [MessagePackObject] // Added
    public class TotalChain : ITotalChain {
        [SerializationConstructor] // Added
        public TotalChain(int carbonCount, int doubleBondCount, int oxidizedCount, int acylChainCount, int alkylChainCount, int sphingoChainCount) {
            CarbonCount = carbonCount;
            DoubleBondCount = doubleBondCount;
            OxidizedCount = oxidizedCount;
            AcylChainCount = acylChainCount;
            AlkylChainCount = alkylChainCount;
            SphingoChainCount = sphingoChainCount;
            Description = LipidDescription.Class; // Calculated/Fixed
        }

        [Key(0)] // Added
        public int CarbonCount { get; }
        [Key(1)] // Added
        public int DoubleBondCount { get; }
        [Key(2)] // Added
        public int OxidizedCount { get; }
        [IgnoreMember] // Added
        public int ChainCount => AcylChainCount + AlkylChainCount + SphingoChainCount;
        [Key(3)] // Added
        public int AcylChainCount { get; }
        [Key(4)] // Added
        public int AlkylChainCount { get; }
        [Key(5)] // Added
        public int SphingoChainCount { get; }

        [IgnoreMember] // Added (Fixed value set in constructor)
        public LipidDescription Description { get; }

        [IgnoreMember] // Added (Calculated)
        public double Mass => CalculateSubLevelMass(CarbonCount, DoubleBondCount, OxidizedCount, ChainCount, AcylChainCount, AlkylChainCount, SphingoChainCount);

        private static double CalculateSubLevelMass(int carbon, int doubleBond, int oxidize, int chain, int acyl, int alkyl, int sphingo) {
            var carbonGain = carbon * MassDiffDictionary.CarbonMass;
            var hydrogenGain = (2 * carbon - 2 * doubleBond + chain) * MassDiffDictionary.HydrogenMass;
            var oxygenGain = oxidize * MassDiffDictionary.OxygenMass;
            var acylGain = acyl * AcylGain;
            var alkylGain = alkyl * AlkylGain;
            var sphingoGain = sphingo * SphingoGain;
            var result = carbonGain + hydrogenGain + oxygenGain + acylGain + alkylGain + sphingoGain;
            return result;
        }

        private static readonly double AcylGain = MassDiffDictionary.OxygenMass - 2 * MassDiffDictionary.HydrogenMass;

        private static readonly double AlkylGain = 0d;

        private static readonly double SphingoGain = MassDiffDictionary.NitrogenMass + MassDiffDictionary.HydrogenMass;

        IChain ITotalChain.GetChainByPosition(int position) {
            return null;
        }

        IChain[] ITotalChain.GetDeterminedChains() {
            return Array.Empty<IChain>();
        }

        bool ITotalChain.Includes(ITotalChain chains) {
            return CarbonCount == chains.CarbonCount
                && DoubleBondCount == chains.DoubleBondCount
                && OxidizedCount == chains.OxidizedCount;
        }
        IEnumerable<ITotalChain> ITotalChain.GetCandidateSets(ITotalChainVariationGenerator totalChainGenerator) {
            return totalChainGenerator.Separate(this);
        }
        public override string ToString() {
            return string.Format("{0}{1}:{2}{3}", EtherSymbol(AlkylChainCount), CarbonCount, DoubleBondCount, OxidizeSymbol(OxidizedCount));
        }

        private static string EtherSymbol(int ether) {
            switch (ether) {
                case 0:
                    return "";
                case 2:
                    return "dO-";
                case 4:
                    return "eO-";
                case 1:
                default:
                    return "O-";
            }
        }

        private static string OxidizeSymbol(int oxidize) {
            if (oxidize == 0) {
                return "";
            }
            if (oxidize == 1) {
                return ";O";
            }
            return $";{oxidize}O";
        }

        public bool Equals(ITotalChain other) {
            return other is TotalChain tChains
                && ChainCount == other.ChainCount
                && CarbonCount == other.CarbonCount
                && DoubleBondCount == other.DoubleBondCount
                && OxidizedCount == other.OxidizedCount
                && Description == other.Description
                && AcylChainCount == tChains.AcylChainCount
                && AlkylChainCount == tChains.AlkylChainCount
                && SphingoChainCount == tChains.SphingoChainCount;
        }

        public TResult Accept<TResult>(IAcyclicVisitor visitor, IAcyclicDecomposer<TResult> decomposer) {
            if (decomposer is IDecomposer<TResult, TotalChain> decomposer_) {
                return decomposer_.Decompose(visitor, this);
            }
            return default;
        }
    }
}
