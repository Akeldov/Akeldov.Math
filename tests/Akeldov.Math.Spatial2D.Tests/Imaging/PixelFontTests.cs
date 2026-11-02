using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class PixelFontTests
{
    private static SpatialRaster<RGBA8BitColor> CreateRaster() => new(
        new RasterGeometry(default(PointXY), new VectorXY(40, 24), new VectorXYInt(40, 24)), new RGBA8BitColor[960]);

    [Test]
    public void DefaultText_RendersExactFiveBySevenConstantsWithCorrectOrientation()
    {
        var raster = CreateRaster();
        raster.DrawText("a", origin: new PointXY(2, 3), height: 7);
        byte[] rows = { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 };
        for (int y = 0; y < 7; y++)
            for (int x = 0; x < 5; x++)
                Assert.That(raster[x + 2, 3 + 6 - y], Is.EqualTo(
                    (rows[y] & (1 << (4 - x))) != 0 ? RGBA8BitColor.Black : default), $"x={x}, row={y}");
        Assert.That(PixelFont.GetGlyph('A').ToArray(), Is.EqualTo(rows));
        Assert.That(raster[7, 3].A, Is.Zero);
    }

    [Test]
    public void DefaultGlyphs_AreReadyAndMatchExplicitSet()
    {
        var a = CreateRaster();
        var b = CreateRaster();
        a.DrawText("Ab!?", default, 7, new RGBA8BitColor(100, 20, 30, 128));
        b.DrawText("AB!?", new GlyphSet(PixelFont.Default, 7), default, 7, new RGBA8BitColor(100, 20, 30, 128));
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(GlyphSet.Default.Font, Is.Null);
        Assert.That(GlyphSet.Default.PixelFont, Is.SameAs(PixelFont.Default));
        Assert.That(GlyphSet.Default.PixelHeight, Is.EqualTo(7));
    }

    [Test]
    public void PixelFont_CustomSpacingAndResolutionAffectLayoutButPreserveDots()
    {
        var glyphs = new GlyphSet(new PixelFont(advance: 8, lineGap: 3), pixelHeight: 14);
        var bounds = glyphs.MeasureText("AA\nA", 7);
        Assert.That(bounds.Min, Is.EqualTo(new PointXY(0, -10)));
        Assert.That(bounds.Max, Is.EqualTo(new PointXY(16, 7)));
        var a = CreateRaster();
        var b = CreateRaster();
        a.DrawText("A", glyphs, default, 7, RGBA8BitColor.Black);
        b.DrawText("A", default, 7);
        Assert.That(a.Values, Is.EqualTo(b.Values));
    }

    [Test]
    public void UnsupportedUnicode_UsesOneQuestionMarkPerScalar()
    {
        var a = CreateRaster();
        var b = CreateRaster();
        a.DrawText("я\U0001F600", default, 7);
        b.DrawText("??", default, 7);
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(PixelFont.GetGlyph('я').ToArray(), Is.EqualTo(PixelFont.GetGlyph('?').ToArray()));
    }

    [Test]
    public void PixelFont_AllConstantsContainSevenFiveBitRows()
    {
        const string characters = " ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,:/#-+()%|=<>?!_'";
        foreach (char character in characters)
        {
            var rows = PixelFont.GetGlyph(character).ToArray();
            Assert.That(rows, Has.Length.EqualTo(7), $"Character {character}");
            Assert.That(rows.All(row => row < 32), Is.True, $"Character {character}");
        }
        Assert.That(PixelFont.GetGlyph(' ').ToArray(), Is.All.Zero);
        Assert.That(PixelFont.GetGlyph('_')[6], Is.EqualTo(31));
    }

    [Test]
    public void DefaultSet_AllowsConcurrentReadOnlyDrawing()
    {
        var expected = CreateRaster();
        expected.DrawText("Test", default, 7);
        Parallel.For(0, 8, _ =>
        {
            var actual = CreateRaster();
            actual.DrawText("Test", default, 7);
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        });
    }

    [Test]
    public void InvalidConfigurationAndDrawingArgumentsThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelFont(4));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelFont(lineGap: -1));
        Assert.Throws<ArgumentNullException>(() => new GlyphSet((PixelFont)null!, 7));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GlyphSet(PixelFont.Default, 6));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().DrawText("A", default, 0));
        Assert.Throws<ArgumentNullException>(() => CreateRaster().DrawText(null!, default, 7));
    }
}
