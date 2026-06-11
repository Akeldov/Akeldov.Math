using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes regions into 8-bit grayscale rasters using signed distance-to-boundary mapping.
    /// </summary>
    public sealed class RegionSignedDistanceGray8BitRasterizer : IRasterizer<IContourBasedRegion, Gray8BitRaster>
    {
        private readonly Func<float, byte> _signedDistanceToGrayLevel;

        /// <summary>
        /// Initializes a new region rasterizer.
        /// </summary>
        /// <param name="signedDistanceToGrayLevel">The function that maps signed distance to the region boundary to an 8-bit grayscale value. Negative distances are inside the region; positive distances are outside.</param>
        public RegionSignedDistanceGray8BitRasterizer(Func<float, byte> signedDistanceToGrayLevel)
        {
            _signedDistanceToGrayLevel = signedDistanceToGrayLevel ?? throw new ArgumentNullException(nameof(signedDistanceToGrayLevel));
        }

        /// <inheritdoc/>
        public Gray8BitRaster Rasterize(IContourBasedRegion source, RasterGrid grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            IReadOnlyList<IPath> curves = GetCurves(source);
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
                    float signedDistance = GetSignedDistanceToRegion(source, point, curves);
                    values[valueIndex++] = _signedDistanceToGrayLevel(signedDistance);
                }
            }

            return new Gray8BitRaster(grid, values);
        }

        private static IReadOnlyList<IPath> GetCurves(IContourBasedRegion region)
        {
            IReadOnlyList<IContour> contours = region.Contours;
            if (contours == null || contours.Count == 0)
                throw new InvalidOperationException("Region must expose at least one contour.");

            var curves = new List<IPath>();

            for (int i = 0; i < contours.Count; i++)
            {
                IContour contour = contours[i];
                if (contour == null)
                    throw new InvalidOperationException("Region contours must not contain null values.");

                IReadOnlyList<IPath> contourCurves = contour.Curves;
                if (contourCurves == null || contourCurves.Count == 0)
                    throw new InvalidOperationException("Region contours must expose at least one bounded parameterized curve.");

                for (int j = 0; j < contourCurves.Count; j++)
                {
                    IPath curve = contourCurves[j];
                    if (curve == null)
                        throw new InvalidOperationException("Region contour curves must not contain null values.");

                    curves.Add(curve);
                }
            }

            return curves;
        }

        private static void ValidateGrid(RasterGrid grid)
        {
            if (!grid.Size.IsFinite || grid.Size.X <= 0f || grid.Size.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid size components must be finite and positive.");

            if (grid.Resolution.X <= 0 || grid.Resolution.Y <= 0)
                throw new ArgumentOutOfRangeException(nameof(grid), "Raster grid resolution components must be positive.");
        }

        private static float GetSignedDistanceToRegion(IContourBasedRegion region, PointXY point, IReadOnlyList<IPath> curves)
        {
            float distance = DistanceToRegionBoundary(point, curves);
            return region.Contains(point) ? -distance : distance;
        }

        private static float DistanceToRegionBoundary(PointXY point, IReadOnlyList<IPath> curves)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < curves.Count; i++)
            {
                IPath curve = curves[i];
                float distance = curve.Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }
    }
}
