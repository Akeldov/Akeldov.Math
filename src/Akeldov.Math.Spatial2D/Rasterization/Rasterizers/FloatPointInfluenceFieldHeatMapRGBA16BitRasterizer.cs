using System;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes floating-point influence fields into 16-bit RGBA rasters using a heat map color scale.
    /// </summary>
    public sealed class FloatPointInfluenceFieldHeatMapRGBA16BitRasterizer :
        IRasterizer<FloatPointInfluenceField, RGBA16BitRaster>
    {
        /// <inheritdoc/>
        public RGBA16BitRaster Rasterize(FloatPointInfluenceField source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            ValidateRange(source);

            var values = new RGBA16BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    float value = source.Sample(point);
                    values[y * grid.Resolution.X + x] = RGBA16BitColor.FromTemperature(value, source.Min, source.Max);
                }
            }

            return new RGBA16BitRaster(grid, values);
        }

        private static void ValidateRange(FloatPointInfluenceField source)
        {
            if (float.IsNaN(source.Min) || float.IsInfinity(source.Min) ||
                float.IsNaN(source.Max) || float.IsInfinity(source.Max) ||
                source.Max < source.Min)
            {
                throw new ArgumentException("Influence field range must be finite and ordered.", nameof(source));
            }
        }

        private static void ValidateGrid(RasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }
    }
}
