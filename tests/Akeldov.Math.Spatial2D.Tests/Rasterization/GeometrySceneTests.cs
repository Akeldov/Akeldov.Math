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
            .AddPointDistanceBasedLayer(segment, RGBA16BitColors.Red, fillDistance: 0.25f);

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
    public void ParameterizedProjection_MapsProjectionWithDelegate()
    {
        var segment = new ParameterizedSegment(
            new PointXY(0.5f, 0.5f),
            new PointXY(2.5f, 0.5f));
        var scene = new GeometryScene<int>((background, foreground) => foreground)
            .AddParameterizedProjectionBasedLayer(
                segment,
                projection => 100 + (int)MathF.Round(projection.CurveCoordinate * 10f));

        var raster = scene.Rasterize(CreateGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(100));
        Assert.That(raster[1, 0], Is.EqualTo(110));
        Assert.That(raster[2, 0], Is.EqualTo(120));
    }

    [Test]
    public void ParameterizedProjection_WithSourceList_UsesNearestProjection()
    {
        var lowerSegment = new ParameterizedSegment(
            new PointXY(0.5f, 0.5f),
            new PointXY(1.5f, 0.5f));
        var upperSegment = new ParameterizedSegment(
            new PointXY(0.5f, 2.5f),
            new PointXY(1.5f, 2.5f));
        var scene = new GeometryScene<int>((background, foreground) => foreground)
            .AddParameterizedProjectionBasedLayer(
                new[] { lowerSegment, upperSegment },
                projection => projection.ProjectedPoint.Y < 1f ? 1 : 2);

        var raster = scene.Rasterize(new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(1f, 3f),
            resolution: new VectorXYInt(1, 3)));

        Assert.That(raster[0, 0], Is.EqualTo(1));
        Assert.That(raster[0, 2], Is.EqualTo(2));
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

    [Test]
    public void AddLayer_WhenBlendIsNull_Throws()
    {
        var scene = new GeometryScene<int>((background, foreground) => foreground);

        Assert.Throws<ArgumentNullException>(() => scene.AddLayer(new ConstantIntLayer(1), null!));
    }

    [Test]
    public void Rasterize_WithLayerBlendOverride_CompositesInInsertionOrder()
    {
        var scene = new GeometryScene<int>(
                backgroundColor: 1,
                defaultLayerBlend: (background, foreground) => background * 10 + foreground)
            .AddLayer(new ConstantIntLayer(2))
            .AddLayer(new ConstantIntLayer(3), (background, foreground) => background + foreground)
            .AddLayer(new ConstantIntLayer(4));

        var raster = scene.Rasterize(CreateGrid(width: 1, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(154));
    }

    [Test]
    public void AddPointDistanceBasedLayer_WithEdgeFalloff_StartsFalloffOutsideFillDistance()
    {
        var point = new PointXY(0f, 0f);
        RGBA16BitColor color = RGBA16BitColors.Red;
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddPointDistanceBasedLayer(point, color, fillDistance: 0.5f, edgeFalloff: 1f);

        var raster = scene.Rasterize(CreateCenteredGrid(width: 3, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(color));
        Assert.That(raster[1, 0], Is.EqualTo(color.ScaleAlpha(0.5f)));
        Assert.That(raster[2, 0], Is.EqualTo(default(RGBA16BitColor)));
    }

    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    [TestCase(-0.1f)]
    public void AddPointDistanceBasedLayer_WhenFillDistanceIsInvalid_Throws(float fillDistance)
    {
        var source = new PointXY(0f, 0f);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.AddPointDistanceBasedLayer(source, RGBA16BitColors.Red, fillDistance));
        Assert.Throws<ArgumentOutOfRangeException>(() => scene.AddPointDistanceBasedLayer(source, RGBA16BitColors.Red, fillDistance, edgeFalloff: 1f));
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    [TestCase(-0.1f)]
    public void AddPointDistanceBasedLayer_WhenEdgeFalloffIsInvalid_Throws(float edgeFalloff)
    {
        var source = new PointXY(0f, 0f);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.AddPointDistanceBasedLayer(
            source,
            RGBA16BitColors.Red,
            fillDistance: 0.5f,
            edgeFalloff: edgeFalloff));
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    [TestCase(-0.1f)]
    public void AddSignedPointDistanceBasedLayer_WhenEdgeFalloffIsInvalid_Throws(float edgeFalloff)
    {
        var source = new Disk(new PointXY(0f, 0f), radius: 1f);
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentOutOfRangeException>(() => scene.AddSignedPointDistanceBasedLayer(
            source,
            RGBA16BitColors.Red,
            edgeFalloff: edgeFalloff));
    }

    [Test]
    public void AddPointDistanceBasedLayer_WhenSourceListIsEmpty_Throws()
    {
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentException>(() => scene.AddPointDistanceBasedLayer(
            Array.Empty<IPointDistanceProvider>(),
            RGBA16BitColors.Red,
            fillDistance: 0.5f,
            edgeFalloff: 1f));
    }

    [Test]
    public void AddPointDistanceBasedLayer_WhenSourceListContainsNull_Throws()
    {
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

        Assert.Throws<ArgumentException>(() => scene.AddPointDistanceBasedLayer(
            new IPointDistanceProvider[] { null! },
            RGBA16BitColors.Red,
            fillDistance: 0.5f,
            edgeFalloff: 1f));
    }

    [Test]
    public void AddParameterizedProjectionBasedLayer_WhenSourceListIsEmpty_Throws()
    {
        var scene = new GeometryScene<int>((background, foreground) => foreground);

        Assert.Throws<ArgumentException>(() => scene.AddParameterizedProjectionBasedLayer(
            Array.Empty<IParameterizedCurve>(),
            projection => 1));
    }

    [Test]
    public void AddParameterizedProjectionBasedLayer_WhenSourceListContainsNull_Throws()
    {
        var scene = new GeometryScene<int>((background, foreground) => foreground);

        Assert.Throws<ArgumentException>(() => scene.AddParameterizedProjectionBasedLayer(
            new IParameterizedCurve[] { null! },
            projection => 1));
    }

    [Test]
    public void AddPointDistanceBasedLayer_WithSourceList_CopiesSources()
    {
        var sources = new List<IPointDistanceProvider> { new PointXY(0f, 0f) };
        var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver)
            .AddPointDistanceBasedLayer(sources, RGBA16BitColors.Red, fillDistance: 0f, edgeFalloff: 1f);

        sources[0] = new PointXY(100f, 100f);

        var raster = scene.Rasterize(CreateCenteredGrid(width: 1, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(RGBA16BitColors.Red));
    }

    [Test]
    public void AddParameterizedProjectionBasedLayer_WithSourceList_CopiesSources()
    {
        var sources = new List<IParameterizedCurve>
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 0f))
        };
        var scene = new GeometryScene<int>((background, foreground) => foreground)
            .AddParameterizedProjectionBasedLayer(
                sources,
                projection => projection.Distance < 0.01f ? 7 : 9);
        sources[0] = new ParameterizedSegment(new PointXY(100f, 100f), new PointXY(101f, 100f));

        var raster = scene.Rasterize(CreateCenteredGrid(width: 1, height: 1));

        Assert.That(raster[0, 0], Is.EqualTo(7));
    }

    private static RasterGrid CreateGrid(int width, int height)
    {
        return new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(width, height),
            resolution: new VectorXYInt(width, height));
    }

    private static RasterGrid CreateCenteredGrid(int width, int height)
    {
        return new RasterGrid(
            origin: new PointXY(-0.5f, -0.5f),
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
