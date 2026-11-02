using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>Stores lazily rasterized TrueType glyph masks at a fixed em resolution.</summary>
    /// <remarks>Reuse this object across draw calls. Instances and their source font require external synchronization for concurrent use.
    /// Supports Unicode scalar values, horizontal kerning and line breaks, but not complex-script shaping or bidirectional layout.</remarks>
    public sealed class GlyphSet
    {
        private readonly Dictionary<int, GlyphMask> _masks = new Dictionary<int, GlyphMask>();

        /// <summary>Creates a reusable glyph cache without rasterizing any glyphs yet.</summary>
        /// <param name="font">The retained source font.</param>
        /// <param name="pixelHeight">The em size in mask pixels, from 1 to 4096. This controls mask detail, not world-space text size.</param>
        public GlyphSet(TrueTypeFont font, int pixelHeight)
        {
            Font = font ?? throw new ArgumentNullException(nameof(font));
            if (pixelHeight <= 0 || pixelHeight > 4096)
                throw new ArgumentOutOfRangeException(nameof(pixelHeight));
            PixelHeight = pixelHeight;
        }

        /// <summary>Gets the retained source font and its metrics.</summary>
        public TrueTypeFont Font { get; }

        /// <summary>Gets the em size in mask pixels.</summary>
        public int PixelHeight { get; }

        /// <summary>Measures logical text bounds relative to the first baseline at (0, 0).</summary>
        /// <param name="text">Text to measure. CR, LF and CRLF start a new line.</param>
        /// <param name="height">The finite positive em size in world units.</param>
        /// <returns>Logical bounds based on advance widths, ascender, descender and line gap; includes spaces, but not glyph overhangs. Empty text returns a zero-area rectangle.</returns>
        public Rectangle MeasureText(string text, float height)
        {
            Layout(text, height, out Rectangle bounds);
            return bounds;
        }

        internal List<GlyphPlacement> Layout(string text, float height, out Rectangle bounds)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (float.IsNaN(height) || float.IsInfinity(height) || height <= 0f)
                throw new ArgumentOutOfRangeException(nameof(height));
            var placements = new List<GlyphPlacement>();
            float scale = height / Font.UnitsPerEm;
            float lineAdvance = (Font.Ascender - Font.Descender + Font.LineGap) * scale;
            float x = 0f, y = 0f, maxX = 0f;
            int previous = -1;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (character == '\r' || character == '\n')
                {
                    if (character == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                    maxX = MathF.Max(maxX, x);
                    x = 0f;
                    y -= lineAdvance;
                    previous = -1;
                    continue;
                }
                int codePoint = character;
                if (char.IsHighSurrogate(character) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                    codePoint = char.ConvertToUtf32(character, text[++i]);
                else if (char.IsSurrogate(character))
                    codePoint = 0xFFFD;
                int index = Font.GetGlyphIndex(codePoint);
                if (previous >= 0)
                    x += Font.GetKerning(previous, index) * scale;
                placements.Add(new GlyphPlacement(codePoint, index, x, y));
                x += Font.GetAdvanceWidth(index) * scale;
                previous = index;
            }
            maxX = MathF.Max(maxX, x);
            if (float.IsInfinity(maxX) || float.IsNaN(maxX) || float.IsInfinity(y) || float.IsNaN(y))
                throw new ArgumentOutOfRangeException(nameof(height), "Text layout exceeds the finite coordinate range.");
            bounds = text.Length == 0 ? default : new Rectangle(new PointXY(0, y + Font.Descender * scale), new PointXY(maxX, Font.Ascender * scale));
            return placements;
        }

        internal GlyphMask GetMask(GlyphPlacement placement)
        {
            if (_masks.TryGetValue(placement.Index, out GlyphMask? mask))
                return mask;
            var outline = Font.GetGlyphOutline(placement.Index);
            float scale = (float)PixelHeight / Font.UnitsPerEm;
            float left = 0, right = 0, bottom = 0, top = 0;
            foreach (var contour in outline.Contours)
            {
                foreach (var segment in contour.Segments)
                {
                    left = MathF.Min(left, MathF.Min(segment.StartPoint.X, MathF.Min(segment.ControlPoint.X, segment.EndPoint.X)) * scale);
                    right = MathF.Max(right, MathF.Max(segment.StartPoint.X, MathF.Max(segment.ControlPoint.X, segment.EndPoint.X)) * scale);
                    bottom = MathF.Min(bottom, MathF.Min(segment.StartPoint.Y, MathF.Min(segment.ControlPoint.Y, segment.EndPoint.Y)) * scale);
                    top = MathF.Max(top, MathF.Max(segment.StartPoint.Y, MathF.Max(segment.ControlPoint.Y, segment.EndPoint.Y)) * scale);
                }
            }
            left = MathF.Floor(left);
            bottom = MathF.Floor(bottom);
            float width = MathF.Ceiling(right) - left;
            float height = MathF.Ceiling(top) - bottom;
            if (width > 4096 || height > 4096)
                throw new NotSupportedException("A glyph mask cannot exceed 4096 pixels on either axis.");
            mask = new GlyphMask((int)width, (int)height, left, bottom);
            var geometry = TextSignedDistanceProvider.Create(Font, char.ConvertFromUtf32(placement.CodePoint), default, PixelHeight);
            RasterizeMask(mask, geometry);
            _masks.Add(placement.Index, mask);
            return mask;
        }

        private static void RasterizeMask(GlyphMask mask, TextSignedDistanceProvider geometry)
        {
            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < 4; sy++)
                        for (int sx = 0; sx < 4; sx++)
                            if (geometry.Contains(new PointXY(mask.Left + x + (sx + 0.5f) / 4, mask.Bottom + y + (sy + 0.5f) / 4)))
                                covered++;
                    mask.Values[y * mask.Width + x] = (byte)MathF.Round(covered * 255f / 16f);
                }
            }
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct GlyphPlacement
        {
            internal GlyphPlacement(int codePoint, int index, float x, float y)
            { CodePoint = codePoint; Index = index; X = x; Y = y; }
            internal int CodePoint { get; }
            internal int Index { get; }
            internal float X { get; }
            internal float Y { get; }
        }

        internal sealed class GlyphMask
        {
            internal GlyphMask(int width, int height, float left, float bottom)
            { Width = width; Height = height; Left = left; Bottom = bottom; Values = new byte[width * height]; }
            internal int Width { get; }
            internal int Height { get; }
            internal float Left { get; }
            internal float Bottom { get; }
            internal byte[] Values { get; }
        }
    }
}
