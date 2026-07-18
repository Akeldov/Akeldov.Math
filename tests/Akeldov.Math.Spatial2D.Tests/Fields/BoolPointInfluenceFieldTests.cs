using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class BoolPointInfluenceFieldTests
{
    [Test]
    public void Sample_WhenUsingNearestSampler_ReturnsNearestSourceValue()
    {
        var sources = new[]
        {
            new BoolPointInfluenceSource(1f, new PointXY(0f, 0f), false),
            new BoolPointInfluenceSource(1f, new PointXY(10f, 0f), true)
        };
        var field = new BoolPointInfluenceField(
            new NearestInfluenceSampler<BoolPointInfluenceSource, bool>(),
            sources);

        bool value = field.Sample(new PointXY(9f, 0f));

        Assert.That(value, Is.True);
    }

    [Test]
    public void Sample_WithCuller_UsesSelectedSources()
    {
        var falseSource = new BoolPointInfluenceSource(1f, new PointXY(0f, 0f), false);
        var trueSource = new BoolPointInfluenceSource(1f, new PointXY(10f, 0f), true);
        var field = new BoolPointInfluenceField(
            new NearestInfluenceSampler<BoolPointInfluenceSource, bool>(),
            new[] { falseSource, trueSource },
            new FixedCuller(new List<BoolPointInfluenceSource> { falseSource }));

        bool value = field.Sample(new PointXY(10f, 0f));

        Assert.That(value, Is.False);
    }

    [Test]
    public void DistinctValues_WhenSourcesContainBothValues_ReturnsReadOnlyFirstOccurrenceOrder()
    {
        var field = new BoolPointInfluenceField(
            new NearestInfluenceSampler<BoolPointInfluenceSource, bool>(),
            new[]
            {
                new BoolPointInfluenceSource(1f, new PointXY(0f, 0f), true),
                new BoolPointInfluenceSource(1f, new PointXY(1f, 0f), false),
                new BoolPointInfluenceSource(1f, new PointXY(2f, 0f), true)
            });

        Assert.That(field.DistinctValues, Is.EqualTo(new[] { true, false }));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<bool>)field.DistinctValues)[0] = false);
    }

    [Test]
    public void Constructor_WhenSourceListChanges_UsesOriginalSourcesAndValues()
    {
        var sources = new List<BoolPointInfluenceSource>
        {
            new BoolPointInfluenceSource(1f, new PointXY(0f, 0f), false)
        };
        var field = new BoolPointInfluenceField(
            new NearestInfluenceSampler<BoolPointInfluenceSource, bool>(),
            sources);

        sources.Clear();
        sources.Add(new BoolPointInfluenceSource(1f, new PointXY(0f, 0f), true));

        Assert.That(field.InfluenceSources, Has.Count.EqualTo(1));
        Assert.That(field.DistinctValues, Is.EqualTo(new[] { false }));
        Assert.That(field.Sample(new PointXY(0f, 0f)), Is.False);
    }

    private sealed class FixedCuller : IInfluenceSourceCuller<BoolPointInfluenceSource>
    {
        private readonly List<BoolPointInfluenceSource> _sources;

        public FixedCuller(List<BoolPointInfluenceSource> sources)
        {
            _sources = sources;
        }

        public List<BoolPointInfluenceSource> Cull(PointXY point)
        {
            return _sources;
        }
    }
}
