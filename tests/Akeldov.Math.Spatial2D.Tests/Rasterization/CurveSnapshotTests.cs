using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class CurveSnapshotTests
{
    private static readonly RasterGeometry SnapshotGrid = new RasterGeometry(
        origin: new PointXY(-3f, -3f),
        size: new VectorXY(6f, 6f),
        resolution: new VectorXYInt(96, 96));

    [TestCaseSource(nameof(CurveCases))]
    public void Rasterize_WhenSingleCurveIsRendered_MatchesApprovedImage(
        string approvedFileName,
        Func<ICurve> createCurve)
    {
        SpatialRaster<Gray8BitColor> raster = createCurve().Rasterize(ToDistanceGray8, SnapshotGrid);

        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCaseSource(nameof(ParameterizedThicknessCurveCases))]
    public void Rasterize_WhenParameterizedCurveThicknessUsesCurveCoordinate_MatchesApprovedImage(
        string approvedFileName,
        Func<IParameterizedCurve> createCurve)
    {
        SpatialRaster<Gray8BitColor> raster = createCurve().Rasterize(ToGrowingThicknessGray8, SnapshotGrid);

        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static IEnumerable<TestCaseData> CurveCases()
    {
        yield return new TestCaseData(
            "line-distance.png",
            Curve(() => new Line(new PointXY(-2.5f, -1.5f), new PointXY(2.5f, 1.75f))))
            .SetName("Line_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-line-distance.png",
            Curve(() => new ParameterizedLine(new PointXY(0.9f, -2.5f), new VectorXY(-0.25f, 1f))))
            .SetName("ParameterizedLine_MatchesApprovedImage");

        yield return new TestCaseData(
            "ray-distance.png",
            Curve(() => new Ray(new PointXY(-2.2f, -1.6f), MathF.PI / 5f)))
            .SetName("Ray_MatchesApprovedImage");

        yield return new TestCaseData(
            "segment-distance.png",
            Curve(() => new Segment(new PointXY(-2.2f, 1.6f), new PointXY(2.2f, -1.3f))))
            .SetName("Segment_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-segment-distance.png",
            Curve(() => new ParameterizedSegment(new PointXY(-1.7f, -2.2f), new PointXY(1.8f, 1.8f))))
            .SetName("ParameterizedSegment_MatchesApprovedImage");

        yield return new TestCaseData(
            "circle-distance.png",
            Curve(() => new Circle(new PointXY(0.1f, -0.15f), 1.75f)))
            .SetName("Circle_MatchesApprovedImage");

        yield return new TestCaseData(
            "rectangle-contour-distance.png",
            Curve(() => new RectangleContour(new PointXY(-2f, -1.4f), new PointXY(2f, 1.4f))))
            .SetName("RectangleContour_MatchesApprovedImage");

        yield return new TestCaseData(
            "oriented-rectangle-contour-distance.png",
            Curve(() => new OrientedRectangleContour(new PointXY(0f, 0f), new VectorXY(4f, 2.2f), MathF.PI / 6f)))
            .SetName("OrientedRectangleContour_MatchesApprovedImage");

        yield return new TestCaseData(
            "composite-contour-distance.png",
            Curve(() => CreateCompositeContour()))
            .SetName("CompositeContour_MatchesApprovedImage");

        yield return new TestCaseData(
            "arc-distance.png",
            Curve(() => new Arc(new PointXY(-0.2f, -0.25f), 2f, MathF.PI / 8f, 5f * MathF.PI / 4f)))
            .SetName("Arc_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-arc-distance.png",
            Curve(() => new ParameterizedArc(
                new PointXY(0.15f, 0.1f),
                2.1f,
                MathF.PI / 6f,
                3f * MathF.PI / 2f,
                AngularDirection.Clockwise)))
            .SetName("ParameterizedArc_MatchesApprovedImage");

        yield return new TestCaseData(
            "quadratic-bezier-distance.png",
            Curve(() => new QuadraticBezier(
                new PointXY(-2.25f, -1.85f),
                new PointXY(-0.25f, 2.45f),
                new PointXY(2.25f, -1.35f))))
            .SetName("QuadraticBezier_MatchesApprovedImage");

        yield return new TestCaseData(
            "cubic-bezier-distance.png",
            Curve(() => new CubicBezier(
                new PointXY(-2.5f, -2.1f),
                new PointXY(-2.15f, 2.3f),
                new PointXY(2.35f, 2.1f),
                new PointXY(2.5f, -1.8f))))
            .SetName("CubicBezier_MatchesApprovedImage");

        yield return new TestCaseData(
            "bezier-curve-distance.png",
            Curve(() => new BezierCurve(
                new PointXY(-2.35f, -1.9f),
                new PointXY(-1.4f, 2.25f),
                new PointXY(0.15f, -2.3f),
                new PointXY(1.5f, 2.15f),
                new PointXY(2.35f, -1.65f))))
            .SetName("BezierCurve_MatchesApprovedImage");
    }

    private static IEnumerable<TestCaseData> ParameterizedThicknessCurveCases()
    {
        yield return new TestCaseData(
            "parameterized-line-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedLine(new PointXY(-0.4f, -2.65f), new VectorXY(0.45f, 1f))))
            .SetName("ParameterizedLine_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-arc-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedArc(
                new PointXY(0f, 0f),
                2f,
                -MathF.PI / 4f,
                5f * MathF.PI / 4f,
                AngularDirection.Counterclockwise)))
            .SetName("ParameterizedArc_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-circle-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedCircle(new PointXY(0f, 0f), 1.8f, -MathF.PI / 2f)))
            .SetName("ParameterizedCircle_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-rectangle-contour-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedRectangleContour(new PointXY(-2f, -1.4f), new PointXY(2f, 1.4f))))
            .SetName("ParameterizedRectangleContour_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-oriented-rectangle-contour-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedOrientedRectangleContour(new PointXY(0f, 0f), new VectorXY(4f, 2.2f), MathF.PI / 6f)))
            .SetName("ParameterizedOrientedRectangleContour_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-composite-contour-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedCompositeContour(CreateCompositePaths())))
            .SetName("ParameterizedCompositeContour_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "ray-growing-thickness.png",
            ParameterizedCurve(() => new Ray(new PointXY(-2.45f, -2.05f), MathF.PI / 5f)))
            .SetName("Ray_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "parameterized-segment-growing-thickness.png",
            ParameterizedCurve(() => new ParameterizedSegment(new PointXY(-2.35f, -2.1f), new PointXY(2.35f, 1.75f))))
            .SetName("ParameterizedSegment_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "quadratic-bezier-growing-thickness.png",
            ParameterizedCurve(() => new QuadraticBezier(
                new PointXY(-2.25f, -1.85f),
                new PointXY(-0.25f, 2.45f),
                new PointXY(2.25f, -1.35f))))
            .SetName("QuadraticBezier_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "cubic-bezier-growing-thickness.png",
            ParameterizedCurve(() => new CubicBezier(
                new PointXY(-2.5f, -2.1f),
                new PointXY(-2.15f, 2.3f),
                new PointXY(2.35f, 2.1f),
                new PointXY(2.5f, -1.8f))))
            .SetName("CubicBezier_GrowingThickness_MatchesApprovedImage");

        yield return new TestCaseData(
            "bezier-curve-growing-thickness.png",
            ParameterizedCurve(() => new BezierCurve(
                new PointXY(-2.35f, -1.9f),
                new PointXY(-1.4f, 2.25f),
                new PointXY(0.15f, -2.3f),
                new PointXY(1.5f, 2.15f),
                new PointXY(2.35f, -1.65f))))
            .SetName("BezierCurve_GrowingThickness_MatchesApprovedImage");
    }

    private static Func<ICurve> Curve(Func<ICurve> createCurve)
    {
        return createCurve;
    }

    private static CompositeContour CreateCompositeContour() => new CompositeContour(CreateCompositePaths());

    private static IFinitePath[] CreateCompositePaths() =>
        new IFinitePath[]
    {
        new ParameterizedSegment(new PointXY(-2f, -1.5f), new PointXY(1f, -1.7320508f)),
        new ParameterizedArc(
            new PointXY(0f, 0f),
            2f,
            -MathF.PI / 3f,
            2f * MathF.PI / 3f,
            AngularDirection.Counterclockwise),
        new ParameterizedSegment(new PointXY(-1f, 1.7320508f), new PointXY(-2f, -1.5f))
    };

    private static Func<IParameterizedCurve> ParameterizedCurve(Func<IParameterizedCurve> createCurve)
    {
        return createCurve;
    }

    private static Gray8BitColor ToDistanceGray8(float distance)
    {
        const float falloffDistance = 0.25f;
        float normalized = 1f - System.Math.Clamp(distance / falloffDistance, 0f, 1f);
        return new Gray8BitColor((byte)MathF.Round(normalized * byte.MaxValue));
    }

    private static Gray8BitColor ToGrowingThicknessGray8(float distance, float curveCoordinate)
    {
        const float baseThickness = 0.05f;
        const float thicknessPerWorldUnit = 0.065f;
        const float maxThicknessGrowth = 0.42f;
        const float edgeFalloff = 0.08f;

        float nonNegativeCoordinate = MathF.Max(0f, curveCoordinate);
        float thickness = baseThickness + MathF.Min(nonNegativeCoordinate * thicknessPerWorldUnit, maxThicknessGrowth);
        float normalized = 1f - System.Math.Clamp((distance - thickness) / edgeFalloff, 0f, 1f);

        return new Gray8BitColor((byte)MathF.Round(normalized * byte.MaxValue));
    }

    private static byte[] SaveToPngBytes(SpatialRaster<Gray8BitColor> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
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
            TestContext.AddTestAttachment(actualPath, "Actual curve distance snapshot");
            Assert.Fail($"Curve approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual curve distance snapshot");
            Assert.Fail($"Curve snapshot changed. Actual image: {actualPath}");
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
