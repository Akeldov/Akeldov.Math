using System.Collections.Generic;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class ParameterizedSegmentChainTests
{
    [Test]
    public void Constructor_CopiesPointsAndCreatesDirectedSegments()
    {
        var points = new[]
        {
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 6f, 3f),
            new PointXYZ(4f, 6f, 15f),
        };

        var chain = new ParameterizedSegmentChain(points);
        points[1] = default;

        Assert.Multiple(() =>
        {
            Assert.That(chain.Points, Has.Count.EqualTo(3));
            AssertPoint(chain.Points[1], 4f, 6f, 3f);
            Assert.That(chain.Segments, Has.Count.EqualTo(2));
            Assert.That(
                chain.Segments[0],
                Is.EqualTo(new ParameterizedSegment(
                    new PointXYZ(1f, 2f, 3f),
                    new PointXYZ(4f, 6f, 3f))));
            Assert.That(
                chain.Segments[1],
                Is.EqualTo(new ParameterizedSegment(
                    new PointXYZ(4f, 6f, 3f),
                    new PointXYZ(4f, 6f, 15f))));
        });
    }

    [Test]
    public void Properties_ExposeDirectedEndpointsAndTotalLength()
    {
        var chain = new ParameterizedSegmentChain(
            new PointXYZ(1f, 2f, 3f),
            new PointXYZ(4f, 6f, 3f),
            new PointXYZ(4f, 6f, 15f));

        Assert.Multiple(() =>
        {
            AssertPoint(chain.StartPoint, 1f, 2f, 3f);
            AssertPoint(chain.EndPoint, 4f, 6f, 15f);
            AssertPoint(chain.EndpointA, 1f, 2f, 3f);
            AssertPoint(chain.EndpointB, 4f, 6f, 15f);
            Assert.That(chain.Length, Is.EqualTo(17f));
        });
    }

    [Test]
    public void FinitePathContract_ExposesChainGeometryAndParameterization()
    {
        IFinitePath path = new ParameterizedSegmentChain(
            default,
            new PointXYZ(3f, 0f, 0f),
            new PointXYZ(3f, 4f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(path.StartPoint, 0f, 0f, 0f);
            AssertPoint(path.EndPoint, 3f, 4f, 0f);
            AssertPoint(path.EndpointA, 0f, 0f, 0f);
            AssertPoint(path.EndpointB, 3f, 4f, 0f);
            Assert.That(path.Length, Is.EqualTo(7f));
            AssertPoint(path.GetPoint(5f), 3f, 2f, 0f);
        });
    }

    [Test]
    public void Constructor_WithReadOnlyList_CopiesPoints()
    {
        IReadOnlyList<PointXYZ> points = new List<PointXYZ>
        {
            default,
            new PointXYZ(1f, 0f, 0f),
        };

        var chain = new ParameterizedSegmentChain(points);

        Assert.That(chain.Points, Is.EqualTo(points));
        Assert.That(chain.Points, Is.Not.SameAs(points));
    }

    [Test]
    public void Constructor_WhenPointsAreNull_Throws()
    {
        var arrayException = Assert.Throws<ArgumentNullException>(() =>
            new ParameterizedSegmentChain((PointXYZ[])null!));
        var listException = Assert.Throws<ArgumentNullException>(() =>
            new ParameterizedSegmentChain((IReadOnlyList<PointXYZ>)null!));

        Assert.Multiple(() =>
        {
            Assert.That(arrayException!.ParamName, Is.EqualTo("points"));
            Assert.That(listException!.ParamName, Is.EqualTo("points"));
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Constructor_WhenFewerThanTwoPointsAreProvided_Throws(int pointCount)
    {
        var points = new PointXYZ[pointCount];

        var exception = Assert.Throws<ArgumentException>(() =>
            new ParameterizedSegmentChain(points));

        Assert.That(exception!.ParamName, Is.EqualTo("points"));
    }

    [Test]
    public void Constructor_WhenAdjacentPointsAreEqual_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ParameterizedSegmentChain(
                default,
                new PointXYZ(1f, 2f, 3f),
                new PointXYZ(1f, 2f, 3f)));

        Assert.That(exception!.ParamName, Is.EqualTo("points"));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void Constructor_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedSegmentChain(default, new PointXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("points"));
    }

    [Test]
    public void Constructor_WhenSegmentLengthIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ParameterizedSegmentChain(
                new PointXYZ(-float.MaxValue, 0f, 0f),
                new PointXYZ(float.MaxValue, 0f, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("points"));
    }

    [Test]
    public void Constructor_WhenTotalLengthExceedsFiniteFloatRange_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ParameterizedSegmentChain(
                default,
                new PointXYZ(float.MaxValue, 0f, 0f),
                new PointXYZ(float.MaxValue, float.MaxValue, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("points"));
    }

    [Test]
    public void GetPoint_ReturnsPointsAcrossConsecutiveSegments()
    {
        var chain = new ParameterizedSegmentChain(
            default,
            new PointXYZ(3f, 0f, 0f),
            new PointXYZ(3f, 4f, 0f),
            new PointXYZ(3f, 4f, 12f));

        Assert.Multiple(() =>
        {
            AssertPoint(chain.GetPoint(0f), 0f, 0f, 0f);
            AssertPoint(chain.GetPoint(2f), 2f, 0f, 0f);
            AssertPoint(chain.GetPoint(3f), 3f, 0f, 0f);
            AssertPoint(chain.GetPoint(5f), 3f, 2f, 0f);
            AssertPoint(chain.GetPoint(7f), 3f, 4f, 0f);
            AssertPoint(chain.GetPoint(19f), 3f, 4f, 12f);
        });
    }

    [TestCase(-1f)]
    [TestCase(8f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var chain = new ParameterizedSegmentChain(
            default,
            new PointXYZ(3f, 0f, 0f),
            new PointXYZ(3f, 4f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            chain.GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void ProjectWithParameter_ReturnsClosestProjectionAndChainCoordinate()
    {
        var chain = new ParameterizedSegmentChain(
            default,
            new PointXYZ(3f, 0f, 0f),
            new PointXYZ(3f, 4f, 0f),
            new PointXYZ(3f, 4f, 12f));

        ParameterizedCurveProjection projection = chain.ProjectWithParameter(new PointXYZ(5f, 2f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 3f, 2f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(5f));
            Assert.That(projection.Distance, Is.EqualTo(2f));
        });
    }

    [TestCase(float.PositiveInfinity, 0f, 0f)]
    [TestCase(0f, float.NegativeInfinity, 0f)]
    [TestCase(0f, 0f, float.PositiveInfinity)]
    public void ProjectWithParameter_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z)
    {
        var chain = new ParameterizedSegmentChain(default, new PointXYZ(1f, 0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            chain.ProjectWithParameter(new PointXYZ(x, y, z)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void ProjectAndDistance_ReturnValuesWithoutCurveCoordinate()
    {
        var chain = new ParameterizedSegmentChain(
            default,
            new PointXYZ(0f, 0f, 5f),
            new PointXYZ(0f, 4f, 5f));

        PointXYZ point = new PointXYZ(3f, 2f, 5f);
        CurveProjection projection = chain.Project(point);
        float distance = chain.Distance(point);

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 0f, 2f, 5f);
            Assert.That(projection.Distance, Is.EqualTo(3f));
            Assert.That(distance, Is.EqualTo(3f));
        });
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
