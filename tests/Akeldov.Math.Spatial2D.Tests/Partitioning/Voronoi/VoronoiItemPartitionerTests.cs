using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiItemPartitionerTests
{
    [Test]
    public void Constructor_WhenSitesIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new VoronoiItemPartitioner<TestItem>(null!));
    }

    [Test]
    public void Constructor_WhenSitesIsEmpty_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VoronoiItemPartitioner<TestItem>(Array.Empty<Site>()));
    }

    [Test]
    public void Constructor_WhenRelaxationIterationsIsNegative_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new VoronoiItemPartitioner<TestItem>(sites, relaxationIterations: -1, EmptyCellPolicy.ThrowException));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void SiteConstructor_WhenWeightIsNegativeOrNaN_Throws(float weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Site(new PointXY(0f, 0f), weight));
    }

    [Test]
    public void SiteConstructor_WhenWeightIsZero_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new Site(new PointXY(0f, 0f), 0f));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void SiteConstructor_WhenPositionCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Site(new PointXY(x, y), 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("position"));
    }

    [Test]
    public void Constructor_WhenAllSiteWeightsAreZero_Throws()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 0f),
            new Site(new PointXY(1f, 1f), 0f)
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs));

        Assert.That(exception!.ParamName, Is.EqualTo("sites"));
    }

    [Test]
    public void SiteConstructor_WhenWeightIsPositiveInfinity_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new Site(new PointXY(0f, 0f), float.PositiveInfinity));
    }

    [Test]
    public void Partition_AssignsEachItemToClosestSite()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(10f, 0f), 1f)
        };
        var texels = new[]
        {
            new TestItem("left", new PointXY(1f, 0f)),
            new TestItem("right", new PointXY(9f, 0f))
        };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites);

        var cells = partitioner.Partition(texels);

        Assert.That(cells, Has.Count.EqualTo(2));
        Assert.That(cells[0].Items.Select(item => item.Id), Is.EqualTo(new[] { "left" }));
        Assert.That(cells[1].Items.Select(item => item.Id), Is.EqualTo(new[] { "right" }));
    }

    [Test]
    public void Partition_WhenItemsContainNull_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var texels = new TestItem[] { null! };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var exception = Assert.Throws<ArgumentException>(() => partitioner.Partition(texels));

        Assert.That(exception!.ParamName, Is.EqualTo("items"));
    }

    [Test]
    public void Partition_WhenItemPositionCoordinateIsInvalid_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var texels = new[] { new TestItem("invalid", new PointXY(float.PositiveInfinity, 0f)) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var exception = Assert.Throws<ArgumentException>(() => partitioner.Partition(texels));

        Assert.That(exception!.ParamName, Is.EqualTo("items"));
    }

    [Test]
    public void Partition_WhenCellItemsAccessed_ReturnsReadOnlyView()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var texels = new[] { new TestItem("item", new PointXY(0f, 0f)) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells[0].Items, Is.Not.InstanceOf<List<TestItem>>());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<TestItem>)cells[0].Items)[0] = new TestItem("replacement", new PointXY(1f, 1f)));
    }

    [Test]
    public void VoronoiItemPartition_WhenItemListChangesAfterConstruction_UsesOriginalItems()
    {
        var items = new List<TestItem> { new TestItem("original", new PointXY(0f, 0f)) };
        var cell = new VoronoiItemPartition<TestItem>(new Site(new PointXY(0f, 0f), 1f), items);

        items.Clear();

        Assert.That(cell.Items, Has.Count.EqualTo(1));
        Assert.That(cell.Items[0].Id, Is.EqualTo("original"));
    }

    [Test]
    public void VoronoiItemPartition_WhenItemsContainNull_Throws()
    {
        var items = new TestItem[] { null! };

        var exception = Assert.Throws<ArgumentException>(() =>
            new VoronoiItemPartition<TestItem>(new Site(new PointXY(0f, 0f), 1f), items));

        Assert.That(exception!.ParamName, Is.EqualTo("items"));
    }

    [Test]
    public void Partition_WhenSiteHasLargerWeight_AssignsFartherItemToIt()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(10f, 0f), 3f)
        };
        var texels = new[]
        {
            new TestItem("weighted-right", new PointXY(3f, 0f))
        };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells[0].Items, Is.Empty);
        Assert.That(cells[1].Items.Select(item => item.Id), Is.EqualTo(new[] { "weighted-right" }));
    }

    [Test]
    public void Partition_WhenSiteWeightIsZero_AssignsOnlyExactSitePointToIt()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 0f),
            new Site(new PointXY(10f, 0f), 1f)
        };
        var texels = new[]
        {
            new TestItem("exact-zero", new PointXY(0f, 0f)),
            new TestItem("near-zero", new PointXY(1f, 0f))
        };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells[0].Items.Select(item => item.Id), Is.EqualTo(new[] { "exact-zero" }));
        Assert.That(cells[1].Items.Select(item => item.Id), Is.EqualTo(new[] { "near-zero" }));
    }

    [Test]
    public void Partition_WhenSiteWeightIsPositiveInfinity_AssignsFinitePointsToIt()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(10f, 0f), float.PositiveInfinity)
        };
        var texels = new[]
        {
            new TestItem("far-from-infinite", new PointXY(-100f, 0f)),
            new TestItem("near-infinite", new PointXY(9f, 0f))
        };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells[0].Items, Is.Empty);
        Assert.That(cells[1].Items.Select(item => item.Id), Is.EqualTo(new[] { "far-from-infinite", "near-infinite" }));
    }

    [Test]
    public void Partition_WhenPointMatchesFiniteSiteAndInfiniteSiteExists_AssignsExactSite()
    {
        var sites = new[]
        {
            new Site(new PointXY(10f, 0f), float.PositiveInfinity),
            new Site(new PointXY(0f, 0f), 1f)
        };
        var texels = new[]
        {
            new TestItem("exact-finite", new PointXY(0f, 0f))
        };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells[0].Items, Is.Empty);
        Assert.That(cells[1].Items.Select(item => item.Id), Is.EqualTo(new[] { "exact-finite" }));
    }

    [Test]
    public void Partition_WhenMultipleInfiniteSitesExist_AssignsNearestInfiniteSite()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 100f), 100f),
            new Site(new PointXY(0f, 0f), float.PositiveInfinity),
            new Site(new PointXY(10f, 0f), float.PositiveInfinity)
        };
        var texels = new[]
        {
            new TestItem("nearest-infinite", new PointXY(9f, 0f))
        };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells[0].Items, Is.Empty);
        Assert.That(cells[1].Items, Is.Empty);
        Assert.That(cells[2].Items.Select(item => item.Id), Is.EqualTo(new[] { "nearest-infinite" }));
    }

    [Test]
    public void Partition_WhenTexelsIsNull_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites);

        Assert.Throws<ArgumentNullException>(() => partitioner.Partition(null!));
    }

    [Test]
    public void Partition_WhenTexelsIsEmpty_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites);

        Assert.Throws<ArgumentOutOfRangeException>(() => partitioner.Partition(Array.Empty<TestItem>()));
    }

    [Test]
    public void Partition_WhenPolicyIsThrowExceptionAndCellIsEmpty_Throws()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };
        var texels = new[] { new TestItem("left", new PointXY(1f, 0f)) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites);

        Assert.Throws<InvalidOperationException>(() => partitioner.Partition(texels));
    }

    [Test]
    public void Partition_WhenPolicyIsExclude_RemovesEmptyCells()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };
        var texels = new[] { new TestItem("left", new PointXY(1f, 0f)) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.Exclude);

        var cells = partitioner.Partition(texels);

        Assert.That(cells, Has.Count.EqualTo(1));
        Assert.That(cells[0].Items.Single().Id, Is.EqualTo("left"));
    }

    [Test]
    public void Partition_WhenPolicyIsLeaveAsIs_ReturnsEmptyCells()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };
        var texels = new[] { new TestItem("left", new PointXY(1f, 0f)) };
        var partitioner = new VoronoiItemPartitioner<TestItem>(sites, EmptyCellPolicy.LeaveAsIs);

        var cells = partitioner.Partition(texels);

        Assert.That(cells, Has.Count.EqualTo(2));
        Assert.That(cells[0].Items, Has.Count.EqualTo(1));
        Assert.That(cells[1].Items, Is.Empty);
    }

    private sealed class TestItem : IHasPosition2D
    {
        public TestItem(string id, PointXY position)
        {
            Id = id;
            Position = position;
        }

        public string Id { get; }

        public PointXY Position { get; }
    }
}
