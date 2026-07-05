using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class RasterGridTests
{
    [Test]
    public void Constructor_WhenValuesAreValid_StoresValuesAndCalculatesCellSize()
    {
        var grid = new SpatialRasterGrid(
            origin: new PointXY(1f, 2f),
            size: new VectorXY(10f, 6f),
            resolution: new VectorXYInt(5, 3));

        AssertVector(grid.Origin, 1f, 2f);
        AssertVector(grid.Size, 10f, 6f);
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(5, 3)));
        AssertVector(grid.CellSize, 2f, 2f);
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void Constructor_WhenOriginCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialRasterGrid(new PointXY(x, y), VectorXY.One, VectorXYInt.One));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [TestCase(0f, 1f)]
    [TestCase(1f, 0f)]
    [TestCase(-1f, 1f)]
    [TestCase(1f, -1f)]
    [TestCase(float.NaN, 1f)]
    [TestCase(1f, float.NaN)]
    [TestCase(float.PositiveInfinity, 1f)]
    [TestCase(1f, float.NegativeInfinity)]
    public void Constructor_WhenSizeIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialRasterGrid(new PointXY(0f, 0f), new VectorXY(x, y), VectorXYInt.One));

        Assert.That(exception!.ParamName, Is.EqualTo("size"));
    }

    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(-1, 1)]
    [TestCase(1, -1)]
    public void Constructor_WhenResolutionIsInvalid_Throws(int x, int y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialRasterGrid(new PointXY(0f, 0f), VectorXY.One, new VectorXYInt(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }

    [Test]
    public void GetCellCenter_WhenIndexIsInsideGrid_ReturnsWorldCoordinateCenter()
    {
        var grid = new SpatialRasterGrid(
            origin: new PointXY(1f, 2f),
            size: new VectorXY(10f, 6f),
            resolution: new VectorXYInt(5, 3));

        PointXY first = grid.GetCellCenter(0, 0);
        PointXY last = grid.GetCellCenter(new VectorXYInt(4, 2));

        AssertVector(first, 2f, 3f);
        AssertVector(last, 10f, 7f);
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(5, 0)]
    [TestCase(0, 3)]
    public void GetCellCenter_WhenIndexIsOutsideGrid_Throws(int x, int y)
    {
        var grid = new SpatialRasterGrid(
            origin: new PointXY(1f, 2f),
            size: new VectorXY(10f, 6f),
            resolution: new VectorXYInt(5, 3));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            grid.GetCellCenter(x, y));

        Assert.That(exception!.ParamName, Is.EqualTo("index"));
    }

    [Test]
    public void Equals_WhenOriginSizeAndResolutionAreEqual_ReturnsTrue()
    {
        var left = new SpatialRasterGrid(new PointXY(0f, 0f), new VectorXY(10f, 6f), new VectorXYInt(5, 3));
        var right = new SpatialRasterGrid(new PointXY(0f, 0f), new VectorXY(10f, 6f), new VectorXYInt(5, 3));

        Assert.That(left, Is.EqualTo(right));
        Assert.That(left == right, Is.True);
        Assert.That(left != right, Is.False);
    }

    private static void AssertVector(VectorXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
