using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class RGBA16BitColorTests
{
    [Test]
    public void Constructor_WhenValuesAreProvided_StoresChannels()
    {
        var color = new RGBA16BitColor(1, 2, 3, 4);

        Assert.That(color.Red, Is.EqualTo(1));
        Assert.That(color.Green, Is.EqualTo(2));
        Assert.That(color.Blue, Is.EqualTo(3));
        Assert.That(color.Alpha, Is.EqualTo(4));
    }

    [Test]
    public void FromNormalized_ConvertsNormalizedChannelsTo16BitChannels()
    {
        RGBA16BitColor color = RGBA16BitColor.FromNormalized(0f, 0.25f, 0.5f, 0.75f);

        Assert.That(color, Is.EqualTo(new RGBA16BitColor(0, 16384, 32768, 49151)));
    }

    [Test]
    public void FromNormalized_WhenValuesAreOutsideNormalizedRange_ClampsChannels()
    {
        RGBA16BitColor color = RGBA16BitColor.FromNormalized(-1f, 2f, float.PositiveInfinity, 0f);

        Assert.That(color, Is.EqualTo(new RGBA16BitColor(0, ushort.MaxValue, ushort.MaxValue, 0)));
    }

    [Test]
    public void FromNormalized_WhenValueIsNaN_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RGBA16BitColor.FromNormalized(float.NaN, 0f, 0f));
    }

    [Test]
    public void Blend_LinearlyBlendsChannels()
    {
        var from = new RGBA16BitColor(0, 10_000, 20_000, 40_000);
        var to = new RGBA16BitColor(10_000, 30_000, 40_000, 0);

        RGBA16BitColor color = RGBA16BitColor.Blend(from, to, 0.25f);

        Assert.That(color, Is.EqualTo(new RGBA16BitColor(2_500, 15_000, 25_000, 30_000)));
    }

    [Test]
    public void Blend_WhenAmountIsOutsideNormalizedRange_ClampsAmount()
    {
        var from = new RGBA16BitColor(1, 2, 3, 4);
        var to = new RGBA16BitColor(5, 6, 7, 8);

        Assert.That(RGBA16BitColor.Blend(from, to, -1f), Is.EqualTo(from));
        Assert.That(RGBA16BitColor.Blend(from, to, 2f), Is.EqualTo(to));
    }

    [Test]
    public void Blend_WhenAmountIsNaN_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RGBA16BitColor.Blend(
                new RGBA16BitColor(1, 2, 3, 4),
                new RGBA16BitColor(5, 6, 7, 8),
                float.NaN));
    }

    [Test]
    public void EqualityMembers_CompareChannelValues()
    {
        var first = new RGBA16BitColor(1, 2, 3, 4);
        var second = new RGBA16BitColor(1, 2, 3, 4);
        var third = new RGBA16BitColor(1, 2, 3, 5);

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first == second, Is.True);
        Assert.That(first != third, Is.True);
        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
    }

    [Test]
    public void ToString_ReturnsChannelValues()
    {
        var color = new RGBA16BitColor(1, 2, 3, 4);

        Assert.That(color.ToString(), Is.EqualTo("rgba16(1, 2, 3, 4)"));
    }
}
