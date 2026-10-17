using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoisePartitionSnapshotTests
{
    [Test]
    public void Partition_WithThreeAreaShares_MatchesApprovedImage()
    {
        var topology = new HexMapTopology(64, 48, Layout.OddR);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(7, 0.3),
            new PerlinNoisePartitionTarget(42, 0.5),
            new PerlinNoisePartitionTarget(99, 0.2)
        };
        var partition = topology.ToPerlinNoiseHexPartitionMap(targets, seed: 42, scale: 18f);
        foreach (var cell in partition.Cells)
            TestContext.WriteLine($"Territory {cell.Id}: {cell.HexIndexes.Count}/{topology.Count} hexes");

        Raster<RGBA16BitColor> raster = partition.Rasterize(topology.Resolution * 12, id => id switch
        {
            7 => new RGBA16BitColor(0x3800, 0x7800, 0xc800, ushort.MaxValue),
            42 => new RGBA16BitColor(0x5000, 0xa000, 0x5800, ushort.MaxValue),
            99 => new RGBA16BitColor(0xc800, 0xb000, 0x7000, ushort.MaxValue),
            _ => throw new InvalidOperationException($"Unexpected territory: {id}")
        });
        const string fileName = "perlin-noise-partition-rgba16.png";
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName.Replace(".png", ".actual.png"));
        raster.SaveAsPng(actualPath);
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
