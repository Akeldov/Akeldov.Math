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
        var first = new HexPartitionCell(new[] { new VectorXYInt(0, 0) });
        var second = new HexPartitionCell(Array.Empty<VectorXYInt>());
        var source = new List<HexPartitionCell> { first, second };
        IHexPartition partition = new HexPartition(source);

        source[0] = second;
        source.Clear();

        Assert.That(partition.Cells, Has.Count.EqualTo(2));
        Assert.That(partition.Cells[0], Is.SameAs(first));
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
        var exception = Assert.Throws<ArgumentNullException>(() => new HexPartition(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Constructor_WhenCollectionContainsNull_Throws(int nullIndex)
    {
        var cell = new HexPartitionCell(Array.Empty<VectorXYInt>());
        var cells = new IHexPartitionCell[] { cell, cell };
        cells[nullIndex] = null!;

        var exception = Assert.Throws<ArgumentException>(() => new HexPartition(cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WithEmptyInput_CreatesEmptyPartition()
    {
        IHexPartition partition = new HexPartition(Array.Empty<IHexPartitionCell>());

        Assert.That(partition.Cells, Is.Empty);
    }

    [TestCase(false, EmptyCellPolicy.LeaveAsIs)]
    [TestCase(true, EmptyCellPolicy.LeaveAsIs)]
    [TestCase(false, EmptyCellPolicy.Exclude)]
    [TestCase(true, EmptyCellPolicy.Exclude)]
    public void VoronoiMaps_ExposeOriginalCellsThroughPartitionContract(bool useMask, EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f), new Site(new PointXY(100f, 0f), 1f) };
        var partitioner = new VoronoiHexPartitioner(sites, policy);
        IHexPartition partition = useMask
            ? partitioner.Partition(centers, new BoolHexMap(centers.Topology, new[] { true, false }))
            : partitioner.Partition(centers);
        IReadOnlyList<VoronoiCell> typedCells = partition switch
        {
            VoronoiHexPartitionMap map => map.Cells,
            MaskedVoronoiHexPartitionMap map => map.Cells,
            _ => throw new InvalidOperationException("Unexpected partition map type.")
        };

        Assert.That(partition.Cells, Is.SameAs(typedCells));
        Assert.That(partition.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 1 : 2));
        Assert.That(partition.Cells[0].HexIndexes,
            Is.EqualTo(useMask
                ? new[] { new VectorXYInt(0, 0) }
                : new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));

        var snapshot = new HexPartition(typedCells);
        Assert.That(snapshot.Cells, Is.Not.SameAs(typedCells));
        for (int i = 0; i < typedCells.Count; i++)
        {
            Assert.That(partition.Cells[i], Is.SameAs(typedCells[i]));
            Assert.That(snapshot.Cells[i], Is.SameAs(typedCells[i]));
        }
    }
}
