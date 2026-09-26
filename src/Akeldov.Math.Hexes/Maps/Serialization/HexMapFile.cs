using Akeldov.Math.Hexes.Geometry;
using System;
using System.IO;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Reads and writes versioned binary hex-map files.
    /// </summary>
    /// <remarks>
    /// Absolute and relative paths are supported. Relative paths are resolved against the process's
    /// current working directory. Parent directories must already exist. All opened files are closed
    /// on success or failure. File-system exceptions propagate to the caller.
    /// </remarks>
    public static class HexMapFile
    {
        /// <summary>
        /// Reads the first Boolean hex-map record from a binary file.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted number of cells, checked before allocating value storage.
        /// Must be non-negative; zero permits only empty maps. Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        /// <returns>A new mutable Boolean hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryReaderExtensions.ReadBoolHexMap"/> and its format validation.
        /// Bytes following the first record are ignored. The file is closed even if reading fails.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCellCount"/> is negative.</exception>
        /// <exception cref="InvalidDataException">Thrown when the record is invalid, has a different map or value kind, or exceeds the cell-count limit.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the file ends before the complete record is available.</exception>
        public static BoolHexMap ReadBoolHexMap(string path, int maxCellCount = int.MaxValue)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadBoolHexMap(maxCellCount);
        }

        /// <summary>
        /// Creates or overwrites a binary file with one Boolean hex-map record.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="map">The Boolean map to serialize. It must not change during serialization.</param>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryWriterExtensions.WriteHexMap(BinaryWriter, IHexMap{bool})"/>.
        /// Preserves topology and values only. Pass a source typed as ISpatialHexMap to a spatial
        /// overload of Write to include geometry. Overload selection uses the compile-time source type.
        /// An existing file is truncated. Parent directories are not created. The file is flushed and
        /// closed on success and closed on failure. Writing is not atomic; an error after opening
        /// the file may leave a partial record.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="map"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is empty or invalid.
        /// </exception>
        public static void Write(string path, IHexMap<bool> map)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.WriteHexMap(map);
        }

        /// <summary>
        /// Reads the first integer hex-map record from a binary file.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted number of cells, checked before allocating value storage.
        /// Must be non-negative; zero permits only empty maps. Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        /// <returns>A new mutable integer hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryReaderExtensions.ReadIntHexMap"/> and its format validation.
        /// Bytes following the first record are ignored. The file is closed even if reading fails.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCellCount"/> is negative.</exception>
        /// <exception cref="InvalidDataException">Thrown when the record is invalid, has a different map or value kind, or exceeds the cell-count limit.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the file ends before the complete record is available.</exception>
        public static IntHexMap ReadIntHexMap(string path, int maxCellCount = int.MaxValue)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadIntHexMap(maxCellCount);
        }

        /// <summary>
        /// Creates or overwrites a binary file with one integer hex-map record.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="map">The integer map to serialize. It must not change during serialization.</param>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryWriterExtensions.WriteHexMap(BinaryWriter, IHexMap{int})"/>.
        /// Preserves topology and values only. Pass a source typed as ISpatialHexMap to a spatial
        /// overload of Write to include geometry. Overload selection uses the compile-time source type.
        /// An existing file is truncated. Parent directories are not created. The file is flushed and
        /// closed on success and closed on failure. Writing is not atomic; an error after opening
        /// the file may leave a partial record.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="map"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is empty or invalid.
        /// </exception>
        public static void Write(string path, IHexMap<int> map)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.WriteHexMap(map);
        }

        /// <summary>
        /// Reads the first floating-point hex-map record from a binary file.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted number of cells, checked before allocating value storage.
        /// Must be non-negative; zero permits only empty maps. Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        /// <returns>A new mutable floating-point hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryReaderExtensions.ReadFloatHexMap"/> and its format validation.
        /// Bytes following the first record are ignored. The file is closed even if reading fails.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCellCount"/> is negative.</exception>
        /// <exception cref="InvalidDataException">Thrown when the record is invalid, has a different map or value kind, or exceeds the cell-count limit.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the file ends before the complete record is available.</exception>
        public static FloatHexMap ReadFloatHexMap(string path, int maxCellCount = int.MaxValue)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadFloatHexMap(maxCellCount);
        }

        /// <summary>
        /// Creates or overwrites a binary file with one floating-point hex-map record.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="map">The floating-point map to serialize. It must not change during serialization.</param>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryWriterExtensions.WriteHexMap(BinaryWriter, IHexMap{float})"/>.
        /// Preserves topology and values only. Pass a source typed as ISpatialHexMap to a spatial
        /// overload of Write to include geometry. Overload selection uses the compile-time source type.
        /// An existing file is truncated. Parent directories are not created. The file is flushed and
        /// closed on success and closed on failure. Writing is not atomic; an error after opening
        /// the file may leave a partial record.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="map"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is empty or invalid.
        /// </exception>
        public static void Write(string path, IHexMap<float> map)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.WriteHexMap(map);
        }

        /// <summary>
        /// Reads the first spatial Boolean hex-map record from a binary file.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted number of cells, checked before allocating value storage.
        /// Must be non-negative; zero permits only empty maps. Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        /// <returns>A new mutable spatial Boolean hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryReaderExtensions.ReadSpatialBoolHexMap"/> and its format validation.
        /// Bytes following the first record are ignored. The file is closed even if reading fails.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCellCount"/> is negative.</exception>
        /// <exception cref="InvalidDataException">Thrown when the record is invalid, has a different map or value kind, or exceeds the cell-count limit.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the file ends before the complete record is available.</exception>
        public static SpatialBoolHexMap ReadSpatialBoolHexMap(string path, int maxCellCount = int.MaxValue)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadSpatialBoolHexMap(maxCellCount);
        }

        /// <summary>
        /// Creates or overwrites a binary file with one spatial Boolean hex-map record.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="map">The spatial Boolean map to serialize. It must not change during serialization.</param>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryWriterExtensions.WriteSpatialHexMap(BinaryWriter, ISpatialHexMap{bool})"/>.
        /// Preserves topology, origin, radius, and values.
        /// An existing file is truncated. Parent directories are not created. The file is flushed and
        /// closed on success and closed on failure. Writing is not atomic; an error after opening
        /// the file may leave a partial record.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="map"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is empty or invalid, or the map's geometry topology differs from its topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the map's origin is non-finite or its radius is not finite and positive.</exception>
        public static void Write(string path, ISpatialHexMap<bool> map)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapGeometry geometry = map.Geometry;
            if (geometry.Topology != map.Topology)
                throw new ArgumentException("The geometry topology must equal the map topology.", nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex radius must be finite and positive.");

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.WriteSpatialHexMap(map);
        }

        /// <summary>
        /// Reads the first spatial integer hex-map record from a binary file.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted number of cells, checked before allocating value storage.
        /// Must be non-negative; zero permits only empty maps. Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        /// <returns>A new mutable spatial integer hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryReaderExtensions.ReadSpatialIntHexMap"/> and its format validation.
        /// Bytes following the first record are ignored. The file is closed even if reading fails.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCellCount"/> is negative.</exception>
        /// <exception cref="InvalidDataException">Thrown when the record is invalid, has a different map or value kind, or exceeds the cell-count limit.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the file ends before the complete record is available.</exception>
        public static SpatialIntHexMap ReadSpatialIntHexMap(string path, int maxCellCount = int.MaxValue)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadSpatialIntHexMap(maxCellCount);
        }

        /// <summary>
        /// Creates or overwrites a binary file with one spatial integer hex-map record.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="map">The spatial integer map to serialize. It must not change during serialization.</param>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryWriterExtensions.WriteSpatialHexMap(BinaryWriter, ISpatialHexMap{int})"/>.
        /// Preserves topology, origin, radius, and values.
        /// An existing file is truncated. Parent directories are not created. The file is flushed and
        /// closed on success and closed on failure. Writing is not atomic; an error after opening
        /// the file may leave a partial record.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="map"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is empty or invalid, or the map's geometry topology differs from its topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the map's origin is non-finite or its radius is not finite and positive.</exception>
        public static void Write(string path, ISpatialHexMap<int> map)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapGeometry geometry = map.Geometry;
            if (geometry.Topology != map.Topology)
                throw new ArgumentException("The geometry topology must equal the map topology.", nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex radius must be finite and positive.");

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.WriteSpatialHexMap(map);
        }

        /// <summary>
        /// Reads the first spatial floating-point hex-map record from a binary file.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="maxCellCount">
        /// The maximum accepted number of cells, checked before allocating value storage.
        /// Must be non-negative; zero permits only empty maps. Defaults to <see cref="int.MaxValue"/>.
        /// </param>
        /// <returns>A new mutable spatial floating-point hex map with independent storage, owned by the caller.</returns>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryReaderExtensions.ReadSpatialFloatHexMap"/> and its format validation.
        /// Bytes following the first record are ignored. The file is closed even if reading fails.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty or invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCellCount"/> is negative.</exception>
        /// <exception cref="InvalidDataException">Thrown when the record is invalid, has a different map or value kind, or exceeds the cell-count limit.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the file ends before the complete record is available.</exception>
        public static SpatialFloatHexMap ReadSpatialFloatHexMap(string path, int maxCellCount = int.MaxValue)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (maxCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCellCount), maxCellCount, "The cell-count limit must be non-negative.");

            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadSpatialFloatHexMap(maxCellCount);
        }

        /// <summary>
        /// Creates or overwrites a binary file with one spatial floating-point hex-map record.
        /// </summary>
        /// <param name="path">The absolute path, or a path relative to the current working directory.</param>
        /// <param name="map">The spatial floating-point map to serialize. It must not change during serialization.</param>
        /// <remarks>
        /// Uses <see cref="HexMapBinaryWriterExtensions.WriteSpatialHexMap(BinaryWriter, ISpatialHexMap{float})"/>.
        /// Preserves topology, origin, radius, and values.
        /// An existing file is truncated. Parent directories are not created. The file is flushed and
        /// closed on success and closed on failure. Writing is not atomic; an error after opening
        /// the file may leave a partial record.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> or <paramref name="map"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is empty or invalid, or the map's geometry topology differs from its topology.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the map's origin is non-finite or its radius is not finite and positive.</exception>
        public static void Write(string path, ISpatialHexMap<float> map)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("The file path must not be empty.", nameof(path));

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            HexMapGeometry geometry = map.Geometry;
            if (geometry.Topology != map.Topology)
                throw new ArgumentException("The geometry topology must equal the map topology.", nameof(map));

            if (!geometry.Origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex map origin components must be finite.");

            if (float.IsNaN(geometry.Radius) || float.IsInfinity(geometry.Radius) || geometry.Radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(map), "Hex radius must be finite and positive.");

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);
            writer.WriteSpatialHexMap(map);
        }
    }
}
