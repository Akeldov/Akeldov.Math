using Akeldov.Math.Hexes.Rectangles;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Rectangles;

public class HexOrientedRectangleTests
{
    [Test]
    public void Constructor_WhenCenterIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexOrientedRectangle(new PointXY(float.PositiveInfinity, 0f), VectorXY.One, SixfoldAngle.Deg0));

        Assert.That(exception!.ParamName, Is.EqualTo("center"));
    }

    [TestCase(0f, 1f)]
    [TestCase(1f, 0f)]
    [TestCase(float.PositiveInfinity, 1f)]
    [TestCase(1f, float.NegativeInfinity)]
    public void Constructor_WhenSizeIsInvalid_Throws(float width, float height)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexOrientedRectangle(new PointXY(0f, 0f), new VectorXY(width, height), SixfoldAngle.Deg0));

        Assert.That(exception!.ParamName, Is.EqualTo("size"));
    }

    [Test]
    public void CreateFromBottomLeftPoint_WhenBottomLeftPointIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            HexOrientedRectangle.CreateFromBottomLeftPoint(
                new PointXY(float.PositiveInfinity, 0f),
                VectorXY.One,
                SixfoldAngle.Deg0));

        Assert.That(exception!.ParamName, Is.EqualTo("bottomLeftPoint"));
    }

    [Test]
    public void CreateFromBottomLeftPoint_WhenSizeIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            HexOrientedRectangle.CreateFromBottomLeftPoint(
                new PointXY(0f, 0f),
                new VectorXY(float.PositiveInfinity, 1f),
                SixfoldAngle.Deg0));

        Assert.That(exception!.ParamName, Is.EqualTo("size"));
    }
}
