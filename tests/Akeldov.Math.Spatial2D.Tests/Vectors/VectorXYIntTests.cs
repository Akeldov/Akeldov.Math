namespace Akeldov.Math.Spatial2D.Tests.Vectors;

public class VectorXYIntTests
{
    [Test]
    public void BasisVectors_ReturnStandardCartesianBasis()
    {
        Assert.Multiple(() =>
        {
            Assert.That(VectorXYInt.BasisX, Is.EqualTo(new VectorXYInt(1, 0)));
            Assert.That(VectorXYInt.BasisY, Is.EqualTo(new VectorXYInt(0, 1)));
        });
    }

    [Test]
    public void Length_WhenSquaredComponentsExceedIntRange_ReturnsFloatLength()
    {
        var vector = new VectorXYInt(50_000, 0);

        Assert.That(vector.Length, Is.EqualTo(50_000f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void HadamardDivide_WithIntegerDivisors_ReturnsFractionalQuotients()
    {
        var dividend = new VectorXYInt(1, 3);
        var divisor = new VectorXYInt(2, 2);

        var quotient = dividend.HadamardDivide(divisor);

        Assert.That(quotient.X, Is.EqualTo(0.5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(quotient.Y, Is.EqualTo(1.5f).Within(GeometryConstants.GeometryEpsilon));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void Rotate_WhenAngleIsInvalid_Throws(float angle)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new VectorXYInt(1, 0).Rotate(angle));

        Assert.That(exception!.ParamName, Is.EqualTo("angle"));
    }

    [Test]
    public void Average_WhenSequenceIsEmpty_ThrowsInvalidOperationException()
    {
        var vectors = Array.Empty<VectorXY>();

        Assert.Throws<InvalidOperationException>(() => vectors.Average());
    }
}
