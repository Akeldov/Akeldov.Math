using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>Stores glyph masks from a TrueType or built-in pixel font at a fixed em resolution.</summary>
    /// <remarks>Reuse this object across draw calls. TrueType instances and their source font require external synchronization for concurrent use.
    /// Pixel-font instances are prepared completely during construction and support concurrent read-only drawing.
    /// Supports Unicode scalar values, horizontal kerning and line breaks, but not complex-script shaping or bidirectional layout.</remarks>
    public sealed class GlyphSet
    {
        private readonly Dictionary<int, GlyphMask> _masks = new Dictionary<int, GlyphMask>();

        /// <summary>Gets the shared, fully prepared 5-by-7 pixel glyph set. Concurrent drawing with this set is supported.</summary>
        public static GlyphSet Default { get; } = new GlyphSet(Imaging.PixelFont.Default, Imaging.PixelFont.GlyphHeight);

        /// <summary>Creates all pixel glyph masks directly from built-in constants, without TrueType parsing or outline rasterization.</summary>
        /// <param name="font">The retained immutable pixel font, including character and line spacing.</param>
        /// <param name="pixelHeight">The em size in mask pixels, from 7 to 4096. Multiples of seven preserve uniform dot sizes.</param>
        public GlyphSet(PixelFont font, int pixelHeight)
        {
            PixelFont = font ?? throw new ArgumentNullException(nameof(font));
            if (pixelHeight < Imaging.PixelFont.GlyphHeight || pixelHeight > 4096)
                throw new ArgumentOutOfRangeException(nameof(pixelHeight));
            PixelHeight = pixelHeight;
            foreach (char character in Imaging.PixelFont.Characters)
            {
                ReadOnlySpan<byte> rows = Imaging.PixelFont.GetGlyph(character);
                int width = (int)MathF.Ceiling(pixelHeight * (5f / 7f));
                var mask = new GlyphMask(width, pixelHeight, 0, 0);
                for (int y = 0; y < pixelHeight; y++)
                {
                    int row = 6 - y * 7 / pixelHeight;
                    for (int x = 0; x < width; x++)
                    {
                        int column = x * 7 / pixelHeight;
                        if (column < 5 && (rows[row] & (1 << (4 - column))) != 0)
                            mask.Values[y * width + x] = 255;
                    }
                }
                _masks.Add(character, mask);
            }
        }

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

        /// <summary>Gets the retained TrueType source, or null for a pixel font.</summary>
        public TrueTypeFont? Font { get; }

        /// <summary>Gets the retained pixel source, or null for a TrueType font.</summary>
        public PixelFont? PixelFont { get; }

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
            float scale = height / (Font?.UnitsPerEm ?? Imaging.PixelFont.GlyphHeight);
            float ascender = Font?.Ascender ?? Imaging.PixelFont.GlyphHeight;
            float descender = Font?.Descender ?? 0;
            float lineAdvance = (ascender - descender + (Font?.LineGap ?? PixelFont!.LineGap)) * scale;
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
                int index = Font != null ? Font.GetGlyphIndex(codePoint) : Imaging.PixelFont.GetCharacter(codePoint);
                if (previous >= 0 && Font != null)
                    x += Font.GetKerning(previous, index) * scale;
                placements.Add(new GlyphPlacement(codePoint, index, x, y));
                x += (Font != null ? Font.GetAdvanceWidth(index) : PixelFont!.Advance) * scale;
                previous = index;
            }
            maxX = MathF.Max(maxX, x);
            if (float.IsInfinity(maxX) || float.IsNaN(maxX) || float.IsInfinity(y) || float.IsNaN(y))
                throw new ArgumentOutOfRangeException(nameof(height), "Text layout exceeds the finite coordinate range.");
            bounds = text.Length == 0 ? default : new Rectangle(new PointXY(0, y + descender * scale), new PointXY(maxX, ascender * scale));
            return placements;
        }

        internal GlyphMask GetMask(GlyphPlacement placement)
        {
            if (_masks.TryGetValue(placement.Index, out GlyphMask? mask))
                return mask;
            var font = Font!; // Pixel masks are eagerly populated by the constructor.
            var outline = font.GetGlyphOutline(placement.Index);
            float scale = (float)PixelHeight / font.UnitsPerEm;
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
            var geometry = TextSignedDistanceProvider.Create(font, char.ConvertFromUtf32(placement.CodePoint), default, PixelHeight);
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
