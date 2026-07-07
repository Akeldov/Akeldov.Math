using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes
{
    public static class HexMapGeometryExtensions
    {
        public static SpatialRaster<byte> Rasterize(
            this HexMapGeometry hexMapGeometry,
            float curveWidth,
            float fadeDistance,
            byte curveColor,
            byte backgroundColor,
            int pixelsPerApothem)
        {
            var topology = new HexMapTopology(hexMapGeometry.Width, hexMapGeometry.Height, hexMapGeometry.Layout);
            var spatialRasterGrid = hexMapGeometry.ToSpatialRasterGrid(pixelsPerApothem);

            var res = topology
                .ToHexEdgeSegments(hexMapGeometry.Apothem)
                .Rasterize(curveWidth, fadeDistance, curveColor, backgroundColor, spatialRasterGrid);

            return res;
        }
    }
}
