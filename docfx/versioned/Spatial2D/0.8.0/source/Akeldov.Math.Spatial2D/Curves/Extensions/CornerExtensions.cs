using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using System;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides helpers for constructing curves from corners.
    /// </summary>
    public static class CornerExtensions
    {
        /// <summary>
        /// Creates a ray from the corner vertex along the internal angle bisector.
        /// </summary>
        /// <param name="firstSidePoint">A point on the first side of the corner.</param>
        /// <param name="vertex">The corner vertex.</param>
        /// <param name="secondSidePoint">A point on the second side of the corner.</param>
        /// <returns>A ray that starts at <paramref name="vertex"/> and points along the internal angle bisector.</returns>
        /// <exception cref="ArgumentException">Thrown when the angle is degenerate.</exception>
        public static Ray CreateAngleBisector(PointXY firstSidePoint, PointXY vertex, PointXY secondSidePoint)
        {
            var lineBA = new Line(vertex, firstSidePoint);
            var lineBC = new Line(vertex, secondSidePoint);

            if (lineBA.Equals(lineBC))
                throw new ArgumentException("The angle must not be degenerate.");

            VectorXY bisector = GetAngleBisectorDirection(firstSidePoint, vertex, secondSidePoint, out _);
            float angle = MathF.Atan2(bisector.Y, bisector.X);

            return new Ray(vertex, angle);
        }

        /// <summary>
        /// Creates an arc tangent to both rays of a corner.
        /// </summary>
        /// <param name="firstSidePoint">A point on the first side of the corner.</param>
        /// <param name="vertex">The corner vertex.</param>
        /// <param name="secondSidePoint">A point on the second side of the corner.</param>
        /// <param name="radius">The radius of the tangent arc.</param>
        /// <returns>An arc tangent to both sides of the corner.</returns>
        /// <exception cref="ArgumentException">Thrown when the angle is degenerate.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radius"/> is not finite and positive.</exception>
        public static Arc CreateFilletArc(PointXY firstSidePoint, PointXY vertex, PointXY secondSidePoint, float radius)
        {
            if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");

            var lineBA = new Line(vertex, firstSidePoint);
            var lineBC = new Line(vertex, secondSidePoint);

            if (lineBA.Equals(lineBC))
                throw new ArgumentException("The angle must not be degenerate.");

            return CreateFilletArc(firstSidePoint, vertex, secondSidePoint, radius, lineBA, lineBC);
        }

        /// <summary>
        /// Creates an arc tangent to both rays of a corner, rejecting angles whose sides are collinear within the specified tolerance.
        /// </summary>
        /// <param name="firstSidePoint">A point on the first side of the corner.</param>
        /// <param name="vertex">The corner vertex.</param>
        /// <param name="secondSidePoint">A point on the second side of the corner.</param>
        /// <param name="radius">The radius of the tangent arc.</param>
        /// <param name="epsilon">The non-negative tolerance used to reject nearly degenerate angles.</param>
        /// <returns>An arc tangent to both sides of the corner.</returns>
        /// <exception cref="ArgumentException">Thrown when the angle is degenerate within <paramref name="epsilon"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radius"/> is not finite and positive, or <paramref name="epsilon"/> is negative, NaN, or infinite.</exception>
        public static Arc CreateFilletArc(PointXY firstSidePoint, PointXY vertex, PointXY secondSidePoint, float radius, float epsilon)
        {
            if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");

            if (epsilon < 0f || float.IsNaN(epsilon) || float.IsInfinity(epsilon))
                throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be finite and non-negative.");

            var lineBA = new Line(vertex, firstSidePoint);
            var lineBC = new Line(vertex, secondSidePoint);

            if (lineBA.Equals(lineBC) || lineBA.Distance(secondSidePoint) <= epsilon)
                throw new ArgumentException("The angle must not be degenerate.");

            return CreateFilletArc(firstSidePoint, vertex, secondSidePoint, radius, lineBA, lineBC);
        }

        private static Arc CreateFilletArc(
            PointXY firstSidePoint,
            PointXY vertex,
            PointXY secondSidePoint,
            float radius,
            Line lineBA,
            Line lineBC)
        {
            PointXY center = GetIncircleCenter(firstSidePoint, vertex, secondSidePoint, radius);

            PointXY tanBA = lineBA.Project(center).ProjectedPoint;
            PointXY tanBC = lineBC.Project(center).ProjectedPoint;

            float angleA = MathF.Atan2((tanBA - center).Y, (tanBA - center).X);
            float angleC = MathF.Atan2((tanBC - center).Y, (tanBC - center).X);

            float startAngle = angleA.NormalizeAngleRad();
            float endAngle = angleC.NormalizeAngleRad();

            float sweep = (endAngle - startAngle).NormalizeAngleRad();
            if (sweep > MathF.PI)
            {
                var t = startAngle;
                startAngle = endAngle;
                endAngle = t;
            }

            return new Arc(center, radius, startAngle, endAngle);
        }

        private static PointXY GetIncircleCenter(PointXY firstSidePoint, PointXY vertex, PointXY secondSidePoint, float radius)
        {
            VectorXY bisector = GetAngleBisectorDirection(firstSidePoint, vertex, secondSidePoint, out float angle);

            float distanceToCenter = radius / MathF.Sin(angle / 2f);
            return vertex + bisector * distanceToCenter;
        }

        private static VectorXY GetAngleBisectorDirection(
            PointXY firstSidePoint,
            PointXY vertex,
            PointXY secondSidePoint,
            out float angle)
        {
            VectorXY dirBA = (firstSidePoint - vertex).Normalize();
            VectorXY dirBC = (secondSidePoint - vertex).Normalize();
            VectorXY bisector = (dirBA + dirBC).Normalize();

            angle = VectorXY.Angle(dirBA, dirBC).NormalizeAngleRad();
            if (angle <= 0f)
                throw new ArgumentException("The angle must be greater than zero.");

            return bisector;
        }

        /// <summary>
        /// Creates a circle tangent to both rays of a corner.
        /// </summary>
        /// <param name="firstSidePoint">A point on the first side of the corner.</param>
        /// <param name="vertex">The corner vertex.</param>
        /// <param name="secondSidePoint">A point on the second side of the corner.</param>
        /// <param name="radius">The circle radius.</param>
        /// <returns>A circle tangent to both sides of the corner.</returns>
        /// <exception cref="ArgumentException">Thrown when the angle is degenerate.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radius"/> is not finite and positive.</exception>
        public static Circle CreateCornerTangentCircle(PointXY firstSidePoint, PointXY vertex, PointXY secondSidePoint, float radius)
        {
            if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");

            var lineBA = new Line(vertex, firstSidePoint);
            var lineBC = new Line(vertex, secondSidePoint);

            if (lineBA.Equals(lineBC))
                throw new ArgumentException("The angle must not be degenerate.");

            PointXY center = GetIncircleCenter(firstSidePoint, vertex, secondSidePoint, radius);

            return new Circle(center, radius);
        }

        /// <summary>
        /// Creates a circle tangent to both rays of a corner, rejecting angles whose sides are collinear within the specified tolerance.
        /// </summary>
        /// <param name="firstSidePoint">A point on the first side of the corner.</param>
        /// <param name="vertex">The corner vertex.</param>
        /// <param name="secondSidePoint">A point on the second side of the corner.</param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="epsilon">The non-negative tolerance used to reject nearly degenerate angles.</param>
        /// <returns>A circle tangent to both sides of the corner.</returns>
        /// <exception cref="ArgumentException">Thrown when the angle is degenerate within <paramref name="epsilon"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radius"/> is not finite and positive, or <paramref name="epsilon"/> is negative, NaN, or infinite.</exception>
        public static Circle CreateCornerTangentCircle(PointXY firstSidePoint, PointXY vertex, PointXY secondSidePoint, float radius, float epsilon)
        {
            if (radius <= 0f || float.IsNaN(radius) || float.IsInfinity(radius))
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be finite and positive.");

            if (epsilon < 0f || float.IsNaN(epsilon) || float.IsInfinity(epsilon))
                throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be finite and non-negative.");

            var lineBA = new Line(vertex, firstSidePoint);
            var lineBC = new Line(vertex, secondSidePoint);

            if (lineBA.Equals(lineBC) || lineBA.Distance(secondSidePoint) <= epsilon)
                throw new ArgumentException("The angle must not be degenerate.");

            PointXY center = GetIncircleCenter(firstSidePoint, vertex, secondSidePoint, radius);

            return new Circle(center, radius);
        }
    }
}
