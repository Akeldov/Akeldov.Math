using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoisePartitionSnapshotTests
{
    [Test]
    public void SplitDisconnectedCells_WithPerlinNoisePartition_MatchesApprovedImage()
    {
        const int seed = 42;
        const string fileName = "perlin-noise-split-disconnected-cells-rgba16.png";
        const string context = "seed=42, scale=18, shares=30/50/20, layout=OddR";
        var topology = new HexMapTopology(64, 48, Layout.OddR);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(7, 0.3),
            new PerlinNoisePartitionTarget(42, 0.5),
            new PerlinNoisePartitionTarget(99, 0.2)
        };
        var source = new HexMapGeometry(topology, 1f).ToPerlinNoiseHexPartitionMap(targets, seed, scale: 18f);

        var result = source.SplitDisconnectedCells();

        Assert.That(result.Cells.Count, Is.GreaterThan(source.Cells.Count), context);
        CellularPottsTests.AssertConsistentAndConnected(result, context);
        foreach (var cell in result.Cells)
            Assert.That(cell.HexIndexes.Select(index => source[index]).Distinct().Count(), Is.EqualTo(1),
                $"Result cell {cell.Id} mixes source territories. {context}");
        TestContext.WriteLine($"Split {source.Cells.Count} territories into {result.Cells.Count} connected cells ({context}).");

        // Give each component its own deterministic color, including components from the same territory.
        RGBA16BitColor ColorForComponent(int id)
        {
            uint color = unchecked(((uint)id + 1) * 2654435761u);
            return new RGBA16BitColor(
                (ushort)(((color >> 16) & 255) * 257),
                (ushort)(((color >> 8) & 255) * 257),
                (ushort)((color & 255) * 257),
                ushort.MaxValue);
        }

        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName.Replace(".png", ".actual.png"));
        result.Rasterize(topology.Resolution * 12, ColorForComponent).SaveAsPng(actualPath);
        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "Partitioning", "PerlinNoise", "Approved", fileName);
        if (!File.Exists(approvedPath) ||
            !PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)))
        {
            TestContext.AddTestAttachment(actualPath, $"Perlin-noise partition after splitting disconnected cells: {context}");
            Assert.Fail($"Split Perlin-noise partition snapshot changed or is missing ({context}). Actual image: {actualPath}");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Partition_WithThreeAreaShares_MatchesApprovedImage(bool worldSpace)
    {
        var topology = new HexMapTopology(64, 48, Layout.OddR);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(7, 0.3),
            new PerlinNoisePartitionTarget(42, 0.5),
            new PerlinNoisePartitionTarget(99, 0.2)
        };
        var geometry = worldSpace
            ? new HexMapGeometry(topology, new VectorXY(-30f, 50f), 2.5f)
            : new HexMapGeometry(topology, 1f);
        var partition = geometry.ToPerlinNoiseHexPartitionMap(targets, seed: 42, scale: worldSpace ? 45f : 18f);
        foreach (var cell in partition.Cells)
            TestContext.WriteLine($"Territory {cell.Id}: {cell.HexIndexes.Count}/{topology.Count} hexes");

        Func<int, RGBA16BitColor> colorSelector = id => id switch
        {
            7 => new RGBA16BitColor(0x3800, 0x7800, 0xc800, ushort.MaxValue),
            42 => new RGBA16BitColor(0x5000, 0xa000, 0x5800, ushort.MaxValue),
            99 => new RGBA16BitColor(0xc800, 0xb000, 0x7000, ushort.MaxValue),
            _ => throw new InvalidOperationException($"Unexpected territory: {id}")
        };
        string fileName = worldSpace ? "perlin-noise-geometry-partition-rgba16.png" : "perlin-noise-partition-rgba16.png";
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName.Replace(".png", ".actual.png"));
        if (worldSpace)
            ((ISpatialHexMap<int>)partition).Rasterize(6f, colorSelector).SaveAsPng(actualPath);
        else
            ((IHexMap<int>)partition).Rasterize(topology.Resolution * 12, colorSelector).SaveAsPng(actualPath);
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
