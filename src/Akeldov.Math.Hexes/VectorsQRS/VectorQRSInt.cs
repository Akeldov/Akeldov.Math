using System;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public readonly struct VectorQRSInt : IEquatable<VectorQRSInt>
    {
        public VectorQRSInt(int q, int r)
        {
            Q = q;
            R = r;
            S = -q - r;
        }

        public VectorQRSInt(int q, int r, int s)
        {
            if (q + r + s != 0)
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
            new VectorQRSInt(left.Q + right.Q, left.R + right.R);

        public static VectorQRSInt operator -(VectorQRSInt left, VectorQRSInt right) =>
            new VectorQRSInt(left.Q - right.Q, left.R - right.R);

        public static VectorQRSInt operator *(VectorQRSInt vector, int scalar) =>
            new VectorQRSInt(vector.Q * scalar, vector.R * scalar);

        public static VectorQRSInt operator *(int scalar, VectorQRSInt vector) =>
            new VectorQRSInt(vector.Q * scalar, vector.R * scalar);

        public static VectorQRSInt operator /(VectorQRSInt vector, int scalar)
        {
            if (scalar == 0)
                throw new DivideByZeroException("Cannot divide vector by zero.");
            return new VectorQRSInt(vector.Q / scalar, vector.R / scalar);
        }
    }
}
