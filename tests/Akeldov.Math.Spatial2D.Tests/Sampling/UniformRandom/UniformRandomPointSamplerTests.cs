using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Sampling.Point.UniformRandom;

namespace Akeldov.Math.Spatial2D.Tests.Sampling.UniformRandom;

public class UniformRandomPointSamplerTests
{
    [Test]
    public void Sample_DrawsBothCoordinatesIndependentlyInWorldBounds()
    {
        var bounds = new Rectangle(new PointXY(-40, 75), new PointXY(80, 90));
        var sampler = new UniformRandomPointSampler(new Random(7321));
        var expected = new Random(7321);
        var points = sampler.Sample(bounds, 128);

        Assert.That(points.Count, Is.EqualTo(128));
        foreach (var point in points)
        {
            Assert.That(point, Is.EqualTo(new PointXY(
                -40 + (float)expected.NextDouble() * 120, 75 + (float)expected.NextDouble() * 15)));
            Assert.That(bounds.Contains(point), Is.True);
        }
    }

    [Test]
    public void Sample_ContinuesSeededSequenceAcrossCallsAndReturnsCallerOwnedLists()
    {
        var size = new VectorXY(4, 7);
        var sampler = new UniformRandomPointSampler(new Random(1742));
        var first = sampler.Sample(size, 3);
        Assert.That(sampler.Sample(size, 0), Is.Empty);
        var second = sampler.Sample(size, 5);

        var bounds = new Rectangle(default, new PointXY(4, 7));
        var together = new UniformRandomPointSampler(new Random(1742)).Sample(bounds, 8);
        Assert.That(first.Concat(second), Is.EqualTo(together));
        first.Clear();
        Assert.That(second.Count, Is.EqualTo(5));
    }

    [Test]
    public void Sample_AllowsCoincidentDrawsAndKeepsRoundedCoordinatesInBounds()
    {
        var bounds = new Rectangle(new PointXY(-100_000_000f, 1), new PointXY(-1, 2));
        var points = new UniformRandomPointSampler(new UpperEdgeRandom()).Sample(bounds, 3);
        Assert.That(points, Is.EqualTo(new[] { bounds.Max, bounds.Max, bounds.Max }));
    }

    [TestCase(0f, 1f)]
    [TestCase(1f, -1f)]
    [TestCase(float.NaN, 1f)]
    [TestCase(1f, float.PositiveInfinity)]
    public void Sample_RejectsInvalidSize(float width, float height)
    {
        var sampler = new UniformRandomPointSampler(new Random(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sampler.Sample(new VectorXY(width, height), 1));
    }

    [Test]
    public void RejectsNullGeneratorNegativeCountAndDegenerateBounds()
    {
        Assert.Throws<ArgumentNullException>(() => new UniformRandomPointSampler(null!));
        var sampler = new UniformRandomPointSampler(new Random(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sampler.Sample(VectorXY.One, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => sampler.Sample(new Rectangle(default, new PointXY(0, 1)), 1));
    }

    private sealed class UpperEdgeRandom : Random
    {
        public override double NextDouble() => 0.9999999999;
    }
}
