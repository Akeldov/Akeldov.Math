using Akeldov.Math.Spatial2D.Imaging;
namespace Akeldov.Math.Hexes
{
    /// <summary>
    /// Defines rendering and output parameters for rasterizing a hex map topology.
    /// </summary>
    public readonly struct HexMapTopologyRasterizationOptions
    {
        /// <summary>
        /// Initializes rasterization options.
        /// </summary>
        /// <param name="margin">The non-negative raster margin in coordinate-space units.</param>
        /// <param name="curveWidth">The rendered edge width in coordinate-space units.</param>
        /// <param name="fadeDistance">The edge fade distance in coordinate-space units.</param>
        /// <param name="curveColor">The grayscale color assigned to edge centers.</param>
        /// <param name="backgroundColor">The grayscale color assigned outside the edge fade distance.</param>
        /// <param name="pixelsPerApothem">The raster resolution density in pixels per hex apothem.</param>
        public HexMapTopologyRasterizationOptions(
            float margin,
            float curveWidth,
            float fadeDistance,
            Gray8BitColor curveColor,
            Gray8BitColor backgroundColor,
            int pixelsPerApothem)
        {
            Margin = margin;
            CurveWidth = curveWidth;
            FadeDistance = fadeDistance;
            CurveColor = curveColor;
            BackgroundColor = backgroundColor;
            PixelsPerApothem = pixelsPerApothem;
        }

        /// <summary>
        /// Gets the raster margin added to each side of the map bounding box, in coordinate-space units.
        /// </summary>
        public float Margin { get; }

        /// <summary>
        /// Gets the rendered edge width in coordinate-space units.
        /// </summary>
        public float CurveWidth { get; }

        /// <summary>
        /// Gets the edge fade distance in coordinate-space units.
        /// </summary>
        public float FadeDistance { get; }

        /// <summary>
        /// Gets the grayscale color assigned to edge centers.
        /// </summary>
        public Gray8BitColor CurveColor { get; }

        /// <summary>
        /// Gets the grayscale color assigned outside the edge fade distance.
        /// </summary>
        public Gray8BitColor BackgroundColor { get; }

        /// <summary>
        /// Gets the raster resolution density in pixels per hex apothem.
        /// </summary>
        public int PixelsPerApothem { get; }
    }
}
