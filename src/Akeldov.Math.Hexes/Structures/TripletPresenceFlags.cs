using System;

namespace Akeldov.Math.Hexes.Topology
{
    [Flags]
    public enum TripletPresenceFlags : byte
    {
        None = 0,
        Main = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        All = Main | Left | Right
    }
}
