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
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray8BitColor> raster = curve.Rasterize(ToGray8, grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(10));
        Assert.That(raster[1, 1].Value, Is.EqualTo(0));
        Assert.That(raster[1, 2].Value, Is.EqualTo(10));
    }

    [Test]
    public void Rasterize_WhenCurveIsLine_MapsDistanceToGray16()
    {
        ICurve curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray16BitColor> raster = curve.Rasterize(ToGray16, grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(1000));
        Assert.That(raster[1, 1].Value, Is.EqualTo(0));
        Assert.That(raster[1, 2].Value, Is.EqualTo(1000));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionIsProvided_MapsDistanceToNearestCurve()
    {
        IReadOnlyList<ICurve> curves = new ICurve[]
        {
            new Line(new PointXY(0f, -1f), new PointXY(1f, -1f)),
            new Line(new PointXY(0f, 1f), new PointXY(1f, 1f))
        };
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray8BitColor> raster = curves.Rasterize(ToGray8, grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(10));
        Assert.That(raster[1, 2].Value, Is.EqualTo(0));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionIsProvided_MapsDistanceToNearestCurveGray16()
    {
        IReadOnlyList<ICurve> curves = new ICurve[]
        {
            new Line(new PointXY(0f, -1f), new PointXY(1f, -1f)),
            new Line(new PointXY(0f, 1f), new PointXY(1f, 1f))
        };
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray16BitColor> raster = curves.Rasterize(ToGray16, grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(1000));
        Assert.That(raster[1, 2].Value, Is.EqualTo(0));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenCurveIsLine_MapsWidthAndFadeToGray8()
    {
        var curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateHalfFadeGrid();

        SpatialRaster<Gray8BitColor> raster = curve.Rasterize(
            curveWidth: 1f,
            fadeDistance: 0.5f,
            curveColor: new Gray8BitColor(200),
            backgroundColor: new Gray8BitColor(100),
            rasterGeometry: grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(150));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenCurveIsLine_MapsWidthAndFadeToGray16()
    {
        var curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateHalfFadeGrid();

        SpatialRaster<Gray16BitColor> raster = curve.Rasterize(
            curveWidth: 1f,
            fadeDistance: 0.5f,
            curveColor: new Gray16BitColor(2000),
            backgroundColor: new Gray16BitColor(1000),
            rasterGeometry: grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(1500));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenCurveIsLine_MapsWidthAndFadeToRGBA8()
    {
        var curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateHalfFadeGrid();

        SpatialRaster<RGBA8BitColor> raster = curve.Rasterize(
            curveWidth: 1f,
            fadeDistance: 0.5f,
            curveColor: new RGBA8BitColor(200, 100, 0, 50),
            backgroundColor: new RGBA8BitColor(100, 200, 50, 250),
            rasterGeometry: grid);

        Assert.That(raster[0, 0], Is.EqualTo(new RGBA8BitColor(150, 150, 25, 150)));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenCurveIsLine_MapsWidthAndFadeToRGBA16()
    {
        var curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateHalfFadeGrid();

        SpatialRaster<RGBA16BitColor> raster = curve.Rasterize(
            curveWidth: 1f,
            fadeDistance: 0.5f,
            curveColor: new RGBA16BitColor(2000, 1000, 0, 500),
            backgroundColor: new RGBA16BitColor(1000, 2000, 500, 2500),
            rasterGeometry: grid);

        Assert.That(raster[0, 0], Is.EqualTo(new RGBA16BitColor(1500, 1500, 250, 1500)));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenCurveCollectionIsProvided_MapsNearestCurveToGray8()
    {
        IReadOnlyList<Line> curves = new[]
        {
            new Line(new PointXY(0f, -1f), new PointXY(1f, -1f)),
            new Line(new PointXY(0f, 1f), new PointXY(1f, 1f))
        };
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray8BitColor> raster = curves.Rasterize(
            curveWidth: 0.5f,
            fadeDistance: 0f,
            curveColor: new Gray8BitColor(200),
            backgroundColor: new Gray8BitColor(10),
            rasterGeometry: grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(200));
        Assert.That(raster[1, 1].Value, Is.EqualTo(10));
        Assert.That(raster[1, 2].Value, Is.EqualTo(200));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenCurveCollectionIsProvided_MapsNearestCurveToRGBA16()
    {
        IReadOnlyList<Line> curves = new[]
        {
            new Line(new PointXY(0f, -1f), new PointXY(1f, -1f)),
            new Line(new PointXY(0f, 1f), new PointXY(1f, 1f))
        };
        RasterGeometry grid = CreateThreeByThreeGrid();
        var curveColor = new RGBA16BitColor(2000, 0, 0, ushort.MaxValue);
        var backgroundColor = new RGBA16BitColor(0, 0, 0, 0);

        SpatialRaster<RGBA16BitColor> raster = curves.Rasterize(
            curveWidth: 0.5f,
            fadeDistance: 0f,
            curveColor: curveColor,
            backgroundColor: backgroundColor,
            rasterGeometry: grid);

        Assert.That(raster[1, 0], Is.EqualTo(curveColor));
        Assert.That(raster[1, 1], Is.EqualTo(backgroundColor));
        Assert.That(raster[1, 2], Is.EqualTo(curveColor));
    }

    [Test]
    public void Rasterize_WhenPointDistanceProviderIsConcreteValueType_MapsDistanceToGray16()
    {
        var source = new PointXY(0f, 0f);
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray16BitColor> raster = source.Rasterize(ToGray16, grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(1000));
        Assert.That(raster[1, 1].Value, Is.EqualTo(0));
        Assert.That(raster[1, 2].Value, Is.EqualTo(1000));
    }

    [Test]
    public void Rasterize_WhenPointDistanceProviderCollectionIsConcreteValueTypeList_MapsNearestDistanceToGray8()
    {
        IReadOnlyList<PointXY> sources = new[]
        {
            new PointXY(0f, -1f),
            new PointXY(0f, 1f)
        };
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray8BitColor> raster = sources.Rasterize(ToGray8, grid);

        Assert.That(raster[1, 0].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(10));
        Assert.That(raster[1, 2].Value, Is.EqualTo(0));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveIsProvided_MapsDistanceAndCurveCoordinateToGray8()
    {
        IParameterizedCurve curve = new ParameterizedSegment(new PointXY(-1f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray8BitColor> raster = curve.Rasterize(ToParameterizedGray8, grid);

        Assert.That(raster[0, 1].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(20));
        Assert.That(raster[2, 1].Value, Is.EqualTo(40));
        Assert.That(raster[1, 2].Value, Is.EqualTo(30));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveIsProvided_MapsDistanceAndCurveCoordinateToGray16()
    {
        IParameterizedCurve curve = new ParameterizedSegment(new PointXY(-1f, 0f), new PointXY(1f, 0f));
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray16BitColor> raster = curve.Rasterize(ToParameterizedGray16, grid);

        Assert.That(raster[0, 1].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(2000));
        Assert.That(raster[2, 1].Value, Is.EqualTo(4000));
        Assert.That(raster[1, 2].Value, Is.EqualTo(3000));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionIsProvided_MapsNearestProjectionToGray8()
    {
        IReadOnlyList<IParameterizedCurve> curves = new IParameterizedCurve[]
        {
            new ParameterizedSegment(new PointXY(-1f, -1f), new PointXY(1f, -1f)),
            new ParameterizedSegment(new PointXY(-1f, 1f), new PointXY(1f, 1f))
        };
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray8BitColor> raster = curves.Rasterize(ToParameterizedGray8, grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(30));
        Assert.That(raster[2, 2].Value, Is.EqualTo(40));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionIsProvided_MapsNearestProjectionToGray16()
    {
        IReadOnlyList<IParameterizedCurve> curves = new IParameterizedCurve[]
        {
            new ParameterizedSegment(new PointXY(-1f, -1f), new PointXY(1f, -1f)),
            new ParameterizedSegment(new PointXY(-1f, 1f), new PointXY(1f, 1f))
        };
        RasterGeometry grid = CreateThreeByThreeGrid();

        SpatialRaster<Gray16BitColor> raster = curves.Rasterize(ToParameterizedGray16, grid);

        Assert.That(raster[0, 0].Value, Is.EqualTo(0));
        Assert.That(raster[1, 1].Value, Is.EqualTo(3000));
        Assert.That(raster[2, 2].Value, Is.EqualTo(4000));
    }

    [Test]
    public void Constructor_WhenParameterizedCurveCollectionGray16MapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ParameterizedCurveCollectionDistanceGray16BitRasterizer(null!));
    }

    [Test]
    public void Constructor_WhenParameterizedCurveGray16MapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ParameterizedCurveDistanceGray16BitRasterizer(null!));
    }

    [Test]
    public void Constructor_WhenPointDistanceProviderCollectionGray16MapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PointDistanceProviderCollectionGray16BitRasterizer(null!));
    }

    [Test]
    public void Constructor_WhenPointDistanceProviderGray16MapperIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PointDistanceProviderGray16BitRasterizer(null!));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        ICurve curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        IReadOnlyList<ICurve> curves = new ICurve[] { curve };
        IParameterizedCurve parameterizedCurve = new ParameterizedSegment(new PointXY(-1f, 0f), new PointXY(1f, 0f));
        IReadOnlyList<IParameterizedCurve> parameterizedCurves = new IParameterizedCurve[] { parameterizedCurve };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.Rasterize(ToGray8, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curve.Rasterize(ToGray16, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curves.Rasterize(ToGray8, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            curves.Rasterize(ToGray16, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            parameterizedCurve.Rasterize(ToParameterizedGray8, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            parameterizedCurve.Rasterize(ToParameterizedGray16, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            parameterizedCurves.Rasterize(ToParameterizedGray8, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            parameterizedCurves.Rasterize(ToParameterizedGray16, default));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionIsEmpty_Throws()
    {
        IReadOnlyList<ICurve> curves = Array.Empty<ICurve>();
        RasterGeometry grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToGray8, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToGray16, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenCurveCollectionContainsNull_Throws()
    {
        IReadOnlyList<ICurve> curves = new ICurve[] { null! };
        RasterGeometry grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToGray8, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToGray16, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionIsEmpty_Throws()
    {
        IReadOnlyList<IParameterizedCurve> curves = Array.Empty<IParameterizedCurve>();
        RasterGeometry grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToParameterizedGray8, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToParameterizedGray16, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WhenParameterizedCurveCollectionContainsNull_Throws()
    {
        IReadOnlyList<IParameterizedCurve> curves = new IParameterizedCurve[] { null! };
        RasterGeometry grid = CreateThreeByThreeGrid();

        var exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToParameterizedGray8, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));

        exception = Assert.Throws<ArgumentException>(() => curves.Rasterize(ToParameterizedGray16, grid));

        Assert.That(exception!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Rasterize_WithCurveStyle_WhenArgumentsAreInvalid_Throws()
    {
        var curve = new Line(new PointXY(0f, 0f), new PointXY(1f, 0f));
        ICurve nullCurveValue = null!;
        IReadOnlyList<ICurve> emptyCurves = Array.Empty<ICurve>();
        IReadOnlyList<ICurve> nullCurve = new ICurve[] { null! };
        RasterGeometry grid = CreateThreeByThreeGrid();

        Assert.That(
            Assert.Throws<ArgumentNullException>(() =>
                nullCurveValue.Rasterize(1f, 0f, new Gray8BitColor(1), new Gray8BitColor(0), grid))!.ParamName,
            Is.EqualTo("curve"));
        Assert.That(
            Assert.Throws<ArgumentNullException>(() =>
                ((IReadOnlyList<Line>)null!).Rasterize(1f, 0f, new Gray8BitColor(1), new Gray8BitColor(0), grid))!.ParamName,
            Is.EqualTo("curves"));
        Assert.That(
            Assert.Throws<ArgumentException>(() =>
                emptyCurves.Rasterize(1f, 0f, new Gray8BitColor(1), new Gray8BitColor(0), grid))!.ParamName,
            Is.EqualTo("curves"));
        Assert.That(
            Assert.Throws<ArgumentException>(() =>
                nullCurve.Rasterize(1f, 0f, new Gray8BitColor(1), new Gray8BitColor(0), grid))!.ParamName,
            Is.EqualTo("curves"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Rasterize(float.NaN, 0f, new Gray8BitColor(1), new Gray8BitColor(0), grid))!.ParamName,
            Is.EqualTo("curveWidth"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Rasterize(1f, -1f, new Gray8BitColor(1), new Gray8BitColor(0), grid))!.ParamName,
            Is.EqualTo("fadeDistance"));
        Assert.That(
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Rasterize(1f, 0f, new Gray8BitColor(1), new Gray8BitColor(0), default))!.ParamName,
            Is.EqualTo("rasterGeometry"));
    }

    private static RasterGeometry CreateHalfFadeGrid()
    {
        return new RasterGeometry(
            origin: new PointXY(-0.5f, 0.5f),
            size: new VectorXY(1f, 0.5f),
            resolution: new VectorXYInt(1, 1));
    }

    private static RasterGeometry CreateThreeByThreeGrid()
    {
        return new RasterGeometry(
            origin: new PointXY(-1.5f, -1.5f),
            size: new VectorXY(3f, 3f),
            resolution: new VectorXYInt(3, 3));
    }

    private static Gray8BitColor ToGray8(float distance)
    {
        return new Gray8BitColor((byte)MathF.Round(distance * 10f));
    }

    private static Gray16BitColor ToGray16(float distance)
    {
        return new Gray16BitColor((ushort)MathF.Round(distance * 1000f));
    }

    private static Gray8BitColor ToParameterizedGray8(float distance, float curveCoordinate)
    {
        return new Gray8BitColor((byte)MathF.Round(distance * 10f + curveCoordinate * 20f));
    }

    private static Gray16BitColor ToParameterizedGray16(float distance, float curveCoordinate)
    {
        return new Gray16BitColor((ushort)MathF.Round(distance * 1000f + curveCoordinate * 2000f));
    }
}
