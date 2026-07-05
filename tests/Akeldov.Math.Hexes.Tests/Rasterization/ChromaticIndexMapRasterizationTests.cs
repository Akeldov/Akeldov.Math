using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Rasterization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class ChromaticIndexMapRasterizationTests
{
    [Test]
    public void Rasterize_UsesPixelsPerApothemForRasterResolution()
    {
        var chromatization = new ChromaticIndexMap(1, 1, Layout.OddR);
        var rasterizer = new HexFieldChromatizationRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            _ => new RGBA16BitColor(1, 2, 3, 4));

        SpatialRasterGrid grid = rasterizer.CreateGrid(chromatization, 3f);
        SpatialRaster<RGBA16BitColor> raster = rasterizer.Rasterize(chromatization, grid);

        Assert.That(raster.Width, Is.EqualTo(6));
        Assert.That(raster.Height, Is.EqualTo(7));
    }

    [Test]
    public void Rasterize_MapsChromaticIndexToColor()
    {
        var chromatization = new ChromaticIndexMap(2, 1, Layout.OddR);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);
        var mappedIndices = new List<byte>();
        var rasterizer = new HexFieldChromatizationRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            chromaticIndex =>
            {
                mappedIndices.Add(chromaticIndex);
                return chromaticIndex == 0 ? red : blue;
            });
        SpatialRasterGrid grid = rasterizer.CreateGrid(chromatization, 4f);

        SpatialRaster<RGBA16BitColor> raster = rasterizer.Rasterize(chromatization, grid);

        Assert.That(mappedIndices, Is.EqualTo(new byte[] { 0, 1 }));
        Assert.That(raster.Values.Count(x => x == red), Is.GreaterThan(0));
        Assert.That(raster.Values.Count(x => x == blue), Is.GreaterThan(0));
    }

    [Test]
    public void Rasterize_UsesProvidedGrid()
    {
        var chromatization = new ChromaticIndexMap(1, 1, Layout.OddR);
        var grid = new SpatialRasterGrid(new PointXY(-2f, -2f), new VectorXY(4f, 4f), new VectorXYInt(4, 4));
        var rasterizer = new HexFieldChromatizationRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            _ => new RGBA16BitColor(1, 2, 3, 4));

        SpatialRaster<RGBA16BitColor> raster = rasterizer.Rasterize(chromatization, grid);

        Assert.That(raster.Grid, Is.EqualTo(grid));
        Assert.That(raster.Width, Is.EqualTo(4));
        Assert.That(raster.Height, Is.EqualTo(4));
        Assert.That(raster.Values.Count(x => x != default), Is.GreaterThan(0));
    }

    [Test]
    public void Constructor_WhenApothemIsInvalid_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexFieldChromatizationRGBA16BitRasterizer(new VectorXY(0f, 0f), 0f, _ => default));
    }

    [Test]
    public void Constructor_WhenOriginIsNotFinite_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new HexFieldChromatizationRGBA16BitRasterizer(new VectorXY(float.PositiveInfinity, 0f), 1f, _ => default));

        Assert.That(exception!.ParamName, Is.EqualTo("origin"));
    }

    [Test]
    public void Constructor_WhenColorMapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new HexFieldChromatizationRGBA16BitRasterizer(new VectorXY(0f, 0f), 1f, null!));
    }

    [Test]
    public void CreateGrid_WhenPixelsPerApothemIsInvalid_Throws()
    {
        var chromatization = new ChromaticIndexMap(1, 1, Layout.OddR);
        var rasterizer = new HexFieldChromatizationRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            2f,
            _ => default);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rasterizer.CreateGrid(chromatization, 0f));
    }

    [Test]
    public void CreateGrid_WhenRasterResolutionDoesNotFitInt32_ThrowsOverflowException()
    {
        var chromatization = new ChromaticIndexMap(1, 1, Layout.OddR);
        var rasterizer = new HexFieldChromatizationRGBA16BitRasterizer(
            new VectorXY(0f, 0f),
            1f,
            _ => default);

        Assert.Throws<OverflowException>(() =>
            rasterizer.CreateGrid(chromatization, float.MaxValue));
    }
}
