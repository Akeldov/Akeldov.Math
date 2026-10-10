using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
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
    }
}
