using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="Ray"/>.
    /// </summary>
    public static class RayIntersectionExtensions
    {
        /// <summary>
        /// Returns the isolated point intersection between a ray and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="line">The line to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. Parallel lines, continuous overlaps, and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, Line line)
        {
            VectorXY rayDirection = source.Direction;
            VectorXY lineDirection = line.Direction;
            float cross = VectorXY.Cross(rayDirection, lineDirection);

            if (cross == 0f)
                return new List<PointXY>();

            VectorXY originDelta = line.ClosestPointToOrigin - source.Origin;
            float rayCoordinate = VectorXY.Cross(originDelta, lineDirection) / cross;

            if (rayCoordinate < 0f)
                return new List<PointXY>();

            PointXY intersection = source.Origin + rayCoordinate * rayDirection;
            return new List<PointXY> { intersection };
        }

        /// <summary>
        /// Returns the isolated point intersection between a ray and a parameterized line using exact comparisons.
        /// </summary>
        /// <param name="source">The source ray.</param>
        /// <param name="line">The parameterized line to intersect with the source ray.</param>
        /// <returns>A new mutable list owned by the caller. Parallel lines, continuous overlaps, and intersections behind the ray return an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Ray source, ParameterizedLine line)
        {
            return GetPointIntersections(source, line.Line);
        }
    }
}
