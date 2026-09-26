using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;
using System.IO;

namespace Akeldov.Math.Hexes
{
    public static partial class HexMapBinaryReaderExtensions
    {
        /// <summary>
        /// Reads one version-1 spatial Boolean hex-map record, preserving its topology, geometry, and values.
        /// </summary>
        /// <param name="reader">The reader positioned at the record's HMAP signature.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted cell count, checked before allocating value storage. Must be
        /// non-negative; zero permits only empty maps. The default is <see cref="int.MaxValue"/>.
        /// Set a smaller limit when the input may declare maps larger than the application needs.
        /// </param>
        /// <returns>A new mutable spatial Boolean hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// <para>
        /// Reads the format documented by <see cref="HexMapBinaryWriterExtensions.WriteSpatialHexMap(BinaryWriter, ISpatialHexMap{bool})"/>.
        /// Every Boolean payload byte must be exactly zero or one.
        /// Origin components must be finite; radius must be finite and positive. Geometry bits
        /// are preserved and the apothem is derived from the radius.
        /// Unknown versions, non-spatial map kinds, and other value kinds are rejected.
        /// </para>
        /// <para>
        /// The reader and its stream remain open. A successful call consumes exactly one record,
        /// leaving subsequent data available. Both seekable and non-seekable streams are supported.
        /// Seekable streams are checked for the full value payload before allocating storage.
        /// If reading fails, the stream position is not restored.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reader"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxCellCount"/> is negative.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown when the header, geometry, or a Boolean value is invalid, or when the cell count
        /// exceeds Int32 capacity or <paramref name="maxCellCount"/>.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the stream ends before the complete header, geometry, or payload is available.
        /// </exception>
        public static SpatialBoolHexMap ReadSpatialBoolHexMap(this BinaryReader reader, int maxCellCount = int.MaxValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            HexMapGeometry geometry = ReadSpatialGeometry(reader, HexMapBinaryFormat.BooleanValueKind, maxCellCount, 1);
            var values = new bool[geometry.Topology.Count];
            for (int index = 0; index < values.Length; index++)
            {
                byte value = reader.ReadByte();
                if (value > 1)
                    throw new InvalidDataException($"Invalid Boolean hex-map value {value} at flat index {index}.");

                values[index] = value == 1;
            }

            return new SpatialBoolHexMap(geometry, values);
        }

        /// <summary>
        /// Reads one version-1 spatial integer hex-map record, preserving its topology, geometry, and values.
        /// </summary>
        /// <param name="reader">The reader positioned at the record's HMAP signature.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted cell count, checked before allocating value storage. Must be
        /// non-negative; zero permits only empty maps. The default is <see cref="int.MaxValue"/>.
        /// Set a smaller limit when the input may declare maps larger than the application needs.
        /// </param>
        /// <returns>A new mutable spatial integer hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// <para>
        /// Reads the format documented by <see cref="HexMapBinaryWriterExtensions.WriteSpatialHexMap(BinaryWriter, ISpatialHexMap{int})"/>.
        /// All Int32 values are accepted, including both extrema.
        /// Origin components must be finite; radius must be finite and positive. Geometry bits
        /// are preserved and the apothem is derived from the radius.
        /// Unknown versions, non-spatial map kinds, and other value kinds are rejected.
        /// </para>
        /// <para>
        /// The reader and its stream remain open. A successful call consumes exactly one record,
        /// leaving subsequent data available. Both seekable and non-seekable streams are supported.
        /// Seekable streams are checked for the full value payload before allocating storage.
        /// If reading fails, the stream position is not restored.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reader"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxCellCount"/> is negative.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown when the header or geometry is invalid, or when the cell count
        /// exceeds Int32 capacity or <paramref name="maxCellCount"/>.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the stream ends before the complete header, geometry, or payload is available.
        /// </exception>
        public static SpatialIntHexMap ReadSpatialIntHexMap(this BinaryReader reader, int maxCellCount = int.MaxValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            HexMapGeometry geometry = ReadSpatialGeometry(reader, HexMapBinaryFormat.Int32ValueKind, maxCellCount, sizeof(int));
            var values = new int[geometry.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = reader.ReadInt32();

            return new SpatialIntHexMap(geometry, values);
        }

        /// <summary>
        /// Reads one version-1 spatial floating-point hex-map record, preserving its topology, geometry, and values.
        /// </summary>
        /// <param name="reader">The reader positioned at the record's HMAP signature.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted cell count, checked before allocating value storage. Must be
        /// non-negative; zero permits only empty maps. The default is <see cref="int.MaxValue"/>.
        /// Set a smaller limit when the input may declare maps larger than the application needs.
        /// </param>
        /// <returns>A new mutable spatial floating-point hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// <para>
        /// Reads the format documented by <see cref="HexMapBinaryWriterExtensions.WriteSpatialHexMap(BinaryWriter, ISpatialHexMap{float})"/>.
        /// All value bit patterns are accepted, including signed zero, subnormals, infinities, and NaN payloads.
        /// Origin components must be finite; radius must be finite and positive. Geometry bits
        /// are preserved and the apothem is derived from the radius.
        /// Unknown versions, non-spatial map kinds, and other value kinds are rejected.
        /// </para>
        /// <para>
        /// The reader and its stream remain open. A successful call consumes exactly one record,
        /// leaving subsequent data available. Both seekable and non-seekable streams are supported.
        /// Seekable streams are checked for the full value payload before allocating storage.
        /// If reading fails, the stream position is not restored.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="reader"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxCellCount"/> is negative.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown when the header or geometry is invalid, or when the cell count
        /// exceeds Int32 capacity or <paramref name="maxCellCount"/>.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the stream ends before the complete header, geometry, or payload is available.
        /// </exception>
        public static SpatialFloatHexMap ReadSpatialFloatHexMap(this BinaryReader reader, int maxCellCount = int.MaxValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            HexMapGeometry geometry = ReadSpatialGeometry(reader, HexMapBinaryFormat.SingleValueKind, maxCellCount, sizeof(float));
            var values = new float[geometry.Topology.Count];
            for (int index = 0; index < values.Length; index++)
                values[index] = BitConverter.Int32BitsToSingle(reader.ReadInt32());

            return new SpatialFloatHexMap(geometry, values);
        }

        // Decode and validate the complete spatial header, including allocation limits and payload length.
        private static HexMapGeometry ReadSpatialGeometry(BinaryReader reader, byte expectedValueKind, int maxCellCount, int valueSize)
        {
            if (reader.ReadUInt32() != HexMapBinaryFormat.Signature)
                throw new InvalidDataException("The hex-map record must begin with the HMAP signature.");

            byte version = reader.ReadByte();
            if (version != HexMapBinaryFormat.Version)
                throw new InvalidDataException($"Unsupported hex-map format version: {version}.");

            byte mapKind = reader.ReadByte();
            if (mapKind != HexMapBinaryFormat.SpatialMapKind)
                throw new InvalidDataException($"Expected a spatial hex map, but found map kind {mapKind}.");

            byte valueKind = reader.ReadByte();
            if (valueKind != expectedValueKind)
                throw new InvalidDataException($"Expected hex-map value kind {expectedValueKind}, but found {valueKind}.");

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            byte layout = reader.ReadByte();

            if (width < 0 || height < 0)
                throw new InvalidDataException("Hex-map dimensions must be non-negative.");

            if (layout > (byte)Layout.EvenQ)
                throw new InvalidDataException($"Invalid hex-map layout: {layout}.");

            long cellCount = (long)width * height;
            if (cellCount > int.MaxValue || cellCount > maxCellCount)
                throw new InvalidDataException($"Hex-map cell count {cellCount} exceeds the supported limit {maxCellCount}.");

            float originX = BitConverter.Int32BitsToSingle(reader.ReadInt32());
            float originY = BitConverter.Int32BitsToSingle(reader.ReadInt32());
            float radius = BitConverter.Int32BitsToSingle(reader.ReadInt32());
            var origin = new VectorXY(originX, originY);
            if (!origin.IsFinite)
                throw new InvalidDataException("Hex map origin components must be finite.");

            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new InvalidDataException("Hex radius must be finite and positive.");

            Stream stream = reader.BaseStream;
            if (stream.CanSeek && stream.Length - stream.Position < cellCount * valueSize)
                throw new EndOfStreamException("The stream does not contain all spatial hex-map values.");

            var topology = new HexMapTopology(width, height, (Layout)layout);
            return new HexMapGeometry(topology, origin, radius);
        }
    }
}
