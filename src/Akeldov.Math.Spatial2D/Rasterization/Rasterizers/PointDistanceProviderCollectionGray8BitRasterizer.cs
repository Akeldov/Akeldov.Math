using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance provider collections into 8-bit grayscale rasters using nearest unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderCollectionGray8BitRasterizer :
        IRasterizer<IReadOnlyList<IPointDistanceProvider>, Gray8BitRaster>
    {
        private readonly Func<float, byte> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps nearest unsigned distance to an 8-bit grayscale value.</param>
        public PointDistanceProviderCollectionGray8BitRasterizer(Func<float, byte> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public Gray8BitRaster Rasterize(IReadOnlyList<IPointDistanceProvider> source, RasterGrid grid)
        {
            ValidateSource(source);
            ValidateGrid(grid);
            var values = new byte[checked(grid.Resolution.X * grid.Resolution.Y)];
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
                    float distance = GetNearestDistance(source, point);
                    values[valueIndex++] = _distanceToGrayLevel(distance);
                }
            }

            return new Gray8BitRaster(grid, values);
        }

        private static float GetNearestDistance(IReadOnlyList<IPointDistanceProvider> sources, PointXY point)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < sources.Count; i++)
            {
                float distance = sources[i].Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private static void ValidateSource(IReadOnlyList<IPointDistanceProvider> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Point-distance provider collection must contain at least one source.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null)
                    throw new ArgumentException("Point-distance provider collection must not contain null sources.", nameof(source));
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
