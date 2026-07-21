using Akeldov.Math.Hexes.Rectangles;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Rectangles;

public class HexOrientedRectangleTests
{
    private const float Epsilon = 0.00001f;

    [Test]
    public void Constructor_WithZeroRotation_ExposesGeometry()
    {
        var rectangle = new HexOrientedRectangle(
            new PointXY(4f, 5f),
            new VectorXY(6f, 2f),
            SixfoldAngle.Deg0);

        Assert.Multiple(() =>
        {
            Assert.That(rectangle.Center, Is.EqualTo(new PointXY(4f, 5f)));
            Assert.That(rectangle.Size, Is.EqualTo(new VectorXY(6f, 2f)));
            Assert.That(rectangle.Rotation, Is.EqualTo(SixfoldAngle.Deg0));
            Assert.That(rectangle.BottomLeft, Is.EqualTo(new PointXY(1f, 4f)));
            Assert.That(rectangle.BottomRight, Is.EqualTo(new PointXY(7f, 4f)));
            Assert.That(rectangle.TopLeft, Is.EqualTo(new PointXY(1f, 6f)));
            Assert.That(rectangle.TopRight, Is.EqualTo(new PointXY(7f, 6f)));
        });
    }

    [Test]
    public void LocalCoordinateMethods_ConvertAndClampWorldPoint()
    {
        var rectangle = new HexOrientedRectangle(
            new PointXY(4f, 5f),
            new VectorXY(6f, 2f),
            SixfoldAngle.Deg0);

        VectorXY local = rectangle.GetLocalCoordinates(new PointXY(4f, 5f));
        VectorXY normalized = rectangle.GetLocalNormalizedCoordinates(new PointXY(4f, 5f));
        VectorXY clampedLocal = rectangle.GetLocalCoordinates(new PointXY(-10f, 20f), isClamped: true);
        VectorXY clampedNormalized = rectangle.GetLocalNormalizedCoordinates(new PointXY(-10f, 20f), isClamped: true);

        Assert.Multiple(() =>
        {
            Assert.That(local, Is.EqualTo(new VectorXY(3f, 1f)));
            Assert.That(normalized, Is.EqualTo(new VectorXY(0.5f, 0.5f)));
            Assert.That(clampedLocal, Is.EqualTo(new VectorXY(0f, 2f)));
            Assert.That(clampedNormalized, Is.EqualTo(new VectorXY(0f, 1f)));
        });
    }

    [TestCase(SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg60)]
    [TestCase(SixfoldAngle.Deg120)]
    [TestCase(SixfoldAngle.Deg180)]
    [TestCase(SixfoldAngle.Deg240)]
    [TestCase(SixfoldAngle.Deg300)]
    public void CreateFromBottomLeftPoint_PreservesBottomLeft(SixfoldAngle rotation)
    {
        PointXY expected = new PointXY(2f, 3f);

        HexOrientedRectangle rectangle = HexOrientedRectangle.CreateFromBottomLeftPoint(
            expected,
            new VectorXY(6f, 2f),
            rotation);

        Assert.Multiple(() =>
        {
            Assert.That(rectangle.BottomLeft.X, Is.EqualTo(expected.X).Within(Epsilon));
            Assert.That(rectangle.BottomLeft.Y, Is.EqualTo(expected.Y).Within(Epsilon));
            Assert.That(rectangle.Size, Is.EqualTo(new VectorXY(6f, 2f)));
            Assert.That(rectangle.Rotation, Is.EqualTo(rotation));
        });
    }

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
    public void Constructor_WhenRotationIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexOrientedRectangle(new PointXY(0f, 0f), VectorXY.One, (SixfoldAngle)42));

        Assert.That(exception!.ParamName, Is.EqualTo("rotation"));
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

    [Test]
    public void CreateFromBottomLeftPoint_WhenRotationIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            HexOrientedRectangle.CreateFromBottomLeftPoint(
                new PointXY(0f, 0f),
                VectorXY.One,
                (SixfoldAngle)42));

        Assert.That(exception!.ParamName, Is.EqualTo("rotation"));
    }
}
