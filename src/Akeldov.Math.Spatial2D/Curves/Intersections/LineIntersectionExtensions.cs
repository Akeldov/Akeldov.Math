using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="Line"/>.
    /// </summary>
    public static class LineIntersectionExtensions
    {
        /// <summary>
        /// Returns the isolated point intersection between two lines using exact comparisons.
        /// </summary>
        /// <param name="source">The source line.</param>
        /// <param name="line">The line to intersect with the source line.</param>
        /// <returns>A new mutable list owned by the caller. Parallel or coincident lines return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Line source, Line line)
        {
            VectorXY sourceDirection = source.Direction;
            VectorXY lineDirection = line.Direction;
            float cross = VectorXY.Cross(sourceDirection, lineDirection);

            if (cross == 0f)
                return new List<PointXY>();

            VectorXY originDelta = line.ClosestPointToOrigin - source.ClosestPointToOrigin;
            float sourceCoordinate = VectorXY.Cross(originDelta, lineDirection) / cross;
            PointXY intersection = source.ClosestPointToOrigin + sourceCoordinate * sourceDirection;

            return new List<PointXY> { intersection };
        }
    }
}
