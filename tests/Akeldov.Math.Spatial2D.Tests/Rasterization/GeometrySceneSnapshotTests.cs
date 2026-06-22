using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class GeometrySceneSnapshotTests
{
    private const string SmileyApprovedFileName = "geometry-scene-smiley-circle-arc-rgba16.png";

    private static readonly RasterGrid SnapshotGrid = new RasterGrid(
        origin: new PointXY(0f, 0f),
        size: new VectorXY(100f, 70f),
        resolution: new VectorXYInt(180, 126));

    [Test]
    public void Rasterize_WithPointEyesArcSmileAndFilledCircle_MatchesApprovedImage()
    {
        // Geometry
        var face = new Circle(new PointXY(50f, 35f), 24f);
        var smile = new Arc(
            new PointXY(50f, 36f),
            radius: 15f,
            startAngle: 7f * MathF.PI / 6f,
            endAngle: 11f * MathF.PI / 6f);
        var eyes = new[]
        {
            new PointXY(40f, 43f),
            new PointXY(60f, 43f)
        };
        var highlights = new[]
        {
            new PointXY(39f, 43.9f),
            new PointXY(59f, 43.9f)
        };
        var cheeks = new[]
        {
            new PointXY(34f, 33f),
            new PointXY(66f, 33f)
        };

        // Colors
        RGBA16BitColor background = RGBA16BitColor.FromNormalized(0.965f, 0.972f, 0.982f, 1f);
        RGBA16BitColor faceFill = RGBA16BitColor.FromNormalized(1.000f, 0.810f, 0.145f, 0.66f);
        RGBA16BitColor faceBoundary = RGBA16BitColor.FromNormalized(0.055f, 0.085f, 0.165f, 0.95f);
        RGBA16BitColor smileColor = RGBA16BitColor.FromNormalized(0.055f, 0.085f, 0.165f, 0.95f);
        RGBA16BitColor eyeColor = RGBA16BitColor.FromNormalized(0.050f, 0.060f, 0.080f, 1f);
        RGBA16BitColor cheekColor = RGBA16BitColor.FromNormalized(0.940f, 0.260f, 0.310f, 0.55f);
        RGBA16BitColor highlightColor = RGBA16BitColor.FromNormalized(1f, 1f, 1f, 0.75f);

        // Scene
        string actualPath = GetActualPath(SmileyApprovedFileName);
        RGBA16BitRaster raster = GeometryScenes.CreateRGBA16Bit(background)
            .Fill(face, faceFill, edgeFalloff: 0.55f)
            .Stroke(face, faceBoundary, width: 1.45f, edgeFalloff: 0.45f)
            .Point(eyes, eyeColor, radius: 2.4f, edgeFalloff: 0.45f)
            .Point(highlights, highlightColor, radius: 0.65f, edgeFalloff: 0.22f)
            .Point(cheeks, cheekColor, radius: 3.2f, edgeFalloff: 1.1f)
            .Stroke(smile, smileColor, width: 2.2f, edgeFalloff: 0.55f)
            .Rasterize(SnapshotGrid);

        raster.SaveAsPng(actualPath);
        byte[] actual = File.ReadAllBytes(actualPath);

        AssertMatchesApprovedPng(SmileyApprovedFileName, actual);
    }

    private static void AssertMatchesApprovedPng(string approvedFileName, byte[] actual)
    {
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Rasterization",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual geometry scene snapshot");
            Assert.Fail($"Geometry scene approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual geometry scene snapshot");
            Assert.Fail($"Geometry scene snapshot changed. Actual image: {actualPath}");
        }
    }

    private static string GetActualPath(string approvedFileName)
    {
        return Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
    }

    private static bool BytesEqual(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
                return false;
        }

        return true;
    }
}
