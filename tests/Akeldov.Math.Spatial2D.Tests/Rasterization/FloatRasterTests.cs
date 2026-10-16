using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class FloatRasterTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void BinaryOperators_CreateIndependentResult(bool subtract)
    {
        var resolution = new VectorXYInt(2, 2);
        var left = new FloatRaster(resolution, new[] { 1.5f, -2f, 0f, 4f });
        var right = new FloatRaster(resolution, new[] { 0.5f, 3f, -1f, 4f });

        FloatRaster result = subtract ? left - right : left + right;

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(resolution));
            Assert.That(result.Values, Is.EqualTo(subtract
                ? new[] { 1f, -5f, 1f, 0f }
                : new[] { 2f, 1f, -1f, 8f }));
            Assert.That(result.Values, Is.Not.SameAs(left.Values));
            Assert.That(result.Values, Is.Not.SameAs(right.Values));
            Assert.That(left.Values, Is.EqualTo(new[] { 1.5f, -2f, 0f, 4f }));
            Assert.That(right.Values, Is.EqualTo(new[] { 0.5f, 3f, -1f, 4f }));
        });
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void MixedBinaryOperators_PreserveOperandOrderAndSources(bool subtract, bool baseFirst)
    {
        var resolution = new VectorXYInt(2, 2);
        var raster = new FloatRaster(resolution, new[] { 1.5f, -2f, 0f, 4f });
        var baseRaster = new Raster<float>(resolution, new[] { 0.5f, 3f, -1f, 4f });

        FloatRaster result = subtract
            ? (baseFirst ? baseRaster - raster : raster - baseRaster)
            : (baseFirst ? baseRaster + raster : raster + baseRaster);

        float[] expected = subtract
            ? (baseFirst ? new[] { -1f, 5f, -1f, 0f } : new[] { 1f, -5f, 1f, 0f })
            : new[] { 2f, 1f, -1f, 8f };

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(resolution));
            Assert.That(result.Values, Is.EqualTo(expected));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(result.Values, Is.Not.SameAs(baseRaster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 1.5f, -2f, 0f, 4f }));
            Assert.That(baseRaster.Values, Is.EqualTo(new[] { 0.5f, 3f, -1f, 4f }));
        });
    }

    [TestCase(false, false, 3.5f, 0f)]
    [TestCase(false, true, 3.5f, 0f)]
    [TestCase(true, false, -0.5f, -4f)]
    [TestCase(true, true, 0.5f, 4f)]
    public void ScalarOperators_PreserveOperandOrderAndSource(
        bool subtract, bool scalarFirst, float first, float second)
    {
        var raster = new FloatRaster(new VectorXYInt(2, 1), new[] { 1.5f, -2f });

        FloatRaster result = subtract
            ? (scalarFirst ? 2f - raster : raster - 2f)
            : (scalarFirst ? 2f + raster : raster + 2f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(raster.Resolution));
            Assert.That(result.Values, Is.EqualTo(new[] { first, second }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 1.5f, -2f }));
        });
    }

    [Test]
    public void UnaryMinus_CreatesIndependentNegatedRaster()
    {
        var raster = new FloatRaster(new VectorXYInt(2, 1), new[] { 1.5f, -2f });

        FloatRaster result = -raster;

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(raster.Resolution));
            Assert.That(result.Values, Is.EqualTo(new[] { -1.5f, 2f }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 1.5f, -2f }));
        });
    }

    [TestCase(1, 4)]
    [TestCase(4, 1)]
    [TestCase(1, 1)]
    public void BinaryOperators_WhenResolutionsDiffer_Throw(int width, int height)
    {
        var left = new FloatRaster(new VectorXYInt(2, 2), new float[4]);
        var right = new FloatRaster(new VectorXYInt(width, height), new float[width * height]);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (Raster<float>)left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + (Raster<float>)right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (Raster<float>)left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - (Raster<float>)right)!.ParamName, Is.EqualTo("right"));
        });
    }

    [Test]
    public void Operators_WhenRasterIsNull_Throw()
    {
        var raster = new FloatRaster(new VectorXYInt(1, 1), new float[1]);
        FloatRaster missing = null!;
        var baseRaster = new Raster<float>(raster.Resolution, new float[1]);
        Raster<float> missingBase = null!;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster + missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster - missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + 2f)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = 2f + missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - 2f)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = 2f - missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = -missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + baseRaster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = baseRaster + missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingBase + raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster + missingBase)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - baseRaster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = baseRaster - missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingBase - raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster - missingBase)!.ParamName, Is.EqualTo("right"));
        });
    }
}
