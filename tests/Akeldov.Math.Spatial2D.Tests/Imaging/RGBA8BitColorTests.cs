using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA8BitColorTests
{
    [Test]
    public void Constructor_WhenValuesAreProvided_StoresChannels()
    {
        var color = new RGBA8BitColor(1, 2, 3, 4);

        Assert.That(color.R, Is.EqualTo(1));
        Assert.That(color.G, Is.EqualTo(2));
        Assert.That(color.B, Is.EqualTo(3));
        Assert.That(color.A, Is.EqualTo(4));
    }

    [Test]
    public void FromNormalized_ConvertsNormalizedChannelsTo8BitChannels()
    {
        RGBA8BitColor color = RGBA8BitColor.FromNormalized(0f, 0.25f, 0.5f, 0.75f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(0, 64, 128, 191)));
    }

    [Test]
    public void PredefinedColors_ReturnExpectedChannelValues()
    {
        Assert.That(RGBA8BitColor.Transparent, Is.EqualTo(default(RGBA8BitColor)));
        Assert.That(RGBA8BitColor.Black, Is.EqualTo(new RGBA8BitColor(0, 0, 0, byte.MaxValue)));
        Assert.That(RGBA8BitColor.White, Is.EqualTo(new RGBA8BitColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
        Assert.That(RGBA8BitColor.Red, Is.EqualTo(new RGBA8BitColor(byte.MaxValue, 0, 0, byte.MaxValue)));
        Assert.That(RGBA8BitColor.Green, Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, 0, byte.MaxValue)));
        Assert.That(RGBA8BitColor.Blue, Is.EqualTo(new RGBA8BitColor(0, 0, byte.MaxValue, byte.MaxValue)));
        Assert.That(RGBA8BitColor.Yellow, Is.EqualTo(new RGBA8BitColor(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue)));
        Assert.That(RGBA8BitColor.Cyan, Is.EqualTo(new RGBA8BitColor(0, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
        Assert.That(RGBA8BitColor.Magenta, Is.EqualTo(new RGBA8BitColor(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue)));
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
            Is.EqualTo(new RGBA8BitColor(0, 0, 128, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(0.5f),
            Is.EqualTo(new RGBA8BitColor(130, byte.MaxValue, 126, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(1f),
            Is.EqualTo(new RGBA8BitColor(128, 0, 0, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenNormalizedValueIsBetweenPaletteEntries_UsesNearestPaletteEntry()
    {
        RGBA8BitColor color = RGBA8BitColor.FromTemperature(0.125f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(0, 0, byte.MaxValue, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenNormalizedValueIsOutsideRange_Clamps()
    {
        Assert.That(
            RGBA8BitColor.FromTemperature(-1f),
            Is.EqualTo(new RGBA8BitColor(0, 0, 128, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(2f),
            Is.EqualTo(new RGBA8BitColor(128, 0, 0, byte.MaxValue)));
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
            Is.EqualTo(new RGBA8BitColor(0, 128, byte.MaxValue, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(50f, 0f, 100f),
            Is.EqualTo(new RGBA8BitColor(130, byte.MaxValue, 126, byte.MaxValue)));
        Assert.That(
            RGBA8BitColor.FromTemperature(75f, 0f, 100f),
            Is.EqualTo(new RGBA8BitColor(byte.MaxValue, 128, 0, byte.MaxValue)));
    }

    [Test]
    public void FromTemperature_WhenValueRangeIsSingleValue_MapsToMiddleTemperatureColor()
    {
        RGBA8BitColor color = RGBA8BitColor.FromTemperature(7f, 7f, 7f);

        Assert.That(color, Is.EqualTo(new RGBA8BitColor(130, byte.MaxValue, 126, byte.MaxValue)));
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
    public void Adjustments_DefaultToSrgbAndPreserveOriginalColor()
    {
        var color = new RGBA8BitColor(100, 200, 60, 73);
        Func<float, RGBA8BitColor> darken = color.Darken;
        Func<float, RGBA8BitColor> lighten = color.Lighten;

        Assert.That(darken(0.3f), Is.EqualTo(new RGBA8BitColor(70, 140, 42, 73)));
        Assert.That(lighten(0.5f), Is.EqualTo(new RGBA8BitColor(178, 228, 158, 73)));
        Assert.That(color.Darken(0.3f), Is.EqualTo(color.Darken(0.3f, ColorAdjustmentMode.Srgb)));
        Assert.That(color.Lighten(0.5f), Is.EqualTo(color.Lighten(0.5f, ColorAdjustmentMode.Srgb)));
        Assert.That(color, Is.EqualTo(new RGBA8BitColor(100, 200, 60, 73)));
    }

    [TestCase(ColorAdjustmentMode.Srgb, 128)]
    [TestCase(ColorAdjustmentMode.LinearRgb, 188)]
    [TestCase(ColorAdjustmentMode.Oklch, 99)]
    public void Adjustments_HalfwayBetweenBlackAndWhiteUsesSelectedSpace(ColorAdjustmentMode mode, byte expected)
    {
        // In linear RGB the halfway value is 0.5 (sRGB byte 188).
        // For neutral OKLCH at L=0.5 the linear RGB value is L^3=0.125 (sRGB byte 99).
        var black = new RGBA8BitColor(0, 0, 0, 42);
        var white = new RGBA8BitColor(255, 255, 255, 42);
        var middle = new RGBA8BitColor(expected, expected, expected, 42);
        Assert.That(black.Lighten(0.5f, mode), Is.EqualTo(middle));
        Assert.That(white.Darken(0.5f, mode), Is.EqualTo(middle));
    }

    [TestCase(ColorAdjustmentMode.Srgb)]
    [TestCase(ColorAdjustmentMode.LinearRgb)]
    [TestCase(ColorAdjustmentMode.Oklch)]
    public void Adjustments_PreserveAlphaAndHaveExactEndpoints(ColorAdjustmentMode mode)
    {
        foreach (byte alpha in new byte[] { 0, 1, 128, 255 })
        {
            var color = new RGBA8BitColor(13, 71, 237, alpha);
            Assert.That(color.Darken(0, mode), Is.EqualTo(color));
            Assert.That(color.Lighten(0, mode), Is.EqualTo(color));
            Assert.That(color.Darken(1, mode), Is.EqualTo(new RGBA8BitColor(0, 0, 0, alpha)));
            Assert.That(color.Lighten(1, mode), Is.EqualTo(new RGBA8BitColor(255, 255, 255, alpha)));
            Assert.That(color.Darken(0.4f, mode).A, Is.EqualTo(alpha));
            Assert.That(color.Lighten(0.4f, mode).A, Is.EqualTo(alpha));
        }
    }

    [TestCase(ColorAdjustmentMode.Srgb)]
    [TestCase(ColorAdjustmentMode.LinearRgb)]
    [TestCase(ColorAdjustmentMode.Oklch)]
    public void Adjustments_GrayRemainsNeutralAndChangesMonotonically(ColorAdjustmentMode mode)
    {
        for (int channel = 0; channel <= 255; channel++)
        {
            var color = new RGBA8BitColor((byte)channel, (byte)channel, (byte)channel, 255);
            var previousDark = color;
            var previousLight = color;
            for (int step = 0; step <= 10; step++)
            {
                var dark = color.Darken(step / 10f, mode);
                var light = color.Lighten(step / 10f, mode);
                Assert.That(dark.R, Is.EqualTo(dark.G).And.EqualTo(dark.B), $"Gray {channel}, step {step}");
                Assert.That(light.R, Is.EqualTo(light.G).And.EqualTo(light.B), $"Gray {channel}, step {step}");
                Assert.That(dark.R, Is.LessThanOrEqualTo(previousDark.R));
                Assert.That(light.R, Is.GreaterThanOrEqualTo(previousLight.R));
                previousDark = dark;
                previousLight = light;
            }
        }
    }

    [TestCase(255, 0, 0)]
    [TestCase(0, 255, 0)]
    [TestCase(0, 0, 255)]
    public void Adjustments_OklchDarkensSaturatedPrimariesByReducingChroma(byte r, byte g, byte b)
    {
        // At a primary's gamut boundary, halving L also halves maximum chroma at fixed hue.
        // Its linear RGB channels scale by (1/2)^3, giving byte 99 for the nonzero channel.
        var result = new RGBA8BitColor(r, g, b, 128).Darken(0.5f, ColorAdjustmentMode.Oklch);
        Assert.That(result.R, Is.EqualTo(r == 0 ? 0 : 99).Within(1));
        Assert.That(result.G, Is.EqualTo(g == 0 ? 0 : 99).Within(1));
        Assert.That(result.B, Is.EqualTo(b == 0 ? 0 : 99).Within(1));
        Assert.That(result.A, Is.EqualTo(128));
    }

    [Test]
    public void Adjustments_InvalidAmountsAndModesThrow()
    {
        var color = new RGBA8BitColor(100, 200, 60, 0);
        foreach (float amount in new[] { -0.1f, 1.1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => color.Darken(amount))!.ParamName, Is.EqualTo("amount"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => color.Lighten(amount))!.ParamName, Is.EqualTo("amount"));
            foreach (var mode in Enum.GetValues<ColorAdjustmentMode>())
            {
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => color.Darken(amount, mode))!.ParamName, Is.EqualTo("amount"));
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => color.Lighten(amount, mode))!.ParamName, Is.EqualTo("amount"));
            }
        }
        foreach (float amount in new[] { 0f, 0.5f, 1f })
            foreach (var mode in new[] { (ColorAdjustmentMode)(-1), (ColorAdjustmentMode)3 })
            {
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => color.Darken(amount, mode))!.ParamName, Is.EqualTo("mode"));
                Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => color.Lighten(amount, mode))!.ParamName, Is.EqualTo("mode"));
            }
    }

    [Test]
    public void ToString_ReturnsChannelValues()
    {
        var color = new RGBA8BitColor(1, 2, 3, 4);

        Assert.That(color.ToString(), Is.EqualTo("rgba8(1, 2, 3, 4)"));
    }
}
