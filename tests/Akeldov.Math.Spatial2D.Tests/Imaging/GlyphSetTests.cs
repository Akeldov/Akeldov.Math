using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Tests.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class GlyphSetTests
{
    private static GlyphSet CreateGlyphs(int pixelHeight = 20)
    {
        using var stream = new MemoryStream(GeometrySceneTextLayerTests.TrueTypeTestFont.CreateTriangleFont());
        return new GlyphSet(TrueTypeFont.Load(stream), pixelHeight);
    }

    private static SpatialRaster<RGBA8BitColor> CreateRaster() => new(
        new RasterGeometry(new PointXY(-2, -12), new VectorXY(20, 24), new VectorXYInt(40, 48)),
        new RGBA8BitColor[40 * 48]);

    [Test]
    public void MeasureText_UsesAdvancesAndLineMetrics()
    {
        var glyphs = CreateGlyphs();
        var single = glyphs.MeasureText("AA", 10);
        Assert.That(single.Min, Is.EqualTo(new PointXY(0, -2)));
        Assert.That(single.Max, Is.EqualTo(new PointXY(12, 8)));
        var multiple = glyphs.MeasureText("A\r\nAA", 10);
        Assert.That(multiple.Min, Is.EqualTo(new PointXY(0, -13)));
        Assert.That(multiple.Max, Is.EqualTo(new PointXY(12, 8)));
        Assert.That(glyphs.MeasureText("", 10).Size, Is.EqualTo(default(VectorXY)));
    }

    [Test]
    public void DrawText_RendersBaselineAndReusesGlyphCache()
    {
        var glyphs = CreateGlyphs();
        var a = CreateRaster();
        var b = CreateRaster();
        var color = new RGBA8BitColor(255, 0, 0, 128);
        a.DrawText("A", glyphs, default, 10, color);
        b.DrawText("A", glyphs, default, 10, color);
        Assert.That(a.Values, Is.EqualTo(b.Values));
        Assert.That(a[8, 26], Is.EqualTo(color), "Interior of triangle above baseline.");
        Assert.That(a[8, 22].A, Is.Zero, "Below baseline must remain empty.");
        Assert.That(a.Values.Any(p => p.A > 0 && p.A < 128), Is.True, "Edges retain coverage antialiasing.");
    }

    [Test]
    public void DrawText_NewlineMatchesSeparateBaselineCalls()
    {
        var glyphs = CreateGlyphs();
        var a = CreateRaster();
        var b = CreateRaster();
        var color = new RGBA8BitColor(0, 255, 0, 255);
        a.DrawText("A\r\nA", glyphs, default, 10, color);
        b.DrawText("A", glyphs, default, 10, color);
        b.DrawText("A", glyphs, new PointXY(0, -11), 10, color);
        Assert.That(a.Values, Is.EqualTo(b.Values));
    }

    [Test]
    public void DrawText_ClipsAndPreservesTransparentOrEmptyText()
    {
        var glyphs = CreateGlyphs();
        var raster = CreateRaster();
        raster[0, 0] = new RGBA8BitColor(10, 20, 30, 40);
        var before = raster.Values.ToArray();
        raster.DrawText("A", glyphs, new PointXY(-100, 0), 10, RGBA8BitColor.Black);
        raster.DrawText("", glyphs, default, 10, RGBA8BitColor.Black);
        raster.DrawText("A", glyphs, default, 10, default);
        Assert.That(raster.Values, Is.EqualTo(before));
    }

    [Test]
    public void MeasureText_SupplementaryUnicodeUsesOneMissingGlyph()
    {
        var glyphs = CreateGlyphs();
        Assert.That(glyphs.MeasureText("\U0001F600", 10), Is.EqualTo(glyphs.MeasureText("B", 10)));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void InvalidHeightThrows(float height)
    {
        var glyphs = CreateGlyphs();
        Assert.Throws<ArgumentOutOfRangeException>(() => glyphs.MeasureText("A", height));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().DrawText("A", glyphs, default, height, default));
    }

    [Test]
    public void InvalidInputsThrow()
    {
        var glyphs = CreateGlyphs();
        Assert.Throws<ArgumentNullException>(() => new GlyphSet((TrueTypeFont)null!, 20));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GlyphSet(glyphs.Font!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GlyphSet(glyphs.Font!, 4097));
        Assert.Throws<ArgumentNullException>(() => glyphs.MeasureText(null!, 10));
        Assert.Throws<ArgumentNullException>(() => CreateRaster().DrawText("A", null!, default, 10, default));
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.DrawText(null!, "A", glyphs, default, 10, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateRaster().DrawText("A", glyphs, new PointXY(float.NaN, 0), 10, default));
    }
}
