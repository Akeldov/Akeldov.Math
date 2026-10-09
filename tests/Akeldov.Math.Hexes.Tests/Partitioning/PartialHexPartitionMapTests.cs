using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class PartialHexPartitionMapTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_WithSparseCells_KeepsReadOnlySnapshotAndNullGaps(Layout layout)
    {
        var first = new HexPartitionCell(42, new[] { new VectorXYInt(0, 0), new VectorXYInt(0, 0) });
        var second = new HexPartitionCell(7, new[] { new VectorXYInt(1, 1) });
        var empty = new HexPartitionCell(0, Array.Empty<VectorXYInt>());
        var cells = new List<HexPartitionCell> { first, second, empty };
        var partition = new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(2, 2, layout), cells);
        IPartialHexPartitionMap<IHexPartitionCell> commonPartition = partition;
        cells.Clear();

        Assert.That(partition.Cells, Has.Count.EqualTo(3));
        Assert.That(partition.Cells[0], Is.SameAs(first));
        Assert.That(partition.Cells[1], Is.SameAs(second));
        Assert.That(partition.Cells[2], Is.SameAs(empty));
        Assert.That(commonPartition.Cells, Is.SameAs(partition.Cells));
        Assert.Throws<NotSupportedException>(() => ((IList<HexPartitionCell>)partition.Cells).Clear());
        var expected = new int?[] { 42, null, null, 7 };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.That(partition[i], Is.EqualTo(expected[i]));
            Assert.That(commonPartition[new VectorXYInt(i % 2, i / 2)], Is.EqualTo(expected[i]));
        }
    }

    [Test]
    public void InheritedIndexers_ShareNullableAssignmentsAcrossViews()
    {
        var index = new VectorXYInt(0, 0);
        var cell = new HexPartitionCell(7, new[] { index });
        var partition = new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(2, 1, Layout.OddR), new[] { cell });
        HexMap<int?> map = partition;
        IPartialHexPartitionMap<HexPartitionCell> partitionView = partition;

        map[0] = null;
        partition[new VectorXYInt(1, 0)] = cell.Id;

        Assert.That(partitionView[index], Is.Null);
        Assert.That(map[1], Is.EqualTo(cell.Id));
        Assert.That(partitionView[1], Is.EqualTo(cell.Id));
        Assert.That(partition.Cells[0], Is.SameAs(cell));
        Assert.That(cell.HexIndexes, Is.EqualTo(new[] { index }));
    }

    [TestCase(0, 0)]
    [TestCase(2, 1)]
    public void Constructor_WithNoCells_LeavesAllHexesUnassigned(int width, int height)
    {
        var partition = new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(width, height, Layout.OddR), Array.Empty<HexPartitionCell>());

        Assert.That(partition.Cells, Is.Empty);
        for (int i = 0; i < partition.Topology.Count; i++)
            Assert.That(partition[i], Is.Null);
    }

    [Test]
    public void Constructor_WhenCellsIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new PartialHexPartitionMap<HexPartitionCell>(default, null!));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WhenCellIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new PartialHexPartitionMap<HexPartitionCell>(default, new HexPartitionCell[] { null! }));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WhenIdsAreDuplicated_Throws()
    {
        var cells = new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()), new HexPartitionCell(7, Array.Empty<VectorXYInt>()) };
        var exception = Assert.Throws<ArgumentException>(() => new PartialHexPartitionMap<HexPartitionCell>(default, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WhenDifferentCellsShareHex_Throws()
    {
        var indexes = new[] { new VectorXYInt(0, 0) };
        var cells = new[] { new HexPartitionCell(7, indexes), new HexPartitionCell(42, indexes) };
        var exception = Assert.Throws<ArgumentException>(() => new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(1, 1, Layout.OddR), cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(2, 0)]
    [TestCase(0, 2)]
    public void Constructor_WhenHexIndexIsOutsideTopology_Throws(int x, int y)
    {
        var cells = new[] { new HexPartitionCell(0, new[] { new VectorXYInt(x, y) }) };
        var exception = Assert.Throws<ArgumentException>(() => new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(2, 2, Layout.OddR), cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Constructor_WhenCustomCellIdIsNegative_Throws(int id)
    {
        var cells = new[] { new CustomCell(id, Array.Empty<VectorXYInt>()) };
        var exception = Assert.Throws<ArgumentException>(() => new PartialHexPartitionMap<CustomCell>(default, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }

    [Test]
    public void Constructor_WhenCustomCellIndexesAreNull_Throws()
    {
        var cells = new[] { new CustomCell(0, null!) };
        var exception = Assert.Throws<ArgumentException>(() => new PartialHexPartitionMap<CustomCell>(default, cells));

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
