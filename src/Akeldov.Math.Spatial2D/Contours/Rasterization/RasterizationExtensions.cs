using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Spatial2D.Contours
{
    /// <summary>
    /// Provides rasterization extensions for contours.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a contour stroke into an 8-bit grayscale raster with a black background.
        /// </summary>
        /// <param name="contour">The contour to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The grayscale stroke color.</param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster with the contour drawn over black.</returns>
        public static SpatialRaster<Gray8BitColor> Rasterize(
            this IContour contour,
            float width,
            float edgeFalloff,
            Gray8BitColor color,
            SpatialRasterGrid spatialRasterGrid)
        {
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));

            return contour.Rasterize(
                width,
                edgeFalloff,
                color,
                default(Gray8BitColor),
                spatialRasterGrid);
        }

        /// <summary>
        /// Rasterizes a contour stroke into a 16-bit grayscale raster with a black background.
        /// </summary>
        /// <param name="contour">The contour to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The grayscale stroke color.</param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster with the contour drawn over black.</returns>
        public static SpatialRaster<Gray16BitColor> Rasterize(
            this IContour contour,
            float width,
            float edgeFalloff,
            Gray16BitColor color,
            SpatialRasterGrid spatialRasterGrid)
        {
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));

            return contour.Rasterize(
                width,
                edgeFalloff,
                color,
                default(Gray16BitColor),
                spatialRasterGrid);
        }

        /// <summary>
        /// Rasterizes a contour stroke into an 8-bit RGBA raster with a transparent background.
        /// </summary>
        /// <param name="contour">The contour to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit RGBA raster with the contour drawn over transparency.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="edgeFalloff"/> is negative, NaN, or infinite,
        /// or when <paramref name="spatialRasterGrid"/> is invalid.
        /// </exception>
        public static SpatialRaster<RGBA8BitColor> Rasterize(
            this IContour contour,
            float width,
            float edgeFalloff,
            RGBA8BitColor color,
            SpatialRasterGrid spatialRasterGrid)
        {
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));

            return contour.Rasterize(
                width,
                edgeFalloff,
                color,
                RGBA8BitColor.Transparent,
                spatialRasterGrid);
        }

        /// <summary>
        /// Rasterizes a contour stroke into a 16-bit RGBA raster with a transparent background.
        /// </summary>
        /// <param name="contour">The contour to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="spatialRasterGrid">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit RGBA raster with the contour drawn over transparency.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contour"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="edgeFalloff"/> is negative, NaN, or infinite,
        /// or when <paramref name="spatialRasterGrid"/> is invalid.
        /// </exception>
        public static SpatialRaster<RGBA16BitColor> Rasterize(
            this IContour contour,
            float width,
            float edgeFalloff,
            RGBA16BitColor color,
            SpatialRasterGrid spatialRasterGrid)
        {
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));

            return contour.Rasterize(
                width,
                edgeFalloff,
                color,
                RGBA16BitColor.Transparent,
                spatialRasterGrid);
        }
    }
}
