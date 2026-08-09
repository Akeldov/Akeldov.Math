namespace Akeldov.Math.Hexes.Tests.Maps.Filtering;

public class FloatHexMapBlurExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void GaussianBlur_WithRadiusOne_UsesHexCenterDistance(Layout layout)
    {
        var topology = new HexMapTopology(5, 5, layout);
        var values = new float[topology.Count];
        values[12] = 1f;
        var map = new FloatHexMap(topology, values);

        FloatHexMap result = map.GaussianBlur(1f, 1);

        double adjacentWeight = System.Math.Exp(-0.5d);
        float expectedCenter = (float)(1d / (1d + 6d * adjacentWeight));

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[12], Is.EqualTo(expectedCenter).Within(1e-7f));
            Assert.That(map[12], Is.EqualTo(1f));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void GaussianBlur_AtBoundaries_RenormalizesPresentWeights(Layout layout)
    {
        var topology = new HexMapTopology(4, 3, layout);
        var map = new FloatHexMap(topology, CreateValues(topology.Count, 7f));

        FloatHexMap result = map.GaussianBlur(1.25f, 2);

        for (int index = 0; index < topology.Count; index++)
            Assert.That(result[index], Is.EqualTo(7f).Within(1e-6f), $"Unexpected value at flat index {index}.");
    }

    [Test]
    public void GaussianBlur_WithoutRadius_UsesThreeSigmaTruncation()
    {
        var topology = new HexMapTopology(7, 7, Layout.OddR);
        var values = new float[topology.Count];
        for (int index = 0; index < values.Length; index++)
            values[index] = index % 5;

        var map = new FloatHexMap(topology, values);

        FloatHexMap automatic = map.GaussianBlur(0.6f);
        FloatHexMap explicitRadius = map.GaussianBlur(0.6f, 2);

        for (int index = 0; index < topology.Count; index++)
            Assert.That(automatic[index], Is.EqualTo(explicitRadius[index]).Within(1e-7f));
    }

    [Test]
    public void GaussianBlur_WithZeroRadius_ReturnsIndependentCopy()
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var map = new FloatHexMap(topology, new[] { 2f, -3f });

        FloatHexMap result = map.GaussianBlur(1f, 0);
        result[0] = 100f;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(100f));
            Assert.That(result[1], Is.EqualTo(-3f));
            Assert.That(map[0], Is.EqualTo(2f));
        });
    }

    [Test]
    public void GaussianBlur_WhenMapIsEmpty_ReturnsEmptyMap()
    {
        var topology = new HexMapTopology(0, 0, Layout.OddR);
        var map = new FloatHexMap(topology);

        FloatHexMap result = map.GaussianBlur(1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void GaussianBlur_WithInvalidArguments_Throws()
    {
        var map = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR));
        IHexMap<float>? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => missing.GaussianBlur(1f))!.ParamName,
                Is.EqualTo("map"));
#pragma warning restore CS8604
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(0f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(-1f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(float.NaN))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(float.PositiveInfinity))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.GaussianBlur(1f, -1))!.ParamName,
                Is.EqualTo("radius"));
        });
    }

    private static float[] CreateValues(int count, float value)
    {
        var values = new float[count];
        Array.Fill(values, value);
        return values;
    }
}
