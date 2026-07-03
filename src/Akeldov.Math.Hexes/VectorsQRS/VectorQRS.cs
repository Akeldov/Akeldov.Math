using System;
using System.Globalization;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Represents a VectorQRS value.
    /// </summary>
    public readonly struct VectorQRS : IEquatable<VectorQRS>
    {
        /// <summary>
        /// Initializes a new instance of the VectorQRS type.
        /// </summary>
        /// <param name="q">The q value.</param>
        /// <param name="r">The r value.</param>
        public VectorQRS(float q, float r)
        {
            Q = q;
            R = r;
            S = -q - r;
        }

        /// <summary>
        /// Gets the Q value.
        /// </summary>
        public float Q { get; }

        /// <summary>
        /// Gets the R value.
        /// </summary>
        public float R { get; }

        /// <summary>
        /// Gets the S value.
        /// </summary>
        public float S { get; }

        /// <summary>
        /// Performs the Zero operation.
        /// </summary>
        public static VectorQRS Zero => new VectorQRS(0f, 0f);

        /// <summary>
        /// Performs the One operation.
        /// </summary>
        public static VectorQRS One => new VectorQRS(1f, 1f);

        /// <summary>
        /// Performs the Equals operation.
        /// </summary>
        /// <param name="other">The Other value.</param>
        public bool Equals(VectorQRS other) => Q.Equals(other.Q) && R.Equals(other.R);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorQRS other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Q, R);

        /// <inheritdoc/>
        public override string ToString() => $"({Q.ToString(CultureInfo.InvariantCulture)}, {R.ToString(CultureInfo.InvariantCulture)})";

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public void Deconstruct(out float x, out float y)
        {
            x = Q;
            y = R;
        }

        /// <summary>
        /// Applies the operator == operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static bool operator ==(VectorQRS left, VectorQRS right) => left.Equals(right);

        /// <summary>
        /// Applies the operator != operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static bool operator !=(VectorQRS left, VectorQRS right) => !left.Equals(right);

        /// <summary>
        /// Applies the operator + operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static VectorQRS operator +(VectorQRS left, VectorQRS right) =>
            new VectorQRS(left.Q + right.Q, left.R + right.R);

        /// <summary>
        /// Applies the operator - operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static VectorQRS operator -(VectorQRS left, VectorQRS right) =>
            new VectorQRS(left.Q - right.Q, left.R - right.R);

        /// <summary>
        /// Applies the operator * operator.
        /// </summary>
        /// <param name="vector">The vector value.</param>
        /// <param name="scalar">The scalar value.</param>
        public static VectorQRS operator *(VectorQRS vector, float scalar) =>
            new VectorQRS(vector.Q * scalar, vector.R * scalar);

        /// <summary>
        /// Applies the operator * operator.
        /// </summary>
        /// <param name="scalar">The scalar value.</param>
        /// <param name="vector">The vector value.</param>
        public static VectorQRS operator *(float scalar, VectorQRS vector) =>
            new VectorQRS(vector.Q * scalar, vector.R * scalar);

        /// <summary>
        /// Applies the operator / operator.
        /// </summary>
        /// <param name="vector">The vector value.</param>
        /// <param name="scalar">The scalar value.</param>
        public static VectorQRS operator /(VectorQRS vector, float scalar) =>
            new VectorQRS(vector.Q / scalar, vector.R / scalar);

        /// <summary>
        /// Performs the implicit operator VectorQRS operation.
        /// </summary>
        /// <param name="v">The v value.</param>
        public static implicit operator VectorQRS(VectorQRSInt v)
        {
            return new VectorQRS(v.Q, v.R);
        }

        /// <summary>
        /// Performs the explicit operator VectorQRSInt operation.
        /// </summary>
        /// <param name="v">The v value.</param>
        public static explicit operator VectorQRSInt(VectorQRS v)
        {
            return new VectorQRSInt((int)v.Q, (int)v.R);
        }
    }
}
