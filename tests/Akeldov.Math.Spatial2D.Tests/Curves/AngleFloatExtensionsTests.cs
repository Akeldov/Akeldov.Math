using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class AngleFloatExtensionsTests
{
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void NormalizeAngleRad_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => angle.NormalizeAngleRad());

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [TestCase(float.NaN, 0f, 1f, "angle")]
    [TestCase(0f, float.PositiveInfinity, 1f, "startAngle")]
    [TestCase(0f, 0f, float.NegativeInfinity, "endAngle")]
    public void IsAngleWithinArc_WhenAnyAngleIsInvalid_Throws(
        float angle,
        float startAngle,
        float endAngle,
        string expectedParamName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            angle.IsAngleWithinArc(startAngle, endAngle));

        Assert.That(exception!.ParamName, Is.EqualTo(expectedParamName));
    }
}
