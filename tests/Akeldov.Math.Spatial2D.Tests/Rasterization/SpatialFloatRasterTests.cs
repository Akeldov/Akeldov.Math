using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class SpatialFloatRasterTests
{
    [Test]
    public void Constructor_RetainsArrayAndImplementsSpatialContract()
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1));
        var values = new[] { 1f, 2f };
        var raster = new SpatialFloatRaster(geometry, values);
        FloatRaster floatRaster = raster;
        ISpatialRaster<float> spatialRaster = raster;

        floatRaster[1] = 7f;

        Assert.Multiple(() =>
        {
            Assert.That(raster.Values, Is.SameAs(values));
            Assert.That(spatialRaster.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialRaster.Resolution, Is.EqualTo(geometry.Resolution));
            Assert.That(spatialRaster[1], Is.EqualTo(7f));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_WhenGeometryIsInvalidOrTooLarge_Throws(bool tooLarge)
    {
        RasterGeometry geometry = tooLarge
            ? new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, new VectorXYInt(50_000, 50_000))
            : default;

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialFloatRaster(geometry, Array.Empty<float>()))!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void ImplicitConversion_SharesArrayAndPreservesGeometry()
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1));
        var raster = new SpatialFloatRaster(geometry, new[] { 1f, 2f });

        SpatialRaster<float> converted = raster;
        converted[0] = 7f;
        raster[1] = 9f;

        Assert.Multiple(() =>
        {
            Assert.That(converted.Geometry, Is.EqualTo(geometry));
            Assert.That(converted.Values, Is.SameAs(raster.Values));
            Assert.That(raster[0], Is.EqualTo(7f));
            Assert.That(converted[1], Is.EqualTo(9f));
        });
    }

    [Test]
    public void ImplicitConversion_WhenSourceIsNull_ReturnsNull()
    {
        SpatialFloatRaster? source = null;

        SpatialRaster<float>? converted = source;

        Assert.That(converted, Is.Null);
    }

    [TestCase(4f, -5f, 8f, 4f)]
    [TestCase(3f, -4f, 8f, 4f)]
    [TestCase(3f, -5f, 4f, 4f)]
    [TestCase(3f, -5f, 8f, 8f)]
    public void SpatialOperators_WhenBoundsDiffer_Throw(float x, float y, float width, float height)
    {
        var leftGeometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 2));
        var rightGeometry = new RasterGeometry(new PointXY(x, y), new VectorXY(width, height), leftGeometry.Resolution);
        var left = new SpatialFloatRaster(leftGeometry, new float[4]);
        var right = new SpatialFloatRaster(rightGeometry, new float[4]);
        SpatialRaster<float> baseLeft = left;
        SpatialRaster<float> baseRight = right;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = baseLeft + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = baseLeft - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + baseRight)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - baseRight)!.ParamName, Is.EqualTo("right"));
        });
    }

    [Test]
    public void NonSpatialOperators_PreserveGeometryAndSources(
        [Values] bool subtract, [Values] bool baseFirst, [Values] bool generic)
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 2));
        var raster = new SpatialFloatRaster(geometry, new[] { 1.5f, -2f, 0f, 4f });
        var plain = new FloatRaster(geometry.Resolution, new[] { 0.5f, 3f, -1f, 4f });
        Raster<float> baseRaster = plain;

        SpatialFloatRaster result = generic
            ? (subtract ? (baseFirst ? baseRaster - raster : raster - baseRaster) : (baseFirst ? baseRaster + raster : raster + baseRaster))
            : (subtract ? (baseFirst ? plain - raster : raster - plain) : (baseFirst ? plain + raster : raster + plain));

        float[] expected = subtract
            ? (baseFirst ? new[] { -1f, 5f, -1f, 0f } : new[] { 1f, -5f, 1f, 0f })
            : new[] { 2f, 1f, -1f, 8f };

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Values, Is.EqualTo(expected));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(result.Values, Is.Not.SameAs(plain.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 1.5f, -2f, 0f, 4f }));
            Assert.That(plain.Values, Is.EqualTo(new[] { 0.5f, 3f, -1f, 4f }));
        });
    }

    [Test]
    public void NonSpatialOperators_WhenResolutionsDiffer_Throw()
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 2));
        var raster = new SpatialFloatRaster(geometry, new float[4]);
        var plain = new FloatRaster(new VectorXYInt(1, 4), new float[4]);
        Raster<float> baseRaster = plain;

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => _ = raster + plain);
            Assert.Throws<ArgumentException>(() => _ = plain + raster);
            Assert.Throws<ArgumentException>(() => _ = raster - plain);
            Assert.Throws<ArgumentException>(() => _ = plain - raster);
            Assert.Throws<ArgumentException>(() => _ = raster + baseRaster);
            Assert.Throws<ArgumentException>(() => _ = baseRaster + raster);
            Assert.Throws<ArgumentException>(() => _ = raster - baseRaster);
            Assert.Throws<ArgumentException>(() => _ = baseRaster - raster);
        });
    }

    [Test]
    public void NonSpatialOperators_WhenRasterIsNull_Throw(
        [Values] bool subtract, [Values] bool baseFirst, [Values] bool generic, [Values] bool spatialNull)
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), VectorXYInt.One);
        SpatialFloatRaster raster = spatialNull ? null! : new SpatialFloatRaster(geometry, new float[1]);
        FloatRaster plain = spatialNull ? new FloatRaster(geometry.Resolution, new float[1]) : null!;
        Raster<float> baseRaster = plain;

        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            SpatialFloatRaster result = generic
                ? (subtract ? (baseFirst ? baseRaster - raster : raster - baseRaster) : (baseFirst ? baseRaster + raster : raster + baseRaster))
                : (subtract ? (baseFirst ? plain - raster : raster - plain) : (baseFirst ? plain + raster : raster + plain));
        });

        Assert.That(exception!.ParamName, Is.EqualTo(spatialNull == baseFirst ? "right" : "left"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void BinaryOperators_CreateIndependentResult(bool subtract)
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 2));
        var left = new SpatialFloatRaster(geometry, new[] { 1.5f, -2f, 0f, 4f });
        var right = new SpatialFloatRaster(geometry, new[] { 0.5f, 3f, -1f, 4f });

        SpatialFloatRaster result = subtract ? left - right : left + right;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
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
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 2));
        var raster = new SpatialFloatRaster(geometry, new[] { 1.5f, -2f, 0f, 4f });
        var baseRaster = new SpatialRaster<float>(geometry, new[] { 0.5f, 3f, -1f, 4f });

        SpatialFloatRaster result = subtract
            ? (baseFirst ? baseRaster - raster : raster - baseRaster)
            : (baseFirst ? baseRaster + raster : raster + baseRaster);

        float[] expected = subtract
            ? (baseFirst ? new[] { -1f, 5f, -1f, 0f } : new[] { 1f, -5f, 1f, 0f })
            : new[] { 2f, 1f, -1f, 8f };

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
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
        var raster = new SpatialFloatRaster(new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1)), new[] { 1.5f, -2f });

        SpatialFloatRaster result = subtract
            ? (scalarFirst ? 2f - raster : raster - 2f)
            : (scalarFirst ? 2f + raster : raster + 2f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(raster.Geometry));
            Assert.That(result.Values, Is.EqualTo(new[] { first, second }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 1.5f, -2f }));
        });
    }

    [Test]
    public void UnaryMinus_CreatesIndependentNegatedRaster()
    {
        var raster = new SpatialFloatRaster(new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1)), new[] { 1.5f, -2f });

        SpatialFloatRaster result = -raster;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(raster.Geometry));
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
        var left = new SpatialFloatRaster(new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 2)), new float[4]);
        var right = new SpatialFloatRaster(new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(width, height)), new float[width * height]);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (SpatialRaster<float>)left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + (SpatialRaster<float>)right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (SpatialRaster<float>)left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - (SpatialRaster<float>)right)!.ParamName, Is.EqualTo("right"));
        });
    }

    [Test]
    public void Operators_WhenRasterIsNull_Throw()
    {
        var raster = new SpatialFloatRaster(new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(1, 1)), new float[1]);
        SpatialFloatRaster missing = null!;
        var baseRaster = new SpatialRaster<float>(raster.Geometry, new float[1]);
        SpatialRaster<float> missingBase = null!;

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
