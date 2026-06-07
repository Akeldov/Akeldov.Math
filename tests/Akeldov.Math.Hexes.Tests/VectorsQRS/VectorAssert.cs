using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

internal static class VectorAssert
{
    public const float Epsilon = 0.0001f;

    public static void AreEqual(VectorQRS actual, float expectedQ, float expectedR)
    {
        Assert.That(actual.Q, Is.EqualTo(expectedQ).Within(Epsilon));
        Assert.That(actual.R, Is.EqualTo(expectedR).Within(Epsilon));
        Assert.That(actual.S, Is.EqualTo(-expectedQ - expectedR).Within(Epsilon));
    }

    public static void AreEqual(VectorXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(Epsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(Epsilon));
    }

    public static void AreEqual(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(Epsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(Epsilon));
    }
}
