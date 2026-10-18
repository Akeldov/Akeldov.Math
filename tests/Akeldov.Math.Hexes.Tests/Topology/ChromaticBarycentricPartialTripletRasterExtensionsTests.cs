using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class ChromaticBarycentricPartialTripletRasterExtensionsTests
{
    private static readonly RGBA8BitColor[] ClassColors8 =
    {
        new(byte.MaxValue, 0, 0, byte.MaxValue),
        new(0, byte.MaxValue, 0, byte.MaxValue),
        new(0, 0, byte.MaxValue, byte.MaxValue),
    };

    private static readonly RGBA16BitColor[] ClassColors16 =
    {
        new(ushort.MaxValue, 0, 0, ushort.MaxValue),
        new(0, ushort.MaxValue, 0, ushort.MaxValue),
        new(0, 0, ushort.MaxValue, ushort.MaxValue),
    };

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToExtrapolatedRasters_WhenWeightsArePresent_UseNormalizedPresentWeights(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(5, 4, 1f, layout);
        var raster = new ChromaticBarycentricPartialTripletRaster(hexMapGeometry, hexMapGeometry.ToRasterGeometry(8f));
        SpatialRaster<RGBA8BitColor> image8 = raster.ToExtrapolatedRGBA8BitRaster();
        SpatialRaster<RGBA16BitColor> image16 = raster.ToExtrapolatedRGBA16BitRaster();
        bool foundPartialValue = false;

        for (int y = 0; y < raster.Resolution.Y; y++)
        {
            for (int x = 0; x < raster.Resolution.X; x++)
            {
                PartialChromaticTriplet<float> weights = raster[x, y];
                float w0 = weights.HasIndex0 ? weights.Index0 : 0f;
                float w1 = weights.HasIndex1 ? weights.Index1 : 0f;
                float w2 = weights.HasIndex2 ? weights.Index2 : 0f;
                float sum = w0 + w1 + w2;
                if (!(sum > 0f))
                    continue;

                foundPartialValue |= weights.Presence != ChromaticTripletPresence.All;
                RGBA8BitColor actual8 = image8[x, y];
                RGBA16BitColor actual16 = image16[x, y];

                Assert.Multiple(() =>
                {
                    Assert.That(actual8, Is.EqualTo(RGBA8BitColor.FromNormalized(w0 / sum, w1 / sum, w2 / sum)), $"{layout}, pixel ({x}, {y}), 8-bit");
                    Assert.That(actual16, Is.EqualTo(RGBA16BitColor.FromNormalized(w0 / sum, w1 / sum, w2 / sum)), $"{layout}, pixel ({x}, {y}), 16-bit");
                });
            }
        }

        Assert.That(foundPartialValue, Is.True, $"{layout} did not exercise a partial triplet.");
    }

    // Each case samples a bounding-box corner notch of a 5x4 map, next to the corner hex in the comment.
    [TestCase(Layout.OddR, true, false, 1)] // Hex (4, 0).
    [TestCase(Layout.OddR, false, true, 2)] // Hex (0, 3).
    [TestCase(Layout.EvenR, false, false, 0)] // Hex (0, 0).
    [TestCase(Layout.EvenR, true, true, 2)] // Hex (4, 3).
    [TestCase(Layout.OddQ, false, true, 0)] // Hex (0, 3).
    [TestCase(Layout.OddQ, true, true, 0)] // Hex (4, 3).
    [TestCase(Layout.EvenQ, false, false, 0)] // Hex (0, 0).
    [TestCase(Layout.EvenQ, true, false, 0)] // Hex (4, 0).
    public void ToExtrapolatedRasters_InBoundingBoxCornerNotches_UseNearestMapHexClassColor(
        Layout layout,
        bool isRightCorner,
        bool isTopCorner,
        int expectedClass)
    {
        var hexMapGeometry = new HexMapGeometry(5, 4, 1f, layout);
        var raster = new ChromaticBarycentricPartialTripletRaster(hexMapGeometry, hexMapGeometry.ToRasterGeometry(8f));
        int x = isRightCorner ? raster.Resolution.X - 1 : 0;
        int y = isTopCorner ? raster.Resolution.Y - 1 : 0;

        Assert.Multiple(() =>
        {
            Assert.That(raster[x, y].Presence, Is.EqualTo(ChromaticTripletPresence.None), "The sample must have no in-map hexes.");
            Assert.That(raster.ToExtrapolatedRGBA8BitRaster()[x, y], Is.EqualTo(ClassColors8[expectedClass]));
            Assert.That(raster.ToExtrapolatedRGBA16BitRaster()[x, y], Is.EqualTo(ClassColors16[expectedClass]));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToExtrapolatedRasters_InMargin_UseClassColorOfNearestMapHexCenter(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(5, 4, 1f, layout);
        var raster = new ChromaticBarycentricPartialTripletRaster(
            hexMapGeometry,
            hexMapGeometry.ToRasterGeometry(4f, margin: 3f));
        SpatialRaster<RGBA8BitColor> image8 = raster.ToExtrapolatedRGBA8BitRaster();
        SpatialRaster<RGBA16BitColor> image16 = raster.ToExtrapolatedRGBA16BitRaster();
        (VectorXY Center, int ChromaticClass)[] mapHexes = GetMapHexes(hexMapGeometry);
        int checkedSamples = 0;

        for (int y = 0; y < raster.Resolution.Y; y++)
        {
            for (int x = 0; x < raster.Resolution.X; x++)
            {
                if (raster[x, y].Presence != ChromaticTripletPresence.None)
                    continue;

                PointXY point = raster.Geometry.GetCellCenter(x, y);
                int[] nearestClasses = GetNearestClasses(point, mapHexes);
                int actualClass = Array.IndexOf(ClassColors8, image8[x, y]);
                checkedSamples++;

                Assert.Multiple(() =>
                {
                    Assert.That(nearestClasses, Does.Contain(actualClass), $"{layout}, pixel ({x}, {y}), point {point}");
                    Assert.That(image16[x, y], Is.EqualTo(ClassColors16[System.Math.Max(actualClass, 0)]), $"{layout}, pixel ({x}, {y}), 16-bit");
                });
            }
        }

        Assert.That(checkedSamples, Is.GreaterThan(0), $"{layout} did not exercise samples without in-map hexes.");
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToExtrapolatedRasters_WithMargin_HaveNoTransparentPixelsAndKeepGeometry(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(5, 4, 1f, layout);
        var raster = new ChromaticBarycentricPartialTripletRaster(
            hexMapGeometry,
            hexMapGeometry.ToRasterGeometry(4f, margin: 3f));
        SpatialRaster<RGBA8BitColor> image8 = raster.ToExtrapolatedRGBA8BitRaster();
        SpatialRaster<RGBA16BitColor> image16 = raster.ToExtrapolatedRGBA16BitRaster();

        Assert.Multiple(() =>
        {
            Assert.That(image8.Geometry, Is.EqualTo(raster.Geometry));
            Assert.That(image16.Geometry, Is.EqualTo(raster.Geometry));
        });

        for (int y = 0; y < raster.Resolution.Y; y++)
        {
            for (int x = 0; x < raster.Resolution.X; x++)
            {
                Assert.That(image8[x, y].A, Is.EqualTo(byte.MaxValue), $"{layout}, pixel ({x}, {y}), 8-bit");
                Assert.That(image16[x, y].A, Is.EqualTo(ushort.MaxValue), $"{layout}, pixel ({x}, {y}), 16-bit");
            }
        }
    }

    [Test]
    public void ToExtrapolatedRGBA8BitRaster_WithTutorialGeometry_MatchesApprovedImage()
    {
        var hexMapGeometry = new HexMapGeometry(width: 5, height: 4, origin: VectorXY.Zero, radius: 1f, layout: Layout.OddR);
        var raster = new ChromaticBarycentricPartialTripletRaster(hexMapGeometry, hexMapGeometry.ToRasterGeometry(pixelsPerApothem: 32f));
        const string approvedFileName = "chromatic-barycentric-extrapolated-rgba8.png";
        string actualPath = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Topology",
            "Approved",
            approvedFileName);

        raster.ToExtrapolatedRGBA8BitRaster().SaveAsPng(actualPath);

        if (!File.Exists(approvedPath))
            Assert.Fail($"Approved image is missing. Actual image: {actualPath}");

        Assert.That(
            PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)),
            Is.True,
            $"Extrapolated raster snapshot changed. Actual image: {actualPath}");
    }

    [Test]
    public void ToExtrapolatedRasters_WhenRasterIsNull_Throw()
    {
        ChromaticBarycentricPartialTripletRaster raster = null!;

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentNullException>(() => raster.ToExtrapolatedRGBA8BitRaster());
            Assert.Throws<ArgumentNullException>(() => raster.ToExtrapolatedRGBA16BitRaster());
        });
    }

    private static (VectorXY Center, int ChromaticClass)[] GetMapHexes(HexMapGeometry hexMapGeometry)
    {
        Layout layout = hexMapGeometry.Topology.Layout;
        var mapHexes = new List<(VectorXY Center, int ChromaticClass)>();
        for (int y = 0; y < hexMapGeometry.Topology.Resolution.Y; y++)
        {
            for (int x = 0; x < hexMapGeometry.Topology.Resolution.X; x++)
            {
                var index = new VectorXYInt(x, y);
                mapHexes.Add((
                    index.GetHexCenter(hexMapGeometry.Radius, hexMapGeometry.Origin, layout),
                    index.GetChromaticClass(layout)));
            }
        }

        return mapHexes.ToArray();
    }

    // Brute force over all map hexes; near-equal distances accept every tied class.
    private static int[] GetNearestClasses(PointXY point, (VectorXY Center, int ChromaticClass)[] mapHexes)
    {
        float[] squaredDistances = mapHexes
            .Select(hex => (hex.Center.X - point.X) * (hex.Center.X - point.X) + (hex.Center.Y - point.Y) * (hex.Center.Y - point.Y))
            .ToArray();
        float minSquaredDistance = squaredDistances.Min();

        return mapHexes
            .Where((_, index) => squaredDistances[index] <= minSquaredDistance + 1e-3f)
            .Select(hex => hex.ChromaticClass)
            .Distinct()
            .ToArray();
    }
}
