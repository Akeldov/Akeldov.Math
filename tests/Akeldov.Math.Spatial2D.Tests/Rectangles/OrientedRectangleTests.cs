using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
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
    public void IRegion_ExposesSignedPointDistanceProviderContract()
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Assert.That(rectangle, Is.InstanceOf<ISignedPointDistanceProvider>());
        Assert.That(rectangle, Is.InstanceOf<IPointDistanceProvider>());
    }

    [Test]
    public void Distance_WhenPointIsInside_ReturnsDistanceToNearestBoundary()
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        float distance = rectangle.Distance(new PointXY(0f, 0f));

        Assert.That(distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Distance_WhenPointIsOutside_ReturnsDistanceToNearestBoundary()
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        float distance = rectangle.Distance(new PointXY(2f, 0f));

        Assert.That(distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_ReturnsNegativeInsideAndPositiveOutside()
    {
        IRegion rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        Assert.That(rectangle.SignedDistance(new PointXY(0f, 0f)), Is.EqualTo(-1f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(rectangle.SignedDistance(new PointXY(2f, 0f)), Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
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
    public void ImplicitConversion_FromRectangle_ReturnsEquivalentZeroRotationRectangle()
    {
        var source = new Rectangle(
            new PointXY(-1f, 2f),
            new PointXY(3f, 5f));

        OrientedRectangle rectangle = source;

        Assert.That(rectangle.Center, Is.EqualTo(source.Center));
        Assert.That(rectangle.Size, Is.EqualTo(source.Size));
        Assert.That(rectangle.Rotation, Is.EqualTo(0f));
        AssertPoint(rectangle.BottomLeft, source.BottomLeft);
        AssertPoint(rectangle.TopRight, source.TopRight);
        Assert.That(rectangle.Contains(new PointXY(0f, 3f)), Is.EqualTo(source.Contains(new PointXY(0f, 3f))));
        Assert.That(rectangle.Contains(new PointXY(4f, 3f)), Is.EqualTo(source.Contains(new PointXY(4f, 3f))));
    }

    [Test]
    public void ToContour_ReturnsClosedRectangleBoundary()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        OrientedRectangleContour contour = rectangle.ToContour();

        Assert.That(contour, Is.InstanceOf<IContour>());
        Assert.That(contour, Is.Not.InstanceOf<IParameterizedContour>());
        Assert.That(contour.Length, Is.EqualTo(12f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(contour.Encloses(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(2f, 0f)), Is.False);
    }

    [Test]
    public void OrientedRectangleContour_Project_ReturnsClosestBoundaryPoint()
    {
        IContour contour = new OrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        CurveProjection projection = contour.Project(new PointXY(2f, 0f));

        AssertPoint(projection.ProjectedPoint, new PointXY(1f, 0f));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void OrientedRectangleContour_RayIntersections_WhenRayOverlapsEdge_ReturnsBoundaryPointsOnRay()
    {
        IContour contour = new OrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);
        var ray = new Ray(new PointXY(1f, 0f), MathF.PI * 0.5f);

        List<PointXY> intersections = contour.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(1f, 0f))), Is.True);
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(1f, 2f))), Is.True);
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_GetPoint_UsesDefaultRightEdgeParameterOrigin()
    {
        var contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        AssertPoint(contour.ParameterOrigin, new PointXY(0f, 2f));
        AssertPoint(contour.GetPoint(0f), new PointXY(0f, 2f));
        AssertPoint(contour.GetPoint(1f), new PointXY(-1f, 2f));
        AssertPoint(contour.GetPoint(3f), new PointXY(-1f, 0f));
        AssertPoint(contour.GetPoint(contour.Length), new PointXY(0f, 2f));
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_GetPoint_WithCustomParameterOrigin_UsesLengthCoordinateAroundBoundary()
    {
        var contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            RectangleContourParameterOrigin.BottomLeft);

        AssertPoint(contour.ParameterOrigin, new PointXY(1f, -2f));
        AssertPoint(contour.GetPoint(0f), new PointXY(1f, -2f));
        AssertPoint(contour.GetPoint(2f), new PointXY(1f, 0f));
        AssertPoint(contour.GetPoint(4f), new PointXY(1f, 2f));
        AssertPoint(contour.GetPoint(contour.Length), new PointXY(1f, -2f));
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_ProjectWithParameter_ReturnsClosestBoundaryCoordinate()
    {
        var contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        ParameterizedCurveProjection projection = contour.ProjectWithParameter(new PointXY(2f, 0f));

        AssertPoint(projection.ProjectedPoint, new PointXY(1f, 0f));
        Assert.That(projection.CurveCoordinate, Is.EqualTo(9f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_ProjectWithParameter_WithCustomParameterOrigin_ReturnsRelativeBoundaryCoordinate()
    {
        var contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            RectangleContourParameterOrigin.BottomLeft);

        ParameterizedCurveProjection projection = contour.ProjectWithParameter(new PointXY(2f, 0f));

        AssertPoint(projection.ProjectedPoint, new PointXY(1f, 0f));
        Assert.That(projection.CurveCoordinate, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_Equals_WhenParameterOriginDiffers_ReturnsFalse()
    {
        var rightEdgeOrigin = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);
        var bottomLeftOrigin = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            RectangleContourParameterOrigin.BottomLeft);

        Assert.That(rightEdgeOrigin, Is.Not.EqualTo(bottomLeftOrigin));
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_Equals_WhenExplicitParameterOriginIsDefault_ReturnsTrue()
    {
        var defaultOrigin = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);
        var explicitDefaultOrigin = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            RectangleContourParameterOrigin.RightEdgeMidpoint);
        var explicitLengthOrigin = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            12f);

        Assert.That(defaultOrigin, Is.EqualTo(explicitDefaultOrigin));
        Assert.That(defaultOrigin, Is.EqualTo(explicitLengthOrigin));
    }

    [Test]
    public void ParameterizedOrientedRectangleContourConstructor_WithParameterOriginCoordinate_UsesBoundaryCoordinate()
    {
        var contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            3f);

        AssertPoint(contour.ParameterOrigin, new PointXY(-1f, 0f));
        AssertPoint(contour.GetPoint(0f), new PointXY(-1f, 0f));
    }

    [TestCase(RectangleContourParameterOrigin.RightEdgeMidpoint, 0f, 2f)]
    [TestCase(RectangleContourParameterOrigin.TopRight, -1f, 2f)]
    [TestCase(RectangleContourParameterOrigin.TopEdgeMidpoint, -1f, 0f)]
    [TestCase(RectangleContourParameterOrigin.TopLeft, -1f, -2f)]
    [TestCase(RectangleContourParameterOrigin.LeftEdgeMidpoint, 0f, -2f)]
    [TestCase(RectangleContourParameterOrigin.BottomLeft, 1f, -2f)]
    [TestCase(RectangleContourParameterOrigin.BottomEdgeMidpoint, 1f, 0f)]
    [TestCase(RectangleContourParameterOrigin.BottomRight, 1f, 2f)]
    public void ParameterizedOrientedRectangleContourConstructor_WithNamedParameterOrigin_UsesNamedBoundaryPoint(
        RectangleContourParameterOrigin parameterOrigin,
        float expectedX,
        float expectedY)
    {
        var contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f,
            parameterOrigin);
        var expected = new PointXY(expectedX, expectedY);

        AssertPoint(contour.ParameterOrigin, expected);
        AssertPoint(contour.GetPoint(0f), expected);
    }

    [Test]
    public void ParameterizedOrientedRectangleContourConstructor_WhenNamedParameterOriginIsUnsupported_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedOrientedRectangleContour(
                new PointXY(0f, 0f),
                new VectorXY(4f, 2f),
                MathF.PI * 0.5f,
                (RectangleContourParameterOrigin)42));

        Assert.That(exception!.ParamName, Is.EqualTo("parameterOrigin"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(-0.001f)]
    [TestCase(12.001f)]
    public void ParameterizedOrientedRectangleContourConstructor_WhenParameterOriginCoordinateIsInvalid_Throws(float parameterOrigin)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedOrientedRectangleContour(
                new PointXY(0f, 0f),
                new VectorXY(4f, 2f),
                MathF.PI * 0.5f,
                parameterOrigin));

        Assert.That(exception!.ParamName, Is.EqualTo("parameterOrigin"));
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_ExplicitConversionToOrientedRectangleContour_ReturnsGeometricContour()
    {
        var source = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        OrientedRectangleContour contour = (OrientedRectangleContour)source;

        Assert.That(contour.Center, Is.EqualTo(source.Center));
        Assert.That(contour.Size, Is.EqualTo(source.Size));
        Assert.That(contour.Rotation, Is.EqualTo(source.Rotation));
        Assert.That(contour, Is.InstanceOf<IContour>());
        Assert.That(contour, Is.Not.InstanceOf<IParameterizedContour>());
    }

    [Test]
    public void ParameterizedOrientedRectangleContour_RayIntersections_WhenRayOverlapsEdge_ReturnsBoundaryPointsOnRay()
    {
        IContour contour = new ParameterizedOrientedRectangleContour(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);
        var ray = new Ray(new PointXY(1f, 0f), MathF.PI * 0.5f);

        List<PointXY> intersections = contour.GetRayIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(1f, 0f))), Is.True);
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(1f, 2f))), Is.True);
    }

    [Test]
    public void ToRegion_ReturnsContourBasedRegion()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);

        ContourBasedRegion region = rectangle.ToRegion();

        Assert.That(region.Contours, Has.Count.EqualTo(1));
        Assert.That(region.Contains(new PointXY(0f, 0f)), Is.True);
    }

    [Test]
    public void ToRegion_ReturnsReadOnlySingleContourView()
    {
        var rectangle = new OrientedRectangle(
            new PointXY(0f, 0f),
            new VectorXY(4f, 2f),
            MathF.PI * 0.5f);
        ContourBasedRegion region = rectangle.ToRegion();

        Assert.That(region.Contours, Has.Count.EqualTo(1));
        Assert.That(region.Contours, Is.Not.InstanceOf<IContour[]>());
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
