using Akeldov.Math.Spatial2D.Imaging;
using System;
using System.IO;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of values with raster-cell resolution but no spatial bounds.
    /// </summary>
    /// <typeparam name="TValue">The value type stored in each raster cell.</typeparam>
    public class Raster<TValue> : IRaster<TValue>
    {
        // Image factories live on the raster type so callers select the requested pixel type.
#pragma warning disable CA1000 // Do not declare static members on generic types
        /// <summary>
        /// Loads a PNG file into a raster with 8-bit or 16-bit RGBA or grayscale values.
        /// </summary>
        /// <param name="path">The input PNG file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// Supported only when <typeparamref name="TValue"/> is <see cref="RGBA8BitColor"/>, <see cref="RGBA16BitColor"/>,
        /// <see cref="Gray8BitColor"/>, or <see cref="Gray16BitColor"/>.
        /// The PNG color type and bit depth must match the raster value type: RGBA or grayscale without alpha.
        /// All PNG scanline filters and Adam7 interlacing are supported.
        /// Channel values are preserved without color-space conversion. Spatial bounds are not stored in PNG.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The raster value type or PNG color format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<TValue> LoadFromPng(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (typeof(TValue) != typeof(RGBA8BitColor) && typeof(TValue) != typeof(RGBA16BitColor) &&
                typeof(TValue) != typeof(Gray8BitColor) && typeof(TValue) != typeof(Gray16BitColor))
                throw new NotSupportedException("PNG loading is supported only for RGBA8BitColor, RGBA16BitColor, Gray8BitColor, and Gray16BitColor rasters.");

            using FileStream stream = File.OpenRead(path);
            return LoadFromPng(stream);
        }

        /// <summary>
        /// Loads a PNG from the current position of a readable stream into a raster with 8-bit or 16-bit RGBA or grayscale values.
        /// </summary>
        /// <param name="stream">The input PNG stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// Supported only when <typeparamref name="TValue"/> is <see cref="RGBA8BitColor"/>, <see cref="RGBA16BitColor"/>,
        /// <see cref="Gray8BitColor"/>, or <see cref="Gray16BitColor"/>.
        /// The PNG color type and bit depth must match the raster value type: RGBA or grayscale without alpha.
        /// All PNG scanline filters and Adam7 interlacing are supported.
        /// Channel values are preserved without color-space conversion. Reading stops after the PNG IEND chunk.
        /// Spatial bounds are not stored in PNG.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The raster value type or PNG color format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The PNG data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<TValue> LoadFromPng(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("PNG stream must be readable.", nameof(stream));

            if (typeof(TValue) == typeof(RGBA8BitColor))
                return (Raster<TValue>)(object)PngDecoder.LoadRgba8(stream);

            if (typeof(TValue) == typeof(Gray8BitColor))
                return (Raster<TValue>)(object)PngDecoder.LoadGray8(stream);

            if (typeof(TValue) == typeof(Gray16BitColor))
                return (Raster<TValue>)(object)PngDecoder.LoadGray16(stream);

            if (typeof(TValue) != typeof(RGBA16BitColor))
                throw new NotSupportedException("PNG loading is supported only for RGBA8BitColor, RGBA16BitColor, Gray8BitColor, and Gray16BitColor rasters.");

            return (Raster<TValue>)(object)PngDecoder.LoadRgba16(stream);
        }
        /// <summary>
        /// Loads a BMP file into a raster with 8-bit grayscale or RGBA values.
        /// </summary>
        /// <param name="path">The input BMP file path.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// <see cref="Gray8BitColor"/> requires an uncompressed indexed 8-bit BMP with a grayscale palette.
        /// <see cref="RGBA8BitColor"/> supports uncompressed indexed 8-bit BMP with any palette and 32-bit BGRA BMP.
        /// Palette indices are resolved to colors; palette entries are opaque. For 32-bit BMP, the fourth byte
        /// is retained as alpha, matching <see cref="RasterBmpExtensions.SaveAsBmp(IRaster{RGBA8BitColor}, string)"/>.
        /// Bottom-up and top-down images are supported. Spatial bounds are not stored in BMP.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="NotSupportedException">The raster value type or BMP format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The BMP data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<TValue> LoadFromBmp(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (typeof(TValue) != typeof(Gray8BitColor) && typeof(TValue) != typeof(RGBA8BitColor))
                throw new NotSupportedException("BMP loading is supported only for Gray8BitColor and RGBA8BitColor rasters.");

            using FileStream stream = File.OpenRead(path);
            return LoadFromBmp(stream);
        }

        /// <summary>
        /// Loads a BMP from the current position of a readable stream into a raster with 8-bit grayscale or RGBA values.
        /// </summary>
        /// <param name="stream">The input BMP stream. It need not support seeking and remains open after loading.</param>
        /// <returns>A new mutable raster owned by the caller, with Y increasing from the bottom image row.</returns>
        /// <remarks>
        /// <see cref="Gray8BitColor"/> requires an uncompressed indexed 8-bit BMP with a grayscale palette.
        /// <see cref="RGBA8BitColor"/> supports uncompressed indexed 8-bit BMP with any palette and 32-bit BGRA BMP.
        /// Palette entries are opaque; 32-bit pixel alpha bytes are retained, matching BMP export.
        /// Bottom-up and top-down images are supported. Reading consumes the size declared in the BMP file header.
        /// Spatial bounds are not stored in BMP.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> is not readable.</exception>
        /// <exception cref="NotSupportedException">The raster value type or BMP format is unsupported.</exception>
        /// <exception cref="InvalidDataException">The BMP data is invalid, truncated, or too large for a raster.</exception>
        public static Raster<TValue> LoadFromBmp(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("BMP stream must be readable.", nameof(stream));

            if (typeof(TValue) == typeof(Gray8BitColor))
                return (Raster<TValue>)(object)BmpDecoder.LoadGray8(stream);

            if (typeof(TValue) != typeof(RGBA8BitColor))
                throw new NotSupportedException("BMP loading is supported only for Gray8BitColor and RGBA8BitColor rasters.");

            return (Raster<TValue>)(object)BmpDecoder.LoadRgba8(stream);
        }
#pragma warning restore CA1000

        /// <summary>
        /// Initializes a new raster with the specified resolution and values.
        /// </summary>
        /// <param name="resolution">The raster resolution in cells. Both components must be positive and their product must fit in a one-dimensional array.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per raster cell.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a resolution component is not positive or the raster cell count exceeds a 32-bit array length.
        /// </exception>
        public Raster(VectorXYInt resolution, TValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (resolution.X <= 0 || resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    resolution,
                    "Raster resolution components must be positive.");

            long cellCount = (long)resolution.X * resolution.Y;
            if (cellCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    resolution,
                    "Raster cell count must fit in a one-dimensional array.");

            int expectedCount = (int)cellCount;

            if (values.Length != expectedCount)
                throw new ArgumentException("Raster value count must match the raster grid resolution.", nameof(values));

            Resolution = resolution;
            Values = values;
        }

        /// <summary>
        /// Gets the raster resolution in cells.
        /// </summary>
        public VectorXYInt Resolution { get; }

        /// <summary>
        /// Gets the retained row-major raster value array.
        /// </summary>
        public TValue[] Values { get; }

        /// <summary>
        /// Gets or sets the value at the specified raster cell.
        /// </summary>
        /// <param name="index">The zero-based raster cell index.</param>
        /// <returns>The value stored at the specified cell.</returns>
        public TValue this[VectorXYInt index]
        {
            get => Values[GetLinearIndex(index.X, index.Y)];
            set => Values[GetLinearIndex(index.X, index.Y)] = value;
        }

        /// <summary>
        /// Gets or sets the value at the specified raster cell.
        /// </summary>
        /// <param name="x">The zero-based X cell index.</param>
        /// <param name="y">The zero-based Y cell index.</param>
        /// <returns>The value stored at the specified cell.</returns>
        public TValue this[int x, int y]
        {
            get => Values[GetLinearIndex(x, y)];
            set => Values[GetLinearIndex(x, y)] = value;
        }

        /// <summary>
        /// Gets or sets the value at the specified flat row-major raster cell index.
        /// </summary>
        /// <param name="index">The zero-based flat row-major raster cell index.</param>
        /// <returns>The value stored at the specified cell.</returns>
        public TValue this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Values.Length)
                    throw new ArgumentOutOfRangeException(nameof(index), "Raster flat index must be inside the raster value array.");

                return Values[index];
            }
            set
            {
                if ((uint)index >= (uint)Values.Length)
                    throw new ArgumentOutOfRangeException(nameof(index), "Raster flat index must be inside the raster value array.");

                Values[index] = value;
            }
        }

        private int GetLinearIndex(int x, int y)
        {
            if ((uint)x >= (uint)Resolution.X)
                throw new ArgumentOutOfRangeException(nameof(x), "Raster X index must be inside the raster width.");

            if ((uint)y >= (uint)Resolution.Y)
                throw new ArgumentOutOfRangeException(nameof(y), "Raster Y index must be inside the raster height.");

            return y * Resolution.X + x;
        }
    }
}
