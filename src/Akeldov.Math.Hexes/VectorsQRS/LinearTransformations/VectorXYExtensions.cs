using Akeldov.Math.Spatial2D;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Vectors.QRS
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Rotates an XY vector counterclockwise about the origin.
        /// </summary>
        /// <param name="point">The vector to rotate.</param>
        /// <param name="angle">The sixfold counterclockwise rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY Rotate(this VectorXY point, SixfoldAngle angle)
        {
            float cos = angle.Cos();
            float sin = angle.Sin();

            float x = point.X * cos - point.Y * sin;
            float y = point.X * sin + point.Y * cos;

            return new VectorXY(x, y);
        }
    }
}
