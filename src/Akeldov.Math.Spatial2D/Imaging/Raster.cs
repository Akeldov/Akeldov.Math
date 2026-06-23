using System;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public sealed class Raster<TColor>
    {
        public Raster(RasterGrid grid, TColor[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            int expectedCount = checked(grid.Resolution.X * grid.Resolution.Y);

            if (values.Length != expectedCount)
                throw new ArgumentException("Grayscale raster value count must match the raster grid resolution.", nameof(values));

            Grid = grid;
            Values = values;
        }

        public RasterGrid Grid { get; }

        public TColor[] Values { get; }

        public int Width => Grid.Resolution.X;

        public int Height => Grid.Resolution.Y;

        public TColor this[int x, int y]
        {
            get => Values[GetLinearIndex(x, y)];
            set => Values[GetLinearIndex(x, y)] = value;
        }

        public Raster<TColor> Clone()
        {
            return new Raster<TColor>(Grid, (TColor[])Values.Clone());
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
