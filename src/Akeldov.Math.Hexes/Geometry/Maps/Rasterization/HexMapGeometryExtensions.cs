using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides rasterization extension methods for hex map geometry.
    /// </summary>
    public static class HexMapGeometryExtensions
    {
        /// <summary>
        /// Rasterizes unique hex edge segments for the whole hex map geometry.
        /// </summary>
        /// <param name="hexMapGeometry">The hex map geometry to rasterize.</param>
        /// <param name="curveWidth">The rendered edge width. The unit is the coordinate-space unit.</param>
        /// <param name="fadeDistance">The edge fade distance. The unit is the coordinate-space unit.</param>
        /// <param name="curveColor">The color assigned to edge centers.</param>
        /// <param name="backgroundColor">The color assigned outside the edge fade distance.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <returns>A raster of the hex map edge segments.</returns>
        public static SpatialRaster<byte> Rasterize(
            this HexMapGeometry hexMapGeometry,
            float curveWidth,
            float fadeDistance,
            byte curveColor,
            byte backgroundColor,
            int pixelsPerApothem)
        {
            var spatialRasterGrid = hexMapGeometry.ToSpatialRasterGrid(pixelsPerApothem);

            var res = hexMapGeometry
                .ToHexEdgeSegments()
                .Rasterize(curveWidth, fadeDistance, curveColor, backgroundColor, spatialRasterGrid);

            return res;
        }
    }
}
