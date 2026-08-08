using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes
{
    internal static class PerlinNoise2D
    {
        private const double InverseSquareRootOfTwo = 0.7071067811865475244d;

        public static double Sample(double x, double y, int seed)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || x < long.MinValue || x >= long.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(x), x, "Noise coordinates must be finite and fit the signed 64-bit lattice range.");

            if (double.IsNaN(y) || double.IsInfinity(y) || y < long.MinValue || y >= long.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(y), y, "Noise coordinates must be finite and fit the signed 64-bit lattice range.");

            long latticeX = (long)System.Math.Floor(x);
            long latticeY = (long)System.Math.Floor(y);
            double localX = x - latticeX;
            double localY = y - latticeY;
            double fadeX = Fade(localX);
            double fadeY = Fade(localY);

            double lowerLeft = Gradient(Hash(latticeX, latticeY, seed), localX, localY);
            double lowerRight = Gradient(Hash(latticeX + 1, latticeY, seed), localX - 1d, localY);
            double upperLeft = Gradient(Hash(latticeX, latticeY + 1, seed), localX, localY - 1d);
            double upperRight = Gradient(Hash(latticeX + 1, latticeY + 1, seed), localX - 1d, localY - 1d);

            double lower = Lerp(lowerLeft, lowerRight, fadeX);
            double upper = Lerp(upperLeft, upperRight, fadeX);
            return Lerp(lower, upper, fadeY) * InverseSquareRootOfTwo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Fade(double value) =>
            value * value * value * (value * (value * 6d - 15d) + 10d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Lerp(double start, double end, double amount) =>
            start + (end - start) * amount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Gradient(ulong hash, double x, double y)
        {
            switch (hash & 7UL)
            {
                case 0UL: return x;
                case 1UL: return -x;
                case 2UL: return y;
                case 3UL: return -y;
                case 4UL: return (x + y) * InverseSquareRootOfTwo;
                case 5UL: return (-x + y) * InverseSquareRootOfTwo;
                case 6UL: return (x - y) * InverseSquareRootOfTwo;
                default: return (-x - y) * InverseSquareRootOfTwo;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Hash(long x, long y, int seed)
        {
            unchecked
            {
                ulong hash = (uint)seed + 0x9E3779B97F4A7C15UL;
                hash ^= (ulong)x + 0x9E3779B97F4A7C15UL + (hash << 6) + (hash >> 2);
                hash ^= (ulong)y + 0xC2B2AE3D27D4EB4FUL + (hash << 6) + (hash >> 2);
                hash ^= hash >> 30;
                hash *= 0xBF58476D1CE4E5B9UL;
                hash ^= hash >> 27;
                hash *= 0x94D049BB133111EBUL;
                return hash ^ (hash >> 31);
            }
        }
    }
}
