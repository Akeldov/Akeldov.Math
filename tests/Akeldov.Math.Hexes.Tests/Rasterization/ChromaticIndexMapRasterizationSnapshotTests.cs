using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Rasterization;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class ChromaticIndexMapRasterizationSnapshotTests
{
    [TestCase(Layout.OddR, "chromatic-index-map-odd-r-rgba16.png")]
    [TestCase(Layout.EvenR, "chromatic-index-map-even-r-rgba16.png")]
    [TestCase(Layout.OddQ, "chromatic-index-map-odd-q-rgba16.png")]
    [TestCase(Layout.EvenQ, "chromatic-index-map-even-q-rgba16.png")]
    public void Rasterize_WithLayout_MatchesApprovedImage(Layout layout, string approvedFileName)
    {
        var chromatization = new ChromaticIndexMap(
            width: 5,
            height: 4,
            layout: layout);
        var rasterizer = new HexFieldChromatizationRGBA16BitRasterizer(
            origin: new VectorXY(0f, 0f),
            apothem: 8f,
            chromaticIndexToColor: ToSnapshotColor);
        SpatialRasterGrid grid = rasterizer.CreateGrid(chromatization, pixelsPerApothem: 8f);
        SpatialRaster<RGBA16BitColor> raster = rasterizer.Rasterize(chromatization, grid);
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
    }

    private static RGBA16BitColor ToSnapshotColor(byte chromaticIndex)
    {
        switch (chromaticIndex)
        {
            case 0:
                return new RGBA16BitColor(0xefff, 0x4750, 0x4750, ushort.MaxValue);
            case 1:
                return new RGBA16BitColor(0x3b60, 0xc990, 0x72a0, ushort.MaxValue);
            case 2:
                return new RGBA16BitColor(0x4760, 0x77a0, 0xe8ff, ushort.MaxValue);
            default:
                return new RGBA16BitColor(0x2020, 0x2020, 0x2020, ushort.MaxValue);
        }
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
            "Rasterization",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual chromatic index map raster snapshot");
            Assert.Fail($"Chromatic index map approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual chromatic index map raster snapshot");
            Assert.Fail($"Chromatic index map raster snapshot changed. Actual image: {actualPath}");
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
