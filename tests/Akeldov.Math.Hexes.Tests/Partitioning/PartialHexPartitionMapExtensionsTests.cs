using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class PartialHexPartitionMapExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SplitDisconnectedCells_WhenUnassignedHexesCutACellInTwo_PreservesTheGap(Layout layout)
    {
        var topology = new HexMapTopology(5, 4, layout);
        var source = CreatePartition(topology, index => index.X == 2 ? null : (int?)42);
        var originalValues = Enumerable.Range(0, topology.Count).Select(i => source[i]).ToArray();
        var originalCells = source.Cells.ToArray();

        var result = source.SplitDisconnectedCells();

        Assert.That(result.Topology, Is.EqualTo(topology));
        Assert.That(result.Cells.Select(cell => cell.Id), Is.EqualTo(new[] { 0, 1 }));
        Assert.That(result.Cells.Select(cell => cell.HexIndexes.Count), Is.EqualTo(new[] { 8, 8 }));
        for (int y = 0; y < topology.Resolution.Y; y++)
        for (int x = 0; x < topology.Resolution.X; x++)
            Assert.That(result[new VectorXYInt(x, y)], Is.EqualTo(x < 2 ? (int?)0 : x == 2 ? null : 1));

        Assert.That(Enumerable.Range(0, topology.Count).Select(i => source[i]), Is.EqualTo(originalValues));
        Assert.That(source.Cells, Is.EqualTo(originalCells));
        AssertConsistent(result);
        result[0] = 99;
        Assert.That(source[0], Is.EqualTo(42));
    }

    [TestCase(Layout.OddR, 2)]
    [TestCase(Layout.OddR, 3)]
    [TestCase(Layout.EvenR, 2)]
    [TestCase(Layout.EvenR, 3)]
    [TestCase(Layout.OddQ, 2)]
    [TestCase(Layout.OddQ, 3)]
    [TestCase(Layout.EvenQ, 2)]
    [TestCase(Layout.EvenQ, 3)]
    public void SplitDisconnectedCells_UsesAllSixNeighborsAtBothParities(Layout layout, int axis)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var center = new VectorXYInt(axis, axis);
        var neighbors = center.GetAdjacents(layout);
        foreach (var neighbor in neighbors)
        {
            var source = CreatePartition(topology, index => index == center || index == neighbor ? (int?)42 : null);

            var result = source.SplitDisconnectedCells();

            Assert.That(result.Cells, Has.Count.EqualTo(1), $"Neighbor: {neighbor}");
            Assert.That(result[center], Is.EqualTo(result[neighbor]), $"Neighbor: {neighbor}");
            Assert.That(result.Cells[result[center]!.Value].HexIndexes, Is.EquivalentTo(new[] { center, neighbor }));
            AssertConsistent(result);
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SplitDisconnectedCells_WithSeveralIslands_SplitsEverySourceCell(Layout layout)
    {
        var source = CreatePartition(new HexMapTopology(5, 1, layout), index => index.X % 2 == 0 ? int.MaxValue : 7);

        var result = source.SplitDisconnectedCells();

        Assert.That(result.Cells, Has.Count.EqualTo(5));
        Assert.That(Enumerable.Range(0, 5).Select(i => result[i]), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        AssertConsistent(result);
    }

    [Test]
    public void SplitDisconnectedCells_WithEmptyCellsAndDuplicateIndexes_OmitsEmptyCellsAndDuplicates()
    {
        var first = new VectorXYInt(0, 0);
        var second = new VectorXYInt(1, 0);
        var source = new PartialHexPartitionMap<HexPartitionCell>(new HexMapTopology(2, 1, Layout.OddR), new[]
        {
            new HexPartitionCell(7, Array.Empty<VectorXYInt>()),
            new HexPartitionCell(42, new[] { second, first, first })
        });

        var result = source.SplitDisconnectedCells();

        Assert.That(result.Cells, Has.Count.EqualTo(1));
        Assert.That(result.Cells[0].HexIndexes, Is.EqualTo(new[] { first, second }));
        Assert.That(result.Cells[0], Is.Not.SameAs(source.Cells[1]));
        AssertConsistent(result);
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(3, 0)]
    [TestCase(4, 3)]
    public void SplitDisconnectedCells_WithNoAssignedHexes_ReturnsEmptyPartition(int width, int height)
    {
        var topology = new HexMapTopology(width, height, Layout.OddR);
        var source = new PartialHexPartitionMap<HexPartitionCell>(topology,
            new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()) });

        var result = source.SplitDisconnectedCells();

        Assert.That(result.Topology, Is.EqualTo(topology));
        Assert.That(result.Cells, Is.Empty);
        Assert.That(Enumerable.Range(0, topology.Count).Select(i => result[i]), Is.All.Null);
    }

    [Test]
    public void SplitDisconnectedCells_WhenNull_Throws()
    {
        PartialHexPartitionMap<HexPartitionCell> source = null!;

        var exception = Assert.Throws<ArgumentNullException>(() => source.SplitDisconnectedCells());

        Assert.That(exception!.ParamName, Is.EqualTo("partition"));
    }

    [Test]
    public void SplitDisconnectedCells_WhenAssignmentsDisagreeWithCells_Throws()
    {
        var source = CreatePartition(new HexMapTopology(2, 1, Layout.OddR), index => index.X);
        source[0] = 1;

        var exception = Assert.Throws<ArgumentException>(() => source.SplitDisconnectedCells());

        Assert.That(exception!.ParamName, Is.EqualTo("partition"));
    }

    [Test]
    public void SplitDisconnectedCells_WhenCustomCellIndexesChange_RevalidatesCells()
    {
        var indexes = new List<VectorXYInt> { new(0, 0) };
        var source = new PartialHexPartitionMap<IHexPartitionCell>(new HexMapTopology(1, 1, Layout.OddR),
            new IHexPartitionCell[] { new MutableCell(indexes) });
        indexes.Clear();

        Assert.Throws<ArgumentException>(() => source.SplitDisconnectedCells());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SplitDisconnectedCells_WhenNullAssignmentsDisagreeWithCells_Throws(bool clearAssignedHex)
    {
        var source = CreatePartition(new HexMapTopology(2, 1, Layout.OddR), index => index.X == 0 ? (int?)7 : null);
        if (clearAssignedHex) source[0] = null;
        else source[1] = 7;

        Assert.That(Assert.Throws<ArgumentException>(() => source.SplitDisconnectedCells())!.ParamName, Is.EqualTo("partition"));
    }

    [Test]
    public void SplitDisconnectedCells_PreservesZeroIdentifiersAsAssignedAndDoesNotWrap()
    {
        var source = CreatePartition(new HexMapTopology(4, 2, Layout.OddR),
            index => index == new VectorXYInt(3, 0) || index == new VectorXYInt(0, 1) ? (int?)0 : null);

        var result = source.SplitDisconnectedCells();

        Assert.That(result.Cells, Has.Count.EqualTo(2));
        Assert.That(Enumerable.Range(0, 8).Select(i => result[i]),
            Is.EqualTo(new int?[] { null, null, null, 0, 1, null, null, null }));
        AssertConsistent(result);
    }

    private static PartialHexPartitionMap<HexPartitionCell> CreatePartition(HexMapTopology topology, Func<VectorXYInt, int?> getId)
    {
        var cells = Enumerable.Range(0, topology.Count)
            .Select(i => new VectorXYInt(i % topology.Resolution.X, i / topology.Resolution.X))
            .Where(index => getId(index).HasValue)
            .GroupBy(index => getId(index)!.Value)
            .Select(group => new HexPartitionCell(group.Key, group.Reverse().ToArray()))
            .Reverse()
            .ToArray();
        return new PartialHexPartitionMap<HexPartitionCell>(topology, cells);
    }

    private static void AssertConsistent(PartialHexPartitionMap<HexPartitionCell> result)
    {
        var indexes = result.Cells.SelectMany(cell => cell.HexIndexes).ToArray();
        int assignedCount = Enumerable.Range(0, result.Topology.Count).Count(i => result[i].HasValue);
        Assert.That(indexes, Has.Length.EqualTo(assignedCount));
        Assert.That(indexes.Distinct().Count(), Is.EqualTo(assignedCount));
        foreach (var cell in result.Cells)
        foreach (var index in cell.HexIndexes)
            Assert.That(result[index], Is.EqualTo(cell.Id));
    }

    private sealed class MutableCell : IHexPartitionCell
    {
        public MutableCell(IReadOnlyList<VectorXYInt> indexes) => HexIndexes = indexes;
        public int Id => 7;
        public IReadOnlyList<VectorXYInt> HexIndexes { get; }
    }
}
