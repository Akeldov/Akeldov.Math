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
        /// Gets the world-space offset of a QRS hex index from the zero hex center.
        /// </summary>
        /// <param name="hexIndex">The QRS index whose offset is required.</param>
        /// <param name="hexRadius">The positive hex radius in coordinate-space units.</param>
        /// <param name="layout">The layout that determines the world-space basis orientation.</param>
        /// <returns>The offset from the zero hex center in coordinate-space units.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetHexOffset(this VectorQRSInt hexIndex, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            float hexApothem = Constants.Radius2Apothem * hexRadius;

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
