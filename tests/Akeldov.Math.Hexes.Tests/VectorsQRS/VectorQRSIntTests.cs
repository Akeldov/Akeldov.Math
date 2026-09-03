using Akeldov.Math.Hexes.Vectors.QRS;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class VectorQRSIntTests
{
    [Test]
    public void Constructor_WithQR_SetsSAsNegativeSum()
    {
        var vector = new VectorQRSInt(2, -5);

        Assert.Multiple(() =>
        {
            Assert.That(vector.Q, Is.EqualTo(2));
            Assert.That(vector.R, Is.EqualTo(-5));
            Assert.That(vector.S, Is.EqualTo(3));
        });
    }

    [Test]
    public void Constructor_WithQRS_AcceptsZeroSum()
    {
        var vector = new VectorQRSInt(2, -5, 3);

        Assert.That(vector, Is.EqualTo(new VectorQRSInt(2, -5)));
    }

    [Test]
    public void Constructor_WithQRS_ThrowsWhenSumIsNotZero()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRSInt(2, -5, 2));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ParamName, Is.EqualTo("s"));
            Assert.That(exception.ActualValue, Is.EqualTo(2));
            Assert.That(exception.Message, Does.Contain("q (2), r (-5), and s (2)"));
        });
    }

    [Test]
    public void Constructor_WithQR_ThrowsWhenDerivedSDoesNotFitInt32()
    {
        var lowException = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new VectorQRSInt(int.MinValue, 0));
        var highException = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = new VectorQRSInt(int.MaxValue, 2));

        Assert.Multiple(() =>
        {
            Assert.That(lowException!.ParamName, Is.EqualTo("r"));
            Assert.That(lowException.ActualValue, Is.EqualTo(0));
            Assert.That(lowException.Message, Does.Contain($"q ({int.MinValue}) and r (0)"));
            Assert.That(highException!.ParamName, Is.EqualTo("r"));
            Assert.That(highException.ActualValue, Is.EqualTo(2));
            Assert.That(highException.Message, Does.Contain($"q ({int.MaxValue}) and r (2)"));
        });
    }

    [Test]
    public void Constructor_WithQRS_UsesWideSumForValidation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRSInt(int.MaxValue, int.MaxValue, 2));
    }

    [Test]
    public void StaticVectors_ReturnExpectedValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(VectorQRSInt.Zero, Is.EqualTo(new VectorQRSInt(0, 0)));
            Assert.That(VectorQRSInt.One, Is.EqualTo(new VectorQRSInt(1, 1)));
        });
    }

    [Test]
    public void EqualityMembers_CompareQAndR()
    {
        var left = new VectorQRSInt(2, -3);
        var same = new VectorQRSInt(2, -3);
        var different = new VectorQRSInt(2, -4);

        Assert.Multiple(() =>
        {
            Assert.That(left.Equals(same), Is.True);
            Assert.That(left.Equals((object)same), Is.True);
            Assert.That(left.Equals((object?)null), Is.False);
            Assert.That(left == same, Is.True);
            Assert.That(left != different, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(same.GetHashCode()));
        });
    }

    [Test]
    public void ToString_ReturnsQAndR()
    {
        Assert.That(new VectorQRSInt(2, -4).ToString(), Is.EqualTo("(2, -4)"));
    }

    [Test]
    public void Deconstruct_ReturnsQAndR()
    {
        var (q, r) = new VectorQRSInt(2, -4);

        Assert.Multiple(() =>
        {
            Assert.That(q, Is.EqualTo(2));
            Assert.That(r, Is.EqualTo(-4));
        });
    }

    [Test]
    public void Operators_ReturnExpectedVectors()
    {
        var left = new VectorQRSInt(5, -7);
        var right = new VectorQRSInt(-2, 3);

        Assert.Multiple(() =>
        {
            Assert.That(left + right, Is.EqualTo(new VectorQRSInt(3, -4)));
            Assert.That(left - right, Is.EqualTo(new VectorQRSInt(7, -10)));
            Assert.That(left * 2, Is.EqualTo(new VectorQRSInt(10, -14)));
            Assert.That(2 * left, Is.EqualTo(new VectorQRSInt(10, -14)));
            Assert.That(left / 2, Is.EqualTo(new VectorQRSInt(2, -3)));
        });
    }

    [Test]
    public void Operators_WhenComponentArithmeticOverflows_Throw()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = new VectorQRSInt(int.MaxValue, -1) + new VectorQRSInt(1, 0));
            Assert.Throws<OverflowException>(() => _ = new VectorQRSInt(int.MinValue + 1, 0) - new VectorQRSInt(2, 0));
            Assert.Throws<OverflowException>(() => _ = new VectorQRSInt((int.MaxValue / 2) + 1, 0) * 2);
            Assert.Throws<OverflowException>(() => _ = 2 * new VectorQRSInt((int.MaxValue / 2) + 1, 0));
        });
    }

    [Test]
    public void Division_ThrowsWhenScalarIsZero()
    {
        Assert.Throws<DivideByZeroException>(() => _ = new VectorQRSInt(1, 2) / 0);
    }
}
