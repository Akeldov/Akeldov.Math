using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Tests.Stress;

[Explicit("Stress tests are excluded from normal test runs.")]
[Category("Stress")]
public class PoissonDiskStressTests
{
    [TestCase(1001)]
    [TestCase(2002)]
    [TestCase(3003)]
    public void Sample_WithLargeField_KeepsThousandsOfSamplesFarEnough(int seed)
    {
        var sampler = new PoissonDiskPointSampler(new Random(seed), maxAttempts: 30);
        var fieldSize = new VectorXY(220f, 180f);

        List<PoissonDiskPointSample> samples = sampler.Sample(fieldSize, minimalDistance: 4f);

        Assert.That(samples, Has.Count.GreaterThan(1_000), $"Seed: {seed}.");
        AssertSamplesAreInsideField(samples, fieldSize, seed);
        AssertEveryPairIsFarEnough(samples, seed);
    }

    private static void AssertSamplesAreInsideField(IReadOnlyList<PoissonDiskPointSample> samples, VectorXY fieldSize, int seed)
    {
        for (int i = 0; i < samples.Count; i++)
        {
            PointXY point = samples[i].Point;

            Assert.That(IsFinite(point), Is.True, $"Seed: {seed}, sample: {i}.");
            Assert.That(point.X, Is.InRange(0f, fieldSize.X), $"Seed: {seed}, sample: {i}.");
            Assert.That(point.Y, Is.InRange(0f, fieldSize.Y), $"Seed: {seed}, sample: {i}.");
        }
    }

    private static void AssertEveryPairIsFarEnough(IReadOnlyList<PoissonDiskPointSample> samples, int seed)
    {
        for (int i = 0; i < samples.Count; i++)
        {
            PoissonDiskPointSample a = samples[i];

            for (int j = i + 1; j < samples.Count; j++)
            {
                PoissonDiskPointSample b = samples[j];
                float requiredDistance = MathF.Max(a.MinimalDistance, b.MinimalDistance);
                float dx = a.Point.X - b.Point.X;
                float dy = a.Point.Y - b.Point.Y;
                float distanceSquared = dx * dx + dy * dy;
                float requiredDistanceWithTolerance = requiredDistance - GeometryConstants.GeometryEpsilon;

                Assert.That(
                    distanceSquared,
                    Is.GreaterThanOrEqualTo(requiredDistanceWithTolerance * requiredDistanceWithTolerance),
                    $"Seed: {seed}, samples: {i} and {j}.");
            }
        }
    }

    private static bool IsFinite(PointXY point)
    {
        return !float.IsNaN(point.X) &&
            !float.IsNaN(point.Y) &&
            !float.IsInfinity(point.X) &&
            !float.IsInfinity(point.Y);
    }
}
