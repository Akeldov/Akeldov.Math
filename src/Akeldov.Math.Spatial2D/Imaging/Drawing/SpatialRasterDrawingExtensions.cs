using System;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>Provides drawing operations on spatial color rasters.</summary>
    public static class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws a round-capped segment into the existing raster using source-over alpha compositing.</summary>
        /// <param name="raster">The raster whose retained values are modified in place.</param>
        /// <param name="segment">The segment with endpoints in world coordinates.</param>
        /// <param name="width">The finite, positive full stroke width in world units.</param>
        /// <param name="color">The stroke color, including its opacity.</param>
        /// <remarks>Uses the same clipping and 4-by-4 coverage sampling as the endpoint overload. A zero-length segment draws a disk.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="raster"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">An endpoint is non-finite or the width is not finite and positive.</exception>
        public static void DrawSegment(this SpatialRaster<RGBA8BitColor> raster, Segment segment, float width, RGBA8BitColor color)
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
        public static void DrawDashedSegment(this SpatialRaster<RGBA8BitColor> raster, Segment segment, float width, float dash, RGBA8BitColor color)
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
