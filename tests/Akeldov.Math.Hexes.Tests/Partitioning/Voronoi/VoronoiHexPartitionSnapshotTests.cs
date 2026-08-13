using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Partitioning.Voronoi;

public class VoronoiHexPartitionSnapshotTests
{
    private const string ApprovedFileName = "voronoi-hex-partition-rgba16.png";

    [Test]
    public void Partition_WithWeightedSites_MatchesApprovedImage()
    {
        var centers = new HexCenterMap(new HexMapGeometry(12, 9, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[]
        {
            new Site(centers[new VectorXYInt(1, 1)], 1f),
            new Site(centers[new VectorXYInt(9, 1)], 1.35f),
            new Site(centers[new VectorXYInt(3, 7)], 0.8f),
            new Site(centers[new VectorXYInt(9, 7)], 1.1f)
        };
        VoronoiHexPartitionMap partition = centers.ToVoronoiHexPartitionMap(sites);
        var resolution = partition.Topology.Resolution * 32;
        Raster<RGBA16BitColor> raster = partition.Rasterize(resolution, ToSnapshotColor);
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, ApprovedFileName.Replace(".png", ".actual.png"));
        raster.SaveAsPng(actualPath);

        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Partitioning",
            "Voronoi",
            "Approved",
            ApprovedFileName);
        if (!File.Exists(approvedPath) ||
            !PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)))
        {
            TestContext.AddTestAttachment(actualPath, "Actual Voronoi hex partition snapshot");
            Assert.Fail($"Voronoi hex partition snapshot changed or is missing. Actual image: {actualPath}");
        }
    }

    private static RGBA16BitColor ToSnapshotColor(VoronoiCell cell) => cell.SiteIndex switch
    {
        0 => new RGBA16BitColor(0xe800, 0x4800, 0x5000, ushort.MaxValue),
        1 => new RGBA16BitColor(0x3800, 0xb800, 0x7000, ushort.MaxValue),
        2 => new RGBA16BitColor(0x4000, 0x7000, 0xe800, ushort.MaxValue),
        3 => new RGBA16BitColor(0xe000, 0xb800, 0x3800, ushort.MaxValue),
        _ => throw new InvalidOperationException($"Unexpected Voronoi site index: {cell.SiteIndex}.")
    };
}
