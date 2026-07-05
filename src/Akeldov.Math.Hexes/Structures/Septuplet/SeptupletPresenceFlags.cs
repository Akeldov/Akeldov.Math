using System;

namespace Akeldov.Math.Hexes.Topology
{
    /// <summary>
    /// Defines SeptupletPresenceFlags values.
    /// </summary>
    [Flags]
    public enum SeptupletPresenceFlags : byte
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
        /// Represents the <c>Adjacent0</c> value.
        /// </summary>
        Adjacent0 = 1 << 1,
        /// <summary>
        /// Represents the <c>Adjacent1</c> value.
        /// </summary>
        Adjacent1 = 1 << 2,
        /// <summary>
        /// Represents the <c>Adjacent2</c> value.
        /// </summary>
        Adjacent2 = 1 << 3,
        /// <summary>
        /// Represents the <c>Adjacent3</c> value.
        /// </summary>
        Adjacent3 = 1 << 4,
        /// <summary>
        /// Represents the <c>Adjacent4</c> value.
        /// </summary>
        Adjacent4 = 1 << 5,
        /// <summary>
        /// Represents the <c>Adjacent5</c> value.
        /// </summary>
        Adjacent5 = 1 << 6,
        /// <summary>
        /// Represents the <c>All</c> value.
        /// </summary>
        All = Main | Adjacent0 | Adjacent1 | Adjacent2 | Adjacent3 | Adjacent4 | Adjacent5
    }
}
