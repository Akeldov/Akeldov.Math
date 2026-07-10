using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class QrsBasisDocumentationSnapshotTests
{
    [TestCase(Layout.OddR, "qrs-basis-pointy-top.png")]
    [TestCase(Layout.OddQ, "qrs-basis-flat-top.png")]
    public void Rasterize_WithDocumentationBasisExample_MatchesApprovedImage(Layout layout, string fileName)
    {
        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
        if (!File.Exists(fontPath))
        {
            Assert.Ignore("Arial is not available on this machine.");
            return;
        }

        var p0 = new PointXY(0, 0);
        VectorXY q = new VectorQRS(1, 0).ToVectorXY(layout);
        VectorXY r = new VectorQRS(-1, 1).ToVectorXY(layout);
        VectorXY s = new VectorQRS(0, -1).ToVectorXY(layout);
        TrueTypeFont font = TrueTypeFont.Load(fontPath);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.White, RGBA16BitColor.AlphaOver);
        var centered = new TextLayoutOptions { Anchor = TextAnchor.Center };

        SpatialRaster<RGBA16BitColor> raster = scene
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + q * 0.82f), RGBA16BitColor.Red, 0.01f, 0.01f)
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + r * 0.82f), RGBA16BitColor.Green, 0.01f, 0.01f)
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + s * 0.82f), RGBA16BitColor.Blue, 0.01f, 0.01f)
            .AddTextLayer(font, "+Q", p0 + q * 0.98f, 0.13f, RGBA16BitColor.Red, 0.01f, centered)
            .AddTextLayer(font, "+R", p0 + r * 0.98f, 0.13f, RGBA16BitColor.Green, 0.01f, centered)
            .AddTextLayer(font, "+S", p0 + s * 0.98f, 0.13f, RGBA16BitColor.Blue, 0.01f, centered)
            .Rasterize(new SpatialRasterGrid(
                new PointXY(-1.1f, -1.1f),
                new VectorXY(2.2f, 2.2f),
                new VectorXYInt(300, 300)));

        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);
        raster.SaveAsPng(actualPath);
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory, "Rasterization", "Approved", fileName);

        if (!File.Exists(approvedPath) || !File.ReadAllBytes(actualPath).SequenceEqual(File.ReadAllBytes(approvedPath)))
        {
            TestContext.AddTestAttachment(actualPath, "Actual QRS basis documentation snapshot");
            Assert.Fail($"QRS basis documentation snapshot changed. Actual image: {actualPath}");
        }
    }
}
