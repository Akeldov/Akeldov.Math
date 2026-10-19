using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class SpatialHexPartitionMapTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_PreservesGeometryAndPartitionAcrossViews(Layout layout)
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(10f, -20f), 3f, layout);
        var first = new HexPartitionCell(7, new[] { new VectorXYInt(0, 0) });
        var second = new HexPartitionCell(42, new[] { new VectorXYInt(1, 0) });
        var cells = new List<HexPartitionCell> { first, second };
        var map = new SpatialHexPartitionMap<HexPartitionCell>(geometry, cells);
        HexPartitionMap<HexPartitionCell> partition = map;
        ISpatialIntHexMap spatial = map;

        cells.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(spatial.Geometry, Is.EqualTo(geometry));
            Assert.That(spatial.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(partition.Cells, Is.EqualTo(new[] { first, second }));
            Assert.That(spatial[0], Is.EqualTo(7));
            Assert.That(spatial[1], Is.EqualTo(42));
            Assert.That(spatial.Min, Is.EqualTo(7));
            Assert.That(spatial.Max, Is.EqualTo(42));
        });

        SpatialIntHexMap mapped = map.MapValues(id => id + 1);
        Assert.That(mapped.Geometry, Is.EqualTo(geometry));
        Assert.That(mapped[0], Is.EqualTo(8));
        Assert.That(mapped[1], Is.EqualTo(43));

        partition[0] = 99;
        Assert.That(spatial[0], Is.EqualTo(99));
        Assert.That(partition.Cells[0].Id, Is.EqualTo(7));
        Assert.That(mapped[0], Is.EqualTo(8));
    }

    [Test]
    public void Constructor_WithDefaultGeometry_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialHexPartitionMap<HexPartitionCell>(default, Array.Empty<HexPartitionCell>()));

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void Constructor_WithIncompletePartition_Throws()
    {
        var geometry = new HexMapGeometry(2, 1, 1f, Layout.OddR);
        var cells = new[] { new HexPartitionCell(7, new[] { new VectorXYInt(0, 0) }) };

        var exception = Assert.Throws<ArgumentException>(() =>
            new SpatialHexPartitionMap<HexPartitionCell>(geometry, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }
}
