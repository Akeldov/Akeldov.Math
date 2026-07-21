using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Gets the world-space center of a QRS-indexed hex using the layout's default zero-hex origin.
        /// </summary>
        /// <param name="q">The Q component of the hex index.</param>
        /// <param name="r">The R component of the hex index.</param>
        /// <param name="hexRadius">The positive hex radius in coordinate-space units.</param>
        /// <param name="layout">The layout that determines the world-space basis orientation.</param>
        public static VectorXY GetHexCenter(int q, int r, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            var origin = GetAxialOrigin(hexRadius, layout);
            return new VectorQRSInt(q, r).GetHexOffset(hexRadius, layout) + origin;
        }

        /// <summary>
        /// Gets the six vertex positions for the specified hex index.
        /// </summary>
        /// <param name="q">The Q component of the hex index.</param>
        /// <param name="r">The R component of the hex index.</param>
        /// <param name="hexRadius">The positive hex radius in coordinate-space units.</param>
        /// <param name="layout">The layout that determines vertex orientation.</param>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static VectorXY[] GetHexVertices(int q, int r, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            return GetHexCenter(q, r, hexRadius, layout).GetHexVertices(hexRadius, layout);
        }

        /// <summary>
        /// Gets the six vertex positions for the specified hex center.
        /// </summary>
        /// <param name="hexCenter">The world-space center of the hex.</param>
        /// <param name="hexRadius">The positive hex radius in coordinate-space units.</param>
        /// <param name="layout">The layout that determines vertex orientation.</param>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static VectorXY[] GetHexVertices(this VectorXY hexCenter, float hexRadius, Layout layout)
        {
            if (!hexCenter.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexCenter), hexCenter, "Hex center components must be finite.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            var normalizedHexVertices = GetNormalizedHexVertices(layout);
            var vertices = new VectorXY[6];
            for (int i = 0; i < 6; i++)
            {
                vertices[i] = hexCenter + normalizedHexVertices[i] * hexRadius;
            }
            return vertices;
        }

        private static VectorXY GetAxialOrigin(float hexRadius, Layout layout)
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
