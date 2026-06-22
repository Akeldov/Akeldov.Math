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
        var scene = GeometryScenes.CreateRGBA16Bit()
            .AddLayer(new ConstantLayer(RGBA16BitColors.Red))
            .AddLayer(new ConstantLayer(RGBA16BitColors.Blue));

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 1, height: 1));

        Assert.That(scene.Layers, Has.Count.EqualTo(2));
        Assert.That(raster[0, 0], Is.EqualTo(RGBA16BitColors.Blue));
    }

    [Test]
    public void Stroke_WithSegment_UsesUnsignedDistance()
    {
        var segment = new Segment(
            new PointXY(0.5f, 0.5f),
            new PointXY(1.5f, 0.5f));
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Stroke(segment, RGBA16BitColors.Red, width: 0.25f);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(RGBA16BitColors.Red));
        Assert.That(raster[1, 0], Is.EqualTo(RGBA16BitColors.Red));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Fill_WithDisk_UsesSignedDistance()
    {
        var disk = new Disk(new PointXY(1.5f, 0.5f), radius: 0.6f);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Fill(disk, RGBA16BitColors.Green);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(default(RGBA16BitColor)));
        Assert.That(raster[1, 0], Is.EqualTo(RGBA16BitColors.Green));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Point_UsesPointDistance()
    {
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Point(new PointXY(1.5f, 0.5f), RGBA16BitColors.Blue, radius: 0.25f);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(default(RGBA16BitColor)));
        Assert.That(raster[1, 0], Is.EqualTo(RGBA16BitColors.Blue));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Point_WithPoints_UsesNearestPointDistance()
    {
        var points = new[]
        {
            new PointXY(0.5f, 0.5f),
            new PointXY(2.5f, 0.5f)
        };
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Point(points, RGBA16BitColors.Blue, radius: 0.25f);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 4, height: 1));

        Assert.That(scene.Layers, Has.Count.EqualTo(1));
        Assert.That(raster[0, 0], Is.EqualTo(RGBA16BitColors.Blue));
        Assert.That(raster[1, 0], Is.EqualTo(default(RGBA16BitColor)));
        Assert.That(raster[2, 0], Is.EqualTo(RGBA16BitColors.Blue));
        Assert.That(raster[3, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Point_WithPoints_CopiesPointCollection()
    {
        var points = new[] { new PointXY(0.5f, 0.5f) };
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Point(points, RGBA16BitColors.Blue, radius: 0.25f);

        points[0] = new PointXY(1.5f, 0.5f);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(RGBA16BitColors.Blue));
        Assert.That(raster[1, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Distance_MapsUnsignedDistanceWithDelegate()
    {
        var point = new PointXY(0.5f, 0.5f);
        var near = new RGBA16BitColor(10, 20, 30, ushort.MaxValue);
        var far = new RGBA16BitColor(40, 50, 60, ushort.MaxValue);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .Distance(point, distance => distance < 1f ? near : far);

        RGBA16BitRaster raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(near));
        Assert.That(raster[1, 0], Is.EqualTo(far));
    }

    [Test]
    public void SignedDistance_MapsSignedDistanceWithDelegate()
    {
        var disk = new Disk(new PointXY(0.5f, 0.5f), radius: 0.6f);
        var inside = new RGBA16BitColor(100, 0, 0, ushort.MaxValue);
        var outside = new RGBA16BitColor(0, 100, 0, ushort.MaxValue);
        var scene = GeometryScenes.CreateRGBA16Bit()
            .SignedDistance(disk, signedDistance => signedDistance <= 0f ? inside : outside);

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
    public void Point_WhenParametersAreInvalid_Throws()
    {
        var scene = GeometryScenes.CreateRGBA16Bit();
        var color = new RGBA16BitColor(1, 2, 3, 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Point(new PointXY(float.PositiveInfinity, 0f), color, radius: 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Point(new PointXY(0f, 0f), color, radius: 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Point(new PointXY(0f, 0f), color, radius: 1f, edgeFalloff: -1f));
        Assert.Throws<ArgumentNullException>(() => scene.Point((IReadOnlyList<PointXY>)null!, color, radius: 1f));
        Assert.Throws<ArgumentException>(() => scene.Point(Array.Empty<PointXY>(), color, radius: 1f));
        Assert.Throws<ArgumentException>(() => scene.Point(new[] { new PointXY(float.PositiveInfinity, 0f) }, color, radius: 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Point(new[] { new PointXY(0f, 0f) }, color, radius: 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.Point(new[] { new PointXY(0f, 0f) }, color, radius: 1f, edgeFalloff: -1f));
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
            .Distance(point, distance => distance < 1f ? 7 : 3);

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
