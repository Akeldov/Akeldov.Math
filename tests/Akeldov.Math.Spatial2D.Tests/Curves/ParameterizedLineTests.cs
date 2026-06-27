using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class ParameterizedLineTests
{
    [Test]
    public void Constructor_WhenDirectionIsZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ParameterizedLine(new PointXY(0f, 0f), VectorXY.Zero));
    }

    [TestCase(float.PositiveInfinity, 0f, "origin")]
    [TestCase(0f, float.NegativeInfinity, "origin")]
    [TestCase(float.NaN, 0f, "direction")]
    [TestCase(0f, float.NaN, "direction")]
    [TestCase(float.PositiveInfinity, 0f, "direction")]
    [TestCase(0f, float.NegativeInfinity, "direction")]
    public void Constructor_WhenOriginOrDirectionCoordinateIsInvalid_Throws(float x, float y, string paramName)
    {
        PointXY origin = paramName == "origin" ? new PointXY(x, y) : new PointXY(0f, 0f);
        VectorXY direction = paramName == "direction" ? new VectorXY(x, y) : new VectorXY(1f, 0f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedLine(origin, direction));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Constructor_WhenReferencePointCoordinateIsInvalid_Throws()
    {
        var line = default(Line);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ParameterizedLine(line, new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("referencePoint"));
    }

    [Test]
    public void Constructor_WhenDirectionIsNotParallelToLine_Throws()
    {
        var line = new Line(new PointXY(0f, 0f), new PointXY(2f, 0f));

        Assert.Throws<ArgumentException>(() => new ParameterizedLine(line, new PointXY(0f, 0f), new VectorXY(0f, 1f)));
    }

    [Test]
    public void Constructor_WithOriginAndDirection_UsesOriginAndNormalizedDirection()
    {
        var line = new ParameterizedLine(new PointXY(2f, 3f), new VectorXY(2f, 0f));

        AssertVector(line.Origin, 2f, 3f);
        AssertVector(line.Direction, 1f, 0f);
        AssertVector(line.Line.ClosestPointToOrigin, 0f, 3f);
    }

    [Test]
    public void DefaultParameterizedLine_RepresentsHorizontalXAxis()
    {
        var line = default(ParameterizedLine);
        var sameLine = new ParameterizedLine(default(Line));

        Assert.That(line, Is.EqualTo(sameLine));
        Assert.That(line.Line, Is.EqualTo(default(Line)));
        AssertVector(line.Origin, 0f, 0f);
        AssertVector(line.Direction, 1f, 0f);
        AssertVector(line.Normal, 0f, 1f);

        var projection = line.ProjectWithParameter(new PointXY(3f, 4f));
        AssertVector(projection.ProjectedPoint, 3f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(3f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(4f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void RayIntersections_WithCustomGeometryEpsilon_WhenRayIsNearlyOnLine_ReturnsRayOrigin()
    {
        const float geometryEpsilon = 0.01f;
        var line = new ParameterizedLine(new PointXY(-5f, 0f), new VectorXY(5f, 0f));
        var ray = new Ray(new PointXY(2f, 0.005f));

        var defaultIntersections = line.GetRayIntersections(ray);
        var tolerantIntersections = line.GetRayIntersections(ray, geometryEpsilon);

        Assert.That(defaultIntersections, Is.Empty);
        Assert.That(tolerantIntersections, Has.Count.EqualTo(1));
        AssertVector(tolerantIntersections[0], 2f, 0.005f);
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void RayIntersections_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var line = default(ParameterizedLine);
        var ray = new Ray(new PointXY(0f, 0f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            line.GetRayIntersections(ray, geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    [Test]
    public void Constructor_WhenReferencePointModeIsGlobalZero_UsesClosestPointToGlobalOrigin()
    {
        var line = new ParameterizedLine(new PointXY(2f, 3f), new PointXY(4f, 3f), LineReferencePointMode.GlobalZero);

        AssertVector(line.Origin, 0f, 3f);
    }

    [Test]
    public void Constructor_WhenReferencePointModeIsPointA_UsesPointA()
    {
        var line = new ParameterizedLine(new PointXY(2f, 3f), new PointXY(4f, 3f), LineReferencePointMode.PointA);

        AssertVector(line.Origin, 2f, 3f);
    }

    [Test]
    public void Constructor_WhenReferencePointModeIsPointB_UsesPointB()
    {
        var line = new ParameterizedLine(new PointXY(2f, 3f), new PointXY(4f, 3f), LineReferencePointMode.PointB);

        AssertVector(line.Origin, 4f, 3f);
    }

    [Test]
    public void Constructor_WhenReferencePointModeIsMidpoint_UsesMidpoint()
    {
        var line = new ParameterizedLine(new PointXY(2f, 3f), new PointXY(4f, 3f), LineReferencePointMode.Midpoint);

        AssertVector(line.Origin, 3f, 3f);
    }

    [Test]
    public void ProjectWithParameter_WhenReferencePointIsProvided_MeasuresCurveCoordinateFromItsProjection()
    {
        var line = new ParameterizedLine(new PointXY(2f, 0f), new PointXY(4f, 0f), new PointXY(2f, 5f));

        var projection = line.ProjectWithParameter(new PointXY(0f, 0f));

        AssertVector(line.Origin, 2f, 0f);
        AssertVector(projection.ProjectedPoint, 0f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(-2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ProjectWithParameter_WhenDirectionIsReversed_MeasuresCurveCoordinateInReversedDirection()
    {
        var geometricLine = new Line(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var line = new ParameterizedLine(geometricLine, new PointXY(0f, 0f), new VectorXY(-1f, 0f));

        var projection = line.ProjectWithParameter(new PointXY(2f, 1f));

        AssertVector(line.Direction, -1f, 0f);
        AssertVector(projection.ProjectedPoint, 2f, 0f);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(-2f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Equals_WhenOriginDiffers_ReturnsFalse()
    {
        var geometricLine = new Line(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var first = new ParameterizedLine(geometricLine, new PointXY(0f, 0f));
        var second = new ParameterizedLine(geometricLine, new PointXY(2f, 0f));

        Assert.That(first.Equals(second), Is.False);
        Assert.That(first.HasSameGeometry(second), Is.True);
    }

    [Test]
    public void Equals_WhenDirectionDiffers_ReturnsFalse()
    {
        var geometricLine = new Line(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var first = new ParameterizedLine(geometricLine, new PointXY(0f, 0f), new VectorXY(1f, 0f));
        var second = new ParameterizedLine(geometricLine, new PointXY(0f, 0f), new VectorXY(-1f, 0f));

        Assert.That(first.Equals(second), Is.False);
        Assert.That(first.HasSameGeometry(second), Is.True);
    }

    [Test]
    public void ExplicitConversionToLine_ReturnsGeometricLine()
    {
        var geometricLine = new Line(new PointXY(0f, 0f), new PointXY(4f, 0f));
        var line = new ParameterizedLine(geometricLine, new PointXY(2f, 0f));

        Line converted = (Line)line;

        Assert.That(converted, Is.EqualTo(geometricLine));
        Assert.That(line.HasSameGeometry(converted), Is.True);
    }

    [Test]
    public void GetHalfPlaneSide_WhenPointIsLeftOfIncreasingDirection_ReturnsLeft()
    {
        var line = new ParameterizedLine(new PointXY(0f, 0f), new VectorXY(1f, 0f));

        HalfPlaneSide side = line.GetHalfPlaneSide(new PointXY(2f, 3f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.Left));
    }

    [Test]
    public void GetHalfPlaneSide_WhenPointIsRightOfIncreasingDirection_ReturnsRight()
    {
        var line = new ParameterizedLine(new PointXY(0f, 0f), new VectorXY(1f, 0f));

        HalfPlaneSide side = line.GetHalfPlaneSide(new PointXY(2f, -3f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.Right));
    }

    [Test]
    public void GetHalfPlaneSide_WhenDirectionIsReversed_UsesParameterizedDirection()
    {
        var line = new ParameterizedLine(new PointXY(0f, 0f), new VectorXY(-1f, 0f));

        HalfPlaneSide side = line.GetHalfPlaneSide(new PointXY(2f, 3f));

        Assert.That(side, Is.EqualTo(HalfPlaneSide.Right));
    }

    [Test]
    public void GetHalfPlaneSide_WhenPointIsOnLineWithinTolerance_ReturnsOnTheLine()
    {
        var line = new ParameterizedLine(new PointXY(0f, 0f), new VectorXY(1f, 0f));

        HalfPlaneSide side = line.GetHalfPlaneSide(new PointXY(2f, 0.005f), geometryEpsilon: 0.01f);

        Assert.That(side, Is.EqualTo(HalfPlaneSide.OnTheLine));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void GetHalfPlaneSide_WhenPointCoordinateIsInvalid_Throws(float x, float y)
    {
        var line = default(ParameterizedLine);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            line.GetHalfPlaneSide(new PointXY(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void GetHalfPlaneSide_WhenGeometryEpsilonIsInvalid_Throws(float geometryEpsilon)
    {
        var line = default(ParameterizedLine);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            line.GetHalfPlaneSide(new PointXY(0f, 0f), geometryEpsilon));

        Assert.That(exception!.ParamName, Is.EqualTo("geometryEpsilon"));
    }

    private static void AssertVector(VectorXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
