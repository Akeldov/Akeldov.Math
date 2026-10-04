using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class PartitionTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_WithMutableInput_KeepsReadOnlySnapshot(bool useVoronoiCell)
    {
        var first = new VectorXYInt(2, -1);
        var second = new VectorXYInt(0, 3);
        var source = new List<VectorXYInt> { first, second, first };
        Partition partition = useVoronoiCell
            ? new VoronoiCell(0, new Site(new PointXY(0f, 0f), 1f), source)
            : new Partition(source);
        IPartition contract = partition;

        source[0] = new VectorXYInt(99, 99);
        source.Clear();

        Assert.That(contract.HexIndexes, Is.EqualTo(new[] { first, second, first }));
        Assert.That(contract.HexIndexes, Is.SameAs(partition.HexIndexes));
        if (partition is VoronoiCell cell)
            Assert.That(cell.HexIndexes, Is.SameAs(contract.HexIndexes));

        var mutableView = (IList<VectorXYInt>)contract.HexIndexes;
        Assert.That(mutableView.IsReadOnly, Is.True);
        Assert.Throws<NotSupportedException>(() => mutableView[0] = second);
        Assert.Throws<NotSupportedException>(() => mutableView.Add(second));
        Assert.Throws<NotSupportedException>(() => mutableView.Clear());
        Assert.That(contract.HexIndexes, Is.EqualTo(new[] { first, second, first }));
    }

    [Test]
    public void Constructor_WithArrayInput_CopiesIndexes()
    {
        var original = new VectorXYInt(1, 2);
        var source = new[] { original };
        var partition = new Partition(source);

        source[0] = new VectorXYInt(3, 4);

        Assert.That(partition.HexIndexes, Is.EqualTo(new[] { original }));
    }

    [Test]
    public void Constructor_WhenHexIndexesIsNull_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new Partition(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("hexIndexes"));
    }

    [Test]
    public void Constructor_WithEmptyInput_CreatesEmptyPartition()
    {
        IPartition partition = new Partition(Array.Empty<VectorXYInt>());

        Assert.That(partition.HexIndexes, Is.Empty);
    }
}
