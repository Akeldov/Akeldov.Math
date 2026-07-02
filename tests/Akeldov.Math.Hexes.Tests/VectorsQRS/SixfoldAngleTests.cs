using Akeldov.Math.Hexes.Vectors.QRS;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class SixfoldAngleTests
{
    [Test]
    public void All_ReturnsAnglesInOrder()
    {
        Assert.That(SixfoldAngles.All, Is.EqualTo(new[]
        {
            SixfoldAngle.Deg0,
            SixfoldAngle.Deg60,
            SixfoldAngle.Deg120,
            SixfoldAngle.Deg180,
            SixfoldAngle.Deg240,
            SixfoldAngle.Deg300
        }));
    }

    [Test]
    public void All_ReturnsCopy()
    {
        SixfoldAngle[] angles = SixfoldAngles.All;
        angles[0] = SixfoldAngle.Deg300;

        Assert.That(SixfoldAngles.All[0], Is.EqualTo(SixfoldAngle.Deg0));
    }

    [TestCase(SixfoldAngle.Deg0, 0f, 1f, 0f, 0f)]
    [TestCase(SixfoldAngle.Deg60, 0.8660254f, 0.5f, 1.0471976f, 60f)]
    [TestCase(SixfoldAngle.Deg120, 0.8660254f, -0.5f, 2.0943952f, 120f)]
    [TestCase(SixfoldAngle.Deg180, 0f, -1f, 3.1415927f, 180f)]
    [TestCase(SixfoldAngle.Deg240, -0.8660254f, -0.5f, 4.1887903f, 240f)]
    [TestCase(SixfoldAngle.Deg300, -0.8660254f, 0.5f, 5.2359877f, 300f)]
    public void TrigonometricValues_ReturnExpectedValues(
        SixfoldAngle angle,
        float expectedSin,
        float expectedCos,
        float expectedRadians,
        float expectedDegrees)
    {
        Assert.Multiple(() =>
        {
            Assert.That(angle.Sin(), Is.EqualTo(expectedSin).Within(VectorAssert.Epsilon));
            Assert.That(angle.Cos(), Is.EqualTo(expectedCos).Within(VectorAssert.Epsilon));
            Assert.That(angle.AsFloatRadians(), Is.EqualTo(expectedRadians).Within(VectorAssert.Epsilon));
            Assert.That(angle.AsFloatDegrees(), Is.EqualTo(expectedDegrees).Within(VectorAssert.Epsilon));
        });
    }

    [TestCase(SixfoldAngle.Deg0, SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg60, SixfoldAngle.Deg300)]
    [TestCase(SixfoldAngle.Deg120, SixfoldAngle.Deg240)]
    [TestCase(SixfoldAngle.Deg180, SixfoldAngle.Deg180)]
    [TestCase(SixfoldAngle.Deg240, SixfoldAngle.Deg120)]
    [TestCase(SixfoldAngle.Deg300, SixfoldAngle.Deg60)]
    public void Negate_ReturnsOppositeAngle(SixfoldAngle angle, SixfoldAngle expected)
    {
        Assert.That(angle.Negate(), Is.EqualTo(expected));
    }

    [TestCase(SixfoldAngle.Deg0, 1, SixfoldAngle.Deg60)]
    [TestCase(SixfoldAngle.Deg300, 1, SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg0, 2, SixfoldAngle.Deg120)]
    [TestCase(SixfoldAngle.Deg240, 2, SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg0, 3, SixfoldAngle.Deg180)]
    [TestCase(SixfoldAngle.Deg180, 3, SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg0, 4, SixfoldAngle.Deg240)]
    [TestCase(SixfoldAngle.Deg120, 4, SixfoldAngle.Deg0)]
    [TestCase(SixfoldAngle.Deg0, 5, SixfoldAngle.Deg300)]
    [TestCase(SixfoldAngle.Deg60, 5, SixfoldAngle.Deg0)]
    public void AddMethods_ReturnExpectedAngles(SixfoldAngle angle, int steps, SixfoldAngle expected)
    {
        SixfoldAngle actual = steps switch
        {
            1 => angle.Add60(),
            2 => angle.Add120(),
            3 => angle.Add180(),
            4 => angle.Add240(),
            5 => angle.Add300(),
            _ => throw new ArgumentOutOfRangeException(nameof(steps))
        };

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void AddMethods_ThrowForInvalidAngle()
    {
        var invalid = (SixfoldAngle)42;

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.Add60());
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.Add120());
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.Add180());
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.Add240());
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.Add300());
        });
    }
}
