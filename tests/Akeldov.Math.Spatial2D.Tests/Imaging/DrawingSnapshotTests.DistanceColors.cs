using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public partial class DrawingSnapshotTests
{
    [Test]
    public void Drawing_SignedParameterizedDistanceColor_MatchesApprovedImage()
    {
        var raster = CreateRaster(400, 300);
        raster.DrawText("Signed distance + parameter", new PointXY(12, 275), 10.5f, Ink);
        raster.DrawText("Left: blue / right: coral", new PointXY(12, 255), 10.5f, Ink);
        var curve = new ParameterizedSegmentChain(
            new PointXY(55, 45), new PointXY(345, 225),
            new PointXY(55, 225), new PointXY(345, 45));
        raster.Draw(curve, (distance, parameter) =>
        {
            float magnitude = MathF.Abs(distance);
            if (magnitude >= 24)
                return default;
            var color = distance < 0 ? Blue : Coral;
            // Bands follow accumulated length, while color distinguishes the two local sides.
            float band = parameter % 40 < 28 ? 1f : 0.35f;
            return new RGBA8BitColor(color.R, color.G, color.B,
                (byte)MathF.Round(220 * band * (1 - magnitude / 24)));
        });
        raster.Draw((ICurve)curve, 1, Ink);
        AssertSnapshot(raster, "drawing-signed-parameterized-distance-color.png");
    }

    [Test]
    public void Drawing_UnsignedDistanceColor_MatchesApprovedImage()
    {
        var raster = CreateRaster(320, 220);
        raster.DrawText("Unsigned distance / rings", new PointXY(12, 195), 10.5f, Ink);
        IPointDistanceProvider figure = new Disk(new PointXY(115.25f, 100.5f), 47.5f);
        raster.Draw(figure, distance =>
        {
            if (distance >= 42)
                return default;
            var color = (int)(distance / 10) % 2 == 0 ? Blue : Coral;
            return new RGBA8BitColor(color.R, color.G, color.B,
                (byte)MathF.Round(220 * (1 - distance / 42)));
        });
        // A second translucent field exercises compositing and clipping at the right edge.
        IPointDistanceProvider segment = new Segment(new PointXY(190, 35), new PointXY(330, 150));
        raster.Draw(segment, distance => new RGBA8BitColor(20, 150, 100,
            (byte)MathF.Round(160 * MathF.Max(0, 1 - distance / 25))));
        AssertSnapshot(raster, "drawing-unsigned-distance-color.png");
    }

    [Test]
    public void Drawing_SignedDistanceColor_MatchesApprovedImage()
    {
        var raster = CreateRaster(320, 220);
        raster.DrawText("Signed distance / inside + outside", new PointXY(12, 195), 10.5f, Ink);
        ISignedPointDistanceProvider figure = new ContourBasedRegion(new IContour[]
        {
            new RectangleContour(new PointXY(35.25f, 25.5f), new PointXY(290.5f, 165.25f)),
            new Circle(new PointXY(105.5f, 95.25f), 38.5f),
            new Circle(new PointXY(230.25f, 95.5f), 23.25f),
        });
        raster.Draw(figure, distance => distance <= 0
            ? new RGBA8BitColor(37, 99, 235, (byte)MathF.Round(110 + 130 * MathF.Min(1, -distance / 25)))
            : new RGBA8BitColor(239, 90, 70, (byte)MathF.Round(200 * MathF.Max(0, 1 - distance / 18))));
        AssertSnapshot(raster, "drawing-signed-distance-color.png");
    }

    [Test]
    public void Drawing_ParameterizedDistanceColor_MatchesApprovedImage()
    {
        var raster = CreateRaster(320, 220);
        raster.DrawText("Distance + curve coordinate", new PointXY(12, 195), 10.5f, Ink);
        var curve = new ParameterizedSegmentChain(
            new PointXY(25.25f, 35.5f), new PointXY(90.5f, 155.25f),
            new PointXY(185.25f, 45.5f), new PointXY(300.5f, 155.25f));
        raster.Draw((IParameterizedCurve)curve, (distance, parameter) =>
        {
            if (distance >= 15 || parameter % 36 >= 27)
                return default;
            float t = parameter / curve.Length;
            return new RGBA8BitColor((byte)MathF.Round(37 + 202 * t),
                (byte)MathF.Round(99 - 9 * t), (byte)MathF.Round(235 - 165 * t),
                (byte)MathF.Round(230 * MathF.Min(1, (15 - distance) / 7)));
        });
        AssertSnapshot(raster, "drawing-parameterized-distance-color.png");
    }
}
