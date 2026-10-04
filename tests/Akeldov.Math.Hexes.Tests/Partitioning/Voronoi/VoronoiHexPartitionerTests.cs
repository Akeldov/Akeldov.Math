using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning.Voronoi;

public class VoronoiHexPartitionerTests
{
    [Test]
    public void Partition_WithParticipationExclaves_GrowsThroughMultipleHexes(
        [Values] bool useExtension,
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude, EmptyCellPolicy.ThrowException)] EmptyCellPolicy emptyPolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var mask = new BoolHexMap(centers.Topology, new[] { true, false, true, true, true });
        var sites = new[] { new Site(centers[0], 10f), new Site(centers[4], 1f) };

        var unchanged = centers.ToVoronoiHexPartitionMap(sites, mask);
        var map = useExtension
            ? centers.ToVoronoiHexPartitionMap(sites, mask, emptyPolicy, ExclavePolicy.ReassignToClosestCell)
            : new VoronoiHexPartitioner(sites, emptyPolicy).Partition(centers, mask, ExclavePolicy.ReassignToClosestCell);

        Assert.Multiple(() =>
        {
            Assert.That(unchanged[2], Is.EqualTo(0));
            Assert.That(unchanged[3], Is.EqualTo(0));
            Assert.That(map[0], Is.EqualTo(0));
            Assert.That(map[1], Is.Null);
            Assert.That(map[2], Is.EqualTo(1));
            Assert.That(map[3], Is.EqualTo(1));
            Assert.That(map[4], Is.EqualTo(1));
            Assert.That(map.Cells, Has.Count.EqualTo(2));
            Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[]
            {
                new VectorXYInt(2, 0), new VectorXYInt(3, 0), new VectorXYInt(4, 0)
            }));
            Assert.That(mask[1], Is.False);
        });
    }

    [Test]
    public void Partition_WithRegionExclave_ReassignsOnlyWithinRegion(
        [Values] bool useExtension, [Values] bool combinedMasks)
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(centers.Topology, new[] { -7, -7, 42, -7, -7, -7, -7 });
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 7).ToArray());
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[6], 1f), new Site(centers[2], 1f) };
        var partitioner = new VoronoiHexPartitioner(sites);
        int[] assignments;
        IReadOnlyList<VoronoiCell> cells;
        if (combinedMasks)
        {
            var map = useExtension
                ? centers.ToVoronoiHexPartitionMap(sites, participation, regions, EmptyCellPolicy.LeaveAsIs,
                    ExclavePolicy.ReassignToClosestCell)
                : partitioner.Partition(centers, participation, regions, ExclavePolicy.ReassignToClosestCell);
            assignments = Enumerable.Range(0, 7).Select(i => map[i]!.Value).ToArray();
            cells = map.Cells;
        }
        else
        {
            var map = useExtension
                ? centers.ToVoronoiHexPartitionMap(sites, regions, exclavePolicy: ExclavePolicy.ReassignToClosestCell)
                : partitioner.Partition(centers, regions, ExclavePolicy.ReassignToClosestCell);
            assignments = Enumerable.Range(0, 7).Select(i => map[i]).ToArray();
            cells = map.Cells;
        }

        Assert.That(assignments, Is.EqualTo(new[] { 0, 0, 2, 1, 1, 1, 1 }));
        Assert.That(cells, Has.Count.EqualTo(3));
        Assert.That(regions[3], Is.EqualTo(-7));
    }

    [Test]
    public void Partition_WithBlockedRegionExclaves_CreatesOneCellPerComponent(
        [Values] bool useExtension, [Values] bool combinedMasks,
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy emptyPolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(centers.Topology, new[] { 0, 1, 0, 0, 1, 0, 0 });
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 7).ToArray());
        var sites = new[] { new Site(centers[0], 3f), new Site(centers[1], 1f), new Site(centers[4], 1f),
            new Site(new PointXY(-100f, -100f), 1f) };
        var partitioner = new VoronoiHexPartitioner(sites, emptyPolicy);
        int[] assignments;
        IReadOnlyList<VoronoiCell> cells;
        if (combinedMasks)
        {
            var map = useExtension
                ? centers.ToVoronoiHexPartitionMap(sites, participation, regions, emptyPolicy,
                    ExclavePolicy.ReassignToClosestCell)
                : partitioner.Partition(centers, participation, regions, ExclavePolicy.ReassignToClosestCell);
            assignments = Enumerable.Range(0, 7).Select(i => map[i]!.Value).ToArray();
            cells = map.Cells;
        }
        else
        {
            var map = useExtension
                ? centers.ToVoronoiHexPartitionMap(sites, regions, emptyPolicy, ExclavePolicy.ReassignToClosestCell)
                : partitioner.Partition(centers, regions, ExclavePolicy.ReassignToClosestCell);
            assignments = Enumerable.Range(0, 7).Select(i => map[i]).ToArray();
            cells = map.Cells;
        }

        int firstNew = emptyPolicy == EmptyCellPolicy.Exclude ? 3 : 4;
        Assert.Multiple(() =>
        {
            Assert.That(cells, Has.Count.EqualTo(firstNew + 2));
            Assert.That(assignments, Is.EqualTo(new[] { 0, 1, firstNew, firstNew, 2, firstNew + 1, firstNew + 1 }));
            Assert.That(cells[firstNew].Center, Is.EqualTo(centers[2]));
            Assert.That(cells[firstNew + 1].Center, Is.EqualTo(centers[5]));
            Assert.That(cells[firstNew].Site.Weight, Is.EqualTo(3f));
            Assert.That(cells[firstNew].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0), new VectorXYInt(3, 0) }));
            Assert.That(cells.Select(cell => cell.Id), Is.EqualTo(Enumerable.Range(0, cells.Count)));
        });
    }

    [Test]
    public void Partition_WithIsolatedParticipationComponents_CreatesCellsAndPreservesExcludedHexes()
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var mask = new BoolHexMap(centers.Topology, new[] { true, true, false, true, true, false, true });
        var sites = new[] { new Site(centers[0], 2f) };

        var map = centers.ToVoronoiHexPartitionMap(sites, mask, ExclavePolicy.ReassignToClosestCell);

        Assert.Multiple(() =>
        {
            Assert.That(Enumerable.Range(0, 7).Select(i => map[i]), Is.EqualTo(new int?[] { 0, 0, null, 1, 1, null, 2 }));
            Assert.That(map.Cells[1].Center, Is.EqualTo(centers[3]));
            Assert.That(map.Cells[2].Center, Is.EqualTo(centers[6]));
            Assert.That(map.Cells, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Partition_WithInvalidExclavePolicy_Throws()
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };
        var mask = new BoolHexMap(centers.Topology, new[] { true });
        var regions = new IntHexMap(centers.Topology);
        var invalid = (ExclavePolicy)123;

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            centers.ToVoronoiHexPartitionMap(sites, mask, invalid))!.ParamName, Is.EqualTo("exclavePolicy"));
        Assert.Throws<ArgumentOutOfRangeException>(() => centers.ToVoronoiHexPartitionMap(sites, regions, invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            centers.ToVoronoiHexPartitionMap(sites, mask, regions, EmptyCellPolicy.LeaveAsIs, invalid));
    }

    [Test]
    public void Partition_WithExclave_SelectsClosestAdjacentCenterWithoutWeights()
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(centers.Topology, new[] { 0, 1, 0, 0, 0 });
        var fartherPosition = new PointXY(centers[4].X + (centers[4].X - centers[3].X) * 0.25f, centers[4].Y);
        var sites = new[] { new Site(centers[0], 10f), new Site(centers[2], 0.01f),
            new Site(fartherPosition, 1f), new Site(centers[1], 1f) };

        var unchanged = centers.ToVoronoiHexPartitionMap(sites, regions);
        var map = centers.ToVoronoiHexPartitionMap(sites, regions, ExclavePolicy.ReassignToClosestCell);

        Assert.That(unchanged[3], Is.EqualTo(0));
        Assert.That(map[3], Is.EqualTo(1));
    }

    [Test]
    public void Partition_WithMaskedSite_KeepsComponentClosestToSite()
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var mask = new BoolHexMap(centers.Topology, new[] { true, true, false, false, true });
        var sites = new[] { new Site(centers[3], 1f) };

        var map = centers.ToVoronoiHexPartitionMap(sites, mask, ExclavePolicy.ReassignToClosestCell);

        Assert.That(map[4], Is.EqualTo(0));
        Assert.That(map[0], Is.EqualTo(1));
        Assert.That(map[1], Is.EqualTo(1));
        Assert.That(map.Cells[1].Center, Is.EqualTo(centers[1]));
    }

    [Test]
    public void Partition_WithBothMasks_RestrictsSitesByRegionAndSkipsExcludedRegions(
        [Values] bool useExtension,
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(4, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { false, true, true, false });
        var regions = new IntHexMap(centers.Topology, new[] { -7, 0, -7, 42 });
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[1], 1f), new Site(centers[3], 1f) };

        var map = useExtension
            ? centers.ToVoronoiHexPartitionMap(sites, participation, regions, policy)
            : new VoronoiHexPartitioner(sites, policy).Partition(centers, participation, regions);

        Assert.Multiple(() =>
        {
            Assert.That(map[0], Is.Null);
            Assert.That(map[3], Is.Null);
            Assert.That(map.Cells[map[1]!.Value].Site, Is.EqualTo(sites[1]));
            Assert.That(map.Cells[map[2]!.Value].Site, Is.EqualTo(sites[0]));
            Assert.That(map.Participates(0), Is.False);
            Assert.That(map.Participates(2), Is.True);
            Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 2 : 3));
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
        });
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs)]
    [TestCase(EmptyCellPolicy.Exclude)]
    [TestCase(EmptyCellPolicy.ThrowException)]
    public void ToVoronoiHexPartitionMap_WithBothMasksAndNoParticipation_AppliesEmptyCellPolicy(EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology);
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(new PointXY(100f, 100f), 1f) };

        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() => centers.ToVoronoiHexPartitionMap(sites, participation, regions, policy));
            return;
        }

        var map = centers.ToVoronoiHexPartitionMap(sites, participation, regions, policy);
        Assert.That(map[0], Is.Null);
        Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 1));
    }

    [Test]
    public void ToVoronoiHexPartitionMap_WithBothMasks_ValidatesMasksAndMissingRegionSites()
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, true });
        var regions = new IntHexMap(centers.Topology, new[] { 0, 1 });
        var sites = new[] { new Site(centers[0], 1f) };
        var differentTopology = new HexMapTopology(1, 1, Layout.OddR);

        Assert.Throws<ArgumentNullException>(() => centers.ToVoronoiHexPartitionMap(sites, null!, regions, EmptyCellPolicy.LeaveAsIs));
        Assert.Throws<ArgumentNullException>(() => centers.ToVoronoiHexPartitionMap(sites, participation, null!, EmptyCellPolicy.LeaveAsIs));
        Assert.Throws<ArgumentException>(() => centers.ToVoronoiHexPartitionMap(sites, new BoolHexMap(differentTopology), regions, EmptyCellPolicy.LeaveAsIs));
        Assert.Throws<ArgumentException>(() => centers.ToVoronoiHexPartitionMap(sites, participation, new IntHexMap(differentTopology), EmptyCellPolicy.LeaveAsIs));
        Assert.Throws<InvalidOperationException>(() => centers.ToVoronoiHexPartitionMap(sites, participation, regions, EmptyCellPolicy.LeaveAsIs));
    }

    [Test]
    public void Partition_AssignsEachHexCenterToNearestSite()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[0]));
        Assert.That(map[0], Is.EqualTo(0));
        Assert.That(map.Cells[map[0]].Center, Is.EqualTo(sites[0].Position));
        Assert.That(map[1], Is.EqualTo(map[0]));
        Assert.That(map.Cells[map[2]].Site, Is.EqualTo(sites[1]));
        Assert.That(map[2], Is.EqualTo(1));
        Assert.That(map.Cells[map[2]].Center, Is.EqualTo(sites[1].Position));
        Assert.That(map.Cells, Has.Count.EqualTo(2));
        Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));
        Assert.That(map[2], Is.EqualTo(map.Cells[1].Id));
        Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
    }

    [Test]
    public void Partition_ReturnsReadOnlyHexMapOfCellIds()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.EvenQ));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);
        IHexMap<int> hexMap = map;

        Assert.That(map, Is.Not.InstanceOf<HexMap<int>>());
        Assert.That(map.Centers, Is.SameAs(hexCenters));
        Assert.That(hexMap.Topology.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
        Assert.That(hexMap.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Cells, Has.Count.EqualTo(1));
        Assert.That(map.Cells[0].HexIndexes, Has.Count.EqualTo(4));
    }

    [Test]
    public void ToVoronoiHexPartitionMap_PartitionsHexCenters()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));

        var map = hexCenters.ToVoronoiHexPartitionMap(sites);
        ISpatialHexMap<int> spatialMap = map;

        Assert.That(map.Centers, Is.SameAs(hexCenters));
        Assert.That(spatialMap.Geometry, Is.EqualTo(hexCenters.Geometry));
        Assert.That(map[0], Is.EqualTo(0));
        Assert.That(map[1], Is.EqualTo(0));
        Assert.That(map[2], Is.EqualTo(1));
        Assert.That(map.Cells, Has.Count.EqualTo(2));
    }

    [Test]
    public void Partition_WithParticipationMask_AssignsOnlyParticipatingHexCenters()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, false, true });
        var partitioner = new VoronoiHexPartitioner(sites);

        MaskedVoronoiHexPartitionMap map = partitioner.Partition(hexCenters, participationMask);
        ISpatialHexMap<int?> spatialMap = map;

        Assert.Multiple(() =>
        {
            Assert.That(map.Centers, Is.SameAs(hexCenters));
            Assert.That(spatialMap.Geometry, Is.EqualTo(hexCenters.Geometry));
            Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
            Assert.That(map[1], Is.Null);
            Assert.That(map[2], Is.EqualTo(map.Cells[1].Id));
            Assert.That(map[new VectorXYInt(0, 0)], Is.EqualTo(map.Cells[0].Id));
            Assert.That(map[new VectorXYInt(1, 0)], Is.Null);
            Assert.That(map.Participates(0), Is.True);
            Assert.That(map.Participates(1), Is.False);
            Assert.That(map.Participates(new VectorXYInt(2, 0)), Is.True);
            Assert.That(map.Cells, Has.Count.EqualTo(2));
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
            Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
        });
    }

    [Test]
    public void ToVoronoiHexPartitionMap_WithParticipationMask_PartitionsOnlyParticipatingHexCenters()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, false, true });

        MaskedVoronoiHexPartitionMap map = hexCenters.ToVoronoiHexPartitionMap(sites, participationMask);

        Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
        Assert.That(map[1], Is.Null);
        Assert.That(map[2], Is.EqualTo(map.Cells[1].Id));
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
        Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
    }

    [Test]
    public void Partition_WithEmptyParticipationMask_KeepsEmptyCellsAndNullAssignments()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(hexCenters.Topology);
        var partitioner = new VoronoiHexPartitioner(sites);

        MaskedVoronoiHexPartitionMap map = partitioner.Partition(hexCenters, participationMask);

        Assert.Multiple(() =>
        {
            Assert.That(map[0], Is.Null);
            Assert.That(map[1], Is.Null);
            Assert.That(map.Participates(0), Is.False);
            Assert.That(map.Participates(1), Is.False);
            Assert.That(map.Cells, Has.Count.EqualTo(2));
            Assert.That(map.Cells[0].HexIndexes, Is.Empty);
            Assert.That(map.Cells[1].HexIndexes, Is.Empty);
        });
    }

    [Test]
    public void Partition_WithRegionMask_AssignsOnlySitesInTheSameRegion(
        [Values(Layout.OddR, Layout.EvenR, Layout.OddQ, Layout.EvenQ)] Layout layout,
        [Values] bool useExtension)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 2, new VectorXY(12f, -8f), 2f, layout));
        var regions = new IntHexMap(hexCenters.Topology, new[] { -7, 0, 42, 42, -7, 0 });
        var sites = new[]
        {
            new Site(hexCenters[0] + new VectorXY(0.1f, -0.1f), 1f),
            new Site(hexCenters[1] + new VectorXY(-0.1f, 0.1f), float.PositiveInfinity),
            new Site(hexCenters[2] + new VectorXY(0.1f, 0.1f), 1f)
        };

        var map = useExtension
            ? hexCenters.ToVoronoiHexPartitionMap(sites, regions)
            : new VoronoiHexPartitioner(sites).Partition(hexCenters, regions);

        Assert.Multiple(() =>
        {
            Assert.That(map.Centers, Is.SameAs(hexCenters));
            Assert.That(map.Cells, Has.Count.EqualTo(3));
            Assert.That(Enumerable.Range(0, 6).Select(i => map[i]), Is.EqualTo(new[] { 0, 1, 2, 2, 0, 1 }));
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 1) }));
            Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(1, 0), new VectorXYInt(2, 1) }));
            Assert.That(map.Cells[2].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0), new VectorXYInt(0, 1) }));
        });
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs, 1f)]
    [TestCase(EmptyCellPolicy.Exclude, 1f)]
    [TestCase(EmptyCellPolicy.LeaveAsIs, float.PositiveInfinity)]
    [TestCase(EmptyCellPolicy.Exclude, float.PositiveInfinity)]
    public void Partition_WithOneRegion_MatchesUnmaskedPartition(EmptyCellPolicy policy, float weight)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(5, 2, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology);
        var sites = new[]
        {
            new Site(hexCenters[0], 0f),
            new Site(hexCenters[1], weight),
            new Site(hexCenters[1], weight),
            new Site(hexCenters[4], weight),
            new Site(hexCenters[9], 3f)
        };
        var partitioner = new VoronoiHexPartitioner(sites, policy);

        var expected = partitioner.Partition(hexCenters);
        var actual = partitioner.Partition(hexCenters, regions);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Cells.Count, Is.EqualTo(expected.Cells.Count));
            for (int i = 0; i < expected.Cells.Count; i++)
            {
                Assert.That(actual.Cells[i].SiteIndex, Is.EqualTo(expected.Cells[i].SiteIndex));
                Assert.That(actual.Cells[i].Site, Is.EqualTo(expected.Cells[i].Site));
                Assert.That(actual.Cells[i].HexIndexes, Is.EqualTo(expected.Cells[i].HexIndexes));
            }

            for (int i = 0; i < hexCenters.Topology.Count; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]));
                Assert.That(actual[i], Is.EqualTo(actual.Cells[actual[i]].Id));
            }
        });
    }

    [Test]
    public void Partition_WithRegionMask_UsesWeightsWithinEachRegion()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology, new[] { 10, 10, 20, 10, 10 });
        var sites = new[]
        {
            new Site(hexCenters[0], 1f),
            new Site(hexCenters[2], float.PositiveInfinity),
            new Site(hexCenters[4], 4f)
        };

        var map = new VoronoiHexPartitioner(sites).Partition(hexCenters, regions);

        Assert.That(Enumerable.Range(0, 5).Select(i => map[i]), Is.EqualTo(new[] { 0, 2, 1, 2, 2 }));
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs)]
    [TestCase(EmptyCellPolicy.Exclude)]
    [TestCase(EmptyCellPolicy.ThrowException)]
    public void Partition_WithRegionMask_WhenAllCellsArePopulated_PreservesCells(EmptyCellPolicy policy)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology, new[] { 10, 20 });
        var sites = new[] { new Site(hexCenters[0], 0f), new Site(hexCenters[1], 1f) };

        var map = new VoronoiHexPartitioner(sites, policy).Partition(hexCenters, regions);

        Assert.That(map.Cells, Has.Count.EqualTo(2));
        Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
        Assert.That(map[1], Is.EqualTo(map.Cells[1].Id));
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs)]
    [TestCase(EmptyCellPolicy.Exclude)]
    [TestCase(EmptyCellPolicy.ThrowException)]
    public void Partition_WithRegionMask_WhenRegionHasNoSites_Throws(EmptyCellPolicy policy)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology, new[] { 10, 71 });
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(hexCenters[0], 1f) }, policy);

        var exception = Assert.Throws<InvalidOperationException>(() => partitioner.Partition(hexCenters, regions));

        Assert.That(exception!.Message, Does.Contain("71"));
    }

    [Test]
    public void Partition_WithRegionMask_WhenRegionHasOnlyZeroWeightSiteAwayFromHex_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology, new[] { 10, 71, 71 });
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 0f) };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new VoronoiHexPartitioner(sites).Partition(hexCenters, regions));

        Assert.That(exception!.Message, Does.Contain("71"));
    }

    [Test]
    public void Partition_WithRegionMask_AppliesEmptyCellPolicyToDuplicateAndOutsideSites(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude, EmptyCellPolicy.ThrowException)] EmptyCellPolicy policy,
        [Values] bool useExtension)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology, new[] { 10, 20 });
        var sites = new[]
        {
            new Site(new PointXY(-100f, 0f), float.PositiveInfinity),
            new Site(hexCenters[1], 1f),
            new Site(hexCenters[1], 1f),
            new Site(hexCenters[0], 1f)
        };
        var partitioner = new VoronoiHexPartitioner(sites, policy);
        VoronoiHexPartitionMap Partition() => useExtension
            ? hexCenters.ToVoronoiHexPartitionMap(sites, regions, policy)
            : partitioner.Partition(hexCenters, regions);

        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() => Partition());
            return;
        }

        var map = Partition();

        Assert.Multiple(() =>
        {
            Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 2 : 4));
            Assert.That(map.Cells.Select(cell => cell.SiteIndex), Is.EqualTo(Enumerable.Range(0, map.Cells.Count)));
            Assert.That(map[0], Is.EqualTo(map.Cells[policy == EmptyCellPolicy.Exclude ? 1 : 3].Id));
            Assert.That(map[1], Is.EqualTo(map.Cells[policy == EmptyCellPolicy.Exclude ? 0 : 1].Id));
            Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[3]));
            Assert.That(map.Cells[map[1]].Site, Is.EqualTo(sites[1]));
            Assert.That(map.Cells[map[0]].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
            Assert.That(map.Cells[map[1]].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(1, 0) }));
            Assert.That(map.Cells.Count(cell => cell.HexIndexes.Count == 0),
                Is.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 2));
        });
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs)]
    [TestCase(EmptyCellPolicy.Exclude)]
    [TestCase(EmptyCellPolicy.ThrowException)]
    public void Partition_WithRegionMask_WhenMapIsEmpty_AppliesEmptyCellPolicy(EmptyCellPolicy policy)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(0, 0, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology);
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(new PointXY(0f, 0f), 1f) }, policy);
        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() => partitioner.Partition(hexCenters, regions));
            return;
        }

        var map = partitioner.Partition(hexCenters, regions);

        Assert.That(map.Topology.Count, Is.Zero);
        Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 1));
        Assert.That(map.Cells.All(cell => cell.HexIndexes.Count == 0), Is.True);
    }

    [Test]
    public void Partition_WithRegionMask_WhenHexCenterIsNotFinite_Throws()
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(float.MaxValue, 0f),
            (float.MaxValue / 4f).ConvertHexApothemToRadius(), Layout.OddR);
        var hexCenters = new HexCenterMap(geometry);
        var regions = new IntHexMap(hexCenters.Topology);
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(hexCenters[0], 1f) });

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => partitioner.Partition(hexCenters, regions));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void Partition_WithRegionMask_WhenEitherMapIsNull_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology);
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(hexCenters[0], 1f) });

        Assert.That(Assert.Throws<ArgumentNullException>(() => partitioner.Partition(null!, regions))!.ParamName,
            Is.EqualTo("hexCenters"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => partitioner.Partition(hexCenters, (IHexMap<int>)null!))!.ParamName,
            Is.EqualTo("regionsMask"));
    }

    [TestCase(1, 2, Layout.OddR)]
    [TestCase(2, 1, Layout.EvenR)]
    public void Partition_WithRegionMask_WhenTopologyDiffers_Throws(int width, int height, Layout layout)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(new HexMapTopology(width, height, layout));
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(hexCenters[0], 1f) });

        var exception = Assert.Throws<ArgumentException>(() => partitioner.Partition(hexCenters, regions));

        Assert.That(exception!.ParamName, Is.EqualTo("regionsMask"));
    }

    [Test]
    public void MaskedPartitionToMutableMaps_ReturnCallerOwnedCopies()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, false, true });
        var partitioner = new VoronoiHexPartitioner(sites);
        MaskedVoronoiHexPartitionMap map = partitioner.Partition(hexCenters, participationMask);

        HexMap<int?> mutableMap = map.ToMutableHexMap();
        BoolHexMap mutableMask = map.ToMutableParticipationMask();
        mutableMap[1] = map.Cells[1].Id;
        mutableMask[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(mutableMap[1], Is.EqualTo(map.Cells[1].Id));
            Assert.That(map[1], Is.Null);
            Assert.That(mutableMask[0], Is.False);
            Assert.That(map.Participates(0), Is.True);
        });
    }

    [Test]
    public void ToMutableHexMap_ReturnsCallerOwnedAssignmentCopy()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);
        var map = partitioner.Partition(hexCenters);

        HexMap<int> mutableMap = map.ToMutableHexMap();
        mutableMap[0] = map.Cells[1].Id;

        Assert.Multiple(() =>
        {
            Assert.That(mutableMap[0], Is.EqualTo(map.Cells[1].Id));
            Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));
            Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
        });
    }

    [Test]
    public void Partition_WhenSiteHasLargerWeight_AssignsFartherCenterToIt()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(10f, 0f), 3f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, new VectorXY(3f, 0f), 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[1]));
        Assert.That(map[0], Is.EqualTo(1));
    }

    [Test]
    public void Partition_WhenSiteWeightIsZero_AssignsOnlyExactSitePointToIt()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 0f),
            new Site(new PointXY(2f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[0]));
        Assert.That(map[0], Is.EqualTo(0));
        Assert.That(map.Cells[map[1]].Site, Is.EqualTo(sites[1]));
        Assert.That(map[1], Is.EqualTo(1));
    }

    [Test]
    public void Partition_WhenInfiniteWeightSiteExists_AssignsFinitePointsToNearestInfiniteSite()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(10f, 0f), float.PositiveInfinity)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, new VectorXY(-100f, 0f), 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[1]));
        Assert.That(map[0], Is.EqualTo(1));
    }

    [Test]
    public void Partition_WithLargeFiniteCoordinates_UsesWideDistanceArithmetic()
    {
        var sites = new[]
        {
            new Site(new PointXY(float.MaxValue, 0f), 1f),
            new Site(new PointXY(float.MaxValue / 2f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map[0], Is.EqualTo(1));
        Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[1]));
    }

    [Test]
    public void Partition_WhenHexCenterCoordinateIsNotFinite_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var geometry = new HexMapGeometry(
            width: 2,
            height: 1,
            origin: new VectorXY(float.MaxValue, 0f),
            radius: (float.MaxValue / 4f).ConvertHexApothemToRadius(),
            layout: Layout.OddR);
        var hexCenters = new HexCenterMap(geometry);
        var partitioner = new VoronoiHexPartitioner(sites);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            partitioner.Partition(hexCenters));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void Partition_WithParticipationMask_DoesNotReadExcludedHexCenters()
    {
        var sites = new[] { new Site(new PointXY(float.MaxValue, 0f), 1f) };
        var geometry = new HexMapGeometry(
            width: 2,
            height: 1,
            origin: new VectorXY(float.MaxValue, 0f),
            radius: (float.MaxValue / 4f).ConvertHexApothemToRadius(),
            layout: Layout.OddR);
        var hexCenters = new HexCenterMap(geometry);
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, false });
        var partitioner = new VoronoiHexPartitioner(sites);

        MaskedVoronoiHexPartitionMap map = partitioner.Partition(hexCenters, participationMask);

        Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
        Assert.That(map[1], Is.Null);
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
    }

    [Test]
    public void Partition_WithParticipationMask_WhenIncludedHexCenterCoordinateIsNotFinite_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var geometry = new HexMapGeometry(
            width: 2,
            height: 1,
            origin: new VectorXY(float.MaxValue, 0f),
            radius: (float.MaxValue / 4f).ConvertHexApothemToRadius(),
            layout: Layout.OddR);
        var hexCenters = new HexCenterMap(geometry);
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { false, true });
        var partitioner = new VoronoiHexPartitioner(sites);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            partitioner.Partition(hexCenters, participationMask));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void VoronoiCell_WhenSiteIndexIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VoronoiCell(-1, new Site(new PointXY(0f, 0f), 1f), Array.Empty<VectorXYInt>()));
    }

    [Test]
    public void VoronoiCell_WhenHexIndexesIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new VoronoiCell(0, new Site(new PointXY(0f, 0f), 1f), null!));
    }

    [Test]
    public void Partition_WhenCellReceivesNoHexes_KeepsEmptyCell()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map.Cells, Has.Count.EqualTo(2));
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
        Assert.That(map.Cells[1].HexIndexes, Is.Empty);
    }

    [Test]
    public void Partition_WithEmptyCellPolicy_HandlesEmptyCellsAndAssignments(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy,
        [Values] bool useExtension)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[]
        {
            new Site(new PointXY(-100f, 0f), 1f),
            new Site(hexCenters[1], 1f),
            new Site(new PointXY(100f, 0f), 1f),
            new Site(hexCenters[0], 1f),
            new Site(new PointXY(200f, 0f), 1f)
        };

        var map = useExtension
            ? hexCenters.ToVoronoiHexPartitionMap(sites, policy)
            : new VoronoiHexPartitioner(sites, policy).Partition(hexCenters);

        int firstCellIndex = policy == EmptyCellPolicy.Exclude ? 0 : 1;
        int secondCellIndex = policy == EmptyCellPolicy.Exclude ? 1 : 3;
        Assert.Multiple(() =>
        {
            Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 2 : 5));
            Assert.That(map.Cells.Select(cell => cell.SiteIndex), Is.EqualTo(Enumerable.Range(0, map.Cells.Count)));
            Assert.That(map[0], Is.EqualTo(map.Cells[secondCellIndex].Id));
            Assert.That(map[1], Is.EqualTo(map.Cells[firstCellIndex].Id));
            Assert.That(map.Cells[map[0]].Site, Is.EqualTo(sites[3]));
            Assert.That(map.Cells[map[1]].Site, Is.EqualTo(sites[1]));
            Assert.That(map.Cells[map[0]].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
            Assert.That(map.Cells[map[1]].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(1, 0) }));
            Assert.That(map.Cells.Count(cell => cell.HexIndexes.Count == 0),
                Is.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 3));
        });
    }

    [Test]
    public void Partition_WithParticipationMaskAndEmptyCellPolicy_HandlesEmptyCellsAndAssignments(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy,
        [Values] bool useExtension)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[]
        {
            new Site(new PointXY(-100f, 0f), 1f),
            new Site(hexCenters[2], 1f),
            new Site(hexCenters[1], 1f),
            new Site(hexCenters[0], 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, false, true });

        var map = useExtension
            ? hexCenters.ToVoronoiHexPartitionMap(sites, participationMask, policy)
            : new VoronoiHexPartitioner(sites, policy).Partition(hexCenters, participationMask);

        int firstCellIndex = policy == EmptyCellPolicy.Exclude ? 0 : 1;
        int secondCellIndex = policy == EmptyCellPolicy.Exclude ? 1 : 3;
        Assert.Multiple(() =>
        {
            Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 2 : 5));
            Assert.That(map.Cells.Select(cell => cell.SiteIndex), Is.EqualTo(Enumerable.Range(0, map.Cells.Count)));
            Assert.That(map[0], Is.EqualTo(map.Cells[secondCellIndex].Id));
            Assert.That(map[1], Is.Null);
            Assert.That(map[2], Is.EqualTo(map.Cells[firstCellIndex].Id));
            Assert.That(map.Cells[map[0]!.Value].Site, Is.EqualTo(sites[3]));
            Assert.That(map.Cells[map[2]!.Value].Site, Is.EqualTo(sites[1]));
            Assert.That(map.Cells[map[0]!.Value].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
            Assert.That(map.Cells[map[2]!.Value].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
            Assert.That(map.Participates(0), Is.True);
            Assert.That(map.Participates(1), Is.False);
            Assert.That(map.Participates(2), Is.True);
            Assert.That(map.Cells.Count(cell => cell.HexIndexes.Count == 0),
                Is.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 3));
        });
    }

    [Test]
    public void ToVoronoiHexPartitionMap_WhenPolicyIsThrowExceptionAndCellIsEmpty_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[]
        {
            new Site(hexCenters[0], 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };

        Assert.Throws<InvalidOperationException>(() =>
            hexCenters.ToVoronoiHexPartitionMap(sites, EmptyCellPolicy.ThrowException));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ToVoronoiHexPartitionMap_WhenPolicyIsThrowExceptionAndMaskEmptiesCell_Throws(bool excludeAll)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 1f) };
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { !excludeAll, false });

        Assert.Throws<InvalidOperationException>(() =>
            hexCenters.ToVoronoiHexPartitionMap(sites, participationMask, EmptyCellPolicy.ThrowException));
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs, 2)]
    [TestCase(EmptyCellPolicy.Exclude, 0)]
    public void ToVoronoiHexPartitionMap_WithEmptyParticipationMask_AppliesEmptyCellPolicy(
        EmptyCellPolicy policy, int expectedCellCount)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 1f) };
        var participationMask = new BoolHexMap(hexCenters.Topology);

        var map = hexCenters.ToVoronoiHexPartitionMap(sites, participationMask, policy);

        Assert.Multiple(() =>
        {
            Assert.That(map.Cells, Has.Count.EqualTo(expectedCellCount));
            Assert.That(map.Cells.All(cell => cell.HexIndexes.Count == 0), Is.True);
            Assert.That(map[0], Is.Null);
            Assert.That(map[1], Is.Null);
        });
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs)]
    [TestCase(EmptyCellPolicy.Exclude)]
    [TestCase(EmptyCellPolicy.ThrowException)]
    public void ToVoronoiHexPartitionMap_WhenAllCellsArePopulated_PreservesCells(EmptyCellPolicy policy)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 1f) };
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, true });

        var map = hexCenters.ToVoronoiHexPartitionMap(sites, policy);
        var maskedMap = hexCenters.ToVoronoiHexPartitionMap(sites, participationMask, policy);

        Assert.Multiple(() =>
        {
            Assert.That(map.Cells, Has.Count.EqualTo(2));
            Assert.That(maskedMap.Cells, Has.Count.EqualTo(2));
            for (int i = 0; i < sites.Length; i++)
            {
                Assert.That(map[i], Is.EqualTo(map.Cells[i].Id));
                Assert.That(map[i], Is.EqualTo(i));
                Assert.That(map.Cells[map[i]].Site, Is.EqualTo(sites[i]));
                Assert.That(maskedMap[i], Is.EqualTo(maskedMap.Cells[i].Id));
                Assert.That(maskedMap[i], Is.EqualTo(i));
                Assert.That(maskedMap.Cells[maskedMap[i]!.Value].Site, Is.EqualTo(sites[i]));
            }
        });
    }

    [Test]
    public void Constructor_WhenSitesIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new VoronoiHexPartitioner(null!));
    }

    [Test]
    public void Constructor_WhenSitesIsEmpty_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VoronoiHexPartitioner(Array.Empty<Site>()));
    }

    [Test]
    public void Constructor_WhenAllSiteWeightsAreZero_Throws()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 0f),
            new Site(new PointXY(1f, 0f), 0f)
        };

        var exception = Assert.Throws<ArgumentException>(() => new VoronoiHexPartitioner(sites));

        Assert.That(exception!.ParamName, Is.EqualTo("sites"));
    }

    [Test]
    public void Partition_WhenHexCenterMapIsNull_Throws()
    {
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(new PointXY(0f, 0f), 1f) });

        Assert.Throws<ArgumentNullException>(() => partitioner.Partition(null!));
    }

    [Test]
    public void Partition_WhenParticipationMaskIsNull_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(new PointXY(0f, 0f), 1f) });

        var exception = Assert.Throws<ArgumentNullException>(() =>
            partitioner.Partition(hexCenters, (IHexMap<bool>)null!));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }

    [Test]
    public void Partition_WhenParticipationMaskHasDifferentTopology_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var partitioner = new VoronoiHexPartitioner(new[] { new Site(new PointXY(0f, 0f), 1f) });

        var exception = Assert.Throws<ArgumentException>(() =>
            partitioner.Partition(hexCenters, participationMask));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }

    [Test]
    public void ToVoronoiHexPartitionMap_WhenHexCenterMapIsNull_Throws()
    {
        HexCenterMap hexCenters = null!;
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            hexCenters.ToVoronoiHexPartitionMap(sites));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void ToVoronoiHexPartitionMap_WhenParticipationMaskIsNull_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            hexCenters.ToVoronoiHexPartitionMap(sites, (IHexMap<bool>)null!));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }
}
