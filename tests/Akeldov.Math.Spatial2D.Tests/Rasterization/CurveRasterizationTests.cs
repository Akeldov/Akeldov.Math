using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class CurveRasterizationTests
{
    [Test]
    public void Rasterize_WhenCurveIsLine_MapsDistanceToGray8()
    {
        ICurve curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        Raster<byte> raster = curve.Rasterize(grid, new PointDistanceProviderGray8BitRasterizer(ToGray8));

        Assert.That(raster[1, 0], Is.EqualTo(10));
        Assert.That(raster[1, 1], Is.EqualTo(0));
        Assert.That(raster[1, 2], Is.EqualTo(10));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionIsProvided_MapsDistanceToNearestCurve()
    {
        IReadOnlyList<ICurve> curves = new ICurve[]
        {
            new Line(new PointXY(0f, -1f), new PointXY(1f, -1f)),
            new Line(new PointXY(0f, 1f), new PointXY(1f, 1f))
        };
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        Raster<byte> raster = curves.Rasterize(grid, new PointDistanceProviderCollectionGray8BitRasterizer(ToGray8));

        Assert.That(raster[1, 0], Is.EqualTo(0));
        Assert.That(raster[1, 1], Is.EqualTo(10));
        Assert.That(raster[1, 2], Is.EqualTo(0));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveIsProvided_MapsDistanceAndCurveCoordinateToGray8()
    {
        IParameterizedCurve curve = new ParameterizedSegment(new PointXY(-1f, 0f), new PointXY(1f, 0f));
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        Raster<byte> raster = curve.Rasterize(
            grid,
            new ParameterizedCurveDistanceGray8BitRasterizer(ToParameterizedGray8));

        Assert.That(raster[0, 1], Is.EqualTo(0));
        Assert.That(raster[1, 1], Is.EqualTo(20));
        Assert.That(raster[2, 1], Is.EqualTo(40));
        Assert.That(raster[1, 2], Is.EqualTo(30));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionIsProvided_MapsNearestProjectionToGray8()
    {
        IReadOnlyList<IParameterizedCurve> curves = new IParameterizedCurve[]
        {
            new ParameterizedSegment(new PointXY(-1f, -1f), new PointXY(1f, -1f)),
            new ParameterizedSegment(new PointXY(-1f, 1f), new PointXY(1f, 1f))
        };
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        Raster<byte> raster = curves.Rasterize(
            grid,
            new ParameterizedCurveCollectionDistanceGray8BitRasterizer(ToParameterizedGray8));

        Assert.That(raster[0, 0], Is.EqualTo(0));
        Assert.That(raster[1, 1], Is.EqualTo(30));
        Assert.That(raster[2, 2], Is.EqualTo(40));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        ICurve curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        IReadOnlyList<ICurve> curves = new ICurve[] { curve };
        IParameterizedCurve parameterizedCurve = new ParameterizedSegment(new PointXY(-1f, 0f), new PointXY(1f, 0f));
        IReadOnlyList<IParameterizedCurve> parameterizedCurves = new IParameterizedCurve[] { parameterizedCurve };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.Rasterize(default, new PointDistanceProviderGray8BitRasterizer(ToGray8)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curves.Rasterize(default, new PointDistanceProviderCollectionGray8BitRasterizer(ToGray8)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            parameterizedCurve.Rasterize(default, new ParameterizedCurveDistanceGray8BitRasterizer(ToParameterizedGray8)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            parameterizedCurves.Rasterize(default, new ParameterizedCurveCollectionDistanceGray8BitRasterizer(ToParameterizedGray8)));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionIsEmpty_Throws()
    {
        IReadOnlyList<ICurve> curves = Array.Empty<ICurve>();
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() =>
            curves.Rasterize(grid, new PointDistanceProviderCollectionGray8BitRasterizer(ToGray8)));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionContainsNull_Throws()
    {
        IReadOnlyList<ICurve> curves = new ICurve[] { null! };
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() =>
            curves.Rasterize(grid, new PointDistanceProviderCollectionGray8BitRasterizer(ToGray8)));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionIsEmpty_Throws()
    {
        IReadOnlyList<IParameterizedCurve> curves = Array.Empty<IParameterizedCurve>();
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() =>
            curves.Rasterize(grid, new ParameterizedCurveCollectionDistanceGray8BitRasterizer(ToParameterizedGray8)));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionContainsNull_Throws()
    {
        IReadOnlyList<IParameterizedCurve> curves = new IParameterizedCurve[] { null! };
        SpatialRasterGrid grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() =>
            curves.Rasterize(grid, new ParameterizedCurveCollectionDistanceGray8BitRasterizer(ToParameterizedGray8)));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    private static SpatialRasterGrid CreateThreeByThreeGrid()
    {
        return new SpatialRasterGrid(
            origin: new PointXY(-1.5f, -1.5f),
            size: new VectorXY(3f, 3f),
            resolution: new VectorXYInt(3, 3));
    }

    private static byte ToGray8(float distance)
    {
        return (byte)MathF.Round(distance * 10f);
    }

    private static byte ToParameterizedGray8(float distance, float curveCoordinate)
    {
        return (byte)MathF.Round(distance * 10f + curveCoordinate * 20f);
    }
}
