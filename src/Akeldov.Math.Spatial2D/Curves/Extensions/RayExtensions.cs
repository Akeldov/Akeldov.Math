using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides helper methods for <see cref="Ray"/>.
    /// </summary>
    public static class RayExtensions
    {
        /// <summary>
        /// Creates a line perpendicular to the specified ray and passing through the specified point.
        /// </summary>
        /// <param name="ray">The source ray.</param>
        /// <param name="point">The point the perpendicular line must pass through.</param>
        /// <returns>A line perpendicular to <paramref name="ray"/> and passing through <paramref name="point"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="point"/> has a NaN or infinite coordinate.</exception>
        public static Line PerpendicularAt(this Ray ray, PointXY point)
        {
            PointXYValidation.ThrowIfNotFinite(
                point,
                nameof(point),
                "Perpendicular line point coordinates must be finite.");

            VectorXY direction = ray.Direction;
            return new Line(
                direction.X,
                direction.Y,
                -(direction.X * point.X + direction.Y * point.Y));
        }
    }
}
