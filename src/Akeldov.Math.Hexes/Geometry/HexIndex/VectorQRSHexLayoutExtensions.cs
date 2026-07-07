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
        /// Converts the vector to the XY representation using the specified map geometry.
        /// </summary>
        /// <remarks>
        /// The geometry origin is not applied because the converted value is a vector, not a point.
        /// </remarks>
        /// <param name="vector">The vector value.</param>
        /// <param name="geometry">The hex map geometry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ToVectorXY(this VectorQRS vector, HexMapGeometry geometry)
        {
            if (float.IsNaN(vector.Q) || float.IsInfinity(vector.Q) ||
                float.IsNaN(vector.R) || float.IsInfinity(vector.R) ||
                float.IsNaN(vector.S) || float.IsInfinity(vector.S))
                throw new ArgumentOutOfRangeException(nameof(vector), vector, "Vector QRS components must be finite.");

            if (float.IsNaN(geometry.Apothem) || float.IsInfinity(geometry.Apothem) || geometry.Apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry apothem must be finite and positive.");

            return ToVectorXY(vector, geometry.Apothem, geometry.Radius, geometry.Layout);
        }

        /// <summary>
        /// Converts the vector to the XY representation using normalized hex-grid axes.
        /// </summary>
        /// <remarks>
        /// The normalized basis uses a hex radius of one coordinate-space unit and a hex apothem of sqrt(3) / 2.
        /// </remarks>
        /// <param name="vector">The vector value.</param>
        /// <param name="layout">The hex layout used to select the QR axis orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY ToVectorXY(this VectorQRS vector, Layout layout)
        {
            if (float.IsNaN(vector.Q) || float.IsInfinity(vector.Q) ||
                float.IsNaN(vector.R) || float.IsInfinity(vector.R) ||
                float.IsNaN(vector.S) || float.IsInfinity(vector.S))
                throw new ArgumentOutOfRangeException(nameof(vector), vector, "Vector QRS components must be finite.");

            return ToVectorXY(vector, Constants.Radius2Apothem, 1f, layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorXY ToVectorXY(VectorQRS vector, float hexApothem, float hexRadius, Layout layout)
        {
            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorXY(
                        2f * hexApothem * vector.Q + hexApothem * vector.R,
                        1.5f * hexRadius * vector.R);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorXY(
                        1.5f * hexRadius * vector.Q,
                        2f * hexApothem * vector.R + hexApothem * vector.Q);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
