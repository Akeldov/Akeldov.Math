using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Grids;

public class IGridRasterizationTests
{
    [Test]
    public void Rasterize_MapsToRGBA16BitColorRaster()
    {
        IGrid<int> grid = new TestGrid<int>(2, 1, new[] { 10, 20 });
        var rasterGrid = new SpatialRasterGrid(
            new PointXY(10f, 20f),
            new VectorXY(30f, 40f),
            new VectorXYInt(2, 1));
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        SpatialRaster<RGBA16BitColor> raster = grid.Rasterize(
            rasterGrid,
            value => value == 10 ? red : blue);

        Assert.Multiple(() =>
        {
            Assert.That(raster.Grid, Is.EqualTo(rasterGrid));
            Assert.That(raster.Values, Is.EqualTo(new[] { red, blue }));
        });
    }

    [Test]
    public void Rasterize_MapsToArbitraryRasterValueType()
    {
        IGrid<int> grid = new TestGrid<int>(2, 1, new[] { 10, 20 });
        var rasterGrid = new SpatialRasterGrid(
            new PointXY(0f, 0f),
            new VectorXY(2f, 1f),
            new VectorXYInt(2, 1));

        SpatialRaster<byte> raster = grid.Rasterize(
            rasterGrid,
            value => (byte)(value / 10));

        Assert.That(raster.Values, Is.EqualTo(new byte[] { 1, 2 }));
    }

    [Test]
    public void Rasterize_WhenArgumentsAreInvalid_Throws()
    {
        IGrid<int> grid = new TestGrid<int>(2, 1, new[] { 10, 20 });
        IGrid<int> nullGrid = null!;
        var rasterGrid = new SpatialRasterGrid(
            new PointXY(0f, 0f),
            new VectorXY(2f, 1f),
            new VectorXYInt(2, 1));
        var mismatchedRasterGrid = new SpatialRasterGrid(
            new PointXY(0f, 0f),
            new VectorXY(3f, 1f),
            new VectorXYInt(3, 1));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullGrid.Rasterize(rasterGrid, _ => 0))!.ParamName,
                Is.EqualTo("grid"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => grid.Rasterize<int, int>(rasterGrid, null!))!.ParamName,
                Is.EqualTo("colorSelector"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => grid.Rasterize(mismatchedRasterGrid, _ => 0))!.ParamName,
                Is.EqualTo("rasterGrid"));
        });
    }

    private sealed class TestGrid<TValue> : IGrid<TValue>
    {
        private readonly TValue[] _values;

        public TestGrid(int width, int height, TValue[] values)
        {
            Width = width;
            Height = height;
            _values = values;
        }

        public int Width { get; }

        public int Height { get; }

        public TValue this[int x, int y] => _values[y * Width + x];

        public TValue this[VectorXYInt index] => _values[index.Y * Width + index.X];

        public TValue this[int index] => _values[index];
    }
}
