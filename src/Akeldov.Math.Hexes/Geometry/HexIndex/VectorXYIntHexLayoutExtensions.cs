using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;
using System.Runtime.CompilerServices;

namespace Akeldov.Math.Hexes.Geometry
{
    /// <summary>
    /// Provides geometry extension methods for XY hex indexes.
    /// </summary>
    public static partial class VectorXYIntExtensions
    {
        /// <summary>
        /// Gets the center of an offset-indexed hex using the layout's default zero-hex origin.
        /// </summary>
        /// <param name="index">The XY offset index of the hex.</param>
        /// <param name="hexRadius">The positive hex radius in coordinate-space units.</param>
        /// <param name="layout">The offset layout of the index.</param>
        /// <returns>The world-space center of the hex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetHexCenter(this VectorXYInt index, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            return index.GetHexCenter(hexRadius, GetOffsetOrigin(hexRadius, layout), layout);
        }

        /// <summary>
        /// Gets the center of an offset-indexed hex relative to a specified zero-hex center.
        /// </summary>
        /// <param name="index">The XY offset index of the hex.</param>
        /// <param name="hexRadius">The positive hex radius in coordinate-space units.</param>
        /// <param name="origin">The world-space center of the zero hex.</param>
        /// <param name="layout">The offset layout of the index.</param>
        /// <returns>The world-space center of the hex.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorXY GetHexCenter(this VectorXYInt index, float hexRadius, VectorXY origin, Layout layout)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            if (!origin.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(origin), origin, "Hex origin components must be finite.");

            float hexApothem = Constants.Radius2Apothem * hexRadius;

            switch (layout)
            {
                case Layout.OddR:
                    return new VectorXY(
                        origin.X + index.X * 2f * hexApothem + ((index.Y & 1) == 1 ? hexApothem : 0f),
                        origin.Y + 1.5f * hexRadius * index.Y);
                case Layout.EvenR:
                    return new VectorXY(
                        origin.X + index.X * 2f * hexApothem + ((index.Y & 1) == 1 ? -hexApothem : 0f),
                        origin.Y + 1.5f * hexRadius * index.Y);
                case Layout.OddQ:
                    return new VectorXY(
                        origin.X + 1.5f * hexRadius * index.X,
                        origin.Y + index.Y * 2f * hexApothem + ((index.X & 1) == 1 ? hexApothem : 0f));
                case Layout.EvenQ:
                    return new VectorXY(
                        origin.X + 1.5f * hexRadius * index.X,
                        origin.Y + index.Y * 2f * hexApothem + ((index.X & 1) == 1 ? -hexApothem : 0f));
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }

        private static VectorXY GetOffsetOrigin(float hexRadius, Layout layout)
        {
            float hexApothem = Constants.Radius2Apothem * hexRadius;

            switch (layout)
            {
                case Layout.OddR:
                case Layout.EvenR:
                    return new VectorXY(hexApothem, hexRadius);
                case Layout.OddQ:
                case Layout.EvenQ:
                    return new VectorXY(hexRadius, hexApothem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout));
            }
        }
    }
}
