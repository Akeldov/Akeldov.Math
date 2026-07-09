using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class GeometrySceneTextLayerTests
{
    [Test]
    public void TrueTypeFont_Load_ReadsMetricsAndCharacterMap()
    {
        TrueTypeFont font = TrueTypeFont.Load(new MemoryStream(TrueTypeTestFont.CreateTriangleFont()));

        Assert.That(font.UnitsPerEm, Is.EqualTo(1000));
        Assert.That(font.Ascender, Is.EqualTo(800));
        Assert.That(font.Descender, Is.EqualTo(-200));
        Assert.That(font.LineGap, Is.EqualTo(100));
        Assert.That(font.GlyphCount, Is.EqualTo(2));
        Assert.That(font.GetGlyphIndex('A'), Is.EqualTo(1));
        Assert.That(font.GetGlyphIndex('B'), Is.EqualTo(0));
        Assert.That(font.GetAdvanceWidth(1), Is.EqualTo(600));
    }

    [Test]
    public void AddTextLayer_RasterizesTrueTypeGlyphFill()
    {
        TrueTypeFont font = TrueTypeFont.Load(new MemoryStream(TrueTypeTestFont.CreateTriangleFont()));
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddTextLayer(
                font,
                "A",
                origin: new PointXY(0f, 0f),
                fontSize: 10f,
                color: RGBA16BitColor.Red,
                edgeFalloff: 0.1f);

        SpatialRaster<RGBA16BitColor> raster = scene.Rasterize(new SpatialRasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(5f, 7f),
            resolution: new VectorXYInt(5, 7)));

        Assert.That(raster[2, 1], Is.EqualTo(RGBA16BitColor.Red));
        Assert.That(raster[4, 6], Is.EqualTo(RGBA16BitColor.Transparent));
    }

    [Test]
    public void AddTextLayer_WithBaselineRightAnchor_PositionsTextByAdvanceWidth()
    {
        TrueTypeFont font = TrueTypeFont.Load(new MemoryStream(TrueTypeTestFont.CreateTriangleFont()));
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddTextLayer(
                font,
                "A",
                origin: new PointXY(6f, 0f),
                fontSize: 10f,
                color: RGBA16BitColor.Red,
                edgeFalloff: 0.1f,
                anchor: TextAnchor.BaselineRight);

        SpatialRaster<RGBA16BitColor> raster = scene.Rasterize(new SpatialRasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(7f, 7f),
            resolution: new VectorXYInt(7, 7)));

        Assert.That(raster[2, 1], Is.EqualTo(RGBA16BitColor.Red));
        Assert.That(raster[6, 1], Is.EqualTo(RGBA16BitColor.Transparent));
    }

    [Test]
    public void TextLayoutOptions_DefaultsUseBaselineLeftAndKerning()
    {
        var options = new TextLayoutOptions();

        Assert.That(options.Anchor, Is.EqualTo(TextAnchor.BaselineLeft));
        Assert.That(options.LetterSpacing, Is.EqualTo(0f));
        Assert.That(options.LineSpacing, Is.EqualTo(0f));
        Assert.That(options.UseKerning, Is.True);
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void AddTextLayer_WhenFontSizeIsInvalid_Throws(float fontSize)
    {
        TrueTypeFont font = TrueTypeFont.Load(new MemoryStream(TrueTypeTestFont.CreateTriangleFont()));
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.AddTextLayer(
            font,
            "A",
            origin: new PointXY(0f, 0f),
            fontSize: fontSize,
            color: RGBA16BitColor.Red,
            edgeFalloff: 0.1f));
    }

    internal static class TrueTypeTestFont
    {
        public static byte[] CreateTriangleFont()
        {
            byte[] glyph = CreateTriangleGlyph();
            byte[] glyf = glyph;
            byte[] loca = BuildLoca(glyf.Length);

            return BuildFont(new[]
            {
                new Table("cmap", BuildCmap()),
                new Table("glyf", glyf),
                new Table("head", BuildHead()),
                new Table("hhea", BuildHhea()),
                new Table("hmtx", BuildHmtx()),
                new Table("loca", loca),
                new Table("maxp", BuildMaxp())
            });
        }

        private static byte[] BuildFont(IReadOnlyList<Table> tables)
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt32(0x00010000);
            writer.WriteUInt16((ushort)tables.Count);
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);

            int tableDataOffset = 12 + tables.Count * 16;
            int currentOffset = tableDataOffset;
            for (int i = 0; i < tables.Count; i++)
            {
                Table table = tables[i];
                writer.WriteTag(table.Tag);
                writer.WriteUInt32(0);
                writer.WriteUInt32((uint)currentOffset);
                writer.WriteUInt32((uint)table.Data.Length);
                currentOffset += Align4(table.Data.Length);
            }

            for (int i = 0; i < tables.Count; i++)
            {
                writer.WriteBytes(tables[i].Data);
                writer.Pad4();
            }

            return writer.ToArray();
        }

        private static byte[] BuildHead()
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt32(0x00010000);
            writer.WriteUInt32(0x00010000);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0x5F0F3CF5);
            writer.WriteUInt16(0);
            writer.WriteUInt16(1000);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(500);
            writer.WriteInt16(700);
            writer.WriteUInt16(0);
            writer.WriteUInt16(8);
            writer.WriteInt16(2);
            writer.WriteInt16(1);
            writer.WriteInt16(0);
            return writer.ToArray();
        }

        private static byte[] BuildHhea()
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt32(0x00010000);
            writer.WriteInt16(800);
            writer.WriteInt16(-200);
            writer.WriteInt16(100);
            writer.WriteUInt16(600);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(600);
            writer.WriteInt16(1);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(2);
            return writer.ToArray();
        }

        private static byte[] BuildMaxp()
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt32(0x00010000);
            writer.WriteUInt16(2);
            return writer.ToArray();
        }

        private static byte[] BuildHmtx()
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt16(500);
            writer.WriteInt16(0);
            writer.WriteUInt16(600);
            writer.WriteInt16(0);
            return writer.ToArray();
        }

        private static byte[] BuildLoca(int glyphLength)
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt32((uint)glyphLength);
            return writer.ToArray();
        }

        private static byte[] BuildCmap()
        {
            byte[] format4 = BuildCmapFormat4();
            var writer = new BigEndianWriter();
            writer.WriteUInt16(0);
            writer.WriteUInt16(1);
            writer.WriteUInt16(3);
            writer.WriteUInt16(1);
            writer.WriteUInt32(12);
            writer.WriteBytes(format4);
            return writer.ToArray();
        }

        private static byte[] BuildCmapFormat4()
        {
            var writer = new BigEndianWriter();
            writer.WriteUInt16(4);
            writer.WriteUInt16(32);
            writer.WriteUInt16(0);
            writer.WriteUInt16(4);
            writer.WriteUInt16(4);
            writer.WriteUInt16(1);
            writer.WriteUInt16(0);
            writer.WriteUInt16(65);
            writer.WriteUInt16(0xFFFF);
            writer.WriteUInt16(0);
            writer.WriteUInt16(65);
            writer.WriteUInt16(0xFFFF);
            writer.WriteInt16(-64);
            writer.WriteInt16(1);
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            return writer.ToArray();
        }

        private static byte[] CreateTriangleGlyph()
        {
            var writer = new BigEndianWriter();
            writer.WriteInt16(1);
            writer.WriteInt16(0);
            writer.WriteInt16(0);
            writer.WriteInt16(500);
            writer.WriteInt16(700);
            writer.WriteUInt16(2);
            writer.WriteUInt16(0);
            writer.WriteByte(0x31);
            writer.WriteByte(0x21);
            writer.WriteByte(0x01);
            writer.WriteInt16(500);
            writer.WriteInt16(-250);
            writer.WriteInt16(700);
            return writer.ToArray();
        }

        private static int Align4(int length)
        {
            int remainder = length % 4;
            return remainder == 0 ? length : length + 4 - remainder;
        }

        private sealed class Table
        {
            public Table(string tag, byte[] data)
            {
                Tag = tag;
                Data = data;
            }

            public string Tag { get; }

            public byte[] Data { get; }
        }

        private sealed class BigEndianWriter
        {
            private readonly List<byte> _bytes = new List<byte>();

            public void WriteByte(byte value)
            {
                _bytes.Add(value);
            }

            public void WriteBytes(byte[] values)
            {
                _bytes.AddRange(values);
            }

            public void WriteTag(string tag)
            {
                for (int i = 0; i < 4; i++)
                {
                    _bytes.Add((byte)tag[i]);
                }
            }

            public void WriteUInt16(ushort value)
            {
                _bytes.Add((byte)(value >> 8));
                _bytes.Add((byte)value);
            }

            public void WriteInt16(short value)
            {
                WriteUInt16(unchecked((ushort)value));
            }

            public void WriteUInt32(uint value)
            {
                _bytes.Add((byte)(value >> 24));
                _bytes.Add((byte)(value >> 16));
                _bytes.Add((byte)(value >> 8));
                _bytes.Add((byte)value);
            }

            public void Pad4()
            {
                while (_bytes.Count % 4 != 0)
                {
                    _bytes.Add(0);
                }
            }

            public byte[] ToArray()
            {
                return _bytes.ToArray();
            }
        }
    }
}
