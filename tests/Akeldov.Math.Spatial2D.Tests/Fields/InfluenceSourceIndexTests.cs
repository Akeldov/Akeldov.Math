using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class InfluenceSourceIndexTests
{
    [Test]
    public void HalfPlaneIndex_WithSingleSource_ReturnsThatSource()
    {
        var source = new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f);
        var index = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(new[] { source });

        List<FloatPointInfluenceSource> selectedSources = index.SelectSources(new PointXY(3f, 4f));

        Assert.That(selectedSources, Has.Count.EqualTo(1));
        Assert.That(selectedSources[0], Is.SameAs(source));
    }

    [Test]
    public void HalfPlaneIndex_WhenSourceListChangesAfterConstruction_UsesOriginalSnapshot()
    {
        var source = new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f);
        var sources = new List<FloatPointInfluenceSource> { source };
        var index = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        sources.Clear();

        List<FloatPointInfluenceSource> selectedSources = index.SelectSources(new PointXY(3f, 4f));

        Assert.That(index.Sources, Has.Count.EqualTo(1));
        Assert.That(index.Sources[0], Is.SameAs(source));
        Assert.That(selectedSources, Has.Count.EqualTo(1));
        Assert.That(selectedSources[0], Is.SameAs(source));
    }

    [Test]
    public void HalfPlaneIndex_WhenSourcesAccessed_ReturnsReadOnlySnapshot()
    {
        var source = new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f);
        var index = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(new[] { source });

        Assert.Throws<NotSupportedException>(() =>
            ((IList<FloatPointInfluenceSource>)index.Sources)[0] =
                new FloatPointInfluenceSource(1f, new PointXY(1f, 0f), 20f));
    }

    [Test]
    public void HalfPlaneIndex_WhenPointSourcesContainNull_Throws()
    {
        var sources = new FloatPointInfluenceSource[] { null! };

        var exception = Assert.Throws<ArgumentException>(() =>
            new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(sources));

        Assert.That(exception!.ParamName, Is.EqualTo("pointSources"));
    }

    [Test]
    public void HalfPlaneIndex_WhenSourcePositionCoordinateIsInvalid_Throws()
    {
        var sources = new[]
        {
            new TestPointSource(new PointXY(float.PositiveInfinity, 0f))
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            new HalfPlaneInfluenceSourceIndex<TestPointSource>(sources));

        Assert.That(exception!.ParamName, Is.EqualTo("pointSources"));
    }

    [Test]
    public void HalfPlaneIndex_WhenPointCoordinateIsInvalid_Throws()
    {
        var source = new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f);
        var index = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(new[] { source });

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            index.SelectSources(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void DelaunayIndex_WithTriangleContainingPoint_ReturnsTriangleSources()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 3f)
        };

        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        List<FloatPointInfluenceSource> selectedSources = index.SelectSources(new PointXY(2f, 3f));

        Assert.That(selectedSources, Is.EquivalentTo(sources));
    }

    [Test]
    public void DelaunayIndex_WhenSourceListChangesAfterConstruction_UsesOriginalSnapshot()
    {
        var sources = new List<FloatPointInfluenceSource>
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 3f)
        };
        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        sources.Clear();

        Assert.That(index.Sources, Has.Count.EqualTo(3));
        Assert.That(index.SelectSources(new PointXY(2f, 3f)), Is.EquivalentTo(index.Sources));
    }

    [Test]
    public void DelaunayIndex_WhenSourcePointsContainNull_Throws()
    {
        var sources = new FloatPointInfluenceSource[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            null!,
            new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 3f)
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources));

        Assert.That(exception!.ParamName, Is.EqualTo("pointSources"));
    }

    [Test]
    public void DelaunayIndex_WhenSourcePositionCoordinateIsInvalid_Throws()
    {
        var sources = new[]
        {
            new TestPointSource(new PointXY(0f, 0f)),
            new TestPointSource(new PointXY(float.PositiveInfinity, 0f)),
            new TestPointSource(new PointXY(0f, 10f))
        };

        var exception = Assert.Throws<ArgumentException>(() =>
            new DelaunayInfluenceSourceIndex<TestPointSource>(sources));

        Assert.That(exception!.ParamName, Is.EqualTo("pointSources"));
    }

    [Test]
    public void DelaunayIndex_WhenPointCoordinateIsInvalid_Throws()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 3f)
        };
        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            index.SelectSources(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void DelaunayIndex_WhenSourcePointsAreCollinear_UsesHalfPlaneSelection()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(20f, 0f), 3f),
            new FloatPointInfluenceSource(1f, new PointXY(100f, 0f), 4f)
        };

        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
        var halfPlaneIndex = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        List<FloatPointInfluenceSource> selectedSources = index.SelectSources(new PointXY(12f, 0f));
        List<FloatPointInfluenceSource> expectedSources = halfPlaneIndex.SelectSources(new PointXY(12f, 0f));

        AssertSameSources(selectedSources, expectedSources);
        Assert.That(selectedSources, Does.Not.Contain(sources[3]));
    }

    [Test]
    public void DelaunayIndex_WhenPointIsOutsideTriangulationNearHullVertex_ReturnsHullVertex()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 3f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 10f), 4f),
            new FloatPointInfluenceSource(1f, new PointXY(5f, 5f), 5f)
        };

        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        List<FloatPointInfluenceSource> selectedSources = index.SelectSources(new PointXY(-5f, -5f));

        Assert.That(selectedSources, Has.Count.EqualTo(1));
        Assert.That(selectedSources[0], Is.SameAs(sources[0]));
    }

    [Test]
    public void DelaunayIndex_WhenPointIsOutsideTriangulationNearHullEdge_ReturnsHullEdge()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 3f),
            new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 4f),
            new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 5f)
        };

        var point = new PointXY(50f, 0f);
        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
        var halfPlaneIndex = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        List<FloatPointInfluenceSource> halfPlaneSources = halfPlaneIndex.SelectSources(point);
        List<FloatPointInfluenceSource> selectedSources = index.SelectSources(point);

        Assert.That(halfPlaneSources, Has.Count.GreaterThan(2));
        Assert.That(halfPlaneSources, Does.Contain(sources[4]));
        Assert.That(selectedSources, Has.Count.EqualTo(2));
        Assert.That(selectedSources, Does.Contain(sources[0]));
        Assert.That(selectedSources, Does.Contain(sources[1]));
        Assert.That(selectedSources, Does.Not.Contain(sources[4]));
    }

    [Test]
    public void DelaunayIndex_WhenSamplingAboveTopHullEdge_ReturnsThreeExpectedRegions()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 2f),
            new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 3f),
            new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 4f),
            new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 5f)
        };

        var index = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);

        List<FloatPointInfluenceSource> leftRegion = index.SelectSources(new PointXY(0f, 0f));
        List<FloatPointInfluenceSource> edgeRegion = index.SelectSources(new PointXY(50f, 0f));
        List<FloatPointInfluenceSource> rightRegion = index.SelectSources(new PointXY(100f, 0f));

        Assert.That(leftRegion, Has.Count.EqualTo(1));
        Assert.That(leftRegion[0], Is.SameAs(sources[0]));
        Assert.That(edgeRegion, Has.Count.EqualTo(2));
        Assert.That(edgeRegion, Does.Contain(sources[0]));
        Assert.That(edgeRegion, Does.Contain(sources[1]));
        Assert.That(rightRegion, Has.Count.EqualTo(1));
        Assert.That(rightRegion[0], Is.SameAs(sources[1]));
    }

    [Test]
    public void DelaunayIndex_WhenSourcePointCountIsLessThanThree_Throws()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 2f)
        };

        var exception = Assert.Throws<ArgumentException>(
            () => new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources));

        Assert.That(exception!.ParamName, Is.EqualTo("pointSources"));
    }

    private static void AssertSameSources(
        IReadOnlyList<FloatPointInfluenceSource> actual,
        IReadOnlyList<FloatPointInfluenceSource> expected)
    {
        Assert.That(actual, Has.Count.EqualTo(expected.Count));

        for (int i = 0; i < expected.Count; i++)
            Assert.That(actual[i], Is.SameAs(expected[i]));
    }

    private sealed class TestPointSource : IPointInfluenceSource
    {
        public TestPointSource(PointXY position)
        {
            Position = position;
        }

        public PointXY Position { get; }

        public float Weight => 1f;

        public float Distance(PointXY point)
        {
            return Position.Distance(point);
        }
    }
}
