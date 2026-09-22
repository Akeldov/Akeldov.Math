using System.Globalization;
using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class ParameterizedLineTests
{
    [Test]
    public void Constructor_WhenDirectionIsZero_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new ParameterizedLine(default(PointXYZ), VectorXYZ.Zero));

        Assert.That(exception!.ParamName, Is.EqualTo("direction"));
    }

    [TestCase(float.PositiveInfinity, 0f, 0f, "origin")]
    [TestCase(0f, float.NegativeInfinity, 0f, "origin")]
    [TestCase(0f, 0f, float.PositiveInfinity, "origin")]
    [TestCase(float.NaN, 0f, 0f, "direction")]
    [TestCase(0f, float.PositiveInfinity, 0f, "direction")]
    [TestCase(0f, 0f, float.NegativeInfinity, "direction")]
    public void Constructor_WhenOriginOrDirectionComponentIsInvalid_Throws(
        float x,
        float y,
        float z,
        string paramName)
    {
        PointXYZ origin = paramName == "origin" ? new PointXYZ(x, y, z) : default;
        VectorXYZ direction = paramName == "direction" ? new VectorXYZ(x, y, z) : VectorXYZ.BasisX;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedLine(origin, direction));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WhenReferencePointCoordinateIsInvalid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedLine(default(Line), new PointXYZ(float.PositiveInfinity, 0f, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("referencePoint"));
    }

    [Test]
    public void Constructor_WhenDirectionIsNotParallelToLine_Throws()
    {
        var line = new Line(default(PointXYZ), VectorXYZ.BasisX);

        var exception = Assert.Throws<ArgumentException>(() =>
            new ParameterizedLine(line, default, VectorXYZ.BasisY));

        Assert.That(exception!.ParamName, Is.EqualTo("direction"));
    }

    [Test]
    public void Constructor_WithOriginAndDirection_UsesOriginAndNormalizedDirection()
    {
        var line = new ParameterizedLine(
            new PointXYZ(2f, 3f, 4f),
            new VectorXYZ(-2f, 0f, 0f));

        Assert.Multiple(() =>
        {
            AssertPoint(line.Origin, 2f, 3f, 4f);
            AssertVector(line.Direction, -1f, 0f, 0f);
            AssertPoint(line.ClosestPointToOrigin, 0f, 3f, 4f);
        });
    }

    [Test]
    public void Constructor_WithExtremeFiniteDirection_NormalizesWithoutOverflowOrUnderflow()
    {
        var largeDirectionLine = new ParameterizedLine(
            default(PointXYZ),
            new VectorXYZ(-float.MaxValue, float.MaxValue, 0f));
        var smallDirectionLine = new ParameterizedLine(
            default(PointXYZ),
            new VectorXYZ(-float.Epsilon, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(
                largeDirectionLine.Direction.Length,
                Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
            AssertVector(smallDirectionLine.Direction, -1f, 0f, 0f);
        });
    }

    [Test]
    public void DefaultParameterizedLine_RepresentsDirectedXAxis()
    {
        var line = default(ParameterizedLine);
        var sameLine = new ParameterizedLine(default(Line));

        ParameterizedCurveProjection projection = line.ProjectWithParameter(new PointXYZ(3f, 5f, 12f));

        Assert.Multiple(() =>
        {
            Assert.That(line, Is.EqualTo(sameLine));
            Assert.That(line.Line, Is.EqualTo(default(Line)));
            AssertPoint(line.Origin, 0f, 0f, 0f);
            AssertVector(line.Direction, 1f, 0f, 0f);
            AssertPoint(projection.ProjectedPoint, 3f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(3f));
            Assert.That(projection.Distance, Is.EqualTo(13f));
        });
    }

    [TestCase(LineReferencePointMode.GlobalZero, 0f)]
    [TestCase(LineReferencePointMode.PointA, 2f)]
    [TestCase(LineReferencePointMode.PointB, 4f)]
    [TestCase(LineReferencePointMode.Midpoint, 3f)]
    public void Constructor_WithReferencePointMode_SelectsExpectedOrigin(
        LineReferencePointMode referencePointMode,
        float expectedX)
    {
        var line = new ParameterizedLine(
            new PointXYZ(2f, 3f, 4f),
            new PointXYZ(4f, 3f, 4f),
            referencePointMode);

        AssertPoint(line.Origin, expectedX, 3f, 4f);
    }

    [Test]
    public void ProjectWithParameter_WhenReferencePointIsProvided_MeasuresFromItsProjection()
    {
        var line = new ParameterizedLine(
            new PointXYZ(2f, 0f, 0f),
            new PointXYZ(4f, 0f, 0f),
            new PointXYZ(2f, 5f, 6f));

        ParameterizedCurveProjection projection = line.ProjectWithParameter(default);

        Assert.Multiple(() =>
        {
            AssertPoint(line.Origin, 2f, 0f, 0f);
            AssertPoint(projection.ProjectedPoint, 0f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(-2f));
            Assert.That(projection.Distance, Is.Zero);
        });
    }

    [Test]
    public void ProjectWithParameter_WhenDirectionIsReversed_MeasuresInReversedDirection()
    {
        var geometricLine = new Line(default(PointXYZ), VectorXYZ.BasisX);
        var line = new ParameterizedLine(geometricLine, default, new VectorXYZ(-2f, 0f, 0f));

        ParameterizedCurveProjection projection = line.ProjectWithParameter(new PointXYZ(2f, 0f, 1f));

        Assert.Multiple(() =>
        {
            AssertVector(line.Direction, -1f, 0f, 0f);
            AssertPoint(projection.ProjectedPoint, 2f, 0f, 0f);
            Assert.That(projection.CurveCoordinate, Is.EqualTo(-2f));
            Assert.That(projection.Distance, Is.EqualTo(1f));
        });
    }

    [Test]
    public void GetPoint_ReturnsPointAtSignedCoordinate()
    {
        var line = new ParameterizedLine(
            new PointXYZ(2f, 3f, 4f),
            new VectorXYZ(0f, -2f, 0f));

        PointXYZ point = line.GetPoint(-5f);

        AssertPoint(point, 2f, 8f, 4f);
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(ParameterizedLine).GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void ParameterizedLine_ImplementsParameterizedCurveContract()
    {
        IParameterizedCurve curve = default(ParameterizedLine);

        Assert.That(curve, Is.InstanceOf<IPointDistanceProvider>());
    }

    [Test]
    public void Equals_WhenOriginDiffers_ReturnsFalseButGeometryMatches()
    {
        var geometricLine = new Line(default(PointXYZ), VectorXYZ.BasisX);
        var first = new ParameterizedLine(geometricLine, default);
        var second = new ParameterizedLine(geometricLine, new PointXYZ(2f, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(first.Equals(second), Is.False);
            Assert.That(first == second, Is.False);
            Assert.That(first != second, Is.True);
            Assert.That(first.HasSameGeometry(second), Is.True);
        });
    }

    [Test]
    public void Equals_WhenDirectionDiffers_ReturnsFalseButGeometryMatches()
    {
        var geometricLine = new Line(default(PointXYZ), VectorXYZ.BasisX);
        var first = new ParameterizedLine(geometricLine, default, VectorXYZ.BasisX);
        var second = new ParameterizedLine(geometricLine, default, new VectorXYZ(-1f, 0f, 0f));

        Assert.Multiple(() =>
        {
            Assert.That(first.Equals(second), Is.False);
            Assert.That(first.HasSameGeometry(second), Is.True);
        });
    }

    [Test]
    public void ExplicitConversionToLine_ReturnsGeometricLine()
    {
        var geometricLine = new Line(new PointXYZ(0f, 2f, 3f), VectorXYZ.BasisX);
        var line = new ParameterizedLine(geometricLine, new PointXYZ(2f, 2f, 3f));

        Line converted = (Line)line;

        Assert.Multiple(() =>
        {
            Assert.That(converted, Is.EqualTo(geometricLine));
            Assert.That(line.HasSameGeometry(converted), Is.True);
        });
    }

    [Test]
    public void DistanceAndProject_DelegateToGeometricLine()
    {
        var line = new ParameterizedLine(
            new PointXYZ(0f, 2f, 3f),
            new VectorXYZ(-1f, 0f, 0f));

        CurveProjection projection = line.Project(new PointXYZ(5f, 5f, 7f));

        Assert.Multiple(() =>
        {
            AssertPoint(projection.ProjectedPoint, 5f, 2f, 3f);
            Assert.That(projection.Distance, Is.EqualTo(5f));
            Assert.That(line.Distance(new PointXYZ(5f, 5f, 7f)), Is.EqualTo(5f));
        });
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var line = new ParameterizedLine(
                new PointXYZ(1.5f, 2.25f, 3.75f),
                VectorXYZ.BasisX);

            Assert.That(line.ToString(), Is.EqualTo("((1.5, 2.25, 3.75) + t*(1, 0, 0))"));
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
