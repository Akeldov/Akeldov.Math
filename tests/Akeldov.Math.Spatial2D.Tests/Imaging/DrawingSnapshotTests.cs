using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Tests.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class DrawingSnapshotTests
{
    private static readonly RGBA8BitColor Ink = new(30, 41, 59, 255);
    private static readonly RGBA8BitColor Blue = new(37, 99, 235, 220);
    private static readonly RGBA8BitColor Coral = new(239, 90, 70, 180);

    [Test]
    public void Drawing_AllPrimitivesAndText_MatchesApprovedImage()
    {
        var glyphs = LoadGlyphs();
        var raster = CreateRaster(640, 440);
        raster.DrawText("Spatial2D / Draw & Fill", glyphs, new PointXY(24, 398), 30, Ink);

        raster.DrawText("FillRectangle + FillCircle", glyphs, new PointXY(24, 354), 19, Ink);
        raster.FillRectangle(new PointXY(25.25f, 258.5f), new PointXY(140.5f, 335.25f), Blue);
        raster.Fill(new Rectangle(new PointXY(110, 275), new PointXY(185, 320)), Coral);
        raster.FillCircle(new PointXY(245.25f, 295.5f), 38.5f, Blue);
        raster.Fill(new Circle(new PointXY(280, 295), 27), Coral);

        raster.DrawText("DrawSegment / DrawDashedSegment", glyphs, new PointXY(340, 354), 17, Ink);
        raster.DrawSegment(new PointXY(350, 326), new PointXY(595, 312), 5, Blue);
        raster.Draw(new Segment(new PointXY(350, 300), new PointXY(595, 286)), 2, Ink);
        raster.DrawDashedSegment(new PointXY(350, 274), new PointXY(595, 260), 4, 14, Coral);
        raster.DrawDashed(new Segment(new PointXY(350, 248), new PointXY(595, 234)), 2, 8, Blue);

        raster.DrawText("DrawContour(points)", glyphs, new PointXY(24, 218), 19, Ink);
        raster.DrawContour(new[]
        {
            new PointXY(30, 115), new PointXY(58, 184), new PointXY(145, 173),
            new PointXY(183, 125), new PointXY(105, 98),
        }, 4, Blue);
        raster.DrawText("DrawContour(contour)", glyphs, new PointXY(230, 218), 19, Ink);
        raster.Draw(new Circle(new PointXY(310, 145), 43), 5, Coral);
        raster.Draw(new Rectangle(new PointXY(270, 110), new PointXY(350, 180)), 2, Ink);

        raster.DrawText("GlyphSet / alpha", glyphs, new PointXY(440, 218), 19, Ink);
        raster.DrawText("Aa Bb 0123", glyphs, new PointXY(438, 170), 26, Blue);
        raster.DrawText("Hello, world!", glyphs, new PointXY(438, 135), 25, Coral);
        raster.DrawText("World coordinates, cached glyphs, antialiasing and transparency.",
            glyphs, new PointXY(24, 52), 20, Ink);
        AssertSnapshot(raster, "drawing-all-primitives-text.png");
    }

    [Test]
    public void Drawing_LoremIpsumText_MatchesApprovedImage()
    {
        var glyphs = LoadGlyphs();
        var raster = CreateRaster(720, 420);
        raster.DrawText("Lorem ipsum", glyphs, new PointXY(30, 365), 42, Ink);
        const string text =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\n" +
            "Sed do eiusmod tempor incididunt ut labore et dolore\n" +
            "magna aliqua. Ut enim ad minim veniam, quis nostrud\n" +
            "exercitation ullamco laboris nisi ut aliquip ex ea commodo\n" +
            "consequat. Duis aute irure dolor in reprehenderit in\n" +
            "voluptate velit esse cillum dolore eu fugiat nulla pariatur.";
        raster.DrawText(text, glyphs, new PointXY(30, 310), 25, Ink);
        raster.DrawText("Lorem ipsum dolor sit amet.", glyphs, new PointXY(30, 100), 18, Blue);
        raster.DrawText("Lorem ipsum dolor sit amet.", glyphs, new PointXY(30, 55), 30, Coral);
        AssertSnapshot(raster, "drawing-lorem-ipsum.png");
    }

    [Test]
    public void Drawing_LoremIpsumPixelFont_MatchesApprovedImage()
    {
        var raster = CreateRaster(720, 420);
        raster.DrawText("Lorem ipsum", origin: new PointXY(30, 365), height: 28);
        const string text =
            "Lorem ipsum dolor sit amet, consectetur\n" +
            "adipiscing elit. Sed do eiusmod tempor incididunt\n" +
            "ut labore et dolore magna aliqua. Ut enim ad minim\n" +
            "veniam, quis nostrud exercitation ullamco laboris\n" +
            "nisi ut aliquip ex ea commodo consequat. Duis aute\n" +
            "irure dolor in reprehenderit in voluptate velit\n" +
            "esse cillum dolore eu fugiat nulla pariatur.";
        raster.DrawText(text, origin: new PointXY(30, 310), height: 14);
        raster.DrawText("Lorem ipsum dolor sit amet.", new PointXY(30, 140), 14, Blue);
        raster.DrawText("Lorem ipsum dolor sit amet.", new PointXY(30, 90), 21, Coral);
        // Fractional dot size exercises antialiased resampling of the same constant masks.
        raster.DrawText("Lorem ipsum 0123456789 !?", new PointXY(30.25f, 45.5f), 10.5f, Ink);
        AssertSnapshot(raster, "drawing-lorem-ipsum-pixel-font.png");
    }

    [Test]
    public void Drawing_PixelFontAlphabets_MatchesApprovedImage()
    {
        var raster = CreateRaster(720, 420);
        raster.DrawText("Latin / Русский", new PointXY(24, 370), 28);
        const string alphabets =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ\n" +
            "abcdefghijklmnopqrstuvwxyz\n\n" +
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ\n" +
            "абвгдеёжзийклмнопрстуфхцчшщъыьэюя\n\n" +
            "Съешь ещё этих мягких французских\n" +
            "булок, да выпей чаю. Ёжик и ёлка.\n" +
            "0123456789 !?.,: +-=/()";
        raster.DrawText(alphabets, new PointXY(24, 315), 21, Ink);
        AssertSnapshot(raster, "drawing-pixel-font-alphabets.png");
    }

    private static GlyphSet LoadGlyphs()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
        if (!File.Exists(path))
            Assert.Ignore("Times New Roman is not available on this machine.");
        return new GlyphSet(TrueTypeFont.Load(path), pixelHeight: 48);
    }

    private static SpatialRaster<RGBA8BitColor> CreateRaster(int width, int height)
    {
        var values = new RGBA8BitColor[width * height];
        Array.Fill(values, new RGBA8BitColor(247, 249, 252, 255));
        return new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(default(PointXY), new VectorXY(width, height), new VectorXYInt(width, height)), values);
    }

    private static void AssertSnapshot(SpatialRaster<RGBA8BitColor> raster, string name)
    {
        string actualDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Actual");
        Directory.CreateDirectory(actualDirectory);
        string actualPath = Path.Combine(actualDirectory, name);
        raster.SaveAsPng(actualPath);
        TestContext.AddTestAttachment(actualPath, "Actual drawing snapshot");
        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Imaging", "Approved", name);
        Assert.That(File.Exists(approvedPath), Is.True, $"Approved snapshot is missing. Actual: {actualPath}");
        Assert.That(PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)),
            Is.True, $"Drawing snapshot changed. Actual: {actualPath}");
    }
}
