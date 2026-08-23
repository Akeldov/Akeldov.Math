using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides intersection calculations for <see cref="CompositeContour"/>.
    /// </summary>
    public static class CompositeContourIntersectionExtensions
    {
        /// <summary>
        /// Returns distinct isolated point intersections between a composite contour and a ray.
        /// </summary>
        /// <param name="source">The source composite contour.</param>
        /// <param name="ray">The ray to intersect with the source contour.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray.</returns>
        public static List<PointXY> GetPointIntersections(this CompositeContour source, Ray ray) =>
            GetPointIntersections(source.Curves, ray);

        internal static List<PointXY> GetPointIntersections(IReadOnlyList<IContourPath> paths, Ray ray)
        {
            var intersections = new List<PointXY>();

            for (int i = 0; i < paths.Count; i++)
            {
                List<PointXY> pathIntersections = paths[i].GetPointIntersections(ray);
                for (int j = 0; j < pathIntersections.Count; j++)
                {
                    if (!intersections.Contains(pathIntersections[j]))
                        intersections.Add(pathIntersections[j]);
                }
            }

            return RayIntersectionExtensions.OrderPointIntersections(ray, intersections);
        }
    }
}
