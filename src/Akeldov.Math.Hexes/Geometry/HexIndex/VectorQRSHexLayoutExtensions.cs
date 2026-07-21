using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides extension methods for converting QRS vectors to XY vectors.
    /// </summary>
    public static partial class VectorQRSExtensions
    {
        /// <summary>
        /// Converts the vector to the XY representation using unit-radius hex-grid axes.
        /// </summary>
        /// <remarks>
        /// Each unit QRS neighbor step maps to the center-to-center distance of a unit-radius regular hexagon,
        /// which is <c>sqrt(3)</c> coordinate-space units.
        /// </remarks>
        /// <param name="vector">The finite QRS vector to convert.</param>
        /// <param name="layout">The hex layout used to select the QR axis orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ToVectorXY(this VectorQRS vector, Layout layout)
        {
            if (float.IsNaN(vector.Q) || float.IsInfinity(vector.Q) ||
                float.IsNaN(vector.R) || float.IsInfinity(vector.R) ||
                float.IsNaN(vector.S) || float.IsInfinity(vector.S))
                throw new ArgumentOutOfRangeException(nameof(vector), vector, "Vector QRS components must be finite.");

            float hexApothem = Constants.Radius2Apothem;

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorXY(
                        2f * hexApothem * vector.Q + hexApothem * vector.R,
                        1.5f * vector.R);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorXY(
                        1.5f * vector.Q,
                        2f * hexApothem * vector.R + hexApothem * vector.Q);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
