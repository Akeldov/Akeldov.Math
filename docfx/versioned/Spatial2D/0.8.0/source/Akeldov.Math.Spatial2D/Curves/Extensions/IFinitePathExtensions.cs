using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides helper methods for <see cref="IFinitePath"/>.
    /// </summary>
    public static class IFinitePathExtensions
    {
        /// <summary>
        /// Returns the point at the specified normalized curve coordinate.
        /// </summary>
        /// <param name="path">The finite path.</param>
        /// <param name="normalizedCurveCoordinate">The normalized curve coordinate in the inclusive range [0, 1].</param>
        /// <returns>The point on <paramref name="path"/> at the specified normalized coordinate.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="normalizedCurveCoordinate"/> is NaN, infinite, or outside the inclusive range [0, 1].
        /// </exception>
        public static PointXY GetPointAtNormalizedCoordinate(
            this IFinitePath path,
            float normalizedCurveCoordinate)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (float.IsNaN(normalizedCurveCoordinate) || float.IsInfinity(normalizedCurveCoordinate))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(normalizedCurveCoordinate),
                    "Normalized curve coordinate must be finite.");
            }

            if (normalizedCurveCoordinate < 0f || normalizedCurveCoordinate > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(normalizedCurveCoordinate),
                    "Normalized curve coordinate must lie in the inclusive range [0, 1].");
            }

            return path.GetPoint(path.Length * normalizedCurveCoordinate);
        }
    }
}
