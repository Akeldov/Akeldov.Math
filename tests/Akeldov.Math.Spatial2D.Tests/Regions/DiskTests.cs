using System.Globalization;

using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Regions;

public class DiskTests
{
    [Test]
    public void Constructor_StoresCenterRadiusAndDiameter()
    {
        var disk = new Disk(new PointXY(1f, 2f), 3f);

        Assert.That(disk.Center, Is.EqualTo(new PointXY(1f, 2f)));
        Assert.That(disk.Radius, Is.EqualTo(3f));
        Assert.That(disk.Diameter, Is.EqualTo(6f));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void Constructor_WhenCenterCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Disk(new PointXY(x, y), 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("center"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenRadiusIsInvalid_Throws(float radius)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Disk(new PointXY(0f, 0f), radius));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [Test]
    public void IRegion_ExposesSignedPointDistanceProviderContract()
    {
        IRegion disk = new Disk(new PointXY(0f, 0f), 5f);

        Assert.That(disk, Is.InstanceOf<ISignedPointDistanceProvider>());
        Assert.That(disk, Is.InstanceOf<IPointDistanceProvider>());
    }

    [TestCase(0f, 0f, true)]
    [TestCase(3f, 4f, true)]
    [TestCase(5f, 0f, true)]
    [TestCase(5.001f, 0f, false)]
    public void Contains_ClassifiesPoint(float x, float y, bool expected)
    {
        IRegion disk = new Disk(new PointXY(0f, 0f), 5f);

        bool contains = disk.Contains(new PointXY(x, y));

        Assert.That(contains, Is.EqualTo(expected));
    }

    [Test]
    public void Contains_WhenPointCoordinateIsInvalid_Throws()
    {
        IRegion disk = new Disk(new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            disk.Contains(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Distance_ReturnsDistanceToBoundary()
    {
        IRegion disk = new Disk(new PointXY(0f, 0f), 5f);

        Assert.That(disk.Distance(new PointXY(3f, 0f)), Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(disk.Distance(new PointXY(7f, 0f)), Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_ReturnsNegativeInsideAndPositiveOutside()
    {
        IRegion disk = new Disk(new PointXY(0f, 0f), 5f);

        Assert.That(disk.SignedDistance(new PointXY(3f, 0f)), Is.EqualTo(-2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(disk.SignedDistance(new PointXY(7f, 0f)), Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(disk.SignedDistance(new PointXY(5f, 0f)), Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WithCustomGeometryEpsilon_WhenPointIsWithinTolerance_ReturnsNegativeDistance()
    {
        IRegion disk = new Disk(new PointXY(0f, 0f), 5f);

        float signedDistance = disk.SignedDistance(new PointXY(5.0005f, 0f), 0.001f);

        Assert.That(signedDistance, Is.EqualTo(-0.0005f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ZeroRadiusDisk_ContainsCenterAndUsesPointBoundaryDistance()
    {
        IRegion disk = new Disk(new PointXY(1f, 2f), 0f);

        Assert.That(disk.Contains(new PointXY(1f, 2f)), Is.True);
        Assert.That(disk.Contains(new PointXY(1.001f, 2f)), Is.False);
        Assert.That(disk.Distance(new PointXY(4f, 6f)), Is.EqualTo(5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ToContour_ReturnsCircleBoundary()
    {
        var disk = new Disk(new PointXY(1f, 2f), 3f);

        Circle contour = disk.ToContour();

        Assert.That(contour.Center, Is.EqualTo(disk.Center));
        Assert.That(contour.Radius, Is.EqualTo(disk.Radius));
    }

    [Test]
    public void Equality_WhenCenterAndRadiusMatch_ReturnsTrue()
    {
        var left = new Disk(new PointXY(1f, 2f), 3f);
        var right = new Disk(new PointXY(1f, 2f), 3f);

        Assert.That(left.Equals(right), Is.True);
        Assert.That(left == right, Is.True);
        Assert.That(left != right, Is.False);
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var disk = new Disk(new PointXY(1.5f, 2.25f), 3.5f);

            Assert.That(disk.ToString(), Is.EqualTo("Disk(center: (1.5, 2.25), radius: 3.5)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
