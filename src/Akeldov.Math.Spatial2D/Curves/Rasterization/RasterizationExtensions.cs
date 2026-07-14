using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;

namespace Akeldov.Math.Spatial2D.Curves
{
    /// <summary>
    /// Provides convenient stroke rasterization extensions for curves.
    /// </summary>
    public static partial class RasterizationExtensions
    {
        /// <summary>
        /// Rasterizes a curve stroke into an 8-bit grayscale raster with a black background.
        /// </summary>
        /// <typeparam name="TCurve">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The grayscale stroke color.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit grayscale raster with the curve drawn over black.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="edgeFalloff"/> is negative, NaN, or infinite,
        /// or when <paramref name="rasterGeometry"/> is invalid.
        /// </exception>
        public static SpatialRaster<Gray8BitColor> Rasterize<TCurve>(
            this TCurve curve,
            float width,
            float edgeFalloff,
            Gray8BitColor color,
            RasterGeometry rasterGeometry)
            where TCurve : ICurve
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));

            return curve.Rasterize(
                width,
                edgeFalloff,
                color,
                default(Gray8BitColor),
                rasterGeometry);
        }

        /// <summary>
        /// Rasterizes a curve stroke into a 16-bit grayscale raster with a black background.
        /// </summary>
        /// <typeparam name="TCurve">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The grayscale stroke color.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit grayscale raster with the curve drawn over black.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="edgeFalloff"/> is negative, NaN, or infinite,
        /// or when <paramref name="rasterGeometry"/> is invalid.
        /// </exception>
        public static SpatialRaster<Gray16BitColor> Rasterize<TCurve>(
            this TCurve curve,
            float width,
            float edgeFalloff,
            Gray16BitColor color,
            RasterGeometry rasterGeometry)
            where TCurve : ICurve
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));

            return curve.Rasterize(
                width,
                edgeFalloff,
                color,
                default(Gray16BitColor),
                rasterGeometry);
        }

        /// <summary>
        /// Rasterizes a curve stroke into an 8-bit RGBA raster with a transparent background.
        /// </summary>
        /// <typeparam name="TCurve">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>An 8-bit RGBA raster with the curve drawn over transparency.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="edgeFalloff"/> is negative, NaN, or infinite,
        /// or when <paramref name="rasterGeometry"/> is invalid.
        /// </exception>
        public static SpatialRaster<RGBA8BitColor> Rasterize<TCurve>(
            this TCurve curve,
            float width,
            float edgeFalloff,
            RGBA8BitColor color,
            RasterGeometry rasterGeometry)
            where TCurve : ICurve
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));

            return curve.Rasterize(
                width,
                edgeFalloff,
                color,
                RGBA8BitColor.Transparent,
                rasterGeometry);
        }

        /// <summary>
        /// Rasterizes a curve stroke into a 16-bit RGBA raster with a transparent background.
        /// </summary>
        /// <typeparam name="TCurve">The curve type.</typeparam>
        /// <param name="curve">The curve to rasterize.</param>
        /// <param name="width">The full stroke width in world coordinate units.</param>
        /// <param name="edgeFalloff">The non-negative alpha falloff outside the stroke edge, in world coordinate units.</param>
        /// <param name="color">The stroke color.</param>
        /// <param name="rasterGeometry">The spatial raster grid that describes the sampled region.</param>
        /// <returns>A 16-bit RGBA raster with the curve drawn over transparency.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="curve"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="width"/> or <paramref name="edgeFalloff"/> is negative, NaN, or infinite,
        /// or when <paramref name="rasterGeometry"/> is invalid.
        /// </exception>
        public static SpatialRaster<RGBA16BitColor> Rasterize<TCurve>(
            this TCurve curve,
            float width,
            float edgeFalloff,
            RGBA16BitColor color,
            RasterGeometry rasterGeometry)
            where TCurve : ICurve
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));

            return curve.Rasterize(
                width,
                edgeFalloff,
                color,
                RGBA16BitColor.Transparent,
                rasterGeometry);
        }
    }
}
