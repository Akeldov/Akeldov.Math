using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class IHexMapRasterizationTests
{
    [Test]
    public void Rasterize_MapsToRGBA16BitColorRaster()
    {
        IHexMap<int> map = CreateMap(new[] { 10, 20 });
        var rasterGrid = new SpatialRasterGrid(
            new PointXY(10f, 20f),
            new VectorXY(30f, 40f),
            new VectorXYInt(2, 1));
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        SpatialRaster<RGBA16BitColor> raster = map.Rasterize(
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
        IHexMap<int> map = CreateMap(new[] { 10, 20 });
        var rasterGrid = new SpatialRasterGrid(
            new PointXY(0f, 0f),
            new VectorXY(2f, 1f),
            new VectorXYInt(2, 1));

        SpatialRaster<byte> raster = map.Rasterize(
            rasterGrid,
            value => (byte)(value / 10));

        Assert.That(raster.Values, Is.EqualTo(new byte[] { 1, 2 }));
    }

    [Test]
    public void Rasterize_WhenArgumentsAreInvalid_Throws()
    {
        IHexMap<int> map = CreateMap(new[] { 10, 20 });
        IHexMap<int> nullMap = null!;
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
                Assert.Throws<ArgumentNullException>(() => nullMap.Rasterize(rasterGrid, _ => 0))!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.Rasterize<int, int>(rasterGrid, null!))!.ParamName,
                Is.EqualTo("colorSelector"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => map.Rasterize(mismatchedRasterGrid, _ => 0))!.ParamName,
                Is.EqualTo("rasterGrid"));
        });
    }

    private static HexMap<int> CreateMap(int[] values)
    {
        var map = new HexMap<int>(new IndexSeptupletMap(2, 1, Layout.OddR));

        for (int i = 0; i < values.Length; i++)
            map[i] = values[i];

        return map;
    }
}
