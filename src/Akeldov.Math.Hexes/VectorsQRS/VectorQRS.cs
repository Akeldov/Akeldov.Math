using System;
using System.Globalization;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public readonly struct VectorQRS : IEquatable<VectorQRS>
    {
        public VectorQRS(float q, float r)
        {
            Q = q;
            R = r;
            S = -q - r;
        }

        public float Q { get; }

        public float R { get; }

        public float S { get; }

        public static VectorQRS Zero => new VectorQRS(0f, 0f);

        public static VectorQRS One => new VectorQRS(1f, 1f);

        public bool Equals(VectorQRS other) => Q.Equals(other.Q) && R.Equals(other.R);

        public override bool Equals(object? obj) => obj is VectorQRS other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Q, R);

        public override string ToString() => $"({Q.ToString(CultureInfo.InvariantCulture)}, {R.ToString(CultureInfo.InvariantCulture)})";

        public void Deconstruct(out float x, out float y)
        {
            x = Q;
            y = R;
        }

        public static bool operator ==(VectorQRS left, VectorQRS right) => left.Equals(right);

        public static bool operator !=(VectorQRS left, VectorQRS right) => !left.Equals(right);

        public static VectorQRS operator +(VectorQRS left, VectorQRS right) =>
            new VectorQRS(left.Q + right.Q, left.R + right.R);

        public static VectorQRS operator -(VectorQRS left, VectorQRS right) =>
            new VectorQRS(left.Q - right.Q, left.R - right.R);

        public static VectorQRS operator *(VectorQRS vector, float scalar) =>
            new VectorQRS(vector.Q * scalar, vector.R * scalar);

        public static VectorQRS operator *(float scalar, VectorQRS vector) =>
            new VectorQRS(vector.Q * scalar, vector.R * scalar);

        public static VectorQRS operator /(VectorQRS vector, float scalar) =>
            new VectorQRS(vector.Q / scalar, vector.R / scalar);

        public static implicit operator VectorQRS(VectorQRSInt v)
        {
            return new VectorQRS(v.Q, v.R);
        }

        public static explicit operator VectorQRSInt(VectorQRS v)
        {
            return new VectorQRSInt((int)v.Q, (int)v.R);
        }
    }
}
