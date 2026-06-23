using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes point-distance providers into 8-bit grayscale rasters using unsigned distance mapping.
    /// </summary>
    public sealed class PointDistanceProviderGray8BitRasterizer : IRasterizer<IPointDistanceProvider, Raster<byte>>
    {
        private readonly Func<float, byte> _distanceToGrayLevel;

        /// <summary>
        /// Initializes a new point-distance provider rasterizer.
        /// </summary>
        /// <param name="distanceToGrayLevel">The function that maps unsigned distance to an 8-bit grayscale value.</param>
        public PointDistanceProviderGray8BitRasterizer(Func<float, byte> distanceToGrayLevel)
        {
            _distanceToGrayLevel = distanceToGrayLevel ?? throw new ArgumentNullException(nameof(distanceToGrayLevel));
        }

        /// <inheritdoc/>
        public Raster<byte> Rasterize(IPointDistanceProvider source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

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
                    float distance = source.Distance(point);
                    values[valueIndex++] = _distanceToGrayLevel(distance);
                }
            }

            return new Raster<byte>(grid, values);
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
