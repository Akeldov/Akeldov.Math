using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexVertexTripletGridRGBA16BitRasterSnapshotTests
{
    [TestCase(Layout.OddR, "hex-vertex-index-triplet-grid-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-index-triplet-grid-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-index-triplet-grid-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-index-triplet-grid-even-q-rgba16.png")]
    public void IndexTripletGrid_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexIndexTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(ToIndexTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-barycentric-triplet-grid-main-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-barycentric-triplet-grid-main-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-barycentric-triplet-grid-main-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-barycentric-triplet-grid-main-even-q-rgba16.png")]
    public void BarycentricTripletGrid_ToRGBA16BitRaster_WithMainWeight_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexBarycentricTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(ToBarycentricMainSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-chromatic-index-triplet-grid-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-chromatic-index-triplet-grid-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-chromatic-index-triplet-grid-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-chromatic-index-triplet-grid-even-q-rgba16.png")]
    public void ChromaticIndexTripletGrid_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexChromaticIndexTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(ToChromaticIndexTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-chromatic-index-partial-triplet-grid-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-chromatic-index-partial-triplet-grid-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-chromatic-index-partial-triplet-grid-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-chromatic-index-partial-triplet-grid-even-q-rgba16.png")]
    public void ChromaticIndexPartialTripletGrid_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var map = new IndexedHexAdjacencyMap(width: 5, height: 4, layout: layout);
        var grid = new HexVertexChromaticIndexPartialTripletGrid(
            map,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(ToChromaticIndexPartialTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RGBA16BitColor ToIndexTripletSnapshotColor(Triplet<VectorXYInt> triplet)
    {
        float main = EncodeIndex(triplet.Main);
        float left = EncodeIndex(triplet.Left);
        float right = EncodeIndex(triplet.Right);

        return new RGBA16BitColor(
            ToChannel(main),
            ToChannel(left),
            ToChannel(right),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToBarycentricMainSnapshotColor(Triplet<float> barycentricCoordinates)
    {
        ushort main = ToChannel(barycentricCoordinates.Main);
        return new RGBA16BitColor(main, main, main, ushort.MaxValue);
    }

    private static RGBA16BitColor ToChromaticIndexTripletSnapshotColor(Triplet<byte> chromaticIndices)
    {
        return new RGBA16BitColor(
            ToChannel(0.18f + 0.34f * chromaticIndices.Main),
            ToChannel(0.18f + 0.34f * chromaticIndices.Left),
            ToChannel(0.18f + 0.34f * chromaticIndices.Right),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToChromaticIndexPartialTripletSnapshotColor(PartialTriplet<byte> chromaticIndices)
    {
        return new RGBA16BitColor(
            ToPresenceChannel(chromaticIndices.Main, chromaticIndices.HasMain),
            ToPresenceChannel(chromaticIndices.Left, chromaticIndices.HasLeft),
            ToPresenceChannel(chromaticIndices.Right, chromaticIndices.HasRight),
            ushort.MaxValue);
    }

    private static float EncodeIndex(VectorXYInt index)
    {
        return 0.08f + 0.075f * (index.X + 1) + 0.12f * (index.Y + 1);
    }

    private static ushort ToChannel(float value)
    {
        value = MathF.Min(MathF.Max(value, 0f), 1f);
        return (ushort)MathF.Round(value * ushort.MaxValue);
    }

    private static ushort ToPresenceChannel(byte chromaticIndex, bool hasValue)
    {
        return hasValue
            ? ToChannel(0.25f + 0.30f * chromaticIndex)
            : (ushort)0;
    }

    private static byte[] SaveToPngBytes(RGBA16BitRaster raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static void AssertMatchesApprovedPng(string approvedFileName, byte[] actual)
    {
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Topology",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual hex vertex triplet grid raster snapshot");
            Assert.Fail($"Hex vertex triplet grid approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual hex vertex triplet grid raster snapshot");
            Assert.Fail($"Hex vertex triplet grid raster snapshot changed. Actual image: {actualPath}");
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
