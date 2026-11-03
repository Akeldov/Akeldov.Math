using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public partial class DrawingSnapshotTests
{
    [Test]
    public void Drawing_Grid_MatchesApprovedImage()
    {
        var values = new RGBA8BitColor[840 * 500];
        Array.Fill(values, new RGBA8BitColor(247, 249, 252, 255));
        var raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(new PointXY(-420, -250), new VectorXY(840, 500), new VectorXYInt(840, 500)), values);
        raster.DrawGrid(20, 0.75f, new RGBA8BitColor(80, 100, 130, 18));
        raster.DrawText("DrawGrid / world coordinates", new PointXY(-396, 210), 21, Ink);
        raster.DrawText("Square / step 30", new PointXY(-390, 175), 10.5f, Ink);
        raster.DrawText("Step 20 x 25 / major every 4", new PointXY(30, 175), 10.5f, Ink);
        var left = new Rectangle(new PointXY(-390, -190), new PointXY(-30, 160));
        var right = new Rectangle(new PointXY(30, -190), new PointXY(390, 160));
        raster.Fill(left, RGBA8BitColor.White);
        raster.Fill(right, RGBA8BitColor.White);
        raster.DrawGrid(new VectorXY(30, 30), 1.25f, new RGBA8BitColor(37, 99, 235, 120), bounds: left);
        raster.DrawGrid(new VectorXY(20, 25), 1.25f, new RGBA8BitColor(30, 41, 59, 35),
            majorEvery: 4, majorColor: new RGBA8BitColor(37, 99, 235, 180), labelHeight: 10.5f, labelColor: Ink, bounds: right);
        raster.DrawText("Clipped bounds. World-zero alignment. Built-in pixel font.",
            new PointXY(-390, -225), 10.5f, Ink);
        AssertSnapshot(raster, "drawing-grid.png");
    }
}
