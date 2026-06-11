using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Rectangles;

public class RectangleTests
{
    [Test]
    public void Constructor_StoresMinAndMaxCorners()
    {
        var rectangle = new Rectangle(
            new PointXY(3f, 1f),
            new PointXY(-1f, 5f));

        Assert.That(rectangle.Min, Is.EqualTo(new PointXY(-1f, 1f)));
        Assert.That(rectangle.Max, Is.EqualTo(new PointXY(3f, 5f)));
        Assert.That(rectangle.Size, Is.EqualTo(new VectorXY(4f, 4f)));
        Assert.That(rectangle.Center, Is.EqualTo(new PointXY(1f, 3f)));
        Assert.That(rectangle.BottomLeft, Is.EqualTo(new PointXY(-1f, 1f)));
        Assert.That(rectangle.BottomRight, Is.EqualTo(new PointXY(3f, 1f)));
        Assert.That(rectangle.TopLeft, Is.EqualTo(new PointXY(-1f, 5f)));
        Assert.That(rectangle.TopRight, Is.EqualTo(new PointXY(3f, 5f)));
    }

    [TestCase(0.5f, 0.5f, true)]
    [TestCase(0f, 0.5f, true)]
    [TestCase(1.5f, 0.5f, false)]
    [TestCase(-0.0005f, 0.5f, false)]
    public void Contains_ClassifiesPoint(float x, float y, bool expected)
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f));

        bool contains = rectangle.Contains(new PointXY(x, y));

        Assert.That(contains, Is.EqualTo(expected));
    }

    [Test]
    public void Contains_WithCustomGeometryEpsilon_IncludesNearbyPoint()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f));

        Assert.That(rectangle.Contains(new PointXY(-0.0005f, 0.5f), 0.001f), Is.True);
    }

    [Test]
    public void Contains_WhenCoordinatesAreTinyOrLarge_ClassifiesWithoutOverflowOrDefaultToleranceLeak()
    {
        const float tiny = 1e-7f;
        var tinyRectangle = new Rectangle(
            new PointXY(-tiny, -tiny),
            new PointXY(tiny, tiny));

        Assert.That(tinyRectangle.Contains(new PointXY(0f, 0f), geometryEpsilon: 0f), Is.True);
        Assert.That(tinyRectangle.Contains(new PointXY(2f * tiny, 0f), geometryEpsilon: 0f), Is.False);

        const float large = 1_000_000f;
        var largeRectangle = new Rectangle(
            new PointXY(large, large),
            new PointXY(large + 100f, large + 100f));

        Assert.That(largeRectangle.Contains(new PointXY(large + 50f, large + 50f)), Is.True);
        Assert.That(largeRectangle.Contains(new PointXY(large + 101f, large + 50f)), Is.False);
    }

    [Test]
    public void Contains_WhenPointCoordinateIsInvalid_Throws()
    {
        var rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            rectangle.Contains(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void ToContour_ReturnsClosedRectangleBoundary()
    {
        var rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        Contour contour = rectangle.ToContour();

        Assert.That(contour.Curves, Has.Count.EqualTo(4));
        Assert.That(contour.Encloses(new PointXY(1f, 0.5f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(3f, 0.5f)), Is.False);
    }

    [Test]
    public void ToRegion_ReturnsContourBasedRegion()
    {
        var rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        Region region = rectangle.ToRegion();

        Assert.That(region.Contours, Has.Count.EqualTo(1));
        Assert.That(region.Contains(new PointXY(1f, 0.5f)), Is.True);
    }

    [Test]
    public void Contours_ReturnsReadOnlySingleContourView()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        Assert.That(rectangle.Contours, Has.Count.EqualTo(1));
        Assert.That(rectangle.Contours, Is.Not.InstanceOf<IContour[]>());
    }

    [Test]
    public void Constructor_WhenSizeIsDegenerate_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Rectangle(new PointXY(0f, 0f), new PointXY(0f, 1f)));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Rectangle(new PointXY(0f, 0f), new PointXY(1f, 0f)));
    }
}
