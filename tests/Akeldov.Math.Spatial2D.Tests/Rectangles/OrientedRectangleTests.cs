using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Rectangles;

public class OrientedRectangleTests
{
    [Test]
    public void Constructor_StoresGeometryAndCorners()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Assert.That(rectangle.Center, Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(rectangle.Size, Is.EqualTo(new VectorXY(4f, 2f)));
        Assert.That(rectangle.Rotation, Is.EqualTo(MathF.PI * 0.5f));
        AssertPoint(rectangle.BottomLeft, new PointXY(1f, -2f));
        AssertPoint(rectangle.BottomRight, new PointXY(1f, 2f));
        AssertPoint(rectangle.TopLeft, new PointXY(-1f, -2f));
        AssertPoint(rectangle.TopRight, new PointXY(-1f, 2f));
    }

    [TestCase(0f, 0f, true)]
    [TestCase(0.9f, 1.9f, true)]
    [TestCase(1.1f, 0f, false)]
    [TestCase(0f, 2.1f, false)]
    public void Contains_ClassifiesPointInRotatedRectangle(float x, float y, bool expected)
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        bool contains = rectangle.Contains(new PointXY(x, y));

        Assert.That(contains, Is.EqualTo(expected));
    }

    [Test]
    public void Contains_WithCustomGeometryEpsilon_IncludesNearbyPoint()
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Assert.That(rectangle.Contains(new PointXY(1.0005f, 0f), 0.001f), Is.True);
    }

    [Test]
    public void GetCenteredLocalCoordinates_ReturnsPointRelativeToLocalAxes()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        VectorXY local = rectangle.GetCenteredLocalCoordinates(new PointXY(0f, 2f));

        Assert.That(local.X, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(local.Y, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void FromBottomLeft_UsesBottomLeftCorner()
    {
        OrientedRectangle rectangle = OrientedRectangle.FromBottomLeft(
            new PointXY(1f, 2f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        AssertPoint(rectangle.BottomLeft, new PointXY(1f, 2f));
        AssertPoint(rectangle.Center, new PointXY(0f, 4f));
    }

    [Test]
    public void ToContour_ReturnsClosedRectangleBoundary()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Contour contour = rectangle.ToContour();

        Assert.That(contour.Curves, Has.Count.EqualTo(4));
        Assert.That(contour.Encloses(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(2f, 0f)), Is.False);
    }

    [Test]
    public void ToRegion_ReturnsContourBasedRegion()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Region region = rectangle.ToRegion();

        Assert.That(region.Contours, Has.Count.EqualTo(1));
        Assert.That(region.Contains(new PointXY(0f, 0f)), Is.True);
    }

    [Test]
    public void Contours_ReturnsReadOnlySingleContourView()
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Assert.That(rectangle.Contours, Has.Count.EqualTo(1));
        Assert.That(rectangle.Contours, Is.Not.InstanceOf<IContour[]>());
        Assert.That(rectangle.Contours, Is.SameAs(rectangle.Contours));
    }

    [Test]
    public void Constructor_WhenArgumentsAreInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrientedRectangle(new PointXY(float.PositiveInfinity, 0f), new VectorXY(1f, 1f), 0f));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrientedRectangle(new PointXY(0f, 0f), new VectorXY(0f, 1f), 0f));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OrientedRectangle(new PointXY(0f, 0f), new VectorXY(1f, 1f), float.NaN));
    }

    private static void AssertPoint(PointXY actual, PointXY expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(GeometryConstants.GeometryEpsilon));
    }
}
