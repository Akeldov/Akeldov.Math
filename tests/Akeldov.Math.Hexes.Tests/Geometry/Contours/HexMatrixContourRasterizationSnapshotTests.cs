using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Geometry.Contours;

public class HexMatrixContourRasterizationSnapshotTests
{
    private static readonly VectorXYInt SnapshotResolution = new VectorXYInt(160, 128);

    [TestCase(Layout.OddR, "polyhex-contour-odd-r.png")]
    [TestCase(Layout.EvenR, "polyhex-contour-even-r.png")]
    [TestCase(Layout.OddQ, "polyhex-contour-odd-q.png")]
    [TestCase(Layout.EvenQ, "polyhex-contour-even-q.png")]
    public void ToContour_RasterizedSegments_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        Segment[] contour = CreateSnapshotGeometry().ToContour(layout);
        Raster<byte> raster = Rasterize(contour);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "polyhex-apothem-offset-contour-odd-r.png")]
    [TestCase(Layout.EvenR, "polyhex-apothem-offset-contour-even-r.png")]
    [TestCase(Layout.OddQ, "polyhex-apothem-offset-contour-odd-q.png")]
    [TestCase(Layout.EvenQ, "polyhex-apothem-offset-contour-even-q.png")]
    public void ToApothemOffsetContour_RasterizedSegments_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        Segment[] contour = CreateSnapshotGeometry().ToApothemOffsetContour(layout);
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

    private static Raster<byte> Rasterize(Segment[] contour)
    {
        IPointDistanceProvider[] distanceProviders = contour
            .Select(segment => (IPointDistanceProvider)segment)
            .ToArray();

        return distanceProviders.Rasterize(
            CreateGrid(contour),
            new PointDistanceProviderCollectionGray8BitRasterizer(ToDistanceGray8));
    }

    private static RasterGrid CreateGrid(Segment[] contour)
    {
        const float padding = 0.75f;

        Segment first = contour[0];
        float minX = MathF.Min(first.EndpointA.X, first.EndpointB.X);
        float minY = MathF.Min(first.EndpointA.Y, first.EndpointB.Y);
        float maxX = MathF.Max(first.EndpointA.X, first.EndpointB.X);
        float maxY = MathF.Max(first.EndpointA.Y, first.EndpointB.Y);

        for (int i = 1; i < contour.Length; i++)
        {
            Segment segment = contour[i];
            minX = MathF.Min(minX, MathF.Min(segment.EndpointA.X, segment.EndpointB.X));
            minY = MathF.Min(minY, MathF.Min(segment.EndpointA.Y, segment.EndpointB.Y));
            maxX = MathF.Max(maxX, MathF.Max(segment.EndpointA.X, segment.EndpointB.X));
            maxY = MathF.Max(maxY, MathF.Max(segment.EndpointA.Y, segment.EndpointB.Y));
        }

        return new RasterGrid(
            origin: new PointXY(minX - padding, minY - padding),
            size: new VectorXY(maxX - minX + 2f * padding, maxY - minY + 2f * padding),
            resolution: SnapshotResolution);
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
