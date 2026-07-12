using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides geometry extension methods for QRS hex indexes.
    /// </summary>
    public static partial class VectorQRSIntExtensions
    {
        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="hexIndex">The hexIndex value.</param>
        /// <param name="hexApothem">The hexApothem value.</param>
        /// <param name="hexRadius">The hexRadius value.</param>
        /// <param name="layout">The layout value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetHexOffset(this VectorQRSInt hexIndex, float hexApothem, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorXY(
                        2f * hexApothem * hexIndex.Q + hexApothem * hexIndex.R,
                        1.5f * hexRadius * hexIndex.R);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorXY(
                        1.5f * hexRadius * hexIndex.Q,
                        2f * hexApothem * hexIndex.R + hexApothem * hexIndex.Q);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
