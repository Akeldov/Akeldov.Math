using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoisePartitionSnapshotTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void PartialPartition_WithEquivalentMasks_MatchesApprovedImage(int maskKind)
    {
        const int seed = 42;
        const float scale = 45f;
        const string fileName = "perlin-noise-partial-partition-rgba16.png";
        const string context = "seed=42, scale=45, shares=30/50/20, layout=OddR";
        var geometry = new HexMapGeometry(64, 48, new VectorXY(-30f, 50f), 2.5f, Layout.OddR);
        var targets = new[]
        {
            new PerlinNoisePartitionTarget(7, 0.3),
            new PerlinNoisePartitionTarget(42, 0.5),
            new PerlinNoisePartitionTarget(99, 0.2)
        };
        var mask = new BoolHexMap(geometry.Topology);
        for (int y = 0; y < geometry.Topology.Resolution.Y; y++)
        for (int x = 0; x < geometry.Topology.Resolution.X; x++)
        {
            // An ellipse with a hole and a dividing channel, plus a separate island.
            int dx = x - 29;
            int dy = y - 24;
            bool ellipse = dx * dx * 18 * 18 + dy * dy * 25 * 25 <= 25 * 25 * 18 * 18;
            bool hole = (x - 23) * (x - 23) + (y - 22) * (y - 22) <= 5 * 5;
            bool channel = x >= 37 && x <= 39;
            bool island = (x - 58) * (x - 58) + (y - 8) * (y - 8) <= 4 * 4;
            mask[new VectorXYInt(x, y)] = (ellipse && !hole && !channel) || island;
        }

        var parameters = new PerlinNoisePartitionParameters(scale);
        var regions = new IntHexMap(geometry.Topology, Enumerable.Repeat(-7, geometry.Topology.Count).ToArray());
        var nullableRegions = new HexMap<int?>(geometry.Topology,
            Enumerable.Range(0, geometry.Topology.Count).Select(i => mask[i] ? (int?)-7 : null).ToArray());
        var partition = maskKind switch
        {
            0 => geometry.PartitionByPerlinNoise(mask, targets, parameters, seed: seed),
            1 => geometry.PartitionByPerlinNoise(nullableRegions, targets, parameters, seed: seed),
            2 => geometry.PartitionByPerlinNoise(mask, regions, targets, parameters, seed: seed),
            _ => throw new ArgumentOutOfRangeException(nameof(maskKind))
        };

        PartialPerlinNoisePartitionTests.AssertPartition(partition, geometry.CreatePerlinNoise(seed, scale),
            mask, targets, context);
        int participatingCount = partition.Cells.Sum(cell => cell.HexIndexes.Count);
        Assert.That(participatingCount, Is.GreaterThan(0).And.LessThan(geometry.Topology.Count), context);
        foreach (var cell in partition.Cells)
            TestContext.WriteLine($"Territory {cell.Id}: {cell.HexIndexes.Count}/{participatingCount} participating hexes");

        Func<int?, RGBA16BitColor> colorSelector = id => id switch
        {
            null => new RGBA16BitColor(0x2800, 0x3000, 0x3800, ushort.MaxValue),
            7 => new RGBA16BitColor(0x3800, 0x7800, 0xc800, ushort.MaxValue),
            42 => new RGBA16BitColor(0x5000, 0xa000, 0x5800, ushort.MaxValue),
            99 => new RGBA16BitColor(0xc800, 0xb000, 0x7000, ushort.MaxValue),
            _ => throw new InvalidOperationException($"Unexpected territory: {id}")
        };
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName.Replace(".png", $".mask-{maskKind}.actual.png"));
        partition.Rasterize(6f, colorSelector).SaveAsPng(actualPath);
        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "Partitioning", "PerlinNoise", "Approved", fileName);
        if (!File.Exists(approvedPath) ||
            !PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)))
        {
            TestContext.AddTestAttachment(actualPath, $"Partial Perlin-noise partition: {context}");
            Assert.Fail($"Partial Perlin-noise partition snapshot changed or is missing ({context}). Actual image: {actualPath}");
        }
    }

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
        var source = new HexMapGeometry(topology, 1f).PartitionByPerlinNoise(targets, new PerlinNoisePartitionParameters(scale: 18f), seed: seed);

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
        var partition = geometry.PartitionByPerlinNoise(targets, new PerlinNoisePartitionParameters(scale: worldSpace ? 45f : 18f), seed: 42);
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
