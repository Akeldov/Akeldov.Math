using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class ContourStrokeRasterizationTests
{
    [Test]
    public void Rasterize_WithGray8Color_MatchesBlackBackgroundCurveRasterization()
    {
        IContour contour = new Circle(new PointXY(0f, 0f), 1f);
        SpatialRasterGrid grid = CreateGrid();
        var color = new Gray8BitColor(200);

        SpatialRaster<Gray8BitColor> actual = contour.Rasterize(0.2f, 0.1f, color, grid);
        SpatialRaster<Gray8BitColor> expected = contour.Rasterize(
            0.2f,
            0.1f,
            color,
            default(Gray8BitColor),
            grid);

        Assert.That(actual.Values, Is.EqualTo(expected.Values));
    }

    [Test]
    public void Rasterize_WithGray16Color_MatchesBlackBackgroundCurveRasterization()
    {
        IContour contour = new Circle(new PointXY(0f, 0f), 1f);
        SpatialRasterGrid grid = CreateGrid();
        var color = new Gray16BitColor(50000);

        SpatialRaster<Gray16BitColor> actual = contour.Rasterize(0.2f, 0.1f, color, grid);
        SpatialRaster<Gray16BitColor> expected = contour.Rasterize(
            0.2f,
            0.1f,
            color,
            default(Gray16BitColor),
            grid);

        Assert.That(actual.Values, Is.EqualTo(expected.Values));
    }

    [Test]
    public void Rasterize_WithRGBA8Color_MatchesTransparentBackgroundCurveRasterization()
    {
        IContour contour = new Circle(new PointXY(0f, 0f), 1f);
        SpatialRasterGrid grid = CreateGrid();

        SpatialRaster<RGBA8BitColor> actual = contour.Rasterize(0.2f, 0.1f, RGBA8BitColor.Red, grid);
        SpatialRaster<RGBA8BitColor> expected = contour.Rasterize(
            0.2f,
            0.1f,
            RGBA8BitColor.Red,
            RGBA8BitColor.Transparent,
            grid);

        Assert.That(actual.Values, Is.EqualTo(expected.Values));
    }

    [Test]
    public void Rasterize_WithRGBA16Color_MatchesTransparentBackgroundCurveRasterization()
    {
        IContour contour = new Circle(new PointXY(0f, 0f), 1f);
        SpatialRasterGrid grid = CreateGrid();

        SpatialRaster<RGBA16BitColor> actual = contour.Rasterize(0.2f, 0.1f, RGBA16BitColor.Red, grid);
        SpatialRaster<RGBA16BitColor> expected = contour.Rasterize(
            0.2f,
            0.1f,
            RGBA16BitColor.Red,
            RGBA16BitColor.Transparent,
            grid);

        Assert.That(actual.Values, Is.EqualTo(expected.Values));
    }

    [Test]
    public void Rasterize_WhenContourIsNull_Throws()
    {
        IContour contour = null!;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            contour.Rasterize(0.2f, 0.1f, RGBA16BitColor.Red, CreateGrid()));

        Assert.That(exception!.ParamName, Is.EqualTo("curve"));
    }

    private static SpatialRasterGrid CreateGrid() => new SpatialRasterGrid(
        origin: new PointXY(-2f, -2f),
        size: new VectorXY(4f, 4f),
        resolution: new VectorXYInt(16, 16));
}
