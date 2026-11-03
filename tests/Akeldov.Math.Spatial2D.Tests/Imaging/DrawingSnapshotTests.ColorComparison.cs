using Akeldov.Math.Spatial2D.Imaging;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public partial class DrawingSnapshotTests
{
    [Test]
    public void Drawing_DarkenLightenComparison_MatchesApprovedImage()
    {
        var raster = CreateRaster(1000, 710);
        raster.DrawText("Darken / Lighten", new PointXY(24, 665), 21, Ink);
        raster.DrawText("Darken 100% -> 0%    |    Original    |    Lighten 0% -> 100%",
            new PointXY(180, 630), 10.5f, Ink);
        var colors = new[]
        {
            new RGBA8BitColor(72, 126, 232, 255),
            new RGBA8BitColor(222, 72, 60, 255),
            new RGBA8BitColor(40, 170, 80, 255),
            new RGBA8BitColor(240, 190, 30, 255),
            new RGBA8BitColor(128, 128, 128, 255)
        };
        string[] modes = { "RGB", "Linear RGB", "OKLCH" };
        for (int group = 0; group < colors.Length; group++)
            for (int mode = 0; mode < 3; mode++)
            {
                float y = 580 - group * 112 - mode * 29;
                raster.DrawText(modes[mode], new PointXY(24, y + 7), 10.5f, Ink);
                for (int step = -5; step <= 5; step++)
                {
                    float amount = MathF.Abs(step) / 5f;
                    var adjustmentMode = (ColorAdjustmentMode)mode;
                    var color = step > 0
                        ? colors[group].Lighten(amount, adjustmentMode)
                        : colors[group].Darken(amount, adjustmentMode);
                    Assert.That(color.A, Is.EqualTo(colors[group].A));
                    if (step == 0)
                        Assert.That(color, Is.EqualTo(colors[group]));
                    if (System.Math.Abs(step) == 5)
                        Assert.That(color.R, Is.EqualTo(step < 0 ? 0 : 255));
                    float x = 180 + (step + 5) * 72;
                    raster.FillRectangle(new PointXY(x, y), new PointXY(x + 68, y + 24), color);
                }
            }
        raster.DrawText("Steps: 20%. OKLCH: fixed hue, chroma reduced only to fit sRGB.",
            new PointXY(24, 30), 10.5f, Ink);
        AssertSnapshot(raster, "drawing-darken-lighten-comparison.png");
    }

}
