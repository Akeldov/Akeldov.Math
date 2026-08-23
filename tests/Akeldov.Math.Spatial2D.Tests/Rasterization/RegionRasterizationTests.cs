using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class RegionRasterizationTests
{
    [Test]
    public void Rasterize_WhenRegionHasHole_UsesRegionFillRule()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });
        var grid = new RasterGeometry(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(4f, 4f),
            resolution: new VectorXYInt(4, 4));

        SpatialRaster<Gray8BitColor> raster = region.Rasterize(ToMaskValue, grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(byte.MaxValue));
        Assert.That(raster[1, 1].Value, Is.EqualTo(byte.MinValue));
        Assert.That(raster[2, 2].Value, Is.EqualTo(byte.MinValue));
        Assert.That(raster[3, 3].Value, Is.EqualTo(byte.MaxValue));
    }

    [Test]
    public void Rasterize_WhenSourceIsRectangleRegion_UsesRegionSignedDistance()
    {
        IRegion region = new Rectangle(new PointXY(0f, 0f), new PointXY(2f, 2f));
        var grid = new RasterGeometry(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        SpatialRaster<Gray8BitColor> raster = region.Rasterize(ToMaskValue, grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(byte.MaxValue));
        Assert.That(raster[1, 0].Value, Is.EqualTo(byte.MaxValue));
        Assert.That(raster[2, 0].Value, Is.EqualTo(byte.MinValue));
    }

    [Test]
    public void Rasterize_WhenSignedDistanceProviderCollectionIsProvided_MapsMinimumSignedDistanceGray8()
    {
        IReadOnlyList<ISignedPointDistanceProvider> regions = CreateSeparatedDiskRegions();
        var grid = CreateThreeByOneGrid();

        SpatialRaster<Gray8BitColor> raster = regions.Rasterize(ToMaskValue, grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(byte.MaxValue));
        Assert.That(raster[1, 0].Value, Is.EqualTo(byte.MinValue));
        Assert.That(raster[2, 0].Value, Is.EqualTo(byte.MaxValue));
    }

    [Test]
    public void Rasterize_WhenSignedDistanceProviderCollectionIsProvided_MapsMinimumSignedDistanceGray16()
    {
        IReadOnlyList<ISignedPointDistanceProvider> regions = CreateSeparatedDiskRegions();
        var grid = CreateThreeByOneGrid();

        SpatialRaster<Gray16BitColor> raster = regions.Rasterize(ToGray16, grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(ushort.MaxValue));
        Assert.That(raster[1, 0].Value, Is.EqualTo(ushort.MinValue));
        Assert.That(raster[2, 0].Value, Is.EqualTo(ushort.MaxValue));
    }

    [Test]
    public void Constructor_WhenSignedDistanceCollectionMapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SignedPointDistanceProviderCollectionGray8BitRasterizer(null!));
        Assert.Throws<ArgumentNullException>(() =>
            new SignedPointDistanceProviderCollectionGray16BitRasterizer(null!));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f)
        });
        IReadOnlyList<ISignedPointDistanceProvider> regions = new ISignedPointDistanceProvider[] { region };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            region.Rasterize(ToMaskValue, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            region.Rasterize(ToGray16, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            regions.Rasterize(ToMaskValue, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            regions.Rasterize(ToGray16, default));
    }

    [Test]
    public void Rasterize_WhenSignedDistanceProviderCollectionIsEmpty_Throws()
    {
        IReadOnlyList<ISignedPointDistanceProvider> regions = Array.Empty<ISignedPointDistanceProvider>();
        var grid = CreateThreeByOneGrid();

        var exception = Assert.Throws<ArgumentException>(() => regions.Rasterize(ToMaskValue, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() => regions.Rasterize(ToGray16, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenSignedDistanceProviderCollectionContainsNull_Throws()
    {
        IReadOnlyList<ISignedPointDistanceProvider> regions = new ISignedPointDistanceProvider[] { null! };
        var grid = CreateThreeByOneGrid();

        var exception = Assert.Throws<ArgumentException>(() => regions.Rasterize(ToMaskValue, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() => regions.Rasterize(ToGray16, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void SaveAsPng_WhenSquareWithSquareHoleIsRasterizedToGray16Bit_WritesPng16()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });
        var grid = new RasterGeometry(
            origin: new PointXY(-0.5f, -0.5f),
            size: new VectorXY(5f, 5f),
            resolution: new VectorXYInt(160, 160));
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "square-with-square-hole-gray16.png");

        region
            .Rasterize(ToDistanceGray16, grid)
            .SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    private static Gray8BitColor ToMaskValue(float signedDistance)
    {
        return signedDistance <= 0f ? Gray8BitColor.White : Gray8BitColor.Black;
    }

    private static Gray16BitColor ToGray16(float signedDistance)
    {
        return signedDistance <= 0f ? Gray16BitColor.White : Gray16BitColor.Black;
    }

    private static Gray16BitColor ToDistanceGray16(float signedDistance)
    {
        if (signedDistance <= 0f)
            return Gray16BitColor.White;

        const float falloffDistance = 0.2f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return new Gray16BitColor((ushort)System.MathF.Round(normalized * ushort.MaxValue));
    }

    private static CompositeContour CreateSquareContour(float left, float bottom, float right, float top)
    {
        return new CompositeContour(new IContourPath[]
        {
            new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
            new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
            new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
            new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
        });
    }

    private static IReadOnlyList<ISignedPointDistanceProvider> CreateSeparatedDiskRegions()
    {
        return new ISignedPointDistanceProvider[]
        {
            new Disk(new PointXY(-1f, 0f), 0.5f),
            new Disk(new PointXY(1f, 0f), 0.5f)
        };
    }

    private static RasterGeometry CreateThreeByOneGrid()
    {
        return new RasterGeometry(
            origin: new PointXY(-1.5f, -0.5f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));
    }
}
