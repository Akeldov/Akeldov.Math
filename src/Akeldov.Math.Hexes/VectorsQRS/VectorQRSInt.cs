using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Represents an integer cube-coordinate vector stored by its independent Q and R components.
    /// </summary>
    public readonly struct VectorQRSInt : IEquatable<VectorQRSInt>
    {
        /// <summary>
        /// Initializes an integer QRS vector and derives <see cref="S"/> so that Q + R + S equals zero.
        /// </summary>
        /// <param name="q">The Q component.</param>
        /// <param name="r">The R component.</param>
        public VectorQRSInt(int q, int r)
        {
            long s = -(long)q - r;
            if (s < int.MinValue || s > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(r),
                    r,
                    $"The values q ({q}) and r ({r}) derive an s component outside Int32.");

            Q = q;
            R = r;
            S = (int)s;
        }

        /// <summary>
        /// Initializes an integer QRS vector from three components whose sum must equal zero.
        /// </summary>
        /// <param name="q">The Q component.</param>
        /// <param name="r">The R component.</param>
        /// <param name="s">The S component.</param>
        public VectorQRSInt(int q, int r, int s)
        {
            if ((long)q + r + s != 0L)
                throw new ArgumentOutOfRangeException(
                    nameof(s),
                    s,
                    $"The components q ({q}), r ({r}), and s ({s}) must sum to zero.");

            Q = q;
            R = r;
            S = s;
        }

        /// <summary>
        /// Gets the Q component.
        /// </summary>
        public int Q { get; }

        /// <summary>
        /// Gets the R component.
        /// </summary>
        public int R { get; }

        /// <summary>
        /// Gets the S component.
        /// </summary>
        public int S { get; }

        /// <summary>
        /// Gets the vector whose components are all zero.
        /// </summary>
        public static VectorQRSInt Zero => new VectorQRSInt(0, 0);

        /// <summary>
        /// Gets the vector with Q and R equal to one and S equal to minus two.
        /// </summary>
        public static VectorQRSInt One => new VectorQRSInt(1, 1);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is VectorQRSInt other && Equals(other);

        /// <summary>
        /// Determines whether this vector and another vector have equal Q and R components.
        /// </summary>
        /// <param name="other">The vector to compare with this vector.</param>
        /// <returns><see langword="true"/> when both independent components are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(VectorQRSInt other) => Q == other.Q && R == other.R;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Q, R);

        /// <inheritdoc/>
        public override string ToString() => $"({Q}, {R})";

        /// <summary>
        /// Deconstructs the vector into its independent Q and R components.
        /// </summary>
        /// <param name="q">Receives the Q component.</param>
        /// <param name="r">Receives the R component.</param>
        public void Deconstruct(out int q, out int r)
        {
            q = Q;
            r = R;
        }

        /// <summary>
        /// Determines whether two vectors have equal Q and R components.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        public static bool operator ==(VectorQRSInt left, VectorQRSInt right) => left.Equals(right);

        /// <summary>
        /// Determines whether two vectors differ in either independent component.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        public static bool operator !=(VectorQRSInt left, VectorQRSInt right) => !left.Equals(right);

        /// <summary>
        /// Adds two integer QRS vectors component-wise using checked arithmetic.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The vector to add.</param>
        public static VectorQRSInt operator +(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(checked(left.Q + right.Q), checked(left.R + right.R));

        /// <summary>
        /// Subtracts one integer QRS vector from another using checked arithmetic.
        /// </summary>
        /// <param name="left">The vector to subtract from.</param>
        /// <param name="right">The vector to subtract.</param>
        public static VectorQRSInt operator -(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(checked(left.Q - right.Q), checked(left.R - right.R));

        /// <summary>
        /// Multiplies every component of an integer QRS vector by a scalar using checked arithmetic.
        /// </summary>
        /// <param name="vector">The vector to scale.</param>
        /// <param name="scalar">The integer scale factor.</param>
        public static VectorQRSInt operator *(VectorQRSInt vector, int scalar) =>
            new VectorQRSInt(checked(vector.Q * scalar), checked(vector.R * scalar));

        /// <summary>
        /// Multiplies every component of an integer QRS vector by a scalar using checked arithmetic.
        /// </summary>
        /// <param name="scalar">The integer scale factor.</param>
        /// <param name="vector">The vector to scale.</param>
        public static VectorQRSInt operator *(int scalar, VectorQRSInt vector) =>
            new VectorQRSInt(checked(vector.Q * scalar), checked(vector.R * scalar));

        /// <summary>
        /// Divides the independent components of an integer QRS vector using integer division.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The integer divisor.</param>
        public static VectorQRSInt operator /(VectorQRSInt vector, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide vector by zero.");
            return new VectorQRSInt(vector.Q / scalar, vector.R / scalar);
        }
    }
}
