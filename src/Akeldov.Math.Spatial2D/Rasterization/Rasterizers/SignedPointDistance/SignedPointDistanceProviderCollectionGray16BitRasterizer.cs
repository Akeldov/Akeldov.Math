using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes signed point-distance provider collections into 16-bit grayscale rasters using minimum signed distance mapping.
    /// </summary>
    public sealed class SignedPointDistanceProviderCollectionGray16BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<ISignedPointDistanceProvider>, Gray16BitColor>
    {
        private readonly Func<float, Gray16BitColor> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new signed point-distance provider collection rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">
        /// The function that maps the minimum signed distance to a 16-bit grayscale value.
        /// Negative distances are inside at least one source; positive distances are outside all sources.
        /// </param>
        public SignedPointDistanceProviderCollectionGray16BitRasterizer(Func<float, Gray16BitColor> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray16BitColor> Rasterize(IReadOnlyList<ISignedPointDistanceProvider> source, RasterGeometry grid)
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
                    float signedDistance = GetMinimumSignedDistance(source, point);
                    values[valueIndex++] = _signedDistanceToGrayLevel(signedDistance);
                }
            }

            return new SpatialRaster<Gray16BitColor>(grid, values);
        }

        private static float GetMinimumSignedDistance(IReadOnlyList<ISignedPointDistanceProvider> sources, PointXY point)
        {
            float minSignedDistance = float.MaxValue;

            for (int i = 0; i < sources.Count; i++)
            {
                float signedDistance = sources[i].SignedDistance(point);
                if (signedDistance < minSignedDistance)
                    minSignedDistance = signedDistance;
            }

            return minSignedDistance;
        }

        private static void ValidateSource(IReadOnlyList<ISignedPointDistanceProvider> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Signed point-distance provider collection must contain at least one source.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null)
                    throw new ArgumentException("Signed point-distance provider collection must not contain null sources.", nameof(source));
            }
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
