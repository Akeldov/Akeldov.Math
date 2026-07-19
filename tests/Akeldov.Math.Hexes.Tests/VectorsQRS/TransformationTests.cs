using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class TransformationTests
{
    [TestCase(SixfoldAngle.Deg0, 2f, -5f)]
    [TestCase(SixfoldAngle.Deg60, 5f, -3f)]
    [TestCase(SixfoldAngle.Deg120, 3f, 2f)]
    [TestCase(SixfoldAngle.Deg180, -2f, 5f)]
    [TestCase(SixfoldAngle.Deg240, -5f, 3f)]
    [TestCase(SixfoldAngle.Deg300, -3f, -2f)]
    public void VectorQRS_RotateBySixfoldAngle_ReturnsExpectedVector(
        SixfoldAngle angle,
        float expectedQ,
        float expectedR)
    {
        var actual = new VectorQRS(2f, -5f).Rotate(angle);

        VectorAssert.AreEqual(actual, expectedQ, expectedR);
    }

    [Test]
    public void VectorQRS_RotateByRadians_UsesAxialBasis()
    {
        var actual = new VectorQRS(2f, 3f).Rotate(MathF.PI / 2f);

        VectorAssert.AreEqual(actual, -4.618802f, 4.041452f);
    }

    [TestCase(SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg60)]
    [TestCase(SixfoldAngle.Deg120)]
    [TestCase(SixfoldAngle.Deg180)]
    [TestCase(SixfoldAngle.Deg240)]
    [TestCase(SixfoldAngle.Deg300)]
    public void VectorQRS_RotateByRadians_MatchesSixfoldRotation(SixfoldAngle angle)
    {
        var point = new VectorQRS(2f, -5f);

        var expected = point.Rotate(angle);
        var actual = point.Rotate(angle.AsFloatRadians());

        VectorAssert.AreEqual(actual, expected.Q, expected.R);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void VectorQRS_RotateByRadians_PreservesWorldSpaceLength(Layout layout)
    {
        var point = new VectorQRS(2f, -5f);

        float expectedLength = point.ToVectorXY(layout).Length;
        float actualLength = point.Rotate(0.731f).ToVectorXY(layout).Length;

        Assert.That(actualLength, Is.EqualTo(expectedLength).Within(VectorAssert.Epsilon));
    }

    [Test]
    public void VectorQRS_RotateByRadians_WhenArgumentsAreNotFinite_Throws()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new VectorQRS(float.NaN, 0f).Rotate(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new VectorQRS(float.MaxValue, float.MaxValue).Rotate(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new VectorQRS(1f, 2f).Rotate(float.PositiveInfinity));
        });
    }

    [TestCase(SixfoldAngle.Deg0, 2, -5)]
    [TestCase(SixfoldAngle.Deg60, 5, -3)]
    [TestCase(SixfoldAngle.Deg120, 3, 2)]
    [TestCase(SixfoldAngle.Deg180, -2, 5)]
    [TestCase(SixfoldAngle.Deg240, -5, 3)]
    [TestCase(SixfoldAngle.Deg300, -3, -2)]
    public void VectorQRSInt_RotateBySixfoldAngle_ReturnsExpectedVector(
        SixfoldAngle angle,
        int expectedQ,
        int expectedR)
    {
        var actual = new VectorQRSInt(2, -5).Rotate(angle);

        Assert.That(actual, Is.EqualTo(new VectorQRSInt(expectedQ, expectedR)));
    }

    [Test]
    public void VectorQRSInt_RotateByRadians_UsesAxialBasis()
    {
        var actual = new VectorQRSInt(2, 3).Rotate(MathF.PI / 2f);

        VectorAssert.AreEqual(actual, -4.618802f, 4.041452f);
    }

    [TestCase(SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg60)]
    [TestCase(SixfoldAngle.Deg120)]
    [TestCase(SixfoldAngle.Deg180)]
    [TestCase(SixfoldAngle.Deg240)]
    [TestCase(SixfoldAngle.Deg300)]
    public void VectorQRSInt_RotateByRadians_MatchesSixfoldRotation(SixfoldAngle angle)
    {
        var point = new VectorQRSInt(2, -5);

        var expected = point.Rotate(angle);
        var actual = point.Rotate(angle.AsFloatRadians());

        VectorAssert.AreEqual(actual, expected.Q, expected.R);
    }

    [Test]
    public void VectorQRSInt_RotateByRadians_WhenAngleIsNotFinite_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VectorQRSInt(1, 2).Rotate(float.NaN));
    }

    [Test]
    public void QRSRotations_ThrowForInvalidSixfoldAngle()
    {
        var invalid = (SixfoldAngle)42;

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRS(1f, 2f).Rotate(invalid));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new VectorQRSInt(1, 2).Rotate(invalid));
        });
    }

    [Test]
    public void VectorXY_RotateBySixfoldAngle_ReturnsExpectedVector()
    {
        var actual = new VectorXY(2f, 0f).Rotate(SixfoldAngle.Deg60);

        VectorAssert.AreEqual(actual, 1f, 1.7320508f);
    }

    [Test]
    public void VectorXYInt_RotateBySixfoldAngle_ReturnsExpectedVector()
    {
        var actual = new VectorXYInt(2, -3).Rotate(SixfoldAngle.Deg180);

        VectorAssert.AreEqual(actual, -2f, 3f);
    }

    [Test]
    public void VectorXY_TransformOverloads_ReturnExpectedVectors()
    {
        var point = new VectorXY(2f, 0f);
        var offset = new VectorXY(10f, 20f);
        var intOffset = new VectorXYInt(10, 20);

        Assert.Multiple(() =>
        {
            VectorAssert.AreEqual(point.Transform(SixfoldAngle.Deg60, offset), 11f, 21.73205f);
            VectorAssert.AreEqual(point.Transform(SixfoldAngle.Deg60, intOffset), 11f, 21.73205f);
            VectorAssert.AreEqual(point.Transform(2f, SixfoldAngle.Deg60, offset), 12f, 23.464102f);
            VectorAssert.AreEqual(point.Transform(2f, SixfoldAngle.Deg60, intOffset), 12f, 23.464102f);
        });
    }

    [Test]
    public void VectorXYInt_TransformOverloads_ReturnExpectedVectors()
    {
        var point = new VectorXYInt(2, 0);
        var offset = new VectorXY(10f, 20f);
        var intOffset = new VectorXYInt(10, 20);

        Assert.Multiple(() =>
        {
            VectorAssert.AreEqual(point.Transform(SixfoldAngle.Deg60, offset), 11f, 21.73205f);
            VectorAssert.AreEqual(point.Transform(SixfoldAngle.Deg60, intOffset), 11f, 21.73205f);
            VectorAssert.AreEqual(point.Transform(2f, SixfoldAngle.Deg60, offset), 12f, 23.464102f);
            VectorAssert.AreEqual(point.Transform(2f, SixfoldAngle.Deg60, intOffset), 12f, 23.464102f);
        });
    }

    [Test]
    public void VectorXY_RotateAroundPivotOverloads_ReturnExpectedVectors()
    {
        var point = new VectorXY(3f, 1f);
        var pivot = new VectorXY(1f, 1f);
        var intPivot = new VectorXYInt(1, 1);

        Assert.Multiple(() =>
        {
            VectorAssert.AreEqual(point.Rotate(pivot, SixfoldAngle.Deg60), 2f, 2.7320508f);
            VectorAssert.AreEqual(point.Rotate(intPivot, SixfoldAngle.Deg60), 2f, 2.7320508f);
        });
    }

    [Test]
    public void VectorXYInt_RotateAroundPivotOverloads_ReturnExpectedVectors()
    {
        var point = new VectorXYInt(3, 1);
        var pivot = new VectorXY(1f, 1f);
        var intPivot = new VectorXYInt(1, 1);

        Assert.Multiple(() =>
        {
            VectorAssert.AreEqual(point.Rotate(pivot, SixfoldAngle.Deg60), 2f, 2.7320508f);
            VectorAssert.AreEqual(point.Rotate(intPivot, SixfoldAngle.Deg60), 2f, 2.7320508f);
        });
    }
}
