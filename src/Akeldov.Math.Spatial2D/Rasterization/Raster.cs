using System;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Stores a rectangular raster of values with raster-cell resolution but no spatial bounds.
    /// </summary>
    /// <typeparam name="TValue">The value type stored in each raster cell.</typeparam>
    public class Raster<TValue> : IRaster<TValue>
    {
        /// <summary>
        /// Initializes a new raster with the specified resolution and values.
        /// </summary>
        /// <param name="resolution">The raster resolution in cells.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per raster cell.
        /// </param>
        public Raster(VectorXYInt resolution, TValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            int expectedCount = checked(resolution.X * resolution.Y);

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
        /// Gets the raster width in cells.
        /// </summary>
        public int Width => Resolution.X;

        /// <summary>
        /// Gets the raster height in cells.
        /// </summary>
        public int Height => Resolution.Y;

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
        /// Creates a raster with the same resolution and a copied value array.
        /// </summary>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public Raster<TValue> Clone()
        {
            return new Raster<TValue>(Resolution, (TValue[])Values.Clone());
        }

        private int GetLinearIndex(int x, int y)
        {
            if ((uint)x >= (uint)Width)
                throw new ArgumentOutOfRangeException(nameof(x), "Raster X index must be inside the raster width.");

            if ((uint)y >= (uint)Height)
                throw new ArgumentOutOfRangeException(nameof(y), "Raster Y index must be inside the raster height.");

            return y * Width + x;
        }
    }
}
