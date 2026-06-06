using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Topology.Grids.Rasterization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexPartialSeptupletMapRasterizationSnapshotTests
{
    [TestCase(Layout.OddR, "index-partial-septuplet-map-main-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-partial-septuplet-map-main-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-partial-septuplet-map-main-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-partial-septuplet-map-main-index-even-q-rgba16.png")]
    public void Rasterize_WithMainIndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var indexSeptupletMap = new IndexSeptupletMap(
            width: 12,
            height: 8,
            layout: layout);
        var indexPartialSeptupletMap = new IndexPartialSeptupletMap(
            width: indexSeptupletMap.Width,
            height: indexSeptupletMap.Height,
            layout: indexSeptupletMap.Layout);
        var adjacencyGrid = new IndexedHexAdjacencyGrid(
            indexSeptupletMap,
            resolution: new VectorXYInt(480, 360));

        RGBA16BitRaster raster = adjacencyGrid.Rasterize(
            adjacency => ToMainIndexColor(adjacency.Main, indexPartialSeptupletMap));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "index-partial-septuplet-map-adjacent-1-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-partial-septuplet-map-adjacent-1-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-partial-septuplet-map-adjacent-1-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-partial-septuplet-map-adjacent-1-index-even-q-rgba16.png")]
    public void Rasterize_WithAdjacent1IndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var indexSeptupletMap = new IndexSeptupletMap(
            width: 12,
            height: 8,
            layout: layout);
        var indexPartialSeptupletMap = new IndexPartialSeptupletMap(
            width: indexSeptupletMap.Width,
            height: indexSeptupletMap.Height,
            layout: indexSeptupletMap.Layout);
        var adjacencyGrid = new IndexedHexAdjacencyGrid(
            indexSeptupletMap,
            resolution: new VectorXYInt(480, 360));

        RGBA16BitRaster raster = adjacencyGrid.Rasterize(
            adjacency => ToAdjacent1IndexColor(adjacency.Main, indexPartialSeptupletMap));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RGBA16BitColor ToMainIndexColor(VectorXYInt index, IndexPartialSeptupletMap map)
    {
        if (!ContainsIndex(index, map.Width, map.Height))
            return new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);

        PartialSeptuplet<VectorXYInt> adjacency = map[index];
        return adjacency.HasMain
            ? ToIndexColor(adjacency.Main, map.Width)
            : new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);
    }

    private static RGBA16BitColor ToAdjacent1IndexColor(VectorXYInt index, IndexPartialSeptupletMap map)
    {
        if (!ContainsIndex(index, map.Width, map.Height))
            return new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);

        PartialSeptuplet<VectorXYInt> adjacency = map[index];
        return adjacency.HasAdjacent1
            ? ToIndexColor(adjacency.Adjacent1, map.Width)
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

    private static bool ContainsIndex(VectorXYInt index, int mapWidth, int mapHeight)
    {
        return (uint)index.X < (uint)mapWidth &&
            (uint)index.Y < (uint)mapHeight;
    }

    private static ushort ToChannel(float value)
    {
        value = MathF.Min(MathF.Max(value, 0f), 1f);
        return (ushort)MathF.Round(value * ushort.MaxValue);
    }

    private static byte[] SaveToPngBytes(RGBA16BitRaster raster, string approvedFileName)
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
            TestContext.AddTestAttachment(actualPath, "Actual index partial septuplet map raster snapshot");
            Assert.Fail($"Index partial septuplet map approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual index partial septuplet map raster snapshot");
            Assert.Fail($"Index partial septuplet map raster snapshot changed. Actual image: {actualPath}");
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
