using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning.Voronoi;

public class VoronoiHexPartitionParametersTests
{
    [Test]
    public void Constructor_DefaultsPreserveEmptyAndDisconnectedCells()
    {
        var parameters = new VoronoiHexPartitionParameters();
        Assert.Multiple(() =>
        {
            Assert.That(parameters.RelaxationIterations, Is.Zero);
            Assert.That(parameters.EmptyCellPolicy, Is.EqualTo(EmptyCellPolicy.LeaveAsIs));
            Assert.That(parameters.ExclavePolicy, Is.EqualTo(ExclavePolicy.LeaveAsIs));
        });
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Constructor_NegativeIterationsAreRejected(int iterations)
    {
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VoronoiHexPartitionParameters(iterations))!.ParamName, Is.EqualTo("relaxationIterations"));
    }

    [TestCase(-1)]
    [TestCase(3)]
    [TestCase(int.MaxValue)]
    public void Constructor_UnknownPoliciesAreRejected(int value)
    {
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VoronoiHexPartitionParameters(emptyCellPolicy: (EmptyCellPolicy)value))!.ParamName, Is.EqualTo("emptyCellPolicy"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VoronoiHexPartitionParameters(exclavePolicy: (ExclavePolicy)value))!.ParamName, Is.EqualTo("exclavePolicy"));
    }

    [Test]
    public void Partition_NullParametersUseDefaultsForEveryOverload()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR), 1f);
        var centers = new HexCenterMap(geometry);
        // Coincident sites leave the second cell empty; Hexes defaults must retain it.
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[0], 1f) };
        var participation = new BoolHexMap(geometry.Topology, Enumerable.Repeat(true, geometry.Topology.Count).ToArray());
        var regions = new IntHexMap(geometry.Topology);
        var nullableRegions = new HexMap<int?>(geometry.Topology, Enumerable.Repeat((int?)0, geometry.Topology.Count).ToArray());

        foreach (var partition in new[]
        {
            centers.PartitionByVoronoi(sites, parameters: null),
            centers.PartitionByVoronoi(regions, sites, parameters: null)
        })
        {
            Assert.That(partition.Cells, Has.Count.EqualTo(2));
            Assert.That(partition.Cells[1].HexIndexes, Is.Empty);
        }
        foreach (var partition in new[]
        {
            centers.PartitionByVoronoi(participation, sites, parameters: null),
            centers.PartitionByVoronoi(nullableRegions, sites, parameters: null),
            centers.PartitionByVoronoi(participation, regions, sites, parameters: null)
        })
        {
            Assert.That(partition.Cells, Has.Count.EqualTo(2));
            Assert.That(partition.Cells[1].HexIndexes, Is.Empty);
        }
    }
}
