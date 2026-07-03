using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public readonly struct VectorQRSInt : IEquatable<VectorQRSInt>
    {
        public VectorQRSInt(int q, int r)
        {
            long s = -(long)q - r;
            if (s < int.MinValue || s > int.MaxValue)
                throw new ArgumentOutOfRangeException("(q, r)", (q, r), "The derived s component must fit in Int32.");

            Q = q;
            R = r;
            S = (int)s;
        }

        public VectorQRSInt(int q, int r, int s)
        {
            if ((long)q + r + s != 0L)
                throw new ArgumentOutOfRangeException("(q, r, s)", (q, r, s), "The sum of (q, r, s) must be zero.");

            Q = q;
            R = r;
            S = s;
        }

        public int Q { get; }

        public int R { get; }

        public int S { get; }

        public static VectorQRSInt Zero => new VectorQRSInt(0, 0);

        public static VectorQRSInt One => new VectorQRSInt(1, 1);

        public override bool Equals(object? obj) => obj is VectorQRSInt other && Equals(other);

        public bool Equals(VectorQRSInt other) => Q == other.Q && R == other.R;

        public override int GetHashCode() => HashCode.Combine(Q, R);

        public override string ToString() => $"({Q}, {R})";

        public void Deconstruct(out int q, out int r)
        {
            q = Q;
            r = R;
        }

        public static bool operator ==(VectorQRSInt left, VectorQRSInt right) => left.Equals(right);

        public static bool operator !=(VectorQRSInt left, VectorQRSInt right) => !left.Equals(right);

        public static VectorQRSInt operator +(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(checked(left.Q + right.Q), checked(left.R + right.R));

        public static VectorQRSInt operator -(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(checked(left.Q - right.Q), checked(left.R - right.R));

        public static VectorQRSInt operator *(VectorQRSInt vector, int scalar) =>
            new VectorQRSInt(checked(vector.Q * scalar), checked(vector.R * scalar));

        public static VectorQRSInt operator *(int scalar, VectorQRSInt vector) =>
            new VectorQRSInt(checked(vector.Q * scalar), checked(vector.R * scalar));

        public static VectorQRSInt operator /(VectorQRSInt vector, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide vector by zero.");
            return new VectorQRSInt(vector.Q / scalar, vector.R / scalar);
        }
    }
}
