using System;

namespace Akeldov.Math.Intervals
{
    public readonly struct IntervalInt : IEquatable<IntervalInt>
    {
        public IntervalInt(int min, int max)
        {
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), max, $"The value of {nameof(max)} ({max}) cannot be less than {nameof(min)} ({min}).");

            Min = min;
            Max = max;
            IncludesMin = true;
            IncludesMax = true;
        }

        public IntervalInt(int min, int max, bool includesMin, bool includesMax)
        {
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), max, $"The value of {nameof(max)} ({max}) cannot be less than {nameof(min)} ({min}).");

            Min = min;
            Max = max;
            IncludesMin = includesMin;
            IncludesMax = includesMax;
        }

        public int Min { get; }

        public int Max { get; }

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

        public bool Equals(IntervalInt other) => Min == other.Min && Max == other.Max && IncludesMin == other.IncludesMin && IncludesMax == other.IncludesMax;

        public override bool Equals(object obj) => obj is IntervalInt other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Min, Max, IncludesMin, IncludesMax);

        public static bool operator ==(IntervalInt left, IntervalInt right) => left.Equals(right);

        public static bool operator !=(IntervalInt left, IntervalInt right) => !(left == right);
    }
}
