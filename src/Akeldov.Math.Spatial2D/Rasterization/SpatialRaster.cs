using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of values sampled on a <see cref="RasterGeometry"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type stored in each raster cell.</typeparam>
    public class SpatialRaster<TValue> : ISpatialRaster<TValue>
    {
        /// <summary>
        /// Initializes a new raster with the specified geometry and values.
        /// </summary>
        /// <param name="geometry">The valid raster geometry. Its resolution cell count must fit in a one-dimensional array.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per raster cell.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="geometry"/> is invalid or its raster cell count exceeds a 32-bit array length.
        /// </exception>
        public SpatialRaster(RasterGeometry geometry, TValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (float.IsNaN(geometry.Origin.X) || float.IsInfinity(geometry.Origin.X) ||
                float.IsNaN(geometry.Origin.Y) || float.IsInfinity(geometry.Origin.Y) ||
                float.IsNaN(geometry.Size.X) || float.IsInfinity(geometry.Size.X) || geometry.Size.X <= 0f ||
                float.IsNaN(geometry.Size.Y) || float.IsInfinity(geometry.Size.Y) || geometry.Size.Y <= 0f ||
                geometry.Resolution.X <= 0 || geometry.Resolution.Y <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(geometry),
                    geometry,
                    "Raster geometry must have finite bounds, positive size, and positive resolution components.");
            }

            long cellCount = (long)geometry.Resolution.X * geometry.Resolution.Y;
            if (cellCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(geometry),
                    geometry,
                    "Raster cell count must fit in a one-dimensional array.");

            int expectedCount = (int)cellCount;

            if (values.Length != expectedCount)
                throw new ArgumentException("Raster value count must match the raster geometry resolution.", nameof(values));

            Geometry = geometry;
            Values = values;
        }

        /// <summary>
        /// Gets the raster geometry.
        /// </summary>
        public RasterGeometry Geometry { get; }

        /// <summary>
        /// Gets the retained row-major raster value array.
        /// </summary>
        public TValue[] Values { get; }

        /// <summary>
        /// Gets the raster resolution in cells.
        /// </summary>
        public VectorXYInt Resolution => Geometry.Resolution;

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

        /// <summary>
        /// Creates a raster with the same geometry and a copied value array.
        /// </summary>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public SpatialRaster<TValue> Clone()
        {
            return new SpatialRaster<TValue>(Geometry, (TValue[])Values.Clone());
        }

        /// <summary>
        /// Creates a non-spatial raster with the same resolution and a copied value array.
        /// </summary>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public Raster<TValue> ToRaster()
        {
            return new Raster<TValue>(Geometry.Resolution, (TValue[])Values.Clone());
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
