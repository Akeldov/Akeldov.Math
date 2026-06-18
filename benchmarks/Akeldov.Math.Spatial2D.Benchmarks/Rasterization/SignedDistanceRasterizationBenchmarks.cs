using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;
using BenchmarkDotNet.Attributes;

namespace Akeldov.Math.Spatial2D.Benchmarks.Rasterization;

[MemoryDiagnoser]
[ShortRunJob]
public class SignedDistanceRasterizationBenchmarks
{
    private CompositeContour _contour = null!;
    private ContourBasedRegion _region = null!;
    private RasterGrid _grid;
    private SignedPointDistanceProviderGray8BitRasterizer _contourGray8Rasterizer = null!;
    private SignedPointDistanceProviderGray16BitRasterizer _contourGray16Rasterizer = null!;
    private SignedPointDistanceProviderGray8BitRasterizer _regionGray8Rasterizer = null!;
    private SignedPointDistanceProviderGray16BitRasterizer _regionGray16Rasterizer = null!;

    [Params(128, 256)]
    public int Resolution { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _contour = CreateSquareContour(0f, 0f, 100f, 100f).FilletCorners(6f);
        _region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 100f, 100f),
            CreateSquareContour(35f, 35f, 65f, 65f)
        });
        _grid = new RasterGrid(
            origin: new PointXY(-10f, -10f),
            size: new VectorXY(120f, 120f),
            resolution: new VectorXYInt(Resolution, Resolution));
        _contourGray8Rasterizer = new SignedPointDistanceProviderGray8BitRasterizer(ToGray8);
        _contourGray16Rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(ToGray16);
        _regionGray8Rasterizer = new SignedPointDistanceProviderGray8BitRasterizer(ToGray8);
        _regionGray16Rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(ToGray16);
    }

    [Benchmark]
    public Gray8BitRaster RasterizeContourGray8()
    {
        return _contour.Rasterize(_grid, _contourGray8Rasterizer);
    }

    [Benchmark]
    public Gray16BitRaster RasterizeContourGray16()
    {
        return _contour.Rasterize(_grid, _contourGray16Rasterizer);
    }

    [Benchmark]
    public Gray8BitRaster RasterizeRegionGray8()
    {
        return _region.Rasterize(_grid, _regionGray8Rasterizer);
    }

    [Benchmark]
    public Gray16BitRaster RasterizeRegionGray16()
    {
        return _region.Rasterize(_grid, _regionGray16Rasterizer);
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

    private static byte ToGray8(float signedDistance)
    {
        if (signedDistance <= 0f)
            return byte.MaxValue;

        const float falloffDistance = 4f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return (byte)MathF.Round(normalized * byte.MaxValue);
    }

    private static ushort ToGray16(float signedDistance)
    {
        if (signedDistance <= 0f)
            return ushort.MaxValue;

        const float falloffDistance = 4f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return (ushort)MathF.Round(normalized * ushort.MaxValue);
    }
}
