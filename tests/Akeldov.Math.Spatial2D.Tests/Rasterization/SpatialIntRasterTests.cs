using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class SpatialIntRasterTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void BinaryOperators_WhenLaterCellOverflows_ThrowAndPreserveSources(bool subtract)
    {
        int boundary = subtract ? int.MinValue : int.MaxValue;
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1));
        var left = new SpatialIntRaster(geometry, new[] { 5, boundary });
        var right = new SpatialIntRaster(geometry, new[] { 1, 1 });
        SpatialRaster<int> spatialLeft = left;
        SpatialRaster<int> spatialRight = right;
        IntRaster plainLeft = left;
        IntRaster plainRight = right;
        Raster<int> genericLeft = left;
        Raster<int> genericRight = right;

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = subtract ? left - right : left + right);
            Assert.Throws<OverflowException>(() => _ = subtract ? spatialLeft - right : spatialLeft + right);
            Assert.Throws<OverflowException>(() => _ = subtract ? left - spatialRight : left + spatialRight);
            Assert.Throws<OverflowException>(() => _ = subtract ? plainLeft - right : plainLeft + right);
            Assert.Throws<OverflowException>(() => _ = subtract ? left - plainRight : left + plainRight);
            Assert.Throws<OverflowException>(() => _ = subtract ? genericLeft - right : genericLeft + right);
            Assert.Throws<OverflowException>(() => _ = subtract ? left - genericRight : left + genericRight);
            Assert.That(left.Values, Is.EqualTo(new[] { 5, boundary }));
            Assert.That(right.Values, Is.EqualTo(new[] { 1, 1 }));
        });
    }

    [Test]
    public void ScalarOperators_WhenLaterCellOverflows_ThrowAndPreserveSources()
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1));
        var maximum = new SpatialIntRaster(geometry, new[] { 5, int.MaxValue });
        var minimum = new SpatialIntRaster(geometry, new[] { 5, int.MinValue });

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = maximum + 1);
            Assert.Throws<OverflowException>(() => _ = 1 + maximum);
            Assert.Throws<OverflowException>(() => _ = minimum - 1);
            Assert.Throws<OverflowException>(() => _ = 1 - minimum);
            Assert.That(maximum.Values, Is.EqualTo(new[] { 5, int.MaxValue }));
            Assert.That(minimum.Values, Is.EqualTo(new[] { 5, int.MinValue }));
        });
    }

    [Test]
    public void UnaryMinus_WhenLaterCellIsIntMinValue_ThrowsAndPreservesSource()
    {
        var geometry = new RasterGeometry(new PointXY(3f, -5f), new VectorXY(8f, 4f), new VectorXYInt(2, 1));
        var raster = new SpatialIntRaster(geometry, new[] { 5, int.MinValue });

        Assert.Throws<OverflowException>(() => _ = -raster);

        Assert.That(raster.Values, Is.EqualTo(new[] { 5, int.MinValue }));
    }

    [Test]
    public void Constructor_RetainsArrayAndImplementsSpatialContract()
    {
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 1));
        var values = new[] { 2, 4 };
        var raster = new SpatialIntRaster(geometry, values);
        IntRaster intRaster = raster;
        ISpatialRaster<int> spatialRaster = raster;

        intRaster[1] = 14;

        Assert.Multiple(() =>
        {
            Assert.That(raster.Values, Is.SameAs(values));
            Assert.That(spatialRaster.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialRaster.Resolution, Is.EqualTo(geometry.Resolution));
            Assert.That(spatialRaster[1], Is.EqualTo(14));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_WhenGeometryIsInvalidOrTooLarge_Throws(bool tooLarge)
    {
        RasterGeometry geometry = tooLarge
            ? new RasterGeometry(new PointXY(0, 0), VectorXY.One, new VectorXYInt(50_000, 50_000))
            : default;

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialIntRaster(geometry, Array.Empty<int>()))!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void ImplicitConversion_SharesArrayAndPreservesGeometry()
    {
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 1));
        var raster = new SpatialIntRaster(geometry, new[] { 2, 4 });

        SpatialRaster<int> converted = raster;
        converted[0] = 14;
        raster[1] = 18;

        Assert.Multiple(() =>
        {
            Assert.That(converted.Geometry, Is.EqualTo(geometry));
            Assert.That(converted.Values, Is.SameAs(raster.Values));
            Assert.That(raster[0], Is.EqualTo(14));
            Assert.That(converted[1], Is.EqualTo(18));
        });
    }

    [Test]
    public void ImplicitConversion_WhenSourceIsNull_ReturnsNull()
    {
        SpatialIntRaster? source = null;

        SpatialRaster<int>? converted = source;

        Assert.That(converted, Is.Null);
    }

    [TestCase(8, -10, 16, 8)]
    [TestCase(6, -8, 16, 8)]
    [TestCase(6, -10, 8, 8)]
    [TestCase(6, -10, 16, 16)]
    public void SpatialOperators_WhenBoundsDiffer_Throw(int x, int y, int width, int height)
    {
        var leftGeometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2));
        var rightGeometry = new RasterGeometry(new PointXY(x, y), new VectorXY(width, height), leftGeometry.Resolution);
        var left = new SpatialIntRaster(leftGeometry, new int[4]);
        var right = new SpatialIntRaster(rightGeometry, new int[4]);
        SpatialRaster<int> baseLeft = left;
        SpatialRaster<int> baseRight = right;

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
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2));
        var raster = new SpatialIntRaster(geometry, new[] { 3, -4, 0, 8 });
        var plain = new IntRaster(geometry.Resolution, new[] { 1, 6, -2, 8 });
        Raster<int> baseRaster = plain;

        SpatialIntRaster result = generic
            ? (subtract ? (baseFirst ? baseRaster - raster : raster - baseRaster) : (baseFirst ? baseRaster + raster : raster + baseRaster))
            : (subtract ? (baseFirst ? plain - raster : raster - plain) : (baseFirst ? plain + raster : raster + plain));

        int[] expected = subtract
            ? (baseFirst ? new[] { -2, 10, -2, 0 } : new[] { 2, -10, 2, 0 })
            : new[] { 4, 2, -2, 16 };

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Values, Is.EqualTo(expected));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(result.Values, Is.Not.SameAs(plain.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4, 0, 8 }));
            Assert.That(plain.Values, Is.EqualTo(new[] { 1, 6, -2, 8 }));
        });
    }

    [Test]
    public void NonSpatialOperators_WhenResolutionsDiffer_Throw()
    {
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2));
        var raster = new SpatialIntRaster(geometry, new int[4]);
        var plain = new IntRaster(new VectorXYInt(1, 4), new int[4]);
        Raster<int> baseRaster = plain;

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
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), VectorXYInt.One);
        SpatialIntRaster raster = spatialNull ? null! : new SpatialIntRaster(geometry, new int[1]);
        IntRaster plain = spatialNull ? new IntRaster(geometry.Resolution, new int[1]) : null!;
        Raster<int> baseRaster = plain;

        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            SpatialIntRaster result = generic
                ? (subtract ? (baseFirst ? baseRaster - raster : raster - baseRaster) : (baseFirst ? baseRaster + raster : raster + baseRaster))
                : (subtract ? (baseFirst ? plain - raster : raster - plain) : (baseFirst ? plain + raster : raster + plain));
        });

        Assert.That(exception!.ParamName, Is.EqualTo(spatialNull == baseFirst ? "right" : "left"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void BinaryOperators_CreateIndependentResult(bool subtract)
    {
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2));
        var left = new SpatialIntRaster(geometry, new[] { 3, -4, 0, 8 });
        var right = new SpatialIntRaster(geometry, new[] { 1, 6, -2, 8 });

        SpatialIntRaster result = subtract ? left - right : left + right;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Values, Is.EqualTo(subtract
                ? new[] { 2, -10, 2, 0 }
                : new[] { 4, 2, -2, 16 }));
            Assert.That(result.Values, Is.Not.SameAs(left.Values));
            Assert.That(result.Values, Is.Not.SameAs(right.Values));
            Assert.That(left.Values, Is.EqualTo(new[] { 3, -4, 0, 8 }));
            Assert.That(right.Values, Is.EqualTo(new[] { 1, 6, -2, 8 }));
        });
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void MixedBinaryOperators_PreserveOperandOrderAndSources(bool subtract, bool baseFirst)
    {
        var geometry = new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2));
        var raster = new SpatialIntRaster(geometry, new[] { 3, -4, 0, 8 });
        var baseRaster = new SpatialRaster<int>(geometry, new[] { 1, 6, -2, 8 });

        SpatialIntRaster result = subtract
            ? (baseFirst ? baseRaster - raster : raster - baseRaster)
            : (baseFirst ? baseRaster + raster : raster + baseRaster);

        int[] expected = subtract
            ? (baseFirst ? new[] { -2, 10, -2, 0 } : new[] { 2, -10, 2, 0 })
            : new[] { 4, 2, -2, 16 };

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Values, Is.EqualTo(expected));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(result.Values, Is.Not.SameAs(baseRaster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4, 0, 8 }));
            Assert.That(baseRaster.Values, Is.EqualTo(new[] { 1, 6, -2, 8 }));
        });
    }

    [TestCase(false, false, 7, 0)]
    [TestCase(false, true, 7, 0)]
    [TestCase(true, false, -1, -8)]
    [TestCase(true, true, 1, 8)]
    public void ScalarOperators_PreserveOperandOrderAndSource(
        bool subtract, bool scalarFirst, int first, int second)
    {
        var raster = new SpatialIntRaster(new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 1)), new[] { 3, -4 });

        SpatialIntRaster result = subtract
            ? (scalarFirst ? 4 - raster : raster - 4)
            : (scalarFirst ? 4 + raster : raster + 4);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(raster.Geometry));
            Assert.That(result.Values, Is.EqualTo(new[] { first, second }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4 }));
        });
    }

    [Test]
    public void UnaryMinus_CreatesIndependentNegatedRaster()
    {
        var raster = new SpatialIntRaster(new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 1)), new[] { 3, -4 });

        SpatialIntRaster result = -raster;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(raster.Geometry));
            Assert.That(result.Values, Is.EqualTo(new[] { -3, 4 }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4 }));
        });
    }

    [TestCase(1, 4)]
    [TestCase(4, 1)]
    [TestCase(1, 1)]
    public void BinaryOperators_WhenResolutionsDiffer_Throw(int width, int height)
    {
        var left = new SpatialIntRaster(new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2)), new int[4]);
        var right = new SpatialIntRaster(new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(width, height)), new int[width * height]);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (SpatialRaster<int>)left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + (SpatialRaster<int>)right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (SpatialRaster<int>)left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - (SpatialRaster<int>)right)!.ParamName, Is.EqualTo("right"));
        });
    }

    [Test]
    public void Operators_WhenRasterIsNull_Throw()
    {
        var raster = new SpatialIntRaster(new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(1, 1)), new int[1]);
        SpatialIntRaster missing = null!;
        var baseRaster = new SpatialRaster<int>(raster.Geometry, new int[1]);
        SpatialRaster<int> missingBase = null!;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster + missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster - missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + 4)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = 4 + missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - 4)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = 4 - missing)!.ParamName, Is.EqualTo("raster"));
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
