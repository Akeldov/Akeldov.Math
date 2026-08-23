using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class CullingMapRasterizationTests
{
    [Test]
    public void RasterizeCullingMap_WhenIndexSelectsOneTwoAndThreeSources_BlendsSourceColorsInLinearRgb()
    {
        TestPointSource[] sources = CreateSources();
        var grid = new RasterGeometry(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        SpatialRaster<RGBA16BitColor> raster = new XBandIndex(sources).RasterizeCullingMap(
            grid,
            SourceColor);

        Assert.That(raster[0, 0], Is.EqualTo(SourceColor(sources[0].Position)));
        Assert.That(raster[1, 0], Is.EqualTo(new RGBA16BitColor(44045, 44045, 0, ushort.MaxValue)));
        Assert.That(raster[2, 0], Is.EqualTo(new RGBA16BitColor(36638, 36638, 36638, ushort.MaxValue)));
    }

    [Test]
    public void RasterizeCullingMap_WhenColorSelectorUsesPosition_ColorsSelectedSourcePosition()
    {
        TestPointSource[] sources = CreateSources();
        var grid = new RasterGeometry(
            origin: new PointXY(2f, 0f),
            size: new VectorXY(1f, 1f),
            resolution: new VectorXYInt(1, 1));

        SpatialRaster<RGBA16BitColor> raster = new ThirdSourceIndex(sources).RasterizeCullingMap(
            grid,
            SourceColor);

        Assert.That(raster[0, 0], Is.EqualTo(SourceColor(sources[2].Position)));
    }

    [Test]
    public void Rasterize_WhenSourceIndexIsNull_Throws()
    {
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(SourceColor);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            rasterizer.Rasterize(null!, CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void CullingMapRGBA16BitRasterizer_WhenConstructedWithNullColorSelector_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CullingMapRGBA16BitRasterizer<TestPointSource>(null!));

        Assert.That(exception!.ParamName, Is.EqualTo("sourcePositionToColor"));
    }

    [Test]
    public void Rasterize_WhenSourceIsEmpty_Throws()
    {
        var index = new FixedIndex(Array.Empty<TestPointSource>(), new List<TestPointSource>());
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(SourceColor);

        var exception = Assert.Throws<ArgumentException>(() =>
            rasterizer.Rasterize(index, CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        TestPointSource[] sources = CreateSources();
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(SourceColor);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rasterizer.Rasterize(new FixedIndex(sources, new List<TestPointSource> { sources[0] }), default));
    }

    [Test]
    public void Rasterize_WhenIndexReturnsNull_ThrowsInvalidOperationException()
    {
        TestPointSource[] sources = CreateSources();
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(SourceColor);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            rasterizer.Rasterize(new FixedIndex(sources, null), CreateGrid()));

        Assert.That(exception!.Message, Does.Contain("returned null"));
    }

    [Test]
    public void Rasterize_WhenIndexReturnsListContainingNull_ThrowsInvalidOperationException()
    {
        TestPointSource[] sources = CreateSources();
        var rasterizer = new CullingMapRGBA16BitRasterizer<TestPointSource>(SourceColor);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            rasterizer.Rasterize(new FixedIndex(sources, new List<TestPointSource> { null! }), CreateGrid()));

        Assert.That(exception!.Message, Does.Contain("containing null"));
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), new VectorXY(1f, 1f), new VectorXYInt(1, 1));
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

    private sealed class XBandIndex : IInfluenceSourceIndex<TestPointSource>
    {
        public XBandIndex(TestPointSource[] sources)
        {
            Sources = Array.AsReadOnly(sources.ToArray());
        }

        public IReadOnlyList<TestPointSource> Sources { get; }

        public List<TestPointSource> SelectSources(PointXY point)
        {
            if (point.X < 1f)
                return new List<TestPointSource> { Sources[0] };

            if (point.X < 2f)
                return new List<TestPointSource> { Sources[0], Sources[1] };

            return new List<TestPointSource> { Sources[0], Sources[1], Sources[2] };
        }
    }

    private sealed class ThirdSourceIndex : IInfluenceSourceIndex<TestPointSource>
    {
        public ThirdSourceIndex(TestPointSource[] sources)
        {
            Sources = Array.AsReadOnly(sources.ToArray());
        }

        public IReadOnlyList<TestPointSource> Sources { get; }

        public List<TestPointSource> SelectSources(PointXY point)
        {
            return new List<TestPointSource> { Sources[2] };
        }
    }

    private sealed class FixedIndex : IInfluenceSourceIndex<TestPointSource>
    {
        private readonly List<TestPointSource>? _selectedSources;

        public FixedIndex(IReadOnlyList<TestPointSource> sources, List<TestPointSource>? selectedSources)
        {
            Sources = Array.AsReadOnly(sources.ToArray());
            _selectedSources = selectedSources;
        }

        public IReadOnlyList<TestPointSource> Sources { get; }

        public List<TestPointSource> SelectSources(PointXY point)
        {
            return _selectedSources!;
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
