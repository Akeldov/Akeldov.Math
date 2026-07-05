using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class ContourRasterizationImageTests
{
    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        var contour = CreateTriangleContour();

        Assert.Throws<ArgumentOutOfRangeException>(() => contour.Rasterize(ToGray8, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => contour.Rasterize(ToGray16, default));
    }

    [Test]
    public void SaveAsBmp_WhenRasterHasZeroSize_Throws()
    {
        var raster = new SpatialRaster<byte>(default, Array.Empty<byte>());
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "zero-size-gray8.bmp");

        if (File.Exists(path))
            File.Delete(path);

        Assert.Throws<ArgumentException>(() => raster.SaveAsBmp(path));
        Assert.That(File.Exists(path), Is.False);
    }

    [Test]
    public void SaveAsPng_WhenRasterHasZeroSize_Throws()
    {
        var raster = new SpatialRaster<ushort>(default, Array.Empty<ushort>());
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "zero-size-gray16.png");

        if (File.Exists(path))
            File.Delete(path);

        Assert.Throws<ArgumentException>(() => raster.SaveAsPng(path));
        Assert.That(File.Exists(path), Is.False);
    }

    [Test]
    public void SaveAsBmp_WhenTriangleIsRasterizedToGray8Bit_WritesBmp8()
    {
        var contour = CreateTriangleContour();
        var grid = CreateTriangleGrid();
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "triangle-gray8.bmp");

        contour
            .Rasterize(ToGray8, grid)
            .SaveAsBmp(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..2], Is.EqualTo(new byte[] { 66, 77 }));
        Assert.That(BitConverter.ToUInt16(bytes, 28), Is.EqualTo(8));
    }

    [Test]
    public void SaveAsPng_WhenTriangleIsRasterizedToGray16Bit_WritesPng16()
    {
        var contour = CreateTriangleContour();
        var grid = CreateTriangleGrid();
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "triangle-gray16.png");

        contour
            .Rasterize(ToGray16, grid)
            .SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    [Test]
    public void SaveAsPng_WhenRoundedSquareIsRasterizedToGray16Bit_WritesPng16()
    {
        CompositeContour contour = CreateSquareContour().FilletCorners(0.35f);
        var grid = new SpatialRasterGrid(
            origin: new PointXY(-0.5f, -0.5f),
            size: new VectorXY(5f, 5f),
            resolution: new VectorXYInt(160, 160));
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "rounded-square-gray16.png");

        contour
            .Rasterize(ToGray16, grid)
            .SaveAsPng(path);

        Assert.That(File.Exists(path), Is.True);
        Assert.That(new FileInfo(path).Length, Is.GreaterThan(0));

        byte[] bytes = File.ReadAllBytes(path);
        Assert.That(bytes[0..8], Is.EqualTo(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));
        Assert.That(bytes[24], Is.EqualTo(16));
        Assert.That(bytes[25], Is.EqualTo(0));
    }

    private static CompositeContour CreateTriangleContour()
    {
        return new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(4f, 0f)),
            new ParameterizedSegment(new PointXY(4f, 0f), new PointXY(2f, 3.5f)),
            new ParameterizedSegment(new PointXY(2f, 3.5f), new PointXY(0f, 0f))
        });
    }

    private static CompositeContour CreateSquareContour()
    {
        return new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(4f, 0f)),
            new ParameterizedSegment(new PointXY(4f, 0f), new PointXY(4f, 4f)),
            new ParameterizedSegment(new PointXY(4f, 4f), new PointXY(0f, 4f)),
            new ParameterizedSegment(new PointXY(0f, 4f), new PointXY(0f, 0f))
        });
    }

    private static SpatialRasterGrid CreateTriangleGrid()
    {
        return new SpatialRasterGrid(
            origin: new PointXY(-0.5f, -0.5f),
            size: new VectorXY(5f, 4.5f),
            resolution: new VectorXYInt(128, 128));
    }

    private static byte ToGray8(float signedDistance)
    {
        signedDistance = MathF.Abs(signedDistance);

        const float falloffDistance = 0.2f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return (byte)System.MathF.Round(normalized * byte.MaxValue);
    }

    private static ushort ToGray16(float signedDistance)
    {
        signedDistance = MathF.Abs(signedDistance);

        const float falloffDistance = 0.2f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return (ushort)System.MathF.Round(normalized * ushort.MaxValue);
    }
}
