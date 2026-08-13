using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexVertexTripletRasterRGBA16BitSnapshotTests
{
    [TestCase(Layout.OddR, "index-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-triplet-raster-even-q-rgba16.png")]
    public void IndexTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var hexMapGeometry = CreateHexMapGeometry(layout);
        var grid = new IndexTripletRaster(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), new VectorXYInt(64, 64)));
        var raster = grid.MapValues(ToIndexTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "index-partial-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "index-partial-triplet-raster-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "index-partial-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "index-partial-triplet-raster-even-q-rgba16.png")]
    public void IndexPartialTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var hexMapGeometry = CreateHexMapGeometry(layout);
        var grid = new IndexPartialTripletRaster(
            hexMapGeometry,
            new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), new VectorXYInt(64, 64)));
        var raster = grid.MapValues(ToIndexPartialTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "barycentric-triplet-raster-main-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "barycentric-triplet-raster-main-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "barycentric-triplet-raster-main-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "barycentric-triplet-raster-main-even-q-rgba16.png")]
    public void BarycentricTripletRaster_ToRGBA16BitRaster_WithMainWeight_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var hexGeometry = new HexMapGeometry(5, 4, VectorXY.Zero, 1f, layout);
        var grid = new BarycentricTripletRaster(
            hexGeometry,
            CreateBarycentricSnapshotGeometry(hexGeometry, new VectorXYInt(64, 64)));
        SpatialRaster<RGBA16BitColor> raster = grid.MapValues(ToBarycentricMainSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RasterGeometry CreateBarycentricSnapshotGeometry(
        HexMapGeometry geometry,
        VectorXYInt resolution)
    {
        Rectangle bounds = geometry.GetBoundingBox();
        VectorXY origin = geometry.Topology.Layout switch
        {
            Layout.EvenR when geometry.Topology.Resolution.Y > 1 => new VectorXY(
                geometry.Origin.X - geometry.Apothem - Akeldov.Math.Hexes.Geometry.Constants.Cos30Deg * geometry.Radius,
                bounds.Min.Y),
            Layout.EvenQ when geometry.Topology.Resolution.X > 1 => new VectorXY(
                bounds.Min.X,
                geometry.Origin.Y - geometry.Apothem - Akeldov.Math.Hexes.Geometry.Constants.Sin60Deg * geometry.Radius),
            _ => new VectorXY(bounds.Min.X, bounds.Min.Y),
        };
        VectorXY size = geometry.Topology.Layout switch
        {
            Layout.EvenR when geometry.Topology.Resolution.Y > 1 => new VectorXY(
                2f * geometry.Apothem * (geometry.Topology.Resolution.X - 1) +
                geometry.Apothem +
                2f * Akeldov.Math.Hexes.Geometry.Constants.Cos30Deg * geometry.Radius,
                bounds.Height),
            Layout.EvenQ when geometry.Topology.Resolution.X > 1 => new VectorXY(
                bounds.Width,
                2f * geometry.Apothem * (geometry.Topology.Resolution.Y - 1) +
                geometry.Apothem +
                2f * Akeldov.Math.Hexes.Geometry.Constants.Sin60Deg * geometry.Radius),
            _ => bounds.Size,
        };

        return new RasterGeometry((PointXY)origin, size, resolution);
    }

    [TestCase(Layout.OddR, "barycentric-partial-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "barycentric-partial-triplet-raster-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "barycentric-partial-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "barycentric-partial-triplet-raster-even-q-rgba16.png")]
    public void BarycentricPartialTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var grid = CreateBarycentricPartialTripletRaster(layout, new VectorXYInt(64, 64));
        SpatialRaster<RGBA16BitColor> raster = grid.MapValues(ToBarycentricPartialTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "chromatic-index-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "chromatic-index-triplet-raster-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "chromatic-index-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "chromatic-index-triplet-raster-even-q-rgba16.png")]
    public void ChromaticIndexTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var hexMapGeometry = CreateHexMapGeometry(layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            new VectorXYInt(64, 64));
        var grid = new ChromaticIndexTripletRaster(
            hexMapGeometry,
            rasterGeometry);
        SpatialRaster<RGBA16BitColor> raster = grid.MapValues(ToChromaticIndexTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "chromatic-index-partial-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "chromatic-index-partial-triplet-raster-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "chromatic-index-partial-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "chromatic-index-partial-triplet-raster-even-q-rgba16.png")]
    public void ChromaticIndexPartialTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var hexMapGeometry = CreateHexMapGeometry(layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            new VectorXYInt(64, 64));
        var grid = new ChromaticIndexPartialTripletRaster(
            hexMapGeometry,
            rasterGeometry);
        SpatialRaster<RGBA16BitColor> raster = grid.MapValues(ToChromaticIndexPartialTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "chromatic-barycentric-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "chromatic-barycentric-triplet-raster-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "chromatic-barycentric-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "chromatic-barycentric-triplet-raster-even-q-rgba16.png")]
    public void ChromaticBarycentricTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var hexMapGeometry = CreateHexMapGeometry(layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            new VectorXYInt(64, 64));
        var grid = new ChromaticBarycentricTripletRaster(hexMapGeometry, rasterGeometry);
        SpatialRaster<RGBA16BitColor> raster = grid.MapValues(ToChromaticBarycentricTripletSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    [TestCase(Layout.OddR, "chromatic-barycentric-partial-triplet-raster-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "chromatic-barycentric-partial-triplet-raster-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "chromatic-barycentric-partial-triplet-raster-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "chromatic-barycentric-partial-triplet-raster-even-q-rgba16.png")]
    public void ChromaticBarycentricPartialTripletRaster_ToRGBA16BitRaster_WithLayout_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        var barycentricGrid = CreateBarycentricPartialTripletRaster(layout, new VectorXYInt(64, 64));
        var grid = new ChromaticBarycentricPartialTripletRaster(
            barycentricGrid.SourceHexMapGeometry,
            barycentricGrid.Geometry);
        SpatialRaster<RGBA16BitColor> raster = grid.MapValues(ToChromaticBarycentricPartialBlendSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static BarycentricPartialTripletRaster CreateBarycentricPartialTripletRaster(
        Layout layout,
        VectorXYInt resolution)
    {
        HexMapGeometry hexMapGeometry = CreateHexMapGeometry(layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            resolution);

        return new BarycentricPartialTripletRaster(hexMapGeometry, rasterGeometry);
    }

    private static HexMapGeometry CreateHexMapGeometry(Layout layout)
    {
        var topology = new HexMapTopology(5, 4, layout);
        var defaultHexMapGeometry = new HexMapGeometry(topology, 1f);
        VectorXY hexOrigin = layout switch
        {
            Layout.OddR => new VectorXY(defaultHexMapGeometry.Apothem, defaultHexMapGeometry.Radius),
            Layout.EvenR => new VectorXY(2f * defaultHexMapGeometry.Apothem, defaultHexMapGeometry.Radius),
            Layout.OddQ => new VectorXY(defaultHexMapGeometry.Radius, defaultHexMapGeometry.Apothem),
            Layout.EvenQ => new VectorXY(defaultHexMapGeometry.Radius, 2f * defaultHexMapGeometry.Apothem),
            _ => throw new ArgumentOutOfRangeException(nameof(layout)),
        };
        return new HexMapGeometry(topology, hexOrigin, defaultHexMapGeometry.Radius);
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

    private static RGBA16BitColor ToIndexPartialTripletSnapshotColor(PartialTriplet<VectorXYInt> triplet)
    {
        return new RGBA16BitColor(
            ToPresenceIndexChannel(triplet.Main, triplet.HasMain),
            ToPresenceIndexChannel(triplet.Left, triplet.HasLeft),
            ToPresenceIndexChannel(triplet.Right, triplet.HasRight),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToBarycentricMainSnapshotColor(Triplet<float> barycentricCoordinates)
    {
        ushort main = ToChannel(barycentricCoordinates.Main);
        return new RGBA16BitColor(main, main, main, ushort.MaxValue);
    }

    private static RGBA16BitColor ToBarycentricPartialTripletSnapshotColor(PartialTriplet<float> barycentricCoordinates)
    {
        return new RGBA16BitColor(
            ToPresenceWeightChannel(barycentricCoordinates.Main, barycentricCoordinates.HasMain),
            ToPresenceWeightChannel(barycentricCoordinates.Left, barycentricCoordinates.HasLeft),
            ToPresenceWeightChannel(barycentricCoordinates.Right, barycentricCoordinates.HasRight),
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

    private static RGBA16BitColor ToChromaticIndexPartialTripletSnapshotColor(PartialTriplet<byte> chromaticIndices)
    {
        return new RGBA16BitColor(
            ToPresenceChannel(chromaticIndices.Main, chromaticIndices.HasMain),
            ToPresenceChannel(chromaticIndices.Left, chromaticIndices.HasLeft),
            ToPresenceChannel(chromaticIndices.Right, chromaticIndices.HasRight),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToChromaticBarycentricPartialBlendSnapshotColor(
        PartialChromaticTriplet<float> barycentricCoordinates)
    {
        return new RGBA16BitColor(
            ToChannel(barycentricCoordinates.HasIndex0 ? barycentricCoordinates.Index0 : 0f),
            ToChannel(barycentricCoordinates.HasIndex1 ? barycentricCoordinates.Index1 : 0f),
            ToChannel(barycentricCoordinates.HasIndex2 ? barycentricCoordinates.Index2 : 0f),
            ushort.MaxValue);
    }

    private static RGBA16BitColor ToChromaticBarycentricTripletSnapshotColor(
        ChromaticTriplet<float> barycentricCoordinates)
    {
        return new RGBA16BitColor(
            ToChannel(barycentricCoordinates.Index0),
            ToChannel(barycentricCoordinates.Index1),
            ToChannel(barycentricCoordinates.Index2),
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

    private static ushort ToPresenceWeightChannel(float weight, bool hasValue)
    {
        return hasValue
            ? ToChannel(0.10f + 0.90f * weight)
            : (ushort)0;
    }

    private static ushort ToPresenceIndexChannel(VectorXYInt index, bool hasValue)
    {
        return hasValue
            ? ToChannel(EncodeIndex(index))
            : (ushort)0;
    }

    private static byte[] SaveToPngBytes(Raster<RGBA16BitColor> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static byte[] SaveToPngBytes(SpatialRaster<RGBA16BitColor> raster, string approvedFileName)
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

        if (!PngSnapshotComparer.AreEquivalent(actual, approved))
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

}
