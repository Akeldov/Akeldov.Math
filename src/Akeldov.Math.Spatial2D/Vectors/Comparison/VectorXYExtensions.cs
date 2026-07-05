using System.Runtime.CompilerServices;

namespace Akeldov.Math.Spatial2D
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Indicates whether two vectors are within the specified Euclidean distance tolerance.
        /// </summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <param name="epsilon">The inclusive distance tolerance.</param>
        /// <returns><see langword="true"/> when the vectors are within tolerance; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostEquals(this VectorXY left, VectorXY right, float epsilon = GeometryConstants.GeometryEpsilon)
        {
            float dx = left.X - right.X;
            float dy = left.Y - right.Y;
            return dx * dx + dy * dy <= epsilon * epsilon;
        }
    }
}
