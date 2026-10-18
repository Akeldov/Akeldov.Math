using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Spatial2D.Tests.Stress;

[Explicit("Stress tests are excluded from normal test runs.")]
[Category("Stress")]
public class VoronoiStressTests
{
    [TestCase(4101)]
    [TestCase(4102)]
    [TestCase(4103)]
    public void Partition_WithLargeWeightedInput_AssignsEveryItemOnceToNearestWeightedSite(int seed)
    {
        var random = new Random(seed);
        Site[] sites = CreateSites(random, siteCount: 96);
        TestItem[] items = CreateItems(random, itemCount: 5_000);
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        IReadOnlyList<VoronoiItemPartition<TestItem>> cells = partitioner.Partition(items);

        Assert.That(cells, Has.Count.EqualTo(sites.Length), $"Seed: {seed}.");

        var assignedItemIds = new HashSet<int>();
        for (int siteIndex = 0; siteIndex < cells.Count; siteIndex++)
        {
            Assert.That(cells[siteIndex].Site, Is.EqualTo(sites[siteIndex]), $"Seed: {seed}, site: {siteIndex}.");

            foreach (TestItem item in cells[siteIndex].Items)
            {
                Assert.That(assignedItemIds.Add(item.Id), Is.True, $"Seed: {seed}, item: {item.Id}.");
                Assert.That(
                    siteIndex,
                    Is.EqualTo(GetNearestWeightedSiteIndex(sites, item.Position)),
                    $"Seed: {seed}, item: {item.Id}.");
            }
        }

        Assert.That(assignedItemIds, Has.Count.EqualTo(items.Length), $"Seed: {seed}.");
    }

    private static Site[] CreateSites(Random random, int siteCount)
    {
        var sites = new Site[siteCount];

        for (int i = 0; i < sites.Length; i++)
        {
            sites[i] = new Site(
                new PointXY(NextFloat(random, -500f, 500f), NextFloat(random, -500f, 500f)),
                NextFloat(random, 0.25f, 6f));
        }

        return sites;
    }

    private static TestItem[] CreateItems(Random random, int itemCount)
    {
        var items = new TestItem[itemCount];

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new TestItem(
                i,
                new PointXY(NextFloat(random, -600f, 600f), NextFloat(random, -600f, 600f)));
        }

        return items;
    }

    private static int GetNearestWeightedSiteIndex(Site[] sites, PointXY point)
    {
        float bestWeightedDistance = float.PositiveInfinity;
        int bestIndex = 0;

        for (int i = 0; i < sites.Length; i++)
        {
            float dx = sites[i].Position.X - point.X;
            float dy = sites[i].Position.Y - point.Y;
            float distanceSquared = dx * dx + dy * dy;

            if (distanceSquared <= GeometryConstants.GeometryEpsilonSquared)
                return i;

            float weightedDistanceSquared = distanceSquared / (sites[i].Weight * sites[i].Weight);
            if (weightedDistanceSquared < bestWeightedDistance)
            {
                bestWeightedDistance = weightedDistanceSquared;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private static float NextFloat(Random random, float min, float max)
    {
        return min + (max - min) * random.NextSingle();
    }

    private sealed class TestItem : IHasPosition2D
    {
        public TestItem(int id, PointXY position)
        {
            Id = id;
            Position = position;
        }

        public int Id { get; }

        public PointXY Position { get; }
    }
}
