using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

[TestFixture(0)]
[TestFixture(1)]
[TestFixture(2)]
[TestFixture(3)]
public class SpatialPartialCellularPottsTests
{
    private readonly int _overload;

    public SpatialPartialCellularPottsTests(int overload) => _overload = overload;

    [TestCase(42, 0)]
    [TestCase(42, 8)]
    public void Diffuse_PreservesGeometryAndMatchesBaseEvolution(int seed, int steps)
    {
        foreach (var layout in new[] { Layout.OddR, Layout.EvenR, Layout.OddQ, Layout.EvenQ })
        foreach (var connectivity in new[] { CellularPottsConnectivity.Preserve, CellularPottsConnectivity.None })
        {
            string context = $"seed={seed}, steps={steps}, layout={layout}, connectivity={connectivity}, overload={_overload}";
            var geometry = new HexMapGeometry(8, 6, new VectorXY(-12f, 23f), 2.5f, layout);
            var full = CellularPottsTests.Partition(geometry.Topology, p => p.X < 4 ? 7 : 42, 7, 42, 99);
            var source = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry, full.Cells.Select(c => new HexPartitionCell(c.Id, c.HexIndexes.Where(p => p.Y > 0).ToArray())).ToArray());
            var original = Enumerable.Range(0, geometry.Topology.Count).Select(i => source[i]).ToArray();
            var regions = new HexMap<int>(geometry.Topology,
                Enumerable.Range(0, geometry.Topology.Count).Select(i => i / 8 < 3 ? -7 : 0).ToArray());
            var targets = new Dictionary<int, float> { [7] = 14f, [42] = 30f };
            var parameters = new CellularPottsParameters(steps, 5, 2, 0.5f, connectivity);
            var expected = ApplyBase(source, regions, targets, parameters, new Random(seed));

            SpatialPartialHexPartitionMap<HexPartitionCell> result = Apply(source, regions, targets, parameters, new Random(seed));

            Assert.That(result.Geometry, Is.EqualTo(geometry), context);
            Assert.That(result.Topology, Is.EqualTo(geometry.Topology), context);
            Assert.That(Enumerable.Range(0, geometry.Topology.Count).Select(i => result[i]),
                Is.EqualTo(Enumerable.Range(0, geometry.Topology.Count).Select(i => expected[i])), context);
            Assert.That(result.Cells.Select(c => c.Id), Is.EqualTo(new[] { 7, 42, 99 }), context);
            for (int i = 0; i < result.Cells.Count; i++)
            {
                Assert.That(result.Cells[i], Is.Not.SameAs(source.Cells[i]), context);
                Assert.That(result.Cells[i].HexIndexes, Is.EqualTo(expected.Cells[i].HexIndexes), context);
            }
            Assert.That(Enumerable.Range(0, geometry.Topology.Count).Select(i => result[i].HasValue),
                Is.EqualTo(original.Select(id => id.HasValue)), context);
            result[0] = 123;
            Assert.That(Enumerable.Range(0, geometry.Topology.Count).Select(i => source[i]), Is.EqualTo(original), context);
        }
    }

    [Test]
    public void Diffuse_ValidatesInputsAndAssignments()
    {
        var geometry = new HexMapGeometry(2, 1, 1f, Layout.OddR);
        var source = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry,
            new[] { new HexPartitionCell(7, new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }) });
        var regions = new HexMap<int>(geometry.Topology);
        var targets = new Dictionary<int, float> { [7] = 2f };
        var parameters = new CellularPottsParameters(0);

        Assert.That(Assert.Throws<ArgumentNullException>(() => Apply(null!, regions, targets, parameters, null))!.ParamName,
            Is.EqualTo("partition"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => Apply(source, regions, targets, null!, null))!.ParamName,
            Is.EqualTo("parameters"));
        if (_overload >= 2)
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => Apply(source, null!, targets, parameters, null))!.ParamName,
                Is.EqualTo("regions"));
            var incompatible = new HexMap<int>(new HexMapTopology(1, 2, Layout.OddR));
            Assert.That(Assert.Throws<ArgumentException>(() => Apply(source, incompatible, targets, parameters, null))!.ParamName,
                Is.EqualTo("regions"));
        }
        if (_overload == 1 || _overload == 3)
        {
            targets[7] = -1f;
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Apply(source, regions, targets, parameters, null))!.ParamName,
                Is.EqualTo("targetAreas"));
            targets[7] = 2f;
        }
        source[0] = null;
        Assert.That(Assert.Throws<ArgumentException>(() => Apply(source, regions, targets, parameters, null))!.ParamName,
            Is.EqualTo("partition"));
    }

    [Test]
    public void Diffuse_EmptyMap_PreservesGeometryAndEmptyCells()
    {
        var geometry = new HexMapGeometry(0, 0, new VectorXY(-12f, 23f), 2.5f, Layout.EvenQ);
        var source = new SpatialPartialHexPartitionMap<HexPartitionCell>(geometry,
            new[] { new HexPartitionCell(7, Array.Empty<VectorXYInt>()) });

        var result = Apply(source, new HexMap<int>(geometry.Topology), null, new CellularPottsParameters(3), null);

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(result.Cells, Has.Count.EqualTo(1));
        Assert.That(result.Cells[0].Id, Is.EqualTo(7));
        Assert.That(result.Cells[0].HexIndexes, Is.Empty);
        Assert.That(result.Cells[0], Is.Not.SameAs(source.Cells[0]));
    }

    private SpatialPartialHexPartitionMap<HexPartitionCell> Apply(SpatialPartialHexPartitionMap<HexPartitionCell> source, HexMap<int> regions,
        IReadOnlyDictionary<int, float>? targets, CellularPottsParameters parameters, Random? random) => _overload switch
    {
        0 => source.DiffuseByCellularPotts(parameters, random),
        1 => source.DiffuseByCellularPotts(targets, parameters, random),
        2 => source.DiffuseByCellularPotts(regions, parameters, random),
        3 => source.DiffuseByCellularPotts(regions, targets, parameters, random),
        _ => throw new InvalidOperationException()
    };

    private PartialHexPartitionMap<HexPartitionCell> ApplyBase(PartialHexPartitionMap<HexPartitionCell> source, HexMap<int> regions,
        IReadOnlyDictionary<int, float>? targets, CellularPottsParameters parameters, Random? random) => _overload switch
    {
        0 => source.DiffuseByCellularPotts(parameters, random),
        1 => source.DiffuseByCellularPotts(targets, parameters, random),
        2 => source.DiffuseByCellularPotts(regions, parameters, random),
        3 => source.DiffuseByCellularPotts(regions, targets, parameters, random),
        _ => throw new InvalidOperationException()
    };
}
