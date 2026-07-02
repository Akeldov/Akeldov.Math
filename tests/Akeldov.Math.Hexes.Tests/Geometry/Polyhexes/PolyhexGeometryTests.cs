using Akeldov.Math.Hexes.Geometry;

namespace Akeldov.Math.Hexes.Tests.Geometry.Polyhexes;

public class PolyhexGeometryTests
{
    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void Constructor_WhenApothemIsInvalid_Throws(float apothem)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PolyhexGeometry(new[,] { { true } }, apothem));

        Assert.That(exception!.ParamName, Is.EqualTo("apothem"));
    }
}
