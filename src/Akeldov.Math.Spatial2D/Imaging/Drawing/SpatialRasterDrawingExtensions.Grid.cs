using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws a square coordinate grid aligned to world coordinate zero.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="step">The finite, positive spacing on both axes in world units.</param>
        /// <param name="width">The finite, positive full line width in world units.</param>
        /// <param name="color">The ordinary line color and opacity.</param>
        /// <remarks>Uses 4-by-4 coverage sampling and source-over compositing once per pixel, including crossings.
        /// Drawing is clipped to the raster. RGB compositing does not apply gamma correction.</remarks>
        /// <exception cref="ArgumentNullException">The raster is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The spacing, width, or a derived coordinate is invalid
        /// or the line indices cannot progress in float precision.</exception>
        public static void DrawGrid(this SpatialRaster<RGBA8BitColor> raster, float step, float width, RGBA8BitColor color)
            => raster.DrawGrid(new VectorXY(step, step), width, color);

        /// <summary>Draws a rectangular coordinate grid aligned to world coordinate zero.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="step">The finite, positive spacing on each axis in world units.</param>
        /// <param name="width">The finite, positive full line width in world units.</param>
        /// <param name="color">The ordinary line color and opacity.</param>
        /// <param name="majorEvery">Every Nth line is major, counting from coordinate zero in both directions. Zero disables major lines.</param>
        /// <param name="majorColor">The major line color; null uses the ordinary line color. Major lines have the same width.</param>
        /// <param name="labelHeight">The finite, non-negative pixel-font height in world units. Zero disables labels.</param>
        /// <param name="labelColor">The label color; null uses the major line color, or the ordinary color when major lines are disabled.</param>
        /// <param name="bounds">The optional world-space drawing rectangle; null uses the raster bounds. Lines and labels are clipped to both.</param>
        /// <remarks>Uses 4-by-4 coverage sampling. Intersections within the grid are composited once;
        /// major lines take precedence over ordinary lines at shared samples.
        /// Labels use the built-in pixel font and invariant coordinates with six significant digits.
        /// Major lines are labeled when enabled; otherwise all lines are candidates.
        /// Labels appear along the bottom and left visible edges; labels that overlap earlier labels or do not fit are skipped.
        /// RGB compositing does not apply gamma correction.</remarks>
        /// <exception cref="ArgumentNullException">The raster is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A spacing, width, interval, label height, or derived coordinate is invalid
        /// or the line indices cannot progress in float precision.</exception>
        public static void DrawGrid(this SpatialRaster<RGBA8BitColor> raster, VectorXY step, float width, RGBA8BitColor color,
            int majorEvery = 0, RGBA8BitColor? majorColor = null, float labelHeight = 0f, RGBA8BitColor? labelColor = null, Rectangle? bounds = null)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (!step.IsFinite || step.X <= 0f || step.Y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(step));
            if (float.IsNaN(width) || float.IsInfinity(width) || width <= 0f)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (majorEvery < 0)
                throw new ArgumentOutOfRangeException(nameof(majorEvery));
            if (float.IsNaN(labelHeight) || float.IsInfinity(labelHeight) || labelHeight < 0f)
                throw new ArgumentOutOfRangeException(nameof(labelHeight));

            var geometry = raster.Geometry;
            var rasterMax = geometry.Origin + geometry.Size;
            if (float.IsInfinity(rasterMax.X) || float.IsInfinity(rasterMax.Y))
                throw new ArgumentOutOfRangeException(nameof(raster));
            var area = bounds ?? new Rectangle(geometry.Origin, rasterMax);
            float minX = MathF.Max(area.Min.X, geometry.Origin.X);
            float minY = MathF.Max(area.Min.Y, geometry.Origin.Y);
            float maxX = MathF.Min(area.Max.X, rasterMax.X);
            float maxY = MathF.Min(area.Max.Y, rasterMax.Y);
            if (minX >= maxX || minY >= maxY)
                return;

            var visible = new Rectangle(new PointXY(minX, minY), new PointXY(maxX, maxY));
            var xCoverage = GetGridAxisCoverage(geometry.Origin.X, geometry.CellSize.X, raster.Resolution.X,
                area.Min.X, area.Max.X, minX, maxX, step.X, width, majorEvery);
            var yCoverage = GetGridAxisCoverage(geometry.Origin.Y, geometry.CellSize.Y, raster.Resolution.Y,
                area.Min.Y, area.Max.Y, minY, maxY, step.Y, width, majorEvery);
            var major = majorColor ?? color;
            CompositeGrid(raster, xCoverage, yCoverage, color, major);
            if (labelHeight > 0f)
                DrawGridLabels(raster, visible, step * (majorEvery > 0 ? majorEvery : 1),
                    labelHeight, labelColor ?? (majorEvery > 0 ? major : color));
        }

        private static GridAxisCoverage[] GetGridAxisCoverage(float origin, float cellSize, int resolution,
            float boundsMin, float boundsMax, float visibleMin, float visibleMax, float step, float width, int majorEvery)
        {
            // Include line centers just outside the viewport when their stroke reaches a visible sample.
            float first = MathF.Ceiling(MathF.Max(boundsMin, visibleMin - width * 0.5f) / step);
            float last = MathF.Floor(MathF.Min(boundsMax, visibleMax + width * 0.5f) / step);
            if (float.IsInfinity(first) || float.IsInfinity(last) ||
                MathF.Abs(first) >= 16777216f || MathF.Abs(last) >= 16777216f)
                throw new ArgumentOutOfRangeException(nameof(step), "Grid indices cannot progress in float precision.");

            float firstMajor = majorEvery > 0 ? MathF.Ceiling(first / majorEvery) : 1f;
            float lastMajor = majorEvery > 0 ? MathF.Floor(last / majorEvery) : 0f;
            var result = new GridAxisCoverage[resolution];
            for (int pixel = 0; pixel < resolution; pixel++)
            {
                byte inside = 0, lines = 0, major = 0;
                for (int sample = 0; sample < 4; sample++)
                {
                    float coordinate = origin + (pixel + (sample + 0.5f) / 4f) * cellSize;
                    if (coordinate < visibleMin || coordinate > visibleMax)
                        continue;
                    byte bit = (byte)(1 << sample);
                    inside |= bit;
                    float index = MathF.Max(first, MathF.Min(last, MathF.Round(coordinate / step)));
                    if (first <= last && MathF.Abs(coordinate - index * step) <= width * 0.5f)
                        lines |= bit;
                    if (firstMajor > lastMajor)
                        continue;
                    float majorIndex = MathF.Max(firstMajor, MathF.Min(lastMajor,
                        MathF.Round(coordinate / step / majorEvery)));
                    if (MathF.Abs(coordinate - (majorIndex * majorEvery) * step) <= width * 0.5f)
                        major |= bit;
                }
                result[pixel] = new GridAxisCoverage(inside, lines, major);
            }
            return result;
        }

        private static void CompositeGrid(SpatialRaster<RGBA8BitColor> raster, GridAxisCoverage[] xs,
            GridAxisCoverage[] ys, RGBA8BitColor color, RGBA8BitColor major)
        {
            for (int y = 0; y < ys.Length; y++)
                for (int x = 0; x < xs.Length; x++)
                {
                    // Cartesian union: vertical coverage + horizontal coverage - shared samples.
                    int allCount = xs[x].Lines * ys[y].Inside + ys[y].Lines * xs[x].Inside - xs[x].Lines * ys[y].Lines;
                    int majorCount = xs[x].Major * ys[y].Inside + ys[y].Major * xs[x].Inside - xs[x].Major * ys[y].Major;
                    float minorAlpha = (allCount - majorCount) * color.A / (16f * 255f);
                    float majorAlpha = majorCount * major.A / (16f * 255f);
                    float sourceAlpha = minorAlpha + majorAlpha;
                    if (sourceAlpha <= 0f)
                        continue;
                    var target = raster[x, y];
                    float targetWeight = target.A / 255f * (1f - sourceAlpha);
                    float alpha = sourceAlpha + targetWeight;
                    raster[x, y] = new RGBA8BitColor(
                        (byte)MathF.Round((color.R * minorAlpha + major.R * majorAlpha + target.R * targetWeight) / alpha),
                        (byte)MathF.Round((color.G * minorAlpha + major.G * majorAlpha + target.G * targetWeight) / alpha),
                        (byte)MathF.Round((color.B * minorAlpha + major.B * majorAlpha + target.B * targetWeight) / alpha),
                        (byte)MathF.Round(alpha * 255f));
                }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct GridAxisCoverage
        {
            internal GridAxisCoverage(byte inside, byte lines, byte major)
            {
                Inside = CountGridSamples(inside);
                Lines = CountGridSamples((byte)(lines | major));
                Major = CountGridSamples(major);
            }

            internal int Inside { get; }
            internal int Lines { get; }
            internal int Major { get; }

            private static int CountGridSamples(byte mask) =>
                (mask & 1) + ((mask >> 1) & 1) + ((mask >> 2) & 1) + ((mask >> 3) & 1);
        }

        private static void DrawGridLabels(SpatialRaster<RGBA8BitColor> raster, Rectangle bounds,
            VectorXY step, float height, RGBA8BitColor color)
        {
            if (color.A == 0)
                return;
            float padding = height * 0.3f;
            DrawGridAxisLabels(raster, bounds, step.X, height, padding, color, horizontal: true);
            DrawGridAxisLabels(raster, bounds, step.Y, height, padding, color, horizontal: false);
        }

        private static void DrawGridAxisLabels(SpatialRaster<RGBA8BitColor> raster, Rectangle bounds,
            float step, float height, float padding, RGBA8BitColor color, bool horizontal)
        {
            if (float.IsInfinity(step))
                return;
            float min = horizontal ? bounds.Min.X : bounds.Min.Y;
            float max = horizontal ? bounds.Max.X : bounds.Max.Y;
            float last = MathF.Floor(max / step);
            float firstPosition = horizontal ? min : min + height + padding;
            for (float index = MathF.Ceiling(firstPosition / step); index <= last;)
            {
                float coordinate = index * step;
                string text = (coordinate == 0f ? 0f : coordinate).ToString("G6", CultureInfo.InvariantCulture);
                var size = GlyphSet.Default.MeasureText(text, height).Size;
                // Advance by the label extent even when it cannot fit, so dense grids stay bounded by image size.
                float extent = horizontal ? size.X : size.Y;
                index = MathF.Max(index + 1f, MathF.Ceiling((coordinate + extent + padding) / step));
                var origin = horizontal
                    ? new PointXY(coordinate + padding, bounds.Min.Y + padding)
                    : new PointXY(bounds.Min.X + padding, coordinate + padding);
                if (origin.X + size.X > bounds.Max.X || origin.Y + size.Y > bounds.Max.Y)
                    continue;
                raster.DrawText(text, origin, height, color);
            }
        }
    }
}
