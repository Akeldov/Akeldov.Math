using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class CullingMapRasterizationTests
{
    [Test]
    public void RasterizeCullingMap_WhenCullerSelectsOneTwoAndThreeSources_BlendsSourceColorsInLinearRgb()
    {
        TestPointSource[] sources = CreateSources();
        var grid = new SpatialRasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        Raster<RGBA16BitColor> raster = sources.RasterizeCullingMap(
            grid,
            new XBandCuller(sources),
            SourceColor);

        Assert.That(raster[0, 0], Is.EqualTo(SourceColor(sources[0].Position)));
        Assert.That(raster[1, 0], Is.EqualTo(new RGBA16BitColor(44045, 44045, 0, ushort.MaxValue)));
        Assert.That(raster[2, 0], Is.EqualTo(new RGBA16BitColor(36638, 36638, 36638, ushort.MaxValue)));
    }

    [Test]
    public void RasterizeCullingMap_WhenColorSelectorUsesPosition_ColorsSelectedSourcePosition()
    {
        TestPointSource[] sources = CreateSources();
        var grid = new SpatialRasterGrid(
            origin: new PointXY(2f, 0f),
            size: new VectorXY(1f, 1f),
            resolution: new VectorXYInt(1, 1));

        Raster<RGBA16BitColor> raster = sources.RasterizeCullingMap(
            grid,
            new ThirdSourceCuller(sources),
            SourceColor);

        Assert.That(raster[0, 0], Is.EqualTo(SourceColor(sources[2].Position)));
    }

    [Test]
    public void CullingMapRGBA16BitRasterizer_WhenConstructedWithNullCuller_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CullingMapRGBA16BitRasterizer<TestPointSource>(null!, SourceColor));
    }

    [Test]
    public void CullingMapRGBA16BitRasterizer_WhenConstructedWithNullColorSelector_Throws()
    {
        var culler = new FixedCuller(new TestPointSource(new PointXY(0f, 0f)));

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CullingMapRGBA16BitRasterizer<TestPointSource>(culler, null!));

        Assert.That(exception!.ParamName, Is.EqualTo("sourcePositionToColor"));
    }

    [Test]
    public void Rasterize_WhenSourceIsEmpty_Throws()
    {
        var culler = new FixedCuller(new TestPointSource(new PointXY(0f, 0f)));
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(culler, SourceColor);

        var exception = Assert.Throws<ArgumentException>(() =>
            rasterizer.Rasterize(Array.Empty<TestPointSource>(), CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        TestPointSource[] sources = CreateSources();
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(new FixedCuller(sources[0]), SourceColor);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rasterizer.Rasterize(sources, default));
    }

    [Test]
    public void Rasterize_WhenCullerReturnsNull_ThrowsInvalidOperationException()
    {
        TestPointSource[] sources = CreateSources();
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(new NullCuller(), SourceColor);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            rasterizer.Rasterize(sources, CreateGrid()));

        Assert.That(exception!.Message, Does.Contain("returned null"));
    }

    [Test]
    public void Rasterize_WhenCullerReturnsForeignSource_ThrowsInvalidOperationException()
    {
        TestPointSource[] sources = CreateSources();
        var foreignSource = new TestPointSource(new PointXY(10f, 10f));
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(new FixedCuller(foreignSource), SourceColor);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            rasterizer.Rasterize(sources, CreateGrid()));

        Assert.That(exception!.Message, Does.Contain("not present"));
    }

    private static SpatialRasterGrid CreateGrid()
    {
        return new SpatialRasterGrid(new PointXY(0f, 0f), new VectorXY(1f, 1f), new VectorXYInt(1, 1));
    }

    private static TestPointSource[] CreateSources()
    {
        return new[]
        {
            new TestPointSource(new PointXY(0f, 0f)),
            new TestPointSource(new PointXY(1f, 0f)),
            new TestPointSource(new PointXY(2f, 0f))
        };
    }

    private static RGBA16BitColor SourceColor(PointXY point)
    {
        if (point.X < 0.5f)
            return new RGBA16BitColor(60000, 0, 0, ushort.MaxValue);

        if (point.X < 1.5f)
            return new RGBA16BitColor(0, 60000, 0, ushort.MaxValue);

        return new RGBA16BitColor(0, 0, 60000, ushort.MaxValue);
    }

    private sealed class XBandCuller : IInfluenceSourceCuller<TestPointSource>
    {
        private readonly TestPointSource[] _sources;

        public XBandCuller(TestPointSource[] sources)
        {
            _sources = sources;
        }

        public List<TestPointSource> Cull(PointXY point)
        {
            if (point.X < 1f)
                return new List<TestPointSource> { _sources[0] };

            if (point.X < 2f)
                return new List<TestPointSource> { _sources[0], _sources[1] };

            return new List<TestPointSource> { _sources[0], _sources[1], _sources[2] };
        }
    }

    private sealed class ThirdSourceCuller : IInfluenceSourceCuller<TestPointSource>
    {
        private readonly TestPointSource[] _sources;

        public ThirdSourceCuller(TestPointSource[] sources)
        {
            _sources = sources;
        }

        public List<TestPointSource> Cull(PointXY point)
        {
            return new List<TestPointSource> { _sources[2] };
        }
    }

    private sealed class FixedCuller : IInfluenceSourceCuller<TestPointSource>
    {
        private readonly TestPointSource _source;

        public FixedCuller(TestPointSource source)
        {
            _source = source;
        }

        public List<TestPointSource> Cull(PointXY point)
        {
            return new List<TestPointSource> { _source };
        }
    }

    private sealed class NullCuller : IInfluenceSourceCuller<TestPointSource>
    {
        public List<TestPointSource> Cull(PointXY point)
        {
            return null!;
        }
    }

    private sealed class TestPointSource : IPointInfluenceSource
    {
        public TestPointSource(PointXY position)
        {
            Position = position;
        }

        public PointXY Position { get; }

        public float Weight => 1f;

        public float Distance(PointXY point)
        {
            float dx = Position.X - point.X;
            float dy = Position.Y - point.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }
}
