using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Identifies which chromatic-index positions are present in a partial chromatic triplet.
    /// </summary>
    [Flags]
    public enum ChromaticTripletPresenceFlags : byte
    {
        /// <summary>
        /// No chromatic-index positions are present.
        /// </summary>
        None = 0,

        /// <summary>
        /// The position for chromatic index zero is present.
        /// </summary>
        Index0 = 1 << 0,

        /// <summary>
        /// The position for chromatic index one is present.
        /// </summary>
        Index1 = 1 << 1,

        /// <summary>
        /// The position for chromatic index two is present.
        /// </summary>
        Index2 = 1 << 2,

        /// <summary>
        /// All chromatic-index positions are present.
        /// </summary>
        All = Index0 | Index1 | Index2
    }
}
