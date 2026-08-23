using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class InfluenceFieldCullingTests
{
    [Test]
    public void Sample_WhenIndexReturnsSubset_UsesSelectedSources()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f),
            new FloatPointInfluenceSource(1f, new PointXY(1f, 1f), 20f)
        };

        var field = new InfluenceField<FloatPointInfluenceSource, float>(
            new SourceCountSampler(),
            new FixedIndex(sources, new List<FloatPointInfluenceSource> { sources[0] }));

        float value = field.Sample(new PointXY(3f, 4f));

        Assert.That(value, Is.EqualTo(1f));
    }

    [Test]
    public void Constructor_WithIndex_UsesIndexOwnedSnapshot()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f)
        };
        var index = new FixedIndex(sources, new List<FloatPointInfluenceSource> { sources[0] });

        var field = new InfluenceField<FloatPointInfluenceSource, float>(
            new SourceCountSampler(),
            index);

        Assert.That(field.InfluenceSources, Is.SameAs(index.Sources));
    }

    [Test]
    public void Constructor_WhenInfluenceSourcesContainNull_Throws()
    {
        var sources = new FloatPointInfluenceSource[] { null! };

        var exception = Assert.Throws<ArgumentException>(() =>
            new InfluenceField<FloatPointInfluenceSource, float>(
                new SourceCountSampler(),
                sources));

        Assert.That(exception!.ParamName, Is.EqualTo("influenceSources"));
    }

    [Test]
    public void Sample_WhenIndexReturnsNull_ThrowsInvalidOperationException()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f)
        };

        var field = new InfluenceField<FloatPointInfluenceSource, float>(
            new SourceCountSampler(),
            new FixedIndex(sources, null));

        var exception = Assert.Throws<InvalidOperationException>(() => field.Sample(new PointXY(0f, 0f)));

        Assert.That(exception!.Message, Does.Contain("returned null"));
    }

    [Test]
    public void Sample_WhenIndexReturnsEmptyList_ThrowsInvalidOperationException()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f)
        };

        var field = new InfluenceField<FloatPointInfluenceSource, float>(
            new SourceCountSampler(),
            new FixedIndex(sources, new List<FloatPointInfluenceSource>()));

        var exception = Assert.Throws<InvalidOperationException>(() => field.Sample(new PointXY(0f, 0f)));

        Assert.That(exception!.Message, Does.Contain("empty source list"));
    }

    [Test]
    public void Sample_WhenIndexReturnsListContainingNull_ThrowsInvalidOperationException()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f)
        };
        var field = new InfluenceField<FloatPointInfluenceSource, float>(
            new SourceCountSampler(),
            new FixedIndex(sources, new List<FloatPointInfluenceSource> { null! }));

        var exception = Assert.Throws<InvalidOperationException>(() => field.Sample(new PointXY(0f, 0f)));

        Assert.That(exception!.Message, Does.Contain("containing null"));
    }

    [Test]
    public void Sample_WhenPointCoordinateIsInvalid_Throws()
    {
        var sources = new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 10f)
        };

        var field = new InfluenceField<FloatPointInfluenceSource, float>(
            new SourceCountSampler(),
            sources);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            field.Sample(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Constructor_WhenHalfPlaneIndexPointSourcesAreEmpty_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(Array.Empty<FloatPointInfluenceSource>()));

        Assert.That(exception!.ParamName, Is.EqualTo("pointSources"));
    }

    private sealed class SourceCountSampler : IInfluenceSampler<FloatPointInfluenceSource, float>
    {
        public float Sample(IReadOnlyList<FloatPointInfluenceSource> influenceSources, PointXY point)
        {
            return influenceSources.Count;
        }
    }

    private sealed class FixedIndex : IInfluenceSourceIndex<FloatPointInfluenceSource>
    {
        private readonly List<FloatPointInfluenceSource>? _selectedSources;

        public FixedIndex(
            IReadOnlyList<FloatPointInfluenceSource> sources,
            List<FloatPointInfluenceSource>? selectedSources)
        {
            Sources = Array.AsReadOnly(sources.ToArray());
            _selectedSources = selectedSources;
        }

        public IReadOnlyList<FloatPointInfluenceSource> Sources { get; }

        public List<FloatPointInfluenceSource> SelectSources(PointXY point)
        {
            return _selectedSources!;
        }
    }
}
