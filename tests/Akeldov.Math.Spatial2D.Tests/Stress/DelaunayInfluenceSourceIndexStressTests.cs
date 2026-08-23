using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Stress;

[Explicit("Stress tests are excluded from normal test runs.")]
[Category("Stress")]
public class DelaunayInfluenceSourceIndexStressTests
{
    [TestCase(5101)]
    [TestCase(5102)]
    [TestCase(5103)]
    public void SelectSources_WithLargePointCloud_ReturnsNonEmptySubsetFromSnapshot(int seed)
    {
        var random = new Random(seed);
        FloatPointInfluenceSource[] sources = CreateSources(random, sourceCount: 350);
        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
        var sourceSet = new HashSet<FloatPointInfluenceSource>(index.Sources);

        for (int i = 0; i < 1_500; i++)
        {
            var point = new PointXY(NextFloat(random, -650f, 650f), NextFloat(random, -650f, 650f));

            List<FloatPointInfluenceSource> selectedSources = index.SelectSources(point);

            Assert.That(selectedSources, Has.Count.InRange(1, 3), $"Seed: {seed}, point: {i}.");
            for (int j = 0; j < selectedSources.Count; j++)
            {
                Assert.That(sourceSet.Contains(selectedSources[j]), Is.True, $"Seed: {seed}, point: {i}, selected: {j}.");
                Assert.That(IsFinite(selectedSources[j].Position), Is.True, $"Seed: {seed}, point: {i}, selected: {j}.");
            }
        }
    }

    private static FloatPointInfluenceSource[] CreateSources(Random random, int sourceCount)
    {
        var sources = new FloatPointInfluenceSource[sourceCount];
        var usedPoints = new List<PointXY>(sourceCount);

        for (int i = 0; i < sources.Length; i++)
        {
            PointXY position;
            do
            {
                position = new PointXY(NextFloat(random, -500f, 500f), NextFloat(random, -500f, 500f));
            }
            while (ContainsAlmostEqual(usedPoints, position));

            usedPoints.Add(position);
            sources[i] = new FloatPointInfluenceSource(1f, position, i);
        }

        return sources;
    }

    private static bool ContainsAlmostEqual(IReadOnlyList<PointXY> points, PointXY candidate)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].AlmostEquals(candidate))
                return true;
        }

        return false;
    }

    private static float NextFloat(Random random, float min, float max)
    {
        return min + (max - min) * random.NextSingle();
    }

    private static bool IsFinite(PointXY point)
    {
        return !float.IsNaN(point.X) &&
            !float.IsNaN(point.Y) &&
            !float.IsInfinity(point.X) &&
            !float.IsInfinity(point.Y);
    }
}
