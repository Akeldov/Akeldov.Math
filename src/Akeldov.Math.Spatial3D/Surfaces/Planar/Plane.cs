using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Subfolders organize sources within the public Surfaces namespace.
namespace Akeldov.Math.Spatial3D.Surfaces
#pragma warning restore IDE0130
{
    /// <summary>
    /// Represents an infinite plane in three-dimensional space.
    /// </summary>
    /// <remarks>
    /// The default value represents the horizontal plane <c>z = 0</c>.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Plane : ISurface, IEquatable<Plane>
    {
        private readonly float _equationA;
        private readonly float _equationB;
        // Store C shifted so default(Plane) represents z = 0 instead of an invalid zero-coefficient equation.
        private readonly float _equationCMinusOne;
        private readonly float _equationD;

        /// <summary>
        /// Initializes a new plane passing through the specified points.
        /// </summary>
        /// <param name="a">The first point defining the plane.</param>
        /// <param name="b">The second point defining the plane.</param>
        /// <param name="c">The third point defining the plane.</param>
        /// <exception cref="ArgumentException">Thrown when the points are collinear.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a point coordinate is NaN or infinite.</exception>
        public Plane(PointXYZ a, PointXYZ b, PointXYZ c)
        {
            if (float.IsNaN(a.X) || float.IsInfinity(a.X) ||
                float.IsNaN(a.Y) || float.IsInfinity(a.Y) ||
                float.IsNaN(a.Z) || float.IsInfinity(a.Z))
                throw new ArgumentOutOfRangeException(nameof(a), "Plane point coordinates must be finite.");

            if (float.IsNaN(b.X) || float.IsInfinity(b.X) ||
                float.IsNaN(b.Y) || float.IsInfinity(b.Y) ||
                float.IsNaN(b.Z) || float.IsInfinity(b.Z))
                throw new ArgumentOutOfRangeException(nameof(b), "Plane point coordinates must be finite.");

            if (float.IsNaN(c.X) || float.IsInfinity(c.X) ||
                float.IsNaN(c.Y) || float.IsInfinity(c.Y) ||
                float.IsNaN(c.Z) || float.IsInfinity(c.Z))
                throw new ArgumentOutOfRangeException(nameof(c), "Plane point coordinates must be finite.");

            VectorXYZ normal = VectorXYZ.Cross(b - a, c - a);
            float equationD = -(normal.X * a.X + normal.Y * a.Y + normal.Z * a.Z);

            Initialize(
                normal.X,
                normal.Y,
                normal.Z,
                equationD,
                out _equationA,
                out _equationB,
                out _equationCMinusOne,
                out _equationD,
                nameof(c));
        }

        /// <summary>
        /// Initializes a new plane passing through the specified point with the specified normal.
        /// </summary>
        /// <param name="point">A point on the plane.</param>
        /// <param name="normal">A non-zero vector perpendicular to the plane.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="normal"/> is the zero vector.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a point coordinate or normal component is NaN or infinite.
        /// </exception>
        public Plane(PointXYZ point, VectorXYZ normal)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Plane point coordinates must be finite.");

            if (!normal.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(normal), "Plane normal components must be finite.");

            float equationD = -(normal.X * point.X + normal.Y * point.Y + normal.Z * point.Z);

            Initialize(
                normal.X,
                normal.Y,
                normal.Z,
                equationD,
                out _equationA,
                out _equationB,
                out _equationCMinusOne,
                out _equationD,
                nameof(normal));
        }

        /// <summary>
        /// Initializes a new plane from the implicit equation <c>ax + by + cz + d = 0</c>.
        /// </summary>
        /// <param name="a">The X coefficient.</param>
        /// <param name="b">The Y coefficient.</param>
        /// <param name="c">The Z coefficient.</param>
        /// <param name="d">The offset coefficient.</param>
        /// <exception cref="ArgumentException">Thrown when all three linear coefficients are zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an equation coefficient is NaN or infinite.</exception>
        public Plane(float a, float b, float c, float d)
        {
            if (float.IsNaN(a) || float.IsInfinity(a))
                throw new ArgumentOutOfRangeException(nameof(a), "Plane equation coefficients must be finite.");

            if (float.IsNaN(b) || float.IsInfinity(b))
                throw new ArgumentOutOfRangeException(nameof(b), "Plane equation coefficients must be finite.");

            if (float.IsNaN(c) || float.IsInfinity(c))
                throw new ArgumentOutOfRangeException(nameof(c), "Plane equation coefficients must be finite.");

            if (float.IsNaN(d) || float.IsInfinity(d))
                throw new ArgumentOutOfRangeException(nameof(d), "Plane equation coefficients must be finite.");

            Initialize(
                a,
                b,
                c,
                d,
                out _equationA,
                out _equationB,
                out _equationCMinusOne,
                out _equationD,
                nameof(c));
        }

        /// <summary>
        /// Gets the normalized X coefficient of the implicit equation <c>ax + by + cz + d = 0</c>.
        /// </summary>
        public float EquationA => _equationA;

        /// <summary>
        /// Gets the normalized Y coefficient of the implicit equation <c>ax + by + cz + d = 0</c>.
        /// </summary>
        public float EquationB => _equationB;

        /// <summary>
        /// Gets the normalized Z coefficient of the implicit equation <c>ax + by + cz + d = 0</c>.
        /// </summary>
        public float EquationC => _equationCMinusOne + 1f;

        /// <summary>
        /// Gets the normalized offset coefficient of the implicit equation <c>ax + by + cz + d = 0</c>.
        /// </summary>
        public float EquationD => _equationD;

        /// <summary>
        /// Gets the normalized canonical vector perpendicular to this plane.
        /// </summary>
        public VectorXYZ Normal => new VectorXYZ(EquationA, EquationB, EquationC);

        /// <summary>
        /// Gets the closest point on this plane to the global coordinate origin.
        /// </summary>
        public PointXYZ ClosestPointToOrigin => new PointXYZ(
            -_equationD * Normal.X,
            -_equationD * Normal.Y,
            -_equationD * Normal.Z);

        /// <summary>
        /// Returns the shortest distance from the specified point to this plane.
        /// </summary>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The distance to this plane.</returns>
        public float Distance(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            return MathF.Abs(GetSignedDistance(point));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Plane other && Equals(other);

        /// <summary>
        /// Indicates whether this plane has the same normalized implicit equation as another plane.
        /// </summary>
        /// <param name="other">The plane to compare with this plane.</param>
        /// <returns><see langword="true"/> if both planes are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Plane other) =>
            EquationA.Equals(other.EquationA) &&
            EquationB.Equals(other.EquationB) &&
            EquationC.Equals(other.EquationC) &&
            EquationD.Equals(other.EquationD);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(EquationA, EquationB, EquationC, EquationD);

        /// <inheritdoc/>
        public SurfaceProjection Project(PointXYZ point)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y) ||
                float.IsNaN(point.Z) || float.IsInfinity(point.Z))
                throw new ArgumentOutOfRangeException(nameof(point), "Point coordinates must be finite.");

            float signedDistance = GetSignedDistance(point);
            PointXYZ projection = point - Normal * signedDistance;

            return new SurfaceProjection(projection, MathF.Abs(signedDistance));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0}*x + {1}*y + {2}*z + {3} = 0)",
                EquationA,
                EquationB,
                EquationC,
                EquationD);
        }

        /// <summary>
        /// Indicates whether two planes are equal.
        /// </summary>
        /// <param name="left">The first plane.</param>
        /// <param name="right">The second plane.</param>
        /// <returns><see langword="true"/> if the planes are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Plane left, Plane right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two planes are different.
        /// </summary>
        /// <param name="left">The first plane.</param>
        /// <param name="right">The second plane.</param>
        /// <returns><see langword="true"/> if the planes are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Plane left, Plane right)
        {
            return !(left == right);
        }

        private float GetSignedDistance(PointXYZ point)
        {
            return EquationA * point.X + EquationB * point.Y + EquationC * point.Z + EquationD;
        }

        private static void Initialize(
            float equationA,
            float equationB,
            float equationC,
            float equationD,
            out float normalizedA,
            out float normalizedB,
            out float normalizedCMinusOne,
            out float normalizedD,
            string invalidParamName)
        {
            float scale = MathF.Sqrt(
                equationA * equationA +
                equationB * equationB +
                equationC * equationC);

            if (scale == 0f)
                throw new ArgumentException(
                    "Plane equation must have at least one non-zero linear coefficient.",
                    invalidParamName);

            normalizedA = equationA / scale;
            normalizedB = equationB / scale;
            float normalizedC = equationC / scale;
            normalizedD = equationD / scale;

            if (normalizedA < 0f ||
                (normalizedA == 0f && normalizedB < 0f) ||
                (normalizedA == 0f && normalizedB == 0f && normalizedC < 0f))
            {
                normalizedA = -normalizedA;
                normalizedB = -normalizedB;
                normalizedC = -normalizedC;
                normalizedD = -normalizedD;
            }

            normalizedCMinusOne = normalizedC - 1f;
        }
    }
}
