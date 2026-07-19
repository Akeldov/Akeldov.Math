using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Identifies which positions of a <see cref="PartialSeptuplet{TValue}"/> contain values.
    /// </summary>
    [Flags]
    public enum SeptupletPresenceFlags : byte
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
        /// Adjacent position 0 is present.
        /// </summary>
        Adjacent0 = 1 << 1,
        /// <summary>
        /// Adjacent position 1 is present.
        /// </summary>
        Adjacent1 = 1 << 2,
        /// <summary>
        /// Adjacent position 2 is present.
        /// </summary>
        Adjacent2 = 1 << 3,
        /// <summary>
        /// Adjacent position 3 is present.
        /// </summary>
        Adjacent3 = 1 << 4,
        /// <summary>
        /// Adjacent position 4 is present.
        /// </summary>
        Adjacent4 = 1 << 5,
        /// <summary>
        /// Adjacent position 5 is present.
        /// </summary>
        Adjacent5 = 1 << 6,
        /// <summary>
        /// The main and all six adjacent positions are present.
        /// </summary>
        All = Main | Adjacent0 | Adjacent1 | Adjacent2 | Adjacent3 | Adjacent4 | Adjacent5
    }
}
