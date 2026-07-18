using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexSeptupletRasterTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_WithSubrectangle_SamplesCorrespondingPartOfFullRaster(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(12, 8, 1f, layout);
        VectorXY size = hexMapGeometry.GetBoundingBoxSize();
        var fullGrid = new IndexSeptupletRaster(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), size, new VectorXYInt(4, 4)));
        var subrectangleGrid = new IndexSeptupletRaster(
            hexMapGeometry,
            new RasterGeometry(
                new PointXY(size.X * 0.25f, size.Y * 0.25f),
                size * 0.5f,
                new VectorXYInt(2, 2)));

        AssertSeptuplet(subrectangleGrid[new VectorXYInt(0, 0)], fullGrid[new VectorXYInt(1, 1)]);
        AssertSeptuplet(subrectangleGrid[new VectorXYInt(1, 0)], fullGrid[new VectorXYInt(2, 1)]);
        AssertSeptuplet(subrectangleGrid[new VectorXYInt(0, 1)], fullGrid[new VectorXYInt(1, 2)]);
        AssertSeptuplet(subrectangleGrid[new VectorXYInt(1, 1)], fullGrid[new VectorXYInt(2, 2)]);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_UsesProvidedGeometries(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(12, 8, new VectorXY(10f, -20f), 2f, layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(8f, -22f),
            new VectorXY(16f, 12f),
            new VectorXYInt(4, 3));
        var grid = new IndexSeptupletRaster(hexMapGeometry, rasterGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(grid, Is.AssignableTo<ISpatialRaster<Septuplet<VectorXYInt>>>());
            Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
            Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
            Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
            Assert.That(grid[11], Is.EqualTo(grid[new VectorXYInt(3, 2)]));
        });
    }

    private static void AssertSeptuplet(
        Septuplet<VectorXYInt> actual,
        Septuplet<VectorXYInt> expected)
    {
        Assert.That(actual.Main, Is.EqualTo(expected.Main));
        Assert.That(actual.Adjacent0, Is.EqualTo(expected.Adjacent0));
        Assert.That(actual.Adjacent1, Is.EqualTo(expected.Adjacent1));
        Assert.That(actual.Adjacent2, Is.EqualTo(expected.Adjacent2));
        Assert.That(actual.Adjacent3, Is.EqualTo(expected.Adjacent3));
        Assert.That(actual.Adjacent4, Is.EqualTo(expected.Adjacent4));
        Assert.That(actual.Adjacent5, Is.EqualTo(expected.Adjacent5));
    }
}
