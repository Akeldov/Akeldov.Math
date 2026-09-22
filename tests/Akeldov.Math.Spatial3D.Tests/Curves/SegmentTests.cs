using System.Globalization;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class SegmentTests
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
            new Segment(startPoint, endPoint));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_DefaultsToIncludedEndpoints()
    {
        var segment = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 6f, 15f));

        Assert.Multiple(() =>
        {
            AssertPoint(segment.EndpointA, 1f, 2f, 3f);
            AssertPoint(segment.EndpointB, 4f, 6f, 15f);
            Assert.That(segment.IncludesEndpointA, Is.True);
            Assert.That(segment.IncludesEndpointB, Is.True);
            Assert.That(segment.Length, Is.EqualTo(13f));
        });
    }

    [Test]
    public void Constructor_WithEndpointInclusion_PreservesFlags()
    {
        var segment = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 6f, 15f),
            includesEndpointA: false,
            includesEndpointB: true);

        Assert.Multiple(() =>
        {
            Assert.That(segment.IncludesEndpointA, Is.False);
            Assert.That(segment.IncludesEndpointB, Is.True);
        });
    }

    [Test]
    public void DefaultSegment_IsExcludedZeroLengthSegmentAtOrigin()
    {
        var segment = default(Segment);

        CurveProjection projection = segment.Project(new PointXYZ(2f, 3f, 6f));

        Assert.Multiple(() =>
        {
            AssertPoint(segment.EndpointA, 0f, 0f, 0f);
            AssertPoint(segment.EndpointB, 0f, 0f, 0f);
            Assert.That(segment.IncludesEndpointA, Is.False);
            Assert.That(segment.IncludesEndpointB, Is.False);
            Assert.That(segment.Length, Is.Zero);
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 0f);
            Assert.That(projection.Distance, Is.EqualTo(7f));
        });
    }

    [Test]
    public void Length_WithLargeFiniteCoordinates_AvoidsIntermediateOverflow()
    {
        var segment = new Segment(default, new PointXYZ(float.MaxValue, 0f, 0f));

        Assert.That(segment.Length, Is.EqualTo(float.MaxValue));
    }

    [Test]
    public void FiniteTwoEndpointCurveContract_ExposesEndpointsAndLength()
    {
        IFiniteTwoEndpointCurve curve = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 6f, 15f));

        Assert.Multiple(() =>
        {
            AssertPoint(curve.EndpointA, 1f, 2f, 3f);
            AssertPoint(curve.EndpointB, 4f, 6f, 15f);
            Assert.That(curve.Length, Is.EqualTo(13f));
            Assert.That(curve, Is.InstanceOf<IPointDistanceProvider>());
        });
    }

    [Test]
    public void Project_WhenPointProjectsInsideSegment_ReturnsInteriorProjection()
    {
        var segment = new Segment(default, new PointXYZ(2f, 2f, 2f));

        CurveProjection projection = segment.Project(new PointXYZ(3f, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1f, 1f, 1f);
            Assert.That(
                projection.Distance,
                Is.EqualTo(MathF.Sqrt(6f)).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void Project_WhenPointProjectsBeforeEndpointA_ClampsToEndpointA()
    {
        var segment = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 2f, 3f));

        CurveProjection projection = segment.Project(new PointXYZ(-2f, 6f, 3f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1f, 2f, 3f);
            Assert.That(projection.Distance, Is.EqualTo(5f));
        });
    }

    [Test]
    public void Project_WhenPointProjectsAfterEndpointB_ClampsToEndpointB()
    {
        var segment = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 2f, 3f));

        CurveProjection projection = segment.Project(new PointXYZ(7f, 6f, 3f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 4f, 2f, 3f);
            Assert.That(projection.Distance, Is.EqualTo(5f));
        });
    }

    [Test]
    public void Project_WhenEndpointsAlmostCoincide_ProjectsOntoShortSegment()
    {
        float tiny = GeometryConstants.GeometryEpsilon * 0.5f;
        var segment = new Segment(default, new PointXYZ(tiny, tiny, tiny));

        CurveProjection projection = segment.Project(new PointXYZ(1f, 1f, 1f));

        AssertPoint(projection.ProjectedPoint, tiny, tiny, tiny, tiny * 0.01f);
    }

    [Test]
    public void Project_WithExtremeFiniteEndpoints_AvoidsIntermediateOverflow()
    {
        var segment = new Segment(
            new PointXYZ(-float.MaxValue, 0f, 0f),
            new PointXYZ(float.MaxValue, 0f, 0f));

        CurveProjection projection = segment.Project(new PointXYZ(0f, 1f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 0f);
            Assert.That(projection.Distance, Is.EqualTo(1f));
        });
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void Project_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z)
    {
        var segment = new Segment(default, new PointXYZ(1f, 0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            segment.Project(new PointXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Distance_ReturnsDistanceToSegment()
    {
        var segment = new Segment(default, new PointXYZ(0f, 0f, 5f));

        float distance = segment.Distance(new PointXYZ(3f, 4f, 2f));

        Assert.That(distance, Is.EqualTo(5f));
    }

    [Test]
    public void Equality_TreatsEndpointsAsUnorderedAndMovesInclusionFlagsWithThem()
    {
        var segment = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            includesEndpointA: false,
            includesEndpointB: true);
        var reversed = new Segment(
            new PointXYZ(4f, 5f, 6f),
            new PointXYZ(1f, 2f, 3f),
            includesEndpointA: true,
            includesEndpointB: false);

        Assert.Multiple(() =>
        {
            Assert.That(segment.Equals(reversed), Is.True);
            Assert.That(segment.GetHashCode(), Is.EqualTo(reversed.GetHashCode()));
            Assert.That(segment == reversed, Is.True);
            Assert.That(segment != reversed, Is.False);
        });
    }

    [Test]
    public void Equality_WhenEndpointInclusionDiffers_ReturnsFalse()
    {
        var closed = new Segment(default, new PointXYZ(1f, 0f, 0f));
        var openAtA = new Segment(
            default,
            new PointXYZ(1f, 0f, 0f),
            includesEndpointA: false,
            includesEndpointB: true);

        Assert.That(closed, Is.Not.EqualTo(openAtA));
    }

    [Test]
    public void TranslationOperators_TranslateEndpointsAndPreserveInclusion()
    {
        var segment = new Segment(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 5f, 6f),
            includesEndpointA: false,
            includesEndpointB: true);
        var translation = new VectorXYZ(10f, 20f, 30f);

        Segment translated = segment + translation;
        Segment restored = translated - translation;

        Assert.Multiple(() =>
        {
            AssertPoint(translated.EndpointA, 11f, 22f, 33f);
            AssertPoint(translated.EndpointB, 14f, 25f, 36f);
            Assert.That(translated.IncludesEndpointA, Is.False);
            Assert.That(translated.IncludesEndpointB, Is.True);
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
            var segment = new Segment(
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
