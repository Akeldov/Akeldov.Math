using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiPartitionPropertyFuzzTests
{
    private const int ScenarioCount = 50;

    [TestCase(42424)]
    [TestCase(86753)]
    public void Partition_WithGeneratedWeightedSites_AssignsEveryItemToNearestWeightedSite(int seed)
    {
        var random = new Random(seed);

        for (int scenario = 0; scenario < ScenarioCount; scenario++)
        {
            Site[] sites = CreateSites(random);
            TestItem[] items = CreateItems(random);
            var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

            var cells = partitioner.Partition(items);

            Assert.That(cells, Has.Count.EqualTo(sites.Length), CaseMessage(seed, scenario, "Cell count must match site count."));

            var assignedItemIds = new HashSet<int>();
            for (int siteIndex = 0; siteIndex < cells.Count; siteIndex++)
            {
                Assert.That(cells[siteIndex].Site, Is.EqualTo(sites[siteIndex]), CaseMessage(seed, scenario, "Cell site order changed."));

                foreach (TestItem item in cells[siteIndex].Items)
                {
                    Assert.That(
                        assignedItemIds.Add(item.Id),
                        Is.True,
                        CaseMessage(seed, scenario, $"Item {item.Id} was assigned more than once."));
                    Assert.That(
                        siteIndex,
                        Is.EqualTo(GetNearestWeightedSiteIndex(sites, item.Position)),
                        CaseMessage(seed, scenario, $"Item {item.Id} was assigned to a non-nearest weighted site."));
                }
            }

            Assert.That(
                assignedItemIds,
                Has.Count.EqualTo(items.Length),
                CaseMessage(seed, scenario, "Every item must be assigned exactly once."));
        }
    }

    private static Site[] CreateSites(Random random)
    {
        int siteCount = random.Next(2, 9);
        var sites = new Site[siteCount];

        for (int i = 0; i < sites.Length; i++)
        {
            sites[i] = new Site(
                new PointXY(NextFloat(random, -100f, 100f), NextFloat(random, -100f, 100f)),
                NextFloat(random, 0.25f, 4f));
        }

        return sites;
    }

    private static TestItem[] CreateItems(Random random)
    {
        int itemCount = random.Next(20, 61);
        var items = new TestItem[itemCount];

        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new TestItem(
                i,
                new PointXY(NextFloat(random, -125f, 125f), NextFloat(random, -125f, 125f)));
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

    private static string CaseMessage(int seed, int scenario, string message)
    {
        return $"{message} Seed: {seed}, scenario: {scenario}.";
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
