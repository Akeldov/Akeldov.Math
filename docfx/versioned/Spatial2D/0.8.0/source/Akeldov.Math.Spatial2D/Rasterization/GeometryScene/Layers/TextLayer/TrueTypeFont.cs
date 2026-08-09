using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Akeldov.Math.Spatial2D.Rasterization
{
    /// <summary>
    /// Represents a loaded TrueType font with quadratic glyph outlines and horizontal metrics.
    /// </summary>
    public sealed class TrueTypeFont
    {
        private readonly byte[] _data;
        private readonly TableRecord _glyfTable;
        private readonly uint[] _glyphLocations;
        private readonly ushort[] _advanceWidths;
        private readonly short[] _leftSideBearings;
        private readonly ICharacterMap _characterMap;
        private readonly Dictionary<uint, short> _kerningPairs;
        private readonly Dictionary<int, TrueTypeGlyphOutline> _glyphCache;

        private TrueTypeFont(
            byte[] data,
            TableRecord glyfTable,
            uint[] glyphLocations,
            ushort[] advanceWidths,
            short[] leftSideBearings,
            ICharacterMap characterMap,
            Dictionary<uint, short> kerningPairs,
            int unitsPerEm,
            int ascender,
            int descender,
            int lineGap,
            int glyphCount)
        {
            _data = data;
            _glyfTable = glyfTable;
            _glyphLocations = glyphLocations;
            _advanceWidths = advanceWidths;
            _leftSideBearings = leftSideBearings;
            _characterMap = characterMap;
            _kerningPairs = kerningPairs;
            _glyphCache = new Dictionary<int, TrueTypeGlyphOutline>();

            UnitsPerEm = unitsPerEm;
            Ascender = ascender;
            Descender = descender;
            LineGap = lineGap;
            GlyphCount = glyphCount;
        }

        /// <summary>
        /// Gets the number of font design units in one em.
        /// </summary>
        public int UnitsPerEm { get; }

        /// <summary>
        /// Gets the horizontal ascender metric in font design units.
        /// </summary>
        public int Ascender { get; }

        /// <summary>
        /// Gets the horizontal descender metric in font design units.
        /// </summary>
        public int Descender { get; }

        /// <summary>
        /// Gets the horizontal line gap metric in font design units.
        /// </summary>
        public int LineGap { get; }

        /// <summary>
        /// Gets the number of glyphs in the font.
        /// </summary>
        public int GlyphCount { get; }

        /// <summary>
        /// Loads a TrueType font from a file.
        /// </summary>
        /// <param name="path">The path to the TrueType font file.</param>
        /// <returns>The loaded font.</returns>
        public static TrueTypeFont Load(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return Load(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Loads a TrueType font from a stream.
        /// </summary>
        /// <param name="stream">The readable stream that contains TrueType font bytes.</param>
        /// <returns>The loaded font.</returns>
        public static TrueTypeFont Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("Font stream must be readable.", nameof(stream));

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Load(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Returns the glyph index mapped from the specified Unicode code point, or zero when no mapping exists.
        /// </summary>
        /// <param name="unicodeCodePoint">The Unicode code point.</param>
        /// <returns>The glyph index.</returns>
        public int GetGlyphIndex(int unicodeCodePoint)
        {
            if (unicodeCodePoint < 0 || unicodeCodePoint > 0x10FFFF)
                throw new ArgumentOutOfRangeException(nameof(unicodeCodePoint), "Unicode code point must be in the valid Unicode range.");

            return _characterMap.GetGlyphIndex(unicodeCodePoint);
        }

        /// <summary>
        /// Returns the advance width for the specified glyph, in font design units.
        /// </summary>
        /// <param name="glyphIndex">The glyph index.</param>
        /// <returns>The glyph advance width in font design units.</returns>
        public int GetAdvanceWidth(int glyphIndex)
        {
            ValidateGlyphIndex(glyphIndex, nameof(glyphIndex));
            return _advanceWidths[glyphIndex];
        }

        /// <summary>
        /// Returns the left side bearing for the specified glyph, in font design units.
        /// </summary>
        /// <param name="glyphIndex">The glyph index.</param>
        /// <returns>The glyph left side bearing in font design units.</returns>
        public int GetLeftSideBearing(int glyphIndex)
        {
            ValidateGlyphIndex(glyphIndex, nameof(glyphIndex));
            return _leftSideBearings[glyphIndex];
        }

        /// <summary>
        /// Returns the legacy TrueType kerning adjustment for the specified glyph pair, in font design units.
        /// </summary>
        /// <param name="leftGlyphIndex">The left glyph index.</param>
        /// <param name="rightGlyphIndex">The right glyph index.</param>
        /// <returns>The kerning adjustment in font design units, or zero when no pair is present.</returns>
        public int GetKerning(int leftGlyphIndex, int rightGlyphIndex)
        {
            ValidateGlyphIndex(leftGlyphIndex, nameof(leftGlyphIndex));
            ValidateGlyphIndex(rightGlyphIndex, nameof(rightGlyphIndex));

            uint key = ((uint)leftGlyphIndex << 16) | (uint)rightGlyphIndex;
            return _kerningPairs.TryGetValue(key, out short value) ? value : 0;
        }

        internal TrueTypeGlyphOutline GetGlyphOutline(int glyphIndex)
        {
            ValidateGlyphIndex(glyphIndex, nameof(glyphIndex));

            if (_glyphCache.TryGetValue(glyphIndex, out TrueTypeGlyphOutline? outline))
                return outline;

            outline = ParseGlyph(glyphIndex, depth: 0);
            _glyphCache.Add(glyphIndex, outline);
            return outline;
        }

        private static TrueTypeFont Load(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length < 12)
                throw new FormatException("TrueType font data is too short.");

            Dictionary<string, TableRecord> tables = ReadTableDirectory(data);
            TableRecord head = GetRequiredTable(tables, "head");
            TableRecord hhea = GetRequiredTable(tables, "hhea");
            TableRecord hmtx = GetRequiredTable(tables, "hmtx");
            TableRecord maxp = GetRequiredTable(tables, "maxp");
            TableRecord loca = GetRequiredTable(tables, "loca");
            TableRecord glyf = GetRequiredTable(tables, "glyf");
            TableRecord cmap = GetRequiredTable(tables, "cmap");

            int unitsPerEm = ReadUInt16(data, head.Offset + 18);
            if (unitsPerEm <= 0)
                throw new FormatException("TrueType head.unitsPerEm must be positive.");

            short indexToLocFormat = ReadInt16(data, head.Offset + 50);
            if (indexToLocFormat != 0 && indexToLocFormat != 1)
                throw new FormatException("TrueType head.indexToLocFormat is not supported.");

            int glyphCount = ReadUInt16(data, maxp.Offset + 4);
            if (glyphCount <= 0)
                throw new FormatException("TrueType maxp.numGlyphs must be positive.");

            int ascender = ReadInt16(data, hhea.Offset + 4);
            int descender = ReadInt16(data, hhea.Offset + 6);
            int lineGap = ReadInt16(data, hhea.Offset + 8);
            int numberOfHorizontalMetrics = ReadUInt16(data, hhea.Offset + 34);
            if (numberOfHorizontalMetrics <= 0 || numberOfHorizontalMetrics > glyphCount)
                throw new FormatException("TrueType hhea.numberOfHMetrics is invalid.");

            uint[] glyphLocations = ReadGlyphLocations(data, loca, glyphCount, indexToLocFormat);
            ReadHorizontalMetrics(data, hmtx, glyphCount, numberOfHorizontalMetrics, out ushort[] advanceWidths, out short[] leftSideBearings);
            ICharacterMap characterMap = ReadCharacterMap(data, cmap);
            Dictionary<uint, short> kerningPairs = tables.TryGetValue("kern", out TableRecord kern)
                ? ReadKerningPairs(data, kern)
                : new Dictionary<uint, short>();

            return new TrueTypeFont(
                data,
                glyf,
                glyphLocations,
                advanceWidths,
                leftSideBearings,
                characterMap,
                kerningPairs,
                unitsPerEm,
                ascender,
                descender,
                lineGap,
                glyphCount);
        }

        private static Dictionary<string, TableRecord> ReadTableDirectory(byte[] data)
        {
            int tableCount = ReadUInt16(data, 4);
            int directoryEnd = 12 + checked(tableCount * 16);
            EnsureRange(data, 0, directoryEnd);

            var tables = new Dictionary<string, TableRecord>(tableCount);
            for (int i = 0; i < tableCount; i++)
            {
                int entryOffset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(data, entryOffset, 4);
                int offset = checked((int)ReadUInt32(data, entryOffset + 8));
                int length = checked((int)ReadUInt32(data, entryOffset + 12));
                EnsureRange(data, offset, length);
                tables[tag] = new TableRecord(offset, length);
            }

            return tables;
        }

        private static TableRecord GetRequiredTable(Dictionary<string, TableRecord> tables, string tag)
        {
            if (!tables.TryGetValue(tag, out TableRecord table))
                throw new FormatException($"TrueType table '{tag}' is missing.");

            return table;
        }

        private static uint[] ReadGlyphLocations(byte[] data, TableRecord loca, int glyphCount, short indexToLocFormat)
        {
            var glyphLocations = new uint[glyphCount + 1];

            if (indexToLocFormat == 0)
            {
                EnsureRange(data, loca.Offset, checked((glyphCount + 1) * 2));
                for (int i = 0; i < glyphLocations.Length; i++)
                {
                    glyphLocations[i] = (uint)(ReadUInt16(data, loca.Offset + i * 2) * 2);
                }
            }
            else
            {
                EnsureRange(data, loca.Offset, checked((glyphCount + 1) * 4));
                for (int i = 0; i < glyphLocations.Length; i++)
                {
                    glyphLocations[i] = ReadUInt32(data, loca.Offset + i * 4);
                }
            }

            return glyphLocations;
        }

        private static void ReadHorizontalMetrics(
            byte[] data,
            TableRecord hmtx,
            int glyphCount,
            int numberOfHorizontalMetrics,
            out ushort[] advanceWidths,
            out short[] leftSideBearings)
        {
            advanceWidths = new ushort[glyphCount];
            leftSideBearings = new short[glyphCount];

            int metricsLength = checked(numberOfHorizontalMetrics * 4 + (glyphCount - numberOfHorizontalMetrics) * 2);
            EnsureRange(data, hmtx.Offset, metricsLength);

            ushort lastAdvanceWidth = 0;
            for (int i = 0; i < numberOfHorizontalMetrics; i++)
            {
                int offset = hmtx.Offset + i * 4;
                lastAdvanceWidth = ReadUInt16(data, offset);
                advanceWidths[i] = lastAdvanceWidth;
                leftSideBearings[i] = ReadInt16(data, offset + 2);
            }

            int bearingOffset = hmtx.Offset + numberOfHorizontalMetrics * 4;
            for (int i = numberOfHorizontalMetrics; i < glyphCount; i++)
            {
                advanceWidths[i] = lastAdvanceWidth;
                leftSideBearings[i] = ReadInt16(data, bearingOffset + (i - numberOfHorizontalMetrics) * 2);
            }
        }

        private static ICharacterMap ReadCharacterMap(byte[] data, TableRecord cmap)
        {
            EnsureRange(data, cmap.Offset, 4);
            int subtableCount = ReadUInt16(data, cmap.Offset + 2);
            EnsureRange(data, cmap.Offset, 4 + subtableCount * 8);

            ICharacterMap? bestMap = null;
            int bestScore = -1;

            for (int i = 0; i < subtableCount; i++)
            {
                int recordOffset = cmap.Offset + 4 + i * 8;
                int platformId = ReadUInt16(data, recordOffset);
                int encodingId = ReadUInt16(data, recordOffset + 2);
                int subtableOffset = checked(cmap.Offset + (int)ReadUInt32(data, recordOffset + 4));
                EnsureRange(data, subtableOffset, 2);
                int format = ReadUInt16(data, subtableOffset);

                ICharacterMap? map = null;
                int score = -1;
                if (format == 12)
                {
                    map = new Format12CharacterMap(data, subtableOffset);
                    score = platformId == 3 && encodingId == 10 ? 100 : platformId == 0 ? 90 : 70;
                }
                else if (format == 4)
                {
                    map = new Format4CharacterMap(data, subtableOffset);
                    score = platformId == 3 && encodingId == 1 ? 80 : platformId == 0 ? 75 : 60;
                }

                if (map != null && score > bestScore)
                {
                    bestMap = map;
                    bestScore = score;
                }
            }

            if (bestMap == null)
                throw new FormatException("TrueType cmap must contain a supported Unicode format 4 or format 12 subtable.");

            return bestMap;
        }

        private static Dictionary<uint, short> ReadKerningPairs(byte[] data, TableRecord kern)
        {
            var pairs = new Dictionary<uint, short>();
            EnsureRange(data, kern.Offset, 4);

            int subtableCount = ReadUInt16(data, kern.Offset + 2);
            int offset = kern.Offset + 4;
            int tableEnd = kern.Offset + kern.Length;

            for (int i = 0; i < subtableCount && offset + 6 <= tableEnd; i++)
            {
                int length = ReadUInt16(data, offset + 2);
                int coverage = ReadUInt16(data, offset + 4);
                if (length < 6 || offset + length > tableEnd)
                    break;

                int format = (coverage >> 8) & 0xFF;
                if (format == 0)
                {
                    int pairCount = ReadUInt16(data, offset + 6);
                    int pairOffset = offset + 14;
                    for (int pairIndex = 0; pairIndex < pairCount && pairOffset + 6 <= offset + length; pairIndex++)
                    {
                        int left = ReadUInt16(data, pairOffset);
                        int right = ReadUInt16(data, pairOffset + 2);
                        short value = ReadInt16(data, pairOffset + 4);
                        pairs[((uint)left << 16) | (uint)right] = value;
                        pairOffset += 6;
                    }
                }

                offset += length;
            }

            return pairs;
        }

        private TrueTypeGlyphOutline ParseGlyph(int glyphIndex, int depth)
        {
            if (depth > 32)
                throw new FormatException("TrueType composite glyph nesting is too deep.");

            uint glyphStart = _glyphLocations[glyphIndex];
            uint glyphEnd = _glyphLocations[glyphIndex + 1];
            if (glyphStart == glyphEnd)
                return TrueTypeGlyphOutline.Empty;

            if (glyphEnd < glyphStart || glyphEnd > _glyfTable.Length)
                throw new FormatException("TrueType glyph location table contains invalid offsets.");

            int offset = checked(_glyfTable.Offset + (int)glyphStart);
            EnsureRange(_data, offset, checked((int)(glyphEnd - glyphStart)));

            short contourCount = ReadInt16(_data, offset);
            if (contourCount >= 0)
                return ParseSimpleGlyph(offset, contourCount);

            return ParseCompositeGlyph(offset, depth);
        }

        private TrueTypeGlyphOutline ParseSimpleGlyph(int glyphOffset, int contourCount)
        {
            if (contourCount == 0)
                return TrueTypeGlyphOutline.Empty;

            int endPointOffset = glyphOffset + 10;
            EnsureRange(_data, endPointOffset, contourCount * 2 + 2);

            var endPointIndices = new ushort[contourCount];
            for (int i = 0; i < contourCount; i++)
            {
                endPointIndices[i] = ReadUInt16(_data, endPointOffset + i * 2);
            }

            int pointCount = endPointIndices[contourCount - 1] + 1;
            int instructionLengthOffset = endPointOffset + contourCount * 2;
            int instructionLength = ReadUInt16(_data, instructionLengthOffset);
            int offset = instructionLengthOffset + 2 + instructionLength;
            EnsureRange(_data, offset, 0);

            var flags = new byte[pointCount];
            for (int i = 0; i < pointCount;)
            {
                byte flag = _data[offset++];
                flags[i++] = flag;

                if ((flag & 0x08) != 0)
                {
                    byte repeatCount = _data[offset++];
                    for (int repeat = 0; repeat < repeatCount; repeat++)
                    {
                        if (i >= flags.Length)
                            throw new FormatException("TrueType glyph flag repeat exceeds point count.");

                        flags[i++] = flag;
                    }
                }
            }

            var xs = new int[pointCount];
            int x = 0;
            for (int i = 0; i < pointCount; i++)
            {
                byte flag = flags[i];
                if ((flag & 0x02) != 0)
                {
                    int dx = _data[offset++];
                    x += (flag & 0x10) != 0 ? dx : -dx;
                }
                else if ((flag & 0x10) == 0)
                {
                    x += ReadInt16(_data, offset);
                    offset += 2;
                }

                xs[i] = x;
            }

            var ys = new int[pointCount];
            int y = 0;
            for (int i = 0; i < pointCount; i++)
            {
                byte flag = flags[i];
                if ((flag & 0x04) != 0)
                {
                    int dy = _data[offset++];
                    y += (flag & 0x20) != 0 ? dy : -dy;
                }
                else if ((flag & 0x20) == 0)
                {
                    y += ReadInt16(_data, offset);
                    offset += 2;
                }

                ys[i] = y;
            }

            var contours = new TrueTypeGlyphContour[contourCount];
            int contourStart = 0;
            for (int i = 0; i < contourCount; i++)
            {
                int contourEnd = endPointIndices[i];
                var points = new GlyphPoint[contourEnd - contourStart + 1];
                for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
                {
                    int sourceIndex = contourStart + pointIndex;
                    points[pointIndex] = new GlyphPoint(
                        new PointXY(xs[sourceIndex], ys[sourceIndex]),
                        (flags[sourceIndex] & 0x01) != 0);
                }

                contours[i] = BuildContour(points);
                contourStart = contourEnd + 1;
            }

            return new TrueTypeGlyphOutline(contours);
        }

        private TrueTypeGlyphOutline ParseCompositeGlyph(int glyphOffset, int depth)
        {
            const int ArgsAreWords = 0x0001;
            const int ArgsAreXyValues = 0x0002;
            const int WeHaveScale = 0x0008;
            const int MoreComponents = 0x0020;
            const int WeHaveXAndYScale = 0x0040;
            const int WeHaveTwoByTwo = 0x0080;
            const int WeHaveInstructions = 0x0100;

            int offset = glyphOffset + 10;
            int flags;
            var contours = new List<TrueTypeGlyphContour>();

            do
            {
                flags = ReadUInt16(_data, offset);
                int componentGlyphIndex = ReadUInt16(_data, offset + 2);
                offset += 4;

                int arg1;
                int arg2;
                if ((flags & ArgsAreWords) != 0)
                {
                    arg1 = ReadInt16(_data, offset);
                    arg2 = ReadInt16(_data, offset + 2);
                    offset += 4;
                }
                else
                {
                    arg1 = (sbyte)_data[offset];
                    arg2 = (sbyte)_data[offset + 1];
                    offset += 2;
                }

                if ((flags & ArgsAreXyValues) == 0)
                    throw new NotSupportedException("Composite glyph point-matching arguments are not supported.");

                float a = 1f;
                float b = 0f;
                float c = 0f;
                float d = 1f;

                if ((flags & WeHaveScale) != 0)
                {
                    a = ReadF2Dot14(_data, offset);
                    d = a;
                    offset += 2;
                }
                else if ((flags & WeHaveXAndYScale) != 0)
                {
                    a = ReadF2Dot14(_data, offset);
                    d = ReadF2Dot14(_data, offset + 2);
                    offset += 4;
                }
                else if ((flags & WeHaveTwoByTwo) != 0)
                {
                    a = ReadF2Dot14(_data, offset);
                    b = ReadF2Dot14(_data, offset + 2);
                    c = ReadF2Dot14(_data, offset + 4);
                    d = ReadF2Dot14(_data, offset + 6);
                    offset += 8;
                }

                TrueTypeGlyphOutline component = ParseGlyph(componentGlyphIndex, depth + 1);
                TrueTypeGlyphOutline transformed = component.Transform(a, b, c, d, arg1, arg2);
                contours.AddRange(transformed.Contours);
            }
            while ((flags & MoreComponents) != 0);

            if ((flags & WeHaveInstructions) != 0)
            {
                int instructionLength = ReadUInt16(_data, offset);
                offset += 2 + instructionLength;
                EnsureRange(_data, offset, 0);
            }

            return new TrueTypeGlyphOutline(contours.ToArray());
        }

        private static TrueTypeGlyphContour BuildContour(IReadOnlyList<GlyphPoint> points)
        {
            var segments = new List<TrueTypeGlyphSegment>();
            if (points.Count == 0)
                return new TrueTypeGlyphContour(Array.Empty<TrueTypeGlyphSegment>());

            PointXY startPoint;
            int index;
            int processed;

            GlyphPoint first = points[0];
            GlyphPoint last = points[points.Count - 1];
            if (first.IsOnCurve)
            {
                startPoint = first.Point;
                index = 1;
                processed = 1;
            }
            else if (last.IsOnCurve)
            {
                startPoint = last.Point;
                index = 0;
                processed = 0;
            }
            else
            {
                startPoint = Midpoint(last.Point, first.Point);
                index = 0;
                processed = 0;
            }

            PointXY current = startPoint;
            while (processed < points.Count)
            {
                GlyphPoint point = points[index % points.Count];
                if (point.IsOnCurve)
                {
                    segments.Add(TrueTypeGlyphSegment.Line(current, point.Point));
                    current = point.Point;
                    index++;
                    processed++;
                    continue;
                }

                GlyphPoint next = points[(index + 1) % points.Count];
                if (next.IsOnCurve)
                {
                    segments.Add(TrueTypeGlyphSegment.Quadratic(current, point.Point, next.Point));
                    current = next.Point;
                    index += 2;
                    processed += 2;
                }
                else
                {
                    PointXY implied = Midpoint(point.Point, next.Point);
                    segments.Add(TrueTypeGlyphSegment.Quadratic(current, point.Point, implied));
                    current = implied;
                    index++;
                    processed++;
                }
            }

            if (!current.Equals(startPoint))
                segments.Add(TrueTypeGlyphSegment.Line(current, startPoint));

            return new TrueTypeGlyphContour(segments.ToArray());
        }

        private void ValidateGlyphIndex(int glyphIndex, string paramName)
        {
            if (glyphIndex < 0 || glyphIndex >= GlyphCount)
                throw new ArgumentOutOfRangeException(paramName, "Glyph index must refer to a glyph in this font.");
        }

        private static PointXY Midpoint(PointXY left, PointXY right)
        {
            return new PointXY(
                (left.X + right.X) * 0.5f,
                (left.Y + right.Y) * 0.5f);
        }

        private static ushort ReadUInt16(byte[] data, int offset)
        {
            EnsureRange(data, offset, 2);
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        private static short ReadInt16(byte[] data, int offset)
        {
            return unchecked((short)ReadUInt16(data, offset));
        }

        private static uint ReadUInt32(byte[] data, int offset)
        {
            EnsureRange(data, offset, 4);
            return ((uint)data[offset] << 24) |
                ((uint)data[offset + 1] << 16) |
                ((uint)data[offset + 2] << 8) |
                data[offset + 3];
        }

        private static float ReadF2Dot14(byte[] data, int offset)
        {
            return ReadInt16(data, offset) / 16384f;
        }

        private static void EnsureRange(byte[] data, int offset, int length)
        {
            if (offset < 0 || length < 0 || offset > data.Length - length)
                throw new FormatException("TrueType font data is truncated or contains invalid offsets.");
        }

        private readonly struct TableRecord
        {
            public TableRecord(int offset, int length)
            {
                Offset = offset;
                Length = length;
            }

            public int Offset { get; }

            public int Length { get; }
        }

        private readonly struct GlyphPoint
        {
            public GlyphPoint(PointXY point, bool isOnCurve)
            {
                Point = point;
                IsOnCurve = isOnCurve;
            }

            public PointXY Point { get; }

            public bool IsOnCurve { get; }
        }

        private interface ICharacterMap
        {
            int GetGlyphIndex(int unicodeCodePoint);
        }

        private sealed class Format4CharacterMap : ICharacterMap
        {
            private readonly byte[] _data;
            private readonly int _subtableOffset;
            private readonly int _length;
            private readonly ushort[] _endCodes;
            private readonly ushort[] _startCodes;
            private readonly short[] _idDeltas;
            private readonly ushort[] _idRangeOffsets;
            private readonly int _idRangeOffsetArrayOffset;

            public Format4CharacterMap(byte[] data, int subtableOffset)
            {
                _data = data;
                _subtableOffset = subtableOffset;
                _length = ReadUInt16(data, subtableOffset + 2);
                int segmentCount = ReadUInt16(data, subtableOffset + 6) / 2;
                int endCodeOffset = subtableOffset + 14;
                int startCodeOffset = endCodeOffset + segmentCount * 2 + 2;
                int idDeltaOffset = startCodeOffset + segmentCount * 2;
                _idRangeOffsetArrayOffset = idDeltaOffset + segmentCount * 2;

                EnsureRange(data, subtableOffset, _length);
                _endCodes = new ushort[segmentCount];
                _startCodes = new ushort[segmentCount];
                _idDeltas = new short[segmentCount];
                _idRangeOffsets = new ushort[segmentCount];

                for (int i = 0; i < segmentCount; i++)
                {
                    _endCodes[i] = ReadUInt16(data, endCodeOffset + i * 2);
                    _startCodes[i] = ReadUInt16(data, startCodeOffset + i * 2);
                    _idDeltas[i] = ReadInt16(data, idDeltaOffset + i * 2);
                    _idRangeOffsets[i] = ReadUInt16(data, _idRangeOffsetArrayOffset + i * 2);
                }
            }

            public int GetGlyphIndex(int unicodeCodePoint)
            {
                if (unicodeCodePoint < 0 || unicodeCodePoint > 0xFFFF)
                    return 0;

                for (int i = 0; i < _endCodes.Length; i++)
                {
                    if (unicodeCodePoint > _endCodes[i])
                        continue;

                    if (unicodeCodePoint < _startCodes[i])
                        return 0;

                    ushort idRangeOffset = _idRangeOffsets[i];
                    if (idRangeOffset == 0)
                        return (unicodeCodePoint + _idDeltas[i]) & 0xFFFF;

                    int idRangeOffsetPosition = _idRangeOffsetArrayOffset + i * 2;
                    int glyphOffset = idRangeOffsetPosition + idRangeOffset + (unicodeCodePoint - _startCodes[i]) * 2;
                    if (glyphOffset < _subtableOffset || glyphOffset + 2 > _subtableOffset + _length)
                        return 0;

                    int glyphIndex = ReadUInt16(_data, glyphOffset);
                    return glyphIndex == 0 ? 0 : (glyphIndex + _idDeltas[i]) & 0xFFFF;
                }

                return 0;
            }
        }

        private sealed class Format12CharacterMap : ICharacterMap
        {
            private readonly CharacterMapGroup[] _groups;

            public Format12CharacterMap(byte[] data, int subtableOffset)
            {
                int length = checked((int)ReadUInt32(data, subtableOffset + 4));
                EnsureRange(data, subtableOffset, length);

                int groupCount = checked((int)ReadUInt32(data, subtableOffset + 12));
                _groups = new CharacterMapGroup[groupCount];
                int groupOffset = subtableOffset + 16;
                for (int i = 0; i < groupCount; i++)
                {
                    _groups[i] = new CharacterMapGroup(
                        ReadUInt32(data, groupOffset),
                        ReadUInt32(data, groupOffset + 4),
                        ReadUInt32(data, groupOffset + 8));
                    groupOffset += 12;
                }
            }

            public int GetGlyphIndex(int unicodeCodePoint)
            {
                uint codePoint = (uint)unicodeCodePoint;
                for (int i = 0; i < _groups.Length; i++)
                {
                    CharacterMapGroup group = _groups[i];
                    if (codePoint < group.StartCode)
                        return 0;

                    if (codePoint <= group.EndCode)
                        return checked((int)(group.StartGlyphIndex + codePoint - group.StartCode));
                }

                return 0;
            }
        }

        private readonly struct CharacterMapGroup
        {
            public CharacterMapGroup(uint startCode, uint endCode, uint startGlyphIndex)
            {
                StartCode = startCode;
                EndCode = endCode;
                StartGlyphIndex = startGlyphIndex;
            }

            public uint StartCode { get; }

            public uint EndCode { get; }

            public uint StartGlyphIndex { get; }
        }
    }

    internal sealed class TrueTypeGlyphOutline
    {
        public static readonly TrueTypeGlyphOutline Empty = new TrueTypeGlyphOutline(Array.Empty<TrueTypeGlyphContour>());

        public TrueTypeGlyphOutline(IReadOnlyList<TrueTypeGlyphContour> contours)
        {
            Contours = contours ?? throw new ArgumentNullException(nameof(contours));
        }

        public IReadOnlyList<TrueTypeGlyphContour> Contours { get; }

        public TrueTypeGlyphOutline Transform(float a, float b, float c, float d, float dx, float dy)
        {
            var contours = new TrueTypeGlyphContour[Contours.Count];
            for (int i = 0; i < contours.Length; i++)
            {
                contours[i] = Contours[i].Transform(a, b, c, d, dx, dy);
            }

            return new TrueTypeGlyphOutline(contours);
        }
    }

    internal sealed class TrueTypeGlyphContour
    {
        public TrueTypeGlyphContour(IReadOnlyList<TrueTypeGlyphSegment> segments)
        {
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public IReadOnlyList<TrueTypeGlyphSegment> Segments { get; }

        public TrueTypeGlyphContour Transform(float a, float b, float c, float d, float dx, float dy)
        {
            var segments = new TrueTypeGlyphSegment[Segments.Count];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = Segments[i].Transform(a, b, c, d, dx, dy);
            }

            return new TrueTypeGlyphContour(segments);
        }
    }

    internal readonly struct TrueTypeGlyphSegment
    {
        private TrueTypeGlyphSegment(TrueTypeGlyphSegmentKind kind, PointXY startPoint, PointXY controlPoint, PointXY endPoint)
        {
            Kind = kind;
            StartPoint = startPoint;
            ControlPoint = controlPoint;
            EndPoint = endPoint;
        }

        public TrueTypeGlyphSegmentKind Kind { get; }

        public PointXY StartPoint { get; }

        public PointXY ControlPoint { get; }

        public PointXY EndPoint { get; }

        public static TrueTypeGlyphSegment Line(PointXY startPoint, PointXY endPoint)
        {
            return new TrueTypeGlyphSegment(TrueTypeGlyphSegmentKind.Line, startPoint, startPoint, endPoint);
        }

        public static TrueTypeGlyphSegment Quadratic(PointXY startPoint, PointXY controlPoint, PointXY endPoint)
        {
            return new TrueTypeGlyphSegment(TrueTypeGlyphSegmentKind.Quadratic, startPoint, controlPoint, endPoint);
        }

        public TrueTypeGlyphSegment Transform(float a, float b, float c, float d, float dx, float dy)
        {
            return new TrueTypeGlyphSegment(
                Kind,
                Transform(StartPoint, a, b, c, d, dx, dy),
                Transform(ControlPoint, a, b, c, d, dx, dy),
                Transform(EndPoint, a, b, c, d, dx, dy));
        }

        private static PointXY Transform(PointXY point, float a, float b, float c, float d, float dx, float dy)
        {
            return new PointXY(
                point.X * a + point.Y * b + dx,
                point.X * c + point.Y * d + dy);
        }
    }

    internal enum TrueTypeGlyphSegmentKind
    {
        Line,
        Quadratic
    }
}
