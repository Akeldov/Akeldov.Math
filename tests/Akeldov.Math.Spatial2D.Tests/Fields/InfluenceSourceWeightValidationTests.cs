using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class InfluenceSourceWeightValidationTests
{
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void FloatPointInfluenceSource_WhenWeightIsInvalid_Throws(float weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatPointInfluenceSource(weight, new PointXY(0f, 0f), 1f));
    }

    [TestCase(0f)]
    [TestCase(float.PositiveInfinity)]
    public void FloatPointInfluenceSource_WhenWeightIsAllowed_DoesNotThrow(float weight)
    {
        Assert.DoesNotThrow(() =>
            new FloatPointInfluenceSource(weight, new PointXY(0f, 0f), 1f));
    }

    [Test]
    public void FloatPointInfluenceSource_WhenValueIsNaN_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), float.NaN));

        Assert.That(exception!.ParamName, Is.EqualTo("value"));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void FloatPointInfluenceSource_WhenPositionCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatPointInfluenceSource(1f, new PointXY(x, y), 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("position"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void IntPointInfluenceSource_WhenWeightIsInvalid_Throws(float weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IntPointInfluenceSource(weight, new PointXY(0f, 0f), 1));
    }

    [TestCase(0f)]
    [TestCase(float.PositiveInfinity)]
    public void IntPointInfluenceSource_WhenWeightIsAllowed_DoesNotThrow(float weight)
    {
        Assert.DoesNotThrow(() =>
            new IntPointInfluenceSource(weight, new PointXY(0f, 0f), 1));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void IntPointInfluenceSource_WhenPositionCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IntPointInfluenceSource(1f, new PointXY(x, y), 1));

        Assert.That(exception!.ParamName, Is.EqualTo("position"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void BoolPointInfluenceSource_WhenWeightIsInvalid_Throws(float weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BoolPointInfluenceSource(weight, new PointXY(0f, 0f), true));
    }

    [TestCase(0f)]
    [TestCase(float.PositiveInfinity)]
    public void BoolPointInfluenceSource_WhenWeightIsAllowed_DoesNotThrow(float weight)
    {
        Assert.DoesNotThrow(() =>
            new BoolPointInfluenceSource(weight, new PointXY(0f, 0f), true));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void BoolPointInfluenceSource_WhenPositionCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BoolPointInfluenceSource(1f, new PointXY(x, y), true));

        Assert.That(exception!.ParamName, Is.EqualTo("position"));
    }

    [Test]
    public void PointInfluenceSourceDistance_WhenPointCoordinateIsInvalid_Throws()
    {
        var source = new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            source.Distance(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void FloatCurveInfluenceSource_WhenConstantWeightIsInvalid_Throws(float weight)
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatCurveInfluenceSource(weight, curve, 1f));
    }

    [TestCase(0f)]
    [TestCase(float.PositiveInfinity)]
    public void FloatCurveInfluenceSource_WhenConstantWeightIsAllowed_DoesNotThrow(float weight)
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));

        Assert.DoesNotThrow(() =>
            new FloatCurveInfluenceSource(weight, curve, 1f));
    }

    [Test]
    public void FloatCurveInfluenceSource_WhenConstantValueIsNaN_Throws()
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FloatCurveInfluenceSource(1f, curve, float.NaN));

        Assert.That(exception!.ParamName, Is.EqualTo("value"));
    }

    [Test]
    public void FloatCurveInfluenceSource_WhenValueProviderReturnsNaN_Throws()
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));
        var source = new FloatCurveInfluenceSource(1f, curve, _ => float.NaN);

        Assert.Throws<InvalidOperationException>(() =>
            source.GetInfluence(new PointXY(0f, 0f)));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void FloatCurveInfluenceSource_WhenWeightProviderReturnsInvalidWeight_Throws(float weight)
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));
        var source = new FloatCurveInfluenceSource(_ => weight, curve, 1f);

        Assert.Throws<InvalidOperationException>(() =>
            source.GetInfluence(new PointXY(0f, 0f)));
    }

    [TestCase(0f)]
    [TestCase(float.PositiveInfinity)]
    public void FloatCurveInfluenceSource_WhenWeightProviderReturnsAllowedWeight_ReturnsWeight(float weight)
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));
        var source = new FloatCurveInfluenceSource(_ => weight, curve, 1f);

        var influence = source.GetInfluence(new PointXY(0f, 0f));

        Assert.That(influence.Weight, Is.EqualTo(weight));
    }

    [Test]
    public void FloatCurveInfluenceSource_WhenPointCoordinateIsInvalid_Throws()
    {
        var curve = new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 1f));
        var source = new FloatCurveInfluenceSource(1f, curve, 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            source.GetInfluence(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void InfluenceSample_WhenWeightIsInvalid_Throws(float weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InfluenceSample<float>(1f, new PointXY(0f, 0f), 0f, weight));
    }

    [TestCase(0f)]
    [TestCase(float.PositiveInfinity)]
    public void InfluenceSample_WhenWeightIsAllowed_DoesNotThrow(float weight)
    {
        Assert.DoesNotThrow(() =>
            new InfluenceSample<float>(1f, new PointXY(0f, 0f), 0f, weight));
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void InfluenceSample_WhenDistanceIsInvalid_Throws(float distance)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InfluenceSample<float>(1f, new PointXY(0f, 0f), distance, 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("distance"));
    }

    [TestCase(0f)]
    [TestCase(1f)]
    public void InfluenceSample_WhenDistanceIsAllowed_DoesNotThrow(float distance)
    {
        Assert.DoesNotThrow(() =>
            new InfluenceSample<float>(1f, new PointXY(0f, 0f), distance, 1f));
    }

    [TestCase(float.PositiveInfinity, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void InfluenceSample_WhenSourcePointCoordinateIsInvalid_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new InfluenceSample<float>(1f, new PointXY(x, y), 0f, 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("sourcePoint"));
    }

    [Test]
    public void InfluenceSample_WhenCreated_StoresSourcePoint()
    {
        var sourcePoint = new PointXY(2f, 3f);

        var sample = new InfluenceSample<float>(1f, sourcePoint, 4f, 5f);

        Assert.That(sample.SourcePoint, Is.EqualTo(sourcePoint));
    }
}
