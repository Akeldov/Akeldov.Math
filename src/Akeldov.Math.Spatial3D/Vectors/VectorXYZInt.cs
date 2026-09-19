using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Represents a three-dimensional vector with integer components.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct VectorXYZInt : IEquatable<VectorXYZInt>
    {
        /// <summary>
        /// Initializes a new integer vector with the specified components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        /// <param name="z">The Z component.</param>
        public VectorXYZInt(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X component.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y component.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the Z component.
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Gets the Euclidean length of this vector.
        /// </summary>
        public float Length => MathF.Sqrt((float)X * X + (float)Y * Y + (float)Z * Z);

        /// <summary>
        /// Gets the vector with all components equal to zero.
        /// </summary>
        public static VectorXYZInt Zero => new VectorXYZInt(0, 0, 0);

        /// <summary>
        /// Gets the vector with all components equal to one.
        /// </summary>
        public static VectorXYZInt One => new VectorXYZInt(1, 1, 1);

        /// <summary>
        /// Gets the standard basis vector along the positive X axis.
        /// </summary>
        public static VectorXYZInt BasisX => new VectorXYZInt(1, 0, 0);

        /// <summary>
        /// Gets the standard basis vector along the positive Y axis.
        /// </summary>
        public static VectorXYZInt BasisY => new VectorXYZInt(0, 1, 0);

        /// <summary>
        /// Gets the standard basis vector along the positive Z axis.
        /// </summary>
        public static VectorXYZInt BasisZ => new VectorXYZInt(0, 0, 1);

        /// <summary>
        /// Indicates whether this vector has the same components as another vector.
        /// </summary>
        /// <param name="other">The vector to compare with this vector.</param>
        /// <returns><see langword="true"/> if all components are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VectorXYZInt other) => X == other.X && Y == other.Y && Z == other.Z;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorXYZInt other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        /// <inheritdoc/>
        public override string ToString() =>
            string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", X, Y, Z);

        /// <summary>
        /// Deconstructs this vector into its X, Y, and Z components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        /// <param name="z">The Z component.</param>
        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Indicates whether two integer vectors have equal components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if all components are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(VectorXYZInt left, VectorXYZInt right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two integer vectors have different components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if any component differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(VectorXYZInt left, VectorXYZInt right) => !left.Equals(right);

        /// <summary>
        /// Adds two integer vectors component by component.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The component-wise sum.</returns>
        public static VectorXYZInt operator +(VectorXYZInt left, VectorXYZInt right) =>
            new VectorXYZInt(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        /// <summary>
        /// Subtracts one integer vector from another component by component.
        /// </summary>
        /// <param name="left">The vector to subtract from.</param>
        /// <param name="right">The vector to subtract.</param>
        /// <returns>The component-wise difference.</returns>
        public static VectorXYZInt operator -(VectorXYZInt left, VectorXYZInt right) =>
            new VectorXYZInt(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        /// <summary>
        /// Multiplies an integer vector by an integer scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZInt operator *(VectorXYZInt vector, int scalar) =>
            new VectorXYZInt(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

        /// <summary>
        /// Multiplies an integer vector by an integer scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZInt operator *(int scalar, VectorXYZInt vector) => vector * scalar;

        /// <summary>
        /// Multiplies an integer vector by a floating-point scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled floating-point vector.</returns>
        public static VectorXYZ operator *(VectorXYZInt vector, float scalar) =>
            new VectorXYZ(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

        /// <summary>
        /// Multiplies an integer vector by a floating-point scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled floating-point vector.</returns>
        public static VectorXYZ operator *(float scalar, VectorXYZInt vector) => vector * scalar;

        /// <summary>
        /// Divides an integer vector by an integer scalar.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The scalar divisor.</param>
        /// <returns>The component-wise integer quotient.</returns>
        /// <exception cref="DivideByZeroException">Thrown when <paramref name="scalar"/> is zero.</exception>
        public static VectorXYZInt operator /(VectorXYZInt vector, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide vector by zero.");

            return new VectorXYZInt(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
        }
    }
}
