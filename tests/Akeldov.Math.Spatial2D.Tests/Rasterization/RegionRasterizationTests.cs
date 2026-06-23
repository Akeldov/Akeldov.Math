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
        var grid = new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(4f, 4f),
            resolution: new VectorXYInt(4, 4));

        Raster<byte> raster = region.Rasterize(grid, new SignedPointDistanceProviderGray8BitRasterizer(ToMaskValue));

        Assert.That(raster[0, 0], Is.EqualTo(byte.MaxValue));
        Assert.That(raster[1, 1], Is.EqualTo(byte.MinValue));
        Assert.That(raster[2, 2], Is.EqualTo(byte.MinValue));
        Assert.That(raster[3, 3], Is.EqualTo(byte.MaxValue));
    }

    [Test]
    public void Rasterize_WhenSourceIsRectangleRegion_UsesRegionSignedDistance()
    {
        IRegion region = new Rectangle(new PointXY(0f, 0f), new PointXY(2f, 2f));
        var grid = new RasterGrid(
            origin: new PointXY(0f, 0f),
            size: new VectorXY(3f, 1f),
            resolution: new VectorXYInt(3, 1));

        Raster<byte> raster = region.Rasterize(grid, new SignedPointDistanceProviderGray8BitRasterizer(ToMaskValue));

        Assert.That(raster[0, 0], Is.EqualTo(byte.MaxValue));
        Assert.That(raster[1, 0], Is.EqualTo(byte.MaxValue));
        Assert.That(raster[2, 0], Is.EqualTo(byte.MinValue));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f)
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            region.Rasterize(default, new SignedPointDistanceProviderGray8BitRasterizer(ToMaskValue)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            region.Rasterize(default, new SignedPointDistanceProviderGray16BitRasterizer(ToGray16)));
    }

    [Test]
    public void SaveAsPng_WhenSquareWithSquareHoleIsRasterizedToGray16Bit_WritesPng16()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });
        var grid = new RasterGrid(
            origin: new PointXY(-0.5f, -0.5f),
            size: new VectorXY(5f, 5f),
            resolution: new VectorXYInt(160, 160));
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "square-with-square-hole-gray16.png");

        region
            .Rasterize(grid, new SignedPointDistanceProviderGray16BitRasterizer(ToDistanceGray16))
            .SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    private static byte ToMaskValue(float signedDistance)
    {
        return signedDistance <= 0f ? byte.MaxValue : byte.MinValue;
    }

    private static ushort ToGray16(float signedDistance)
    {
        return signedDistance <= 0f ? ushort.MaxValue : ushort.MinValue;
    }

    private static ushort ToDistanceGray16(float signedDistance)
    {
        if (signedDistance <= 0f)
            return ushort.MaxValue;

        const float falloffDistance = 0.2f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return (ushort)System.MathF.Round(normalized * ushort.MaxValue);
    }

    private static CompositeContour CreateSquareContour(float left, float bottom, float right, float top)
    {
        return new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
            new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
            new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
            new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
        });
    }
}
