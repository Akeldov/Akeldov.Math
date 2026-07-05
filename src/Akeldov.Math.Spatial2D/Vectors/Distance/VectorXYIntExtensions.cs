using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Returns the Euclidean distance between two integer vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The Euclidean distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this VectorXYInt a, VectorXYInt b)
        {
            float dx = (float)a.X - b.X;
            float dy = (float)a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the Euclidean distance between an integer vector and a floating-point vector.
        /// </summary>
        /// <param name="a">The integer vector.</param>
        /// <param name="b">The floating-point vector.</param>
        /// <returns>The Euclidean distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this VectorXYInt a, VectorXY b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }
}
