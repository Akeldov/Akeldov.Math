using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides exact intersection calculations for <see cref="Circle"/>.
    /// </summary>
    public static class CircleIntersectionExtensions
    {
        /// <summary>
        /// Returns isolated point intersections between a circle and a ray using exact comparisons.
        /// </summary>
        /// <param name="source">The source circle.</param>
        /// <param name="ray">The ray to intersect with the source circle.</param>
        /// <returns>A new mutable list owned by the caller, ordered in the forward direction of the ray.</returns>
        public static List<PointXY> GetPointIntersections(this Circle source, Ray ray)
        {
            var fullCircle = new Arc(source.Center, source.Radius, 0f, 2f * MathF.PI);
            return ArcIntersectionExtensions.GetPointIntersections(fullCircle, ray);
        }
    }
}
