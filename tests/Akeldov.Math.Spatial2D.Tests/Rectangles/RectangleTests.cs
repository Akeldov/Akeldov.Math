using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
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
    public void IRegion_ExposesSignedPointDistanceProviderContract()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(4f, 2f));

        Assert.That(rectangle, Is.InstanceOf<ISignedPointDistanceProvider>());
        Assert.That(rectangle, Is.InstanceOf<IPointDistanceProvider>());
    }

    [Test]
    public void Distance_WhenPointIsInside_ReturnsDistanceToNearestBoundary()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(4f, 2f));

        float distance = rectangle.Distance(new PointXY(2f, 1f));

        Assert.That(distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Distance_WhenPointIsOutsideCorner_ReturnsDistanceToNearestCorner()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(4f, 2f));

        float distance = rectangle.Distance(new PointXY(5f, 4f));

        Assert.That(distance, Is.EqualTo(MathF.Sqrt(5f)).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_ReturnsNegativeInsideAndPositiveOutside()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(4f, 2f));

        Assert.That(rectangle.SignedDistance(new PointXY(2f, 1f)), Is.EqualTo(-1f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(rectangle.SignedDistance(new PointXY(5f, 1f)), Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WhenPointIsImmediatelyOutside_ReturnsPositiveDistance()
    {
        IRegion rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(4f, 2f));

        float signedDistance = rectangle.SignedDistance(new PointXY(-0.0005f, 1f));

        Assert.That(signedDistance, Is.EqualTo(0.0005f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Contains_WhenCoordinatesAreTinyOrLarge_ClassifiesWithoutOverflowOrDefaultToleranceLeak()
    {
        const float tiny = 1e-7f;
        var tinyRectangle = new Rectangle(
            new PointXY(-tiny, -tiny),
            new PointXY(tiny, tiny));

        Assert.That(tinyRectangle.Contains(new PointXY(0f, 0f)), Is.True);
        Assert.That(tinyRectangle.Contains(new PointXY(2f * tiny, 0f)), Is.False);

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

        RectangleContour contour = rectangle.ToContour();

        Assert.That(contour, Is.InstanceOf<IContour>());
        Assert.That(contour, Is.Not.InstanceOf<IParameterizedContour>());
        Assert.That(contour.Length, Is.EqualTo(6f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(contour.Encloses(new PointXY(1f, 0.5f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(3f, 0.5f)), Is.False);
    }

    [Test]
    public void RectangleContour_Project_ReturnsClosestBoundaryPoint()
    {
        IContour contour = new RectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        CurveProjection projection = contour.Project(new PointXY(3f, 0.5f));

        Assert.That(projection.ProjectedPoint.AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void RectangleContour_PointIntersections_WhenRayOverlapsEdge_ReturnsEmpty()
    {
        var contour = new RectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var ray = new Ray(new PointXY(1f, 0f));

        List<PointXY> intersections = contour.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ParameterizedRectangleContour_GetPoint_UsesDefaultRightEdgeParameterOrigin()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        Assert.That(contour.ParameterOrigin, Is.EqualTo(new PointXY(2f, 0.5f)));
        Assert.That(contour.GetPoint(0f).AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(contour.GetPoint(0.5f).AlmostEquals(new PointXY(2f, 1f)), Is.True);
        Assert.That(contour.GetPoint(2.5f).AlmostEquals(new PointXY(0f, 1f)), Is.True);
        Assert.That(contour.GetPoint(contour.Length).AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
    }

    [Test]
    public void ParameterizedRectangleContour_GetPoint_WhenDirectionIsClockwise_UsesReversedTraversal()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            ContourDirection.Clockwise);

        Assert.That(contour.ContourDirection, Is.EqualTo(ContourDirection.Clockwise));
        Assert.That(contour.ParameterOrigin, Is.EqualTo(new PointXY(2f, 0.5f)));
        Assert.That(contour.GetPoint(0f).AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(contour.GetPoint(0.5f).AlmostEquals(new PointXY(2f, 0f)), Is.True);
        Assert.That(contour.GetPoint(2.5f).AlmostEquals(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.GetPoint(contour.Length).AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
    }

    [Test]
    public void ParameterizedRectangleContour_GetPoint_WithCustomParameterOrigin_UsesLengthCoordinateAroundBoundary()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            RectangleContourParameterOrigin.BottomLeft);

        Assert.That(contour.ParameterOrigin, Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(contour.GetPoint(0f).AlmostEquals(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.GetPoint(1.5f).AlmostEquals(new PointXY(1.5f, 0f)), Is.True);
        Assert.That(contour.GetPoint(2.5f).AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(contour.GetPoint(contour.Length).AlmostEquals(new PointXY(0f, 0f)), Is.True);
    }

    [Test]
    public void ParameterizedRectangleContour_GetPoint_WithCustomParameterOriginAndClockwiseDirection_UsesReversedTraversal()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            RectangleContourParameterOrigin.BottomLeft,
            ContourDirection.Clockwise);

        Assert.That(contour.ParameterOrigin, Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(contour.GetPoint(0f).AlmostEquals(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.GetPoint(0.5f).AlmostEquals(new PointXY(0f, 0.5f)), Is.True);
        Assert.That(contour.GetPoint(contour.Length).AlmostEquals(new PointXY(0f, 0f)), Is.True);
    }

    [Test]
    public void ParameterizedRectangleContour_ProjectWithParameter_ReturnsClosestBoundaryCoordinate()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        ParameterizedCurveProjection projection = contour.ProjectWithParameter(new PointXY(3f, 0.5f));

        Assert.That(projection.ProjectedPoint.AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedRectangleContour_ProjectWithParameter_WhenDirectionIsClockwise_ReturnsRelativeBoundaryCoordinate()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            ContourDirection.Clockwise);

        ParameterizedCurveProjection projection = contour.ProjectWithParameter(new PointXY(1f, -1f));

        Assert.That(projection.ProjectedPoint.AlmostEquals(new PointXY(1f, 0f)), Is.True);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(1.5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedRectangleContour_ProjectWithParameter_WithCustomParameterOrigin_ReturnsRelativeBoundaryCoordinate()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            RectangleContourParameterOrigin.BottomLeft);

        ParameterizedCurveProjection projection = contour.ProjectWithParameter(new PointXY(3f, 0.5f));

        Assert.That(projection.ProjectedPoint.AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(2.5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedRectangleContour_Equals_WhenParameterOriginDiffers_ReturnsFalse()
    {
        var rightEdgeOrigin = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var bottomLeftOrigin = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            RectangleContourParameterOrigin.BottomLeft);

        Assert.That(rightEdgeOrigin, Is.Not.EqualTo(bottomLeftOrigin));
    }

    [Test]
    public void ParameterizedRectangleContour_Equals_WhenContourDirectionDiffers_ReturnsFalse()
    {
        var counterclockwise = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var clockwise = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            ContourDirection.Clockwise);

        Assert.That(counterclockwise, Is.Not.EqualTo(clockwise));
    }

    [Test]
    public void ParameterizedRectangleContour_Equals_WhenExplicitParameterOriginIsDefault_ReturnsTrue()
    {
        var defaultOrigin = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var explicitDefaultOrigin = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            RectangleContourParameterOrigin.RightEdgeMidpoint);
        var explicitLengthOrigin = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            6f);

        Assert.That(defaultOrigin, Is.EqualTo(explicitDefaultOrigin));
        Assert.That(defaultOrigin, Is.EqualTo(explicitLengthOrigin));
    }

    [Test]
    public void ParameterizedRectangleContourConstructor_WithParameterOriginCoordinate_UsesBoundaryCoordinate()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            1.5f);

        var expected = new PointXY(1f, 1f);
        Assert.That(contour.ParameterOrigin.AlmostEquals(expected), Is.True);
        Assert.That(contour.GetPoint(0f).AlmostEquals(expected), Is.True);
    }

    [TestCase(RectangleContourParameterOrigin.RightEdgeMidpoint, 2f, 0.5f)]
    [TestCase(RectangleContourParameterOrigin.TopRight, 2f, 1f)]
    [TestCase(RectangleContourParameterOrigin.TopEdgeMidpoint, 1f, 1f)]
    [TestCase(RectangleContourParameterOrigin.TopLeft, 0f, 1f)]
    [TestCase(RectangleContourParameterOrigin.LeftEdgeMidpoint, 0f, 0.5f)]
    [TestCase(RectangleContourParameterOrigin.BottomLeft, 0f, 0f)]
    [TestCase(RectangleContourParameterOrigin.BottomEdgeMidpoint, 1f, 0f)]
    [TestCase(RectangleContourParameterOrigin.BottomRight, 2f, 0f)]
    public void ParameterizedRectangleContourConstructor_WithNamedParameterOrigin_UsesNamedBoundaryPoint(
        RectangleContourParameterOrigin parameterOrigin,
        float expectedX,
        float expectedY)
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f),
            parameterOrigin);
        var expected = new PointXY(expectedX, expectedY);

        Assert.That(contour.ParameterOrigin.AlmostEquals(expected), Is.True);
        Assert.That(contour.GetPoint(0f).AlmostEquals(expected), Is.True);
    }

    [Test]
    public void ParameterizedRectangleContourConstructor_WhenNamedParameterOriginIsUnsupported_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedRectangleContour(
                new PointXY(0f, 0f),
                new PointXY(2f, 1f),
                (RectangleContourParameterOrigin)42));

        Assert.That(exception!.ParamName, Is.EqualTo("parameterOrigin"));
    }

    [Test]
    public void ParameterizedRectangleContourConstructor_WhenContourDirectionIsUnsupported_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedRectangleContour(
                new PointXY(0f, 0f),
                new PointXY(2f, 1f),
                (ContourDirection)42));

        Assert.That(exception!.ParamName, Is.EqualTo("contourDirection"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(-0.001f)]
    [TestCase(6.001f)]
    public void ParameterizedRectangleContourConstructor_WhenParameterOriginCoordinateIsInvalid_Throws(float parameterOrigin)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedRectangleContour(
                new PointXY(0f, 0f),
                new PointXY(2f, 1f),
                parameterOrigin));

        Assert.That(exception!.ParamName, Is.EqualTo("parameterOrigin"));
    }

    [Test]
    public void ParameterizedRectangleContour_ExplicitConversionToRectangleContour_ReturnsGeometricContour()
    {
        var source = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        RectangleContour contour = (RectangleContour)source;

        Assert.That(contour.Min, Is.EqualTo(source.Min));
        Assert.That(contour.Max, Is.EqualTo(source.Max));
        Assert.That(contour, Is.InstanceOf<IContour>());
        Assert.That(contour, Is.Not.InstanceOf<IParameterizedContour>());
    }

    [Test]
    public void ParameterizedRectangleContour_RayIntersections_ReturnBoundaryIntersections()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var ray = new Ray(new PointXY(-1f, 0.5f));

        List<PointXY> intersections = contour.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(0f, 0.5f))), Is.True);
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(2f, 0.5f))), Is.True);
    }

    [Test]
    public void ParameterizedRectangleContour_PointIntersections_WhenRayOverlapsEdge_ReturnsEmpty()
    {
        var contour = new ParameterizedRectangleContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var ray = new Ray(new PointXY(1f, 0f));

        List<PointXY> intersections = contour.GetPointIntersections(ray);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void ToRegion_ReturnsContourBasedRegion()
    {
        var rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));

        ContourBasedRegion region = rectangle.ToRegion();

        Assert.That(region.Contours, Has.Count.EqualTo(1));
        Assert.That(region.Contains(new PointXY(1f, 0.5f)), Is.True);
    }

    [TestCase(2f, 0.5f)]
    [TestCase(2f, 1f)]
    public void ToRegion_PreservesContainsForBoundaryPoint(float x, float y)
    {
        var rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        var boundaryPoint = new PointXY(x, y);

        bool rectangleContains = rectangle.Contains(boundaryPoint);
        bool regionContains = rectangle.ToRegion().Contains(boundaryPoint);

        Assert.That(rectangleContains, Is.True);
        Assert.That(regionContains, Is.EqualTo(rectangleContains));
    }

    [Test]
    public void ToRegion_ReturnsReadOnlySingleContourView()
    {
        var rectangle = new Rectangle(
            new PointXY(0f, 0f),
            new PointXY(2f, 1f));
        ContourBasedRegion region = rectangle.ToRegion();

        Assert.That(region.Contours, Has.Count.EqualTo(1));
        Assert.That(region.Contours, Is.Not.InstanceOf<IContour[]>());
    }

    [Test]
    public void Constructors_WhenRectangleIsDegenerate_CreateSegmentOrPoint()
    {
        var verticalRectangle = new Rectangle(
            new PointXY(2f, 3f),
            new PointXY(2f, -1f));
        var horizontalContour = new RectangleContour(
            new PointXY(3f, 4f),
            new PointXY(-1f, 4f));
        var pointContour = new ParameterizedRectangleContour(
            new PointXY(5f, 6f),
            new PointXY(5f, 6f));

        Assert.That(verticalRectangle.Min, Is.EqualTo(new PointXY(2f, -1f)));
        Assert.That(verticalRectangle.Max, Is.EqualTo(new PointXY(2f, 3f)));
        Assert.That(verticalRectangle.Size, Is.EqualTo(new VectorXY(0f, 4f)));
        Assert.That(horizontalContour.Min, Is.EqualTo(new PointXY(-1f, 4f)));
        Assert.That(horizontalContour.Max, Is.EqualTo(new PointXY(3f, 4f)));
        Assert.That(horizontalContour.Length, Is.EqualTo(8f));
        Assert.That(pointContour.Min, Is.EqualTo(new PointXY(5f, 6f)));
        Assert.That(pointContour.Max, Is.EqualTo(new PointXY(5f, 6f)));
        Assert.That(pointContour.Length, Is.Zero);
    }

    [Test]
    public void DefaultValues_AreEquivalentToExplicitPointAtOrigin()
    {
        var rectangle = new Rectangle(default, default);
        var contour = new RectangleContour(default, default);
        var parameterizedContour = new ParameterizedRectangleContour(default, default);

        Assert.That(default(Rectangle), Is.EqualTo(rectangle));
        Assert.That(default(RectangleContour), Is.EqualTo(contour));
        Assert.That(default(ParameterizedRectangleContour), Is.EqualTo(parameterizedContour));
        Assert.That(default(Rectangle).Contains(default), Is.True);
        Assert.That(default(RectangleContour).Encloses(default), Is.True);
        Assert.That(default(ParameterizedRectangleContour).ParameterOrigin, Is.EqualTo(default(PointXY)));
        Assert.That(default(ParameterizedRectangleContour).GetPoint(0f), Is.EqualTo(default(PointXY)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(ParameterizedRectangleContour).GetPoint(float.Epsilon));
    }

    [Test]
    public void Constructors_WhenCornerIsNotFinite_Throw()
    {
        var invalidCorner = new PointXY(float.PositiveInfinity, 0f);

        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(default, invalidCorner))!.ParamName,
            Is.EqualTo("cornerB"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleContour(default, invalidCorner))!.ParamName,
            Is.EqualTo("cornerB"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedRectangleContour(default, invalidCorner))!.ParamName,
            Is.EqualTo("cornerB"));
    }

    [Test]
    public void Constructors_WhenSizeComponentOverflows_Throw()
    {
        var min = new PointXY(float.MinValue, 0f);
        var max = new PointXY(float.MaxValue, 1f);

        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(min, max))!.ParamName,
            Is.EqualTo("cornerB"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() => new RectangleContour(min, max))!.ParamName,
            Is.EqualTo("cornerB"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterizedRectangleContour(min, max))!.ParamName,
            Is.EqualTo("cornerB"));
    }

    [Test]
    public void Rectangle_WhenDegenerate_ClassifiesPointAndSegmentGeometry()
    {
        var segment = new Rectangle(
            new PointXY(2f, -1f),
            new PointXY(2f, 3f));
        var point = new Rectangle(
            new PointXY(1f, 2f),
            new PointXY(1f, 2f));

        Assert.That(segment.Contains(new PointXY(2f, 1f)), Is.True);
        Assert.That(segment.Contains(new PointXY(2.5f, 1f)), Is.False);
        Assert.That(segment.Distance(new PointXY(2f, 1f)), Is.Zero);
        Assert.That(segment.Distance(new PointXY(5f, 1f)), Is.EqualTo(3f));
        Assert.That(segment.SignedDistance(new PointXY(2f, 1f)), Is.Zero);
        Assert.That(segment.SignedDistance(new PointXY(5f, 1f)), Is.EqualTo(3f));
        Assert.That(point.Contains(new PointXY(1f, 2f)), Is.True);
        Assert.That(point.Contains(new PointXY(1f, 3f)), Is.False);
        Assert.That(point.Distance(new PointXY(4f, 6f)), Is.EqualTo(5f));
        Assert.That(point.SignedDistance(new PointXY(4f, 6f)), Is.EqualTo(5f));
    }

    [Test]
    public void RectangleContour_WhenDegenerate_ProjectsAndMeasuresBoundary()
    {
        var segment = new RectangleContour(
            new PointXY(2f, -1f),
            new PointXY(2f, 3f));
        var point = new RectangleContour(
            new PointXY(1f, 2f),
            new PointXY(1f, 2f));

        CurveProjection segmentProjection = segment.Project(new PointXY(5f, 1f));
        CurveProjection pointProjection = point.Project(new PointXY(4f, 6f));

        Assert.That(segment.Length, Is.EqualTo(8f));
        Assert.That(segment.Encloses(new PointXY(2f, 1f)), Is.True);
        Assert.That(segment.Encloses(new PointXY(2.5f, 1f)), Is.False);
        Assert.That(segmentProjection.ProjectedPoint, Is.EqualTo(new PointXY(2f, 1f)));
        Assert.That(segmentProjection.Distance, Is.EqualTo(3f));
        Assert.That(segment.SignedDistance(new PointXY(2f, 1f)), Is.Zero);
        Assert.That(point.Length, Is.Zero);
        Assert.That(pointProjection.ProjectedPoint, Is.EqualTo(new PointXY(1f, 2f)));
        Assert.That(pointProjection.Distance, Is.EqualTo(5f));
        Assert.That(point.SignedDistance(new PointXY(4f, 6f)), Is.EqualTo(5f));
    }

    [Test]
    public void ParameterizedRectangleContour_WhenSegment_TraversesBothDirections()
    {
        var verticalContour = new ParameterizedRectangleContour(
            new PointXY(2f, -1f),
            new PointXY(2f, 3f));
        var horizontalContour = new ParameterizedRectangleContour(
            new PointXY(-1f, 2f),
            new PointXY(3f, 2f));

        ParameterizedCurveProjection projection = verticalContour.ProjectWithParameter(new PointXY(5f, 2f));

        Assert.That(verticalContour.Length, Is.EqualTo(8f));
        Assert.That(verticalContour.ParameterOrigin, Is.EqualTo(new PointXY(2f, 1f)));
        Assert.That(verticalContour.GetPoint(0f), Is.EqualTo(new PointXY(2f, 1f)));
        Assert.That(verticalContour.GetPoint(2f), Is.EqualTo(new PointXY(2f, 3f)));
        Assert.That(verticalContour.GetPoint(4f), Is.EqualTo(new PointXY(2f, 1f)));
        Assert.That(verticalContour.GetPoint(6f), Is.EqualTo(new PointXY(2f, -1f)));
        Assert.That(verticalContour.GetPoint(8f), Is.EqualTo(new PointXY(2f, 1f)));
        Assert.That(projection.ProjectedPoint, Is.EqualTo(new PointXY(2f, 2f)));
        Assert.That(projection.CurveCoordinate, Is.EqualTo(1f));
        Assert.That(projection.Distance, Is.EqualTo(3f));
        Assert.That(verticalContour.SignedDistance(new PointXY(2f, 2f)), Is.Zero);
        Assert.That(horizontalContour.Length, Is.EqualTo(8f));
        Assert.That(horizontalContour.ParameterOrigin, Is.EqualTo(new PointXY(3f, 2f)));
        Assert.That(horizontalContour.GetPoint(0f), Is.EqualTo(new PointXY(3f, 2f)));
        Assert.That(horizontalContour.GetPoint(2f), Is.EqualTo(new PointXY(1f, 2f)));
        Assert.That(horizontalContour.GetPoint(4f), Is.EqualTo(new PointXY(-1f, 2f)));
        Assert.That(horizontalContour.GetPoint(6f), Is.EqualTo(new PointXY(1f, 2f)));
        Assert.That(horizontalContour.GetPoint(8f), Is.EqualTo(new PointXY(3f, 2f)));
    }

    [Test]
    public void ParameterizedRectangleContour_WithZeroHeight_CanonicalizesBottomRightParameterOrigin()
    {
        var namedOrigin = new ParameterizedRectangleContour(
            new PointXY(-2f, 0f),
            new PointXY(2f, 0f),
            RectangleContourParameterOrigin.BottomRight);
        var lengthOrigin = new ParameterizedRectangleContour(
            new PointXY(-2f, 0f),
            new PointXY(2f, 0f),
            8f);

        Assert.That(namedOrigin, Is.EqualTo(lengthOrigin));
        Assert.That(namedOrigin.ParameterOrigin, Is.EqualTo(new PointXY(2f, 0f)));
        Assert.That(namedOrigin.GetPoint(0f), Is.EqualTo(lengthOrigin.GetPoint(0f)));
    }

    [Test]
    public void DegenerateConversions_PreserveGeometry()
    {
        var rectangle = new Rectangle(
            new PointXY(2f, -1f),
            new PointXY(2f, 3f));

        RectangleContour contour = rectangle.ToContour();
        ContourBasedRegion contourBasedRegion = rectangle.ToRegion();
        var parameterizedContour = (ParameterizedRectangleContour)contour;

        Assert.That(contour.ToRegion(), Is.EqualTo(rectangle));
        Assert.That(parameterizedContour.ToRegion(), Is.EqualTo(rectangle));
        Assert.That(contourBasedRegion.Contains(new PointXY(2f, 1f)), Is.True);
        Assert.That(contourBasedRegion.Contains(new PointXY(2.5f, 1f)), Is.False);
        Assert.That(contourBasedRegion.Distance(new PointXY(5f, 1f)), Is.EqualTo(3f));
    }

    [Test]
    public void RectangleContour_RayIntersections_HandleDegenerateHitMissAndOverlap()
    {
        var verticalSegment = new RectangleContour(
            new PointXY(2f, 0f),
            new PointXY(2f, 2f));
        var horizontalSegment = new RectangleContour(
            new PointXY(0f, 1f),
            new PointXY(2f, 1f));
        var point = new RectangleContour(
            new PointXY(2f, 1f),
            new PointXY(2f, 1f));

        List<PointXY> hit = verticalSegment.GetPointIntersections(new Ray(new PointXY(0f, 1f)));
        List<PointXY> miss = verticalSegment.GetPointIntersections(new Ray(new PointXY(0f, 3f)));
        List<PointXY> overlap = horizontalSegment.GetPointIntersections(new Ray(new PointXY(-1f, 1f)));
        List<PointXY> pointHit = point.GetPointIntersections(new Ray(new PointXY(0f, 1f)));

        Assert.That(hit, Is.EqualTo(new[] { new PointXY(2f, 1f) }));
        Assert.That(miss, Is.Empty);
        Assert.That(overlap, Is.Empty);
        Assert.That(pointHit, Is.EqualTo(new[] { new PointXY(2f, 1f) }));
    }

    [Test]
    public void ParameterizedRectangleContour_RayIntersections_HandleDegenerateHitMissAndOverlap()
    {
        var verticalSegment = new ParameterizedRectangleContour(
            new PointXY(2f, 0f),
            new PointXY(2f, 2f));
        var horizontalSegment = new ParameterizedRectangleContour(
            new PointXY(0f, 1f),
            new PointXY(2f, 1f));

        List<PointXY> hit = verticalSegment.GetPointIntersections(new Ray(new PointXY(0f, 1f)));
        List<PointXY> miss = verticalSegment.GetPointIntersections(new Ray(new PointXY(0f, 3f)));
        List<PointXY> overlap = horizontalSegment.GetPointIntersections(new Ray(new PointXY(-1f, 1f)));

        Assert.That(hit, Is.EqualTo(new[] { new PointXY(2f, 1f) }));
        Assert.That(miss, Is.Empty);
        Assert.That(overlap, Is.Empty);
    }

    [Test]
    public void ParameterizedRectangleContourConstructor_WhenPointParameterOriginIsPositive_Throws()
    {
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ParameterizedRectangleContour(default, default, float.Epsilon))!.ParamName,
            Is.EqualTo("parameterOrigin"));
    }
}
