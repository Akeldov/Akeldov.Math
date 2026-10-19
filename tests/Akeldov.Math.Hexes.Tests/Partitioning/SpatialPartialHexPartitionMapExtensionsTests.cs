using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning;

public class SpatialPartialHexPartitionMapExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SplitDisconnectedCells_PreservesGeometryAndUnassignedGap(Layout layout)
    {
        var geometry = new HexMapGeometry(5, 1, new VectorXY(10f, -20f), 3f, layout);
        var cells = new[]
        {
            new HexPartitionCell(42, new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0), new VectorXYInt(3, 0), new VectorXYInt(4, 0) }),
        };
        var source = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry, cells);

        SpatialPartialHexPartitionMap<HexPartitionCell> result = source.SplitDisconnectedCells();

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
        Assert.That(Enumerable.Range(0, 5).Select(i => result[i]), Is.EqualTo(new int?[] { 0, 0, null, 1, 1 }));
        Assert.That(result.Cells.Select(cell => cell.Id), Is.EqualTo(new[] { 0, 1 }));
        Assert.That(result.Cells.Select(cell => cell.HexIndexes.Count), Is.EqualTo(new[] { 2, 2 }));
        foreach (var cell in result.Cells)
        foreach (var index in cell.HexIndexes)
            Assert.That(result[index], Is.EqualTo(cell.Id));
        Assert.That(source.Cells, Is.EqualTo(cells));
        Assert.That(Enumerable.Range(0, 5).Select(i => source[i]), Is.EqualTo(new int?[] { 42, 42, null, 42, 42 }));
        result[0] = 99;
        result[2] = 99;
        Assert.That(source[0], Is.EqualTo(42));
        Assert.That(source[2], Is.Null);
    }

    [Test]
    public void SplitDisconnectedCells_OnPartialVoronoiMap_SelectsSpatialOverload()
    {
        var geometry = new HexMapGeometry(3, 1, new VectorXY(10f, -20f), 3f, Layout.EvenQ);
        var centers = new HexCenterMap(geometry);
        var mask = new BoolHexMap(geometry.Topology, new[] { true, false, true });
        var source = centers.PartitionByVoronoi(mask, new[] { new Site(centers[0], 1f) });

        SpatialPartialHexPartitionMap<HexPartitionCell> result = source.SplitDisconnectedCells();

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Cells, Has.Count.EqualTo(2));
        Assert.That(result[1], Is.Null);
    }

    [Test]
    public void SplitDisconnectedCells_WhenNull_Throws()
    {
        SpatialPartialHexPartitionMap<HexPartitionCell> source = null!;

        Assert.That(Assert.Throws<ArgumentNullException>(() => source.SplitDisconnectedCells())!.ParamName,
            Is.EqualTo("partition"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SplitDisconnectedCells_WhenNullAssignmentsDisagreeWithCells_Throws(bool clearAssignedHex)
    {
        var geometry = new HexMapGeometry(2, 1, 1f, Layout.OddR);
        var source = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry,
            new[] { new HexPartitionCell(7, new[] { new VectorXYInt(0, 0) }) });
        if (clearAssignedHex) source[0] = null;
        else source[1] = 7;

        Assert.That(Assert.Throws<ArgumentException>(() => source.SplitDisconnectedCells())!.ParamName,
            Is.EqualTo("partition"));
    }

    [TestCase(0, 0)]
    [TestCase(2, 1)]
    public void SplitDisconnectedCells_WithNoAssignedHexes_PreservesGeometry(int width, int height)
    {
        var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 3f, Layout.EvenQ);
        var source = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry,
            new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()) });

        SpatialPartialHexPartitionMap<HexPartitionCell> result = source.SplitDisconnectedCells();

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Cells, Is.Empty);
        Assert.That(Enumerable.Range(0, result.Topology.Count).Select(i => result[i]), Is.All.Null);
    }
}
