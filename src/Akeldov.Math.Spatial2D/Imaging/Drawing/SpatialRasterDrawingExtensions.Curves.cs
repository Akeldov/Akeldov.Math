using System;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws a curve stroke using its distance queries, clipped to the raster bounds.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve in world coordinates, including infinite lines and rays.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling, round endpoints and source-over compositing.
        /// Each destination pixel is composited once. Distance accuracy follows the curve implementation.
        /// Color channels are composited without gamma correction.</remarks>
        /// <exception cref="ArgumentNullException">The raster or curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, ICurve curve, float width, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (color.A == 0)
                return;
            DrawCurveCoverage(raster, curve, null, width, color);
        }
        /// <summary>Draws a Line stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, Line curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a Ray stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, Ray curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a Arc stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, Arc curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a ParameterizedLine stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, ParameterizedLine curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a ParameterizedSegment stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, ParameterizedSegment curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a ParameterizedArc stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, ParameterizedArc curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a ParameterizedSegmentChain stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, ParameterizedSegmentChain curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a QuadraticBezier stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, QuadraticBezier curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a CubicBezier stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, CubicBezier curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a BSpline stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, BSpline curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
        /// <summary>Draws a Nurbs stroke in world coordinates.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="curve">The curve to draw.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing, clipped to raster bounds.</remarks>
        /// <exception cref="ArgumentNullException">The raster or a reference-type curve is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, Nurbs curve, float width, RGBA8BitColor color)
            => raster.Draw((ICurve)curve, width, color);
    }
}
