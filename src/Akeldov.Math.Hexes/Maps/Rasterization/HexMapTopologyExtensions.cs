using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes
{
    public static class HexMapTopologyExtensions
    {
        public static SpatialRaster<byte> Rasterize(
            this HexMapTopology hexMapTopology,
            float apothem,
            float margin,
            float curveWidth,            
            float fadeDistance,
            byte curveColor,
            byte backgroundColor,
            int pixelsPerApothem)
        {
            var hexMapGeometry = new HexMapGeometry(hexMapTopology.Width, hexMapTopology.Height, new VectorXY(0, 0), apothem, hexMapTopology.Layout);
            var spatialRasterGrid = hexMapGeometry.ToSpatialRasterGrid(pixelsPerApothem, margin);

            var res = hexMapGeometry
                .ToHexEdgeSegments()
                .Rasterize(curveWidth, fadeDistance, curveColor, backgroundColor, spatialRasterGrid);

            return res;
        }
    }
}
