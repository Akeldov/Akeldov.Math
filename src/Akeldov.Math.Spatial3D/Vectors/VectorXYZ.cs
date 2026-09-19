using System;
using System.Globalization;
using System.Runtime.InteropServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Akeldov.Math.Spatial3D
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Represents a three-dimensional vector with single-precision floating-point components.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct VectorXYZ : IEquatable<VectorXYZ>
    {
        /// <summary>
        /// Initializes a new vector with the specified components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        /// <param name="z">The Z component.</param>
        public VectorXYZ(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X component.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the Y component.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the Z component.
        /// </summary>
        public float Z { get; }

        /// <summary>
        /// Gets the Euclidean length of this vector.
        /// </summary>
        public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// Gets the squared Euclidean length of this vector.
        /// </summary>
        public float SquaredLength => X * X + Y * Y + Z * Z;

        /// <summary>
        /// Gets a value indicating whether all components are neither NaN nor infinite.
        /// </summary>
        public bool IsFinite =>
            !float.IsNaN(X) && !float.IsInfinity(X) &&
            !float.IsNaN(Y) && !float.IsInfinity(Y) &&
            !float.IsNaN(Z) && !float.IsInfinity(Z);

        /// <summary>
        /// Gets the vector with all components equal to zero.
        /// </summary>
        public static VectorXYZ Zero => new VectorXYZ(0f, 0f, 0f);

        /// <summary>
        /// Gets the vector with all components equal to one.
        /// </summary>
        public static VectorXYZ One => new VectorXYZ(1f, 1f, 1f);

        /// <summary>
        /// Gets the standard basis vector along the positive X axis.
        /// </summary>
        public static VectorXYZ BasisX => new VectorXYZ(1f, 0f, 0f);

        /// <summary>
        /// Gets the standard basis vector along the positive Y axis.
        /// </summary>
        public static VectorXYZ BasisY => new VectorXYZ(0f, 1f, 0f);

        /// <summary>
        /// Gets the standard basis vector along the positive Z axis.
        /// </summary>
        public static VectorXYZ BasisZ => new VectorXYZ(0f, 0f, 1f);

        /// <summary>
        /// Returns a vector with the same direction and a length of one.
        /// </summary>
        /// <returns>The normalized vector, or <see cref="Zero"/> when this vector has zero length.</returns>
        public VectorXYZ Normalize()
        {
            float length = Length;
            return length == 0f ? Zero : new VectorXYZ(X / length, Y / length, Z / length);
        }

        /// <summary>
        /// Indicates whether this vector has the same components as another vector.
        /// </summary>
        /// <param name="other">The vector to compare with this vector.</param>
        /// <returns><see langword="true"/> if all components are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VectorXYZ other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorXYZ other && Equals(other);

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
        public void Deconstruct(out float x, out float y, out float z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The dot product.</returns>
        public static float Dot(in VectorXYZ left, in VectorXYZ right) =>
            left.X * right.X + left.Y * right.Y + left.Z * right.Z;

        /// <summary>
        /// Returns the right-handed cross product of two vectors.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The vector cross product.</returns>
        public static VectorXYZ Cross(in VectorXYZ left, in VectorXYZ right) =>
            new VectorXYZ(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X);

        /// <summary>
        /// Indicates whether two vectors have equal components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if all components are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(VectorXYZ left, VectorXYZ right) => left.Equals(right);

        /// <summary>
        /// Indicates whether two vectors have different components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns><see langword="true"/> if any component differs; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(VectorXYZ left, VectorXYZ right) => !left.Equals(right);

        /// <summary>
        /// Adds two vectors component by component.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The component-wise sum.</returns>
        public static VectorXYZ operator +(VectorXYZ left, VectorXYZ right) =>
            new VectorXYZ(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        /// <summary>
        /// Subtracts one vector from another component by component.
        /// </summary>
        /// <param name="left">The vector to subtract from.</param>
        /// <param name="right">The vector to subtract.</param>
        /// <returns>The component-wise difference.</returns>
        public static VectorXYZ operator -(VectorXYZ left, VectorXYZ right) =>
            new VectorXYZ(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZ operator *(VectorXYZ vector, float scalar) =>
            new VectorXYZ(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZ operator *(float scalar, VectorXYZ vector) => vector * scalar;

        /// <summary>
        /// Multiplies a vector by an integer scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZ operator *(VectorXYZ vector, int scalar) =>
            new VectorXYZ(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

        /// <summary>
        /// Multiplies a vector by an integer scalar.
        /// </summary>
        /// <param name="scalar">The scalar multiplier.</param>
        /// <param name="vector">The vector to multiply.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZ operator *(int scalar, VectorXYZ vector) => vector * scalar;

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The scalar divisor.</param>
        /// <returns>The scaled vector.</returns>
        public static VectorXYZ operator /(VectorXYZ vector, float scalar) =>
            new VectorXYZ(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);

        /// <summary>
        /// Converts an integer vector to a floating-point vector.
        /// </summary>
        /// <param name="vector">The integer vector to convert.</param>
        public static implicit operator VectorXYZ(VectorXYZInt vector) =>
            new VectorXYZ(vector.X, vector.Y, vector.Z);

        /// <summary>
        /// Converts a floating-point vector to an integer vector by truncating each component.
        /// </summary>
        /// <param name="vector">The floating-point vector to convert.</param>
        public static explicit operator VectorXYZInt(VectorXYZ vector) =>
            new VectorXYZInt((int)vector.X, (int)vector.Y, (int)vector.Z);
    }
}
