using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class IFloatFieldExtensionsTests
{
    [TestCase(100f, 200f)]
    [TestCase(-100f, -200f)]
    [TestCase(0f, 0f)]
    public void ToSites_WithVariableField_SamplesWorldCoordinates(float minX, float minY)
    {
        var bounds = new Rectangle(new PointXY(minX, minY), new PointXY(minX + 20f, minY + 10f));
        var field = new WorldGradientFloatField(bounds);

        var sites = field.ToSites(bounds, maxPoissonSamplingAttempts: 30, new Random(12345));

        Assert.That(sites.Length, Is.GreaterThan(1));
        foreach (var site in sites)
        {
            Assert.That(bounds.Contains(site.Position), Is.True, $"Site {site.Position} is outside the bounds.");
            Assert.That(site.Weight, Is.EqualTo(field.Sample(site.Position)));
        }
        Assert.That(sites.Select(site => site.Weight).Distinct().Count(), Is.GreaterThan(1));
    }

    [Test]
    public void ToSites_WithNullField_ThrowsArgumentNullException()
    {
        var bounds = new Rectangle(new PointXY(100f, 200f), new PointXY(120f, 210f));

        var exception = Assert.Throws<ArgumentNullException>(() =>
            IFloatFieldExtensions.ToSites(null!, bounds, 30, new Random(12345)));

        Assert.That(exception!.ParamName, Is.EqualTo("minimalDistanceField"));
    }

    private sealed class WorldGradientFloatField : IFloatField
    {
        private readonly Rectangle _bounds;

        public WorldGradientFloatField(Rectangle bounds)
        {
            _bounds = bounds;
        }

        public float Min => 2f;

        public float Max => 4f;

        public float Sample(PointXY point)
        {
            Assert.That(_bounds.Contains(point), Is.True, $"Field sampled outside world bounds at {point}.");
            return Min + (point.X - _bounds.Min.X) / _bounds.Width +
                (point.Y - _bounds.Min.Y) / _bounds.Height;
        }
    }
}
