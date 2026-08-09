using System;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curves into 8-bit grayscale rasters using projection-to-curve mapping.
    /// </summary>
    public sealed class ParameterizedCurveDistanceGray8BitRasterizer : ISpatialRasterizer<IParameterizedCurve, Gray8BitColor>
    {
        private readonly Func<float, float, Gray8BitColor> _projectionToGrayLevel;

        /// <summary>
        /// Initializes a new parameterized curve rasterizer.
        /// </summary>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the curve and curve coordinate to an 8-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate.
        /// </param>
        public ParameterizedCurveDistanceGray8BitRasterizer(Func<float, float, Gray8BitColor> projectionToGrayLevel)
        {
            _projectionToGrayLevel = projectionToGrayLevel ?? throw new ArgumentNullException(nameof(projectionToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<Gray8BitColor> Rasterize(IParameterizedCurve source, RasterGeometry grid)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateGrid(grid);
            var values = new Gray8BitColor[checked(grid.Resolution.X * grid.Resolution.Y)];
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
                    ParameterizedCurveProjection projection = source.ProjectWithParameter(point);
                    values[valueIndex++] = _projectionToGrayLevel(projection.Distance, projection.CurveCoordinate);
                }
            }

            return new SpatialRaster<Gray8BitColor>(grid, values);
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
