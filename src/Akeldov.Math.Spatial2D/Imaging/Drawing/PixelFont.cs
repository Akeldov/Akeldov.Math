using System;
using System.Collections.Generic;

namespace Akeldov.Math.Spatial2D.Imaging
{
    /// <summary>Built-in 5-by-7 bitmap font with uppercase and lowercase Latin and Russian letters, digits and punctuation.</summary>
    /// <remarks>Unsupported Unicode characters use '?'.</remarks>
    public sealed partial class PixelFont
    {
        /// <summary>Glyph width in font dots.</summary>
        public const int GlyphWidth = 5;
        /// <summary>Glyph height in font dots and units per em.</summary>
        public const int GlyphHeight = 7;
        private static readonly Dictionary<char, byte[]> Glyphs = CreateGlyphs();

        /// <summary>Gets the immutable built-in font with one-dot character and line spacing.</summary>
        public static PixelFont Default { get; } = new PixelFont();

        /// <summary>Creates a built-in pixel font with configurable spacing.</summary>
        /// <param name="advance">Character advance in font dots, at least five.</param>
        /// <param name="lineGap">Non-negative gap between lines in font dots.</param>
        public PixelFont(int advance = 6, int lineGap = 1)
        {
            if (advance < GlyphWidth)
                throw new ArgumentOutOfRangeException(nameof(advance));
            if (lineGap < 0)
                throw new ArgumentOutOfRangeException(nameof(lineGap));
            Advance = advance;
            LineGap = lineGap;
        }

        /// <summary>Gets the character advance in font dots.</summary>
        public int Advance { get; }
        /// <summary>Gets the gap between lines in font dots.</summary>
        public int LineGap { get; }

        /// <summary>Returns a read-only view of the seven retained glyph rows, top first, with five low bits per row.</summary>
        /// <param name="character">The case-sensitive character; unsupported characters use '?'.</param>
        /// <returns>A read-only structural view of the built-in glyph data.</returns>
        public static ReadOnlySpan<byte> GetGlyph(char character) => Glyphs[GetCharacter(character)];

        internal static char GetCharacter(int codePoint)
        {
            char character = codePoint <= char.MaxValue ? (char)codePoint : '?';
            return Glyphs.ContainsKey(character) ? character : '?';
        }

        internal static IEnumerable<char> Characters => Glyphs.Keys;

        private static Dictionary<char, byte[]> CreateGlyphs()
        {
            const string characters = " ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,:/#-+()%|=<>?!_'";
            string[] rows =
            {
                "00000000000000", "0E11111F111111", "1E11111E11111E", "0E11101010110E",
                "1C12111111121C", "1F10101E10101F", "1F10101E101010", "0E11101711110F",
                "1111111F111111", "0E04040404040E", "0702020202120C", "11121418141211",
                "1010101010101F", "111B1515111111", "11111915131111", "0E11111111110E",
                "1E11111E101010", "0E11111115120D", "1E11111E141211", "0F10100E01011E",
                "1F040404040404", "1111111111110E", "11111111110A04", "1111111515150A",
                "11110A040A1111", "1111110A040404", "1F01020408101F",
                "0E11131519110E", "040C040404040E", "0E11010204081F", "1F02040201110E",
                "02060A121F0202", "1F101E0101110E", "0608101E11110E", "1F010204080808",
                "0E11110E11110E", "0E11110F01020C", "00000000000C0C", "000000000C0408",
                "000C0C000C0C00", "00010204081000", "0A0A1F0A1F0A0A", "0000001F000000",
                "0004041F040400", "02040808080402", "08040202020408", "18190204081303",
                "04040404040404", "00001F001F0000", "02040810080402", "08040201020408",
                "0E110102040004", "04040404040004", "0000000000001F", "0C040800000000",
            };
            var glyphs = new Dictionary<char, byte[]>();
            for (int i = 0; i < characters.Length; i++)
            {
                var glyph = new byte[GlyphHeight];
                for (int y = 0; y < GlyphHeight; y++)
                    glyph[y] = Convert.ToByte(rows[i].Substring(y * 2, 2), 16);
                glyphs.Add(characters[i], glyph);
            }
            AddAlphabetGlyphs(glyphs);
            return glyphs;
        }
    }
}
