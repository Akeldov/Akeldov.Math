using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class NurbsTests
{
    [Test]
    public void RationalQuadratic_RepresentsQuarterCircleInThreeDimensions()
    {
        Nurbs curve = CreateQuarterCircle();

        Assert.Multiple(() =>
        {
            AssertPoint(curve.StartPoint, 1f, 2f, 0f);
            AssertPoint(curve.EndPoint, 0f, 2f, 1f);
            AssertPoint(curve.GetPointAt(0.5f), MathF.Sqrt(0.5f), 2f, MathF.Sqrt(0.5f));
            Assert.That(curve.EndpointA, Is.EqualTo(curve.StartPoint));
            Assert.That(curve.EndpointB, Is.EqualTo(curve.EndPoint));
        });

        for (int i = 0; i <= 100; i++)
        {
            PointXYZ point = curve.GetPointAt(i / 100f);
            Assert.That(
                point.X * point.X + point.Z * point.Z,
                Is.EqualTo(1f).Within(2e-7f),
                $"Sample {i}");
            Assert.That(point.Y, Is.EqualTo(2f), $"Sample {i}");
        }

        Assert.Multiple(() =>
        {
            Assert.That(curve.Length, Is.EqualTo(MathF.PI / 2f).Within(5e-5f));
            Assert.That(
                curve.Distance(new PointXYZ(2f, 2f, 2f)),
                Is.EqualTo(MathF.Sqrt(8f) - 1f).Within(1e-4f));
            Assert.That(curve.Distance(new PointXYZ(0f, 2f, 0f)), Is.EqualTo(1f).Within(1e-4f));
        });
    }

    [Test]
    public void UnitWeightsAndClampedKnots_MatchNonPlanarCubicBezier()
    {
        var points = new[]
        {
            new PointXYZ(-2f, 0f, 0f),
            new PointXYZ(-1f, 3f, 2f),
            new PointXYZ(1f, -2f, 5f),
            new PointXYZ(2f, 1f, 9f),
        };
        var curve = new Nurbs(
            3,
            points,
            new[] { 1f, 1f, 1f, 1f },
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
        float[] weights = Enumerable.Repeat(1f, degree + 1).ToArray();
        float[] knots = Enumerable.Repeat(0f, degree + 1)
            .Concat(Enumerable.Repeat(1f, degree + 1))
            .ToArray();
        var curve = new Nurbs(degree, points, weights, knots);

        Assert.Multiple(() =>
        {
            Assert.That(curve.Degree, Is.EqualTo(degree));
            AssertPoint(
                curve.GetPointAt(0.25f),
                degree * 0.25f,
                degree * 0.5f,
                degree * -0.25f);
        });
    }

    [Test]
    public void UnclampedKnots_UseActiveDomainIncludingItsEndpoints()
    {
        var curve = new Nurbs(
            2,
            new[]
            {
                new PointXYZ(0f, 0f, 0f),
                new PointXYZ(2f, 4f, 1f),
                new PointXYZ(4f, 0f, 2f),
                new PointXYZ(6f, 4f, 3f),
            },
            new[] { 1f, 1f, 1f, 1f },
            new[] { -2f, -1f, 0f, 1f, 2f, 3f, 4f });

        Assert.Multiple(() =>
        {
            Assert.That(curve.KnotStart, Is.Zero);
            Assert.That(curve.KnotEnd, Is.EqualTo(2f));
            AssertPoint(curve.StartPoint, 1f, 2f, 0.5f);
            AssertPoint(curve.EndPoint, 5f, 2f, 2.5f);
            AssertPoint(curve.GetPointAt(0.5f), 3f, 2f, 1.5f);
            Assert.That(curve.GetPointAtKnot(0f), Is.EqualTo(curve.StartPoint));
            Assert.That(curve.GetPointAtKnot(2f), Is.EqualTo(curve.EndPoint));
            Assert.That(curve.GetPoint(0f), Is.EqualTo(curve.StartPoint));
            Assert.That(curve.GetPoint(curve.Length), Is.EqualTo(curve.EndPoint));
        });
    }

    [Test]
    public void RepeatedInteriorKnot_PreservesCornerInApproximation()
    {
        var curve = new Nurbs(
            2,
            new[]
            {
                new PointXYZ(0f, 0f, 0f),
                new PointXYZ(1f, 1f, 1f),
                new PointXYZ(2f, 0f, 2f),
                new PointXYZ(3f, -1f, 3f),
                new PointXYZ(4f, 0f, 4f),
            },
            new[] { 1f, 1f, 1f, 1f, 1f },
            new[] { 0f, 0f, 0f, 0.2f, 0.2f, 1f, 1f, 1f });

        List<ParameterizedSegment> segments = curve.Flatten();

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAtKnot(0.2f), 2f, 0f, 2f);
            Assert.That(segments, Has.Count.EqualTo(128));
            AssertPoint(segments[63].EndPoint, 2f, 0f, 2f);
            Assert.That(segments[64].StartPoint, Is.EqualTo(segments[63].EndPoint));
        });
    }

    [Test]
    public void NarrowNonUniformSpan_IsPreservedInDistanceAndLengthCoordinates()
    {
        var curve = new Nurbs(
            1,
            new[]
            {
                default(PointXYZ),
                new PointXYZ(0f, 4f, 0f),
                new PointXYZ(3f, 4f, 3f),
            },
            new[] { 1f, 2f, 1f },
            new[] { 0f, 0f, 1e-20f, 1f, 1f });

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAtKnot(1e-20f), 0f, 4f, 0f);
            Assert.That(curve.Distance(new PointXYZ(0f, 4f, 0f)), Is.Zero);
            Assert.That(curve.Length, Is.EqualTo(4f + 3f * MathF.Sqrt(2f)).Within(1e-6f));
        });
    }

    [Test]
    public void ProjectionAndLengthTraversal_UseSameApproximation()
    {
        Nurbs curve = CreateQuarterCircle();
        var query = new PointXYZ(2f, 3f, 0.7f);

        ParameterizedCurveProjection projection = curve.ProjectWithParameter(query);

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPoint(projection.CurveCoordinate), projection.ProjectedPoint);
            Assert.That(curve.Project(query).ProjectedPoint, Is.EqualTo(projection.ProjectedPoint));
            Assert.That(curve.Distance(query), Is.EqualTo(projection.Distance));
            Assert.That(
                GetDistance(projection.ProjectedPoint, query),
                Is.EqualTo(projection.Distance).Within(2e-7f));
            Assert.That(projection.CurveCoordinate, Is.InRange(0f, curve.Length));
            Assert.That(curve.ProjectWithParameter(new PointXYZ(2f, 2f, -1f)).CurveCoordinate, Is.Zero);
            Assert.That(
                curve.ProjectWithParameter(new PointXYZ(-1f, 2f, 2f)).CurveCoordinate,
                Is.EqualTo(curve.Length));
        });
    }

    [Test]
    public void DegenerateCurve_HasZeroLengthAndStableProjection()
    {
        var point = new PointXYZ(2f, -3f, 4f);
        var curve = new Nurbs(
            2,
            new[] { point, point, point },
            new[] { 1f, 2f, 4f },
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
    public void Constructor_CopiesInputsAndFlattenReturnsCallerOwnedSegments()
    {
        var points = new[] { default(PointXYZ), new PointXYZ(2f, 0f, 2f) };
        var weights = new[] { 1f, 1f };
        var knots = new[] { 0f, 0f, 1f, 1f };
        var curve = new Nurbs(1, points, weights, knots);
        points[0] = new PointXYZ(10f, 10f, 10f);
        weights[0] = 5f;
        knots[0] = -1f;

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAt(0.5f), 1f, 0f, 1f);
            Assert.That(curve.ControlPoints[0], Is.EqualTo(default(PointXYZ)));
            Assert.That(curve.Weights[0], Is.EqualTo(1f));
            Assert.That(curve.Knots[0], Is.Zero);
        });
        Assert.Throws<NotSupportedException>(() =>
            ((IList<PointXYZ>)curve.ControlPoints)[0] = points[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)curve.Weights)[0] = 2f);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)curve.Knots)[0] = -1f);

        curve.Flatten().Clear();

        Assert.That(curve.Flatten(), Has.Count.EqualTo(64));
    }

    [TestCase(1e-30f)]
    [TestCase(1e30f)]
    public void CommonWeightScale_DoesNotChangeCurve(float scale)
    {
        Nurbs original = CreateQuarterCircle();
        var scaled = new Nurbs(
            original.Degree,
            original.ControlPoints,
            original.Weights.Select(weight => weight * scale).ToArray(),
            original.Knots);

        for (int i = 0; i <= 20; i++)
            AssertPoint(scaled.GetPointAt(i / 20f), original.GetPointAt(i / 20f));
    }

    [Test]
    public void SubdivisionCount_ControlsApproximationWithoutChangingSpline()
    {
        Nurbs coarse = CreateQuarterCircle(1);
        Nurbs fine = CreateQuarterCircle(128);

        Assert.Multiple(() =>
        {
            Assert.That(coarse.Flatten(), Has.Count.EqualTo(1));
            Assert.That(fine.Flatten(), Has.Count.EqualTo(128));
            Assert.That(fine.SegmentsPerKnotSpan, Is.EqualTo(128));
            Assert.That(coarse.GetPointAt(0.37f), Is.EqualTo(fine.GetPointAt(0.37f)));
            Assert.That(
                MathF.Abs(fine.Length - MathF.PI / 2f),
                Is.LessThan(MathF.Abs(coarse.Length - MathF.PI / 2f)));
        });
    }

    [Test]
    public void LargeCoordinatesAndWeights_DoNotOverflowIntermediateArithmetic()
    {
        var curve = new Nurbs(
            1,
            new[]
            {
                new PointXYZ(-1e30f, 0f, -1e30f),
                new PointXYZ(1e30f, 0f, 1e30f),
            },
            new[] { 1e30f, 1e30f },
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
        Nurbs valid = CreateQuarterCircle();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Nurbs(2, null!, valid.Weights, valid.Knots));
            Assert.Throws<ArgumentNullException>(() =>
                new Nurbs(2, valid.ControlPoints, null!, valid.Knots));
            Assert.Throws<ArgumentNullException>(() =>
                new Nurbs(2, valid.ControlPoints, valid.Weights, null!));
            Assert.Throws<ArgumentException>(() =>
                new Nurbs(2, valid.ControlPoints, new[] { 1f }, valid.Knots));
            Assert.Throws<ArgumentException>(() =>
                new Nurbs(2, valid.ControlPoints, valid.Weights, new[] { 0f, 1f }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Nurbs(1, Array.Empty<PointXYZ>(), Array.Empty<float>(), valid.Knots));
        });
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenDegreeIsInvalid_Throws(int degree)
    {
        Nurbs valid = CreateQuarterCircle();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Nurbs(degree, valid.ControlPoints, valid.Weights, valid.Knots));

        Assert.That(exception!.ParamName, Is.EqualTo("degree"));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenWeightIsInvalid_Throws(float weight)
    {
        Nurbs valid = CreateQuarterCircle();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Nurbs(2, valid.ControlPoints, new[] { 1f, weight, 1f }, valid.Knots));

        Assert.That(exception!.ParamName, Is.EqualTo("weights"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MaxValue)]
    public void Constructor_WhenSubdivisionCountIsInvalid_Throws(int count)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateQuarterCircle(count));

        Assert.That(exception!.ParamName, Is.EqualTo("segmentsPerKnotSpan"));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void Constructor_WhenControlPointCoordinateIsNonFinite_Throws(float x, float y, float z)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Nurbs(
                1,
                new[] { new PointXYZ(x, y, z), default(PointXYZ) },
                new[] { 1f, 1f },
                new[] { 0f, 0f, 1f, 1f }));

        Assert.That(exception!.ParamName, Is.EqualTo("controlPoints"));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Constructor_WhenKnotIsNonFinite_Throws(float value)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Nurbs(
                1,
                new[] { default(PointXYZ), new PointXYZ(1f, 1f, 1f) },
                new[] { 1f, 1f },
                new[] { 0f, 0f, 1f, value }));

        Assert.That(exception!.ParamName, Is.EqualTo("knots"));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void ProjectionMethods_WhenPointCoordinateIsNonFinite_Throw(float x, float y, float z)
    {
        Nurbs curve = CreateQuarterCircle();
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
    [TestCase(2f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void InvalidParameters_AreRejected(float parameter)
    {
        Nurbs curve = CreateQuarterCircle();

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
        Nurbs valid = CreateQuarterCircle();

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() =>
                new Nurbs(
                    2,
                    valid.ControlPoints,
                    valid.Weights,
                    new[] { 0f, 0f, 0f, 1f, 0.5f, 1f }));
            Assert.Throws<ArgumentException>(() =>
                new Nurbs(
                    2,
                    valid.ControlPoints,
                    valid.Weights,
                    new[] { 0f, 0f, 0f, 0f, 1f, 1f }));
            Assert.Throws<ArgumentException>(() =>
                new Nurbs(
                    1,
                    new[]
                    {
                        default(PointXYZ),
                        new PointXYZ(1f, 1f, 1f),
                        new PointXYZ(2f, 2f, 2f),
                        new PointXYZ(3f, 3f, 3f),
                    },
                    new[] { 1f, 1f, 1f, 1f },
                    new[] { 0f, 0f, 0.5f, 0.5f, 1f, 1f }));
            Assert.Throws<ArgumentException>(() =>
                new Nurbs(
                    1,
                    valid.ControlPoints,
                    valid.Weights,
                    new[] { 0f, 0f, 0f, 1f, 1f }));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Nurbs(
                    1,
                    new[]
                    {
                        new PointXYZ(-float.MaxValue, 0f, 0f),
                        new PointXYZ(float.MaxValue, 0f, 0f),
                    },
                    new[] { 1f, 1f },
                    new[] { 0f, 0f, 1f, 1f }));
        });
    }

    private static Nurbs CreateQuarterCircle(int segmentsPerKnotSpan = 64) => new Nurbs(
        2,
        new[]
        {
            new PointXYZ(1f, 2f, 0f),
            new PointXYZ(1f, 2f, 1f),
            new PointXYZ(0f, 2f, 1f),
        },
        new[] { 1f, MathF.Sqrt(0.5f), 1f },
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
