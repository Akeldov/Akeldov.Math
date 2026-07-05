using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class CullingMapSnapshotTests
{
    private static readonly SpatialRasterGrid SnapshotGrid = new SpatialRasterGrid(
        origin: new PointXY(0f, 0f),
        size: new VectorXY(100f, 70f),
        resolution: new VectorXYInt(160, 112));

    [Test]
    public void RasterizeCullingMap_WithDelaunayCuller_MatchesApprovedImage()
    {
        FloatPointInfluenceSource[] sources = CreateSources();
        var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
        Dictionary<PointXY, RGBA16BitColor> sourceColors = CreateSourceColors(sources);

        Raster<RGBA16BitColor> raster = sources.RasterizeCullingMap(
            SnapshotGrid,
            culler,
            point => sourceColors[point]);

        byte[] actual = SaveToPngBytes(raster, "delaunay-culling-map-rgba16.png");

        AssertMatchesApprovedPng("delaunay-culling-map-rgba16.png", actual);
    }

    [Test]
    public void RasterizeCullingMap_WithHalfPlaneCuller_MatchesApprovedImage()
    {
        FloatPointInfluenceSource[] sources = CreateSources();
        var culler = new HalfPlaneCuller<FloatPointInfluenceSource>(sources);
        Dictionary<PointXY, RGBA16BitColor> sourceColors = CreateSourceColors(sources);

        Raster<RGBA16BitColor> raster = sources.RasterizeCullingMap(
            SnapshotGrid,
            culler,
            point => sourceColors[point]);

        byte[] actual = SaveToPngBytes(raster, "half-plane-culling-map-rgba16.png");

        AssertMatchesApprovedPng("half-plane-culling-map-rgba16.png", actual);
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

    private static Dictionary<PointXY, RGBA16BitColor> CreateSourceColors(
        IReadOnlyList<FloatPointInfluenceSource> sources)
    {
        var colors = new Dictionary<PointXY, RGBA16BitColor>(sources.Count)
        {
            { sources[0].Position, new RGBA16BitColor(0xefef, 0x4444, 0x4444, 0xffff) },
            { sources[1].Position, new RGBA16BitColor(0x2222, 0xc5c5, 0x5e5e, 0xffff) },
            { sources[2].Position, new RGBA16BitColor(0x3b3b, 0x8282, 0xf6f6, 0xffff) },
            { sources[3].Position, new RGBA16BitColor(0xf5f5, 0x9e9e, 0x0b0b, 0xffff) },
            { sources[4].Position, new RGBA16BitColor(0xa8a8, 0x5555, 0xf7f7, 0xffff) }
        };

        return colors;
    }

    private static byte[] SaveToPngBytes(Raster<RGBA16BitColor> raster, string approvedFileName)
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
            TestContext.AddTestAttachment(actualPath, "Actual culling map snapshot");
            Assert.Fail($"Culling map approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual culling map snapshot");
            Assert.Fail($"Culling map snapshot changed. Actual image: {actualPath}");
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
