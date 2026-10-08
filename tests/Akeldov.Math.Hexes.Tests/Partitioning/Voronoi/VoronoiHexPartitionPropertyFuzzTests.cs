using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning.Voronoi;

public class VoronoiHexPartitionPropertyFuzzTests
{
    [TestCase(137, Layout.OddR)]
    [TestCase(137, Layout.EvenR)]
    [TestCase(137, Layout.OddQ)]
    [TestCase(137, Layout.EvenQ)]
    [TestCase(2026, Layout.OddR)]
    [TestCase(2026, Layout.EvenR)]
    [TestCase(2026, Layout.OddQ)]
    [TestCase(2026, Layout.EvenQ)]
    [TestCase(81017, Layout.OddR)]
    [TestCase(81017, Layout.EvenR)]
    [TestCase(81017, Layout.OddQ)]
    [TestCase(81017, Layout.EvenQ)]
    public void ReassignExclaves_PreservesRegionsAndParticipationAndProducesConnectedCells(
        int seed, Layout layout)
    {
        var random = new Random(seed);
        for (int iteration = 0; iteration < 40; iteration++)
        {
            string context = $"Seed {seed}, iteration {iteration}, layout {layout}";
            var centers = new HexCenterMap(new HexMapGeometry(9, 7, new VectorXY(13f, -21f), 2f, layout));
            var regionValues = Enumerable.Range(0, centers.Topology.Count).Select(_ => random.Next(3) - 1).ToArray();
            var regions = new IntHexMap(centers.Topology, regionValues);
            var participationValues = Enumerable.Range(0, centers.Topology.Count).Select(_ => random.Next(4) != 0).ToArray();
            var participation = new BoolHexMap(centers.Topology, participationValues);
            var siteHexes = regionValues.Distinct().Select(region => Array.IndexOf(regionValues, region))
                .Concat(Enumerable.Range(0, 5).Select(_ => random.Next(centers.Topology.Count))).ToArray();
            var sites = siteHexes.Select(index => new Site(centers[index], random.Next(1, 21) / 4f)).ToArray();
            var map = centers.ToPartialVoronoiHexPartitionMap(sites, participation, regions,
                EmptyCellPolicy.Exclude, ExclavePolicy.ReassignToClosestCell);
            var repeated = centers.ToPartialVoronoiHexPartitionMap(sites, participation, regions,
                EmptyCellPolicy.Exclude, ExclavePolicy.ReassignToClosestCell);

            Assert.That(Enumerable.Range(0, centers.Topology.Count).Select(i => map[i]),
                Is.EqualTo(Enumerable.Range(0, centers.Topology.Count).Select(i => repeated[i])), context);
            Assert.That(map.Cells.Select(cell => cell.Site), Is.EqualTo(repeated.Cells.Select(cell => cell.Site)), context);
            Assert.That(map.Cells.Sum(cell => cell.HexIndexes.Count), Is.EqualTo(participationValues.Count(value => value)), context);
            for (int i = 0; i < centers.Topology.Count; i++)
            {
                Assert.That(map.Participates(i), Is.EqualTo(participationValues[i]), context);
                Assert.That(map[i].HasValue, Is.EqualTo(participationValues[i]), context);
                if (map[i].HasValue)
                    Assert.That(map.Cells[map[i]!.Value].HexIndexes,
                        Does.Contain(new VectorXYInt(i % 9, i / 9)), context);
            }

            foreach (VoronoiHexPartitionCell cell in map.Cells)
            {
                var remaining = new HashSet<VectorXYInt>(cell.HexIndexes);
                Assert.That(remaining, Is.Not.Empty, context);
                Assert.That(cell.HexIndexes.Select(index => regions[index]).Distinct().Count(), Is.EqualTo(1), context);
                var queue = new Queue<VectorXYInt>();
                queue.Enqueue(cell.HexIndexes[0]);
                remaining.Remove(cell.HexIndexes[0]);
                while (queue.Count > 0)
                    foreach (VectorXYInt neighbor in queue.Dequeue().GetAdjacents(layout))
                        if (remaining.Remove(neighbor))
                            queue.Enqueue(neighbor);

                Assert.That(remaining, Is.Empty, $"{context}, cell {cell.Id}");
            }
            Assert.That(regionValues, Is.EqualTo(Enumerable.Range(0, centers.Topology.Count).Select(i => regions[i])), context);
        }
    }
}
