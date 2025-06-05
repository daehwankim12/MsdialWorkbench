using CompMs.Common.DataStructure;
using MessagePack; // Added
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompMs.Common.Lipidomics
{
    [Union(0, typeof(Oxidized))] // Added
    public interface IOxidized : IVisitableElement, IEquatable<IOxidized>
    {
        int Count { get; }
        int DecidedCount { get; }
        int UnDecidedCount { get; }

        ReadOnlyCollection<int> Oxidises { get; }

        bool Includes(IOxidized oxidized);
    }

    [MessagePackObject] // Added
    public sealed class Oxidized : IOxidized
    {
        [SerializationConstructor] // Added
        public Oxidized(int count, IList<int> oxidises) {
            Count = count;
            Oxidises = new ReadOnlyCollection<int>(oxidises ?? new List<int>()); // Ensure oxidises is not null
        }

        // This constructor can remain for application logic, MP will use the attributed one.
        public Oxidized(int count, params int[] oxidises) : this(count, (IList<int>)oxidises) {
            // Count = count;
            // Oxidises = new ReadOnlyCollection<int>(oxidises);
        }

        [Key(0)] // Added
        public int Count { get; }

        [IgnoreMember] // Added
        public int DecidedCount => Oxidises.Count;
        [IgnoreMember] // Added
        public int UnDecidedCount => Count - DecidedCount;

        [Key(1)] // Added
        public ReadOnlyCollection<int> Oxidises { get; }

        public override string ToString() {
            if (Count == 0) {
                return "";
            }
            else if (UnDecidedCount == 0) {
                // return ";" + string.Join(",", Oxidises.Select(o => o.ToString() + "OH")); 
                return "(" + string.Join(",", Oxidises.Select(o => o.ToString() + "OH")) + ")"; // fix 20221201 MT
            }
            else if (Count == 1) {
                return ";O";
            }
            else {
                //return $";O{Count}";
                return $";O{Count}";
            }
        }

        public static Oxidized CreateFromPosition(params int[] oxidises) {
            return new Oxidized(oxidises.Length, oxidises);
        }

        public TResult Accept<TResult>(IAcyclicVisitor visitor, IAcyclicDecomposer<TResult> decomposer) {
            if (decomposer is IDecomposer<TResult, Oxidized> concrete) {
                return concrete.Decompose(visitor, this);
            }
            return default;
        }

        public bool Includes(IOxidized oxidized) {
            return Count == oxidized.Count && DecidedCount <= oxidized.DecidedCount && Oxidises.All(oxidized.Oxidises.Contains);
        }

        public bool Equals(IOxidized other) {
            return Count == other.Count && DecidedCount == other.DecidedCount &&
                Oxidises.All(ox => other.Oxidises.Any(ox.Equals));
        }
    }
}
