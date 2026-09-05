using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class NurbsTests
{
    [Test]
    public void RationalQuadratic_RepresentsQuarterCircle()
    {
        Nurbs curve = CreateQuarterCircle();

        Assert.That(curve.StartPoint, Is.EqualTo(new PointXY(1f, 0f)));
        Assert.That(curve.EndPoint, Is.EqualTo(new PointXY(0f, 1f)));
        AssertPoint(curve.GetPointAt(0.5f), new PointXY(MathF.Sqrt(0.5f), MathF.Sqrt(0.5f)));
        for (int i = 0; i <= 100; i++)
        {
            PointXY point = curve.GetPointAt(i / 100f);
            Assert.That(point.X * point.X + point.Y * point.Y, Is.EqualTo(1f).Within(2e-7f), $"Sample {i}");
        }

        Assert.That(curve.Length, Is.EqualTo(MathF.PI / 2f).Within(5e-5f));
        Assert.That(curve.Distance(new PointXY(2f, 2f)), Is.EqualTo(MathF.Sqrt(8f) - 1f).Within(1e-4f));
        Assert.That(curve.Distance(new PointXY(0f, 0f)), Is.EqualTo(1f).Within(1e-4f));
    }

    [Test]
    public void UnitWeightsAndClampedKnots_MatchCubicBezier()
    {
        var points = new[] { new PointXY(-2f, 0f), new PointXY(-1f, 3f), new PointXY(1f, -2f), new PointXY(2f, 1f) };
        var curve = new Nurbs(3, points, new[] { 1f, 1f, 1f, 1f }, new[] { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f });
        var bezier = new CubicBezier(points[0], points[1], points[2], points[3]);

        for (int i = 0; i <= 100; i++)
            AssertPoint(curve.GetPointAt(i / 100f), bezier.GetPointAt(i / 100f));
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(33)]
    public void Degree_IsNotLimitedToQuadraticOrCubic(int degree)
    {
        PointXY[] points = Enumerable.Range(0, degree + 1).Select(i => new PointXY(i, 2 * i)).ToArray();
        float[] knots = Enumerable.Repeat(0f, degree + 1).Concat(Enumerable.Repeat(1f, degree + 1)).ToArray();
        var curve = new Nurbs(degree, points, Enumerable.Repeat(1f, degree + 1).ToArray(), knots);

        AssertPoint(curve.GetPointAt(0.25f), new PointXY(degree * 0.25f, degree * 0.5f));
    }

    [Test]
    public void UnclampedKnots_UseActiveDomainAndEvaluateBothEndpoints()
    {
        var curve = new Nurbs(2,
            new[] { new PointXY(0f, 0f), new PointXY(2f, 4f), new PointXY(4f, 0f), new PointXY(6f, 4f) },
            new[] { 1f, 1f, 1f, 1f }, new[] { -2f, -1f, 0f, 1f, 2f, 3f, 4f });

        Assert.That(curve.KnotStart, Is.Zero);
        Assert.That(curve.KnotEnd, Is.EqualTo(2f));
        AssertPoint(curve.StartPoint, new PointXY(1f, 2f));
        AssertPoint(curve.EndPoint, new PointXY(5f, 2f));
        AssertPoint(curve.GetPointAt(0.5f), new PointXY(3f, 2f));
        Assert.That(curve.GetPointAtKnot(0f), Is.EqualTo(curve.StartPoint));
        Assert.That(curve.GetPointAtKnot(2f), Is.EqualTo(curve.EndPoint));
        Assert.That(curve.GetPoint(0f), Is.EqualTo(curve.StartPoint));
        Assert.That(curve.GetPoint(curve.Length), Is.EqualTo(curve.EndPoint));
    }

    [Test]
    public void RepeatedDomainEnd_SelectsLastNonEmptySpan()
    {
        var curve = new Nurbs(2,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 2f), new PointXY(2f, 0f), new PointXY(9f, 9f) },
            new[] { 1f, 1f, 1f, 1f }, new[] { -1f, 0f, 0f, 1f, 1f, 2f, 3f });

        Assert.That(curve.GetPointAt(1f), Is.EqualTo(new PointXY(2f, 0f)));
        Assert.That(curve.GetPointAtKnot(1f), Is.EqualTo(curve.EndPoint));
    }

    [Test]
    public void RepeatedInteriorKnot_PreservesCornerAndPolylineJoin()
    {
        var curve = new Nurbs(2,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 1f), new PointXY(2f, 0f), new PointXY(3f, -1f), new PointXY(4f, 0f) },
            new[] { 1f, 1f, 1f, 1f, 1f }, new[] { 0f, 0f, 0f, 0.2f, 0.2f, 1f, 1f, 1f });

        Assert.That(curve.GetPointAtKnot(0.2f), Is.EqualTo(new PointXY(2f, 0f)));
        List<ParameterizedSegment> segments = curve.Flatten();
        Assert.That(segments, Has.Count.EqualTo(128));
        Assert.That(segments[63].EndPoint, Is.EqualTo(new PointXY(2f, 0f)));
        Assert.That(segments[64].StartPoint, Is.EqualTo(segments[63].EndPoint));
    }

    [Test]
    public void VeryNarrowNonUniformSpan_IsIncludedInDistanceAndLength()
    {
        var curve = new Nurbs(1,
            new[] { new PointXY(0f, 0f), new PointXY(0f, 10f), new PointXY(1f, 0f) },
            new[] { 1f, 2f, 1f }, new[] { 0f, 0f, 1e-20f, 1f, 1f });

        Assert.That(curve.GetPointAtKnot(1e-20f), Is.EqualTo(new PointXY(0f, 10f)));
        Assert.That(curve.Distance(new PointXY(0f, 10f)), Is.Zero);
        Assert.That(curve.Length, Is.EqualTo(10f + MathF.Sqrt(101f)).Within(2e-6f));
    }

    [Test]
    public void ProjectionAndLengthCoordinate_UseSameApproximation()
    {
        Nurbs curve = CreateQuarterCircle();
        var query = new PointXY(2f, 0.7f);
        ParameterizedCurveProjection projection = curve.ProjectWithParameter(query);

        AssertPoint(curve.GetPoint(projection.CurveCoordinate), projection.ProjectedPoint);
        Assert.That(curve.Project(query).ProjectedPoint, Is.EqualTo(projection.ProjectedPoint));
        Assert.That(curve.Distance(query), Is.EqualTo(projection.Distance));
        Assert.That(projection.ProjectedPoint.Distance(query), Is.EqualTo(projection.Distance).Within(2e-7f));
        Assert.That(projection.CurveCoordinate, Is.InRange(0f, curve.Length));
        Assert.That(curve.ProjectWithParameter(new PointXY(2f, -1f)).CurveCoordinate, Is.Zero);
        Assert.That(curve.ProjectWithParameter(new PointXY(-1f, 2f)).CurveCoordinate, Is.EqualTo(curve.Length));
    }

    [Test]
    public void CoincidentControlPoints_HaveZeroLengthAndStableProjection()
    {
        var point = new PointXY(2f, -3f);
        var curve = new Nurbs(2, new[] { point, point, point }, new[] { 1f, 2f, 4f }, new[] { 0f, 0f, 0f, 1f, 1f, 1f });

        Assert.That(curve.Length, Is.Zero);
        Assert.That(curve.GetPoint(0f), Is.EqualTo(point));
        Assert.That(curve.GetPointAt(0.37f), Is.EqualTo(point));
        Assert.That(curve.ProjectWithParameter(new PointXY(5f, 1f)).CurveCoordinate, Is.Zero);
        Assert.That(curve.Distance(new PointXY(5f, 1f)), Is.EqualTo(5f));
        Assert.That(curve.Flatten(), Is.Empty);
    }

    [Test]
    public void ZeroLengthSpansInApproximation_DoNotBreakLengthLookup()
    {
        var curve = new Nurbs(1,
            new[] { new PointXY(0f, 0f), new PointXY(0f, 0f), new PointXY(2f, 0f), new PointXY(2f, 0f) },
            new[] { 1f, 1f, 1f, 1f }, new[] { 0f, 0f, 1f, 2f, 3f, 3f });

        AssertPoint(curve.GetPoint(1f), new PointXY(1f, 0f));
        Assert.That(curve.Length, Is.EqualTo(2f));
        Assert.That(curve.ProjectWithParameter(new PointXY(2f, 1f)).CurveCoordinate, Is.EqualTo(2f));
    }

    [Test]
    public void SubdivisionCount_ControlsApproximationWithoutChangingSpline()
    {
        Nurbs coarse = CreateQuarterCircle(1);
        Nurbs fine = CreateQuarterCircle(128);

        Assert.That(coarse.Flatten(), Has.Count.EqualTo(1));
        Assert.That(fine.Flatten(), Has.Count.EqualTo(128));
        Assert.That(coarse.SegmentsPerKnotSpan, Is.EqualTo(1));
        Assert.That(coarse.GetPointAt(0.37f), Is.EqualTo(fine.GetPointAt(0.37f)));
        Assert.That(MathF.Abs(fine.Length - MathF.PI / 2f), Is.LessThan(MathF.Abs(coarse.Length - MathF.PI / 2f)));
    }

    [Test]
    public void Constructor_CopiesInputsAndExposesReadOnlyState()
    {
        var points = new[] { new PointXY(0f, 0f), new PointXY(2f, 0f) };
        var weights = new[] { 1f, 1f };
        var knots = new[] { 0f, 0f, 1f, 1f };
        var curve = new Nurbs(1, points, weights, knots);
        points[0] = new PointXY(10f, 10f);
        weights[0] = 5f;
        knots[0] = -1f;

        Assert.That(curve.GetPointAt(0.5f), Is.EqualTo(new PointXY(1f, 0f)));
        Assert.That(curve.ControlPoints[0], Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(curve.Weights[0], Is.EqualTo(1f));
        Assert.That(curve.Knots[0], Is.Zero);
        Assert.Throws<NotSupportedException>(() => ((IList<PointXY>)curve.ControlPoints)[0] = points[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)curve.Weights)[0] = 2f);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)curve.Knots)[0] = -1f);

        List<ParameterizedSegment> flattened = curve.Flatten();
        flattened.Clear();
        Assert.That(curve.Flatten(), Has.Count.EqualTo(64));
    }

    [Test]
    public void Curve_CanParticipateInCompositeContour()
    {
        Nurbs arc = CreateQuarterCircle();
        var contour = new CompositeContour(new IContourPath[]
        {
            arc,
            new ParameterizedSegment(arc.EndPoint, new PointXY(0f, 0f)),
            new ParameterizedSegment(new PointXY(0f, 0f), arc.StartPoint)
        });

        Assert.That(contour.Encloses(new PointXY(0.25f, 0.25f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(0.9f, 0.9f)), Is.False);
        Assert.That(arc.CountRightwardCrossings(new PointXY(-1f, 0f)), Is.EqualTo(1));
        Assert.That(arc.CountRightwardCrossings(new PointXY(-1f, 1f)), Is.Zero);
    }

    [TestCase(1e-30f)]
    [TestCase(1e30f)]
    public void CommonWeightScale_DoesNotChangeCurve(float scale)
    {
        Nurbs original = CreateQuarterCircle();
        var scaled = new Nurbs(original.Degree, original.ControlPoints, original.Weights.Select(w => w * scale).ToArray(), original.Knots);

        for (int i = 0; i <= 20; i++)
            AssertPoint(scaled.GetPointAt(i / 20f), original.GetPointAt(i / 20f));
    }

    [Test]
    public void LargeCoordinatesAndWeights_DoNotOverflowIntermediateArithmetic()
    {
        var curve = new Nurbs(1, new[] { new PointXY(-1e30f, 0f), new PointXY(1e30f, 0f) },
            new[] { 1e30f, 1e30f }, new[] { -float.MaxValue, -float.MaxValue, float.MaxValue, float.MaxValue });

        AssertPoint(curve.GetPointAt(0.5f), new PointXY(0f, 0f));
        Assert.That(curve.Length, Is.EqualTo(2e30f).Within(2e23f));
        Assert.That(curve.Distance(new PointXY(0f, 1e30f)), Is.EqualTo(1e30f));
    }

    [Test]
    public void Constructor_WhenLengthExceedsFloatRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nurbs(1,
            new[] { new PointXY(-float.MaxValue, 0f), new PointXY(float.MaxValue, 0f) },
            new[] { 1f, 1f }, new[] { 0f, 0f, 1f, 1f }));
    }

    [Test]
    public void Constructor_RejectsNullInputsAndMismatchedCounts()
    {
        var points = new[] { new PointXY(0f, 0f), new PointXY(1f, 1f) };
        var weights = new[] { 1f, 1f };
        var knots = new[] { 0f, 0f, 1f, 1f };
        Assert.Throws<ArgumentNullException>(() => new Nurbs(1, null!, weights, knots));
        Assert.Throws<ArgumentNullException>(() => new Nurbs(1, points, null!, knots));
        Assert.Throws<ArgumentNullException>(() => new Nurbs(1, points, weights, null!));
        Assert.Throws<ArgumentException>(() => new Nurbs(1, points, new[] { 1f }, knots));
        Assert.Throws<ArgumentException>(() => new Nurbs(1, points, weights, new[] { 0f, 1f }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nurbs(1, Array.Empty<PointXY>(), Array.Empty<float>(), knots));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenDegreeIsInvalid_Throws(int degree)
    {
        Nurbs valid = CreateQuarterCircle();
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nurbs(degree, valid.ControlPoints, valid.Weights, valid.Knots));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenWeightIsInvalid_Throws(float weight)
    {
        Nurbs valid = CreateQuarterCircle();
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nurbs(2, valid.ControlPoints, new[] { 1f, weight, 1f }, valid.Knots));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void NonFinitePointsAndKnots_AreRejected(float value)
    {
        Nurbs curve = CreateQuarterCircle();
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nurbs(1,
            new[] { new PointXY(value, 0f), new PointXY(0f, 0f) }, new[] { 1f, 1f }, new[] { 0f, 0f, 1f, 1f }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Nurbs(1,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 1f) }, new[] { 1f, 1f }, new[] { 0f, 0f, 1f, value }));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.Distance(new PointXY(value, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.Project(new PointXY(0f, value)));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.ProjectWithParameter(new PointXY(value, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.CountRightwardCrossings(new PointXY(0f, value)));
    }

    [TestCase(-0.1f)]
    [TestCase(2f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void InvalidParameters_AreRejected(float parameter)
    {
        Nurbs curve = CreateQuarterCircle();
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAt(parameter));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(parameter));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPoint(parameter));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenSubdivisionCountIsInvalid_Throws(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateQuarterCircle(count));
    }

    [Test]
    public void Constructor_RejectsDecreasingCollapsedAndDiscontinuousKnotVectors()
    {
        Nurbs valid = CreateQuarterCircle();
        Assert.Throws<ArgumentException>(() => new Nurbs(2, valid.ControlPoints, valid.Weights, new[] { 0f, 0f, 0f, 1f, 0.5f, 1f }));
        Assert.Throws<ArgumentException>(() => new Nurbs(2, valid.ControlPoints, valid.Weights, new[] { 0f, 0f, 0f, 0f, 1f, 1f }));
        Assert.Throws<ArgumentException>(() => new Nurbs(1,
            new[] { new PointXY(0f, 0f), new PointXY(1f, 1f), new PointXY(2f, 2f), new PointXY(3f, 3f) },
            new[] { 1f, 1f, 1f, 1f }, new[] { 0f, 0f, 0.5f, 0.5f, 1f, 1f }));
        Assert.Throws<ArgumentException>(() => new Nurbs(1, valid.ControlPoints, valid.Weights, new[] { 0f, 0f, 0f, 1f, 1f }));
    }

    private static Nurbs CreateQuarterCircle(int segmentsPerKnotSpan = 64) => new Nurbs(2,
        new[] { new PointXY(1f, 0f), new PointXY(1f, 1f), new PointXY(0f, 1f) },
        new[] { 1f, MathF.Sqrt(0.5f), 1f }, new[] { 0f, 0f, 0f, 1f, 1f, 1f }, segmentsPerKnotSpan);

    private static void AssertPoint(PointXY actual, PointXY expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(1e-6f));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1e-6f));
    }
}
