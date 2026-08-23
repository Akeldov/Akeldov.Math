using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class FloatCurveInfluenceSourceCapabilityTests
{
    [Test]
    public void Constructor_DoesNotExposeContourCrossingCapability()
    {
        var curve = new ParameterizedSegment(
            new PointXY(4f, -1f),
            new PointXY(4f, 1f));
        var source = new FloatCurveInfluenceSource(1f, curve, 0f);
        Assert.That(source, Is.Not.InstanceOf<IRightwardCrossingProvider>());
    }
}
