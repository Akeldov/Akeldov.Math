using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class RasterTests
{
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(-1, 1)]
    [TestCase(1, -1)]
    [TestCase(-2, -2)]
    public void Constructor_WhenResolutionIsNotPositive_Throws(int width, int height)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Raster<int>(new VectorXYInt(width, height), Array.Empty<int>()));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }

    [Test]
    public void Constructor_WhenResolutionProductIsPositiveButComponentsAreNegative_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Raster<int>(new VectorXYInt(-2, -2), new int[4]));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }

    [Test]
    public void Constructor_WhenCellCountExceedsArrayCapacity_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Raster<int>(new VectorXYInt(50_000, 50_000), Array.Empty<int>()));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }
}
