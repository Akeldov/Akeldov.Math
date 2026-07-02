using Akeldov.Math.Hexes.Rasterization;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class HexFieldTopologyRasterizationTests
{
    [Test]
    public void Rasterize_UsesPixelsPerApothemForRasterResolution()
    {
        var topology = new IndexSeptupletMap(1, 1, Layout.OddR);
        var rasterizer = new HexFieldTopologyRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            _ => new RGBA16BitColor(1, 2, 3, 4));

        RasterGrid grid = rasterizer.CreateGrid(topology, 3f);
        Raster<RGBA16BitColor> raster = rasterizer.Rasterize(topology, grid);

        Assert.That(raster.Width, Is.EqualTo(6));
        Assert.That(raster.Height, Is.EqualTo(7));
    }

    [Test]
    public void Rasterize_MapsHexIndexToColor()
    {
        var topology = new IndexSeptupletMap(2, 1, Layout.OddR);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);
        var mappedIndices = new List<VectorXYInt>();
        var rasterizer = new HexFieldTopologyRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            index =>
            {
                mappedIndices.Add(index);
                return index.X == 0 ? red : blue;
            });
        RasterGrid grid = rasterizer.CreateGrid(topology, 4f);

        Raster<RGBA16BitColor> raster = rasterizer.Rasterize(topology, grid);

        Assert.That(mappedIndices, Is.EqualTo(new[]
        {
            new VectorXYInt(0, 0),
            new VectorXYInt(1, 0)
        }));
        Assert.That(raster.Values.Count(x => x == red), Is.GreaterThan(0));
        Assert.That(raster.Values.Count(x => x == blue), Is.GreaterThan(0));
    }

    [Test]
    public void Rasterize_UsesProvidedGrid()
    {
        var topology = new IndexSeptupletMap(1, 1, Layout.OddR);
        var grid = new RasterGrid(new PointXY(-2f, -2f), new VectorXY(4f, 4f), new VectorXYInt(4, 4));
        var rasterizer = new HexFieldTopologyRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            _ => new RGBA16BitColor(1, 2, 3, 4));

        Raster<RGBA16BitColor> raster = rasterizer.Rasterize(topology, grid);

        Assert.That(raster.Grid, Is.EqualTo(grid));
        Assert.That(raster.Width, Is.EqualTo(4));
        Assert.That(raster.Height, Is.EqualTo(4));
        Assert.That(raster.Values.Count(x => x != default), Is.GreaterThan(0));
    }

    [Test]
    public void Constructor_WhenApothemIsInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexFieldTopologyRGBA16BitRasterizer(new VectorXY(0f, 0f), 0f, _ => default));
    }

    [Test]
    public void Constructor_WhenOriginIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexFieldTopologyRGBA16BitRasterizer(new VectorXY(float.PositiveInfinity, 0f), 1f, _ => default));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [Test]
    public void Constructor_WhenColorMapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new HexFieldTopologyRGBA16BitRasterizer(new VectorXY(0f, 0f), 1f, null!));
    }

    [Test]
    public void CreateGrid_WhenPixelsPerApothemIsInvalid_Throws()
    {
        var topology = new IndexSeptupletMap(1, 1, Layout.OddR);
        var rasterizer = new HexFieldTopologyRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            _ => default);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rasterizer.CreateGrid(topology, 0f));
    }
}
