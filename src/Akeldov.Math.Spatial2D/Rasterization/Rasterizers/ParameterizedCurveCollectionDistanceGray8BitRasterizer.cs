using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Rasterizes parameterized curve collections into 8-bit grayscale rasters using nearest projection mapping.
    /// </summary>
    public sealed class ParameterizedCurveCollectionDistanceGray8BitRasterizer :
        ISpatialRasterizer<IReadOnlyList<IParameterizedCurve>, SpatialRaster<byte>>
    {
        private readonly Func<float, float, byte> _projectionToGrayLevel;

        /// <summary>
        /// Initializes a new parameterized curve collection rasterizer.
        /// </summary>
        /// <param name="projectionToGrayLevel">
        /// The function that maps distance to the nearest curve and its curve coordinate to an 8-bit grayscale value.
        /// The first argument is distance in world coordinate units; the second argument is curve coordinate on the nearest curve.
        /// </param>
        public ParameterizedCurveCollectionDistanceGray8BitRasterizer(Func<float, float, byte> projectionToGrayLevel)
        {
            _projectionToGrayLevel = projectionToGrayLevel ?? throw new ArgumentNullException(nameof(projectionToGrayLevel));
        }

        /// <inheritdoc/>
        public SpatialRaster<byte> Rasterize(IReadOnlyList<IParameterizedCurve> source, SpatialRasterGrid grid)
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
                    ParameterizedCurveProjection projection = ProjectToNearestCurve(source, point);
                    values[valueIndex++] = _projectionToGrayLevel(projection.Distance, projection.CurveCoordinate);
                }
            }

            return new SpatialRaster<byte>(grid, values);
        }

        private static ParameterizedCurveProjection ProjectToNearestCurve(
            IReadOnlyList<IParameterizedCurve> curves,
            PointXY point)
        {
            ParameterizedCurveProjection nearestProjection = curves[0].ProjectWithParameter(point);

            for (int i = 1; i < curves.Count; i++)
            {
                ParameterizedCurveProjection projection = curves[i].ProjectWithParameter(point);
                if (projection.Distance < nearestProjection.Distance)
                    nearestProjection = projection;
            }

            return nearestProjection;
        }

        private static void ValidateSource(IReadOnlyList<IParameterizedCurve> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.Count == 0)
                throw new ArgumentException("Parameterized curve collection must contain at least one curve.", nameof(source));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == null)
                    throw new ArgumentException("Parameterized curve collection must not contain null curves.", nameof(source));
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
