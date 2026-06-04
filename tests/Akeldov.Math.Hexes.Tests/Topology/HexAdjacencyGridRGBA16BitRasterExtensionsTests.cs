using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using HexAdjacencyGrid = Akeldov.Math.Hexes.Geometry.HexAdjacencyGrid;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexAdjacencyGridRGBA16BitRasterExtensionsTests
{
    [Test]
    public void ToRGBA16BitRaster_UsesGridGeometry()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);
        var grid = new HexAdjacencyGrid(
            map,
            VectorXY.Zero,
            2f,
            new VectorXY(-1f, -2f),
            new VectorXY(4f, 5f),
            new VectorXYInt(3, 2));

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(_ => new RGBA16BitColor(1, 2, 3, 4));

        Assert.That(raster.Grid, Is.EqualTo(new RasterGrid(
            new PointXY(-1f, -2f),
            new VectorXY(4f, 5f),
            new VectorXYInt(3, 2))));
        Assert.That(raster.Values, Has.Length.EqualTo(6));
    }

    [Test]
    public void ToRGBA16BitRaster_MapsHitCellsByIndexedAdjacency()
    {
        var map = new HexAdjacencyMap(2, 1, Layout.OddR);
        var grid = new HexAdjacencyGrid(map, VectorXY.Zero, 2f, new VectorXYInt(4, 1));
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            adjacency => adjacency.HasOwnIndex && adjacency.Index == 0 ? red : blue);

        Assert.That(raster.Values, Is.EqualTo(new[]
        {
            red,
            red,
            blue,
            blue
        }));
    }

    [Test]
    public void ToRGBA16BitRaster_MapsHitCellsByFlatHexIndex()
    {
        var map = new HexAdjacencyMap(2, 1, Layout.OddR);
        var grid = new HexAdjacencyGrid(map, VectorXY.Zero, 2f, new VectorXYInt(4, 1));
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            (hexIndex, _) => hexIndex == 0 ? red : blue);

        Assert.That(raster.Values, Is.EqualTo(new[]
        {
            red,
            red,
            blue,
            blue
        }));
    }

    [Test]
    public void ToRGBA16BitRaster_UsesEmptyColorForMissCells()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);
        var grid = new HexAdjacencyGrid(
            map,
            VectorXY.Zero,
            2f,
            new VectorXY(100f, 100f),
            VectorXY.One,
            VectorXYInt.One);
        var emptyColor = new RGBA16BitColor(1, 2, 3, 4);
        var mapperWasCalled = false;

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            _ =>
            {
                mapperWasCalled = true;
                return new RGBA16BitColor(9, 9, 9, 9);
            },
            emptyColor);

        Assert.That(mapperWasCalled, Is.False);
        Assert.That(raster.Values, Is.EqualTo(new[] { emptyColor }));
    }

    [Test]
    public void ToRGBA16BitRaster_WhenGridIsNull_Throws()
    {
        HexAdjacencyGrid grid = null!;

        Assert.Throws<ArgumentNullException>(() => grid.ToRGBA16BitRaster(_ => default));
    }

    [Test]
    public void ToRGBA16BitRaster_WhenMapperIsNull_Throws()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);
        var grid = new HexAdjacencyGrid(map, VectorXY.Zero, 2f, VectorXYInt.One);

        Assert.Throws<ArgumentNullException>(() => grid.ToRGBA16BitRaster((Func<IndexedHexAdjacency, RGBA16BitColor>)null!));
        Assert.Throws<ArgumentNullException>(() => grid.ToRGBA16BitRaster((Func<int, HexAdjacency, RGBA16BitColor>)null!));
    }
}
