using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class BSplineTests
{
    [Test]
    public void ClampedQuadratic_MatchesBezier()
    {
        BSpline curve = CreateArch();
        var bezier = new QuadraticBezier(curve.ControlPoints[0], curve.ControlPoints[1], curve.ControlPoints[2]);

        for (int i = 0; i <= 100; i++)
            AssertPoint(curve.GetPointAt(i / 100f), bezier.GetPointAt(i / 100f));

        AssertPoint(curve.GetPointAt(0.5f), new PointXY(1f, 1f));
        Assert.That(curve.StartPoint, Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(curve.EndPoint, Is.EqualTo(new PointXY(2f, 0f)));
        Assert.That(curve.EndpointA, Is.EqualTo(curve.StartPoint));
        Assert.That(curve.EndpointB, Is.EqualTo(curve.EndPoint));
    }

    [Test]
    public void ClampedCubic_MatchesBezier()
    {
        var points = new[] { new PointXY(-2f, 0f), new PointXY(-1f, 3f), new PointXY(1f, -2f), new PointXY(2f, 1f) };
        var curve = new BSpline(3, points, new[] { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f });
        var bezier = new CubicBezier(points[0], points[1], points[2], points[3]);

        for (int i = 0; i <= 100; i++)
            AssertPoint(curve.GetPointAt(i / 100f), bezier.GetPointAt(i / 100f));
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(33)]
    public void ArbitraryDegree_HandlesCollinearControlPoints(int degree)
    {
        PointXY[] points = Enumerable.Range(0, degree + 1).Select(i => new PointXY(i, 2 * i)).ToArray();
        float[] knots = Enumerable.Repeat(0f, degree + 1).Concat(Enumerable.Repeat(1f, degree + 1)).ToArray();
        var curve = new BSpline(degree, points, knots);

        Assert.That(curve.Degree, Is.EqualTo(degree));
        AssertPoint(curve.GetPointAt(0.25f), new PointXY(degree * 0.25f, degree * 0.5f));
        Assert.That(curve.Length, Is.EqualTo(degree * MathF.Sqrt(5f)).Within(1e-5f));
    }

    [Test]
    public void NarrowNonUniformSpan_IsPreservedInDistanceAndLengthCoordinates()
    {
        var curve = new BSpline(1,
            new[] { new PointXY(0f, 0f), new PointXY(0f, 4f), new PointXY(3f, 4f) },
            new[] { 0f, 0f, 1e-20f, 1f, 1f });

        Assert.That(curve.GetPointAtKnot(1e-20f), Is.EqualTo(new PointXY(0f, 4f)));
        Assert.That(curve.Length, Is.EqualTo(7f));
        AssertPoint(curve.GetPoint(5f), new PointXY(1f, 4f));
        Assert.That(curve.Distance(new PointXY(0f, 4f)), Is.Zero);
        ParameterizedCurveProjection projection = curve.ProjectWithParameter(new PointXY(1f, 1f));
        AssertPoint(projection.ProjectedPoint, new PointXY(0f, 1f));
        Assert.That(projection.Distance, Is.EqualTo(1f));
        Assert.That(projection.CurveCoordinate, Is.EqualTo(1f));
    }

    [Test]
    public void UnclampedKnots_UseActiveDomainIncludingItsEndpoints()
    {
        var curve = new BSpline(2,
            new[] { new PointXY(0f, 0f), new PointXY(2f, 4f), new PointXY(4f, 0f), new PointXY(6f, 4f) },
            new[] { -4f, -3f, -2f, -1f, 0f, 1f, 2f });

        Assert.That(curve.KnotStart, Is.EqualTo(-2f));
        Assert.That(curve.KnotEnd, Is.Zero);
        AssertPoint(curve.GetPointAtKnot(-2f), new PointXY(1f, 2f));
        AssertPoint(curve.GetPointAtKnot(0f), new PointXY(5f, 2f));
        AssertPoint(curve.GetPointAtKnot(-1f), new PointXY(3f, 2f));
        Assert.That(curve.GetPointAt(0.5f), Is.EqualTo(curve.GetPointAtKnot(-1f)));
        Assert.That(curve.GetPointAt(0f), Is.EqualTo(curve.StartPoint));
        Assert.That(curve.GetPointAt(1f), Is.EqualTo(curve.EndPoint));
        Assert.That(curve.GetPoint(0f), Is.EqualTo(curve.StartPoint));
        Assert.That(curve.GetPoint(curve.Length), Is.EqualTo(curve.EndPoint));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(-3f));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(1f));
    }

    [Test]
    public void RepeatedDomainEnd_SelectsLastNonEmptySpan()
    {
        var curve = new BSpline(2,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 2f), new PointXY(2f, 0f), new PointXY(9f, 9f) },
            new[] { -1f, 0f, 0f, 1f, 1f, 2f, 3f });

        Assert.That(curve.GetPointAt(1f), Is.EqualTo(new PointXY(2f, 0f)));
        Assert.That(curve.GetPointAtKnot(1f), Is.EqualTo(curve.EndPoint));
    }

    [Test]
    public void RepeatedInteriorKnot_PreservesCornerInApproximation()
    {
        var curve = new BSpline(2,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 1f), new PointXY(2f, 0f), new PointXY(3f, 1f), new PointXY(4f, 0f) },
            new[] { 0f, 0f, 0f, 0.2f, 0.2f, 1f, 1f, 1f });

        Assert.That(curve.GetPointAtKnot(0.2f), Is.EqualTo(new PointXY(2f, 0f)));
        List<ParameterizedSegment> segments = curve.Flatten();
        Assert.That(segments, Has.Count.EqualTo(128));
        Assert.That(segments[63].EndPoint, Is.EqualTo(new PointXY(2f, 0f)));
        Assert.That(segments[64].StartPoint, Is.EqualTo(segments[63].EndPoint));
        Assert.That(segments[63].StartPoint.Y, Is.GreaterThan(0f));
        Assert.That(segments[64].EndPoint.Y, Is.GreaterThan(0f));
    }

    [Test]
    public void ProjectionAndLengthTraversal_UseSameApproximation()
    {
        BSpline curve = CreateArch();
        var point = new PointXY(0.6f, 2f);
        ParameterizedCurveProjection projection = curve.ProjectWithParameter(point);

        AssertPoint(curve.GetPoint(projection.CurveCoordinate), projection.ProjectedPoint);
        Assert.That(curve.Project(point).ProjectedPoint, Is.EqualTo(projection.ProjectedPoint));
        Assert.That(curve.Distance(point), Is.EqualTo(projection.Distance));
        Assert.That(projection.ProjectedPoint.Distance(point), Is.EqualTo(projection.Distance).Within(1e-6f));
        Assert.That(curve.ProjectWithParameter(new PointXY(-1f, -1f)).CurveCoordinate, Is.Zero);
        Assert.That(curve.ProjectWithParameter(new PointXY(3f, -1f)).CurveCoordinate, Is.EqualTo(curve.Length));
    }

    [Test]
    public void DegenerateCurve_HasZeroLengthAndStableProjection()
    {
        var point = new PointXY(2f, -3f);
        var curve = new BSpline(2, new[] { point, point, point }, new[] { 0f, 0f, 0f, 1f, 1f, 1f });

        Assert.That(curve.Length, Is.Zero);
        Assert.That(curve.GetPoint(0f), Is.EqualTo(point));
        Assert.That(curve.GetPointAt(0.37f), Is.EqualTo(point));
        Assert.That(curve.ProjectWithParameter(new PointXY(5f, 1f)).CurveCoordinate, Is.Zero);
        Assert.That(curve.Distance(new PointXY(5f, 1f)), Is.EqualTo(5f));
        Assert.That(curve.Flatten(), Is.Empty);
        Assert.That(curve.CountRightwardCrossings(new PointXY(0f, -3f)), Is.Zero);
    }

    [Test]
    public void RepeatedControlPoints_DoNotBreakLengthLookup()
    {
        var curve = new BSpline(1,
            new[] { new PointXY(0f, 0f), new PointXY(0f, 0f), new PointXY(2f, 0f), new PointXY(2f, 0f) },
            new[] { 0f, 0f, 1f, 2f, 3f, 3f });

        AssertPoint(curve.GetPoint(1f), new PointXY(1f, 0f));
        Assert.That(curve.Length, Is.EqualTo(2f));
        Assert.That(curve.ProjectWithParameter(new PointXY(2f, 1f)).CurveCoordinate, Is.EqualTo(2f));
    }

    [Test]
    public void Constructor_CopiesInputsAndFlattenReturnsCallerOwnedSegments()
    {
        var points = new[] { new PointXY(0f, 0f), new PointXY(2f, 0f) };
        var knots = new[] { 0f, 0f, 1f, 1f };
        var curve = new BSpline(1, points, knots);
        points[0] = new PointXY(10f, 10f);
        knots[0] = -1f;

        Assert.That(curve.GetPointAt(0.5f), Is.EqualTo(new PointXY(1f, 0f)));
        Assert.That(curve.ControlPoints[0], Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(curve.Knots[0], Is.Zero);
        Assert.Throws<NotSupportedException>(() => ((IList<PointXY>)curve.ControlPoints)[0] = points[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)curve.Knots)[0] = -1f);
        curve.Flatten().Clear();
        Assert.That(curve.Flatten(), Has.Count.EqualTo(64));
    }

    [Test]
    public void SubdivisionCount_ControlsApproximationWithoutChangingSpline()
    {
        BSpline coarse = CreateArch(1);
        BSpline fine = CreateArch(128);

        Assert.That(coarse.Flatten(), Has.Count.EqualTo(1));
        Assert.That(fine.Flatten(), Has.Count.EqualTo(128));
        Assert.That(fine.SegmentsPerKnotSpan, Is.EqualTo(128));
        Assert.That(coarse.GetPointAt(0.37f), Is.EqualTo(fine.GetPointAt(0.37f)));
        Assert.That(coarse.Length, Is.EqualTo(2f));
        Assert.That(fine.Length, Is.GreaterThan(coarse.Length));
    }

    [Test]
    public void Curve_CanParticipateInCompositeContour()
    {
        BSpline arch = CreateArch();
        var contour = new CompositeContour(new IContourPath[]
        {
            arch, new ParameterizedSegment(arch.EndPoint, arch.StartPoint)
        });

        Assert.That(contour.Encloses(new PointXY(1f, 0.25f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(1f, 1.5f)), Is.False);
        Assert.That(arch.CountRightwardCrossings(new PointXY(-1f, 0.5f)), Is.EqualTo(2));
        Assert.That(arch.CountRightwardCrossings(new PointXY(-1f, 1f)), Is.Zero);
    }

    [Test]
    public void LargeCoordinatesAndKnotDomain_DoNotOverflowIntermediateArithmetic()
    {
        var curve = new BSpline(1, new[] { new PointXY(-1e30f, 0f), new PointXY(1e30f, 0f) },
            new[] { -float.MaxValue, -float.MaxValue, float.MaxValue, float.MaxValue });

        AssertPoint(curve.GetPointAt(0.5f), new PointXY(0f, 0f));
        Assert.That(curve.Length, Is.EqualTo(2e30f).Within(2e23f));
        Assert.That(curve.Distance(new PointXY(0f, 1e30f)), Is.EqualTo(1e30f));
    }

    [Test]
    public void Constructor_RejectsNullInputsAndInvalidCounts()
    {
        BSpline valid = CreateArch();
        Assert.Throws<ArgumentNullException>(() => new BSpline(2, null!, valid.Knots));
        Assert.Throws<ArgumentNullException>(() => new BSpline(2, valid.ControlPoints, null!));
        Assert.Throws<ArgumentException>(() => new BSpline(2, valid.ControlPoints, new[] { 0f, 1f }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BSpline(1, Array.Empty<PointXY>(), valid.Knots));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BSpline(1, new[] { new PointXY(0f, 0f) }, valid.Knots));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenDegreeIsInvalid_Throws(int degree)
    {
        BSpline valid = CreateArch();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new BSpline(degree, valid.ControlPoints, valid.Knots));
        Assert.That(exception!.ParamName, Is.EqualTo("degree"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenSubdivisionCountIsInvalid_Throws(int count)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateArch(count));
        Assert.That(exception!.ParamName, Is.EqualTo("segmentsPerKnotSpan"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void NonFiniteInputs_AreRejected(float value)
    {
        BSpline curve = CreateArch();
        Assert.Throws<ArgumentOutOfRangeException>(() => new BSpline(1,
            new[] { new PointXY(value, 0f), new PointXY(0f, 0f) }, new[] { 0f, 0f, 1f, 1f }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BSpline(1,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 1f) }, new[] { 0f, 0f, 1f, value }));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.Distance(new PointXY(value, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.Project(new PointXY(0f, value)));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.ProjectWithParameter(new PointXY(value, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.CountRightwardCrossings(new PointXY(0f, value)));
    }

    [TestCase(-0.1f)]
    [TestCase(10f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void InvalidParameters_AreRejected(float parameter)
    {
        BSpline curve = CreateArch();
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAt(parameter));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(parameter));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPoint(parameter));
    }

    [Test]
    public void Constructor_RejectsInvalidKnotVectorsAndUnrepresentableLength()
    {
        BSpline valid = CreateArch();
        Assert.Throws<ArgumentException>(() => new BSpline(2, valid.ControlPoints, new[] { 0f, 0f, 0f, 1f, 0.5f, 1f }));
        Assert.Throws<ArgumentException>(() => new BSpline(2, valid.ControlPoints, new[] { 0f, 0f, 0f, 0f, 1f, 1f }));
        Assert.Throws<ArgumentException>(() => new BSpline(1,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 1f), new PointXY(2f, 2f), new PointXY(3f, 3f) },
            new[] { 0f, 0f, 0.5f, 0.5f, 1f, 1f }));
        Assert.Throws<ArgumentException>(() => new BSpline(1, valid.ControlPoints, new[] { 0f, 0f, 0f, 1f, 1f }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BSpline(1,
            new[] { new PointXY(-float.MaxValue, 0f), new PointXY(float.MaxValue, 0f) }, new[] { 0f, 0f, 1f, 1f }));
    }

    private static BSpline CreateArch(int segmentsPerKnotSpan = 64) => new BSpline(2,
        new[] { new PointXY(0f, 0f), new PointXY(1f, 2f), new PointXY(2f, 0f) },
        new[] { 0f, 0f, 0f, 1f, 1f, 1f }, segmentsPerKnotSpan);

    private static void AssertPoint(PointXY actual, PointXY expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(1e-6f));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1e-6f));
    }
}
