using System;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Stores a rectangular raster of color values sampled on a <see cref="SpatialRasterGrid"/>.
    /// </summary>
    /// <typeparam name="TColor">The color value type stored in each raster cell.</typeparam>
    public class SpatialRaster<TColor> : IGrid<TColor>
    {
        /// <summary>
        /// Initializes a new raster with the specified grid and color values.
        /// </summary>
        /// <param name="grid">The raster sampling grid.</param>
        /// <param name="values">
        /// The cell values in row-major order. The array is retained as raster state and must contain one value per grid cell.
        /// </param>
        public SpatialRaster(SpatialRasterGrid grid, TColor[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            int expectedCount = checked(grid.Resolution.X * grid.Resolution.Y);

            if (values.Length != expectedCount)
                throw new ArgumentException("Raster value count must match the raster grid resolution.", nameof(values));

            Grid = grid;
            Values = values;
        }

        /// <summary>
        /// Gets the raster sampling grid.
        /// </summary>
        public SpatialRasterGrid Grid { get; }

        /// <summary>
        /// Gets the retained row-major raster value array.
        /// </summary>
        public TColor[] Values { get; }

        /// <summary>
        /// Gets the raster width in cells.
        /// </summary>
        public int Width => Grid.Resolution.X;

        /// <summary>
        /// Gets the raster height in cells.
        /// </summary>
        public int Height => Grid.Resolution.Y;

        /// <summary>
        /// Gets or sets the value at the specified raster cell.
        /// </summary>
        /// <param name="index">The zero-based raster cell index.</param>
        /// <returns>The value stored at the specified cell.</returns>
        public TColor this[VectorXYInt index]
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
        public TColor this[int x, int y]
        {
            get => Values[GetLinearIndex(x, y)];
            set => Values[GetLinearIndex(x, y)] = value;
        }

        /// <summary>
        /// Gets or sets the value at the specified flat row-major raster cell index.
        /// </summary>
        /// <param name="index">The zero-based flat row-major raster cell index.</param>
        /// <returns>The value stored at the specified cell.</returns>
        public TColor this[int index]
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
        /// Creates a raster with the same grid and a copied value array.
        /// </summary>
        /// <returns>A new raster whose value array is new, mutable, and owned by the caller.</returns>
        public SpatialRaster<TColor> Clone()
        {
            return new SpatialRaster<TColor>(Grid, (TColor[])Values.Clone());
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
