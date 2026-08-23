using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class InfluenceFieldClampingTests
{
    [Test]
    public void FloatPointInfluenceField_WhenSourceListChangesAfterConstruction_UsesOriginalSources()
    {
        var sources = new List<FloatPointInfluenceSource>
        {
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 10f)
        };
        var field = new FloatPointInfluenceField(
            new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
            sources);

        sources.Clear();
        sources.Add(new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 100f));

        Assert.That(field.InfluenceSources, Has.Count.EqualTo(2));
        Assert.That(field.Min, Is.EqualTo(0f));
        Assert.That(field.Max, Is.EqualTo(10f));
        Assert.That(field.Sample(new PointXY(10f, 0f)), Is.EqualTo(10f));
    }

    [Test]
    public void FloatPointInfluenceField_WhenInfluenceSourcesAccessed_ReturnsReadOnlyView()
    {
        var field = new FloatPointInfluenceField(
            new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
            new[]
            {
                new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
                new FloatPointInfluenceSource(1f, new PointXY(1f, 1f), 5f)
            });

        Assert.That(field.InfluenceSources, Is.Not.InstanceOf<FloatPointInfluenceSource[]>());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<FloatPointInfluenceSource>)field.InfluenceSources)[0] =
                new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 100f));
    }

    [Test]
    public void FloatPointInfluenceField_WhenDistinctValuesAccessed_ReturnsReadOnlyView()
    {
        var field = new FloatPointInfluenceField(
            new NearestFloatInfluenceSampler<FloatPointInfluenceSource>(),
            new[]
            {
                new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
                new FloatPointInfluenceSource(1f, new PointXY(1f, 1f), 5f)
            });

        Assert.That(field.DistinctValues, Is.Not.InstanceOf<List<float>>());
        Assert.Throws<NotSupportedException>(() => ((IList<float>)field.DistinctValues)[0] = 100f);
    }

    [Test]
    public void IntPointInfluenceField_WhenDistinctValuesAccessed_ReturnsReadOnlyView()
    {
        var field = new IntPointInfluenceField(
            new NearestIntInfluenceSampler<IntPointInfluenceSource>(),
            new[]
            {
                new IntPointInfluenceSource(1f, new PointXY(0f, 0f), 2),
                new IntPointInfluenceSource(1f, new PointXY(1f, 1f), 7)
            });

        Assert.That(field.DistinctValues, Is.Not.InstanceOf<List<int>>());
        Assert.Throws<NotSupportedException>(() => ((IList<int>)field.DistinctValues)[0] = 100);
    }

    [Test]
    public void FloatPointInfluenceField_WhenSamplerReturnsBelowMin_ClampsToMin()
    {
        var field = new FloatPointInfluenceField(
            new ConstantSampler<FloatPointInfluenceSource, float>(-10f),
            new[]
            {
                new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
                new FloatPointInfluenceSource(1f, new PointXY(1f, 1f), 5f)
            });

        float value = field.Sample(new PointXY(10f, 10f));

        Assert.That(value, Is.EqualTo(0f));
    }

    [Test]
    public void FloatPointInfluenceField_WhenSamplerReturnsNaN_Throws()
    {
        var field = new FloatPointInfluenceField(
            new ConstantSampler<FloatPointInfluenceSource, float>(float.NaN),
            new[]
            {
                new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
                new FloatPointInfluenceSource(1f, new PointXY(1f, 1f), 5f)
            });

        Assert.Throws<InvalidOperationException>(() => field.Sample(new PointXY(10f, 10f)));
    }

    [Test]
    public void IntPointInfluenceField_WhenSamplerReturnsAboveMax_ClampsToMax()
    {
        var field = new IntPointInfluenceField(
            new ConstantSampler<IntPointInfluenceSource, int>(30),
            new[]
            {
                new IntPointInfluenceSource(1f, new PointXY(0f, 0f), 2),
                new IntPointInfluenceSource(1f, new PointXY(1f, 1f), 7)
            });

        int value = field.Sample(new PointXY(10f, 10f));

        Assert.That(value, Is.EqualTo(7));
    }

    [Test]
    public void FloatCurveInfluenceField_WhenSamplerReturnsOutsideRange_ClampsToRange()
    {
        var field = new FloatCurveInfluenceField(
            new ConstantSampler<ICurveInfluenceSource<float>, float>(100f),
            CreateCurveSources(),
            min: -2f,
            max: 3f);

        float value = field.Sample(new PointXY(10f, 10f));

        Assert.That(value, Is.EqualTo(3f));
    }

    [Test]
    public void FloatCurveInfluenceField_WhenSamplerReturnsNaN_Throws()
    {
        var field = new FloatCurveInfluenceField(
            new ConstantSampler<ICurveInfluenceSource<float>, float>(float.NaN),
            CreateCurveSources(),
            min: -2f,
            max: 3f);

        Assert.Throws<InvalidOperationException>(() => field.Sample(new PointXY(10f, 10f)));
    }

    [TestCase(3f, 2f, "min")]
    [TestCase(float.NaN, 2f, "min")]
    [TestCase(1f, float.NaN, "max")]
    public void FloatCurveInfluenceField_WhenRangeIsInvalid_Throws(float min, float max, string paramName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatCurveInfluenceField(
                new ConstantSampler<ICurveInfluenceSource<float>, float>(0f),
                CreateCurveSources(),
                min,
                max));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [TestCase(3f, 2f, "min")]
    [TestCase(float.NaN, 2f, "min")]
    [TestCase(1f, float.NaN, "max")]
    public void FloatCurveInfluenceField_WithIndex_WhenRangeIsInvalid_Throws(float min, float max, string paramName)
    {
        var sources = CreateCurveSources();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatCurveInfluenceField(
                new ConstantSampler<ICurveInfluenceSource<float>, float>(0f),
                new FixedCurveIndex(sources),
                min,
                max));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    private static ICurveInfluenceSource<float>[] CreateCurveSources()
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(10f, 0f));

        return new ICurveInfluenceSource<float>[]
        {
            new FloatCurveInfluenceSource(1f, curve, 0f)
        };
    }

    private sealed class ConstantSampler<TSource, TValue> : IInfluenceSampler<TSource, TValue>
        where TSource : IInfluenceSource<TValue>
    {
        private readonly TValue _value;

        public ConstantSampler(TValue value)
        {
            _value = value;
        }

        public TValue Sample(IReadOnlyList<TSource> influenceSources, PointXY point)
        {
            return _value;
        }
    }

    private sealed class FixedCurveIndex : IInfluenceSourceIndex<ICurveInfluenceSource<float>>
    {
        public FixedCurveIndex(IReadOnlyList<ICurveInfluenceSource<float>> sources)
        {
            Sources = Array.AsReadOnly(sources.ToArray());
        }

        public IReadOnlyList<ICurveInfluenceSource<float>> Sources { get; }

        public List<ICurveInfluenceSource<float>> SelectSources(PointXY point)
        {
            return new List<ICurveInfluenceSource<float>> { Sources[0] };
        }
    }
}
