using System;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>
    /// Stores mutable 8-bit RGBA raster values on a raster grid.
    /// </summary>
    public sealed class RGBA8BitRaster
    {
        /// <summary>
        /// Initializes a new 8-bit RGBA raster and stores the supplied value buffer by reference.
        /// </summary>
        /// <param name="grid">The raster grid that describes the image coordinates.</param>
        /// <param name="values">The mutable RGBA value buffer indexed as y * Width + x.</param>
        public RGBA8BitRaster(RasterGrid grid, RGBA8BitColor[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            ValidateValueCount(grid, values);

            Grid = grid;
            Values = values;
        }

        /// <summary>
        /// Gets the raster grid that describes the image coordinates.
        /// </summary>
        public RasterGrid Grid { get; }

        /// <summary>
        /// Gets the mutable RGBA value buffer indexed as y * Width + x.
        /// </summary>
        /// <remarks>
        /// The raster stores the supplied array by reference for performance. Changes to this array are reflected in the raster.
        /// Use <see cref="Clone"/> when a detached snapshot is needed.
        /// </remarks>
        public RGBA8BitColor[] Values { get; }

        /// <summary>
        /// Gets the raster width in pixels.
        /// </summary>
        public int Width => Grid.Resolution.X;

        /// <summary>
        /// Gets the raster height in pixels.
        /// </summary>
        public int Height => Grid.Resolution.Y;

        /// <summary>
        /// Gets or sets the RGBA value at the specified raster coordinates.
        /// </summary>
        /// <param name="x">The zero-based X pixel index.</param>
        /// <param name="y">The zero-based Y pixel index.</param>
        /// <returns>The RGBA value at the specified raster coordinates.</returns>
        public RGBA8BitColor this[int x, int y]
        {
            get => Values[GetIndex(x, y)];
            set => Values[GetIndex(x, y)] = value;
        }

        /// <summary>
        /// Creates a detached copy of this raster and its value buffer.
        /// </summary>
        /// <returns>A raster snapshot with an independent mutable value buffer.</returns>
        public RGBA8BitRaster Clone()
        {
            return new RGBA8BitRaster(Grid, (RGBA8BitColor[])Values.Clone());
        }

        private int GetIndex(int x, int y)
        {
            if ((uint)x >= (uint)Width)
                throw new ArgumentOutOfRangeException(nameof(x), "Raster X index must be inside the raster width.");

            if ((uint)y >= (uint)Height)
                throw new ArgumentOutOfRangeException(nameof(y), "Raster Y index must be inside the raster height.");

            return y * Width + x;
        }

        private static void ValidateValueCount(RasterGrid grid, RGBA8BitColor[] values)
        {
            int expectedCount = checked(grid.Resolution.X * grid.Resolution.Y);

            if (values.Length != expectedCount)
                throw new ArgumentException("RGBA raster value count must match the raster grid resolution.", nameof(values));
        }
    }
}
