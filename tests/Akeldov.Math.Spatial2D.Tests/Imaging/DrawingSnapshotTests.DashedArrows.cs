using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public partial class DrawingSnapshotTests
{
    [Test]
    public void Drawing_DashedArrows_MatchesApprovedImage()
    {
        var raster = CreateRaster(640, 440);
        raster.DrawText("DrawArrow / dash", new PointXY(24, 400), 21, Ink);
        raster.DrawText("dash = 0", new PointXY(24, 330), 14, Ink);
        raster.DrawArrow(new PointXY(180, 337), new PointXY(595, 337), 3, 30, 28, 0, Blue);
        raster.DrawText("dash = 12", new PointXY(24, 245), 14, Ink);
        raster.DrawArrow(new ParameterizedSegment(new PointXY(180, 252), new PointXY(595, 252)),
            3, 30, 28, 12, Blue);
        raster.DrawText("dash = 24", new PointXY(24, 160), 14, Ink);
        raster.DrawArrow(new ParameterizedSegment(new PointXY(595, 167), new PointXY(180, 167)),
            3, 30, 28, 24, Coral);
        raster.DrawText("alpha / 14", new PointXY(24, 75), 14, Ink);
        raster.DrawArrow(new PointXY(180, 60), new PointXY(595, 105), 6, 35, 30, 14,
            new RGBA8BitColor(37, 99, 235, 128));
        raster.DrawText("Solid heads. Round caps. One composite per pixel.", new PointXY(24, 24), 10.5f, Ink);
        AssertSnapshot(raster, "drawing-dashed-arrows.png");
    }
}
