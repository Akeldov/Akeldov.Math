using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class GeometrySceneSnapshotTests
{
    private const string SmileyApprovedFileName = "geometry-scene-smiley-circle-arc-rgba16.png";
    private const string PrismApprovedFileName = "geometry-scene-triangular-prism-rgba16.png";
    private const string SimpleTextApprovedFileName = "geometry-scene-simple-text-rgba16.png";

    private static readonly RasterGeometry SnapshotGrid = new RasterGeometry(
        origin: new PointXY(0f, 0f),
        size: new VectorXY(100f, 70f),
        resolution: new VectorXYInt(180, 126));

    private static readonly RasterGeometry PrismSnapshotGrid = new RasterGeometry(
        origin: new PointXY(0f, 0f),
        size: new VectorXY(100f, 70f),
        resolution: new VectorXYInt(2160, 1512));

    private static readonly RasterGeometry TextSnapshotGrid = new RasterGeometry(
        origin: new PointXY(0f, 0f),
        size: new VectorXY(8f, 8f),
        resolution: new VectorXYInt(128, 128));

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
        var raster = new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver)
            .AddSignedPointDistanceBasedLayer(face, faceFill, edgeFalloff: 0.55f)
            .AddPointDistanceBasedLayer(face, faceBoundary, fillDistance: 1.45f, edgeFalloff: 0.45f)
            .AddPointDistanceBasedLayer(eyes, eyeColor, fillDistance: 2.4f, edgeFalloff: 0.45f)
            .AddPointDistanceBasedLayer(highlights, highlightColor, fillDistance: 0.65f, edgeFalloff: 0.22f)
            .AddPointDistanceBasedLayer(cheeks, cheekColor, fillDistance: 3.2f, edgeFalloff: 1.1f)
            .AddPointDistanceBasedLayer(smile, smileColor, fillDistance: 2.2f, edgeFalloff: 0.55f)
            .Rasterize(SnapshotGrid);

        raster.SaveAsPng(actualPath);
        byte[] actual = File.ReadAllBytes(actualPath);

        AssertMatchesApprovedPng(SmileyApprovedFileName, actual);
    }

    [Test]
    public void Rasterize_WithTriangularPrism_MatchesApprovedImage()
    {
        // Geometry
        float prismSide = 36f;
        float prismHeight = prismSide * MathF.Sqrt(3f) / 2f;
        float prismBaseY = 18f;
        float prismCenterX = 52f;
        var prismLeft = new PointXY(prismCenterX - prismSide / 2f, prismBaseY);
        var prismTop = new PointXY(prismCenterX, prismBaseY + prismHeight);
        var prismRight = new PointXY(prismCenterX + prismSide / 2f, prismBaseY);
        var prism = new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(prismLeft, prismTop),
            new ParameterizedSegment(prismTop, prismRight),
            new ParameterizedSegment(prismRight, prismLeft)
        });
        float beamY = prismBaseY + 16f;
        float beamEdgeOffsetX = (beamY - prismBaseY) / MathF.Sqrt(3f);
        float beamOuterY = beamY - 8f;
        var beamStart = new PointXY(PrismSnapshotGrid.Origin.X, beamOuterY);
        var beamLeftFaceBend = new PointXY(prismLeft.X + beamEdgeOffsetX, beamY);
        var beamRightFaceBend = new PointXY(prismRight.X - beamEdgeOffsetX, beamY);
        var beamRightFaceBendDelta = new VectorXY(1f, 0f);
        var beamEdgeEnd = new PointXY(PrismSnapshotGrid.Origin.X + PrismSnapshotGrid.Size.X, beamOuterY);
        var beamEnd = beamRightFaceBend + (beamEdgeEnd - beamRightFaceBend) * 1.55f;
        var incomingBeam = new ParameterizedSegment(beamStart, beamLeftFaceBend).ExtendEnd(2f);
        var prismBeam = new ParameterizedSegment(beamLeftFaceBend, beamRightFaceBend + beamRightFaceBendDelta).Extend(2f);
        var outgoingBeam = new ParameterizedSegment(beamRightFaceBend, beamEnd).ExtendStart(2f);

        // Colors
        RGBA16BitColor background = RGBA16BitColor.FromNormalized(0.018f, 0.020f, 0.030f, 1f);
        RGBA16BitColor prismFill = RGBA16BitColor.FromNormalized(0.500f, 0.640f, 0.790f, 1f);
        RGBA16BitColor beamColor = RGBA16BitColor.FromNormalized(0.900f, 0.960f, 1.000f, 0.74f);
        RGBA16BitColor[] rainbowStops =
        {
            RGBA16BitColor.FromNormalized(1.000f, 0.090f, 0.080f, 0.78f),
            RGBA16BitColor.FromNormalized(1.000f, 0.520f, 0.050f, 0.78f),
            RGBA16BitColor.FromNormalized(1.000f, 0.930f, 0.120f, 0.78f),
            RGBA16BitColor.FromNormalized(0.160f, 0.860f, 0.280f, 0.78f),
            RGBA16BitColor.FromNormalized(0.100f, 0.560f, 1.000f, 0.78f),
            RGBA16BitColor.FromNormalized(0.520f, 0.220f, 1.000f, 0.78f)
        };
        float beamEdgeFalloff = 0.48f;
        float prismBeamInitialHalfWidth = 0.07f;
        float prismBeamEndHalfWidth = 2.2f;
        float rainbowInitialHalfWidth = 0.8f;

        Func<float, RGBA16BitColor> prismInteriorFalloff = d =>
        {
            return d <= 0
                ? prismFill.ScaleAlpha(1f / (1f + -d * 2))
                : prismFill.ScaleAlpha(1f / (1f + d * 5));
        };

        Func<PointXY, ParameterizedCurveProjection, RGBA16BitColor> incomingBeamColor = (point, p) =>
        {
            if (prism.Encloses(point))
                return RGBA16BitColor.Transparent;

            if (p.Distance <= prismBeamInitialHalfWidth)
                return beamColor;

            float edgeDistance = p.Distance - prismBeamInitialHalfWidth;
            return beamColor.ScaleAlpha(1f - MathF.Min(edgeDistance, beamEdgeFalloff) / beamEdgeFalloff);
        };

        Func<PointXY, ParameterizedCurveProjection, RGBA16BitColor> prismBeamColor = (point, p) =>
        {
            if (!prism.Encloses(point))
                return RGBA16BitColor.Transparent;

            var normalizedCurveCoordinate = p.CurveCoordinate / prismBeam.Length - 0.09f;

            float beamHalfWidth = prismBeamInitialHalfWidth + normalizedCurveCoordinate * (prismBeamEndHalfWidth - prismBeamInitialHalfWidth);
            float distanceCoverage = p.Distance <= beamHalfWidth
                ? 1f
                : 1f - MathF.Min(p.Distance - beamHalfWidth, beamEdgeFalloff) / beamEdgeFalloff;
            float initialCoverageIntegral = 2f * prismBeamInitialHalfWidth + beamEdgeFalloff;
            float coverageIntegral = 2f * beamHalfWidth + beamEdgeFalloff;
            float widthCoverage = initialCoverageIntegral / coverageIntegral;

            return beamColor.ScaleAlpha(widthCoverage * distanceCoverage);
        };

        Func <PointXY, ParameterizedCurveProjection, RGBA16BitColor> projectionToRainbowColor = (point, p) =>
        {
            if (prism.Encloses(point))
                return RGBA16BitColor.Transparent;

            float beamHalfWidth = rainbowInitialHalfWidth + (p.CurveCoordinate + 3f) / 9f;
            float signedDistance = outgoingBeam.GetHalfPlaneSide(point) switch
            {
                HalfPlaneSide.Left => p.Distance,
                HalfPlaneSide.Right => -p.Distance,
                _ => 0f
            };
            float spectralPosition = MathF.Min(MathF.Max((beamHalfWidth - signedDistance) / (2f * beamHalfWidth), 0f), 1f);
            float scaledPosition = spectralPosition * (rainbowStops.Length - 1);
            int stopIndex = (int)MathF.Floor(scaledPosition);
            RGBA16BitColor rainbowColor = stopIndex >= rainbowStops.Length - 1
                ? rainbowStops[rainbowStops.Length - 1]
                : RGBA16BitColor.Blend(
                    rainbowStops[stopIndex],
                    rainbowStops[stopIndex + 1],
                    scaledPosition - stopIndex);

            if (p.Distance <= beamHalfWidth)
                return rainbowColor;

            float edgeDistance = p.Distance - beamHalfWidth;
            return rainbowColor.ScaleAlpha(1f - MathF.Min(edgeDistance, beamEdgeFalloff) / beamEdgeFalloff);
        };

        // Scene
        string actualPath = GetActualPath(PrismApprovedFileName);
        var raster = new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver)            
            .AddParameterizedProjectionBasedLayer(incomingBeam, incomingBeamColor)
            .AddParameterizedProjectionBasedLayer(prismBeam, prismBeamColor)
            .AddParameterizedProjectionBasedLayer(outgoingBeam, projectionToRainbowColor)
            .AddSignedPointDistanceBasedLayer(prism, prismInteriorFalloff)
            .Rasterize(PrismSnapshotGrid);

        raster.SaveAsPng(actualPath);
        byte[] actual = File.ReadAllBytes(actualPath);

        AssertMatchesApprovedPng(PrismApprovedFileName, actual);
    }

    [Test]
    public void Rasterize_WithSimpleTrueTypeText_MatchesApprovedImage()
    {
        string? fontPath = GetSystemTimesNewRomanPath();
        if (fontPath is null)
        {
            Assert.Ignore("Times New Roman is not available on this machine.");
            return;
        }

        TrueTypeFont font = TrueTypeFont.Load(fontPath);

        RGBA16BitColor background = RGBA16BitColor.FromNormalized(0.965f, 0.972f, 0.982f, 1f);
        RGBA16BitColor textColor = RGBA16BitColor.FromNormalized(0.055f, 0.085f, 0.165f, 0.95f);

        string actualPath = GetActualPath(SimpleTextApprovedFileName);
        var raster = new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver)
            .AddTextLayer(
                font,
                "A",
                origin: new PointXY(1f, 1f),
                fontSize: 6f,
                color: textColor,
                edgeFalloff: 0.08f)
            .Rasterize(TextSnapshotGrid);

        raster.SaveAsPng(actualPath);
        byte[] actual = File.ReadAllBytes(actualPath);

        AssertMatchesApprovedPng(SimpleTextApprovedFileName, actual);
    }

    private static string? GetSystemTimesNewRomanPath()
    {
        string fontPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "times.ttf");

        return File.Exists(fontPath) ? fontPath : null;
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
