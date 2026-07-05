using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Defines TripletPresenceFlags values.
    /// </summary>
    [Flags]
    public enum TripletPresenceFlags : byte
    {
        /// <summary>
        /// Represents the <c>None</c> value.
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents the <c>Main</c> value.
        /// </summary>
        Main = 1 << 0,
        /// <summary>
        /// Represents the <c>Left</c> value.
        /// </summary>
        Left = 1 << 1,
        /// <summary>
        /// Represents the <c>Right</c> value.
        /// </summary>
        Right = 1 << 2,
        /// <summary>
        /// Represents the <c>All</c> value.
        /// </summary>
        All = Main | Left | Right
    }
}
