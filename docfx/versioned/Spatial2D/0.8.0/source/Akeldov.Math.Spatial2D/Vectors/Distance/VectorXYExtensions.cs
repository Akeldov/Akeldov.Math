using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Returns the Euclidean distance between two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The Euclidean distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this VectorXY a, VectorXY b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the Euclidean distance between a floating-point vector and an integer vector.
        /// </summary>
        /// <param name="a">The floating-point vector.</param>
        /// <param name="b">The integer vector.</param>
        /// <returns>The Euclidean distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this VectorXY a, VectorXYInt b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }
}
