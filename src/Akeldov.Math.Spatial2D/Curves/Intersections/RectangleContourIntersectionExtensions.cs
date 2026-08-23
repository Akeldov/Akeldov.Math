using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="RectangleContour"/>.
    /// </summary>
    public static class RectangleContourIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a rectangle contour and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source rectangle contour.</param>
        /// <param name="ray">The ray to intersect with the source contour.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray. A continuous overlap returns an empty list.</returns>
        public static List<PointXY> GetPointIntersections(this RectangleContour source, Ray ray)
        {
            var edges = new[]
            {
                new Segment(source.BottomLeft, source.BottomRight),
                new Segment(source.BottomRight, source.TopRight),
                new Segment(source.TopRight, source.TopLeft),
                new Segment(source.TopLeft, source.BottomLeft),
            };

            return GetPointIntersections(edges, ray);
        }

        internal static List<PointXY> GetPointIntersections(IReadOnlyList<Segment> edges, Ray ray)
        {
            var intersections = new List<PointXY>();

            for (int i = 0; i < edges.Count; i++)
            {
                if (RayIntersectionExtensions.HasContinuousIntersection(ray, edges[i]))
                    return new List<PointXY>();

                List<PointXY> edgeIntersections = SegmentIntersectionExtensions.GetPointIntersections(edges[i], ray);
                for (int j = 0; j < edgeIntersections.Count; j++)
                {
                    if (!intersections.Contains(edgeIntersections[j]))
                        intersections.Add(edgeIntersections[j]);
                }
            }

            return RayIntersectionExtensions.OrderPointIntersections(ray, intersections);
        }
    }
}
