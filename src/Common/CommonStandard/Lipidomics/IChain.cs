using CompMs.Common.DataStructure;
using MessagePack; // Added
using System;
using System.Collections.Generic;

namespace CompMs.Common.Lipidomics
{
    [Union(0, typeof(AcylChain))] // Added
    [Union(1, typeof(AlkylChain))] // Added
    [Union(2, typeof(SphingoChain))] // Added
    public interface IChain : IVisitableElement, IEquatable<IChain> {
        int CarbonCount { get; }
        IDoubleBond DoubleBond { get; }
        IOxidized Oxidized { get; }
        int DoubleBondCount { get; }
        int OxidizedCount { get; }
        double Mass { get; }

        bool Includes(IChain chain);
        IEnumerable<IChain> GetCandidates(IChainGenerator generator);
    }
}
