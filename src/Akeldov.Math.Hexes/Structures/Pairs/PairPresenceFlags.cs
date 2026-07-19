using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Identifies which positions of a <see cref="PartialPair{TValue}"/> contain values.
    /// </summary>
    [Flags]
    public enum PairPresenceFlags : byte
    {
        /// <summary>
        /// Neither position is present.
        /// </summary>
        None = 0,
        /// <summary>
        /// The left position is present.
        /// </summary>
        Left = 1 << 0,
        /// <summary>
        /// The right position is present.
        /// </summary>
        Right = 1 << 1,
        /// <summary>
        /// Both positions are present.
        /// </summary>
        All = Left | Right
    }
}
