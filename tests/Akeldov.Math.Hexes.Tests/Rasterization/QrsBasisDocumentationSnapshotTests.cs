using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class QrsBasisDocumentationSnapshotTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void GetComponentAxes_ReconstructsQrsDifferences(Layout layout)
    {
        (VectorXY q, VectorXY r, VectorXY s) = GetComponentAxes(layout);

        Assert.Multiple(() =>
        {
            Assert.That(q.Length, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(r.Length, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(s.Length, Is.EqualTo(1f).Within(0.0001f));
            Assert.That((q + r + s).Length, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                ((q - s) - new VectorQRS(1, 0).ToVectorXY(layout)).Length,
                Is.EqualTo(0f).Within(0.0001f));
            Assert.That(
                ((r - s) - new VectorQRS(0, 1).ToVectorXY(layout)).Length,
                Is.EqualTo(0f).Within(0.0001f));
        });
    }

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
        (VectorXY q, VectorXY r, VectorXY s) = GetComponentAxes(layout);
        TrueTypeFont font = TrueTypeFont.Load(fontPath);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.White, RGBA16BitColor.AlphaOver);
        var centered = new TextLayoutOptions { Anchor = TextAnchor.Center };
        RGBA16BitColor xColor = RGBA16BitColor.FromNormalized(1f, 0f, 0f, 0.5f);
        RGBA16BitColor yColor = RGBA16BitColor.FromNormalized(0f, 1f, 0f, 0.5f);

        SpatialRaster<RGBA16BitColor> raster = scene
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, new PointXY(1f, 0f)), xColor, 0.006f, 0.006f)
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, new PointXY(0f, 1f)), yColor, 0.006f, 0.006f)
            .AddTextLayer(font, "+X", new PointXY(0.9f, -0.08f), 0.11f, xColor, 0.01f, centered)
            .AddTextLayer(font, "+Y", new PointXY(0.08f, 0.9f), 0.11f, yColor, 0.01f, centered)
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + q), RGBA16BitColor.Red, 0.01f, 0.01f)
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + r), RGBA16BitColor.Green, 0.01f, 0.01f)
            .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + s), RGBA16BitColor.Blue, 0.01f, 0.01f)
            .AddTextLayer(font, "+Q", p0 + q * 1.12f, 0.13f, RGBA16BitColor.Red, 0.01f, centered)
            .AddTextLayer(font, "+R", p0 + r * 1.12f, 0.13f, RGBA16BitColor.Green, 0.01f, centered)
            .AddTextLayer(font, "+S", p0 + s * 1.12f, 0.13f, RGBA16BitColor.Blue, 0.01f, centered)
            .Rasterize(new RasterGeometry(
                new PointXY(-1.25f, -1.25f),
                new VectorXY(2.5f, 2.5f),
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

    private static (VectorXY q, VectorXY r, VectorXY s) GetComponentAxes(Layout layout)
    {
        VectorXY qMinusS = new VectorQRS(1, 0).ToVectorXY(layout);
        VectorXY rMinusS = new VectorQRS(0, 1).ToVectorXY(layout);
        VectorXY s = (qMinusS + rMinusS) * (-1f / 3f);
        VectorXY q = qMinusS + s;
        VectorXY r = rMinusS + s;
        return (q, r, s);
    }
}
