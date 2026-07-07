using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Provides rasterization extension methods for hex map topology values.
    /// </summary>
    public static class HexMapTopologyExtensions
    {
        /// <summary>
        /// Rasterizes unique hex edge segments for the whole topology with the zero hex center at the coordinate origin.
        /// </summary>
        /// <param name="hexMapTopology">The topology to rasterize.</param>
        /// <param name="apothem">The hex apothem. The unit is the coordinate-space unit.</param>
        /// <param name="margin">The non-negative raster margin added to each side of the map bounding box. The unit is the coordinate-space unit.</param>
        /// <param name="curveWidth">The rendered edge width. The unit is the coordinate-space unit.</param>
        /// <param name="fadeDistance">The edge fade distance. The unit is the coordinate-space unit.</param>
        /// <param name="curveColor">The color assigned to edge centers.</param>
        /// <param name="backgroundColor">The color assigned outside the edge fade distance.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <returns>A raster of the hex map edge segments.</returns>
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
            return hexMapTopology.Rasterize(
                apothem,
                VectorXY.Zero,
                margin,
                curveWidth,
                fadeDistance,
                curveColor,
                backgroundColor,
                pixelsPerApothem);
        }

        /// <summary>
        /// Rasterizes unique hex edge segments for the whole topology with the specified zero hex center.
        /// </summary>
        /// <param name="hexMapTopology">The topology to rasterize.</param>
        /// <param name="apothem">The hex apothem. The unit is the coordinate-space unit.</param>
        /// <param name="origin">The center of the zero hex.</param>
        /// <param name="margin">The non-negative raster margin added to each side of the map bounding box. The unit is the coordinate-space unit.</param>
        /// <param name="curveWidth">The rendered edge width. The unit is the coordinate-space unit.</param>
        /// <param name="fadeDistance">The edge fade distance. The unit is the coordinate-space unit.</param>
        /// <param name="curveColor">The color assigned to edge centers.</param>
        /// <param name="backgroundColor">The color assigned outside the edge fade distance.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        /// <returns>A raster of the hex map edge segments.</returns>
        public static SpatialRaster<byte> Rasterize(
            this HexMapTopology hexMapTopology,
            float apothem,
            VectorXY origin,
            float margin,
            float curveWidth,
            float fadeDistance,
            byte curveColor,
            byte backgroundColor,
            int pixelsPerApothem)
        {
            var hexMapGeometry = new HexMapGeometry(hexMapTopology.Width, hexMapTopology.Height, origin, apothem, hexMapTopology.Layout);
            var spatialRasterGrid = hexMapGeometry.ToSpatialRasterGrid(pixelsPerApothem, margin);

            var res = hexMapGeometry
                .ToHexEdgeSegments()
                .Rasterize(curveWidth, fadeDistance, curveColor, backgroundColor, spatialRasterGrid);

            return res;
        }
    }
}
