using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoisePartitionSnapshotTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Partition_WithThreeAreaShares_MatchesApprovedImage(bool spatial)
    {
        var topology = new HexMapTopology(64, 48, Layout.OddR);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(7, 0.3),
            new PerlinNoisePartitionTarget(42, 0.5),
            new PerlinNoisePartitionTarget(99, 0.2)
        };
        var geometry = new HexMapGeometry(topology, new VectorXY(-30f, 50f), 2.5f);
        HexPartitionMap<HexPartitionCell> partition = spatial
            ? geometry.ToPerlinNoiseHexPartitionMap(targets, seed: 42, scale: 45f)
            : topology.ToPerlinNoiseHexPartitionMap(targets, seed: 42, scale: 18f);
        foreach (var cell in partition.Cells)
            TestContext.WriteLine($"Territory {cell.Id}: {cell.HexIndexes.Count}/{topology.Count} hexes");

        Func<int, RGBA16BitColor> colorSelector = id => id switch
        {
            7 => new RGBA16BitColor(0x3800, 0x7800, 0xc800, ushort.MaxValue),
            42 => new RGBA16BitColor(0x5000, 0xa000, 0x5800, ushort.MaxValue),
            99 => new RGBA16BitColor(0xc800, 0xb000, 0x7000, ushort.MaxValue),
            _ => throw new InvalidOperationException($"Unexpected territory: {id}")
        };
        string fileName = spatial ? "perlin-noise-geometry-partition-rgba16.png" : "perlin-noise-partition-rgba16.png";
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName.Replace(".png", ".actual.png"));
        if (spatial)
            ((ISpatialHexMap<int>)partition).Rasterize(6f, colorSelector).SaveAsPng(actualPath);
        else
            partition.Rasterize(topology.Resolution * 12, colorSelector).SaveAsPng(actualPath);
        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "Partitioning", "PerlinNoise", "Approved", fileName);
        if (!File.Exists(approvedPath) ||
            !PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)))
        {
            TestContext.AddTestAttachment(actualPath, "Perlin-noise partition, seed=42, shares=30/50/20");
            Assert.Fail($"Perlin-noise partition snapshot changed or is missing. Actual image: {actualPath}");
        }
    }
}
