using Akeldov.Math.Spatial2D;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides extension methods for hex-grid operations.
    /// </summary>
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Transforms the value using the specified parameters.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="angle">The angle value.</param>
        /// <param name="offset">The offset value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXY point, SixfoldAngle angle, VectorXY offset)
        {
            var rotated = point.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Transforms the value using the specified parameters.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="angle">The angle value.</param>
        /// <param name="offset">The offset value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXY point, SixfoldAngle angle, VectorXYInt offset)
        {
            var rotated = point.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Transforms the value using the specified parameters.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="scaleFactor">The scaleFactor value.</param>
        /// <param name="angle">The angle value.</param>
        /// <param name="offset">The offset value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXY point, float scaleFactor, SixfoldAngle angle, VectorXY offset)
        {
            var scaled = point * scaleFactor;
            var rotated = scaled.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Transforms the value using the specified parameters.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="scaleFactor">The scaleFactor value.</param>
        /// <param name="angle">The angle value.</param>
        /// <param name="offset">The offset value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXY point, float scaleFactor, SixfoldAngle angle, VectorXYInt offset)
        {
            var scaled = point * scaleFactor;
            var rotated = scaled.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Rotates the value using the specified angle.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="pivot">The pivot value.</param>
        /// <param name="angle">The angle value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Rotate(this VectorXY point, VectorXY pivot, SixfoldAngle angle)
        {
            float cos = angle.Cos();
            float sin = angle.Sin();

            VectorXY offset = point - pivot;

            float rotatedX = offset.X * cos - offset.Y * sin;
            float rotatedY = offset.X * sin + offset.Y * cos;

            return new VectorXY(rotatedX, rotatedY) + pivot;
        }

        /// <summary>
        /// Rotates the value using the specified angle.
        /// </summary>
        /// <param name="point">The point value.</param>
        /// <param name="pivot">The pivot value.</param>
        /// <param name="angle">The angle value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Rotate(this VectorXY point, VectorXYInt pivot, SixfoldAngle angle)
        {
            float cos = angle.Cos();
            float sin = angle.Sin();

            VectorXY offset = point - pivot;

            float rotatedX = offset.X * cos - offset.Y * sin;
            float rotatedY = offset.X * sin + offset.Y * cos;

            return new VectorXY(rotatedX, rotatedY) + pivot;
        }
    }
}
