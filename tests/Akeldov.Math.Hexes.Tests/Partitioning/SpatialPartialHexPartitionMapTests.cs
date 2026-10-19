using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class SpatialPartialHexPartitionMapTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_PreservesGeometryAndPartialPartitionAcrossViews(Layout layout)
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(10f, -20f), 3f, layout);
        var first = new HexPartitionCell(7, new[] { new VectorXYInt(0, 0) });
        var second = new HexPartitionCell(42, new[] { new VectorXYInt(2, 0) });
        var cells = new List<HexPartitionCell> { first, second };
        var map = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry, cells);
        PartialHexPartitionMap<HexPartitionCell> partition = map;
        ISpatialHexMap<int?> spatial = map;

        cells.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(spatial.Geometry, Is.EqualTo(geometry));
            Assert.That(spatial.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(partition.Cells, Is.EqualTo(new[] { first, second }));
            Assert.That(spatial[0], Is.EqualTo(7));
            Assert.That(spatial[1], Is.Null);
            Assert.That(spatial[2], Is.EqualTo(42));
        });

        SpatialBoolHexMap participation = map.MapValues(id => id.HasValue);
        Assert.That(participation.Geometry, Is.EqualTo(geometry));
        Assert.That(participation[0], Is.True);
        Assert.That(participation[1], Is.False);
        Assert.That(participation[2], Is.True);

        partition[0] = null;
        partition[1] = 99;
        Assert.That(spatial[0], Is.Null);
        Assert.That(spatial[1], Is.EqualTo(99));
        Assert.That(partition.Cells[0].Id, Is.EqualTo(7));
        Assert.That(participation[0], Is.True);
        Assert.That(participation[1], Is.False);
    }

    [Test]
    public void Constructor_WithDefaultGeometry_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialPartialHexPartitionMap<HexPartitionCell>(default, Array.Empty<HexPartitionCell>()));

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [TestCase(0, 0)]
    [TestCase(2, 1)]
    public void Constructor_WithNoCells_LeavesAllHexesUnassigned(int width, int height)
    {
        var geometry = new HexMapGeometry(width, height, 1f, Layout.OddR);
        var map = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry, Array.Empty<HexPartitionCell>());

        Assert.That(map.Geometry, Is.EqualTo(geometry));
        Assert.That(map.Cells, Is.Empty);
        for (int i = 0; i < map.Topology.Count; i++)
            Assert.That(map[i], Is.Null);
    }

    [Test]
    public void Constructor_WithOverlappingCells_Throws()
    {
        var geometry = new HexMapGeometry(2, 1, 1f, Layout.OddR);
        var index = new VectorXYInt(0, 0);
        var cells = new[]
        {
            new HexPartitionCell(7, new[] { index }),
            new HexPartitionCell(42, new[] { index }),
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry, cells));

        Assert.That(exception!.ParamName, Is.EqualTo("cells"));
    }
}
