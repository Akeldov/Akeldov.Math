using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System;

namespace Akeldov.Math.Hexes.Topology.Maps.BoundingBox
{
    public static class HexAdjacencyMapExtensions
    {
        public static VectorXY GetBoundingBoxSize(this HexAdjacencyMap hexAdjacencyMap, float hexRadius)
        {
            if (hexAdjacencyMap == null)
                throw new ArgumentNullException(nameof(hexAdjacencyMap));

            if (float.IsNaN(hexRadius) || float.IsInfinity(hexRadius) || hexRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(hexRadius), hexRadius, "Hex radius must be finite and positive.");

            var resolution = new VectorXYInt(hexAdjacencyMap.Width, hexAdjacencyMap.Height);
            return resolution.BoundingBox(
                hexRadius.ConvertHexRadiusToApothem(),
                hexRadius,
                hexAdjacencyMap.Layout);
        }
    }
}
