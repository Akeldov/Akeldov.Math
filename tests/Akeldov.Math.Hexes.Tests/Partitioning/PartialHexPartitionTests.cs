using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class PartialHexPartitionTests
{
    [TestCase(Layout.OddR, EmptyCellPolicy.LeaveAsIs)]
    [TestCase(Layout.EvenR, EmptyCellPolicy.LeaveAsIs)]
    [TestCase(Layout.OddQ, EmptyCellPolicy.LeaveAsIs)]
    [TestCase(Layout.EvenQ, EmptyCellPolicy.LeaveAsIs)]
    [TestCase(Layout.OddR, EmptyCellPolicy.Exclude)]
    [TestCase(Layout.EvenR, EmptyCellPolicy.Exclude)]
    [TestCase(Layout.OddQ, EmptyCellPolicy.Exclude)]
    [TestCase(Layout.EvenQ, EmptyCellPolicy.Exclude)]
    public void PartialVoronoiMap_ExposesNullableIdsAndOriginalCells(Layout layout, EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 2, new VectorXY(12f, -8f), 2f, layout));
        var sites = new[]
        {
            new Site(new PointXY(10000f, 10000f), 1f),
            new Site(centers[0], 1f),
            new Site(centers[5], 1f)
        };
        var mask = new BoolHexMap(centers.Topology, new[] { true, false, true, true, false, true });
        var map = new VoronoiHexPartitioner(sites, policy).Partition(centers, mask);
        PartialHexPartitionMap<VoronoiHexPartitionCell> partitionMap = map;
        IPartialHexPartitionMap<VoronoiHexPartitionCell> partition = map;
        IPartialHexPartitionMap<IHexPartitionCell> commonPartition = partition;
        IReadOnlyList<VoronoiHexPartitionCell> typedCells = partition.Cells;
        IHexMap<int?> ids = partition;
        ISpatialHexMap<int?> spatialMap = map;

        Assert.That(partition.Cells, Is.SameAs(map.Cells));
        Assert.That(partitionMap.Cells, Is.SameAs(map.Cells));
        Assert.That(map, Is.InstanceOf<HexMap<int?>>());
        Assert.That(commonPartition.Cells, Is.SameAs(typedCells));
        Assert.That(partition.Topology, Is.EqualTo(centers.Topology));
        Assert.That(spatialMap.Geometry, Is.EqualTo(centers.Geometry));
        Assert.That(map, Is.Not.InstanceOf<IHexMap<VoronoiHexPartitionCell?>>());
        Assert.That(partition.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 2 : 3));
        Assert.That(partition[0], Is.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 1));
        Assert.That(partition[5], Is.EqualTo(policy == EmptyCellPolicy.Exclude ? 1 : 2));

        int assignedCount = 0;
        for (int i = 0; i < partition.Topology.Count; i++)
        {
            var index = new VectorXYInt(i % 3, i / 3);
            int? id = ids[i];
            Assert.That(ids[index], Is.EqualTo(id), $"Flat index {i}, layout {layout}.");
            Assert.That(commonPartition[index], Is.EqualTo(id));
            Assert.That(id.HasValue, Is.EqualTo(mask[i]));
            Assert.That(map.Participates(index), Is.EqualTo(id.HasValue));
            if (!id.HasValue)
                continue;

            assignedCount++;
            Assert.That(partition.Cells[id.Value], Is.SameAs(map.Cells[id.Value]));
            Assert.That(commonPartition.Cells[id.Value], Is.SameAs(partition.Cells[id.Value]));
            Assert.That(partition.Cells[id.Value].Id, Is.EqualTo(id.Value));
            Assert.That(partition.Cells[id.Value].HexIndexes, Does.Contain(index));
        }
        Assert.That(partition.Cells.Sum(cell => cell.HexIndexes.Count), Is.EqualTo(assignedCount));
        Assert.Throws<NotSupportedException>(() => ((IList<VoronoiHexPartitionCell>)map.Cells).Clear());
        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[partition.Topology.Count]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[new VectorXYInt(3, 0)]);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void PartialVoronoiMap_WithUniformMask_ReturnsNullOrZeroIds(bool participates)
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };
        var mask = new BoolHexMap(centers.Topology, new[] { participates, participates });
        IPartialHexPartitionMap<VoronoiHexPartitionCell> partition = centers.ToPartialVoronoiHexPartitionMap(sites, mask);

        int? expected = participates ? 0 : null;
        Assert.That(partition[0], Is.EqualTo(expected));
        Assert.That(partition[1], Is.EqualTo(expected));
        Assert.That(partition.Cells, Has.Count.EqualTo(1));
        Assert.That(partition.Cells[0].HexIndexes, Has.Count.EqualTo(participates ? 2 : 0));
    }
}
