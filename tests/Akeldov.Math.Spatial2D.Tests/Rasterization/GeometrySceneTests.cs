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
        GeometryScene<RGBA16BitColor> scene = new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver);

        var raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(background));
        Assert.That(raster[1, 0], Is.EqualTo(background));
    }

    [Test]
    public void Stroke_WithSegment_UsesUnsignedDistance()
    {
        var segment = new Segment(
            new PointXY(0.5f, 0.5f),
            new PointXY(1.5f, 0.5f));
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddPointDistanceBasedLayer(segment, RGBA16BitColors.Red, width: 0.25f);

        var raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(RGBA16BitColors.Red));
        Assert.That(raster[1, 0], Is.EqualTo(RGBA16BitColors.Red));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [Test]
    public void Distance_MapsUnsignedDistanceWithDelegate()
    {
        var point = new PointXY(0.5f, 0.5f);
        var near = new RGBA16BitColor(10, 20, 30, ushort.MaxValue);
        var far = new RGBA16BitColor(40, 50, 60, ushort.MaxValue);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddPointDistanceBasedLayer(point, distance => distance < 1f ? near : far);

        var raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(near));
        Assert.That(raster[1, 0], Is.EqualTo(far));
    }

    [Test]
    public void SignedDistance_MapsSignedDistanceWithDelegate()
    {
        var disk = new Disk(new PointXY(0.5f, 0.5f), radius: 0.6f);
        var inside = new RGBA16BitColor(100, 0, 0, ushort.MaxValue);
        var outside = new RGBA16BitColor(0, 100, 0, ushort.MaxValue);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddSignedPointDistanceBasedLayer(disk, signedDistance => signedDistance <= 0f ? inside : outside);

        var raster = scene.Rasterize(CreateGrid(width: 2, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(inside));
        Assert.That(raster[1, 0], Is.EqualTo(outside));
    }

    [Test]
    public void AddLayer_WhenLayerIsNull_Throws()
    {
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentNullException>(() => scene.AddLayer(null!));
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

        public RGBA16BitColor Blend(RGBA16BitColor background, RGBA16BitColor foreground)
        {
            return foreground;
        }

        public RGBA16BitColor Sample(PointXY point)
        {
            return _color;
        }
    }

    private sealed class ConstantIntLayer : IGeometrySceneLayer<int>
    {
        private readonly int _color;
        private readonly Func<int, int, int> _blend;

        public ConstantIntLayer(int color, Func<int, int, int>? blend = null)
        {
            _color = color;
            _blend = blend ?? ((background, foreground) => foreground);
        }

        public int Blend(int background, int foreground)
        {
            return _blend(background, foreground);
        }

        public int Sample(PointXY point)
        {
            return _color;
        }
    }
}
