using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Rasterization;

public class HexMapTopologyLayoutRasterizationSnapshotTests
{
    [TestCase(Layout.OddR, "hex-map-topology-layout-odd-r.png")]
    [TestCase(Layout.EvenR, "hex-map-topology-layout-even-r.png")]
    [TestCase(Layout.OddQ, "hex-map-topology-layout-odd-q.png")]
    [TestCase(Layout.EvenQ, "hex-map-topology-layout-even-q.png")]
    public void Rasterize_WithDocumentationLayoutExample_MatchesApprovedImage(
        Layout layout,
        string approvedFileName)
    {
        string fontPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "arial.ttf");
        if (!File.Exists(fontPath))
        {
            Assert.Ignore("Arial is not available on this machine.");
            return;
        }

        TrueTypeFont font = TrueTypeFont.Load(fontPath);
        SpatialRaster<Gray8BitColor> raster = new HexMapTopology(4, 3, layout)
            .Rasterize(
                100f.ConvertHexApothemToRadius(),
                new HexMapTopologyRasterizationOptions(30f, 1f, 1f, Gray8BitColor.Black, Gray8BitColor.White, 100),
                new HexMapTopologyXYLabelsRasterizationOptions(
                    font, 22f, Gray8BitColor.Black, 0.8f, new VectorXY(0f, 17f)),
                new HexMapTopologyQRSLabelsRasterizationOptions(
                    font, 16f, new Gray8BitColor(80), 0.8f, new VectorXY(0f, -17f)));
        byte[] actual = SaveToPngBytes(raster, approvedFileName);

        AssertMatchesApprovedPng(approvedFileName, actual);
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
            TestContext.AddTestAttachment(actualPath, "Actual hex map topology layout raster snapshot");
            Assert.Fail($"Hex map topology layout approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!PngSnapshotComparer.AreEquivalent(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual hex map topology layout raster snapshot");
            Assert.Fail($"Hex map topology layout raster snapshot changed. Actual image: {actualPath}");
        }
    }

    private static string GetActualPath(string approvedFileName)
    {
        return Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
    }

}
