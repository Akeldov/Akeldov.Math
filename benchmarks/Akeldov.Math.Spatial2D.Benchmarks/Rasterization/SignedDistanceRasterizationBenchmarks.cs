using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
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
    private SpatialRasterGrid _grid;
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
        _grid = new SpatialRasterGrid(
            origin: new PointXY(-10f, -10f),
            size: new VectorXY(120f, 120f),
            resolution: new VectorXYInt(Resolution, Resolution));
        _contourGray8Rasterizer = new SignedPointDistanceProviderGray8BitRasterizer(ToGray8);
        _contourGray16Rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(ToGray16);
        _regionGray8Rasterizer = new SignedPointDistanceProviderGray8BitRasterizer(ToGray8);
        _regionGray16Rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(ToGray16);
    }

    [Benchmark]
    public SpatialRaster<Gray8BitColor> RasterizeContourGray8()
    {
        return _contour.Rasterize(_grid, _contourGray8Rasterizer);
    }

    [Benchmark]
    public SpatialRaster<Gray16BitColor> RasterizeContourGray16()
    {
        return _contour.Rasterize(_grid, _contourGray16Rasterizer);
    }

    [Benchmark]
    public SpatialRaster<Gray8BitColor> RasterizeRegionGray8()
    {
        return _region.Rasterize(_grid, _regionGray8Rasterizer);
    }

    [Benchmark]
    public SpatialRaster<Gray16BitColor> RasterizeRegionGray16()
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

    private static Gray8BitColor ToGray8(float signedDistance)
    {
        if (signedDistance <= 0f)
            return Gray8BitColor.White;

        const float falloffDistance = 4f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return new Gray8BitColor((byte)MathF.Round(normalized * byte.MaxValue));
    }

    private static Gray16BitColor ToGray16(float signedDistance)
    {
        if (signedDistance <= 0f)
            return Gray16BitColor.White;

        const float falloffDistance = 4f;
        float normalized = 1f - System.Math.Clamp(signedDistance / falloffDistance, 0f, 1f);
        return new Gray16BitColor((ushort)MathF.Round(normalized * ushort.MaxValue));
    }
}
