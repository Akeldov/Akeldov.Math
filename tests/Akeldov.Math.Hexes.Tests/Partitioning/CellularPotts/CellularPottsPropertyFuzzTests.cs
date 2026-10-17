using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class CellularPottsPropertyFuzzTests
{
    [TestCase(42, CellularPottsConnectivity.Preserve)]
    [TestCase(173, CellularPottsConnectivity.Preserve)]
    [TestCase(42, CellularPottsConnectivity.None)]
    [TestCase(173, CellularPottsConnectivity.None)]
    public void Evolution_AtZeroTemperature_NeverIncreasesTheFullMapEnergy(int seed, CellularPottsConnectivity connectivity)
    {
        foreach (Layout layout in Enum.GetValues<Layout>())
        {
            var topology = new HexMapTopology(9, 8, layout);
            var tip = new VectorXYInt(3, 3);
            var root = tip.GetAdjacents(layout)[3];
            // Attach the tip through one neighbor, independently of offset-grid parity.
            var source = CellularPottsTests.Partition(topology,
                p => p.X < 2 || p == tip || p == root ? 7 : p.X < 6 ? 42 : 99, 7, 42, 99);
            Assert.That(tip.GetAdjacents(layout).Count(p => source[p] == 7), Is.EqualTo(1), $"seed={seed}, layout={layout}");
            var targets = source.Cells.ToDictionary(c => c.Id, c => (float)c.HexIndexes.Count);
            var parameters = new CellularPottsParameters(2, 0, 1.75f, 0.3f, targets, connectivity);
            var random = new Random(seed);
            var current = source;
            double initialEnergy = FullMapEnergy(current, parameters);
            double previousEnergy = initialEnergy;
            for (int iteration = 0; iteration < 20; iteration++)
            {
                current = current.ApplyCellularPotts(parameters, random: random);
                double energy = FullMapEnergy(current, parameters);
                string context = $"seed={seed}, layout={layout}, connectivity={connectivity}, iteration={iteration}";
                Assert.That(energy, Is.LessThanOrEqualTo(previousEnergy + 1e-9), context);
                var rebuilt = new HexPartitionMap<HexPartitionCell>(topology, current.Cells);
                Assert.That(CellularPottsTests.Values(current), Is.EqualTo(CellularPottsTests.Values(rebuilt)), context);
                previousEnergy = energy;
            }
            Assert.That(previousEnergy, Is.LessThan(initialEnergy), $"No relaxation: seed={seed}, layout={layout}, connectivity={connectivity}");
        }
    }

    [TestCase(42)]
    [TestCase(173)]
    public void Evolution_WithoutConnectivity_PreservesCoverageAndReproducibility(int seed)
    {
        foreach (Layout layout in Enum.GetValues<Layout>())
        {
            var topology = new HexMapTopology(12, 10, layout);
            var ids = new[] { 7, 42, 99, 125, 999 };
            var source = CellularPottsTests.Partition(topology, p => ids[p.X / 6 + 2 * (p.Y / 5)], ids);
            var original = CellularPottsTests.Values(source);
            var targets = source.Cells.ToDictionary(c => c.Id, c => (float)c.HexIndexes.Count);
            var parameters = new CellularPottsParameters(2, 1e6f, 1, 0.25f, targets, CellularPottsConnectivity.None);
            var random = new Random(seed);
            var current = source;
            for (int iteration = 0; iteration < 12; iteration++)
            {
                string context = $"seed={seed}, layout={layout}, iteration={iteration}";
                current = current.ApplyCellularPotts(parameters, random: random);
                // Rebuilding from retained cells independently checks complete, non-overlapping coverage.
                var rebuilt = new HexPartitionMap<HexPartitionCell>(topology, current.Cells);
                Assert.That(CellularPottsTests.Values(current), Is.EqualTo(CellularPottsTests.Values(rebuilt)), context);
                Assert.That(current.Cells.Select(c => c.Id), Is.EqualTo(ids), context);
                Assert.That(current.Cells[^1].HexIndexes, Is.Empty, context);
            }
            var replay = source.ApplyCellularPotts(
                new CellularPottsParameters(24, parameters.Temperature, parameters.BoundaryEnergy,
                    parameters.AreaPenalty, parameters.TargetAreas, parameters.Connectivity),
                random: new Random(seed));
            Assert.That(CellularPottsTests.Values(current), Is.EqualTo(CellularPottsTests.Values(replay)), $"seed={seed}, layout={layout}");
            Assert.That(CellularPottsTests.Values(source), Is.EqualTo(original), $"seed={seed}, layout={layout}");
        }
    }

    [TestCase(42)]
    [TestCase(173)]
    [TestCase(20261017)]
    public void Evolution_PreservesConnectivityAndCoverage(int seed)
    {
        foreach (Layout layout in Enum.GetValues<Layout>())
        {
            var topology = new HexMapTopology(18, 15, layout);
            var source = CellularPottsTests.Partition(topology,
                p => (p.X / 6 + 3 * (p.Y / 5)) * 7 + 11, Enumerable.Range(0, 9).Select(i => i * 7 + 11).ToArray());
            var original = CellularPottsTests.Values(source);
            var random = new Random(seed);
            var targets = source.Cells.ToDictionary(c => c.Id, c => (float)c.HexIndexes.Count);
            var parameters = new CellularPottsParameters(4, 1e6f, 1, 0.25f, targets);
            var current = source;
            for (int iteration = 0; iteration < 12; iteration++)
            {
                using (Assert.EnterMultipleScope())
                {
                    current = current.ApplyCellularPotts(parameters, random: random);
                    CellularPottsTests.AssertConsistentAndConnected(current, $"seed={seed}, layout={layout}, iteration={iteration}");
                    Assert.That(current.Cells.Select(c => c.Id), Is.EqualTo(source.Cells.Select(c => c.Id)),
                        $"seed={seed}, layout={layout}, iteration={iteration}");
                    Assert.That(current.Cells.All(c => c.HexIndexes.Count > 0), Is.True,
                        $"seed={seed}, layout={layout}, iteration={iteration}");
                }
            }
            var replay = source.ApplyCellularPotts(
                new CellularPottsParameters(48, parameters.Temperature, parameters.BoundaryEnergy,
                    parameters.AreaPenalty, parameters.TargetAreas, parameters.Connectivity),
                random: new Random(seed));
            Assert.That(CellularPottsTests.Values(current), Is.EqualTo(CellularPottsTests.Values(replay)), $"seed={seed}, layout={layout}");
            Assert.That(CellularPottsTests.Values(source), Is.EqualTo(original), $"seed={seed}, layout={layout}");
            Assert.That(CellularPottsTests.Values(current), Is.Not.EqualTo(original), $"No evolution: seed={seed}, layout={layout}");
        }
    }

    private static double FullMapEnergy(HexPartitionMap<HexPartitionCell> map, CellularPottsParameters parameters)
    {
        int width = map.Topology.Resolution.X;
        int height = map.Topology.Resolution.Y;
        int unlikeEdges = 0;
        for (int site = 0; site < map.Topology.Count; site++)
        {
            var index = new VectorXYInt(site % width, site / width);
            foreach (var neighbor in index.GetAdjacents(map.Topology.Layout))
            {
                if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= height)
                    continue;
                int next = neighbor.Y * width + neighbor.X;
                if (next > site && map[site] != map[next])
                    unlikeEdges++;
            }
        }

        double energy = (double)parameters.BoundaryEnergy * unlikeEdges;
        foreach (var cell in map.Cells)
        {
            double deviation = cell.HexIndexes.Count - (double)parameters.TargetAreas[cell.Id];
            energy += parameters.AreaPenalty * deviation * deviation;
        }
        return energy;
    }
}
