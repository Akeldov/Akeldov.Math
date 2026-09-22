using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class BSplineTests
{
    [Test]
    public void ClampedQuadratic_MatchesQuadraticBezierInThreeDimensions()
    {
        BSpline curve = CreateArch();
        var bezier = new QuadraticBezier(
            curve.ControlPoints[0],
            curve.ControlPoints[1],
            curve.ControlPoints[2]);

        for (int i = 0; i <= 100; i++)
            AssertPoint(curve.GetPointAt(i / 100f), bezier.GetPointAt(i / 100f));

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAt(0.5f), 1f, 1f, 2f);
            AssertPoint(curve.StartPoint, 0f, 0f, 0f);
            AssertPoint(curve.EndPoint, 2f, 0f, 4f);
            Assert.That(curve.EndpointA, Is.EqualTo(curve.StartPoint));
            Assert.That(curve.EndpointB, Is.EqualTo(curve.EndPoint));
        });
    }

    [Test]
    public void ClampedCubic_MatchesNonPlanarCubicBezier()
    {
        var points = new[]
        {
            new PointXYZ(-2f, 0f, 0f),
            new PointXYZ(-1f, 3f, 2f),
            new PointXYZ(1f, -2f, 5f),
            new PointXYZ(2f, 1f, 9f),
        };
        var curve = new BSpline(
            3,
            points,
            new[] { 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f });
        var bezier = new CubicBezier(points[0], points[1], points[2], points[3]);

        for (int i = 0; i <= 100; i++)
            AssertPoint(curve.GetPointAt(i / 100f), bezier.GetPointAt(i / 100f));
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(33)]
    public void ArbitraryDegree_HandlesCollinearControlPoints(int degree)
    {
        PointXYZ[] points = Enumerable.Range(0, degree + 1)
            .Select(i => new PointXYZ(i, 2 * i, -i))
            .ToArray();
        float[] knots = Enumerable.Repeat(0f, degree + 1)
            .Concat(Enumerable.Repeat(1f, degree + 1))
            .ToArray();
        var curve = new BSpline(degree, points, knots);

        Assert.Multiple(() =>
        {
            Assert.That(curve.Degree, Is.EqualTo(degree));
            AssertPoint(
                curve.GetPointAt(0.25f),
                degree * 0.25f,
                degree * 0.5f,
                degree * -0.25f);
            Assert.That(curve.Length, Is.EqualTo(degree * MathF.Sqrt(6f)).Within(1e-5f));
        });
    }

    [Test]
    public void NarrowNonUniformSpan_IsPreservedInDistanceAndLengthCoordinates()
    {
        var curve = new BSpline(
            1,
            new[]
            {
                default(PointXYZ),
                new PointXYZ(0f, 4f, 0f),
                new PointXYZ(3f, 4f, 0f),
            },
            new[] { 0f, 0f, 1e-20f, 1f, 1f });

        ParameterizedCurveProjection projection = curve.ProjectWithParameter(new PointXYZ(1f, 1f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAtKnot(1e-20f), 0f, 4f, 0f);
            Assert.That(curve.Length, Is.EqualTo(7f));
            AssertPoint(curve.GetPoint(5f), 1f, 4f, 0f);
            Assert.That(curve.Distance(new PointXYZ(0f, 4f, 0f)), Is.Zero);
            AssertPoint(projection.ProjectedPoint, 0f, 1f, 0f);
            Assert.That(projection.Distance, Is.EqualTo(1f));
            Assert.That(projection.CurveCoordinate, Is.EqualTo(1f));
        });
    }

    [Test]
    public void UnclampedKnots_UseActiveDomainIncludingItsEndpoints()
    {
        var curve = new BSpline(
            2,
            new[]
            {
                new PointXYZ(0f, 0f, 0f),
                new PointXYZ(2f, 4f, 2f),
                new PointXYZ(4f, 0f, 4f),
                new PointXYZ(6f, 4f, 6f),
            },
            new[] { -4f, -3f, -2f, -1f, 0f, 1f, 2f });

        Assert.Multiple(() =>
        {
            Assert.That(curve.KnotStart, Is.EqualTo(-2f));
            Assert.That(curve.KnotEnd, Is.Zero);
            AssertPoint(curve.GetPointAtKnot(-2f), 1f, 2f, 1f);
            AssertPoint(curve.GetPointAtKnot(0f), 5f, 2f, 5f);
            AssertPoint(curve.GetPointAtKnot(-1f), 3f, 2f, 3f);
            Assert.That(curve.GetPointAt(0.5f), Is.EqualTo(curve.GetPointAtKnot(-1f)));
            Assert.That(curve.GetPointAt(0f), Is.EqualTo(curve.StartPoint));
            Assert.That(curve.GetPointAt(1f), Is.EqualTo(curve.EndPoint));
            Assert.That(curve.GetPoint(0f), Is.EqualTo(curve.StartPoint));
            Assert.That(curve.GetPoint(curve.Length), Is.EqualTo(curve.EndPoint));
        });

        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(-3f));
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(1f));
    }

    [Test]
    public void RepeatedDomainEnd_SelectsLastNonEmptySpan()
    {
        var curve = new BSpline(
            2,
            new[]
            {
                new PointXYZ(0f, 0f, 0f),
                new PointXYZ(1f, 2f, 1f),
                new PointXYZ(2f, 0f, 2f),
                new PointXYZ(9f, 9f, 9f),
            },
            new[] { -1f, 0f, 0f, 1f, 1f, 2f, 3f });

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAt(1f), 2f, 0f, 2f);
            Assert.That(curve.GetPointAtKnot(1f), Is.EqualTo(curve.EndPoint));
        });
    }

    [Test]
    public void RepeatedInteriorKnot_PreservesCornerInApproximation()
    {
        var curve = new BSpline(
            2,
            new[]
            {
                new PointXYZ(0f, 0f, 0f),
                new PointXYZ(1f, 1f, 1f),
                new PointXYZ(2f, 0f, 2f),
                new PointXYZ(3f, 1f, 3f),
                new PointXYZ(4f, 0f, 4f),
            },
            new[] { 0f, 0f, 0f, 0.2f, 0.2f, 1f, 1f, 1f });

        List<ParameterizedSegment> segments = curve.Flatten();

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAtKnot(0.2f), 2f, 0f, 2f);
            Assert.That(segments, Has.Count.EqualTo(128));
            AssertPoint(segments[63].EndPoint, 2f, 0f, 2f);
            Assert.That(segments[64].StartPoint, Is.EqualTo(segments[63].EndPoint));
            Assert.That(segments[63].StartPoint.Y, Is.GreaterThan(0f));
            Assert.That(segments[64].EndPoint.Y, Is.GreaterThan(0f));
        });
    }

    [Test]
    public void ProjectionAndLengthTraversal_UseSameApproximation()
    {
        BSpline curve = CreateArch();
        var point = new PointXYZ(0.6f, 2f, 1.2f);

        ParameterizedCurveProjection projection = curve.ProjectWithParameter(point);

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPoint(projection.CurveCoordinate), projection.ProjectedPoint);
            Assert.That(curve.Project(point).ProjectedPoint, Is.EqualTo(projection.ProjectedPoint));
            Assert.That(curve.Distance(point), Is.EqualTo(projection.Distance));
            Assert.That(
                GetDistance(projection.ProjectedPoint, point),
                Is.EqualTo(projection.Distance).Within(1e-6f));
            Assert.That(curve.ProjectWithParameter(new PointXYZ(-1f, -1f, -2f)).CurveCoordinate, Is.Zero);
            Assert.That(
                curve.ProjectWithParameter(new PointXYZ(3f, -1f, 6f)).CurveCoordinate,
                Is.EqualTo(curve.Length));
        });
    }

    [Test]
    public void DegenerateCurve_HasZeroLengthAndStableProjection()
    {
        var point = new PointXYZ(2f, -3f, 4f);
        var curve = new BSpline(
            2,
            new[] { point, point, point },
            new[] { 0f, 0f, 0f, 1f, 1f, 1f });

        Assert.Multiple(() =>
        {
            Assert.That(curve.Length, Is.Zero);
            Assert.That(curve.GetPoint(0f), Is.EqualTo(point));
            Assert.That(curve.GetPointAt(0.37f), Is.EqualTo(point));
            Assert.That(
                curve.ProjectWithParameter(new PointXYZ(5f, 1f, 4f)).CurveCoordinate,
                Is.Zero);
            Assert.That(curve.Distance(new PointXYZ(5f, 1f, 4f)), Is.EqualTo(5f));
            Assert.That(curve.Flatten(), Is.Empty);
        });
    }

    [Test]
    public void RepeatedControlPoints_DoNotBreakLengthLookup()
    {
        var curve = new BSpline(
            1,
            new[]
            {
                default(PointXYZ),
                default(PointXYZ),
                new PointXYZ(2f, 0f, 2f),
                new PointXYZ(2f, 0f, 2f),
            },
            new[] { 0f, 0f, 1f, 2f, 3f, 3f });
        float halfLength = MathF.Sqrt(2f);

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPoint(halfLength), 1f, 0f, 1f);
            Assert.That(curve.Length, Is.EqualTo(2f * halfLength));
            Assert.That(
                curve.ProjectWithParameter(new PointXYZ(2f, 1f, 2f)).CurveCoordinate,
                Is.EqualTo(curve.Length));
        });
    }

    [Test]
    public void Constructor_CopiesInputsAndFlattenReturnsCallerOwnedSegments()
    {
        var points = new[] { default(PointXYZ), new PointXYZ(2f, 0f, 2f) };
        var knots = new[] { 0f, 0f, 1f, 1f };
        var curve = new BSpline(1, points, knots);
        points[0] = new PointXYZ(10f, 10f, 10f);
        knots[0] = -1f;

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAt(0.5f), 1f, 0f, 1f);
            Assert.That(curve.ControlPoints[0], Is.EqualTo(default(PointXYZ)));
            Assert.That(curve.Knots[0], Is.Zero);
        });
        Assert.Throws<NotSupportedException>(() =>
            ((IList<PointXYZ>)curve.ControlPoints)[0] = points[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)curve.Knots)[0] = -1f);

        curve.Flatten().Clear();

        Assert.That(curve.Flatten(), Has.Count.EqualTo(64));
    }

    [Test]
    public void SubdivisionCount_ControlsApproximationWithoutChangingSpline()
    {
        BSpline coarse = CreateArch(1);
        BSpline fine = CreateArch(128);

        Assert.Multiple(() =>
        {
            Assert.That(coarse.Flatten(), Has.Count.EqualTo(1));
            Assert.That(fine.Flatten(), Has.Count.EqualTo(128));
            Assert.That(fine.SegmentsPerKnotSpan, Is.EqualTo(128));
            Assert.That(coarse.GetPointAt(0.37f), Is.EqualTo(fine.GetPointAt(0.37f)));
            Assert.That(coarse.Length, Is.EqualTo(MathF.Sqrt(20f)));
            Assert.That(fine.Length, Is.GreaterThan(coarse.Length));
        });
    }

    [Test]
    public void LargeCoordinatesAndKnotDomain_DoNotOverflowIntermediateArithmetic()
    {
        var curve = new BSpline(
            1,
            new[]
            {
                new PointXYZ(-1e30f, 0f, -1e30f),
                new PointXYZ(1e30f, 0f, 1e30f),
            },
            new[] { -float.MaxValue, -float.MaxValue, float.MaxValue, float.MaxValue });

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAt(0.5f), 0f, 0f, 0f, 1e23f);
            Assert.That(curve.Length, Is.EqualTo(2e30f * MathF.Sqrt(2f)).Within(3e23f));
            Assert.That(curve.Distance(new PointXYZ(0f, 1e30f, 0f)), Is.EqualTo(1e30f));
        });
    }

    [Test]
    public void Constructor_RejectsNullInputsAndInvalidCounts()
    {
        BSpline valid = CreateArch();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentNullException>(() => new BSpline(2, null!, valid.Knots));
            Assert.Throws<ArgumentNullException>(() => new BSpline(2, valid.ControlPoints, null!));
            Assert.Throws<ArgumentException>(() =>
                new BSpline(2, valid.ControlPoints, new[] { 0f, 1f }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BSpline(1, Array.Empty<PointXYZ>(), valid.Knots));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BSpline(1, new[] { default(PointXYZ) }, valid.Knots));
        });
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenDegreeIsInvalid_Throws(int degree)
    {
        BSpline valid = CreateArch();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BSpline(degree, valid.ControlPoints, valid.Knots));

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

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void Constructor_WhenControlPointCoordinateIsNonFinite_Throws(float x, float y, float z)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BSpline(
                1,
                new[] { new PointXYZ(x, y, z), default(PointXYZ) },
                new[] { 0f, 0f, 1f, 1f }));

        Assert.That(exception!.ParamName, Is.EqualTo("controlPoints"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenKnotIsNonFinite_Throws(float value)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BSpline(
                1,
                new[] { default(PointXYZ), new PointXYZ(1f, 1f, 1f) },
                new[] { 0f, 0f, 1f, value }));

        Assert.That(exception!.ParamName, Is.EqualTo("knots"));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void ProjectionMethods_WhenPointCoordinateIsNonFinite_Throw(float x, float y, float z)
    {
        BSpline curve = CreateArch();
        PointXYZ point = new PointXYZ(x, y, z);

        var distanceException = Assert.Throws<ArgumentOutOfRangeException>(() => curve.Distance(point));
        var projectException = Assert.Throws<ArgumentOutOfRangeException>(() => curve.Project(point));
        var parameterizedException = Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.ProjectWithParameter(point));

        Assert.Multiple(() =>
        {
            Assert.That(distanceException!.ParamName, Is.EqualTo("point"));
            Assert.That(projectException!.ParamName, Is.EqualTo("point"));
            Assert.That(parameterizedException!.ParamName, Is.EqualTo("point"));
        });
    }

    [TestCase(-0.1f)]
    [TestCase(10f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void InvalidParameters_AreRejected(float parameter)
    {
        BSpline curve = CreateArch();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAt(parameter));
            Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAtKnot(parameter));
            Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPoint(parameter));
        });
    }

    [Test]
    public void Constructor_RejectsInvalidKnotVectorsAndUnrepresentableLength()
    {
        BSpline valid = CreateArch();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
                new BSpline(2, valid.ControlPoints, new[] { 0f, 0f, 0f, 1f, 0.5f, 1f }));
            Assert.Throws<ArgumentException>(() =>
                new BSpline(2, valid.ControlPoints, new[] { 0f, 0f, 0f, 0f, 1f, 1f }));
            Assert.Throws<ArgumentException>(() =>
                new BSpline(
                    1,
                    new[]
                    {
                        default(PointXYZ),
                        new PointXYZ(1f, 1f, 1f),
                        new PointXYZ(2f, 2f, 2f),
                        new PointXYZ(3f, 3f, 3f),
                    },
                    new[] { 0f, 0f, 0.5f, 0.5f, 1f, 1f }));
            Assert.Throws<ArgumentException>(() =>
                new BSpline(1, valid.ControlPoints, new[] { 0f, 0f, 0f, 1f, 1f }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BSpline(
                    1,
                    new[]
                    {
                        new PointXYZ(-float.MaxValue, 0f, 0f),
                        new PointXYZ(float.MaxValue, 0f, 0f),
                    },
                    new[] { 0f, 0f, 1f, 1f }));
        });
    }

    private static BSpline CreateArch(int segmentsPerKnotSpan = 64) => new BSpline(
        2,
        new[]
        {
            default(PointXYZ),
            new PointXYZ(1f, 2f, 2f),
            new PointXYZ(2f, 0f, 4f),
        },
        new[] { 0f, 0f, 0f, 1f, 1f, 1f },
        segmentsPerKnotSpan);

    private static float GetDistance(PointXYZ left, PointXYZ right)
    {
        double dx = (double)right.X - left.X;
        double dy = (double)right.Y - left.Y;
        double dz = (double)right.Z - left.Z;
        return (float)global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static void AssertPoint(
        PointXYZ actual,
        PointXYZ expected,
        float tolerance = 1e-6f)
    {
        AssertPoint(actual, expected.X, expected.Y, expected.Z, tolerance);
    }

    private static void AssertPoint(
        PointXYZ actual,
        float expectedX,
        float expectedY,
        float expectedZ,
        float tolerance = 1e-6f)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expectedX).Within(tolerance));
            Assert.That(actual.Y, Is.EqualTo(expectedY).Within(tolerance));
            Assert.That(actual.Z, Is.EqualTo(expectedZ).Within(tolerance));
        });
    }
}
