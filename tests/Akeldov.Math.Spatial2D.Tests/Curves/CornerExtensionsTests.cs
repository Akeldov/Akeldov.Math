using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class CornerExtensionsTests
{
    [Test]
    public void CreateAngleBisector_WhenAngleIsRight_CreatesRayFromVertexAlongInternalBisector()
    {
        var a = new PointXY(4f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(0f, 7f);

        var ray = CornerExtensions.CreateAngleBisector(a, b, c);

        float expectedDirectionComponent = MathF.Sqrt(0.5f);
        AssertVector(ray.Origin, 0f, 0f);
        AssertVector(ray.Direction, expectedDirectionComponent, expectedDirectionComponent);
        Assert.That(ray.Angle, Is.EqualTo(MathF.PI / 4f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void CreateCornerTangentCircle_WhenAngleIsRight_CreatesCircleTangentToSides()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(0f, 1f);

        var circle = CornerExtensions.CreateCornerTangentCircle(a, b, c, 2f);

        AssertVector(circle.Center, 2f, 2f);
        Assert.That(circle.Radius, Is.EqualTo(2f));
        Assert.That(new Line(b, a).Distance(circle.Center), Is.EqualTo(circle.Radius).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(new Line(b, c).Distance(circle.Center), Is.EqualTo(circle.Radius).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void CreateFilletArc_WhenAngleIsRight_CreatesArcTangentToSides()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(0f, 1f);

        var arc = CornerExtensions.CreateFilletArc(a, b, c, 2f);

        AssertVector(arc.Center, 2f, 2f);
        Assert.That(arc.Radius, Is.EqualTo(2f));
        Assert.That(new Line(b, a).Distance(arc.Center), Is.EqualTo(arc.Radius).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(new Line(b, c).Distance(arc.Center), Is.EqualTo(arc.Radius).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void CreateCurvesInAngle_WhenAngleIsStraight_Throws()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(-1f, 0f);

        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateCornerTangentCircle(a, b, c, 1f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateFilletArc(a, b, c, 1f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateAngleBisector(a, b, c));
    }

    [Test]
    public void CreateCurvesInAngle_WhenAngleIsZero_Throws()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(2f, 0f);

        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateCornerTangentCircle(a, b, c, 1f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateFilletArc(a, b, c, 1f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateAngleBisector(a, b, c));
    }

    [Test]
    public void CreateCurvesInAngle_WhenSideEndpointEqualsVertex_Throws()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(0f, 1f);

        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateCornerTangentCircle(b, b, c, 1f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateFilletArc(a, b, b, 1f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateAngleBisector(b, b, c));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateAngleBisector(a, b, b));
    }

    [Test]
    public void CreateCurvesInAngle_WhenAngleIsAlmostZero_DoesNotThrow()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(1f, 0.01f);

        Assert.DoesNotThrow(() => CornerExtensions.CreateCornerTangentCircle(a, b, c, 1f));
        Assert.DoesNotThrow(() => CornerExtensions.CreateFilletArc(a, b, c, 1f));
    }

    [Test]
    public void CreateCurvesInAngle_WithEpsilon_WhenAngleIsWithinTolerance_Throws()
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(1f, 0.01f);

        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateCornerTangentCircle(a, b, c, 1f, epsilon: 0.02f));
        Assert.Throws<ArgumentException>(() => CornerExtensions.CreateFilletArc(a, b, c, 1f, epsilon: 0.02f));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void CreateCurvesInAngle_WhenRadiusIsInvalid_Throws(float radius)
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(0f, 1f);

        var incircleException = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CornerExtensions.CreateCornerTangentCircle(a, b, c, radius));
        var arcException = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CornerExtensions.CreateFilletArc(a, b, c, radius));

        Assert.That(incircleException!.ParamName, Is.EqualTo("radius"));
        Assert.That(arcException!.ParamName, Is.EqualTo("radius"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void CreateCurvesInAngle_WhenEpsilonIsInvalid_Throws(float epsilon)
    {
        var a = new PointXY(1f, 0f);
        var b = new PointXY(0f, 0f);
        var c = new PointXY(0f, 1f);

        var incircleException = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CornerExtensions.CreateCornerTangentCircle(a, b, c, 1f, epsilon));
        var arcException = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CornerExtensions.CreateFilletArc(a, b, c, 1f, epsilon));

        Assert.That(incircleException!.ParamName, Is.EqualTo("epsilon"));
        Assert.That(arcException!.ParamName, Is.EqualTo("epsilon"));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }

    private static void AssertVector(VectorXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
