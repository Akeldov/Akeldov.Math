using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes.Tests.Geometry.Polyhexes;

public class PolyhexGeometryTests
{
    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Constructor_WhenRadiusIsInvalid_Throws(float radius)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PolyhexGeometry(new[,] { { true } }, radius));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    [Test]
    public void Constructor_DerivesApothemFromRadius()
    {
        var geometry = new PolyhexGeometry(new[,] { { true } }, 2f);

        Assert.That(geometry.HexRadius, Is.EqualTo(2f));
        Assert.That(geometry.HexApothem, Is.EqualTo(2f.ConvertHexRadiusToApothem()));
    }
}
