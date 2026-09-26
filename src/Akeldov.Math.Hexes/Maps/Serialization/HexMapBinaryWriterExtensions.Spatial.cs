using Akeldov.Math.Hexes.Geometry;
using System;
using System.IO;

namespace Akeldov.Math.Hexes
{
    public static partial class HexMapBinaryWriterExtensions
    {
        /// <summary>
        /// Writes a spatial Boolean map's topology, origin, radius, and values as one version-1 binary record.
        /// </summary>
        /// <param name="writer">The writer positioned where the record should begin.</param>
        /// <param name="map">The spatial Boolean map to serialize.</param>
        /// <remarks>
        /// <para>
        /// The record contains the four ASCII bytes <c>HMAP</c>, version byte <c>1</c>,
        /// spatial map-kind byte <c>1</c>, Boolean value-kind byte <c>1</c>, width and height
        /// as little-endian Int32 values, and one layout byte
        /// (<c>0</c> = OddR, <c>1</c> = EvenR, <c>2</c> = OddQ, <c>3</c> = EvenQ).
        /// The 16-byte header is followed by Origin.X, Origin.Y, and Radius, each encoded as
        /// four little-endian IEEE 754 binary32 bytes. Their exact bits are preserved.
        /// The apothem is derived from the radius when reading.
        /// Starting at offset 28, values use row-major order with X advancing first and
        /// one byte per cell: zero for false or one for true.
        /// </para>
        /// <para>
        /// Empty maps retain their geometry and both dimensions. The source must not change
        /// during serialization and its geometry topology must equal its map topology.
        /// The writer and its stream remain open and are not flushed or repositioned.
        /// Both seekable and non-seekable streams are supported. Records can be followed by other data.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="writer"/> or <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the geometry topology differs from the map topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin is not finite, or its radius is not finite and positive.
        /// </exception>
        public static void Write(this BinaryWriter writer, ISpatialHexMap<bool> map)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            HexMapGeometry geometry = map.Geometry;
            if (geometry.Topology != topology)
                throw new ArgumentException("The geometry topology must equal the map topology.", nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex radius must be finite and positive.");

            int count = topology.Count;

            writer.Write(HexMapBinaryFormat.Signature);
            writer.Write(HexMapBinaryFormat.Version);
            writer.Write(HexMapBinaryFormat.SpatialMapKind);
            writer.Write(HexMapBinaryFormat.BooleanValueKind);
            writer.Write(topology.Resolution.X);
            writer.Write(topology.Resolution.Y);
            writer.Write((byte)topology.Layout);
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Origin.X));
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Origin.Y));
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Radius));

            for (int index = 0; index < count; index++)
                writer.Write(map[index]);
        }

        /// <summary>
        /// Writes a spatial integer map's topology, origin, radius, and values as one version-1 binary record.
        /// </summary>
        /// <param name="writer">The writer positioned where the record should begin.</param>
        /// <param name="map">The spatial integer map to serialize.</param>
        /// <remarks>
        /// <para>
        /// The record contains the four ASCII bytes <c>HMAP</c>, version byte <c>1</c>,
        /// spatial map-kind byte <c>1</c>, Int32 value-kind byte <c>2</c>, width and height
        /// as little-endian Int32 values, and one layout byte
        /// (<c>0</c> = OddR, <c>1</c> = EvenR, <c>2</c> = OddQ, <c>3</c> = EvenQ).
        /// The 16-byte header is followed by Origin.X, Origin.Y, and Radius, each encoded as
        /// four little-endian IEEE 754 binary32 bytes. Their exact bits are preserved.
        /// The apothem is derived from the radius when reading.
        /// Starting at offset 28, values use row-major order with X advancing first and
        /// four bytes per cell as a signed little-endian Int32 in two's-complement representation.
        /// </para>
        /// <para>
        /// Empty maps retain their geometry and both dimensions. The source must not change
        /// during serialization and its geometry topology must equal its map topology.
        /// The writer and its stream remain open and are not flushed or repositioned.
        /// Both seekable and non-seekable streams are supported. Records can be followed by other data.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="writer"/> or <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the geometry topology differs from the map topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin is not finite, or its radius is not finite and positive.
        /// </exception>
        public static void Write(this BinaryWriter writer, ISpatialHexMap<int> map)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            HexMapGeometry geometry = map.Geometry;
            if (geometry.Topology != topology)
                throw new ArgumentException("The geometry topology must equal the map topology.", nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex radius must be finite and positive.");

            int count = topology.Count;

            writer.Write(HexMapBinaryFormat.Signature);
            writer.Write(HexMapBinaryFormat.Version);
            writer.Write(HexMapBinaryFormat.SpatialMapKind);
            writer.Write(HexMapBinaryFormat.Int32ValueKind);
            writer.Write(topology.Resolution.X);
            writer.Write(topology.Resolution.Y);
            writer.Write((byte)topology.Layout);
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Origin.X));
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Origin.Y));
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Radius));

            for (int index = 0; index < count; index++)
                writer.Write(map[index]);
        }

        /// <summary>
        /// Writes a spatial floating-point map's topology, origin, radius, and values as one version-1 binary record.
        /// </summary>
        /// <param name="writer">The writer positioned where the record should begin.</param>
        /// <param name="map">The spatial floating-point map to serialize.</param>
        /// <remarks>
        /// <para>
        /// The record contains the four ASCII bytes <c>HMAP</c>, version byte <c>1</c>,
        /// spatial map-kind byte <c>1</c>, Single value-kind byte <c>3</c>, width and height
        /// as little-endian Int32 values, and one layout byte
        /// (<c>0</c> = OddR, <c>1</c> = EvenR, <c>2</c> = OddQ, <c>3</c> = EvenQ).
        /// The 16-byte header is followed by Origin.X, Origin.Y, and Radius, each encoded as
        /// four little-endian IEEE 754 binary32 bytes. Their exact bits are preserved.
        /// The apothem is derived from the radius when reading.
        /// Starting at offset 28, values use row-major order with X advancing first and
        /// four bytes per cell as little-endian IEEE 754 binary32, preserving every value bit.
        /// </para>
        /// <para>
        /// Empty maps retain their geometry and both dimensions. The source must not change
        /// during serialization and its geometry topology must equal its map topology.
        /// The writer and its stream remain open and are not flushed or repositioned.
        /// Both seekable and non-seekable streams are supported. Records can be followed by other data.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="writer"/> or <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the geometry topology differs from the map topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the geometry origin is not finite, or its radius is not finite and positive.
        /// </exception>
        public static void Write(this BinaryWriter writer, ISpatialHexMap<float> map)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapTopology topology = map.Topology;
            HexMapGeometry geometry = map.Geometry;
            if (geometry.Topology != topology)
                throw new ArgumentException("The geometry topology must equal the map topology.", nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex radius must be finite and positive.");

            int count = topology.Count;

            writer.Write(HexMapBinaryFormat.Signature);
            writer.Write(HexMapBinaryFormat.Version);
            writer.Write(HexMapBinaryFormat.SpatialMapKind);
            writer.Write(HexMapBinaryFormat.SingleValueKind);
            writer.Write(topology.Resolution.X);
            writer.Write(topology.Resolution.Y);
            writer.Write((byte)topology.Layout);
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Origin.X));
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Origin.Y));
            writer.Write(BitConverter.SingleToInt32Bits(geometry.Radius));

            for (int index = 0; index < count; index++)
                writer.Write(BitConverter.SingleToInt32Bits(map[index]));
        }
    }
}
