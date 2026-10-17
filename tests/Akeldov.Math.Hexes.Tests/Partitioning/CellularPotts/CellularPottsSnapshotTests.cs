using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

public class CellularPottsSnapshotTests
{
    [Test]
    public void Evolution_WithConnectivity_MatchesApprovedImage() =>
        AssertSnapshot(CellularPottsConnectivity.Preserve, "cellular-potts-preserve-rgba16.png");

    [Test]
    public void Evolution_WithoutConnectivity_MatchesApprovedImage() =>
        AssertSnapshot(CellularPottsConnectivity.None, "cellular-potts-none-rgba16.png");

    private static void AssertSnapshot(CellularPottsConnectivity connectivity, string fileName)
    {
        const int seed = 42;
        var topology = new HexMapTopology(30, 24, Layout.OddR);
        var source = CellularPottsTests.Partition(topology,
            p => p.X / 10 + 3 * (p.Y / 8), Enumerable.Range(0, 9).ToArray());
        var parameters = new CellularPottsParameters(100, temperature: 1.5, boundaryEnergy: 2, areaPenalty: 1, connectivity: connectivity);
        var result = source.ApplyCellularPotts(parameters, random: new Random(seed));
        string context = $"seed={seed}, connectivity={connectivity}";
        var participation = new BoolHexMap(topology, Enumerable.Repeat(true, topology.Count).ToArray());
        var regions = new IntHexMap(topology);
        var partialSource = new PartialHexPartitionMap<HexPartitionCell>(topology, source.Cells);
        foreach (var masked in PartialCellularPottsTests.Variants(source, partialSource, participation, regions, parameters, true))
            Assert.That(PartialCellularPottsTests.Values(masked), Is.EqualTo(CellularPottsTests.Values(result).Select(id => (int?)id)), context);
        Assert.That(PartialCellularPottsTests.Values(partialSource.ApplyCellularPotts(parameters, new Random(seed))),
            Is.EqualTo(CellularPottsTests.Values(result).Select(id => (int?)id)), context);
        if (connectivity == CellularPottsConnectivity.Preserve)
        {
            CellularPottsTests.AssertConsistentAndConnected(result, context);
        }
        else
        {
            // At low temperature, an unrestricted run need not produce fragments.
            var rebuilt = new HexPartitionMap<HexPartitionCell>(topology, result.Cells);
            Assert.That(CellularPottsTests.Values(result), Is.EqualTo(CellularPottsTests.Values(rebuilt)), context);
        }

        Raster<RGBA16BitColor> raster = result.Rasterize(topology.Resolution * 24, ToSnapshotColor);
        string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName.Replace(".png", ".actual.png"));
        raster.SaveAsPng(actualPath);
        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "Partitioning", "CellularPotts", "Approved", fileName);
        if (!File.Exists(approvedPath) ||
            !PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actualPath), File.ReadAllBytes(approvedPath)))
        {
            TestContext.AddTestAttachment(actualPath, $"Actual Cellular Potts snapshot: {context}");
            Assert.Fail($"Cellular Potts snapshot changed or is missing ({context}). Actual image: {actualPath}");
        }
    }

    private static RGBA16BitColor ToSnapshotColor(int cellId) => cellId switch
    {
        0 => new RGBA16BitColor(0xe800, 0x4800, 0x5000, ushort.MaxValue),
        1 => new RGBA16BitColor(0x3800, 0xb800, 0x7000, ushort.MaxValue),
        2 => new RGBA16BitColor(0x4000, 0x7000, 0xe800, ushort.MaxValue),
        3 => new RGBA16BitColor(0xe000, 0xb800, 0x3800, ushort.MaxValue),
        4 => new RGBA16BitColor(0xa000, 0x6000, 0xd800, ushort.MaxValue),
        5 => new RGBA16BitColor(0x3000, 0xb800, 0xc800, ushort.MaxValue),
        6 => new RGBA16BitColor(0xf000, 0x8800, 0x3800, ushort.MaxValue),
        7 => new RGBA16BitColor(0xd800, 0x7000, 0xa800, ushort.MaxValue),
        8 => new RGBA16BitColor(0x9800, 0xb800, 0x4800, ushort.MaxValue),
        _ => throw new InvalidOperationException($"Unexpected Cellular Potts cell identifier: {cellId}.")
    };
}
