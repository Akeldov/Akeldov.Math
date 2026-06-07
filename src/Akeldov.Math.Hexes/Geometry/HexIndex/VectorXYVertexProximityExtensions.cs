using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Geometry
{
    public static partial class VectorXYExtensions
    {
        public static (VectorXYInt hexIndex, HexVertex hexVertex) GetClosestVertexIndex(
            this PointXY point,
            float apothem,
            float radius,
            VectorXY hexFieldOrigin,
            Layout layout)
        {
            var hexIndex = point.ToXYIndex(radius, hexFieldOrigin, layout);
            var hexCenter = hexIndex.GetHexCenter(apothem, radius, hexFieldOrigin, layout);
            var closestVertexIndex = point.GetClosestVertexIndex(radius, hexCenter, layout);

            return (hexIndex, (HexVertex)closestVertexIndex);
        }

        public static int GetClosestVertexIndex(
            this PointXY point,
            float radius,
            VectorXY hexCenter,
            Layout layout)
        {
            var normalizedHexVertexes = GetNormalizedHexVertexes(layout);

            float minDist = float.MaxValue;
            int closestVertexIndex = 0;

            for (int i = 0; i < 6; i++)
            {
                VectorXY vertex = hexCenter + normalizedHexVertexes[i] * radius;
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
