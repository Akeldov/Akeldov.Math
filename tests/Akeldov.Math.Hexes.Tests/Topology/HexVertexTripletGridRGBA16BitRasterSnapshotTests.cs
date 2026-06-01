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
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            ToIndexTripletSnapshotColor,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-index-triplet-grid-fill-empty-cells-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-index-triplet-grid-fill-empty-cells-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-index-triplet-grid-fill-empty-cells-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-index-triplet-grid-fill-empty-cells-even-q-rgba16.png")]
    public void IndexTripletGrid_ToRGBA16BitRaster_WithFillEmptyCells_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexIndexTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64),
            fillMode: HexVertexTripletGridFillMode.FillEmptyCells);
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            ToIndexTripletSnapshotColor,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-barycentric-grid-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-barycentric-grid-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-barycentric-grid-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-barycentric-grid-even-q-rgba16.png")]
    public void BarycentricGrid_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexBarycentricGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            ToBarycentricSnapshotColor,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-barycentric-grid-fill-empty-cells-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-barycentric-grid-fill-empty-cells-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-barycentric-grid-fill-empty-cells-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-barycentric-grid-fill-empty-cells-even-q-rgba16.png")]
    public void BarycentricGrid_ToRGBA16BitRaster_WithFillEmptyCells_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexBarycentricGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64),
            fillMode: HexVertexTripletGridFillMode.FillEmptyCells);
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            ToBarycentricSnapshotColor,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
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
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            ToChromaticIndexTripletSnapshotColor,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-chromatic-index-triplet-grid-fill-empty-cells-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-chromatic-index-triplet-grid-fill-empty-cells-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-chromatic-index-triplet-grid-fill-empty-cells-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-chromatic-index-triplet-grid-fill-empty-cells-even-q-rgba16.png")]
    public void ChromaticIndexTripletGrid_ToRGBA16BitRaster_WithFillEmptyCells_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = new HexVertexChromaticIndexTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64),
            fillMode: HexVertexTripletGridFillMode.FillEmptyCells);
        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            ToChromaticIndexTripletSnapshotColor,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-chromatic-barycentric-blend-grid-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-chromatic-barycentric-blend-grid-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-chromatic-barycentric-blend-grid-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-chromatic-barycentric-blend-grid-even-q-rgba16.png")]
    public void ChromaticIndexTripletAndBarycentricGrid_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var chromaticGrid = new HexVertexChromaticIndexTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64));
        var barycentricGrid = new HexVertexBarycentricGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64));
        RGBA16BitRaster raster = ToChromaticBarycentricBlendRaster(
            chromaticGrid,
            barycentricGrid,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "hex-vertex-chromatic-barycentric-blend-grid-fill-empty-cells-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "hex-vertex-chromatic-barycentric-blend-grid-fill-empty-cells-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "hex-vertex-chromatic-barycentric-blend-grid-fill-empty-cells-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "hex-vertex-chromatic-barycentric-blend-grid-fill-empty-cells-even-q-rgba16.png")]
    public void ChromaticIndexTripletAndBarycentricGrid_ToRGBA16BitRaster_WithFillEmptyCells_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var chromaticGrid = new HexVertexChromaticIndexTripletGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64),
            fillMode: HexVertexTripletGridFillMode.FillEmptyCells);
        var barycentricGrid = new HexVertexBarycentricGrid(
            hexWidth: 5,
            hexHeight: 4,
            layout: layout,
            hexOrigin: VectorXY.Zero,
            hexApothem: 8f,
            resolution: new VectorXYInt(64, 64),
            fillMode: HexVertexTripletGridFillMode.FillEmptyCells);
        RGBA16BitRaster raster = ToChromaticBarycentricBlendRaster(
            chromaticGrid,
            barycentricGrid,
            new RGBA16BitColor(0x1010, 0x1010, 0x1010, ushort.MaxValue));
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

    private static RGBA16BitColor ToBarycentricSnapshotColor(Triplet<float> barycentricCoordinates)
    {
        return new RGBA16BitColor(
            ToChannel(barycentricCoordinates.Main),
            ToChannel(barycentricCoordinates.Left),
            ToChannel(barycentricCoordinates.Right),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToChromaticIndexTripletSnapshotColor(Triplet<byte> chromaticIndices)
    {
        return new RGBA16BitColor(
            ToChannel(0.18f + 0.34f * chromaticIndices.Main),
            ToChannel(0.18f + 0.34f * chromaticIndices.Left),
            ToChannel(0.18f + 0.34f * chromaticIndices.Right),
            ushort.MaxValue);
    }

    private static RGBA16BitRaster ToChromaticBarycentricBlendRaster(
        HexVertexChromaticIndexTripletGrid chromaticGrid,
        HexVertexBarycentricGrid barycentricGrid,
        RGBA16BitColor emptyColor)
    {
        var values = new RGBA16BitColor[chromaticGrid.Count];
        bool[] chromaticHasHex = chromaticGrid.HasHex;
        bool[] barycentricHasHex = barycentricGrid.HasHex;
        Triplet<byte>[] chromaticIndices = chromaticGrid.ChromaticIndices;
        Triplet<float>[] barycentricCoordinates = barycentricGrid.BarycentricCoordinates;

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = chromaticHasHex[i] && barycentricHasHex[i]
                ? ToChromaticBarycentricBlendSnapshotColor(chromaticIndices[i], barycentricCoordinates[i])
                : emptyColor;
        }

        return new RGBA16BitRaster(
            new RasterGrid((PointXY)chromaticGrid.Origin, chromaticGrid.Size, chromaticGrid.Resolution),
            values);
    }

    private static RGBA16BitColor ToChromaticBarycentricBlendSnapshotColor(
        Triplet<byte> chromaticIndices,
        Triplet<float> barycentricCoordinates)
    {
        float red = GetChromaticChannel(chromaticIndices.Main, 0) * barycentricCoordinates.Main +
            GetChromaticChannel(chromaticIndices.Left, 0) * barycentricCoordinates.Left +
            GetChromaticChannel(chromaticIndices.Right, 0) * barycentricCoordinates.Right;
        float green = GetChromaticChannel(chromaticIndices.Main, 1) * barycentricCoordinates.Main +
            GetChromaticChannel(chromaticIndices.Left, 1) * barycentricCoordinates.Left +
            GetChromaticChannel(chromaticIndices.Right, 1) * barycentricCoordinates.Right;
        float blue = GetChromaticChannel(chromaticIndices.Main, 2) * barycentricCoordinates.Main +
            GetChromaticChannel(chromaticIndices.Left, 2) * barycentricCoordinates.Left +
            GetChromaticChannel(chromaticIndices.Right, 2) * barycentricCoordinates.Right;

        return new RGBA16BitColor(
            ToChannel(red),
            ToChannel(green),
            ToChannel(blue),
            ushort.MaxValue);
    }

    private static float GetChromaticChannel(byte chromaticIndex, byte channelIndex)
    {
        return chromaticIndex == channelIndex ? 1f : 0f;
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
