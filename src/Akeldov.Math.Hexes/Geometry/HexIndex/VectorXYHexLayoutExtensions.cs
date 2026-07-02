using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        public static VectorXY GetHexCenter(int q, int r, float hexApothem, float hexRadius, Layout layout)
        {
            var origin = GetAxialOrigin(hexApothem, hexRadius, layout);
            return new VectorQRSInt(q, r).GetHexOffset(hexApothem, hexRadius, layout) + origin;
        }

        /// <summary>
        /// Gets the six vertex positions for the specified hex index.
        /// </summary>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static VectorXY[] GetHexVertexes(int q, int r, float hexApothem, float hexRadius, Layout layout)
        {
            return GetHexCenter(q, r, hexApothem, hexRadius, layout).GetHexVertexes(hexRadius, layout);
        }

        /// <summary>
        /// Gets the six vertex positions for the specified hex center.
        /// </summary>
        /// <returns>A new, mutable array owned by the caller.</returns>
        public static VectorXY[] GetHexVertexes(this VectorXY hexCenter, float hexRadius, Layout layout)
        {
            var normalizedHexVertexes = GetNormalizedHexVertexes(layout);
            var vertexes = new VectorXY[6];
            for (int i = 0; i < 6; i++)
            {
                vertexes[i] = hexCenter + normalizedHexVertexes[i] * hexRadius;
            }
            return vertexes;
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
