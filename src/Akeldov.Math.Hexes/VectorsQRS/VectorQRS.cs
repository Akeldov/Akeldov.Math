using System;
using System.Globalization;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Represents a fractional cube-coordinate vector stored by its independent Q and R components.
    /// </summary>
    public readonly struct VectorQRS : IEquatable<VectorQRS>
    {
        /// <summary>
        /// Initializes a fractional QRS vector and derives <see cref="S"/> so that Q + R + S equals zero.
        /// </summary>
        /// <param name="q">The Q component.</param>
        /// <param name="r">The R component.</param>
        public VectorQRS(float q, float r)
        {
            Q = q;
            R = r;
            S = -q - r;
        }

        /// <summary>
        /// Gets the Q component.
        /// </summary>
        public float Q { get; }

        /// <summary>
        /// Gets the R component.
        /// </summary>
        public float R { get; }

        /// <summary>
        /// Gets the derived S component, equal to <c>-Q - R</c>.
        /// </summary>
        public float S { get; }

        /// <summary>
        /// Gets the vector whose components are all zero.
        /// </summary>
        public static VectorQRS Zero => new VectorQRS(0f, 0f);

        /// <summary>
        /// Gets the vector with Q and R equal to one and S equal to minus two.
        /// </summary>
        public static VectorQRS One => new VectorQRS(1f, 1f);

        /// <summary>
        /// Determines whether this vector and another vector have equal Q and R components.
        /// </summary>
        /// <param name="other">The vector to compare with this vector.</param>
        /// <returns><see langword="true"/> when both independent components are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VectorQRS other) => Q.Equals(other.Q) && R.Equals(other.R);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorQRS other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Q, R);

        /// <inheritdoc/>
        public override string ToString() => $"({Q.ToString(CultureInfo.InvariantCulture)}, {R.ToString(CultureInfo.InvariantCulture)})";

        /// <summary>
        /// Deconstructs the vector into its independent Q and R components.
        /// </summary>
        /// <param name="x">Receives the Q component.</param>
        /// <param name="y">Receives the R component.</param>
        public void Deconstruct(out float x, out float y)
        {
            x = Q;
            y = R;
        }

        /// <summary>
        /// Determines whether two vectors have equal Q and R components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        public static bool operator ==(VectorQRS left, VectorQRS right) => left.Equals(right);

        /// <summary>
        /// Determines whether two vectors differ in either independent component.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        public static bool operator !=(VectorQRS left, VectorQRS right) => !left.Equals(right);

        /// <summary>
        /// Adds two QRS vectors component-wise.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The vector to add.</param>
        public static VectorQRS operator +(VectorQRS left, VectorQRS right) =>
            new VectorQRS(left.Q + right.Q, left.R + right.R);

        /// <summary>
        /// Subtracts one QRS vector from another component-wise.
        /// </summary>
        /// <param name="left">The vector to subtract from.</param>
        /// <param name="right">The vector to subtract.</param>
        public static VectorQRS operator -(VectorQRS left, VectorQRS right) =>
            new VectorQRS(left.Q - right.Q, left.R - right.R);

        /// <summary>
        /// Multiplies every component of a QRS vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to scale.</param>
        /// <param name="scalar">The scale factor.</param>
        public static VectorQRS operator *(VectorQRS vector, float scalar) =>
            new VectorQRS(vector.Q * scalar, vector.R * scalar);

        /// <summary>
        /// Multiplies every component of a QRS vector by a scalar.
        /// </summary>
        /// <param name="scalar">The scale factor.</param>
        /// <param name="vector">The vector to scale.</param>
        public static VectorQRS operator *(float scalar, VectorQRS vector) =>
            new VectorQRS(vector.Q * scalar, vector.R * scalar);

        /// <summary>
        /// Divides every component of a QRS vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The divisor.</param>
        public static VectorQRS operator /(VectorQRS vector, float scalar) =>
            new VectorQRS(vector.Q / scalar, vector.R / scalar);

        /// <summary>
        /// Converts an integer QRS vector to its exact fractional representation.
        /// </summary>
        /// <param name="v">The integer vector to convert.</param>
        public static implicit operator VectorQRS(VectorQRSInt v)
        {
            return new VectorQRS(v.Q, v.R);
        }

        /// <summary>
        /// Converts a fractional QRS vector to integer components by truncating Q and R toward zero.
        /// </summary>
        /// <param name="v">The fractional vector to convert.</param>
        public static explicit operator VectorQRSInt(VectorQRS v)
        {
            return new VectorQRSInt((int)v.Q, (int)v.R);
        }
    }
}
