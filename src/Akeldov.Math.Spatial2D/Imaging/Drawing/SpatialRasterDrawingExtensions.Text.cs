using System;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Imaging
{
    public static partial class SpatialRasterDrawingExtensions
    {
        /// <summary>Draws black text using the shared, prebuilt 5-by-7 pixel glyph set.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="text">Case-sensitive Latin and Russian text; unsupported characters use '?'. Line breaks are supported.</param>
        /// <param name="origin">The first baseline in world coordinates, with Y increasing upwards.</param>
        /// <param name="height">The finite positive seven-dot glyph height in world units.</param>
        public static void DrawText(this SpatialRaster<RGBA8BitColor> raster, string text, PointXY origin, float height)
            => raster.DrawText(text, GlyphSet.Default, origin, height, RGBA8BitColor.Black);

        /// <summary>Draws colored text using the shared, prebuilt 5-by-7 pixel glyph set.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="text">Case-sensitive Latin and Russian text; unsupported characters use '?'. Line breaks are supported.</param>
        /// <param name="origin">The first baseline in world coordinates, with Y increasing upwards.</param>
        /// <param name="height">The finite positive seven-dot glyph height in world units.</param>
        /// <param name="color">The text color and opacity.</param>
        public static void DrawText(this SpatialRaster<RGBA8BitColor> raster, string text, PointXY origin, float height, RGBA8BitColor color)
            => raster.DrawText(text, GlyphSet.Default, origin, height, color);

        /// <summary>Draws text using lazily cached glyph coverage masks and source-over compositing.</summary>
        /// <param name="raster">The raster modified in place.</param>
        /// <param name="text">Unicode text. CR, LF and CRLF start a new line; unsupported characters use the font's missing glyph.</param>
        /// <param name="glyphs">The reusable glyph cache.</param>
        /// <param name="origin">The first line's baseline origin in world coordinates, with Y increasing upwards.</param>
        /// <param name="height">The finite positive em size in world units, independent of the mask resolution.</param>
        /// <param name="color">The text color and opacity.</param>
        /// <remarks>Uses horizontal kerning and font line metrics. Mask resampling uses 4-by-4 coverage samples per destination cell.
        /// Does not perform complex-script shaping, bidirectional layout or tab expansion. Color channels are composited without gamma correction.</remarks>
        /// <exception cref="ArgumentNullException">The raster, text or glyph set is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The origin or height is invalid, or text placement exceeds the finite coordinate range.</exception>
        public static void DrawText(this SpatialRaster<RGBA8BitColor> raster, string text, GlyphSet glyphs, PointXY origin, float height, RGBA8BitColor color)
        {
            if (raster == null)
                throw new ArgumentNullException(nameof(raster));
            if (glyphs == null)
                throw new ArgumentNullException(nameof(glyphs));
            if (float.IsNaN(origin.X) || float.IsInfinity(origin.X) || float.IsNaN(origin.Y) || float.IsInfinity(origin.Y))
                throw new ArgumentOutOfRangeException(nameof(origin));
            var placements = glyphs.Layout(text, height, out _);
            float scale = height / glyphs.PixelHeight;
            if (scale == 0f)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (color.A == 0)
                return;
            foreach (var placement in placements)
            {
                var mask = glyphs.GetMask(placement);
                if (mask.Width == 0 || mask.Height == 0)
                    continue;
                float left = origin.X + placement.X + mask.Left * scale;
                float bottom = origin.Y + placement.Y + mask.Bottom * scale;
                float right = left + mask.Width * scale;
                float top = bottom + mask.Height * scale;
                if (float.IsInfinity(left) || float.IsInfinity(bottom) || float.IsInfinity(right) || float.IsInfinity(top))
                    throw new ArgumentOutOfRangeException(nameof(height), "Glyph placement exceeds the finite coordinate range.");
                DrawGlyphMask(raster, mask, left, bottom, right, top, scale, color);
            }
        }

        private static void DrawGlyphMask(SpatialRaster<RGBA8BitColor> raster, GlyphSet.GlyphMask mask,
            float left, float bottom, float right, float top, float scale, RGBA8BitColor color)
        {
            var grid = raster.Geometry;
            float cellWidth = grid.Size.X / raster.Resolution.X;
            float cellHeight = grid.Size.Y / raster.Resolution.Y;
            int x0 = (int)MathF.Max(0, MathF.Min(raster.Resolution.X, MathF.Floor((left - grid.Origin.X) / cellWidth)));
            int x1 = (int)MathF.Max(0, MathF.Min(raster.Resolution.X, MathF.Ceiling((right - grid.Origin.X) / cellWidth)));
            int y0 = (int)MathF.Max(0, MathF.Min(raster.Resolution.Y, MathF.Floor((bottom - grid.Origin.Y) / cellHeight)));
            int y1 = (int)MathF.Max(0, MathF.Min(raster.Resolution.Y, MathF.Ceiling((top - grid.Origin.Y) / cellHeight)));
            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    int coverage = 0;
                    for (int sy = 0; sy < 4; sy++)
                    {
                        for (int sx = 0; sx < 4; sx++)
                        {
                            float mx = (grid.Origin.X - left + (x + (sx + 0.5f) / 4) * cellWidth) / scale;
                            float my = (grid.Origin.Y - bottom + (y + (sy + 0.5f) / 4) * cellHeight) / scale;
                            if (mx >= 0 && mx < mask.Width && my >= 0 && my < mask.Height)
                                coverage += mask.Values[(int)my * mask.Width + (int)mx];
                        }
                    }
                    if (coverage > 0)
                        raster[x, y] = Composite(raster[x, y], color, coverage / (16f * 255f));
                }
            }
        }
    }
}
