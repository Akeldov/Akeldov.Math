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
        /// Converts the vector to the QRS representation using the specified map geometry.
        /// </summary>
        /// <remarks>
        /// The geometry origin is not applied because the converted value is a vector, not a point.
        /// </remarks>
        /// <param name="vector">The vector value.</param>
        /// <param name="geometry">The hex map geometry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ToVectorQRS(this VectorXY vector, HexMapGeometry geometry)
        {
            if (!vector.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(vector), vector, "Vector XY components must be finite.");

            if (float.IsNaN(geometry.Apothem) || float.IsInfinity(geometry.Apothem) || geometry.Apothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(geometry), geometry, "Hex map geometry apothem must be finite and positive.");

            return ToVectorQRS(vector, geometry.Radius, geometry.Layout);
        }

        /// <summary>
        /// Converts the vector to the QRS representation using normalized hex-grid axes.
        /// </summary>
        /// <remarks>
        /// The normalized basis maps each unit QR axis step to an XY vector with a length of one coordinate-space unit.
        /// </remarks>
        /// <param name="vector">The vector value.</param>
        /// <param name="layout">The hex layout used to select the QR axis orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorQRS ToVectorQRS(this VectorXY vector, Layout layout)
        {
            if (!vector.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(vector), vector, "Vector XY components must be finite.");

            return ToVectorQRS(vector, 0.5f * Constants.Apothem2Radius, layout);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VectorQRS ToVectorQRS(VectorXY vector, float hexRadius, Layout layout)
        {
            float invertedHexRadius = 1f / hexRadius;

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorQRS(
                        (0.5773502588f * vector.X - 0.3333333333f * vector.Y) * invertedHexRadius,
                        0.6666666666f * vector.Y * invertedHexRadius);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorQRS(
                        0.6666666666f * vector.X * invertedHexRadius,
                        (0.5773502588f * vector.Y - 0.3333333333f * vector.X) * invertedHexRadius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
