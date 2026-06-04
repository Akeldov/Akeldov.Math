using System;

namespace Akeldov.Math.Hexes.Topology
{
    [Flags]
    public enum SeptupletPresenceFlags : byte
    {
        None = 0,
        Main = 1 << 0,
        Adjacent0 = 1 << 1,
        Adjacent1 = 1 << 2,
        Adjacent2 = 1 << 3,
        Adjacent3 = 1 << 4,
        Adjacent4 = 1 << 5,
        Adjacent5 = 1 << 6,
        All = Main | Adjacent0 | Adjacent1 | Adjacent2 | Adjacent3 | Adjacent4 | Adjacent5
    }
}
