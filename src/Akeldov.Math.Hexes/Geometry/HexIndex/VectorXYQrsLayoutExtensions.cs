using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides extension methods for converting XY vectors to QRS vectors.
    /// </summary>
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Converts the vector to the QRS representation using unit-radius hex-grid axes.
        /// </summary>
        /// <remarks>
        /// Each unit QRS neighbor step corresponds to the center-to-center distance of a unit-radius regular hexagon,
        /// which is <c>sqrt(3)</c> coordinate-space units.
        /// </remarks>
        /// <param name="vector">The finite XY vector to convert.</param>
        /// <param name="layout">The hex layout used to select the QR axis orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ToVectorQRS(this VectorXY vector, Layout layout)
        {
            if (!vector.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(vector), vector, "Vector XY components must be finite.");

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorQRS(
                        0.5f * Constants.Apothem2Radius * vector.X - (1f / 3f) * vector.Y,
                        (2f / 3f) * vector.Y);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorQRS(
                        (2f / 3f) * vector.X,
                        0.5f * Constants.Apothem2Radius * vector.Y - (1f / 3f) * vector.X);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
