using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA8BitColorTests
{
    [Test]
    public void Constructor_WhenValuesAreProvided_StoresChannels()
    {
        var color = new RGBA8BitColor(1, 2, 3, 4);

        Assert.That(color.Red, Is.EqualTo(1));
        Assert.That(color.Green, Is.EqualTo(2));
        Assert.That(color.Blue, Is.EqualTo(3));
        Assert.That(color.Alpha, Is.EqualTo(4));
    }

    [Test]
    public void FromNormalized_ConvertsNormalizedChannelsTo8BitChannels()
    {
        RGBA8BitColor color = RGBA8BitColor.FromNormalized(0f, 0.25f, 0.5f, 0.75f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(0, 64, 128, 191)));
    }

    [Test]
    public void FromNormalized_WhenValuesAreOutsideNormalizedRange_ClampsChannels()
    {
        RGBA8BitColor color = RGBA8BitColor.FromNormalized(-1f, 2f, float.PositiveInfinity, 0f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, byte.MaxValue, 0)));
    }

    [Test]
    public void FromNormalized_WhenValueIsNaN_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RGBA8BitColor.FromNormalized(float.NaN, 0f, 0f));
    }

    [Test]
    public void FromTemperature_WhenNormalizedValueIsOnStops_ReturnsTemperatureColors()
    {
        Assert.That(
            RGBA8BitColor.FromTemperature(0f),
            Is.EqualTo(new RGBA8BitColor(0, 0, byte.MaxValue, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(0.5f),
            Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, 0, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(1f),
            Is.EqualTo(new RGBA8BitColor(byte.MaxValue, 0, 0, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenNormalizedValueIsBetweenStops_InterpolatesChannels()
    {
        RGBA8BitColor color = RGBA8BitColor.FromTemperature(0.125f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(0, 128, byte.MaxValue, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenNormalizedValueIsOutsideRange_Clamps()
    {
        Assert.That(
            RGBA8BitColor.FromTemperature(-1f),
            Is.EqualTo(new RGBA8BitColor(0, 0, byte.MaxValue, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(2f),
            Is.EqualTo(new RGBA8BitColor(byte.MaxValue, 0, 0, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenNormalizedValueIsInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RGBA8BitColor.FromTemperature(float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => RGBA8BitColor.FromTemperature(float.PositiveInfinity));
    }

    [Test]
    public void FromTemperature_WhenValueRangeIsProvided_NormalizesValue()
    {
        Assert.That(
            RGBA8BitColor.FromTemperature(25f, 0f, 100f),
            Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(50f, 0f, 100f),
            Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, 0, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(75f, 0f, 100f),
            Is.EqualTo(new RGBA8BitColor(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenValueRangeIsSingleValue_MapsToMiddleTemperatureColor()
    {
        RGBA8BitColor color = RGBA8BitColor.FromTemperature(7f, 7f, 7f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, 0, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenValueRangeInputsAreInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RGBA8BitColor.FromTemperature(float.NaN, 0f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => RGBA8BitColor.FromTemperature(0f, float.NaN, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => RGBA8BitColor.FromTemperature(0f, 1f, 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => RGBA8BitColor.FromTemperature(0f, 0f, float.PositiveInfinity));
    }

    [Test]
    public void Blend_LinearlyBlendsChannels()
    {
        var from = new RGBA8BitColor(0, 10, 20, 40);
        var to = new RGBA8BitColor(10, 30, 40, 0);

        RGBA8BitColor color = RGBA8BitColor.Blend(from, to, 0.25f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(2, 15, 25, 30)));
    }

    [Test]
    public void Blend_WhenAmountIsOutsideNormalizedRange_ClampsAmount()
    {
        var from = new RGBA8BitColor(1, 2, 3, 4);
        var to = new RGBA8BitColor(5, 6, 7, 8);

        Assert.That(RGBA8BitColor.Blend(from, to, -1f), Is.EqualTo(from));
        Assert.That(RGBA8BitColor.Blend(from, to, 2f), Is.EqualTo(to));
    }

    [Test]
    public void Blend_WhenAmountIsNaN_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RGBA8BitColor.Blend(
                new RGBA8BitColor(1, 2, 3, 4),
                new RGBA8BitColor(5, 6, 7, 8),
                float.NaN));
    }

    [Test]
    public void EqualityMembers_CompareChannelValues()
    {
        var first = new RGBA8BitColor(1, 2, 3, 4);
        var second = new RGBA8BitColor(1, 2, 3, 4);
        var third = new RGBA8BitColor(1, 2, 3, 5);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first == second, Is.True);
        Assert.That(first != third, Is.True);
        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
    }

    [Test]
    public void ToString_ReturnsChannelValues()
    {
        var color = new RGBA8BitColor(1, 2, 3, 4);

        Assert.That(color.ToString(), Is.EqualTo("rgba8(1, 2, 3, 4)"));
    }
}
