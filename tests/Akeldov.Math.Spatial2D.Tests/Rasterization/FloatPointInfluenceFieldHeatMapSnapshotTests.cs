using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class FloatPointInfluenceFieldHeatMapSnapshotTests
{
    private static readonly RasterGeometry SnapshotGrid = new RasterGeometry(
        origin: new PointXY(0f, 0f),
        size: new VectorXY(100f, 70f),
        resolution: new VectorXYInt(160, 112));

    [Test]
    public void RasterizeHeatMap_WithNearestField_MatchesApprovedImage()
    {
        var field = new FloatPointInfluenceField(
            new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
            CreateSources());

        SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(SnapshotGrid);
        byte[] actual = SaveToPngBytes(raster, "float-point-influence-field-nearest-heatmap-rgba16.png");

        AssertMatchesApprovedPng("float-point-influence-field-nearest-heatmap-rgba16.png", actual);
    }

    [Test]
    public void RasterizeHeatMap_WithInverseDistanceWeightedField_MatchesApprovedImage()
    {
        var field = new FloatPointInfluenceField(
            new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>(),
            CreateSources());

        SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(SnapshotGrid);
        byte[] actual = SaveToPngBytes(raster, "float-point-influence-field-heatmap-rgba16.png");

        AssertMatchesApprovedPng("float-point-influence-field-heatmap-rgba16.png", actual);
    }

    [Test]
    public void RasterizeHeatMap_WithBarycentricField_MatchesApprovedImage()
    {
        var field = new FloatPointInfluenceField(
            new BarycentricFloatSampler<FloatPointInfluenceSource>(),
            CreateSources());

        SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(SnapshotGrid);
        byte[] actual = SaveToPngBytes(raster, "float-point-influence-field-barycentric-heatmap-rgba16.png");

        AssertMatchesApprovedPng("float-point-influence-field-barycentric-heatmap-rgba16.png", actual);
    }

    [Test]
    public void RasterizeHeatMap_WithFourPointBinaryBarycentricField_MatchesApprovedImage()
    {
        var field = new FloatPointInfluenceField(
            new BarycentricFloatSampler<FloatPointInfluenceSource>(),
            CreateBinaryCornerSources());

        SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(SnapshotGrid);
        byte[] actual = SaveToPngBytes(raster, "float-point-influence-field-binary-corners-heatmap-rgba16.png");

        AssertMatchesApprovedPng("float-point-influence-field-binary-corners-heatmap-rgba16.png", actual);
    }

    private static FloatPointInfluenceSource[] CreateSources()
    {
        return new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),
            new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),
            new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),
            new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)
        };
    }

    private static FloatPointInfluenceSource[] CreateBinaryCornerSources()
    {
        return new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(0f, 70f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(100f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(100f, 70f), 1f)
        };
    }

    private static byte[] SaveToPngBytes(SpatialRaster<RGBA16BitColor> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static void AssertMatchesApprovedPng(string approvedFileName, byte[] actual)
    {
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Rasterization",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual float point influence field heat map snapshot");
            Assert.Fail($"Float point influence field heat map approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!PngSnapshotComparer.AreEquivalent(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual float point influence field heat map snapshot");
            Assert.Fail($"Float point influence field heat map snapshot changed. Actual image: {actualPath}");
        }
    }

    private static string GetActualPath(string approvedFileName)
    {
        return Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
    }

}
