using System;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance providers into 8-bit grayscale rasters using signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderGray8BitRasterizer : ISpatialRasterizer<ISignedPointDistanceProvider, byte>
    {
        private readonly Func<float, byte> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">The function that maps signed distance to an 8-bit grayscale value. Negative distances are inside the source; positive distances are outside.</param>
        public SignedPointDistanceProviderGray8BitRasterizer(Func<float, byte> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<byte> Rasterize(ISignedPointDistanceProvider source, SpatialRasterGrid grid)
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
                    float signedDistance = source.SignedDistance(point);
                    values[valueIndex++] = _signedDistanceToGrayLevel(signedDistance);
                }
            }

            return new SpatialRaster<byte>(grid, values);
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
