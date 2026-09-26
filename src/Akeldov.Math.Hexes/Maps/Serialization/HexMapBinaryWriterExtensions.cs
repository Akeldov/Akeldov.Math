using System;
using System.IO;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Writes hex maps to versioned binary records.
    /// </summary>
    public static partial class HexMapBinaryWriterExtensions
    {
        /// <summary>
        /// Writes a Boolean map's topology and values as one version-1 binary record.
        /// </summary>
        /// <param name="writer">The writer positioned where the record should begin.</param>
        /// <param name="map">The Boolean map to serialize.</param>
        /// <remarks>
        /// <para>
        /// The record contains the four ASCII bytes <c>HMAP</c>, version byte <c>1</c>,
        /// topology-only map-kind byte <c>0</c>, Boolean value-kind byte <c>1</c>, width and
        /// height as little-endian Int32 values, and one layout byte
        /// (<c>0</c> = OddR, <c>1</c> = EvenR, <c>2</c> = OddQ, <c>3</c> = EvenQ).
        /// The 16-byte header is followed by one byte per cell: <c>0</c> for false or <c>1</c>
        /// for true. Values use row-major order, with X advancing first.
        /// </para>
        /// <para>
        /// Empty maps retain both dimensions and their layout. Spatial sources are written as
        /// topology-only maps; their origin and radius are not serialized. The source must not
        /// change during serialization. The writer and its stream remain open and are not
        /// flushed or repositioned. Records can be followed by other data or more records.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="writer"/> or <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static void Write(this BinaryWriter writer, IHexMap<bool> map)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            int count = topology.Count;

            writer.Write(HexMapBinaryFormat.Signature);
            writer.Write(HexMapBinaryFormat.Version);
            writer.Write(HexMapBinaryFormat.TopologyMapKind);
            writer.Write(HexMapBinaryFormat.BooleanValueKind);
            writer.Write(topology.Resolution.X);
            writer.Write(topology.Resolution.Y);
            writer.Write((byte)topology.Layout);

            for (int index = 0; index < count; index++)
                writer.Write(map[index]);
        }

        /// <summary>
        /// Writes an integer map's topology and values as one version-1 binary record.
        /// </summary>
        /// <param name="writer">The writer positioned where the record should begin.</param>
        /// <param name="map">The integer map to serialize.</param>
        /// <remarks>
        /// <para>
        /// The record contains the four ASCII bytes <c>HMAP</c>, version byte <c>1</c>,
        /// topology-only map-kind byte <c>0</c>, Int32 value-kind byte <c>2</c>, width and
        /// height as little-endian Int32 values, and one layout byte
        /// (<c>0</c> = OddR, <c>1</c> = EvenR, <c>2</c> = OddQ, <c>3</c> = EvenQ).
        /// The 16-byte header is followed by one signed little-endian Int32 per cell,
        /// using four bytes in two's-complement representation. Values use row-major order,
        /// with X advancing first. The entire Int32 value range is supported.
        /// </para>
        /// <para>
        /// Empty maps retain both dimensions and their layout. Spatial sources are written as
        /// topology-only maps; their origin and radius are not serialized. The source must not
        /// change during serialization. The writer and its stream remain open and are not
        /// flushed or repositioned. Records can be followed by other data or more records.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="writer"/> or <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static void Write(this BinaryWriter writer, IHexMap<int> map)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            int count = topology.Count;

            writer.Write(HexMapBinaryFormat.Signature);
            writer.Write(HexMapBinaryFormat.Version);
            writer.Write(HexMapBinaryFormat.TopologyMapKind);
            writer.Write(HexMapBinaryFormat.Int32ValueKind);
            writer.Write(topology.Resolution.X);
            writer.Write(topology.Resolution.Y);
            writer.Write((byte)topology.Layout);

            for (int index = 0; index < count; index++)
                writer.Write(map[index]);
        }

        /// <summary>
        /// Writes a floating-point map's topology and values as one version-1 binary record.
        /// </summary>
        /// <param name="writer">The writer positioned where the record should begin.</param>
        /// <param name="map">The floating-point map to serialize.</param>
        /// <remarks>
        /// <para>
        /// The record contains the four ASCII bytes <c>HMAP</c>, version byte <c>1</c>,
        /// topology-only map-kind byte <c>0</c>, Single value-kind byte <c>3</c>, width and
        /// height as little-endian Int32 values, and one layout byte
        /// (<c>0</c> = OddR, <c>1</c> = EvenR, <c>2</c> = OddQ, <c>3</c> = EvenQ).
        /// The 16-byte header is followed by one IEEE 754 binary32 value per cell, using four
        /// little-endian bytes. Values use row-major order, with X advancing first. Raw bits
        /// are preserved, including signed zero, subnormal values, infinities, and NaN payloads.
        /// </para>
        /// <para>
        /// Empty maps retain both dimensions and their layout. Spatial sources are written as
        /// topology-only maps; their origin and radius are not serialized. The source must not
        /// change during serialization. The writer and its stream remain open and are not
        /// flushed or repositioned. Records can be followed by other data or more records.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="writer"/> or <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static void Write(this BinaryWriter writer, IHexMap<float> map)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            int count = topology.Count;

            writer.Write(HexMapBinaryFormat.Signature);
            writer.Write(HexMapBinaryFormat.Version);
            writer.Write(HexMapBinaryFormat.TopologyMapKind);
            writer.Write(HexMapBinaryFormat.SingleValueKind);
            writer.Write(topology.Resolution.X);
            writer.Write(topology.Resolution.Y);
            writer.Write((byte)topology.Layout);

            for (int index = 0; index < count; index++)
                writer.Write(BitConverter.SingleToInt32Bits(map[index]));
        }
    }
}
