using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.PerlinNoise;

public class PerlinNoisePartitionParametersTests
{
    [Test]
    public void Constructor_DefaultsMatchNoiseGenerator()
    {
        var parameters = new PerlinNoisePartitionParameters(42, 7f);
        Assert.Multiple(() =>
        {
            Assert.That(parameters.Seed, Is.EqualTo(42));
            Assert.That(parameters.Scale, Is.EqualTo(7f));
            Assert.That(parameters.Octaves, Is.EqualTo(4));
            Assert.That(parameters.Persistence, Is.EqualTo(0.5f));
            Assert.That(parameters.Lacunarity, Is.EqualTo(2f));
            Assert.That(parameters.Offset, Is.EqualTo(default(VectorXY)));
        });
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    [TestCase(-1f)]
    public void Constructor_InvalidNoiseSettingsAreRejected(float value)
    {
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PerlinNoisePartitionParameters(42, value))!.ParamName, Is.EqualTo("scale"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PerlinNoisePartitionParameters(42, 7f, persistence: value))!.ParamName, Is.EqualTo("persistence"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PerlinNoisePartitionParameters(42, 7f, lacunarity: value))!.ParamName, Is.EqualTo("lacunarity"));
    }

    [Test]
    public void Constructor_EnforcesBoundsAndAcceptsBoundaryValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionParameters(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionParameters(0, 1, octaves: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionParameters(0, 1, persistence: 1.01f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionParameters(0, 1, lacunarity: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionParameters(0, 1, offset: new VectorXY(float.NaN, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoisePartitionParameters(0, 1, offset: new VectorXY(0, float.PositiveInfinity)));
        Assert.DoesNotThrow(() => new PerlinNoisePartitionParameters(int.MinValue, float.Epsilon, 1, 0, float.Epsilon));
        Assert.DoesNotThrow(() => new PerlinNoisePartitionParameters(int.MaxValue, float.MaxValue, 1, 1, float.MaxValue));
    }

    [Test]
    public void Partition_NullParametersAreRejectedForBothOverloads()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        var targets = new[] { new PerlinNoisePartitionTarget(7, 1) };
        var mask = new BoolHexMap(geometry.Topology);

        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            geometry.ToPerlinNoiseHexPartitionMap(targets, null!))!.ParamName, Is.EqualTo("parameters"));
        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            geometry.ToPerlinNoiseHexPartitionMap(targets, mask, null!))!.ParamName, Is.EqualTo("parameters"));
    }
}
