using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps.Generation;

public class HexMapGeometryPerlinNoiseExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void CreatePerlinNoise_UsesWorldCentersAndRetainsGeometry(Layout layout)
    {
        var geometry = new HexMapGeometry(17, 13, new VectorXY(-13.5f, 7.25f), 2.5f, layout);
        var centers = new HexCenterMap(geometry);
        var offset = new VectorXY(3.25f, -7.5f);
        var map = geometry.CreatePerlinNoise(123, 6f, octaves: 1, offset: offset);

        Assert.That(((ISpatialHexMap<float>)map).Geometry, Is.EqualTo(geometry));
        Assert.That(map.Topology, Is.EqualTo(geometry.Topology));
        Assert.That(map.Min, Is.GreaterThanOrEqualTo(0f));
        Assert.That(map.Max, Is.LessThanOrEqualTo(1f));
        Assert.That(map.Max, Is.GreaterThan(map.Min));
        for (int i = 0; i < map.Topology.Count; i++)
        {
            double expected = 0.5 + 0.5 * PerlinNoise2D.Sample(
                ((double)centers[i].X + offset.X) / 6d,
                ((double)centers[i].Y + offset.Y) / 6d, 123);
            Assert.That(map[i], Is.EqualTo((float)expected).Within(1e-6f), $"{layout}, index={i}");
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void CreatePerlinNoise_UnitGeometryMatchesTopology(Layout layout)
    {
        var topology = new HexMapTopology(11, 9, layout);
        var expected = topology.CreatePerlinNoise(42, 7f);
        var actual = new HexMapGeometry(topology, 1f).CreatePerlinNoise(42, 7f);

        for (int i = 0; i < topology.Count; i++)
            Assert.That(actual[i], Is.EqualTo(expected[i]), $"{layout}, index={i}");
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void CreatePerlinNoise_OverlappingChunksSampleSameWorldField(Layout layout)
    {
        var geometry = new HexMapGeometry(13, 11, new VectorXY(-20f, 30f), 2.5f, layout);
        var origin = new VectorXYInt(2, 2).GetHexCenter(geometry.Radius, geometry.Origin, layout);
        var chunk = new HexMapGeometry(11, 9, origin, geometry.Radius, layout);
        var whole = geometry.CreatePerlinNoise(91, 15f);
        var part = chunk.CreatePerlinNoise(91, 15f);

        for (int y = 0; y < 9; y++)
            for (int x = 0; x < 11; x++)
                Assert.That(part[new VectorXYInt(x, y)],
                    Is.EqualTo(whole[new VectorXYInt(x + 2, y + 2)]).Within(2e-6f), $"{layout}, ({x}, {y})");
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void CreatePerlinNoise_ScalingWorldAndNoiseTogetherPreservesValues(Layout layout)
    {
        var geometry = new HexMapGeometry(11, 9, new VectorXY(-12f, 7f), 2.5f, layout);
        var scaled = new HexMapGeometry(geometry.Topology, geometry.Origin * 2f, geometry.Radius * 2f);
        var first = geometry.CreatePerlinNoise(37, 9f, offset: new VectorXY(2f, -3f));
        var second = scaled.CreatePerlinNoise(37, 18f, offset: new VectorXY(4f, -6f));

        for (int i = 0; i < geometry.Topology.Count; i++)
            Assert.That(second[i], Is.EqualTo(first[i]), $"{layout}, index={i}");
    }

    [Test]
    public void CreatePerlinNoise_DefaultGeometryIsRejected()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => default(HexMapGeometry).CreatePerlinNoise(42, 7f));
        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void CreatePerlinNoise_EmptyGeometryRetainedAndParametersValidated()
    {
        var geometry = new HexMapGeometry(0, 3, new VectorXY(-12f, 7f), 2.5f, Layout.EvenQ);
        var map = geometry.CreatePerlinNoise(42, 7f);
        Assert.That(map.Geometry, Is.EqualTo(geometry));
        Assert.That(map.Topology.Count, Is.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.CreatePerlinNoise(42, 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.CreatePerlinNoise(42, 7f, octaves: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.CreatePerlinNoise(42, 7f, persistence: float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.CreatePerlinNoise(42, 7f, lacunarity: 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => geometry.CreatePerlinNoise(42, 7f, offset: new VectorXY(float.NaN, 0f)));
    }
}
