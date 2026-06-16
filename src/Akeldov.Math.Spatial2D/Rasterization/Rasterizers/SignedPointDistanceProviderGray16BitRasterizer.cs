using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance providers into 16-bit grayscale rasters using signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderGray16BitRasterizer : IRasterizer<ISignedPointDistanceProvider, Gray16BitRaster>
    {
        private readonly Func<float, ushort> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">The function that maps signed distance to a 16-bit grayscale value. Negative distances are inside the source; positive distances are outside.</param>
        public SignedPointDistanceProviderGray16BitRasterizer(Func<float, ushort> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public Gray16BitRaster Rasterize(ISignedPointDistanceProvider source, RasterGrid grid)
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
                    float signedDistance = source.SignedDistance(point);
                    values[valueIndex++] = _signedDistanceToGrayLevel(signedDistance);
                }
            }

            return new Gray16BitRaster(grid, values);
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
