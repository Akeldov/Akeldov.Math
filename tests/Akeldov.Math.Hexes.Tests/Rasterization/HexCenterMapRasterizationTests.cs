using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Rasterization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class HexCenterMapRasterizationTests
{
    [Test]
    public void Rasterize_UsesPixelsPerApothemForRasterResolution()
    {
        var geometry = new HexCenterMap(1, 1, new VectorXY(0f, 0f), 2f, Layout.OddR);

        SpatialRasterGrid grid = HexFieldGeometryRGBA16BitRasterizer.CreateGrid(geometry, 3f);
        SpatialRaster<RGBA16BitColor> raster = new HexFieldGeometryRGBA16BitRasterizer(
                _ => new RGBA16BitColor(1, 2, 3, 4))
            .Rasterize(geometry, grid);

        Assert.That(raster.Width, Is.EqualTo(6));
        Assert.That(raster.Height, Is.EqualTo(7));
    }

    [Test]
    public void Rasterize_MapsHexCenterToColor()
    {
        var geometry = new HexCenterMap(2, 1, new VectorXY(0f, 0f), 2f, Layout.OddR);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);
        var mappedCenters = new List<PointXY>();

        SpatialRasterGrid grid = HexFieldGeometryRGBA16BitRasterizer.CreateGrid(geometry, 4f);
        SpatialRaster<RGBA16BitColor> raster = new HexFieldGeometryRGBA16BitRasterizer(
                center =>
            {
                mappedCenters.Add(center);
                return center.X < 1f ? red : blue;
            })
            .Rasterize(geometry, grid);

        Assert.That(mappedCenters, Is.EqualTo(new[]
        {
            new PointXY(0f, 0f),
            new PointXY(4f, 0f)
        }));
        Assert.That(raster.Values.Count(x => x == red), Is.GreaterThan(0));
        Assert.That(raster.Values.Count(x => x == blue), Is.GreaterThan(0));
    }

    [Test]
    public void Constructor_WhenColorMapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new HexFieldGeometryRGBA16BitRasterizer(null!));
    }

    [Test]
    public void CreateGrid_WhenPixelsPerApothemIsInvalid_Throws()
    {
        var geometry = new HexCenterMap(1, 1, new VectorXY(0f, 0f), 2f, Layout.OddR);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            HexFieldGeometryRGBA16BitRasterizer.CreateGrid(geometry, 0f));
    }

    [Test]
    public void CreateGrid_WhenRasterResolutionDoesNotFitInt32_ThrowsOverflowException()
    {
        var geometry = new HexCenterMap(1, 1, new VectorXY(0f, 0f), 1f, Layout.OddR);

        Assert.Throws<OverflowException>(() =>
            HexFieldGeometryRGBA16BitRasterizer.CreateGrid(geometry, float.MaxValue));
    }

    [Test]
    public void Rasterize_UsesProvidedGrid()
    {
        var geometry = new HexCenterMap(1, 1, new VectorXY(0f, 0f), 2f, Layout.OddR);
        var grid = new SpatialRasterGrid(new PointXY(-2f, -2f), new VectorXY(4f, 4f), new VectorXYInt(4, 4));

        SpatialRaster<RGBA16BitColor> raster = new HexFieldGeometryRGBA16BitRasterizer(
                _ => new RGBA16BitColor(1, 2, 3, 4))
            .Rasterize(geometry, grid);

        Assert.That(raster.Grid, Is.EqualTo(grid));
        Assert.That(raster.Width, Is.EqualTo(4));
        Assert.That(raster.Height, Is.EqualTo(4));
        Assert.That(raster.Values.Count(x => x != default), Is.GreaterThan(0));
    }
}
