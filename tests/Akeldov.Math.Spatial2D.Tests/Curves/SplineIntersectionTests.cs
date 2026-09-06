using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class SplineIntersectionTests
{
    [Test]
    public void BSplineIntersections_SolveTheSplineInsteadOfItsPolylineApproximation()
    {
        BSpline curve = CreatePolynomialArch();
        var tangent = new Line(new PointXY(-1f, 1f), new PointXY(3f, 1f));

        List<PointXY> intersections = curve.GetPointIntersections(tangent);

        Assert.That(curve.Flatten(), Has.Count.EqualTo(1));
        Assert.That(intersections, Is.EqualTo(new[] { new PointXY(1f, 1f) }));
    }

    [Test]
    public void NurbsIntersections_SolveTheRationalSplineInsteadOfItsPolylineApproximation()
    {
        Nurbs curve = CreateRationalArch();
        var tangent = new Line(new PointXY(-1f, 1f), new PointXY(3f, 1f));

        List<PointXY> intersections = curve.GetPointIntersections(tangent);

        Assert.That(curve.Flatten(), Has.Count.EqualTo(1));
        Assert.That(intersections, Is.EqualTo(new[] { new PointXY(1f, 1f) }));
    }

    [Test]
    public void BSplineAndNurbsOverloads_AreAvailableInBothDirections()
    {
        BSpline polynomial = CreatePolynomialArch();
        Nurbs rational = CreateRationalArch();
        var ray = new Ray(new PointXY(-1f, 0.5f));
        var line = new Line(new PointXY(-1f, 0.5f), new PointXY(3f, 0.5f));
        var parameterizedLine = new ParameterizedLine(new PointXY(-1f, 0.5f), new VectorXY(1f, 0f));
        var segment = new Segment(new PointXY(-1f, 0.5f), new PointXY(3f, 0.5f));
        var parameterizedSegment = new ParameterizedSegment(new PointXY(-1f, 0.5f), new PointXY(3f, 0.5f));
        var segmentChain = new ParameterizedSegmentChain(new PointXY(-1f, 0.5f), new PointXY(3f, 0.5f));
        var arc = new Arc(new PointXY(1f, 0f), 1f, 0f, 2f * MathF.PI);
        var parameterizedArc = new ParameterizedArc(
            new PointXY(1f, 0f),
            1f,
            0f,
            2f * MathF.PI,
            AngularDirection.Counterclockwise);
        var quadratic = new QuadraticBezier(
            new PointXY(-1f, 0.5f),
            new PointXY(1f, 0.5f),
            new PointXY(3f, 0.5f));
        var cubic = new CubicBezier(
            new PointXY(-1f, 0.5f),
            new PointXY(1f / 3f, 0.5f),
            new PointXY(5f / 3f, 0.5f),
            new PointXY(3f, 0.5f));

        AssertSymmetric(polynomial.GetPointIntersections(ray), ray.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(line), line.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(parameterizedLine), parameterizedLine.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(segment), segment.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(parameterizedSegment), parameterizedSegment.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(segmentChain), segmentChain.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(arc), arc.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(parameterizedArc), parameterizedArc.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(quadratic), quadratic.GetPointIntersections(polynomial));
        AssertSymmetric(polynomial.GetPointIntersections(cubic), cubic.GetPointIntersections(polynomial));

        AssertSymmetric(rational.GetPointIntersections(ray), ray.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(line), line.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(parameterizedLine), parameterizedLine.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(segment), segment.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(parameterizedSegment), parameterizedSegment.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(segmentChain), segmentChain.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(arc), arc.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(parameterizedArc), parameterizedArc.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(quadratic), quadratic.GetPointIntersections(rational));
        AssertSymmetric(rational.GetPointIntersections(cubic), cubic.GetPointIntersections(rational));
    }

    [Test]
    public void SplinePairIntersections_AreExactAndSupportAllCombinations()
    {
        BSpline polynomial = CreatePolynomialArch();
        Nurbs rational = CreateRationalArch();
        BSpline polynomialTangent = CreatePolynomialTangent();
        Nurbs rationalTangent = CreateRationalTangent();
        PointXY expected = new PointXY(1f, 1f);

        Assert.That(polynomial.GetPointIntersections(polynomialTangent), Is.EqualTo(new[] { expected }));
        Assert.That(polynomial.GetPointIntersections(rationalTangent), Is.EqualTo(new[] { expected }));
        Assert.That(rational.GetPointIntersections(polynomialTangent), Is.EqualTo(new[] { expected }));
        Assert.That(rational.GetPointIntersections(rationalTangent), Is.EqualTo(new[] { expected }));
    }

    [Test]
    public void CurvedSplinePairs_ReturnAllTransverseIntersections()
    {
        BSpline polynomialArch = CreatePolynomialArch();
        var polynomialValley = new BSpline(2,
            new[] { new PointXY(0f, 1f), new PointXY(1f, 0f), new PointXY(2f, 1f) },
            new[] { 0f, 0f, 0f, 1f, 1f, 1f },
            segmentsPerKnotSpan: 1);
        Nurbs rationalArch = CreateRationalArch();
        var rationalValley = new Nurbs(2,
            new[] { new PointXY(0f, 1f), new PointXY(1f, 0.5f), new PointXY(2f, 1f) },
            new[] { 1f, 2f, 1f },
            new[] { 0f, 0f, 0f, 1f, 1f, 1f },
            segmentsPerKnotSpan: 1);

        List<PointXY> polynomialIntersections = polynomialArch.GetPointIntersections(polynomialValley);
        List<PointXY> rationalIntersections = rationalArch.GetPointIntersections(rationalValley);

        Assert.That(polynomialIntersections, Has.Count.EqualTo(2));
        AssertPoint(polynomialIntersections[0], new PointXY(1f - MathF.Sqrt(1f / 3f), 2f / 3f));
        AssertPoint(polynomialIntersections[1], new PointXY(1f + MathF.Sqrt(1f / 3f), 2f / 3f));
        Assert.That(rationalIntersections, Has.Count.EqualTo(2));
        AssertPoint(rationalIntersections[0], rationalArch.GetPointAt((3f - MathF.Sqrt(3f)) / 6f));
        AssertPoint(rationalIntersections[1], rationalArch.GetPointAt((3f + MathF.Sqrt(3f)) / 6f));
    }

    [Test]
    public void CurvedSplineIntersections_AreRestrictedByTargetParametersWithoutRoundingLoss()
    {
        BSpline curve = CreatePolynomialArch();
        var line = new Line(new PointXY(-1f, -0.2f), new PointXY(3f, 1f));
        var segment = new Segment(new PointXY(-1f, -0.2f), new PointXY(3f, 1f));
        var circle = new Arc(new PointXY(1f, 0.5f), 0.6f, 0f, 2f * MathF.PI);

        List<PointXY> lineIntersections = curve.GetPointIntersections(line);
        List<PointXY> segmentIntersections = curve.GetPointIntersections(segment);
        List<PointXY> circleIntersections = curve.GetPointIntersections(circle);

        Assert.That(lineIntersections, Has.Count.EqualTo(2));
        Assert.That(segmentIntersections, Is.EquivalentTo(lineIntersections));
        Assert.That(circleIntersections, Has.Count.EqualTo(2));
    }

    [Test]
    public void SplineAndBezierIntersections_MatchTheOriginalBezierEquations()
    {
        var polynomialSource = new QuadraticBezier(
            new PointXY(-2f, 0f),
            new PointXY(0f, 3f),
            new PointXY(2f, 0f));
        var target = new QuadraticBezier(
            new PointXY(-2f, 2f),
            new PointXY(1f, -2f),
            new PointXY(2f, 2f));
        var splineSource = new BSpline(2,
            new[] { polynomialSource.StartPoint, polynomialSource.ControlPoint, polynomialSource.EndPoint },
            new[] { 0f, 0f, 0f, 1f, 1f, 1f },
            segmentsPerKnotSpan: 1);
        var splineTarget = new Nurbs(2,
            new[] { target.StartPoint, target.ControlPoint, target.EndPoint },
            new[] { 1f, 1f, 1f },
            new[] { 0f, 0f, 0f, 1f, 1f, 1f },
            segmentsPerKnotSpan: 1);

        List<PointXY> expected = polynomialSource.GetPointIntersections(target);
        List<PointXY> splineToBezier = splineSource.GetPointIntersections(target);
        List<PointXY> splineToSpline = splineSource.GetPointIntersections(splineTarget);

        AssertPointSets(splineToBezier, expected);
        AssertPointSets(splineToSpline, expected);
    }

    [Test]
    public void ContinuousSplineOverlaps_AreOmitted()
    {
        BSpline polynomial = CreatePolynomialArch();
        Nurbs rational = CreateRationalArch();
        var polynomialBezier = new QuadraticBezier(
            polynomial.ControlPoints[0],
            polynomial.ControlPoints[1],
            polynomial.ControlPoints[2]);
        var cubicRational = new Nurbs(3,
            new[]
            {
                new PointXY(-2f, 0f),
                new PointXY(-1f, 3f),
                new PointXY(1f, -2f),
                new PointXY(2f, 1f)
            },
            new[] { 1f, 2f, 0.75f, 1.5f },
            new[] { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f },
            segmentsPerKnotSpan: 1);

        Assert.That(polynomial.GetPointIntersections(polynomial), Is.Empty);
        Assert.That(polynomial.GetPointIntersections(polynomialBezier), Is.Empty);
        Assert.That(rational.GetPointIntersections(rational), Is.Empty);
        Assert.That(cubicRational.GetPointIntersections(cubicRational), Is.Empty);
    }

    private static BSpline CreatePolynomialArch() => new BSpline(2,
        new[] { new PointXY(0f, 0f), new PointXY(1f, 2f), new PointXY(2f, 0f) },
        new[] { 0f, 0f, 0f, 1f, 1f, 1f },
        segmentsPerKnotSpan: 1);

    private static Nurbs CreateRationalArch() => new Nurbs(2,
        new[] { new PointXY(0f, 0f), new PointXY(1f, 1.5f), new PointXY(2f, 0f) },
        new[] { 1f, 2f, 1f },
        new[] { 0f, 0f, 0f, 1f, 1f, 1f },
        segmentsPerKnotSpan: 1);

    private static BSpline CreatePolynomialTangent() => new BSpline(1,
        new[] { new PointXY(-1f, 1f), new PointXY(3f, 1f) },
        new[] { 0f, 0f, 1f, 1f },
        segmentsPerKnotSpan: 1);

    private static Nurbs CreateRationalTangent() => new Nurbs(1,
        new[] { new PointXY(-1f, 1f), new PointXY(3f, 1f) },
        new[] { 1f, 3f },
        new[] { 0f, 0f, 1f, 1f },
        segmentsPerKnotSpan: 1);

    private static void AssertSymmetric(List<PointXY> forward, List<PointXY> reverse)
    {
        Assert.That(forward, Is.Not.Empty);
        Assert.That(reverse, Is.EquivalentTo(forward));

        forward.Clear();
        Assert.That(reverse, Is.Not.Empty);
    }

    private static void AssertPoint(PointXY actual, PointXY expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(1e-5f));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1e-5f));
    }

    private static void AssertPointSets(List<PointXY> actual, List<PointXY> expected)
    {
        Assert.That(actual, Has.Count.EqualTo(expected.Count));
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.That(actual.Exists(point => point.Distance(expected[i]) <= 1e-5f), Is.True,
                $"Expected intersection {expected[i]} was not found.");
        }
    }
}
