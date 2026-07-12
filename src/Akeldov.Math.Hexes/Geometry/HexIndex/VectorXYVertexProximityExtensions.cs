using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        /// <summary>
        /// Returns the containing hex index and the closest vertex of that hex.
        /// </summary>
        /// <param name="point">The Point value.</param>
        /// <param name="radius">The hex radius from center to vertex. The unit is the coordinate-space unit.</param>
        /// <param name="hexFieldOrigin">The center of the zero hex.</param>
        /// <param name="layout">The hex layout.</param>
        public static (VectorXYInt hexIndex, HexVertex hexVertex) GetClosestHexVertexIndex(
            this PointXY point,
            float radius,
            VectorXY hexFieldOrigin,
            Layout layout)
        {
            float apothem = radius.ConvertHexRadiusToApothem();
            var hexIndex = point.ToXYIndex(radius, hexFieldOrigin, layout);
            var hexCenter = hexIndex.GetHexCenter(apothem, radius, hexFieldOrigin, layout);
            var closestVertexIndex = point.GetClosestVertexIndex(radius, hexCenter, layout);

            return (hexIndex, (HexVertex)closestVertexIndex);
        }

        /// <summary>
        /// Gets a value derived from the specified hex-grid data.
        /// </summary>
        /// <param name="point">The Point value.</param>
        /// <param name="radius">The Radius value.</param>
        /// <param name="hexCenter">The HexCenter value.</param>
        /// <param name="layout">The Layout value.</param>
        public static int GetClosestVertexIndex(
            this PointXY point,
            float radius,
            VectorXY hexCenter,
            Layout layout)
        {
            if (float.IsNaN(point.X) || float.IsInfinity(point.X) ||
                float.IsNaN(point.Y) || float.IsInfinity(point.Y))
                throw new ArgumentOutOfRangeException(nameof(point), point, "Point coordinates must be finite.");

            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hex radius must be finite and positive.");

            if (!hexCenter.IsFinite)
                throw new ArgumentOutOfRangeException(nameof(hexCenter), hexCenter, "Hex center components must be finite.");

            var normalizedHexVertices = GetNormalizedHexVertices(layout);

            float minDist = float.MaxValue;
            int closestVertexIndex = 0;

            for (int i = 0; i < 6; i++)
            {
                VectorXY vertex = hexCenter + normalizedHexVertices[i] * radius;
                float dist = Distance(point, vertex);

                if (dist < minDist)
                {
                    minDist = dist;
                    closestVertexIndex = i;
                }
            }

            return closestVertexIndex;
        }

        private static float Distance(PointXY point, VectorXY vertex)
        {
            float x = point.X - vertex.X;
            float y = point.Y - vertex.Y;
            return MathF.Sqrt(x * x + y * y);
        }
    }
}
