using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public partial class DrawingSnapshotTests
{
    [TestCase(240, 180, "drawing-region-default-square-pixels.png")]
    [TestCase(120, 180, "drawing-region-default-wide-pixels.png")]
    [TestCase(240, 90, "drawing-region-default-tall-pixels.png")]
    public void Drawing_RegionDefaultFading_MatchesApprovedImage(int width, int height, string snapshot)
    {
        var values = new RGBA8BitColor[width * height];
        Array.Fill(values, new RGBA8BitColor(247, 249, 252, 255));
        // Keep world geometry fixed so only the pixel dimensions and default fading change.
        var raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(new PointXY(-20, -10), new VectorXY(240, 180), new VectorXYInt(width, height)), values);

        IRegion disk = new Disk(new PointXY(35.25f, 115.5f), 30.25f);
        IRegion rectangle = new OrientedRectangle(new PointXY(150, 115), new VectorXY(85, 45), MathF.PI / 6f);
        raster.Draw(disk, Blue);
        raster.Draw(rectangle, Coral);

        var region = new ContourBasedRegion(new IContour[]
        {
            new RectangleContour(new PointXY(-8.5f, 0.25f), new PointXY(230, 65.5f)),
            new Circle(new PointXY(35, 32), 20),
            new RectangleContour(new PointXY(80.25f, 15.5f), new PointXY(120.75f, 50.25f)),
        });
        raster.Draw(region, new RGBA8BitColor(239, 90, 70, 128));
        // Separate translucent regions overlap using source-over compositing.
        raster.Draw(new Disk(new PointXY(175, 60), 25.5f), Blue);

        AssertSnapshot(raster, snapshot);
    }

    [Test]
    public void Drawing_RegionFading_MatchesApprovedImage()
    {
        var raster = CreateRaster(480, 360);
        raster.DrawText("Draw / IRegion", new PointXY(20, 325), 21, Ink);
        raster.DrawText("Disk / rotated rectangle", new PointXY(20, 295), 10.5f, Ink);

        IRegion disk = new Disk(new PointXY(110.25f, 220.5f), 50.25f);
        IRegion rectangle = new OrientedRectangle(new PointXY(340, 220), new VectorXY(125, 60), MathF.PI / 6f);
        raster.Draw(disk, 10f, Blue);
        raster.Draw(rectangle, 16f, Coral);

        raster.DrawText("Holes / fading / clipping", new PointXY(20, 145), 10.5f, Ink);
        raster.FillRectangle(new PointXY(20, 15), new PointXY(460, 125), new RGBA8BitColor(210, 225, 240, 255));
        var region = new ContourBasedRegion(new IContour[]
        {
            new RectangleContour(new PointXY(35, 25), new PointXY(495, 115)),
            new Circle(new PointXY(105, 70), 26),
            new Circle(new PointXY(245, 70), 12),
            new Circle(new PointXY(245, 70), 33),
            new RectangleContour(new PointXY(340, 45), new PointXY(415, 95)),
        });
        // Interior stays solid while the exterior and holes fade to transparency.
        raster.Draw(region, 12.5f, new RGBA8BitColor(239, 90, 70, 128));

        AssertSnapshot(raster, "drawing-region-fading.png");
    }
}
