using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexPartialSeptupletRasterSnapshotTests
{
    private const int MapWidth = 12;
    private const int MapHeight = 8;

    [TestCase(Layout.OddR, "index-partial-septuplet-raster-main-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-partial-septuplet-raster-main-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-partial-septuplet-raster-main-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-partial-septuplet-raster-main-index-even-q-rgba16.png")]
    public void MapValues_WithMainIndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var indexPartialSeptupletMap = new IndexPartialSeptupletMap(
            new HexMapTopology(MapWidth, MapHeight, layout));
        var hexMapGeometry = CreateLegacySnapshotGeometry(indexPartialSeptupletMap.Topology);
        var indexPartialSeptupletGrid = new IndexPartialSeptupletRaster(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), new VectorXYInt(480, 360)));

        var raster = indexPartialSeptupletGrid.MapValues(ToMainIndexColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "index-partial-septuplet-raster-adjacent-1-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-partial-septuplet-raster-adjacent-1-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-partial-septuplet-raster-adjacent-1-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-partial-septuplet-raster-adjacent-1-index-even-q-rgba16.png")]
    public void MapValues_WithAdjacent1IndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var indexPartialSeptupletMap = new IndexPartialSeptupletMap(
            new HexMapTopology(MapWidth, MapHeight, layout));
        var hexMapGeometry = CreateLegacySnapshotGeometry(indexPartialSeptupletMap.Topology);
        var indexPartialSeptupletGrid = new IndexPartialSeptupletRaster(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), new VectorXYInt(480, 360)));

        var raster = indexPartialSeptupletGrid.MapValues(ToAdjacent1IndexColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RGBA16BitColor ToMainIndexColor(PartialSeptuplet<VectorXYInt> adjacency)
    {
        return adjacency.HasMain
            ? ToIndexColor(adjacency.Main, MapWidth)
            : new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);
    }

    private static HexMapGeometry CreateLegacySnapshotGeometry(HexMapTopology topology)
    {
        const float radius = 1f;
        float apothem = radius.ConvertHexRadiusToApothem();
        VectorXY origin = topology.Layout switch
        {
            Layout.OddR => new VectorXY(apothem, radius),
            Layout.EvenR => new VectorXY(2f * apothem, radius),
            Layout.OddQ => new VectorXY(radius, apothem),
            Layout.EvenQ => new VectorXY(radius, 2f * apothem),
            _ => throw new ArgumentOutOfRangeException(nameof(topology)),
        };

        return new HexMapGeometry(topology, origin, radius);
    }

    private static RGBA16BitColor ToAdjacent1IndexColor(PartialSeptuplet<VectorXYInt> adjacency)
    {
        return adjacency.HasAdjacent1
            ? ToIndexColor(adjacency.Adjacent1, MapWidth)
            : new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);
    }

    private static RGBA16BitColor ToIndexColor(VectorXYInt index, int mapWidth)
    {
        int flatIndex = index.Y * mapWidth + index.X;
        float red = 0.12f + 0.07f * index.X;
        float green = 0.18f + 0.14f * index.Y;
        float blue = 0.82f - 0.012f * flatIndex;

        return new RGBA16BitColor(
            ToChannel(red),
            ToChannel(green),
            ToChannel(blue),
            ushort.MaxValue);
    }

    private static ushort ToChannel(float value)
    {
        value = MathF.Min(MathF.Max(value, 0f), 1f);
        return (ushort)MathF.Round(value * ushort.MaxValue);
    }

    private static byte[] SaveToPngBytes(IRaster<RGBA16BitColor> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static void AssertMatchesApprovedPng(string approvedFileName, byte[] actual)
    {
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Topology",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual index partial septuplet grid raster snapshot");
            Assert.Fail($"Index partial septuplet grid approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!PngSnapshotComparer.AreEquivalent(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual index partial septuplet grid raster snapshot");
            Assert.Fail($"Index partial septuplet grid raster snapshot changed. Actual image: {actualPath}");
        }
    }

    private static string GetActualPath(string approvedFileName)
    {
        return Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
    }

}
