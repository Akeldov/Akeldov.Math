using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws an arrow with an open, two-sided head and constant stroke width.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="start">The shaft start in world coordinates.</param>
        /// <param name="end">The head tip in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="headLength">The finite, positive axial head length in world units.</param>
        /// <param name="headWidth">The finite, positive full transverse head span in world units, excluding stroke thickness.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses round caps, 4-by-4 coverage sampling and source-over compositing once per pixel.
        /// Drawing is clipped to the raster bounds. Coincident endpoints draw nothing.
        /// Head dimensions are not reduced for short arrows and may extend behind the start.</remarks>
        /// <exception cref="ArgumentNullException">The raster is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">An endpoint or dimension is invalid, or derived coordinates exceed the finite float range.</exception>
        public static void DrawArrow(this SpatialRaster<RGBA8BitColor> raster, PointXY start, PointXY end,
            float width, float headLength, float headWidth, RGBA8BitColor color)
            => raster.DrawArrow(start, end, width, headLength, headWidth, 0f, color);

        /// <summary>Draws an arrow with a solid open head and an optionally dashed shaft.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="start">The shaft start in world coordinates, where the first dash begins.</param>
        /// <param name="end">The head tip in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="headLength">The finite, positive axial head length in world units.</param>
        /// <param name="headWidth">The finite, positive full transverse head span in world units, excluding stroke thickness.</param>
        /// <param name="dash">The finite, non-negative length of each dash and gap in world units, before round caps. Zero selects a solid shaft.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses round caps, 4-by-4 coverage sampling and source-over compositing once per pixel
        /// across all dashes and both head sides. The final dash is shortened at the tip; the head is always solid,
        /// including when the shaft ends in a gap. Caps extend into gaps.
        /// Drawing is clipped to raster bounds. Coincident endpoints draw nothing.
        /// Head dimensions are not reduced for short arrows and may extend behind the start.</remarks>
        /// <exception cref="ArgumentNullException">The raster is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">An endpoint or dimension is invalid, derived coordinates or dashed length
        /// exceed the finite float range, or dash spacing is too small for float progression.</exception>
        public static void DrawArrow(this SpatialRaster<RGBA8BitColor> raster, PointXY start, PointXY end,
            float width, float headLength, float headWidth, float dash, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (float.IsNaN(start.X) || float.IsInfinity(start.X) || float.IsNaN(start.Y) || float.IsInfinity(start.Y))
                throw new ArgumentOutOfRangeException(nameof(start));
            if (float.IsNaN(end.X) || float.IsInfinity(end.X) || float.IsNaN(end.Y) || float.IsInfinity(end.Y))
                throw new ArgumentOutOfRangeException(nameof(end));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (float.IsNaN(headLength) || float.IsInfinity(headLength) || headLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(headLength));
            if (float.IsNaN(headWidth) || float.IsInfinity(headWidth) || headWidth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(headWidth));
            if (float.IsNaN(dash) || float.IsInfinity(dash) || dash < 0f)
                throw new ArgumentOutOfRangeException(nameof(dash));

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float scale = MathF.Max(MathF.Abs(dx), MathF.Abs(dy));
            if (float.IsInfinity(scale))
                throw new ArgumentOutOfRangeException(nameof(end));
            if (scale == 0f)
                return;
            float ux = dx / scale;
            float uy = dy / scale;
            float norm = MathF.Sqrt(ux * ux + uy * uy);
            ux /= norm;
            uy /= norm;
            var head = GetArrowHeadEndpoints(end, new VectorXY(ux, uy), headLength, headWidth);
            float length = scale * norm;
            if (dash > 0f && float.IsInfinity(length))
                throw new ArgumentOutOfRangeException(nameof(end));
            if (dash > 0f && length + dash * 2f == length)
                throw new ArgumentOutOfRangeException(nameof(dash), "Dash spacing is too small for the arrow length.");
            if (color.A == 0)
                return;

            var shaft = new Segment(start, end);
            var strokes = new List<Segment> { new Segment(end, head.Left), new Segment(end, head.Right) };
            if (dash == 0f)
                strokes.Add(shaft);
            else
                AddArrowDashes(strokes, start, end, new VectorXY(ux, uy), length, dash);
            DrawCurveCoverage(raster, shaft, strokes, width, color);
        }

        /// <summary>Draws an open-headed arrow from StartPoint towards EndPoint.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="segment">The directed shaft in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="headLength">The finite, positive axial head length in world units.</param>
        /// <param name="headWidth">The finite, positive full transverse head span in world units, excluding stroke thickness.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses the endpoint overload's clipping and single-pass compositing. Coincident endpoints draw nothing.
        /// Head dimensions are not reduced for short arrows. Endpoint inclusion flags do not affect drawing.</remarks>
        /// <exception cref="ArgumentNullException">The raster is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A dimension is invalid or derived coordinates exceed the finite float range.</exception>
        public static void DrawArrow(this SpatialRaster<RGBA8BitColor> raster, ParameterizedSegment segment,
            float width, float headLength, float headWidth, RGBA8BitColor color)
            => raster.DrawArrow(segment.StartPoint, segment.EndPoint, width, headLength, headWidth, color);

        /// <summary>Draws an arrow from StartPoint towards EndPoint with a solid open head and an optionally dashed shaft.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="segment">The directed shaft in world coordinates. The first dash begins at StartPoint.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="headLength">The finite, positive axial head length in world units.</param>
        /// <param name="headWidth">The finite, positive full transverse head span in world units, excluding stroke thickness.</param>
        /// <param name="dash">The finite, non-negative length of each dash and gap in world units, before round caps. Zero selects a solid shaft.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <remarks>Uses the endpoint overload's clipping, sampling and single-pass compositing.
        /// The head stays solid, even if the tip lies in a gap. Coincident endpoints draw nothing.
        /// Head dimensions are not reduced for short arrows. Endpoint inclusion flags do not affect drawing.</remarks>
        /// <exception cref="ArgumentNullException">The raster is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A dimension is invalid, derived coordinates or dashed length
        /// exceed the finite float range, or dash spacing is too small for float progression.</exception>
        public static void DrawArrow(this SpatialRaster<RGBA8BitColor> raster, ParameterizedSegment segment,
            float width, float headLength, float headWidth, float dash, RGBA8BitColor color)
            => raster.DrawArrow(segment.StartPoint, segment.EndPoint, width, headLength, headWidth, dash, color);

        private static (PointXY Left, PointXY Right) GetArrowHeadEndpoints(PointXY end, VectorXY direction,
            float headLength, float headWidth)
        {
            float baseX = end.X - direction.X * headLength;
            float baseY = end.Y - direction.Y * headLength;
            float halfSpan = headWidth * 0.5f;
            var left = new PointXY(baseX - direction.Y * halfSpan, baseY + direction.X * halfSpan);
            var right = new PointXY(baseX + direction.Y * halfSpan, baseY - direction.X * halfSpan);
            if (float.IsInfinity(left.X) || float.IsInfinity(left.Y) ||
                float.IsInfinity(right.X) || float.IsInfinity(right.Y))
                throw new ArgumentOutOfRangeException(nameof(headLength), "Arrow head coordinates exceed the finite float range.");
            return (left, right);
        }

        private static void AddArrowDashes(List<Segment> strokes, PointXY start, PointXY end,
            VectorXY direction, float length, float dash)
        {
            float step = dash * 2f;
            for (float distance = 0f; distance < length; distance += step)
            {
                float dashEnd = MathF.Min(length, distance + dash);
                var a = distance == 0f ? start : start + direction * distance;
                var b = dashEnd == length ? end : start + direction * dashEnd;
                strokes.Add(new Segment(a, b));
                if (dashEnd == length)
                    break;
            }
        }
    }
}
