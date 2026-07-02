using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        public static VectorXY GetHexCenter(int q, int r, float hexApothem, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            var origin = GetAxialOrigin(hexApothem, hexRadius, layout);
            return new VectorQRSInt(q, r).GetHexOffset(hexApothem, hexRadius, layout) + origin;
        }

        /// <summary>
        /// Gets the six vertex positions for the specified hex index.
        /// </summary>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static VectorXY[] GetHexVertices(int q, int r, float hexApothem, float hexRadius, Layout layout)
        {
            if (float.IsNaN(hexApothem) || float.IsInfinity(hexApothem) || hexApothem <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexApothem), hexApothem, "Hex apothem must be finite and positive.");

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            return GetHexCenter(q, r, hexApothem, hexRadius, layout).GetHexVertices(hexRadius, layout);
        }

        [Obsolete("Use GetHexVertices instead.")]
        public static VectorXY[] GetHexVertexes(int q, int r, float hexApothem, float hexRadius, Layout layout)
        {
            return GetHexVertices(q, r, hexApothem, hexRadius, layout);
        }

        /// <summary>
        /// Gets the six vertex positions for the specified hex center.
        /// </summary>
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

        [Obsolete("Use GetHexVertices instead.")]
        public static VectorXY[] GetHexVertexes(this VectorXY hexCenter, float hexRadius, Layout layout)
        {
            return hexCenter.GetHexVertices(hexRadius, layout);
        }

        private static VectorXY GetAxialOrigin(float hexApothem, float hexRadius, Layout layout)
        {
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
