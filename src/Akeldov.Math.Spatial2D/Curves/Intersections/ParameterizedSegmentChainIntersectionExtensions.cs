using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="ParameterizedSegmentChain"/>.
    /// </summary>
    public static class ParameterizedSegmentChainIntersectionExtensions
    {
        /// <summary>
        /// Returns the distinct isolated point intersections between a segment chain and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment chain.</param>
        /// <param name="line">The line to intersect with the source chain.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the canonical direction of the line. Points belonging to continuous overlaps are omitted.</returns>
        public static List<PointXY> GetPointIntersections(this ParameterizedSegmentChain source, Line line)
        {
            List<PointXY> intersections = new List<PointXY>();

            for (int i = 0; i < source.Segments.Count; i++)
            {
                List<PointXY> segmentIntersections =
                    ParameterizedSegmentIntersectionExtensions.GetPointIntersections(source.Segments[i], line);

                for (int j = 0; j < segmentIntersections.Count; j++)
                {
                    PointXY intersection = segmentIntersections[j];
                    if (!BelongsToContinuousOverlap(source, intersection, line) &&
                        !intersections.Contains(intersection))
                    {
                        intersections.Add(intersection);
                    }
                }
            }

            intersections.Sort((left, right) =>
                VectorXY.Dot(left - line.ClosestPointToOrigin, line.Direction).CompareTo(
                    VectorXY.Dot(right - line.ClosestPointToOrigin, line.Direction)));

            return intersections;
        }

        /// <summary>
        /// Determines whether a point belongs to a chain segment that continuously overlaps the line.
        /// </summary>
        /// <param name="source">The segment chain to inspect.</param>
        /// <param name="point">The point to classify.</param>
        /// <param name="line">The intersecting line.</param>
        /// <returns><see langword="true"/> when the point belongs to a continuously overlapping segment; otherwise, <see langword="false"/>.</returns>
        private static bool BelongsToContinuousOverlap(ParameterizedSegmentChain source, PointXY point, Line line)
        {
            for (int i = 0; i < source.Segments.Count; i++)
            {
                ParameterizedSegment segment = source.Segments[i];
                if (GetSignedDistance(line, segment.StartPoint) != 0f ||
                    GetSignedDistance(line, segment.EndPoint) != 0f)
                {
                    continue;
                }

                VectorXY segmentDirection = segment.EndPoint - segment.StartPoint;
                VectorXY startToPoint = point - segment.StartPoint;
                float pointCoordinate = VectorXY.Dot(startToPoint, segmentDirection);

                if (VectorXY.Cross(segmentDirection, startToPoint) == 0f &&
                    pointCoordinate >= 0f &&
                    pointCoordinate <= segmentDirection.SquaredLength)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the signed distance from a point to a normalized line equation.
        /// </summary>
        /// <param name="line">The line that defines the signed-distance function.</param>
        /// <param name="point">The point to evaluate.</param>
        /// <returns>The signed distance in world coordinate units.</returns>
        private static float GetSignedDistance(Line line, PointXY point) =>
            line.EquationA * point.X + line.EquationB * point.Y + line.EquationC;
    }
}
