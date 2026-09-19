namespace Akeldov.Math.Spatial3D.Tests.Vectors;

public class VectorXYZIntTests
{
    private const float Tolerance = 1e-6f;

    [Test]
    public void BasisVectors_ReturnStandardCartesianBasis()
    {
        Assert.Multiple(() =>
        {
            Assert.That(VectorXYZInt.BasisX, Is.EqualTo(new VectorXYZInt(1, 0, 0)));
            Assert.That(VectorXYZInt.BasisY, Is.EqualTo(new VectorXYZInt(0, 1, 0)));
            Assert.That(VectorXYZInt.BasisZ, Is.EqualTo(new VectorXYZInt(0, 0, 1)));
        });
    }

    [Test]
    public void Length_WhenSquaredComponentsExceedIntRange_ReturnsFloatLength()
    {
        var vector = new VectorXYZInt(50_000, 0, 0);

        Assert.That(vector.Length, Is.EqualTo(50_000f).Within(Tolerance));
    }

    [Test]
    public void ArithmeticOperators_OperateComponentByComponent()
    {
        var left = new VectorXYZInt(1, 2, 3);
        var right = new VectorXYZInt(4, 6, 8);

        Assert.Multiple(() =>
        {
            Assert.That(left + right, Is.EqualTo(new VectorXYZInt(5, 8, 11)));
            Assert.That(right - left, Is.EqualTo(new VectorXYZInt(3, 4, 5)));
            Assert.That(left * 2, Is.EqualTo(new VectorXYZInt(2, 4, 6)));
            Assert.That(2 * left, Is.EqualTo(new VectorXYZInt(2, 4, 6)));
            Assert.That(right / 2, Is.EqualTo(new VectorXYZInt(2, 3, 4)));
            Assert.That(left * 0.5f, Is.EqualTo(new VectorXYZ(0.5f, 1f, 1.5f)));
            Assert.That(0.5f * left, Is.EqualTo(new VectorXYZ(0.5f, 1f, 1.5f)));
        });
    }

    [Test]
    public void Division_WhenScalarIsZero_ThrowsDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => _ = VectorXYZInt.One / 0);
    }

    [Test]
    public void Conversions_ConvertBetweenIntegerAndFloatingPointVectors()
    {
        VectorXYZ floatingPoint = new VectorXYZInt(1, -2, 3);
        var integer = (VectorXYZInt)new VectorXYZ(1.9f, -2.9f, 3.1f);

        Assert.Multiple(() =>
        {
            Assert.That(floatingPoint, Is.EqualTo(new VectorXYZ(1f, -2f, 3f)));
            Assert.That(integer, Is.EqualTo(new VectorXYZInt(1, -2, 3)));
        });
    }

    [Test]
    public void Equality_UsesComponentEquality()
    {
        var vector = new VectorXYZInt(1, 2, 3);
        var equal = new VectorXYZInt(1, 2, 3);
        var different = new VectorXYZInt(1, 2, 4);

        Assert.Multiple(() =>
        {
            Assert.That(vector.Equals(equal), Is.True);
            Assert.That(vector == equal, Is.True);
            Assert.That(vector != different, Is.True);
            Assert.That(vector.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
        });
    }

    [Test]
    public void Deconstruct_ReturnsComponents()
    {
        var (x, y, z) = new VectorXYZInt(1, 2, 3);

        Assert.Multiple(() =>
        {
            Assert.That(x, Is.EqualTo(1));
            Assert.That(y, Is.EqualTo(2));
            Assert.That(z, Is.EqualTo(3));
        });
    }

    [Test]
    public void ToString_ReturnsComponents()
    {
        Assert.That(new VectorXYZInt(1, -2, 3).ToString(), Is.EqualTo("(1, -2, 3)"));
    }
}
