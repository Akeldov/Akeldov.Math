using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class GeometrySceneTests
{
    [Test]
    public void Rasterize_WhenSceneHasNoLayers_ReturnsBackgroundColor()
    {
        var background = new RGBA16BitColor(100, 200, 300, 400);
        GeometryScene<RGBA16BitColor> scene = GeometryScenes.CreateRGBA16Bit(background);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(background));
        Assert.That(raster[1, 0], Is.EqualTo(background));
    }

    [Test]
    public void Rasterize_CompositesLayersInInsertionOrder()
    {
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .AddLayer(new ConstantLayer(red))
            .AddLayer(new ConstantLayer(blue));

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 1, height: 1));

        Assert.That(scene.Layers, Has.Count.EqualTo(2));
        Assert.That(raster[0, 0], Is.EqualTo(blue));
    }

    [Test]
    public void Stroke_WithSegment_UsesUnsignedDistance()
    {
        var color = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var segment = new Segment(
            new PointXY(0.5f, 0.5f),
            new PointXY(1.5f, 0.5f));
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Stroke(segment, color, width: 0.25f);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(color));
        Assert.That(raster[1, 0], Is.EqualTo(color));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Fill_WithDisk_UsesSignedDistance()
    {
        var color = new RGBA16BitColor(0, ushort.MaxValue, 0, ushort.MaxValue);
        var disk = new Disk(new PointXY(1.5f, 0.5f), radius: 0.6f);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Fill(disk, color);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(default(RGBA16BitColor)));
        Assert.That(raster[1, 0], Is.EqualTo(color));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void DrawPoint_UsesPointDistance()
    {
        var color = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .DrawPoint(new PointXY(1.5f, 0.5f), color, radius: 0.25f);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(default(RGBA16BitColor)));
        Assert.That(raster[1, 0], Is.EqualTo(color));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void DrawDistance_MapsUnsignedDistanceWithDelegate()
    {
        var point = new PointXY(0.5f, 0.5f);
        var near = new RGBA16BitColor(10, 20, 30, ushort.MaxValue);
        var far = new RGBA16BitColor(40, 50, 60, ushort.MaxValue);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .DrawDistance(point, distance => distance < 1f ? near : far);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(near));
        Assert.That(raster[1, 0], Is.EqualTo(far));
    }

    [Test]
    public void DrawSignedDistance_MapsSignedDistanceWithDelegate()
    {
        var disk = new Disk(new PointXY(0.5f, 0.5f), radius: 0.6f);
        var inside = new RGBA16BitColor(100, 0, 0, ushort.MaxValue);
        var outside = new RGBA16BitColor(0, 100, 0, ushort.MaxValue);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .DrawSignedDistance(disk, signedDistance => signedDistance <= 0f ? inside : outside);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(inside));
        Assert.That(raster[1, 0], Is.EqualTo(outside));
    }

    [Test]
    public void AddLayer_WhenLayerIsNull_Throws()
    {
        var scene = GeometryScenes.CreateRGBA16Bit();

        Assert.Throws<ArgumentNullException>(() => scene.AddLayer(null!));
    }

    [Test]
    public void Stroke_WhenParametersAreInvalid_Throws()
    {
        var scene = GeometryScenes.CreateRGBA16Bit();
        var point = new PointXY(0f, 0f);
        var color = new RGBA16BitColor(1, 2, 3, 4);

        Assert.Throws<ArgumentNullException>(() => scene.Stroke(null!, color, width: 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Stroke(point, color, width: 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Stroke(point, color, width: 1f, edgeFalloff: -1f));
    }

    [Test]
    public void Fill_WhenParametersAreInvalid_Throws()
    {
        var scene = GeometryScenes.CreateRGBA16Bit();
        var color = new RGBA16BitColor(1, 2, 3, 4);
        var disk = new Disk(new PointXY(0f, 0f), radius: 1f);

        Assert.Throws<ArgumentNullException>(() => scene.Fill(null!, color));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Fill(disk, color, edgeFalloff: float.NaN));
    }

    [Test]
    public void DrawPoint_WhenParametersAreInvalid_Throws()
    {
        var scene = GeometryScenes.CreateRGBA16Bit();
        var color = new RGBA16BitColor(1, 2, 3, 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.DrawPoint(new PointXY(float.PositiveInfinity, 0f), color, radius: 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.DrawPoint(new PointXY(0f, 0f), color, radius: 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.DrawPoint(new PointXY(0f, 0f), color, radius: 1f, edgeFalloff: -1f));
    }

    [Test]
    public void Rasterize_WhenGridIsInvalid_Throws()
    {
        var scene = GeometryScenes.CreateRGBA16Bit();

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Rasterize(default));
    }

    [Test]
    public void GeometryScene_WhenUsingCustomColorType_RasterizesValues()
    {
        var point = new PointXY(0.5f, 0.5f);
        var scene = new GeometryScene<int>(
                backgroundColor: 10,
                blend: (background, foreground) => background + foreground,
                applyCoverage: (color, coverage) => (int)MathF.Round(color * coverage))
            .DrawDistance(point, distance => distance < 1f ? 7 : 3);

        int[] values = scene.RasterizeValues(CreateGrid(width: 2, height: 1));

        Assert.That(values, Is.EqualTo(new[] { 17, 13 }));
    }

    [Test]
    public void Rasterize_WithCustomRasterFactory_UsesNewGenericColorBuffer()
    {
        var scene = new GeometryScene<int>(
                backgroundColor: 1,
                blend: (background, foreground) => background + foreground,
                applyCoverage: (color, coverage) => color)
            .AddLayer(new ConstantIntLayer(2));

        string raster = scene.Rasterize(
            CreateGrid(width: 2, height: 1),
            (grid, values) => $"{grid.Resolution.X}x{grid.Resolution.Y}:{string.Join(",", values)}");

        Assert.That(raster, Is.EqualTo("2x1:3,3"));
    }

    [Test]
    public void GeometryScene_WhenColorDelegatesAreNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GeometryScene<int>(
            blend: null!,
            applyCoverage: (color, coverage) => color));

        Assert.Throws<ArgumentNullException>(() => new GeometryScene<int>(
            blend: (background, foreground) => foreground,
            applyCoverage: null!));
    }

    private static RasterGrid CreateGrid(int width, int height)
    {
        return new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(width, height),
            resolution: new VectorXYInt(width, height));
    }

    private sealed class ConstantLayer : IGeometrySceneLayer<RGBA16BitColor>
    {
        private readonly RGBA16BitColor _color;

        public ConstantLayer(RGBA16BitColor color)
        {
            _color = color;
        }

        public RGBA16BitColor Sample(PointXY point)
        {
            return _color;
        }
    }

    private sealed class ConstantIntLayer : IGeometrySceneLayer<int>
    {
        private readonly int _color;

        public ConstantIntLayer(int color)
        {
            _color = color;
        }

        public int Sample(PointXY point)
        {
            return _color;
        }
    }
}
