using System.Globalization;

namespace Akeldov.Math.Spatial3D.Tests.Vectors;

public class VectorXYZTests
{
    private const float Tolerance = 1e-6f;

    [Test]
    public void BasisVectors_ReturnStandardCartesianBasis()
    {
        Assert.Multiple(() =>
        {
            Assert.That(VectorXYZ.BasisX, Is.EqualTo(new VectorXYZ(1f, 0f, 0f)));
            Assert.That(VectorXYZ.BasisY, Is.EqualTo(new VectorXYZ(0f, 1f, 0f)));
            Assert.That(VectorXYZ.BasisZ, Is.EqualTo(new VectorXYZ(0f, 0f, 1f)));
        });
    }

    [Test]
    public void Length_ReturnsEuclideanLength()
    {
        var vector = new VectorXYZ(2f, 3f, 6f);

        Assert.Multiple(() =>
        {
            Assert.That(vector.Length, Is.EqualTo(7f).Within(Tolerance));
            Assert.That(vector.SquaredLength, Is.EqualTo(49f));
        });
    }

    [Test]
    public void Normalize_WhenVectorHasLength_ReturnsUnitVector()
    {
        var vector = new VectorXYZ(2f, 3f, 6f);

        var normalized = vector.Normalize();

        Assert.That(normalized.Length, Is.EqualTo(1f).Within(Tolerance));
        Assert.That(normalized, Is.EqualTo(new VectorXYZ(2f / 7f, 3f / 7f, 6f / 7f)));
    }

    [Test]
    public void Normalize_WhenVectorIsZero_ReturnsZero()
    {
        Assert.That(VectorXYZ.Zero.Normalize(), Is.EqualTo(VectorXYZ.Zero));
    }

    [Test]
    public void Dot_ReturnsScalarProduct()
    {
        var left = new VectorXYZ(1f, 2f, 3f);
        var right = new VectorXYZ(4f, -5f, 6f);

        Assert.That(VectorXYZ.Dot(left, right), Is.EqualTo(12f));
    }

    [Test]
    public void Cross_ReturnsRightHandedVectorProduct()
    {
        var cross = VectorXYZ.Cross(VectorXYZ.BasisX, VectorXYZ.BasisY);

        Assert.That(cross, Is.EqualTo(VectorXYZ.BasisZ));
    }

    [Test]
    public void ArithmeticOperators_OperateComponentByComponent()
    {
        var left = new VectorXYZ(1f, 2f, 3f);
        var right = new VectorXYZ(4f, 5f, 6f);

        Assert.Multiple(() =>
        {
            Assert.That(left + right, Is.EqualTo(new VectorXYZ(5f, 7f, 9f)));
            Assert.That(right - left, Is.EqualTo(new VectorXYZ(3f, 3f, 3f)));
            Assert.That(left * 2f, Is.EqualTo(new VectorXYZ(2f, 4f, 6f)));
            Assert.That(2f * left, Is.EqualTo(new VectorXYZ(2f, 4f, 6f)));
            Assert.That(left * 2, Is.EqualTo(new VectorXYZ(2f, 4f, 6f)));
            Assert.That(2 * left, Is.EqualTo(new VectorXYZ(2f, 4f, 6f)));
            Assert.That(right / 2f, Is.EqualTo(new VectorXYZ(2f, 2.5f, 3f)));
        });
    }

    [Test]
    public void Equality_UsesExactComponentEquality()
    {
        var vector = new VectorXYZ(1f, 2f, 3f);
        var equal = new VectorXYZ(1f, 2f, 3f);
        var different = new VectorXYZ(1f, 2f, 4f);

        Assert.Multiple(() =>
        {
            Assert.That(vector.Equals(equal), Is.True);
            Assert.That(vector == equal, Is.True);
            Assert.That(vector != different, Is.True);
            Assert.That(vector.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
        });
    }

    [TestCase(float.NaN, 0f, 0f)]
    [TestCase(0f, float.PositiveInfinity, 0f)]
    [TestCase(0f, 0f, float.NegativeInfinity)]
    public void IsFinite_WhenComponentIsNaNOrInfinity_ReturnsFalse(float x, float y, float z)
    {
        Assert.That(new VectorXYZ(x, y, z).IsFinite, Is.False);
    }

    [Test]
    public void Deconstruct_ReturnsComponents()
    {
        var (x, y, z) = new VectorXYZ(1f, 2f, 3f);

        Assert.Multiple(() =>
        {
            Assert.That(x, Is.EqualTo(1f));
            Assert.That(y, Is.EqualTo(2f));
            Assert.That(z, Is.EqualTo(3f));
        });
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            Assert.That(new VectorXYZ(1.5f, 2.25f, -3.75f).ToString(), Is.EqualTo("(1.5, 2.25, -3.75)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
