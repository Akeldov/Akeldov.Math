using System;

namespace Akeldov.Math.Intervals
{
    public readonly struct Interval : IEquatable<Interval>
    {
        public Interval(float min, float max)
        {
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), max, $"The value of {nameof(max)} ({max}) cannot be less than {nameof(min)} ({min}).");

            Min = min;
            Max = max;
            IncludesMin = true;
            IncludesMax = true;
        }


        public Interval(float min, float max, bool includesMin, bool includesMax)
        {
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), max, $"The value of {nameof(max)} ({max}) cannot be less than {nameof(min)} ({min}).");

            Min = min;
            Max = max;
            IncludesMin = includesMin;
            IncludesMax = includesMax;
        }

        public float Min { get; }

        public float Max { get; }

        public bool IncludesMin { get; }

        public bool IncludesMax { get; }

        public override string ToString()
        {
            if (IncludesMin)
            {
                if (IncludesMax)
                    return $"[{Min}, {Max}]";
                else
                    return $"[{Min}, {Max})";
            }
            else
            {
                if (IncludesMax)
                    return $"({Min}, {Max}]";
                else
                    return $"({Min}, {Max})";
            }
        }

        public bool Equals(Interval other) => Min == other.Min && Max == other.Max && IncludesMin == other.IncludesMin && IncludesMax == other.IncludesMax;

        public override bool Equals(object obj) => obj is Interval other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Min, Max, IncludesMin, IncludesMax);

        public static bool operator ==(Interval left, Interval right) => left.Equals(right);

        public static bool operator !=(Interval left, Interval right) => !(left == right);
    }
}
