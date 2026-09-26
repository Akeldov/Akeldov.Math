namespace Akeldov.Math.Hexes
{
    internal static class HexMapBinaryFormat
    {
        // The little-endian UInt32 representation of the four ASCII bytes HMAP.
        internal const uint Signature = 0x50414D48;
        internal const byte Version = 1;
        internal const byte TopologyMapKind = 0;
        internal const byte BooleanValueKind = 1;
    }
}
