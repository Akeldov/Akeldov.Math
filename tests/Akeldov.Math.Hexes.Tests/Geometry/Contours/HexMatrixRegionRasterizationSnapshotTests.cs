using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Hexes.Tests.Geometry.Contours;

public class HexMatrixRegionRasterizationSnapshotTests
{
    private static readonly VectorXYInt SnapshotResolution = new VectorXYInt(160, 128);

    [TestCase(Layout.OddR, "polyhex-contour-odd-r.png")]
    [TestCase(Layout.EvenR, "polyhex-contour-even-r.png")]
    [TestCase(Layout.OddQ, "polyhex-contour-odd-q.png")]
    [TestCase(Layout.EvenQ, "polyhex-contour-even-q.png")]
    public void ToRegion_RasterizedSegments_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        ContourBasedRegion region = CreateSnapshotGeometry().ToRegion(layout);
        Raster<byte> raster = Rasterize(region);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "polyhex-apothem-offset-contour-odd-r.png")]
    [TestCase(Layout.EvenR, "polyhex-apothem-offset-contour-even-r.png")]
    [TestCase(Layout.OddQ, "polyhex-apothem-offset-contour-odd-q.png")]
    [TestCase(Layout.EvenQ, "polyhex-apothem-offset-contour-even-q.png")]
    public void ToApothemOffsetRegion_RasterizedSegments_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        PolyhexGeometry geometry = CreateSnapshotGeometry();
        IFinitePath[] contour = geometry
            .ToRegion(layout)
            .Contours
            .SelectMany(GetCurves)
            .Concat(geometry.ToApothemOffsetRegion(layout).Contours.SelectMany(GetCurves))
            .ToArray();
        Raster<byte> raster = Rasterize(contour);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "polyhex-contour-with-hole-odd-r.png")]
    [TestCase(Layout.EvenR, "polyhex-contour-with-hole-even-r.png")]
    [TestCase(Layout.OddQ, "polyhex-contour-with-hole-odd-q.png")]
    [TestCase(Layout.EvenQ, "polyhex-contour-with-hole-even-q.png")]
    public void ToRegion_WithHole_RasterizedSegments_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        ContourBasedRegion region = CreateHollowSnapshotGeometry().ToRegion(layout);

        Assert.That(region.Contours, Has.Count.GreaterThan(1));

        Raster<byte> raster = Rasterize(region);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "polyhex-apothem-offset-contour-with-hole-odd-r.png")]
    [TestCase(Layout.EvenR, "polyhex-apothem-offset-contour-with-hole-even-r.png")]
    [TestCase(Layout.OddQ, "polyhex-apothem-offset-contour-with-hole-odd-q.png")]
    [TestCase(Layout.EvenQ, "polyhex-apothem-offset-contour-with-hole-even-q.png")]
    public void ToApothemOffsetRegion_WithHole_RasterizedSegments_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        PolyhexGeometry geometry = CreateHollowSnapshotGeometry();
        ContourBasedRegion offsetRegion = geometry.ToApothemOffsetRegion(layout);

        Assert.That(offsetRegion.Contours, Has.Count.GreaterThan(1));

        IFinitePath[] contour = geometry
            .ToRegion(layout)
            .Contours
            .SelectMany(GetCurves)
            .Concat(offsetRegion.Contours.SelectMany(GetCurves))
            .ToArray();
        Raster<byte> raster = Rasterize(contour);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static PolyhexGeometry CreateSnapshotGeometry()
    {
        return new PolyhexGeometry(
            new bool[,]
            {
                { false, true,  true,  false },
                { true,  true,  true,  false },
                { true,  false, true,  true  },
                { false, true,  true,  true  },
                { false, false, true,  false }
            },
            apothem: 1f);
    }

    private static PolyhexGeometry CreateHollowSnapshotGeometry()
    {
        return new PolyhexGeometry(
            new bool[,]
            {
                { true, true,  true,  true,  true },
                { true, true,  false, false, true },
                { true, false, false, false, true },
                { true, false, false, true,  true },
                { true, true,  true,  true,  true }
            },
            apothem: 1f);
    }

    private static Raster<byte> Rasterize(ContourBasedRegion region)
    {
        return Rasterize(region.Contours.SelectMany(GetCurves).ToArray());
    }

    private static Raster<byte> Rasterize(IReadOnlyList<IFinitePath> contour)
    {
        IPointDistanceProvider[] distanceProviders = contour
            .Select(curve => (IPointDistanceProvider)curve)
            .ToArray();

        return distanceProviders.Rasterize(
            CreateGrid(contour),
            new PointDistanceProviderCollectionGray8BitRasterizer(ToDistanceGray8));
    }

    private static RasterGrid CreateGrid(IReadOnlyList<IFinitePath> contour)
    {
        const float padding = 0.75f;

        IFinitePath first = contour[0];
        float minX = MathF.Min(first.StartPoint.X, first.EndPoint.X);
        float minY = MathF.Min(first.StartPoint.Y, first.EndPoint.Y);
        float maxX = MathF.Max(first.StartPoint.X, first.EndPoint.X);
        float maxY = MathF.Max(first.StartPoint.Y, first.EndPoint.Y);
        IncludeCurveBounds(first, ref minX, ref minY, ref maxX, ref maxY);

        for (int i = 1; i < contour.Count; i++)
        {
            IFinitePath curve = contour[i];
            minX = MathF.Min(minX, MathF.Min(curve.StartPoint.X, curve.EndPoint.X));
            minY = MathF.Min(minY, MathF.Min(curve.StartPoint.Y, curve.EndPoint.Y));
            maxX = MathF.Max(maxX, MathF.Max(curve.StartPoint.X, curve.EndPoint.X));
            maxY = MathF.Max(maxY, MathF.Max(curve.StartPoint.Y, curve.EndPoint.Y));
            IncludeCurveBounds(curve, ref minX, ref minY, ref maxX, ref maxY);
        }

        return new RasterGrid(
            origin: new PointXY(minX - padding, minY - padding),
            size: new VectorXY(maxX - minX + 2f * padding, maxY - minY + 2f * padding),
            resolution: SnapshotResolution);
    }

    private static IReadOnlyList<IFinitePath> GetCurves(IContour contour)
    {
        if (contour is ICompositeContour compositeContour)
            return compositeContour.Curves;

        throw new InvalidOperationException("Polyhex contour snapshot requires composite contours.");
    }

    private static void IncludeCurveBounds(
        IFinitePath curve,
        ref float minX,
        ref float minY,
        ref float maxX,
        ref float maxY)
    {
        if (curve is not ParameterizedArc arc)
            return;

        minX = MathF.Min(minX, arc.Center.X - arc.Radius);
        minY = MathF.Min(minY, arc.Center.Y - arc.Radius);
        maxX = MathF.Max(maxX, arc.Center.X + arc.Radius);
        maxY = MathF.Max(maxY, arc.Center.Y + arc.Radius);
    }

    private static byte ToDistanceGray8(float distance)
    {
        const float falloffDistance = 0.08f;
        float normalized = 1f - System.Math.Clamp(distance / falloffDistance, 0f, 1f);
        return (byte)MathF.Round(normalized * byte.MaxValue);
    }

    private static byte[] SaveToPngBytes(Raster<byte> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static void AssertMatchesApprovedPng(string approvedFileName, byte[] actual)
    {
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Geometry",
            "Contours",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual polyhex contour raster snapshot");
            Assert.Fail($"Polyhex contour approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual polyhex contour raster snapshot");
            Assert.Fail($"Polyhex contour raster snapshot changed. Actual image: {actualPath}");
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
