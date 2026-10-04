using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class HexPartitionTests
{
    [Test]
    public void Constructor_WithMutableInput_KeepsReadOnlyCollectionAndOriginalCells()
    {
        var first = new HexPartitionCell(42, new[] { new VectorXYInt(0, 0) });
        var second = new HexPartitionCell(7, Array.Empty<VectorXYInt>());
        var source = new List<HexPartitionCell> { first, second };
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        IHexPartition partition = new HexPartition(topology, source);

        source[0] = second;
        source.Clear();

        Assert.That(partition.Cells, Has.Count.EqualTo(2));
        Assert.That(partition.Cells[0], Is.SameAs(first));
        Assert.That(partition[0], Is.EqualTo(first.Id));
        Assert.That(partition[new VectorXYInt(0, 0)], Is.EqualTo(first.Id));
        Assert.That(partition.Cells[1], Is.SameAs(second));

        var mutableView = (IList<IHexPartitionCell>)partition.Cells;
        Assert.That(mutableView.IsReadOnly, Is.True);
        Assert.Throws<NotSupportedException>(() => mutableView[0] = second);
        Assert.Throws<NotSupportedException>(() => mutableView.Add(second));
        Assert.Throws<NotSupportedException>(() => mutableView.Clear());
        Assert.That(partition.Cells, Has.Count.EqualTo(2));
        Assert.That(partition.Cells[0], Is.SameAs(first));
    }

    [Test]
    public void Constructor_WhenCellsIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new HexPartition(default, null!));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Constructor_WhenCollectionContainsNull_Throws(int nullIndex)
    {
        var cell = new HexPartitionCell(0, Array.Empty<VectorXYInt>());
        var cells = new IHexPartitionCell[] { cell, cell };
        cells[nullIndex] = null!;

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(default, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WithEmptyInput_CreatesEmptyPartition()
    {
        IHexPartition partition = new HexPartition(default, Array.Empty<IHexPartitionCell>());

        Assert.That(partition.Cells, Is.Empty);
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs)]
    [TestCase(EmptyCellPolicy.Exclude)]
    public void VoronoiMap_ExposesOriginalCellsAndIdsThroughPartitionContract(EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(new PointXY(100f, 0f), 1f), new Site(centers[0], 1f) };
        var partitioner = new VoronoiHexPartitioner(sites, policy);
        var map = partitioner.Partition(centers);
        IHexPartition partition = map;
        IHexMap<int> idMap = partition;
        IReadOnlyList<VoronoiCell> typedCells = map.Cells;

        Assert.That(partition.Cells, Is.SameAs(typedCells));
        Assert.That(partition.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 1 : 2));
        int assignedId = policy == EmptyCellPolicy.Exclude ? 0 : 1;
        Assert.That(partition.Topology, Is.EqualTo(centers.Topology));
        Assert.That(partition.Cells[assignedId].HexIndexes,
            Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));

        var snapshot = new HexPartition(partition.Topology, typedCells);
        Assert.That(snapshot.Cells, Is.Not.SameAs(typedCells));
        for (int i = 0; i < typedCells.Count; i++)
        {
            Assert.That(partition.Cells[i], Is.SameAs(typedCells[i]));
            Assert.That(snapshot.Cells[i], Is.SameAs(typedCells[i]));
            Assert.That(partition.Cells[i].Id, Is.EqualTo(typedCells[i].SiteIndex));
        }
        for (int i = 0; i < partition.Topology.Count; i++)
        {
            var index = new VectorXYInt(i, 0);
            Assert.That(partition[i], Is.EqualTo(assignedId));
            Assert.That(idMap[index], Is.EqualTo(assignedId));
            Assert.That(snapshot[i], Is.EqualTo(assignedId));
            Assert.That(snapshot[index], Is.EqualTo(assignedId));
            Assert.That(map[i], Is.EqualTo(idMap[i]));
        }
        Assert.Throws<IndexOutOfRangeException>(() => _ = idMap[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = idMap[partition.Topology.Count]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[new VectorXYInt(2, 0)]);
    }

    [Test]
    public void MaskedVoronoiMap_WithExcludedHexes_DoesNotImplementFullPartitionContract()
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };
        var mask = new BoolHexMap(centers.Topology, new[] { true, false });
        var map = new VoronoiHexPartitioner(sites).Partition(centers, mask);

        Assert.That(map, Is.Not.InstanceOf<IHexPartition>());
        Assert.That(map[1], Is.Null);
        Assert.Throws<ArgumentException>(() => new HexPartition(map.Topology, map.Cells));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_WithSparseIds_MapsIndexesInRowMajorOrder(Layout layout)
    {
        var topology = new HexMapTopology(2, 2, layout);
        var source = new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 1), new VectorXYInt(0, 0) };
        var cells = new[]
        {
            new HexPartitionCell(int.MaxValue, source),
            new HexPartitionCell(7, new[] { new VectorXYInt(1, 0), new VectorXYInt(0, 1) }),
            new HexPartitionCell(0, Array.Empty<VectorXYInt>())
        };
        var partition = new HexPartition(topology, cells);
        source[0] = new VectorXYInt(99, 99);
        cells[0] = cells[1];

        Assert.That(partition.Topology, Is.EqualTo(topology));
        var expected = new[] { int.MaxValue, 7, 7, int.MaxValue };
        for (int i = 0; i < topology.Count; i++)
        {
            Assert.That(partition[i], Is.EqualTo(expected[i]));
            Assert.That(partition[new VectorXYInt(i % 2, i / 2)], Is.EqualTo(expected[i]));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Constructor_WhenHexesAreUnassigned_Throws(bool noCells)
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var cells = noCells
            ? Array.Empty<HexPartitionCell>()
            : new[] { new HexPartitionCell(0, new[] { new VectorXYInt(0, 0) }) };

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(topology, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_WhenIdsAreDuplicated_Throws(bool emptyCells)
    {
        var indexes = emptyCells ? Array.Empty<VectorXYInt>() : new[] { new VectorXYInt(0, 0) };
        var cells = new[] { new HexPartitionCell(7, indexes), new HexPartitionCell(7, indexes) };
        var topology = emptyCells ? default : new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(topology, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WhenDifferentCellsShareHex_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var indexes = new[] { new VectorXYInt(0, 0) };
        var cells = new[] { new HexPartitionCell(7, indexes), new HexPartitionCell(42, indexes) };

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(topology, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(2, 0)]
    [TestCase(0, 2)]
    public void Constructor_WhenHexIndexIsOutsideTopology_Throws(int x, int y)
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var cells = new[] { new HexPartitionCell(0, new[] { new VectorXYInt(x, y) }) };

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(topology, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(2, 0)]
    [TestCase(0, 1)]
    public void Indexers_WhenIndexIsOutsideTopology_Throw(int x, int y)
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var cells = new[] { new HexPartitionCell(0, new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }) };
        IHexPartition partition = new HexPartition(topology, cells);

        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[new VectorXYInt(x, y)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = partition[topology.Count]);
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Constructor_WhenCustomCellIdIsNegative_Throws(int id)
    {
        var cells = new[] { new CustomCell(id, Array.Empty<VectorXYInt>()) };

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(default, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WhenCustomCellIndexesAreNull_Throws()
    {
        var cells = new[] { new CustomCell(0, null!) };

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(default, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    private sealed class CustomCell : IHexPartitionCell
    {
        public CustomCell(int id, IReadOnlyList<VectorXYInt> hexIndexes)
        {
            Id = id;
            HexIndexes = hexIndexes;
        }

        public int Id { get; }

        public IReadOnlyList<VectorXYInt> HexIndexes { get; }
    }
}
