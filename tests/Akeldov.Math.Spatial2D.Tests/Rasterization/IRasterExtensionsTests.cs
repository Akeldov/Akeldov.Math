using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class IRasterExtensionsTests
{
    [Test]
    public void MapValues_ReturnsRasterWithMappedValuesInRowMajorOrder()
    {
        IRaster<int> raster = new TestRaster<int>(2, 2, new[] { 10, 20, 30, 40 });

        Raster<string> result = raster.MapValues(value => value.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
            Assert.That(result.Values, Is.EqualTo(new[] { "10", "20", "30", "40" }));
        });
    }

    [Test]
    public void MapValues_WithSpatialRaster_PreservesGeometry()
    {
        var geometry = new RasterGeometry(
            new PointXY(10f, 20f),
            new VectorXY(30f, 40f),
            new VectorXYInt(2, 1));
        var raster = new SpatialRaster<int>(geometry, new[] { 10, 20 });

        SpatialRaster<byte> result = raster.MapValues(value => (byte)(value / 10));

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Values, Is.EqualTo(new byte[] { 1, 2 }));
        });
    }

    [Test]
    public void MapValues_WhenArgumentsAreInvalid_Throws()
    {
        IRaster<int> raster = new TestRaster<int>(2, 1, new[] { 10, 20 });
        IRaster<int> nullRaster = null!;
        ISpatialRaster<int> nullSpatialRaster = null!;
        var geometry = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(2f, 1f),
            new VectorXYInt(2, 1));
        var spatialRaster = new SpatialRaster<int>(geometry, new[] { 10, 20 });
        var mismatchedGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(3f, 1f),
            new VectorXYInt(3, 1));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullRaster.MapValues(value => value))!.ParamName,
                Is.EqualTo("raster"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => raster.MapValues((Func<int, int>)null!))!.ParamName,
                Is.EqualTo("selector"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullSpatialRaster.MapValues(value => value))!.ParamName,
                Is.EqualTo("raster"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => spatialRaster.MapValues((Func<int, int>)null!))!.ParamName,
                Is.EqualTo("selector"));
        });
    }

    private sealed class TestRaster<TValue> : IRaster<TValue>
    {
        private readonly TValue[] _values;

        public TestRaster(int width, int height, TValue[] values)
        {
            Resolution = new VectorXYInt(width, height);
            _values = values;
        }

        public VectorXYInt Resolution { get; }

        public TValue this[int x, int y] => _values[y * Resolution.X + x];

        public TValue this[VectorXYInt index] => this[index.X, index.Y];

        public TValue this[int index] => _values[_values.Length - index - 1];
    }
}
