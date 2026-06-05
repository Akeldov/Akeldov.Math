using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Topology.Grids.Rasterization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexAdjacencyMapRasterizationSnapshotTests
{
    [TestCase(Layout.OddR, "hex-adjacency-map-main-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-adjacency-map-main-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-adjacency-map-main-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-adjacency-map-main-index-even-q-rgba16.png")]
    public void Rasterize_WithMainIndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var adjacencyMap = new HexAdjacencyMap(
            width: 12,
            height: 8,
            layout: layout);
        var adjacencyGrid = new IndexedHexAdjacencyGrid(
            adjacencyMap,
            resolution: new VectorXYInt(480, 360));

        RGBA16BitRaster raster = adjacencyGrid.Rasterize(adjacency => ToMainIndexColor(adjacency, adjacencyMap.Width));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-adjacency-map-adjacent-1-index-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-adjacency-map-adjacent-1-index-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-adjacency-map-adjacent-1-index-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-adjacency-map-adjacent-1-index-even-q-rgba16.png")]
    public void Rasterize_WithAdjacent1IndexColor_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var adjacencyMap = new HexAdjacencyMap(
            width: 12,
            height: 8,
            layout: layout);
        var adjacencyGrid = new IndexedHexAdjacencyGrid(
            adjacencyMap,
            resolution: new VectorXYInt(480, 360));

        RGBA16BitRaster raster = adjacencyGrid.Rasterize(adjacency => ToAdjacent1IndexColor(adjacency, adjacencyMap.Width));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RGBA16BitColor ToMainIndexColor(Septuplet<int> adjacency, int mapWidth)
    {
        if (adjacency.Main < 0)
            return new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);

        return ToIndexColor(adjacency.Main, mapWidth);
    }

    private static RGBA16BitColor ToAdjacent1IndexColor(Septuplet<int> adjacency, int mapWidth)
    {
        if (adjacency.Adjacent1 < 0)
            return new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue);

        return ToIndexColor(adjacency.Adjacent1, mapWidth);
    }

    private static RGBA16BitColor ToIndexColor(int index, int mapWidth)
    {
        int x = index % mapWidth;
        int y = index / mapWidth;
        float red = 0.12f + 0.07f * x;
        float green = 0.18f + 0.14f * y;
        float blue = 0.82f - 0.012f * index;

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
            TestContext.AddTestAttachment(actualPath, "Actual hex adjacency map raster snapshot");
            Assert.Fail($"Hex adjacency map approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual hex adjacency map raster snapshot");
            Assert.Fail($"Hex adjacency map raster snapshot changed. Actual image: {actualPath}");
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
