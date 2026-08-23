using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class FloatCurveInfluenceSourceIntersectionTests
{
    [Test]
    public void GetPointIntersections_DelegatesToUnderlyingCurve()
    {
        var curve = new ParameterizedSegment(
            new PointXY(4f, -1f),
            new PointXY(4f, 1f));
        var source = new FloatCurveInfluenceSource(1f, curve, 0f);
        var ray = new Ray(new PointXY(0f, 0f));

        var intersections = source.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(1));
        Assert.That(intersections[0].X, Is.EqualTo(4f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(intersections[0].Y, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }
}
