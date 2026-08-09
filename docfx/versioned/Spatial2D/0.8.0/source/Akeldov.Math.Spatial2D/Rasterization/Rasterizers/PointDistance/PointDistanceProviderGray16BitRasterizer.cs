using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance providers into 16-bit grayscale rasters using unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderGray16BitRasterizer : ISpatialRasterizer<IPointDistanceProvider, Gray16BitColor>
    {
        private readonly Func<float, Gray16BitColor> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps unsigned distance to a 16-bit grayscale value.</param>
        public PointDistanceProviderGray16BitRasterizer(Func<float, Gray16BitColor> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IPointDistanceProvider source, RasterGeometry grid)
        {
            return Rasterize<IPointDistanceProvider>(source, grid);
        }

        /// <summary>
        /// Rasterizes a point-distance provider into a 16-bit grayscale raster using unsigned distance mapping.
        /// </summary>
        /// <typeparam name="T">The point-distance provider type.</typeparam>
        /// <param name="source">The point-distance provider to rasterize.</param>
        /// <param name="grid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster produced from the point-distance provider at each cell center.</returns>
        public SpatialRaster<Gray16BitColor> Rasterize<T>(T source, RasterGeometry grid)
            where T : IPointDistanceProvider
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

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
                    float distance = source.Distance(point);
                    values[valueIndex++] = _distanceToGrayLevel(distance);
                }
            }

            return new SpatialRaster<Gray16BitColor>(grid, values);
        }

        private static void ValidateGrid(RasterGeometry grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }
    }
}
