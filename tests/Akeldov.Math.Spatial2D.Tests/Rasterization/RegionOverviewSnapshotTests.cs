using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class RegionOverviewSnapshotTests
{
    private static readonly SpatialRasterGrid SnapshotGrid = new SpatialRasterGrid(
        new PointXY(-3f, -3f),
        new VectorXY(6f, 6f),
        new VectorXYInt(96, 96));

    [TestCaseSource(nameof(RegionCases))]
    public void Rasterize_WhenRegionOverviewIsRendered_MatchesApprovedImage(
        string approvedFileName,
        Func<IRegion> createRegion)
    {
        SpatialRaster<Gray8BitColor> raster = createRegion().Rasterize(ToGray8, SnapshotGrid);
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, approvedFileName.Replace(".png", ".actual.png"));
        raster.SaveAsPng(actualPath);

        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Rasterization", "Approved", approvedFileName);
        if (!File.Exists(approvedPath))
        {
            TestContext.AddTestAttachment(actualPath, "Actual region overview snapshot");
            Assert.Fail($"Approved image is missing. Actual image: {actualPath}");
        }

        Assert.That(File.ReadAllBytes(actualPath), Is.EqualTo(File.ReadAllBytes(approvedPath)));
    }

    private static IEnumerable<TestCaseData> RegionCases()
    {
        yield return new TestCaseData("disk-region.png", Region(() => new Disk(new PointXY(0f, 0f), 1.8f))).SetName("DiskRegion_MatchesApprovedImage");
        yield return new TestCaseData("rectangle-region.png", Region(() => new Rectangle(new PointXY(-2f, -1.4f), new PointXY(2f, 1.4f)))).SetName("RectangleRegion_MatchesApprovedImage");
        yield return new TestCaseData("oriented-rectangle-region.png", Region(() => new OrientedRectangle(new PointXY(0f, 0f), new VectorXY(4f, 2.2f), MathF.PI / 6f))).SetName("OrientedRectangleRegion_MatchesApprovedImage");
        yield return new TestCaseData("contour-based-region.png", Region(CreateContourBasedRegion)).SetName("ContourBasedRegion_MatchesApprovedImage");
    }

    private static IContourBasedRegion CreateContourBasedRegion() => new ContourBasedRegion(new IContour[]
    {
        new Circle(new PointXY(0f, 0f), 2.2f),
        new Circle(new PointXY(0.6f, 0.2f), 0.8f)
    });

    private static Func<IRegion> Region(Func<IRegion> createRegion) => createRegion;

    private static Gray8BitColor ToGray8(float signedDistance)
    {
        const float edgeFalloff = 0.15f;
        float coverage = signedDistance <= 0f
            ? 1f
            : 1f - System.Math.Clamp(signedDistance / edgeFalloff, 0f, 1f);
        return new Gray8BitColor((byte)MathF.Round(coverage * byte.MaxValue));
    }
}
