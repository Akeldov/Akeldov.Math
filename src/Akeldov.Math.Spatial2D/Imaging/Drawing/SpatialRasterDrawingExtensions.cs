using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>Provides drawing operations on spatial color rasters.</summary>
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws a solid closed polygonal contour.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="points">World-space boundary vertices, read during the call and not retained.</param>
        /// <param name="width">The positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        public static void DrawContour(this SpatialRaster<RGBA8BitColor> raster, IReadOnlyList<PointXY> points, float width, RGBA8BitColor color)
            => raster.DrawContour(points, width, color, 0f);

        /// <summary>Draws a solid contour.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="contour">The contour in world coordinates.</param>
        /// <param name="width">The positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, IContour contour, float width, RGBA8BitColor color)
            => raster.DrawDashed(contour, width, color, 0f);
        /// <summary>Draws a closed polygonal contour into the existing raster.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="points">Boundary vertices in world coordinates. At least three vertices are required; the first may be repeated at the end. The list is read during this call and is not retained.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <param name="dash">The finite, non-negative dash and gap length in world units; zero selects a solid stroke.</param>
        /// <remarks>The last vertex connects to the first. Dash phase continues across vertices from the first vertex.</remarks>
        /// <exception cref="ArgumentNullException">The raster or points are null.</exception>
        /// <exception cref="ArgumentException">The vertices do not form a valid contour.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Coordinates or drawing parameters are invalid.</exception>
        public static void DrawContour(this SpatialRaster<RGBA8BitColor> raster, IReadOnlyList<PointXY> points, float width, RGBA8BitColor color, float dash)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            raster.DrawDashed(new CompositeContour(points), width, color, dash);
        }

        /// <summary>Draws a contour with antialiasing and source-over compositing.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="contour">The contour in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color and opacity.</param>
        /// <param name="dash">The finite, non-negative dash and gap length in world units; zero selects a solid stroke.</param>
        /// <remarks>
        /// Uses 4-by-4 coverage sampling and composites each pixel once, including overlapping strokes.
        /// Solid strokes use the contour distance directly. Dashed strokes have round caps and continuous
        /// phase from contour coordinate zero. Dashed curves are approximated by segments no longer in
        /// curve coordinate than half the smaller cell dimension. The final dash is shortened at the seam.
        /// Built-in circles, rectangles and composite contours support dashes; custom contours must implement
        /// IParameterizedContour for dashes. Color channels are composited without gamma correction.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The raster or contour is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Drawing parameters or contour length are invalid, or sampling cannot advance in float precision.</exception>
        /// <exception cref="ArgumentException">A custom dashed contour has no length parameterization.</exception>
        public static void DrawDashed(this SpatialRaster<RGBA8BitColor> raster, IContour contour, float width, RGBA8BitColor color, float dash)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (contour == null)
                throw new ArgumentNullException(nameof(contour));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (float.IsNaN(dash) || float.IsInfinity(dash) || dash < 0f)
                throw new ArgumentOutOfRangeException(nameof(dash));

            List<Segment>? strokes = null;
            if (dash > 0f)
            {
                IParameterizedContour parameterized = contour switch
                {
                    IParameterizedContour value => value,
                    ICompositeContour value => new ParameterizedCompositeContour(value.Curves),
                    Circle value => new ParameterizedCircle(value.Center, value.Radius),
                    RectangleContour value => (ParameterizedRectangleContour)value,
                    OrientedRectangleContour value => (ParameterizedOrientedRectangleContour)value,
                    _ => throw new ArgumentException("Dashed contours require length parameterization.", nameof(contour)),
                };
                float length = parameterized.Length;
                if (float.IsNaN(length) || float.IsInfinity(length) || length < 0f)
                    throw new ArgumentOutOfRangeException(nameof(contour));
                float sampleStep = MathF.Min(raster.Geometry.CellSize.X, raster.Geometry.CellSize.Y) * 0.5f;
                sampleStep = MathF.Min(sampleStep, dash);
                if (sampleStep <= 0f || length + sampleStep == length)
                    throw new ArgumentOutOfRangeException(nameof(dash), "Sampling cannot advance at this contour length.");
                if (color.A == 0)
                    return;
                strokes = new List<Segment>();
                for (float offset = 0f; offset < length; offset += dash * 2f)
                {
                    float stop = MathF.Min(length, offset + dash);
                    for (float position = offset; position < stop;)
                    {
                        float next = MathF.Min(stop, position + sampleStep);
                        strokes.Add(new Segment(parameterized.GetPoint(position), parameterized.GetPoint(next)));
                        position = next;
                    }
                    if (stop == length)
                        break;
                }
            }
            if (color.A == 0)
                return;

            DrawContourCoverage(raster, contour, strokes, width, color);
        }

        private static void DrawContourCoverage(SpatialRaster<RGBA8BitColor> raster, IContour contour,
            List<Segment>? strokes, float width, RGBA8BitColor color)
        {
            var geometry = raster.Geometry;
            float radius = width * 0.5f;
            for (int y = 0; y < raster.Resolution.Y; y++)
            {
                for (int x = 0; x < raster.Resolution.X; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            var point = new PointXY(
                                geometry.Origin.X + (x + (sx + 0.5f) / 4) * geometry.CellSize.X,
                                geometry.Origin.Y + (y + (sy + 0.5f) / 4) * geometry.CellSize.Y);
                            bool inside = strokes == null ? contour.Distance(point) <= radius : false;
                            if (strokes != null)
                            {
                                foreach (var stroke in strokes)
                                {
                                    if (stroke.Distance(point) <= radius)
                                    {
                                        inside = true;
                                        break;
                                    }
                                }
                            }
                            if (inside)
                                covered++;
                        }
                    }
                    if (covered > 0)
                        raster[x, y] = Composite(raster[x, y], color, covered / 16f);
                }
            }
        }

        /// <summary>Fills an axis-aligned rectangle using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="rectangle">The rectangle in world coordinates.</param>
        /// <param name="color">The fill color, including its opacity.</param>
        /// <remarks>Coverage is the fraction of each cell's area inside the rectangle. Zero-area rectangles draw nothing.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        public static void Fill(this SpatialRaster<RGBA8BitColor> raster, Rectangle rectangle, RGBA8BitColor color)
        {
            raster.FillRectangle(rectangle.Min, rectangle.Max, color);
        }

        /// <summary>Fills an axis-aligned rectangle using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="cornerA">The first finite corner in world coordinates.</param>
        /// <param name="cornerB">The opposite finite corner in world coordinates. Corner order does not matter.</param>
        /// <param name="color">The fill color, including its opacity.</param>
        /// <remarks>
        /// Uses the raster geometry with Y increasing upwards and clips to its bounds.
        /// Antialiasing uses the fraction of each cell's area inside the rectangle, including rectangular cells.
        /// Zero-area rectangles draw nothing. Color channels are composited without gamma correction.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A corner coordinate is non-finite.</exception>
        public static void FillRectangle(this SpatialRaster<RGBA8BitColor> raster, PointXY cornerA, PointXY cornerB, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (float.IsNaN(cornerA.X) || float.IsInfinity(cornerA.X) || float.IsNaN(cornerA.Y) || float.IsInfinity(cornerA.Y))
                throw new ArgumentOutOfRangeException(nameof(cornerA));
            if (float.IsNaN(cornerB.X) || float.IsInfinity(cornerB.X) || float.IsNaN(cornerB.Y) || float.IsInfinity(cornerB.Y))
                throw new ArgumentOutOfRangeException(nameof(cornerB));
            if (color.A == 0 || cornerA.X == cornerB.X || cornerA.Y == cornerB.Y)
                return;

            var geometry = raster.Geometry;
            float cellWidth = geometry.Size.X / raster.Resolution.X;
            float cellHeight = geometry.Size.Y / raster.Resolution.Y;
            float left = MathF.Max(0, MathF.Min(raster.Resolution.X,
                (MathF.Min(cornerA.X, cornerB.X) - geometry.Origin.X) / cellWidth));
            float right = MathF.Max(0, MathF.Min(raster.Resolution.X,
                (MathF.Max(cornerA.X, cornerB.X) - geometry.Origin.X) / cellWidth));
            float bottom = MathF.Max(0, MathF.Min(raster.Resolution.Y,
                (MathF.Min(cornerA.Y, cornerB.Y) - geometry.Origin.Y) / cellHeight));
            float top = MathF.Max(0, MathF.Min(raster.Resolution.Y,
                (MathF.Max(cornerA.Y, cornerB.Y) - geometry.Origin.Y) / cellHeight));
            if (left >= right || bottom >= top)
                return;

            int xEnd = (int)MathF.Ceiling(right);
            int yEnd = (int)MathF.Ceiling(top);
            for (int y = (int)MathF.Floor(bottom); y < yEnd; y++)
            {
                float coverageY = MathF.Min(y + 1f, top) - MathF.Max(y, bottom);
                for (int x = (int)MathF.Floor(left); x < xEnd; x++)
                {
                    float coverageX = MathF.Min(x + 1f, right) - MathF.Max(x, left);
                    raster[x, y] = Composite(raster[x, y], color, coverageX * coverageY);
                }
            }
        }

        /// <summary>Fills a circle in the existing raster using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="circle">The circle with center and radius in world units.</param>
        /// <param name="color">The fill color, including its opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and clips to the raster bounds. A zero radius draws nothing.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        public static void Fill(this SpatialRaster<RGBA8BitColor> raster, Circle circle, RGBA8BitColor color)
        {
            raster.FillCircle(circle.Center, circle.Radius, color);
        }

        /// <summary>Fills a circle in the existing raster using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="center">The finite center point in world coordinates.</param>
        /// <param name="radius">The finite, non-negative radius in world units.</param>
        /// <param name="color">The fill color, including its opacity.</param>
        /// <remarks>
        /// Uses the raster geometry with Y increasing upwards and clips to its bounds.
        /// Coverage is approximated with a fixed 4-by-4 sample grid per cell, including rectangular cells.
        /// A zero radius draws nothing. Color channels are composited without gamma correction.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The center is non-finite or the radius is negative or non-finite.</exception>
        public static void FillCircle(this SpatialRaster<RGBA8BitColor> raster, PointXY center, float radius, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (float.IsNaN(center.X) || float.IsInfinity(center.X) || float.IsNaN(center.Y) || float.IsInfinity(center.Y))
                throw new ArgumentOutOfRangeException(nameof(center));
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius < 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (radius == 0f || color.A == 0)
                return;

            var geometry = raster.Geometry;
            float cellWidth = geometry.Size.X / raster.Resolution.X;
            float cellHeight = geometry.Size.Y / raster.Resolution.Y;
            int left = (int)MathF.Max(0, MathF.Min(raster.Resolution.X,
                MathF.Floor((center.X - radius - geometry.Origin.X) / cellWidth)));
            int right = (int)MathF.Max(0, MathF.Min(raster.Resolution.X,
                MathF.Ceiling((center.X + radius - geometry.Origin.X) / cellWidth)));
            int bottom = (int)MathF.Max(0, MathF.Min(raster.Resolution.Y,
                MathF.Floor((center.Y - radius - geometry.Origin.Y) / cellHeight)));
            int top = (int)MathF.Max(0, MathF.Min(raster.Resolution.Y,
                MathF.Ceiling((center.Y + radius - geometry.Origin.Y) / cellHeight)));

            for (int y = bottom; y < top; y++)
            {
                for (int x = left; x < right; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            // Normalize before squaring to avoid overflowing the radius squared.
                            float dx = ((geometry.Origin.X - center.X) + (x + (sx + 0.5f) / 4) * cellWidth) / radius;
                            float dy = ((geometry.Origin.Y - center.Y) + (y + (sy + 0.5f) / 4) * cellHeight) / radius;
                            if (dx * dx + dy * dy <= 1f)
                                covered++;
                        }
                    }

                    if (covered != 0)
                        raster[x, y] = Composite(raster[x, y], color, covered / 16f);
                }
            }
        }

        /// <summary>Draws a round-capped segment into the existing raster using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="segment">The segment with endpoints in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color, including its opacity.</param>
        /// <remarks>Uses the same clipping and 4-by-4 coverage sampling as the endpoint overload. A zero-length segment draws a disk.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">An endpoint is non-finite or the width is not finite and positive.</exception>
        public static void Draw(this SpatialRaster<RGBA8BitColor> raster, Segment segment, float width, RGBA8BitColor color)
        {
            raster.DrawSegment(segment.EndpointA, segment.EndpointB, width, color);
        }

        /// <summary>Draws a dashed segment into the existing raster with round-capped strokes.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="segment">The segment in world coordinates. The first dash begins at EndpointA and proceeds towards EndpointB.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="dash">The finite, positive length of each dash and gap in world units, before adding round caps.</param>
        /// <param name="color">The stroke color, including its opacity.</param>
        /// <remarks>Uses the endpoint overload's clipping, antialiasing and compositing. The last dash is shortened at EndpointB. A zero-length segment draws nothing.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">An endpoint, width or dash is invalid, the length exceeds the finite float range, or dash spacing is too small for float progression.</exception>
        public static void DrawDashed(this SpatialRaster<RGBA8BitColor> raster, Segment segment, float width, float dash, RGBA8BitColor color)
        {
            raster.DrawDashedSegment(segment.EndpointA, segment.EndpointB, width, dash, color);
        }

        /// <summary>Draws a round-capped line into the existing raster using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="start">The start point in world coordinates.</param>
        /// <param name="end">The end point in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color, including its opacity.</param>
        /// <remarks>
        /// Uses the raster geometry, with Y increasing upwards, and clips drawing to its bounds.
        /// Coverage is approximated with a fixed 4-by-4 sample grid per cell, including rectangular cells.
        /// A zero-length line draws a disk of diameter <paramref name="width"/>.
        /// Color channels are composited directly without gamma correction.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A point is non-finite or the width is not finite and positive.</exception>
        public static void DrawSegment(this SpatialRaster<RGBA8BitColor> raster, PointXY start, PointXY end, float width, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (float.IsNaN(start.X) || float.IsInfinity(start.X) || float.IsNaN(start.Y) || float.IsInfinity(start.Y))
                throw new ArgumentOutOfRangeException(nameof(start));
            if (float.IsNaN(end.X) || float.IsInfinity(end.X) || float.IsNaN(end.Y) || float.IsInfinity(end.Y))
                throw new ArgumentOutOfRangeException(nameof(end));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (color.A == 0)
                return;

            var geometry = raster.Geometry;
            float cellWidth = geometry.Size.X / raster.Resolution.X;
            float cellHeight = geometry.Size.Y / raster.Resolution.Y;
            float radius = width / 2f;
            // Clamp in floating point before converting: off-screen coordinates can exceed Int32.
            int left = (int)MathF.Max(0, MathF.Min(raster.Resolution.X,
                MathF.Floor((MathF.Min(start.X, end.X) - radius - geometry.Origin.X) / cellWidth)));
            int right = (int)MathF.Max(0, MathF.Min(raster.Resolution.X,
                MathF.Ceiling((MathF.Max(start.X, end.X) + radius - geometry.Origin.X) / cellWidth)));
            int bottom = (int)MathF.Max(0, MathF.Min(raster.Resolution.Y,
                MathF.Floor((MathF.Min(start.Y, end.Y) - radius - geometry.Origin.Y) / cellHeight)));
            int top = (int)MathF.Max(0, MathF.Min(raster.Resolution.Y,
                MathF.Ceiling((MathF.Max(start.Y, end.Y) + radius - geometry.Origin.Y) / cellHeight)));

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float lengthSquared = dx * dx + dy * dy;
            for (int y = bottom; y < top; y++)
            {
                for (int x = left; x < right; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            float px = (geometry.Origin.X - start.X) + (x + (sx + 0.5f) / 4) * cellWidth;
                            float py = (geometry.Origin.Y - start.Y) + (y + (sy + 0.5f) / 4) * cellHeight;
                            float t = lengthSquared == 0 ? 0 : MathF.Max(0, MathF.Min(1, (px * dx + py * dy) / lengthSquared));
                            float ex = px - t * dx;
                            float ey = py - t * dy;
                            if (ex * ex + ey * ey <= radius * radius)
                                covered++;
                        }
                    }

                    if (covered != 0)
                        raster[x, y] = Composite(raster[x, y], color, covered / 16f);
                }
            }
        }

        /// <summary>Draws a dashed line into the existing raster with round-capped strokes.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="start">The start point in world coordinates, where the first dash begins.</param>
        /// <param name="end">The end point in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="dash">The finite, positive length of each dash and each gap in world units, measured along the line before adding round caps.</param>
        /// <param name="color">The stroke color, including its opacity.</param>
        /// <remarks>
        /// Each dash uses <see cref="DrawSegment(SpatialRaster{RGBA8BitColor}, PointXY, PointXY, float, RGBA8BitColor)"/> for clipping, antialiasing and source-over compositing.
        /// The last dash is shortened at the end point. A zero-length line draws nothing.
        /// Round caps extend into gaps; overlapping dashes are composited separately.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A point, width or dash is invalid, the line length exceeds the finite float range, or dash spacing is too small for float progression along the line.</exception>
        public static void DrawDashedSegment(this SpatialRaster<RGBA8BitColor> raster, PointXY start, PointXY end, float width, float dash, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (float.IsNaN(start.X) || float.IsInfinity(start.X) || float.IsNaN(start.Y) || float.IsInfinity(start.Y))
                throw new ArgumentOutOfRangeException(nameof(start));
            if (float.IsNaN(end.X) || float.IsInfinity(end.X) || float.IsNaN(end.Y) || float.IsInfinity(end.Y))
                throw new ArgumentOutOfRangeException(nameof(end));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (float.IsNaN(dash) || float.IsInfinity(dash) || dash <= 0f)
                throw new ArgumentOutOfRangeException(nameof(dash));

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float scale = MathF.Max(MathF.Abs(dx), MathF.Abs(dy));
            if (float.IsInfinity(scale))
                throw new ArgumentOutOfRangeException(nameof(end));
            if (scale == 0f)
                return;
            float nx = dx / scale;
            float ny = dy / scale;
            float length = scale * MathF.Sqrt(nx * nx + ny * ny);
            if (float.IsInfinity(length))
                throw new ArgumentOutOfRangeException(nameof(end));
            float step = dash * 2f;
            if (length + step == length)
                throw new ArgumentOutOfRangeException(nameof(dash), "Dash spacing is too small for the line length.");
            if (color.A == 0)
                return;

            float ux = dx / length;
            float uy = dy / length;
            for (float distance = 0f; distance < length; distance += step)
            {
                float dashEnd = MathF.Min(length, distance + dash);
                var a = distance == 0f ? start : new PointXY(start.X + ux * distance, start.Y + uy * distance);
                var b = dashEnd == length ? end : new PointXY(start.X + ux * dashEnd, start.Y + uy * dashEnd);
                raster.DrawSegment(a, b, width, color);
                if (dashEnd == length)
                    break;
            }
        }

        private static RGBA8BitColor Composite(RGBA8BitColor target, RGBA8BitColor source, float coverage)
        {
            float sourceAlpha = source.A / 255f * coverage;
            float targetWeight = target.A / 255f * (1 - sourceAlpha);
            float alpha = sourceAlpha + targetWeight;
            return new RGBA8BitColor(
                (byte)MathF.Round((source.R * sourceAlpha + target.R * targetWeight) / alpha),
                (byte)MathF.Round((source.G * sourceAlpha + target.G * targetWeight) / alpha),
                (byte)MathF.Round((source.B * sourceAlpha + target.B * targetWeight) / alpha),
                (byte)MathF.Round(alpha * 255));
        }
    }
}
