using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance provider collections into 16-bit grayscale rasters using nearest unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderCollectionGray16BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<IPointDistanceProvider>, Gray16BitColor>
    {
        private readonly Func<float, Gray16BitColor> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps nearest unsigned distance to a 16-bit grayscale value.</param>
        public PointDistanceProviderCollectionGray16BitRasterizer(Func<float, Gray16BitColor> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IReadOnlyList<IPointDistanceProvider> source, SpatialRasterGrid grid)
        {
            return Rasterize<IPointDistanceProvider>(source, grid);
        }

        /// <summary>
        /// Rasterizes point-distance providers into a 16-bit grayscale raster using nearest unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="source">The point-distance providers to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the nearest point-distance provider at each cell center.</returns>
        public SpatialRaster<Gray16BitColor> Rasterize<T>(IReadOnlyList<T> source, SpatialRasterGrid grid)
            where T : IPointDistanceProvider
        {
            ValidateSource(source);
            ValidateGrid(grid);
            var values = new Gray16BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];
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

            return new SpatialRaster<Gray16BitColor>(grid, values);
        }

        private static float GetNearestDistance<T>(IReadOnlyList<T> sources, PointXY point)
            where T : IPointDistanceProvider
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

        private static void ValidateSource<T>(IReadOnlyList<T> source)
            where T : IPointDistanceProvider
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Point-distance provider collection must contain at least one source.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] is null)
                    throw new ArgumentException("Point-distance provider collection must not contain null sources.", nameof(source));
            }
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
