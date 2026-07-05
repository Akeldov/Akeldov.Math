using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Defines PairPresenceFlags values.
    /// </summary>
    [Flags]
    public enum PairPresenceFlags : byte
    {
        /// <summary>
        /// Represents the <c>None</c> value.
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents the <c>Left</c> value.
        /// </summary>
        Left = 1 << 0,
        /// <summary>
        /// Represents the <c>Right</c> value.
        /// </summary>
        Right = 1 << 1,
        /// <summary>
        /// Represents the <c>All</c> value.
        /// </summary>
        All = Left | Right
    }
}

