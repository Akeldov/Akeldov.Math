using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexSeptupletGridRasterizationSnapshotTests
{
    [TestCase(Layout.OddR, "index-septuplet-grid-main-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-septuplet-grid-main-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-septuplet-grid-main-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-septuplet-grid-main-index-even-q-rgba16.png")]
    public void MapValues_WithMainIndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var indexSeptupletMap = new IndexSeptupletMap(new HexMapTopology(12, 8, layout));
        var hexMapGeometry = CreateLegacySnapshotGeometry(indexSeptupletMap.Topology);
        var indexSeptupletGrid = new IndexSeptupletGrid(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), new VectorXYInt(480, 360)));

        var raster = indexSeptupletGrid.MapValues(adjacency => ToMainIndexColor(adjacency, indexSeptupletMap.Topology.Resolution.X));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "index-septuplet-grid-adjacent-1-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-septuplet-grid-adjacent-1-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-septuplet-grid-adjacent-1-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-septuplet-grid-adjacent-1-index-even-q-rgba16.png")]
    public void MapValues_WithAdjacent1IndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var indexSeptupletMap = new IndexSeptupletMap(new HexMapTopology(12, 8, layout));
        var hexMapGeometry = CreateLegacySnapshotGeometry(indexSeptupletMap.Topology);
        var indexSeptupletGrid = new IndexSeptupletGrid(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), new VectorXYInt(480, 360)));

        var raster = indexSeptupletGrid.MapValues(adjacency => ToAdjacent1IndexColor(adjacency, indexSeptupletMap.Topology.Resolution.X));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RGBA16BitColor ToMainIndexColor(Septuplet<VectorXYInt> adjacency, int mapWidth)
    {
        return ToIndexColor(adjacency.Main, mapWidth);
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

    private static RGBA16BitColor ToAdjacent1IndexColor(Septuplet<VectorXYInt> adjacency, int mapWidth)
    {
        return ToIndexColor(adjacency.Adjacent1, mapWidth);
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
            TestContext.AddTestAttachment(actualPath, "Actual index septuplet grid raster snapshot");
            Assert.Fail($"Index septuplet grid approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual index septuplet grid raster snapshot");
            Assert.Fail($"Index septuplet grid raster snapshot changed. Actual image: {actualPath}");
        }
    }

    private static string GetActualPath(string approvedFileName)
    {
        return Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
    }

    private static bool BytesEqual(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
                return false;
        }

        return true;
    }
}
