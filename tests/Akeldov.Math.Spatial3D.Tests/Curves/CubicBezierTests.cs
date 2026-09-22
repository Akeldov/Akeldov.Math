using System.Globalization;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class CubicBezierTests
{
    [TestCase(float.PositiveInfinity, 0f, 0f, "startPoint")]
    [TestCase(0f, float.NegativeInfinity, 0f, "startPoint")]
    [TestCase(0f, 0f, float.PositiveInfinity, "startPoint")]
    [TestCase(float.NegativeInfinity, 0f, 0f, "controlPointA")]
    [TestCase(0f, float.PositiveInfinity, 0f, "controlPointA")]
    [TestCase(0f, 0f, float.NegativeInfinity, "controlPointA")]
    [TestCase(float.PositiveInfinity, 0f, 0f, "controlPointB")]
    [TestCase(0f, float.NegativeInfinity, 0f, "controlPointB")]
    [TestCase(0f, 0f, float.PositiveInfinity, "controlPointB")]
    [TestCase(float.NegativeInfinity, 0f, 0f, "endPoint")]
    [TestCase(0f, float.PositiveInfinity, 0f, "endPoint")]
    [TestCase(0f, 0f, float.NegativeInfinity, "endPoint")]
    public void Constructor_WhenControlPointCoordinateIsInvalid_Throws(
        float x,
        float y,
        float z,
        string paramName)
    {
        PointXYZ startPoint = paramName == "startPoint" ? new PointXYZ(x, y, z) : default;
        PointXYZ controlPointA = paramName == "controlPointA" ? new PointXYZ(x, y, z) : default;
        PointXYZ controlPointB = paramName == "controlPointB" ? new PointXYZ(x, y, z) : default;
        PointXYZ endPoint = paramName == "endPoint" ? new PointXYZ(x, y, z) : default;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CubicBezier(startPoint, controlPointA, controlPointB, endPoint));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Properties_ExposeDirectedControlPointsAndApproximateLength()
    {
        var curve = new CubicBezier(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(1f, 6f, 3f),
            new PointXYZ(4f, 6f, 9f),
            new PointXYZ(4f, 2f, 12f));

        Assert.Multiple(() =>
        {
            AssertPoint(curve.StartPoint, 1f, 2f, 3f);
            AssertPoint(curve.ControlPointA, 1f, 6f, 3f);
            AssertPoint(curve.ControlPointB, 4f, 6f, 9f);
            AssertPoint(curve.EndPoint, 4f, 2f, 12f);
            AssertPoint(curve.EndpointA, 1f, 2f, 3f);
            AssertPoint(curve.EndpointB, 4f, 2f, 12f);
            Assert.That(curve.Length, Is.GreaterThan(curve.StartPoint.Distance(curve.EndPoint)));
        });
    }

    [Test]
    public void DefaultCubicBezier_IsZeroLengthPathAtOrigin()
    {
        var curve = default(CubicBezier);

        Assert.Multiple(() =>
        {
            AssertPoint(curve.StartPoint, 0f, 0f, 0f);
            AssertPoint(curve.ControlPointA, 0f, 0f, 0f);
            AssertPoint(curve.ControlPointB, 0f, 0f, 0f);
            AssertPoint(curve.EndPoint, 0f, 0f, 0f);
            Assert.That(curve.Length, Is.Zero);
            AssertPoint(curve.GetPointAt(0.5f), 0f, 0f, 0f);
            AssertPoint(curve.GetPoint(0f), 0f, 0f, 0f);
        });
    }

    [Test]
    public void FinitePathContract_WhenCurveIsCollinear_ExposesLengthCoordinatesAndProjection()
    {
        IFinitePath curve = new CubicBezier(
            default,
            new PointXYZ(1f, 0f, 0f),
            new PointXYZ(2f, 0f, 0f),
            new PointXYZ(3f, 0f, 0f));

        ParameterizedCurveProjection projection = curve.ProjectWithParameter(new PointXYZ(1.5f, 2f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(curve.StartPoint, 0f, 0f, 0f);
            AssertPoint(curve.EndPoint, 3f, 0f, 0f);
            AssertPoint(curve.EndpointA, 0f, 0f, 0f);
            AssertPoint(curve.EndpointB, 3f, 0f, 0f);
            Assert.That(curve.Length, Is.EqualTo(3f).Within(GeometryConstants.GeometryEpsilon));
            AssertPoint(curve.GetPoint(1.5f), 1.5f, 0f, 0f);
            AssertPoint(projection.ProjectedPoint, 1.5f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(1.5f).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(projection.Distance, Is.EqualTo(2f).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void GetPointAt_ReturnsCubicPointOnNonPlanarCurve()
    {
        var curve = new CubicBezier(
            default,
            new PointXYZ(0f, 3f, 3f),
            new PointXYZ(3f, 3f, 6f),
            new PointXYZ(3f, 0f, 9f));

        Assert.Multiple(() =>
        {
            AssertPoint(curve.GetPointAt(0f), 0f, 0f, 0f);
            AssertPoint(curve.GetPointAt(0.5f), 1.5f, 2.25f, 4.5f);
            AssertPoint(curve.GetPointAt(1f), 3f, 0f, 9f);
        });
    }

    [TestCase(-0.1f)]
    [TestCase(1.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPointAt_WhenParameterIsInvalid_Throws(float t)
    {
        var curve = CreateCurve();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetPointAt(t));

        Assert.That(exception!.ParamName, Is.EqualTo("t"));
    }

    [Test]
    public void GetPointAt_WithExtremeFiniteCoordinates_AvoidsIntermediateOverflow()
    {
        var curve = new CubicBezier(
            new PointXYZ(-float.MaxValue, 0f, 0f),
            new PointXYZ(float.MaxValue, 0f, 0f),
            new PointXYZ(-float.MaxValue, 0f, 0f),
            new PointXYZ(float.MaxValue, 0f, 0f));

        PointXYZ point = curve.GetPointAt(0.5f);

        AssertPoint(point, 0f, 0f, 0f);
    }

    [TestCase(-1f)]
    [TestCase(4f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var curve = new CubicBezier(
            default,
            new PointXYZ(1f, 0f, 0f),
            new PointXYZ(2f, 0f, 0f),
            new PointXYZ(3f, 0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void Flatten_ReturnsNewCallerOwnedMutableDirectedSegments()
    {
        var curve = CreateCurve();

        List<ParameterizedSegment> segments = curve.Flatten(4);

        Assert.Multiple(() =>
        {
            Assert.That(segments, Has.Count.EqualTo(4));
            AssertPoint(segments[0].StartPoint, 0f, 0f, 0f);
            AssertPoint(segments[3].EndPoint, 3f, 0f, 9f);
        });

        segments.Clear();

        Assert.That(curve.Flatten(4), Has.Count.EqualTo(4));
    }

    [Test]
    public void Flatten_WhenCurveIsDegenerate_OmitsZeroLengthSegments()
    {
        var curve = default(CubicBezier);

        List<ParameterizedSegment> segments = curve.Flatten(4);

        Assert.That(segments, Is.Empty);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Flatten_WhenSegmentCountIsInvalid_Throws(int segmentCount)
    {
        var curve = CreateCurve();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.Flatten(segmentCount));

        Assert.That(exception!.ParamName, Is.EqualTo("segmentCount"));
    }

    [Test]
    public void Project_WhenClosestPointIsInterior_ReturnsPointOnOriginalCurve()
    {
        var curve = CreateCurve();

        CurveProjection projection = curve.Project(new PointXYZ(1.5f, 3f, 4.5f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1.5f, 2.25f, 4.5f);
            Assert.That(projection.Distance, Is.EqualTo(0.75f).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void ProjectAndDistance_WithExtremeFiniteControlPoints_AvoidIntermediateOverflow()
    {
        float third = float.MaxValue / 3f;
        var curve = new CubicBezier(
            new PointXYZ(-float.MaxValue, 0f, 0f),
            new PointXYZ(-third, 0f, 0f),
            new PointXYZ(third, 0f, 0f),
            new PointXYZ(float.MaxValue, 0f, 0f));

        CurveProjection projection = curve.Project(default);
        float distance = curve.Distance(default);

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 0f);
            Assert.That(projection.Distance, Is.Zero);
            Assert.That(distance, Is.Zero);
        });
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void ProjectionMethods_WhenPointCoordinateIsInvalid_Throw(float x, float y, float z)
    {
        var curve = CreateCurve();
        PointXYZ point = new PointXYZ(x, y, z);

        var projectException = Assert.Throws<ArgumentOutOfRangeException>(() => curve.Project(point));
        var parameterizedException = Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.ProjectWithParameter(point));
        var distanceException = Assert.Throws<ArgumentOutOfRangeException>(() => curve.Distance(point));

        Assert.Multiple(() =>
        {
            Assert.That(projectException!.ParamName, Is.EqualTo("point"));
            Assert.That(parameterizedException!.ParamName, Is.EqualTo("point"));
            Assert.That(distanceException!.ParamName, Is.EqualTo("point"));
        });
    }

    [Test]
    public void Equality_TreatsControlPointOrderAsSignificant()
    {
        var curve = new CubicBezier(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            new PointXYZ(7f, 8f, 10f),
            new PointXYZ(11f, 12f, 13f));
        var same = new CubicBezier(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            new PointXYZ(7f, 8f, 10f),
            new PointXYZ(11f, 12f, 13f));
        var reversed = new CubicBezier(
            new PointXYZ(11f, 12f, 13f),
            new PointXYZ(7f, 8f, 10f),
            new PointXYZ(4f, 5f, 6f),
            new PointXYZ(1f, 2f, 3f));

        Assert.Multiple(() =>
        {
            Assert.That(curve.Equals(same), Is.True);
            Assert.That(curve.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(curve == same, Is.True);
            Assert.That(curve != same, Is.False);
            Assert.That(curve.Equals(reversed), Is.False);
        });
    }

    [Test]
    public void TranslationOperators_TranslateAllControlPoints()
    {
        var curve = new CubicBezier(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            new PointXYZ(7f, 8f, 9f),
            new PointXYZ(10f, 11f, 12f));
        var translation = new VectorXYZ(10f, 20f, 30f);

        CubicBezier translated = curve + translation;
        CubicBezier restored = translated - translation;

        Assert.Multiple(() =>
        {
            AssertPoint(translated.StartPoint, 11f, 22f, 33f);
            AssertPoint(translated.ControlPointA, 14f, 25f, 36f);
            AssertPoint(translated.ControlPointB, 17f, 28f, 39f);
            AssertPoint(translated.EndPoint, 20f, 31f, 42f);
            Assert.That(restored, Is.EqualTo(curve));
        });
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var curve = new CubicBezier(
                new PointXYZ(1.5f, 2.25f, 3.75f),
                new PointXYZ(4.5f, 5.25f, 6.75f),
                new PointXYZ(7.5f, 8.25f, 9.75f),
                new PointXYZ(10.5f, 11.25f, 12.75f));

            Assert.That(
                curve.ToString(),
                Is.EqualTo(
                    "CubicBezier((1.5, 2.25, 3.75), (4.5, 5.25, 6.75), " +
                    "(7.5, 8.25, 9.75), (10.5, 11.25, 12.75))"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static CubicBezier CreateCurve() => new CubicBezier(
        default,
        new PointXYZ(0f, 3f, 3f),
        new PointXYZ(3f, 3f, 6f),
        new PointXYZ(3f, 0f, 9f));

    private static void AssertPoint(
        PointXYZ actual,
        float expectedX,
        float expectedY,
        float expectedZ,
        float tolerance = GeometryConstants.GeometryEpsilon)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expectedX).Within(tolerance));
            Assert.That(actual.Y, Is.EqualTo(expectedY).Within(tolerance));
            Assert.That(actual.Z, Is.EqualTo(expectedZ).Within(tolerance));
        });
    }
}
