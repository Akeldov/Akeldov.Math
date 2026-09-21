using System.Globalization;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class LineTests
{
    [Test]
    public void Constructor_WhenPointsAreEqual_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Line(new PointXYZ(2f, 3f, 4f), new PointXYZ(2f, 3f, 4f)));

        Assert.That(exception!.ParamName, Is.EqualTo("b"));
    }

    [Test]
    public void Constructor_WhenPointsAreAlmostEqual_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new Line(
            default,
            new PointXYZ(GeometryConstants.GeometryEpsilon * 0.5f, 0f, 0f)));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f, "a")]
    [TestCase(0f, float.NegativeInfinity, 0f, "a")]
    [TestCase(0f, 0f, float.PositiveInfinity, "b")]
    public void Constructor_WhenPointCoordinateIsInvalid_Throws(float x, float y, float z, string paramName)
    {
        PointXYZ a = paramName == "a" ? new PointXYZ(x, y, z) : default;
        PointXYZ b = paramName == "b" ? new PointXYZ(x, y, z) : new PointXYZ(1f, 0f, 0f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Line(a, b));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WhenDirectionIsZero_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Line(default, VectorXYZ.Zero));

        Assert.That(exception!.ParamName, Is.EqualTo("direction"));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f, "point")]
    [TestCase(0f, float.NegativeInfinity, 0f, "point")]
    [TestCase(0f, 0f, float.PositiveInfinity, "direction")]
    public void Constructor_WhenPointOrDirectionComponentIsInvalid_Throws(
        float x,
        float y,
        float z,
        string paramName)
    {
        PointXYZ point = paramName == "point" ? new PointXYZ(x, y, z) : default;
        VectorXYZ direction = paramName == "direction" ? new VectorXYZ(x, y, z) : VectorXYZ.BasisX;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Line(point, direction));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WithPointAndDirection_CanonicalizesDirectionAndOrigin()
    {
        var line = new Line(new PointXYZ(2f, 3f, 4f), new VectorXYZ(-2f, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertVector(line.Direction, 1f, 0f, 0f);
            AssertPoint(line.ClosestPointToOrigin, 0f, 3f, 4f);
            Assert.That(line.Distance(new PointXYZ(-5f, 3f, 4f)), Is.Zero);
        });
    }

    [Test]
    public void Constructor_WithExtremeFiniteDirection_NormalizesWithoutOverflowOrUnderflow()
    {
        var largeDirectionLine = new Line(
            default(PointXYZ),
            new VectorXYZ(float.MaxValue, float.MaxValue, 0f));
        var smallDirectionLine = new Line(
            default(PointXYZ),
            new VectorXYZ(float.Epsilon, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(
                largeDirectionLine.Direction.Length,
                Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
            AssertVector(smallDirectionLine.Direction, 1f, 0f, 0f);
        });
    }

    [Test]
    public void Constructor_WithExtremeFinitePoints_NormalizesWithoutOverflow()
    {
        var line = new Line(
            new PointXYZ(-float.MaxValue, 0f, 0f),
            new PointXYZ(float.MaxValue, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertVector(line.Direction, 1f, 0f, 0f);
            AssertPoint(line.ClosestPointToOrigin, 0f, 0f, 0f);
        });
    }

    [Test]
    public void DefaultLine_RepresentsXAxis()
    {
        var line = default(Line);
        var sameLine = new Line(default(PointXYZ), new PointXYZ(1f, 0f, 0f));

        var projection = line.Project(new PointXYZ(3f, 5f, 12f));

        Assert.Multiple(() =>
        {
            Assert.That(line, Is.EqualTo(sameLine));
            AssertVector(line.Direction, 1f, 0f, 0f);
            AssertPoint(line.ClosestPointToOrigin, 0f, 0f, 0f);
            Assert.That(line.Distance(new PointXYZ(3f, 5f, 12f)), Is.EqualTo(13f));
            AssertPoint(projection.ProjectedPoint, 3f, 0f, 0f);
            Assert.That(projection.Distance, Is.EqualTo(13f));
        });
    }

    [Test]
    public void Line_ImplementsCurveContract()
    {
        ICurve curve = default(Line);

        Assert.That(curve, Is.InstanceOf<IPointDistanceProvider>());
    }

    [Test]
    public void Equals_WhenSameLineIsBuiltFromDifferentPointsAndDirections_ReturnsTrue()
    {
        var line = new Line(new PointXYZ(2f, 3f, 4f), new PointXYZ(4f, 3f, 4f));
        var sameLine = new Line(new PointXYZ(8f, 3f, 4f), new VectorXYZ(-5f, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(line.Equals(sameLine), Is.True);
            Assert.That(line.GetHashCode(), Is.EqualTo(sameLine.GetHashCode()));
            Assert.That(line == sameLine, Is.True);
            Assert.That(line != sameLine, Is.False);
        });
    }

    [Test]
    public void Project_WhenLineIsOblique_ReturnsClosestPoint()
    {
        var line = new Line(default(PointXYZ), new VectorXYZ(1f, 1f, 0f));

        var projection = line.Project(new PointXYZ(2f, 0f, 3f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 1f, 1f, 0f);
            Assert.That(projection.Distance, Is.EqualTo(MathF.Sqrt(11f)).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    [Test]
    public void Distance_WhenPointCoordinateIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Line).Distance(new PointXYZ(float.PositiveInfinity, 0f, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Project_WhenPointCoordinateIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Line).Project(new PointXYZ(0f, float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var line = new Line(new PointXYZ(0f, 1.5f, 2.25f), VectorXYZ.BasisX);

            Assert.That(line.ToString(), Is.EqualTo("((0, 1.5, 2.25) + t*(1, 0, 0))"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private static void AssertVector(VectorXYZ actual, float expectedX, float expectedY, float expectedZ)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Z, Is.EqualTo(expectedZ).Within(GeometryConstants.GeometryEpsilon));
        });
    }

    private static void AssertPoint(PointXYZ actual, float expectedX, float expectedY, float expectedZ)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
            Assert.That(actual.Z, Is.EqualTo(expectedZ).Within(GeometryConstants.GeometryEpsilon));
        });
    }
}
