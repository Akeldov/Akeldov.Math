using System.Globalization;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class ParameterizedSegmentTests
{
    [TestCase(float.PositiveInfinity, 0f, 0f, "startPoint")]
    [TestCase(0f, float.NegativeInfinity, 0f, "startPoint")]
    [TestCase(0f, 0f, float.PositiveInfinity, "startPoint")]
    [TestCase(float.NegativeInfinity, 0f, 0f, "endPoint")]
    [TestCase(0f, float.PositiveInfinity, 0f, "endPoint")]
    [TestCase(0f, 0f, float.NegativeInfinity, "endPoint")]
    public void Constructor_WhenEndpointCoordinateIsInvalid_Throws(
        float x,
        float y,
        float z,
        string paramName)
    {
        PointXYZ startPoint = paramName == "startPoint" ? new PointXYZ(x, y, z) : default;
        PointXYZ endPoint = paramName == "endPoint" ? new PointXYZ(x, y, z) : new PointXYZ(1f, 1f, 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedSegment(startPoint, endPoint));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_DefaultsToIncludedEndpointsAndPreservesDirection()
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(4f, 6f, 15f),
            new PointXYZ(1f, 2f, 3f));

        Assert.Multiple(() =>
        {
            AssertPoint(segment.StartPoint, 4f, 6f, 15f);
            AssertPoint(segment.EndPoint, 1f, 2f, 3f);
            AssertPoint(segment.EndpointA, 4f, 6f, 15f);
            AssertPoint(segment.EndpointB, 1f, 2f, 3f);
            Assert.That(segment.IncludesStartPoint, Is.True);
            Assert.That(segment.IncludesEndPoint, Is.True);
            Assert.That(segment.Length, Is.EqualTo(13f));
        });
    }

    [Test]
    public void Constructor_WithEndpointInclusion_PreservesFlags()
    {
        var segment = new ParameterizedSegment(
            default,
            new PointXYZ(1f, 0f, 0f),
            includesStartPoint: false,
            includesEndPoint: true);

        Assert.Multiple(() =>
        {
            Assert.That(segment.IncludesStartPoint, Is.False);
            Assert.That(segment.IncludesEndPoint, Is.True);
        });
    }

    [Test]
    public void DefaultParameterizedSegment_IsExcludedZeroLengthPathAtOrigin()
    {
        var segment = default(ParameterizedSegment);

        ParameterizedCurveProjection projection = segment.ProjectWithParameter(new PointXYZ(2f, 3f, 6f));

        Assert.Multiple(() =>
        {
            AssertPoint(segment.StartPoint, 0f, 0f, 0f);
            AssertPoint(segment.EndPoint, 0f, 0f, 0f);
            Assert.That(segment.IncludesStartPoint, Is.False);
            Assert.That(segment.IncludesEndPoint, Is.False);
            Assert.That(segment.Length, Is.Zero);
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.Zero);
            Assert.That(projection.Distance, Is.EqualTo(7f));
        });
    }

    [Test]
    public void FinitePathContract_ExposesDirectedEndpointsLengthAndParameterization()
    {
        IFinitePath path = new ParameterizedSegment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 6f, 15f));

        Assert.Multiple(() =>
        {
            AssertPoint(path.StartPoint, 1f, 2f, 3f);
            AssertPoint(path.EndPoint, 4f, 6f, 15f);
            AssertPoint(path.EndpointA, 1f, 2f, 3f);
            AssertPoint(path.EndpointB, 4f, 6f, 15f);
            Assert.That(path.Length, Is.EqualTo(13f));
            AssertPoint(path.GetPoint(13f), 4f, 6f, 15f);
        });
    }

    [Test]
    public void GetPoint_ReturnsPointAtLengthCoordinate()
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(5f, 2f, 3f),
            new PointXYZ(1f, 2f, 3f));

        Assert.Multiple(() =>
        {
            AssertPoint(segment.GetPoint(0f), 5f, 2f, 3f);
            AssertPoint(segment.GetPoint(2f), 3f, 2f, 3f);
            AssertPoint(segment.GetPoint(4f), 1f, 2f, 3f);
        });
    }

    [TestCase(-1f)]
    [TestCase(5f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var segment = new ParameterizedSegment(default, new PointXYZ(4f, 0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void GetPoint_WhenSegmentIsShorterThanGeometryEpsilon_InterpolatesAlongSegment()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new ParameterizedSegment(default, new PointXYZ(tiny, tiny, tiny));

        PointXYZ point = segment.GetPoint(segment.Length);

        AssertPoint(point, tiny, tiny, tiny, tiny * 0.01f);
    }

    [Test]
    public void GetPoint_WithLargeFiniteLength_ReturnsEndpointWithoutIntermediateOverflow()
    {
        var segment = new ParameterizedSegment(default, new PointXYZ(float.MaxValue, 0f, 0f));

        PointXYZ point = segment.GetPoint(segment.Length);

        AssertPoint(point, float.MaxValue, 0f, 0f);
    }

    [Test]
    public void ProjectWithParameter_WhenPointProjectsInsideSegment_ReturnsDirectedCoordinate()
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(5f, 0f, 0f),
            new PointXYZ(1f, 0f, 0f));

        ParameterizedCurveProjection projection = segment.ProjectWithParameter(new PointXYZ(3f, 2f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 3f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(2f));
            Assert.That(projection.Distance, Is.EqualTo(2f));
        });
    }

    [TestCase(8f, 5f, 0f)]
    [TestCase(-2f, 1f, 4f)]
    public void ProjectWithParameter_WhenPointProjectsOutsideSegment_ClampsToEndpoint(
        float pointX,
        float expectedProjectionX,
        float expectedCoordinate)
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(5f, 0f, 0f),
            new PointXYZ(1f, 0f, 0f));

        ParameterizedCurveProjection projection = segment.ProjectWithParameter(new PointXYZ(pointX, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, expectedProjectionX, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(expectedCoordinate));
        });
    }

    [Test]
    public void ProjectWithParameter_WhenEndpointsAlmostCoincide_ProjectsOntoShortSegment()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new ParameterizedSegment(default, new PointXYZ(tiny, tiny, tiny));

        ParameterizedCurveProjection projection = segment.ProjectWithParameter(new PointXYZ(1f, 1f, 1f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, tiny, tiny, tiny, tiny * 0.01f);
            Assert.That(
                projection.CurveCoordinate,
                Is.EqualTo(segment.Length).Within(tiny * 0.01f));
        });
    }

    [Test]
    public void ProjectWithParameter_WithExtremeFiniteEndpoints_AvoidsIntermediateOverflow()
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(-float.MaxValue, 0f, 0f),
            new PointXYZ(float.MaxValue, 0f, 0f));

        ParameterizedCurveProjection projection = segment.ProjectWithParameter(new PointXYZ(0f, 1f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(float.MaxValue));
            Assert.That(projection.Distance, Is.EqualTo(1f));
        });
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void ProjectWithParameter_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z)
    {
        var segment = new ParameterizedSegment(default, new PointXYZ(1f, 0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.ProjectWithParameter(new PointXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void ProjectAndDistance_ReturnValuesWithoutCurveCoordinate()
    {
        var segment = new ParameterizedSegment(default, new PointXYZ(0f, 0f, 5f));

        CurveProjection projection = segment.Project(new PointXYZ(3f, 4f, 2f));
        float distance = segment.Distance(new PointXYZ(3f, 4f, 2f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 2f);
            Assert.That(projection.Distance, Is.EqualTo(5f));
            Assert.That(distance, Is.EqualTo(5f));
        });
    }

    [Test]
    public void Equality_TreatsEndpointDirectionAsSignificant()
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            includesStartPoint: false,
            includesEndPoint: true);
        var same = new ParameterizedSegment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            includesStartPoint: false,
            includesEndPoint: true);
        var reversed = new ParameterizedSegment(
            new PointXYZ(4f, 5f, 6f),
            new PointXYZ(1f, 2f, 3f),
            includesStartPoint: true,
            includesEndPoint: false);

        Assert.Multiple(() =>
        {
            Assert.That(segment.Equals(same), Is.True);
            Assert.That(segment.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(segment == same, Is.True);
            Assert.That(segment != same, Is.False);
            Assert.That(segment.Equals(reversed), Is.False);
        });
    }

    [Test]
    public void ExplicitConversionToSegment_PreservesEndpointsAndInclusion()
    {
        var parameterizedSegment = new ParameterizedSegment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            includesStartPoint: false,
            includesEndPoint: true);

        Segment segment = (Segment)parameterizedSegment;

        Assert.Multiple(() =>
        {
            AssertPoint(segment.EndpointA, 1f, 2f, 3f);
            AssertPoint(segment.EndpointB, 4f, 5f, 6f);
            Assert.That(segment.IncludesEndpointA, Is.False);
            Assert.That(segment.IncludesEndpointB, Is.True);
        });
    }

    [Test]
    public void TranslationOperators_TranslateEndpointsAndPreserveDirectionAndInclusion()
    {
        var segment = new ParameterizedSegment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            includesStartPoint: false,
            includesEndPoint: true);
        var translation = new VectorXYZ(10f, 20f, 30f);

        ParameterizedSegment translated = segment + translation;
        ParameterizedSegment restored = translated - translation;

        Assert.Multiple(() =>
        {
            AssertPoint(translated.StartPoint, 11f, 22f, 33f);
            AssertPoint(translated.EndPoint, 14f, 25f, 36f);
            Assert.That(translated.IncludesStartPoint, Is.False);
            Assert.That(translated.IncludesEndPoint, Is.True);
            Assert.That(restored, Is.EqualTo(segment));
        });
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var segment = new ParameterizedSegment(
                new PointXYZ(1.5f, 2.25f, 3.75f),
                new PointXYZ(4.5f, 5.25f, 6.75f));

            Assert.That(
                segment.ToString(),
                Is.EqualTo("((1.5, 2.25, 3.75) - (4.5, 5.25, 6.75))"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

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
