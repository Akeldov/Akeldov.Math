using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact line-intersection calculations for <see cref="Segment"/>.
    /// </summary>
    public static class SegmentIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a segment and a line using exact comparisons.
        /// </summary>
        /// <param name="source">The source segment.</param>
        /// <param name="line">The line to intersect with the source segment.</param>
        /// <returns>A new mutable list owned by the caller. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this Segment source, Line line)
        {
            List<PointXY> intersections = new List<PointXY>();
            VectorXY segmentDirection = source.EndpointB - source.EndpointA;

            if (segmentDirection.SquaredLength == 0f)
            {
                if ((source.IncludesEndpointA || source.IncludesEndpointB) &&
                    GetSignedDistance(line, source.EndpointA) == 0f)
                {
                    intersections.Add(source.EndpointA);
                }

                return intersections;
            }

            float endpointADistance = GetSignedDistance(line, source.EndpointA);
            float endpointBDistance = GetSignedDistance(line, source.EndpointB);
            float distanceDelta = endpointADistance - endpointBDistance;

            if (distanceDelta == 0f)
                return intersections;

            float segmentCoordinate = endpointADistance / distanceDelta;
            if (segmentCoordinate < 0f || segmentCoordinate > 1f)
                return intersections;

            PointXY intersection = source.EndpointA + segmentCoordinate * segmentDirection;

            if (segmentCoordinate == 0f)
            {
                if (source.IncludesEndpointA)
                    intersections.Add(intersection);
            }
            else if (segmentCoordinate == 1f)
            {
                if (source.IncludesEndpointB)
                    intersections.Add(intersection);
            }
            else
            {
                intersections.Add(intersection);
            }

            return intersections;
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
