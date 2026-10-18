using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class PartialCellularPottsPropertyFuzzTests
{
    [TestCase(42, CellularPottsConnectivity.Preserve)]
    [TestCase(173, CellularPottsConnectivity.Preserve)]
    [TestCase(20261018, CellularPottsConnectivity.Preserve)]
    [TestCase(42, CellularPottsConnectivity.None)]
    [TestCase(173, CellularPottsConnectivity.None)]
    [TestCase(20261018, CellularPottsConnectivity.None)]
    public void Evolution_PreservesUnassignedHexesConnectivityAndEnergy(int seed, CellularPottsConnectivity connectivity)
    {
        var random = new Random(seed);
        foreach (Layout layout in Enum.GetValues<Layout>())
        for (int scenario = 0; scenario < 4; scenario++)
        {
            string context = $"seed={seed}, layout={layout}, connectivity={connectivity}, scenario={scenario}";
            var topology = new HexMapTopology(12, 10, layout);
            var full = CellularPottsTests.Partition(topology, p => p.X < 4 ? 7 : p.X < 8 ? 42 : 99, 7, 42, 99, 125);
            int top = random.Next(0, 2);
            var mask = new BoolHexMap(topology, Enumerable.Range(0, topology.Count)
                .Select(i => i / 12 >= top && i / 12 < 9 && (connectivity == CellularPottsConnectivity.Preserve || random.Next(5) != 0)).ToArray());
            var current = new PartialHexPartitionMap<HexPartitionCell>(topology,
                full.Cells.Select(c => new HexPartitionCell(c.Id, c.HexIndexes.Where(p => mask[p]).ToArray())).ToArray());
            var before = PartialCellularPottsTests.Values(current);
            var initialAreas = Enumerable.Range(0, topology.Count).Where(i => current[i].HasValue)
                .GroupBy(i => current[i]!.Value).ToDictionary(g => g.Key, g => g.Count());
            var parameters = new CellularPottsParameters(2, 0, 1.75f, 0.3f, connectivity: connectivity);
            var evolutionRandom = new Random(seed);
            for (int iteration = 0; iteration < 8; iteration++)
            {
                string stepContext = $"{context}, iteration={iteration}";
                // Missing targets use the areas at the start of this particular application.
                var targets = Enumerable.Range(0, topology.Count).Where(i => current[i].HasValue)
                    .GroupBy(i => current[i]!.Value).ToDictionary(g => g.Key, g => g.Count());
                double energyBefore = Energy(current, targets, parameters);
                var previous = current;
                current = current.DiffuseByCellularPotts(parameters, evolutionRandom);
                Assert.That(Energy(current, targets, parameters), Is.LessThanOrEqualTo(energyBefore + 1e-9), stepContext);
                Assert.That(PartialCellularPottsTests.Values(current).Select(id => id.HasValue), Is.EqualTo(before.Select(id => id.HasValue)), stepContext);
                var rebuilt = new PartialHexPartitionMap<HexPartitionCell>(topology, current.Cells);
                Assert.That(PartialCellularPottsTests.Values(current), Is.EqualTo(PartialCellularPottsTests.Values(rebuilt)), stepContext);
                Assert.That(current.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42, 99, 125 }), stepContext);
                foreach (int i in Enumerable.Range(0, topology.Count).Where(i => current[i].HasValue))
                    Assert.That(initialAreas.ContainsKey(current[i]!.Value), Is.True, stepContext);
                if (connectivity == CellularPottsConnectivity.Preserve)
                    AssertConnected(current, initialAreas.Keys, stepContext);
                // Replaying an application with a fixed seed must reproduce the evolution.
                if (iteration == 0)
                {
                    var replay = previous.DiffuseByCellularPotts(parameters, new Random(seed));
                    Assert.That(PartialCellularPottsTests.Values(replay), Is.EqualTo(PartialCellularPottsTests.Values(current)), stepContext);
                }
            }
            Assert.That(CellularPottsTests.Values(full), Is.EqualTo(Enumerable.Range(0, topology.Count).Select(i => i % 12 < 4 ? 7 : i % 12 < 8 ? 42 : 99)), context);
        }
    }

    private static double Energy(PartialHexPartitionMap<HexPartitionCell> map,
        Dictionary<int, int> targets, CellularPottsParameters parameters)
    {
        int width = map.Topology.Resolution.X;
        int edges = 0;
        var areas = new Dictionary<int, int>();
        for (int i = 0; i < map.Topology.Count; i++)
        {
            if (!map[i].HasValue) continue;
            int key = map[i]!.Value;
            areas.TryGetValue(key, out int area);
            areas[key] = area + 1;
            foreach (var neighbor in new VectorXYInt(i % width, i / width).GetAdjacents(map.Topology.Layout))
            {
                if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= width || neighbor.Y >= map.Topology.Resolution.Y) continue;
                int next = neighbor.Y * width + neighbor.X;
                if (next > i && map[next].HasValue && map[next] != map[i]) edges++;
            }
        }
        double energy = (double)parameters.BoundaryEnergy * edges;
        foreach (var target in targets)
        {
            areas.TryGetValue(target.Key, out int area);
            double deviation = area - target.Value;
            energy += parameters.AreaPenalty * deviation * deviation;
        }
        return energy;
    }

    private static void AssertConnected(PartialHexPartitionMap<HexPartitionCell> map,
        IEnumerable<int> initialCells, string context)
    {
        foreach (var key in initialCells)
        {
            var remaining = Enumerable.Range(0, map.Topology.Count).Where(i => map[i] == key)
                .Select(i => new VectorXYInt(i % map.Topology.Resolution.X, i / map.Topology.Resolution.X)).ToHashSet();
            Assert.That(remaining, Is.Not.Empty, $"cell={key}, {context}");
            var queue = new Queue<VectorXYInt>();
            var first = remaining.First();
            queue.Enqueue(first);
            remaining.Remove(first);
            while (queue.Count > 0)
                foreach (var neighbor in queue.Dequeue().GetAdjacents(map.Topology.Layout))
                    if (remaining.Remove(neighbor)) queue.Enqueue(neighbor);
            Assert.That(remaining, Is.Empty, $"cell={key}, {context}");
        }
    }
}
