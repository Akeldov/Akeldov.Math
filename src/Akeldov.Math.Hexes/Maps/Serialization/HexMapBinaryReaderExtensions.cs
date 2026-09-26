using System;
using System.IO;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Reads hex maps from versioned binary records.
    /// </summary>
    public static partial class HexMapBinaryReaderExtensions
    {
        /// <summary>
        /// Reads one version-1 Boolean hex-map record, preserving its dimensions, layout, and values.
        /// </summary>
        /// <param name="reader">The reader positioned at the record's HMAP signature.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted cell count, checked before allocating value storage. Must be
        /// non-negative; zero permits only empty maps. The default is <see cref="int.MaxValue"/>.
        /// Set a smaller limit when the input may declare maps larger than the application needs.
        /// </param>
        /// <returns>A new mutable Boolean hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// <para>
        /// Reads the format documented by <see cref="HexMapBinaryWriterExtensions.Write(BinaryWriter, IHexMap{bool})"/>.
        /// Every Boolean payload byte must be exactly zero or one. The format version is
        /// independent of the package version. Unknown versions, map kinds, and value kinds are rejected.
        /// </para>
        /// <para>
        /// The reader and its stream remain open. A successful call consumes exactly one record,
        /// leaving subsequent data available. Both seekable and non-seekable streams are supported.
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
        /// Thrown when the header or a Boolean value is invalid, or when the cell count exceeds
        /// Int32 capacity or <paramref name="maxCellCount"/>.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the stream ends before the complete header or payload is available.
        /// </exception>
        public static BoolHexMap ReadBoolHexMap(this BinaryReader reader, int maxCellCount = int.MaxValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            if (reader.ReadUInt32() != HexMapBinaryFormat.Signature)
                throw new InvalidDataException("The hex-map record must begin with the HMAP signature.");

            byte version = reader.ReadByte();
            if (version != HexMapBinaryFormat.Version)
                throw new InvalidDataException($"Unsupported hex-map format version: {version}.");

            byte mapKind = reader.ReadByte();
            if (mapKind != HexMapBinaryFormat.TopologyMapKind)
                throw new InvalidDataException($"Unsupported hex-map kind: {mapKind}.");

            byte valueKind = reader.ReadByte();
            if (valueKind != HexMapBinaryFormat.BooleanValueKind)
                throw new InvalidDataException($"Expected Boolean hex-map values, but found value kind {valueKind}.");

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

            Stream stream = reader.BaseStream;
            if (stream.CanSeek && stream.Length - stream.Position < cellCount)
                throw new EndOfStreamException("The stream does not contain all Boolean hex-map values.");

            var topology = new HexMapTopology(width, height, (Layout)layout);
            var values = new bool[(int)cellCount];
            for (int index = 0; index < values.Length; index++)
            {
                byte value = reader.ReadByte();
                if (value > 1)
                    throw new InvalidDataException($"Invalid Boolean hex-map value {value} at flat index {index}.");

                values[index] = value == 1;
            }

            return new BoolHexMap(topology, values);
        }

        /// <summary>
        /// Reads one version-1 integer hex-map record, preserving its dimensions, layout, and values.
        /// </summary>
        /// <param name="reader">The reader positioned at the record's HMAP signature.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted cell count, checked before allocating value storage. Must be
        /// non-negative; zero permits only empty maps. The default is <see cref="int.MaxValue"/>.
        /// Each cell requires four payload bytes. Set a smaller limit when the input may declare
        /// maps larger than the application needs.
        /// </param>
        /// <returns>A new mutable integer hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// <para>
        /// Reads the format documented by <see cref="HexMapBinaryWriterExtensions.Write(BinaryWriter, IHexMap{int})"/>.
        /// All Int32 values are accepted, including both extrema. The format version is independent
        /// of the package version. Unknown versions, map kinds, and non-Int32 value kinds are rejected.
        /// </para>
        /// <para>
        /// The reader and its stream remain open. A successful call consumes exactly one record,
        /// leaving subsequent data available. Both seekable and non-seekable streams are supported.
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
        /// Thrown when the header is invalid, or when the cell count exceeds Int32 capacity or
        /// <paramref name="maxCellCount"/>.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the stream ends before the complete header or payload is available.
        /// </exception>
        public static IntHexMap ReadIntHexMap(this BinaryReader reader, int maxCellCount = int.MaxValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            if (reader.ReadUInt32() != HexMapBinaryFormat.Signature)
                throw new InvalidDataException("The hex-map record must begin with the HMAP signature.");

            byte version = reader.ReadByte();
            if (version != HexMapBinaryFormat.Version)
                throw new InvalidDataException($"Unsupported hex-map format version: {version}.");

            byte mapKind = reader.ReadByte();
            if (mapKind != HexMapBinaryFormat.TopologyMapKind)
                throw new InvalidDataException($"Unsupported hex-map kind: {mapKind}.");

            byte valueKind = reader.ReadByte();
            if (valueKind != HexMapBinaryFormat.Int32ValueKind)
                throw new InvalidDataException($"Expected Int32 hex-map values, but found value kind {valueKind}.");

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

            Stream stream = reader.BaseStream;
            if (stream.CanSeek && stream.Length - stream.Position < cellCount * sizeof(int))
                throw new EndOfStreamException("The stream does not contain all Int32 hex-map values.");

            var topology = new HexMapTopology(width, height, (Layout)layout);
            var values = new int[(int)cellCount];
            for (int index = 0; index < values.Length; index++)
                values[index] = reader.ReadInt32();

            return new IntHexMap(topology, values);
        }

        /// <summary>
        /// Reads one version-1 floating-point hex-map record, preserving its topology and value bits.
        /// </summary>
        /// <param name="reader">The reader positioned at the record's HMAP signature.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted cell count, checked before allocating value storage. Must be
        /// non-negative; zero permits only empty maps. The default is <see cref="int.MaxValue"/>.
        /// Each cell requires four payload bytes. Set a smaller limit when the input may declare
        /// maps larger than the application needs.
        /// </param>
        /// <returns>A new mutable floating-point hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// <para>
        /// Reads the format documented by <see cref="HexMapBinaryWriterExtensions.Write(BinaryWriter, IHexMap{float})"/>.
        /// All IEEE 754 binary32 bit patterns are accepted, including signed zero, subnormal
        /// values, infinities, and NaN payloads. No normalization or arithmetic conversion is
        /// performed. The format version is independent of the package version. Unknown versions,
        /// map kinds, and non-Single value kinds are rejected.
        /// </para>
        /// <para>
        /// The reader and its stream remain open. A successful call consumes exactly one record,
        /// leaving subsequent data available. Both seekable and non-seekable streams are supported.
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
        /// Thrown when the header is invalid, or when the cell count exceeds Int32 capacity or
        /// <paramref name="maxCellCount"/>.
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// Thrown when the stream ends before the complete header or payload is available.
        /// </exception>
        public static FloatHexMap ReadFloatHexMap(this BinaryReader reader, int maxCellCount = int.MaxValue)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            if (reader.ReadUInt32() != HexMapBinaryFormat.Signature)
                throw new InvalidDataException("The hex-map record must begin with the HMAP signature.");

            byte version = reader.ReadByte();
            if (version != HexMapBinaryFormat.Version)
                throw new InvalidDataException($"Unsupported hex-map format version: {version}.");

            byte mapKind = reader.ReadByte();
            if (mapKind != HexMapBinaryFormat.TopologyMapKind)
                throw new InvalidDataException($"Unsupported hex-map kind: {mapKind}.");

            byte valueKind = reader.ReadByte();
            if (valueKind != HexMapBinaryFormat.SingleValueKind)
                throw new InvalidDataException($"Expected Single hex-map values, but found value kind {valueKind}.");

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

            Stream stream = reader.BaseStream;
            if (stream.CanSeek && stream.Length - stream.Position < cellCount * sizeof(float))
                throw new EndOfStreamException("The stream does not contain all Single hex-map values.");

            var topology = new HexMapTopology(width, height, (Layout)layout);
            var values = new float[(int)cellCount];
            for (int index = 0; index < values.Length; index++)
                values[index] = BitConverter.Int32BitsToSingle(reader.ReadInt32());

            return new FloatHexMap(topology, values);
        }
    }
}
