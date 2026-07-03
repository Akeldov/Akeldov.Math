using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Represents a VectorQRSInt value.
    /// </summary>
    public readonly struct VectorQRSInt : IEquatable<VectorQRSInt>
    {
        /// <summary>
        /// Initializes a new instance of the VectorQRSInt type.
        /// </summary>
        /// <param name="q">The q value.</param>
        /// <param name="r">The r value.</param>
        public VectorQRSInt(int q, int r)
        {
            long s = -(long)q - r;
            if (s < int.MinValue || s > int.MaxValue)
                throw new ArgumentOutOfRangeException("(q, r)", (q, r), "The derived s component must fit in Int32.");

            Q = q;
            R = r;
            S = (int)s;
        }

        /// <summary>
        /// Initializes a new instance of the VectorQRSInt type.
        /// </summary>
        /// <param name="q">The q value.</param>
        /// <param name="r">The r value.</param>
        /// <param name="s">The s value.</param>
        public VectorQRSInt(int q, int r, int s)
        {
            if ((long)q + r + s != 0L)
                throw new ArgumentOutOfRangeException("(q, r, s)", (q, r, s), "The sum of (q, r, s) must be zero.");

            Q = q;
            R = r;
            S = s;
        }

        /// <summary>
        /// Gets the Q value.
        /// </summary>
        public int Q { get; }

        /// <summary>
        /// Gets the R value.
        /// </summary>
        public int R { get; }

        /// <summary>
        /// Gets the S value.
        /// </summary>
        public int S { get; }

        /// <summary>
        /// Performs the Zero operation.
        /// </summary>
        public static VectorQRSInt Zero => new VectorQRSInt(0, 0);

        /// <summary>
        /// Performs the One operation.
        /// </summary>
        public static VectorQRSInt One => new VectorQRSInt(1, 1);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorQRSInt other && Equals(other);

        /// <summary>
        /// Performs the Equals operation.
        /// </summary>
        /// <param name="other">The other value.</param>
        public bool Equals(VectorQRSInt other) => Q == other.Q && R == other.R;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Q, R);

        /// <inheritdoc/>
        public override string ToString() => $"({Q}, {R})";

        /// <summary>
        /// Performs the Deconstruct operation.
        /// </summary>
        /// <param name="q">The q value.</param>
        /// <param name="r">The r value.</param>
        public void Deconstruct(out int q, out int r)
        {
            q = Q;
            r = R;
        }

        /// <summary>
        /// Applies the operator == operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static bool operator ==(VectorQRSInt left, VectorQRSInt right) => left.Equals(right);

        /// <summary>
        /// Applies the operator != operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static bool operator !=(VectorQRSInt left, VectorQRSInt right) => !left.Equals(right);

        /// <summary>
        /// Applies the operator + operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static VectorQRSInt operator +(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(checked(left.Q + right.Q), checked(left.R + right.R));

        /// <summary>
        /// Applies the operator - operator.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        public static VectorQRSInt operator -(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(checked(left.Q - right.Q), checked(left.R - right.R));

        /// <summary>
        /// Applies the operator * operator.
        /// </summary>
        /// <param name="vector">The vector value.</param>
        /// <param name="scalar">The scalar value.</param>
        public static VectorQRSInt operator *(VectorQRSInt vector, int scalar) =>
            new VectorQRSInt(checked(vector.Q * scalar), checked(vector.R * scalar));

        /// <summary>
        /// Applies the operator * operator.
        /// </summary>
        /// <param name="scalar">The scalar value.</param>
        /// <param name="vector">The vector value.</param>
        public static VectorQRSInt operator *(int scalar, VectorQRSInt vector) =>
            new VectorQRSInt(checked(vector.Q * scalar), checked(vector.R * scalar));

        /// <summary>
        /// Applies the operator / operator.
        /// </summary>
        /// <param name="vector">The vector value.</param>
        /// <param name="scalar">The scalar value.</param>
        public static VectorQRSInt operator /(VectorQRSInt vector, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide vector by zero.");
            return new VectorQRSInt(vector.Q / scalar, vector.R / scalar);
        }
    }
}
