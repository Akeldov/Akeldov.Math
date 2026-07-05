using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance providers into 16-bit grayscale rasters using unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderGray16BitRasterizer : ISpatialRasterizer<IPointDistanceProvider, ushort>
    {
        private readonly Func<float, ushort> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps unsigned distance to a 16-bit grayscale value.</param>
        public PointDistanceProviderGray16BitRasterizer(Func<float, ushort> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<ushort> Rasterize(IPointDistanceProvider source, SpatialRasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            var values = new ushort[checked(grid.Resolution.X * grid.Resolution.Y)];
            VectorXY cellSize = grid.CellSize;
            float firstX = grid.Origin.X + cellSize.X * 0.5f;
            float firstY = grid.Origin.Y + cellSize.Y * 0.5f;

            for (int y = 0; y < grid.Resolution.Y; y++)
            {
                float pointY = firstY + y * cellSize.Y;
                int valueIndex = y * grid.Resolution.X;
                for (int x = 0; x < grid.Resolution.X; x++)
                {
                    PointXY point = new PointXY(firstX + x * cellSize.X, pointY);
                    float distance = source.Distance(point);
                    values[valueIndex++] = _distanceToGrayLevel(distance);
                }
            }

            return new SpatialRaster<ushort>(grid, values);
        }

        private static void ValidateGrid(SpatialRasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }
    }
}

