using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class CoordinatesConversionTests
{
    [Test]
    public void ToNormalizedAxial_DividesByHexRadius()
    {
        var normalized = new VectorQRS(6f, -9f).ToNormalizedAxial(3f);

        Assert.That(normalized, Is.EqualTo(new VectorQRS(2f, -3f)));
    }

    [Test]
    public void ToNormalizedAxial_ThrowsWhenHexRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(1f, 2f).ToNormalizedAxial(0f));
    }

    [Test]
    public void ToQRSIndex_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new VectorXYInt(1, 2).ToQRSIndex((Layout)42));
    }

    [Test]
    public void ToXYIndex_ThrowsForInvalidLayout()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new VectorQRSInt(1, 2).ToXYIndex((Layout)42));
    }
}
