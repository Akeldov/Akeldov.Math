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
        var resolution = new VectorXYInt(2, 1);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        Raster<RGBA16BitColor> raster = map.Rasterize(
            resolution,
            value => value == 10 ? red : blue);

        Assert.Multiple(() =>
        {
            Assert.That(raster.Resolution, Is.EqualTo(resolution));
            Assert.That(raster.Values, Is.EqualTo(new[] { red, blue }));
        });
    }

    [Test]
    public void Rasterize_MapsToArbitraryRasterValueType()
    {
        IHexMap<int> map = CreateMap(new[] { 10, 20 });
        var resolution = new VectorXYInt(2, 1);

        Raster<byte> raster = map.Rasterize(
            resolution,
            value => (byte)(value / 10));

        Assert.That(raster.Values, Is.EqualTo(new byte[] { 1, 2 }));
    }

    [Test]
    public void Rasterize_WhenArgumentsAreInvalid_Throws()
    {
        IHexMap<int> map = CreateMap(new[] { 10, 20 });
        IHexMap<int> nullMap = null!;
        var resolution = new VectorXYInt(2, 1);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.Rasterize(resolution, _ => 0))!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.Rasterize<int, int>(resolution, null!))!.ParamName,
                Is.EqualTo("colorSelector"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => map.Rasterize(new VectorXYInt(0, 1), _ => 0))!.ParamName,
                Is.EqualTo("resolution"));
        });
    }

    private static HexMap<int> CreateMap(int[] values)
    {
        var map = new HexMap<int>(2, 1, Layout.OddR);

        for (int i = 0; i < values.Length; i++)
            map[i] = values[i];

        return map;
    }
}
