using Akeldov.Math.Spatial2D;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    /// <summary>
    /// Provides affine transformations for integer XY vectors using sixfold rotations.
    /// </summary>
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Rotates an integer vector about the origin, then translates it by a floating-point offset.
        /// </summary>
        /// <param name="point">The integer vector to transform.</param>
        /// <param name="angle">The counterclockwise rotation.</param>
        /// <param name="offset">The translation applied after rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXYInt point, SixfoldAngle angle, VectorXY offset)
        {
            var rotated = point.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Rotates an integer vector about the origin, then translates it by an integer offset.
        /// </summary>
        /// <param name="point">The integer vector to transform.</param>
        /// <param name="angle">The counterclockwise rotation.</param>
        /// <param name="offset">The integer translation applied after rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXYInt point, SixfoldAngle angle, VectorXYInt offset)
        {
            var rotated = point.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Scales and rotates an integer vector about the origin, then translates it by a floating-point offset.
        /// </summary>
        /// <param name="point">The integer vector to transform.</param>
        /// <param name="scaleFactor">The uniform scale applied before rotation.</param>
        /// <param name="angle">The counterclockwise rotation applied after scaling.</param>
        /// <param name="offset">The translation applied last.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXYInt point, float scaleFactor, SixfoldAngle angle, VectorXY offset)
        {
            var scaled = point * scaleFactor;
            var rotated = scaled.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Scales and rotates an integer vector about the origin, then translates it by an integer offset.
        /// </summary>
        /// <param name="point">The integer vector to transform.</param>
        /// <param name="scaleFactor">The uniform scale applied before rotation.</param>
        /// <param name="angle">The counterclockwise rotation applied after scaling.</param>
        /// <param name="offset">The integer translation applied last.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Transform(this VectorXYInt point, float scaleFactor, SixfoldAngle angle, VectorXYInt offset)
        {
            var scaled = point * scaleFactor;
            var rotated = scaled.Rotate(angle);
            return rotated + offset;
        }

        /// <summary>
        /// Rotates an integer vector counterclockwise about an integer pivot.
        /// </summary>
        /// <param name="point">The integer vector to rotate.</param>
        /// <param name="pivot">The integer center of rotation.</param>
        /// <param name="angle">The counterclockwise rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Rotate(this VectorXYInt point, VectorXYInt pivot, SixfoldAngle angle)
        {
            float cos = angle.Cos();
            float sin = angle.Sin();

            VectorXY offset = point - pivot;

            float rotatedX = offset.X * cos - offset.Y * sin;
            float rotatedY = offset.X * sin + offset.Y * cos;

            return new VectorXY(rotatedX, rotatedY) + pivot;
        }

        /// <summary>
        /// Rotates an integer vector counterclockwise about a floating-point pivot.
        /// </summary>
        /// <param name="point">The integer vector to rotate.</param>
        /// <param name="pivot">The center of rotation.</param>
        /// <param name="angle">The counterclockwise rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Rotate(this VectorXYInt point, VectorXY pivot, SixfoldAngle angle)
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
