using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class BezierTests
{
    [Test]
    public void QuadraticBezier_GetPointAt_ReturnsQuadraticPoint()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));

        AssertPoint(curve.GetPointAt(0f), 0f, 0f);
        AssertPoint(curve.GetPointAt(0.5f), 1f, 1f);
        AssertPoint(curve.GetPointAt(1f), 2f, 0f);
    }

    [Test]
    public void CubicBezier_GetPointAt_ReturnsCubicPoint()
    {
        var curve = new CubicBezier(
            new PointXY(0f, 0f),
            new PointXY(0f, 3f),
            new PointXY(3f, 3f),
            new PointXY(3f, 0f));

        AssertPoint(curve.GetPointAt(0f), 0f, 0f);
        AssertPoint(curve.GetPointAt(0.5f), 1.5f, 2.25f);
        AssertPoint(curve.GetPointAt(1f), 3f, 0f);
    }

    [Test]
    public void QuadraticBezier_Project_WhenClosestPointIsInterior_ReturnsCurvePoint()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));

        CurveProjection projection = curve.Project(new PointXY(1f, 2f));

        AssertPoint(projection.ProjectedPoint, 1f, 1f);
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void CubicBezier_Project_WhenClosestPointIsInterior_ReturnsCurvePoint()
    {
        var curve = new CubicBezier(
            new PointXY(0f, 0f),
            new PointXY(0f, 3f),
            new PointXY(3f, 3f),
            new PointXY(3f, 0f));

        CurveProjection projection = curve.Project(new PointXY(1.5f, 3f));

        AssertPoint(projection.ProjectedPoint, 1.5f, 2.25f);
        Assert.That(projection.Distance, Is.EqualTo(0.75f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void QuadraticBezier_FinitePathContract_WhenCurveIsCollinear_ExposesLengthCoordinatesAndProjection()
    {
        IFinitePath curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f),
            new PointXY(2f, 0f));

        AssertPoint(curve.StartPoint, 0f, 0f);
        AssertPoint(curve.EndPoint, 2f, 0f);
        AssertPoint(curve.EndpointA, 0f, 0f);
        AssertPoint(curve.EndpointB, 2f, 0f);
        Assert.That(curve.Length, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        AssertPoint(curve.GetPoint(1f), 1f, 0f);

        ParameterizedCurveProjection projection = curve.ProjectWithParameter(new PointXY(1f, 2f));

        AssertPoint(projection.ProjectedPoint, 1f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void CubicBezier_RayIntersections_SolvesCurvePolynomial()
    {
        var curve = new CubicBezier(
            new PointXY(0f, 0f),
            new PointXY(0f, 3f),
            new PointXY(3f, 3f),
            new PointXY(3f, 0f));
        var ray = new Ray(new PointXY(-1f, 2.25f));

        List<PointXY> intersections = curve.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 1.5f, 2.25f);
    }

    [Test]
    public void QuadraticBezier_RayIntersections_SolvesCurvePolynomialAndOrdersAlongRay()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));
        var ray = new Ray(new PointXY(3f, 0.5f), MathF.PI);

        List<PointXY> intersections = curve.GetPointIntersections(ray);

        float rootOffset = MathF.Sqrt(0.5f);
        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f + rootOffset, 0.5f);
        AssertPoint(intersections[1], 1f - rootOffset, 0.5f);
    }

    [Test]
    public void CubicBezier_RayIntersections_FiltersPolynomialRootsBehindRay()
    {
        var curve = new CubicBezier(
            new PointXY(0f, 0f),
            new PointXY(0f, 3f),
            new PointXY(3f, 3f),
            new PointXY(3f, 0f));
        var ray = new Ray(new PointXY(1.5f, 1f));

        List<PointXY> intersections = curve.GetPointIntersections(ray);

        float parameter = (3f + MathF.Sqrt(5f)) / 6f;
        PointXY expected = curve.GetPointAt(parameter);
        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], expected.X, expected.Y);
    }

    [Test]
    public void QuadraticBezier_LineIntersections_SolvesCurvePolynomial()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));
        var line = new Line(new PointXY(-1f, 0.5f), new PointXY(3f, 0.5f));

        List<PointXY> intersections = curve.GetPointIntersections(line);

        float rootOffset = MathF.Sqrt(0.5f);
        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f - rootOffset, 0.5f);
        AssertPoint(intersections[1], 1f + rootOffset, 0.5f);
    }

    [Test]
    public void QuadraticBezier_ParameterizedLineIntersections_UsesParameterizedDirection()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));
        var geometricLine = new Line(new PointXY(-1f, 0.5f), new PointXY(3f, 0.5f));
        var line = new ParameterizedLine(geometricLine, new PointXY(0f, 0.5f), new VectorXY(-1f, 0f));

        List<PointXY> intersections = curve.GetPointIntersections(line);

        float rootOffset = MathF.Sqrt(0.5f);
        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f + rootOffset, 0.5f);
        AssertPoint(intersections[1], 1f - rootOffset, 0.5f);
    }

    [Test]
    public void QuadraticBezier_SegmentIntersections_RestrictsAndOrdersIntersections()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));
        var segment = new Segment(new PointXY(3f, 0.5f), new PointXY(1f, 0.5f));

        List<PointXY> intersections = curve.GetPointIntersections(segment);

        float rootOffset = MathF.Sqrt(0.5f);
        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 1f + rootOffset, 0.5f);
    }

    [Test]
    public void CubicBezier_LineIntersections_SolvesCurvePolynomial()
    {
        var curve = new CubicBezier(
            new PointXY(0f, 0f),
            new PointXY(0f, 3f),
            new PointXY(3f, 3f),
            new PointXY(3f, 0f));
        var line = new Line(new PointXY(-1f, 1f), new PointXY(4f, 1f));

        List<PointXY> intersections = curve.GetPointIntersections(line);

        float firstParameter = (3f - MathF.Sqrt(5f)) / 6f;
        float secondParameter = (3f + MathF.Sqrt(5f)) / 6f;
        PointXY firstExpected = curve.GetPointAt(firstParameter);
        PointXY secondExpected = curve.GetPointAt(secondParameter);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], firstExpected.X, firstExpected.Y);
        AssertPoint(intersections[1], secondExpected.X, secondExpected.Y);
    }

    [Test]
    public void CubicBezier_LineIntersections_WhenPolynomialHasThreeRoots_ReturnsAllPoints()
    {
        var curve = new CubicBezier(
            new PointXY(0f, -3f / 32f),
            new PointXY(1f, 13f / 96f),
            new PointXY(2f, -13f / 96f),
            new PointXY(3f, 3f / 32f));
        var line = new Line(new PointXY(-1f, 0f), new PointXY(4f, 0f));

        List<PointXY> intersections = curve.GetPointIntersections(line);

        Assert.That(intersections, Has.Count.EqualTo(3));
        AssertPoint(intersections[0], 0.75f, 0f);
        AssertPoint(intersections[1], 1.5f, 0f);
        AssertPoint(intersections[2], 2.25f, 0f);
    }

    [Test]
    public void BezierLineIntersections_WhenLineIsOutsideCurveWithinGeometryEpsilon_ReturnsEmpty()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));
        float lineY = 1f + GeometryConstants.GeometryEpsilon * 0.5f;
        var line = new Line(new PointXY(-1f, lineY), new PointXY(3f, lineY));

        List<PointXY> intersections = curve.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void BezierLineIntersections_WhenCurveContinuouslyOverlapsLine_ReturnsEmpty()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f),
            new PointXY(2f, 0f));
        var line = new Line(new PointXY(-1f, 0f), new PointXY(3f, 0f));

        List<PointXY> intersections = curve.GetPointIntersections(line);

        Assert.That(intersections, Is.Empty);
    }

    [Test]
    public void QuadraticBezierArcIntersections_WhenCurveCrossesCircle_ReturnsPointsInArcOrder()
    {
        var curve = new QuadraticBezier(
            new PointXY(-2f, 0f),
            new PointXY(0f, 0f),
            new PointXY(2f, 0f));
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        List<PointXY> intersections = curve.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f, 0f);
        AssertPoint(intersections[1], -1f, 0f);
    }

    [Test]
    public void QuadraticBezierArcIntersections_WhenCurveIsTangent_ReturnsRepeatedRootOnce()
    {
        var curve = new QuadraticBezier(
            new PointXY(-2f, 1f),
            new PointXY(0f, 1f),
            new PointXY(2f, 1f));
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        List<PointXY> intersections = curve.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 0f, 1f);
    }

    [Test]
    public void CubicBezierArcIntersections_WhenCurveCrossesCircle_ReturnsPointsInArcOrder()
    {
        var curve = new CubicBezier(
            new PointXY(-2f, 0f),
            new PointXY(-2f / 3f, 0f),
            new PointXY(2f / 3f, 0f),
            new PointXY(2f, 0f));
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        List<PointXY> intersections = curve.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f, 0f);
        AssertPoint(intersections[1], -1f, 0f);
    }

    [Test]
    public void CubicBezierArcIntersections_WhenCurveIsTangent_ReturnsRepeatedRootOnce()
    {
        var curve = new CubicBezier(
            new PointXY(-2f, 1f),
            new PointXY(-2f / 3f, 1f),
            new PointXY(2f / 3f, 1f),
            new PointXY(2f, 1f));
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);

        List<PointXY> intersections = curve.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(1));
        AssertPoint(intersections[0], 0f, 1f);
    }

    [Test]
    public void QuadraticBezierParameterizedArcIntersections_WhenArcIsClockwise_ReturnsPointsInAngularDirection()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, -2f),
            new PointXY(0f, 0f),
            new PointXY(0f, 2f));
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            2f * MathF.PI,
            AngularDirection.Clockwise);

        List<PointXY> intersections = curve.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 0f, -1f);
        AssertPoint(intersections[1], 0f, 1f);
    }

    [Test]
    public void CubicBezierParameterizedArcIntersections_WhenArcIsClockwise_ReturnsPointsInAngularDirection()
    {
        var curve = new CubicBezier(
            new PointXY(0f, -2f),
            new PointXY(0f, -2f / 3f),
            new PointXY(0f, 2f / 3f),
            new PointXY(0f, 2f));
        var arc = new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            2f * MathF.PI,
            AngularDirection.Clockwise);

        List<PointXY> intersections = curve.GetPointIntersections(arc);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 0f, -1f);
        AssertPoint(intersections[1], 0f, 1f);
    }

    [Test]
    public void LineQuadraticBezierIntersections_ReturnsPointsInBezierOrder()
    {
        var line = new Line(new PointXY(-2f, 0.5f), new PointXY(2f, 0.5f));
        var curve = new QuadraticBezier(
            new PointXY(2f, 1f),
            new PointXY(0f, -1f),
            new PointXY(-2f, 1f));

        List<PointXY> intersections = line.GetPointIntersections(curve);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections[0].X, Is.GreaterThan(0f));
        Assert.That(intersections[1].X, Is.LessThan(0f));
    }

    [Test]
    public void ArcQuadraticBezierIntersections_ReturnsPointsInBezierOrder()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);
        var curve = new QuadraticBezier(
            new PointXY(2f, 0f),
            new PointXY(0f, 0f),
            new PointXY(-2f, 0f));

        List<PointXY> intersections = arc.GetPointIntersections(curve);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f, 0f);
        AssertPoint(intersections[1], -1f, 0f);
    }

    [Test]
    public void QuadraticBezierQuadraticBezierIntersections_IsolatesQuarticAndOrdersAlongTarget()
    {
        var source = new QuadraticBezier(
            new PointXY(-2f, 1f),
            new PointXY(0f, -1f),
            new PointXY(2f, 1f));
        var target = new QuadraticBezier(
            new PointXY(-2f, -0.5f),
            new PointXY(0f, 1.5f),
            new PointXY(2f, -0.5f));

        List<PointXY> intersections = source.GetPointIntersections(target);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], -1f, 0.25f);
        AssertPoint(intersections[1], 1f, 0.25f);
    }

    [Test]
    public void QuadraticBezierQuadraticBezierIntersections_WhenTargetIsLinear_ReturnsPointsInTargetOrder()
    {
        var source = new QuadraticBezier(
            new PointXY(-2f, 1f),
            new PointXY(0f, -1f),
            new PointXY(2f, 1f));
        var target = new QuadraticBezier(
            new PointXY(2f, 0.5f),
            new PointXY(0f, 0.5f),
            new PointXY(-2f, 0.5f));

        List<PointXY> intersections = source.GetPointIntersections(target);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections[0].X, Is.GreaterThan(0f));
        Assert.That(intersections[1].X, Is.LessThan(0f));
    }

    [Test]
    public void CubicBezierQuadraticBezierIntersections_IsolatesSexticAndOrdersAlongTarget()
    {
        var source = new CubicBezier(
            new PointXY(2f, 0.25f),
            new PointXY(2f / 3f, 0.25f),
            new PointXY(-2f / 3f, 0.25f),
            new PointXY(-2f, 0.25f));
        var target = new QuadraticBezier(
            new PointXY(-2f, -0.5f),
            new PointXY(0f, 1.5f),
            new PointXY(2f, -0.5f));

        List<PointXY> intersections = source.GetPointIntersections(target);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], -1f, 0.25f);
        AssertPoint(intersections[1], 1f, 0.25f);
    }

    [Test]
    public void LineCubicBezierIntersections_ReturnsPointsInBezierOrder()
    {
        var line = new Line(new PointXY(-2f, 0.5f), new PointXY(2f, 0.5f));
        var curve = new CubicBezier(
            new PointXY(2f, 1f),
            new PointXY(2f / 3f, -1f / 3f),
            new PointXY(-2f / 3f, -1f / 3f),
            new PointXY(-2f, 1f));

        List<PointXY> intersections = line.GetPointIntersections(curve);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections[0].X, Is.GreaterThan(0f));
        Assert.That(intersections[1].X, Is.LessThan(0f));
    }

    [Test]
    public void ArcCubicBezierIntersections_ReturnsPointsInBezierOrder()
    {
        var arc = new Arc(new PointXY(0f, 0f), 1f, 0f, 2f * MathF.PI);
        var curve = new CubicBezier(
            new PointXY(2f, 0f),
            new PointXY(2f / 3f, 0f),
            new PointXY(-2f / 3f, 0f),
            new PointXY(-2f, 0f));

        List<PointXY> intersections = arc.GetPointIntersections(curve);

        Assert.That(intersections, Has.Count.EqualTo(2));
        AssertPoint(intersections[0], 1f, 0f);
        AssertPoint(intersections[1], -1f, 0f);
    }

    [Test]
    public void QuadraticBezierCubicBezierIntersections_ReturnsPointsInCubicOrder()
    {
        var source = new QuadraticBezier(
            new PointXY(-2f, 1f),
            new PointXY(0f, -1f),
            new PointXY(2f, 1f));
        var target = new CubicBezier(
            new PointXY(2f, 0.5f),
            new PointXY(2f / 3f, 0.5f),
            new PointXY(-2f / 3f, 0.5f),
            new PointXY(-2f, 0.5f));

        List<PointXY> intersections = source.GetPointIntersections(target);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections[0].X, Is.GreaterThan(0f));
        Assert.That(intersections[1].X, Is.LessThan(0f));
    }

    [Test]
    public void CubicBezierCubicBezierIntersections_UsesOriginalResultantAndOrdersAlongTarget()
    {
        var source = new CubicBezier(
            new PointXY(1f, 0f),
            new PointXY(1f / 3f, 0f),
            new PointXY(-1f / 3f, 0f),
            new PointXY(-1f, 0f));
        var target = new CubicBezier(
            new PointXY(0f, -0.09375f),
            new PointXY(0f, 0.13541667f),
            new PointXY(0f, -0.13541667f),
            new PointXY(1f, 0.09375f));

        List<PointXY> intersections = source.GetPointIntersections(target);

        Assert.That(intersections, Has.Count.EqualTo(3));
        AssertPoint(intersections[0], 0.015625f, 0f);
        AssertPoint(intersections[1], 0.125f, 0f);
        AssertPoint(intersections[2], 0.421875f, 0f);
    }

    [Test]
    public void CubicBezierCubicBezierIntersections_WhenTargetIsLinear_ReturnsPointsInTargetOrder()
    {
        var source = new CubicBezier(
            new PointXY(-2f, 1f),
            new PointXY(-2f / 3f, -1f / 3f),
            new PointXY(2f / 3f, -1f / 3f),
            new PointXY(2f, 1f));
        var target = new CubicBezier(
            new PointXY(2f, 0.5f),
            new PointXY(2f / 3f, 0.5f),
            new PointXY(-2f / 3f, 0.5f),
            new PointXY(-2f, 0.5f));

        List<PointXY> intersections = source.GetPointIntersections(target);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections[0].X, Is.GreaterThan(0f));
        Assert.That(intersections[1].X, Is.LessThan(0f));
    }

    [Test]
    public void QuadraticBezier_CountRightwardCrossings_CountsRootsAndExcludesTangent()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));

        Assert.That(curve.CountRightwardCrossings(new PointXY(-1f, 0.5f)), Is.EqualTo(2));
        Assert.That(curve.CountRightwardCrossings(new PointXY(1f, 0.5f)), Is.EqualTo(1));
        Assert.That(curve.CountRightwardCrossings(new PointXY(-1f, 1f)), Is.Zero);
    }

    [Test]
    public void CubicBezier_CountRightwardCrossings_WhenScanlineHasThreeRoots_ReturnsThree()
    {
        var curve = new CubicBezier(
            new PointXY(0f, -1f),
            new PointXY(1f, 2f),
            new PointXY(2f, -2f),
            new PointXY(3f, 1f));

        Assert.That(curve.CountRightwardCrossings(new PointXY(-1f, 0f)), Is.EqualTo(3));
    }

    [Test]
    public void QuadraticBezier_Flatten_ReturnsNewCallerOwnedMutableSegments()
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));

        List<ParameterizedSegment> segments = curve.Flatten(4);

        Assert.That(segments, Has.Count.EqualTo(4));
        AssertPoint(segments[0].StartPoint, 0f, 0f);
        AssertPoint(segments[3].EndPoint, 2f, 0f);

        segments.Clear();

        Assert.That(curve.Flatten(4), Has.Count.EqualTo(4));
    }

    [Test]
    public void QuadraticBezier_WhenUsedInCompositeContour_CanEncloseArea()
    {
        var arch = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 2f),
            new PointXY(2f, 0f));
        var baseLine = new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(0f, 0f));
        var contour = new CompositeContour(new IFinitePath[] { arch, baseLine });

        Assert.That(contour.Encloses(new PointXY(1f, 0.25f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(1f, 1.5f)), Is.False);
    }

    [TestCase(-0.1f)]
    [TestCase(1.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void GetPointAt_WhenParameterIsInvalid_Throws(float t)
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f),
            new PointXY(2f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAt(t));

        Assert.That(exception!.ParamName, Is.EqualTo("t"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Flatten_WhenSegmentCountIsInvalid_Throws(int segmentCount)
    {
        var curve = new QuadraticBezier(
            new PointXY(0f, 0f),
            new PointXY(1f, 1f),
            new PointXY(2f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => curve.Flatten(segmentCount));

        Assert.That(exception!.ParamName, Is.EqualTo("segmentCount"));
    }

    private static void AssertPoint(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
