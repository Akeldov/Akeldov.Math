using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class SpatialHexPartitionMapExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SplitDisconnectedCells_PreservesGeometryAndCreatesIndependentComponents(Layout layout)
    {
        var geometry = new HexMapGeometry(5, 1, new VectorXY(10f, -20f), 3f, layout);
        var cells = new[]
        {
            new HexPartitionCell(42, new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0), new VectorXYInt(3, 0), new VectorXYInt(4, 0) }),
            new HexPartitionCell(7, new[] { new VectorXYInt(2, 0) }),
        };
        var source = new SpatialHexPartitionMap<HexPartitionCell>(geometry, cells);

        SpatialHexPartitionMap<HexPartitionCell> result = source.SplitDisconnectedCells();

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
        Assert.That(Enumerable.Range(0, 5).Select(i => result[i]), Is.EqualTo(new[] { 0, 0, 1, 2, 2 }));
        Assert.That(result.Cells.Select(cell => cell.Id), Is.EqualTo(new[] { 0, 1, 2 }));
        Assert.That(result.Cells.Select(cell => cell.HexIndexes.Count), Is.EqualTo(new[] { 2, 1, 2 }));
        foreach (var cell in result.Cells)
        foreach (var index in cell.HexIndexes)
            Assert.That(result[index], Is.EqualTo(cell.Id));
        Assert.That(source.Cells, Is.EqualTo(cells));
        Assert.That(Enumerable.Range(0, 5).Select(i => source[i]), Is.EqualTo(new[] { 42, 42, 7, 42, 42 }));
        result[0] = 99;
        Assert.That(source[0], Is.EqualTo(42));
    }

    [Test]
    public void SplitDisconnectedCells_OnVoronoiMap_SelectsSpatialOverload()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(10f, -20f), 3f, Layout.EvenQ);
        var centers = new HexCenterMap(geometry);
        var source = centers.PartitionByVoronoi(new[] { new Site(centers[0], 1f) });

        SpatialHexPartitionMap<HexPartitionCell> result = source.SplitDisconnectedCells();

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Cells, Has.Count.EqualTo(1));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(2));
    }

    [Test]
    public void SplitDisconnectedCells_WhenNull_Throws()
    {
        SpatialHexPartitionMap<HexPartitionCell> source = null!;

        Assert.That(Assert.Throws<ArgumentNullException>(() => source.SplitDisconnectedCells())!.ParamName,
            Is.EqualTo("partition"));
    }

    [Test]
    public void SplitDisconnectedCells_WhenAssignmentsDisagreeWithCells_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var source = new SpatialHexPartitionMap<HexPartitionCell>(geometry,
            new[] { new HexPartitionCell(7, new[] { new VectorXYInt(0, 0) }) });
        source[0] = 42;

        Assert.That(Assert.Throws<ArgumentException>(() => source.SplitDisconnectedCells())!.ParamName,
            Is.EqualTo("partition"));
    }

    [Test]
    public void SplitDisconnectedCells_WithEmptyTopology_PreservesGeometry()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(10f, -20f), 3f, Layout.EvenQ);
        var source = new SpatialHexPartitionMap<HexPartitionCell>(geometry,
            new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()) });

        SpatialHexPartitionMap<HexPartitionCell> result = source.SplitDisconnectedCells();

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Cells, Is.Empty);
    }
}
