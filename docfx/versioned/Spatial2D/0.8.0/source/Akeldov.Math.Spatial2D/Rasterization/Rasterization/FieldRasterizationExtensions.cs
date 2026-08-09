using System;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a spatial field by sampling it at the center of each raster cell.
        /// </summary>
        /// <typeparam name="TFieldValue">The value type sampled from the field.</typeparam>
        /// <typeparam name="TRasterValue">The raster cell value type produced by the selector.</typeparam>
        /// <param name="field">The spatial field to sample.</param>
        /// <param name="grid">The raster geometry that describes the sampled region.</param>
        /// <param name="selector">The function that maps each sampled field value to a raster value.</param>
        /// <returns>
        /// A spatial raster whose value array is new, mutable, and owned by the caller.
        /// </returns>
        public static SpatialRaster<TRasterValue> Rasterize<TFieldValue, TRasterValue>(
            this IField<TFieldValue> field,
            RasterGeometry grid,
            Func<TFieldValue, TRasterValue> selector)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            if (float.IsNaN(grid.Origin.X) || float.IsInfinity(grid.Origin.X) ||
                float.IsNaN(grid.Origin.Y) || float.IsInfinity(grid.Origin.Y) ||
                !grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f ||
                grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(grid),
                    grid,
                    "Raster geometry must have finite bounds, positive size, and positive resolution components.");
            }

            long cellCount = (long)grid.Resolution.X * grid.Resolution.Y;
            if (cellCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(grid),
                    grid,
                    "Raster cell count must fit in a one-dimensional array.");
            }

            var values = new TRasterValue[(int)cellCount];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            int valueIndex = 0;
            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    var point = new PointXY(firstX + x * cellSize.X, pointY);
                    values[valueIndex++] = selector(field.Sample(point));
                }
            }

            return new SpatialRaster<TRasterValue>(grid, values);
        }
    }
}
