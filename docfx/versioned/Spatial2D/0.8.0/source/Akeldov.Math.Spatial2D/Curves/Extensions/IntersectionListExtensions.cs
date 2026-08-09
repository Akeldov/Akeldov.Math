using System.Collections.Generic;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Curves
{
    internal static class IntersectionListExtensions
    {
        public static void AddDistinct(
            this List<PointXY> intersections,
            PointXY point,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].AlmostEquals(point, geometryEpsilon))
                    return;
            }

            intersections.Add(point);
        }
    }
}
