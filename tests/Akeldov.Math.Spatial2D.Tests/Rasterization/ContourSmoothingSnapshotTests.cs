using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class ContourSmoothingSnapshotTests
{
    private static readonly RasterGeometry SnapshotGrid = new RasterGeometry(
        origin: new PointXY(-0.5f, -0.5f),
        size: new VectorXY(5f, 5f),
        resolution: new VectorXYInt(160, 160));

    [TestCase(false, "square-before-smoothing.png")]
    [TestCase(true, "square-after-smoothing.png")]
    public void Rasterize_WhenSquareSmoothingIsCompared_MatchesApprovedImage(
        bool smooth,
        string approvedFileName)
    {
        var square = new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(4f, 0f),
            new PointXY(4f, 4f),
            new PointXY(0f, 4f));

        IContour contour = smooth ? square.FilletCorners(radius: 0.5f) : square;
        SpatialRaster<Gray8BitColor> raster = contour.Rasterize(
            width: 0.08f,
            edgeFalloff: 0.04f,
            color: new Gray8BitColor(byte.MaxValue),
            rasterGeometry: SnapshotGrid);

        string actualPath = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
        raster.SaveAsPng(actualPath);

        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Rasterization",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            TestContext.AddTestAttachment(actualPath, "Actual contour smoothing snapshot");
            Assert.Fail($"Approved image is missing. Actual image: {actualPath}");
        }

        Assert.That(
            PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)),
            Is.True);
    }
}
