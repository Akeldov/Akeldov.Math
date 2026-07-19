using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Identifies which positions of a <see cref="PartialTriplet{TValue}"/> contain values.
    /// </summary>
    [Flags]
    public enum TripletPresenceFlags : byte
    {
        /// <summary>
        /// No position is present.
        /// </summary>
        None = 0,
        /// <summary>
        /// The main position is present.
        /// </summary>
        Main = 1 << 0,
        /// <summary>
        /// The left position is present.
        /// </summary>
        Left = 1 << 1,
        /// <summary>
        /// The right position is present.
        /// </summary>
        Right = 1 << 2,
        /// <summary>
        /// All three positions are present.
        /// </summary>
        All = Main | Left | Right
    }
}
