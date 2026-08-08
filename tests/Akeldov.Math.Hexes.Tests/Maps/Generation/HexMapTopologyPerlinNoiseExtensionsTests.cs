using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps.Generation;

public class HexMapTopologyPerlinNoiseExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void CreatePerlinNoise_ReturnsFiniteNormalizedValuesForEveryLayout(Layout layout)
    {
        var topology = new HexMapTopology(17, 13, layout);

        FloatHexMap map = topology.CreatePerlinNoise(
            seed: 12345,
            scale: 6f,
            octaves: 5,
            persistence: 0.55f,
            lacunarity: 2.1f,
            offset: new VectorXY(3.25f, -7.5f));

        Assert.That(map.Topology, Is.EqualTo(topology));
        Assert.Multiple(() =>
        {
            Assert.That(map.Min, Is.GreaterThanOrEqualTo(0f));
            Assert.That(map.Max, Is.LessThanOrEqualTo(1f));
            Assert.That(map.Max, Is.GreaterThan(map.Min));
        });

        for (int index = 0; index < topology.Count; index++)
            Assert.That(float.IsNaN(map[index]) || float.IsInfinity(map[index]), Is.False);
    }

    [Test]
    public void CreatePerlinNoise_WithSameArguments_ReturnsSameValues()
    {
        var topology = new HexMapTopology(11, 9, Layout.OddR);

        FloatHexMap first = topology.CreatePerlinNoise(37, 4.5f, 3, 0.4f, 2.25f, new VectorXY(-2f, 5f));
        FloatHexMap second = topology.CreatePerlinNoise(37, 4.5f, 3, 0.4f, 2.25f, new VectorXY(-2f, 5f));

        for (int index = 0; index < topology.Count; index++)
            Assert.That(second[index], Is.EqualTo(first[index]), $"Value differs at flat index {index}.");
    }

    [Test]
    public void CreatePerlinNoise_WithDifferentSeed_ChangesValues()
    {
        var topology = new HexMapTopology(11, 9, Layout.EvenQ);

        FloatHexMap first = topology.CreatePerlinNoise(seed: 1, scale: 4f);
        FloatHexMap second = topology.CreatePerlinNoise(seed: 2, scale: 4f);

        bool hasDifferentValue = false;
        for (int index = 0; index < topology.Count; index++)
            hasDifferentValue |= first[index] != second[index];

        Assert.That(hasDifferentValue, Is.True);
    }

    [Test]
    public void CreatePerlinNoise_WithZeroPersistence_UsesOnlyFirstOctave()
    {
        var topology = new HexMapTopology(7, 5, Layout.EvenR);

        FloatHexMap singleOctave = topology.CreatePerlinNoise(seed: 73, scale: 3f, octaves: 1);
        FloatHexMap zeroPersistence = topology.CreatePerlinNoise(
            seed: 73,
            scale: 3f,
            octaves: 8,
            persistence: 0f,
            lacunarity: float.MaxValue);

        for (int index = 0; index < topology.Count; index++)
            Assert.That(zeroPersistence[index], Is.EqualTo(singleOctave[index]));
    }

    [Test]
    public void CreatePerlinNoise_WithHorizontalOffset_ContinuesSameNoiseField()
    {
        var largerTopology = new HexMapTopology(6, 4, Layout.OddR);
        var shiftedTopology = new HexMapTopology(5, 4, Layout.OddR);
        float adjacentCenterDistance = MathF.Sqrt(3f);

        FloatHexMap larger = largerTopology.CreatePerlinNoise(seed: 91, scale: 5f);
        FloatHexMap shifted = shiftedTopology.CreatePerlinNoise(
            seed: 91,
            scale: 5f,
            offset: new VectorXY(adjacentCenterDistance, 0f));

        for (int y = 0; y < shiftedTopology.Resolution.Y; y++)
        {
            for (int x = 0; x < shiftedTopology.Resolution.X; x++)
            {
                Assert.That(
                    shifted[new VectorXYInt(x, y)],
                    Is.EqualTo(larger[new VectorXYInt(x + 1, y)]).Within(1e-6f),
                    $"Shifted field differs at ({x}, {y}).");
            }
        }
    }

    [Test]
    public void CreatePerlinNoise_WithEmptyTopology_ReturnsEmptyMap()
    {
        var topology = new HexMapTopology(0, 3, Layout.OddQ);

        FloatHexMap map = topology.CreatePerlinNoise(seed: 1, scale: 2f);

        Assert.Multiple(() =>
        {
            Assert.That(map.Topology, Is.EqualTo(topology));
            Assert.That(map.Topology.Count, Is.Zero);
        });
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidScale_Throws(float scale)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => topology.CreatePerlinNoise(1, scale));

        Assert.That(exception!.ParamName, Is.EqualTo("scale"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CreatePerlinNoise_WithInvalidOctaveCount_Throws(int octaves)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => topology.CreatePerlinNoise(1, 1f, octaves));

        Assert.That(exception!.ParamName, Is.EqualTo("octaves"));
    }

    [TestCase(-0.1f)]
    [TestCase(1.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidPersistence_Throws(float persistence)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => topology.CreatePerlinNoise(1, 1f, persistence: persistence));

        Assert.That(exception!.ParamName, Is.EqualTo("persistence"));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidLacunarity_Throws(float lacunarity)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => topology.CreatePerlinNoise(1, 1f, lacunarity: lacunarity));

        Assert.That(exception!.ParamName, Is.EqualTo("lacunarity"));
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void CreatePerlinNoise_WithNonFiniteOffset_Throws(float x, float y)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => topology.CreatePerlinNoise(1, 1f, offset: new VectorXY(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("offset"));
    }
}
