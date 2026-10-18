using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning.Voronoi;

public class HexCenterMapExtensionsTests
{
    public enum PartitionOverload
    {
        Full,
        Regions,
        Participation,
        NullableRegions,
        CombinedMasks
    }

    [Test]
    public void PartitionExtensions_WithEquivalentMasks_ApplyRelaxationAndPoliciesConsistently(
        [Values(0, 1, 2)] int iterations, [Values] ExclavePolicy exclavePolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(9, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 9).ToArray());
        var regions = new IntHexMap(centers.Topology);
        var nullableRegions = new HexMap<int?>(centers.Topology, Enumerable.Repeat((int?)0, 9).ToArray());
        var sites = new[] { new Site(centers[0], 2f), new Site(centers[2], 2f) };

        var full = centers.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(iterations, exclavePolicy: exclavePolicy));
        var regional = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(iterations, exclavePolicy: exclavePolicy));
        var masked = centers.PartitionByVoronoi(participation, sites, new VoronoiHexPartitionParameters(iterations, exclavePolicy: exclavePolicy));
        var nullable = centers.PartitionByVoronoi(nullableRegions, sites, new VoronoiHexPartitionParameters(iterations, exclavePolicy: exclavePolicy));
        var combined = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(iterations, exclavePolicy: exclavePolicy));

        int firstCellCount = iterations + 2;
        Assert.That(Enumerable.Range(0, 9).Select(i => full[i]),
            Is.EqualTo(Enumerable.Range(0, 9).Select(i => i < firstCellCount ? 0 : 1)));
        Assert.That(Enumerable.Range(0, 9).Select(i => regional[i]), Is.EqualTo(Enumerable.Range(0, 9).Select(i => full[i])));
        Assert.That(regional.Cells.Select(cell => cell.Site), Is.EqualTo(full.Cells.Select(cell => cell.Site)));
        foreach (var partial in new[] { masked, nullable, combined })
        {
            Assert.That(Enumerable.Range(0, 9).Select(i => partial[i]), Is.EqualTo(Enumerable.Range(0, 9).Select(i => (int?)full[i])));
            Assert.That(partial.Cells.Select(cell => cell.Site), Is.EqualTo(full.Cells.Select(cell => cell.Site)));
        }
    }

    [Test]
    public void PartitionExtensions_WithAllOptionalArgumentsOmitted_PreserveEmptyCellsAndDisableRelaxation()
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, true, true });
        var regions = new IntHexMap(centers.Topology);
        var nullableRegions = new HexMap<int?>(centers.Topology, new int?[] { 0, 0, 0 });
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[0], 1f) };

        var full = centers.PartitionByVoronoi(sites);
        var regional = centers.PartitionByVoronoi(regions, sites);
        var masked = centers.PartitionByVoronoi(participation, sites);
        var nullable = centers.PartitionByVoronoi(nullableRegions, sites);
        var combined = centers.PartitionByVoronoi(participation, regions, sites);

        foreach (var map in new[] { full, regional })
        {
            Assert.That(map.Cells.Select(cell => cell.Site), Is.EqualTo(sites));
            Assert.That(Enumerable.Range(0, 3).Select(i => map[i]), Is.All.EqualTo(0));
            Assert.That(map.Cells[1].HexIndexes, Is.Empty);
        }
        foreach (var map in new[] { masked, nullable, combined })
        {
            Assert.That(map.Cells.Select(cell => cell.Site), Is.EqualTo(sites));
            Assert.That(Enumerable.Range(0, 3).Select(i => map[i]), Is.All.EqualTo(0));
            Assert.That(map.Cells[1].HexIndexes, Is.Empty);
        }
    }

    [Test]
    public void PartitionExtensions_WithNegativeIterations_RejectArgumentInEveryOverload(
        [Values(-1, int.MinValue)] int iterations)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true });
        var regions = new IntHexMap(centers.Topology);
        var nullableRegions = new HexMap<int?>(centers.Topology, new int?[] { 0 });
        var sites = new[] { new Site(centers[0], 1f) };

        var calls = new TestDelegate[]
        {
            () => centers.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(iterations)),
            () => centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(iterations)),
            () => centers.PartitionByVoronoi(participation, sites, new VoronoiHexPartitionParameters(iterations)),
            () => centers.PartitionByVoronoi(nullableRegions, sites, new VoronoiHexPartitionParameters(iterations)),
            () => centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(iterations))
        };
        foreach (var call in calls)
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(call)!.ParamName, Is.EqualTo("relaxationIterations"));
    }

    [TestCase(1, 3)]
    [TestCase(2, 4)]
    public void PartitionByVoronoi_WithRelaxation_MovesSitesAndRepartitions(int iterations, int firstCellCount)
    {
        var centers = new HexCenterMap(new HexMapGeometry(9, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 9).ToArray());
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(centers[0], 2f), new Site(centers[2], 2f) };
        var originalSites = sites.ToArray();

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(relaxationIterations: iterations));

        var expectedFirst = iterations == 1
            ? new PointXY((centers[0].X + centers[1].X) / 2f, centers[0].Y)
            : centers[1];
        var expectedSecond = iterations == 1
            ? centers[5]
            : new PointXY((centers[5].X + centers[6].X) / 2f, centers[0].Y);
        Assert.Multiple(() =>
        {
            Assert.That(Enumerable.Range(0, 9).Select(i => map[i]),
                Is.EqualTo(Enumerable.Range(0, 9).Select(i => (int?)(i < firstCellCount ? 0 : 1))));
            Assert.That(map.Cells[0].Center.X, Is.EqualTo(expectedFirst.X).Within(0.00001f));
            Assert.That(map.Cells[1].Center.X, Is.EqualTo(expectedSecond.X).Within(0.00001f));
            Assert.That(map.Cells.Select(cell => cell.Site.Weight), Is.EqualTo(new[] { 2f, 2f }));
            Assert.That(sites, Is.EqualTo(originalSites));
            Assert.That(map.Cells[0].HexIndexes, Has.Count.EqualTo(firstCellCount));
        });
    }

    [Test]
    public void PartitionByVoronoi_WithZeroRelaxation_PreservesExistingBehavior(
        [Values] EmptyCellPolicy emptyPolicy, [Values] ExclavePolicy exclavePolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, true, false, true, true, true, true });
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(centers[0], 2f), new Site(centers[6], 1f) };

        var expected = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy, exclavePolicy: exclavePolicy));
        var actual = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(0, emptyCellPolicy: emptyPolicy, exclavePolicy: exclavePolicy));

        Assert.That(Enumerable.Range(0, 7).Select(i => actual[i]), Is.EqualTo(Enumerable.Range(0, 7).Select(i => expected[i])));
        Assert.That(actual.Cells.Select(cell => cell.Site), Is.EqualTo(expected.Cells.Select(cell => cell.Site)));
        for (int i = 0; i < actual.Cells.Count; i++)
            Assert.That(actual.Cells[i].HexIndexes, Is.EqualTo(expected.Cells[i].HexIndexes));
    }

    [TestCase(1)]
    [TestCase(3)]
    public void PartitionByVoronoi_WithRelaxation_KeepsOriginalRegionsAndSkipsExcludedHexes(int iterations)
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, false, true, false, true });
        var regions = new IntHexMap(centers.Topology, new[] { -7, 42, 42, 42, -7 });
        var sites = new[] { new Site(centers[0], 3f), new Site(centers[2], 1f) };

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(iterations));

        Assert.Multiple(() =>
        {
            Assert.That(Enumerable.Range(0, 5).Select(i => map[i]), Is.EqualTo(new int?[] { 0, null, 1, null, 0 }));
            Assert.That(map.Cells[0].Center.X, Is.EqualTo(centers[2].X).Within(0.00001f));
            Assert.That(map.Cells[1].Center, Is.EqualTo(centers[2]));
            Assert.That(map.Cells[0].Site.Weight, Is.EqualTo(3f));
            Assert.That(Enumerable.Range(0, 5).Select(i => participation[i]), Is.EqualTo(new[] { true, false, true, false, true }));
            Assert.That(Enumerable.Range(0, 5).Select(i => regions[i]), Is.EqualTo(new[] { -7, 42, 42, 42, -7 }));
        });
    }

    [Test]
    public void PartitionByVoronoi_WithRelaxation_AppliesExclavePolicyToRelaxedSites()
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, true, false, true, true, true, true });
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[6], 1f) };

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(1,
            exclavePolicy: ExclavePolicy.ReassignToClosestCell));

        Assert.That(Enumerable.Range(0, 7).Select(i => map[i]), Is.EqualTo(new int?[] { 0, 0, null, 1, 1, 1, 1 }));
        Assert.That(map.Cells[0].Center.X, Is.EqualTo((centers[0].X + centers[1].X + centers[3].X) / 3f).Within(0.00001f));
        Assert.That(map.Cells[1].Center.X, Is.EqualTo(centers[5].X).Within(0.00001f));
    }

    [Test]
    public void PartitionByVoronoi_WithRelaxation_PreservesOrExcludesEmptySites(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, true, true });
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(centers[0], 1f), new Site(new PointXY(100f, 100f), 1f) };

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(2, emptyCellPolicy: policy));

        Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 1 : 2));
        Assert.That(map.Cells[0].Center.X, Is.EqualTo(centers[1].X).Within(0.00001f));
        if (policy == EmptyCellPolicy.LeaveAsIs)
        {
            Assert.That(map.Cells[1].Site, Is.EqualTo(sites[1]));
            Assert.That(map.Cells[1].HexIndexes, Is.Empty);
        }
        Assert.Throws<InvalidOperationException>(() =>
            centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(2, emptyCellPolicy: EmptyCellPolicy.ThrowException)));
    }

    [Test]
    public void PartitionExtensions_WithRelaxation_AppliesEmptyPolicyOnlyAfterFinalAssignment(
        [Values] PartitionOverload overload, [Values] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[0], 1f) };

        var result = PartitionForOverload(overload, centers, sites, 1, policy);

        Assert.That(result.Cells, Has.Count.EqualTo(2));
        Assert.That(result.Assignments, Is.EqualTo(new int?[] { 1, 0, 0 }));
        Assert.That(result.Cells[0].Center.X, Is.EqualTo(centers[1].X).Within(0.00001f));
        Assert.That(result.Cells[1].Site, Is.EqualTo(sites[1]));
    }

    [Test]
    public void PartitionByVoronoi_WithRelaxationAndNoParticipation_AppliesEmptyCellPolicy(
        [Values] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology);
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(centers[0], 1f) };

        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() =>
                centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(2, emptyCellPolicy: policy)));
            return;
        }

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(2, emptyCellPolicy: policy));
        Assert.That(Enumerable.Range(0, 2).Select(i => map[i]), Is.All.Null);
        Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 1));
        if (policy == EmptyCellPolicy.LeaveAsIs)
            Assert.That(map.Cells[0].Site, Is.EqualTo(sites[0]));
    }

    [Test]
    public void PartitionByVoronoi_WithNegativeRelaxationIterations_Throws()
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true });
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(centers[0], 1f) };

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(-1)))!.ParamName,
            Is.EqualTo("relaxationIterations"));
    }

    [Test]
    public void Partition_WithNullableRegions_ExcludesNullHexesAndAssignsOnlyWithinRegion(
        [Values(Layout.OddR, Layout.EvenR, Layout.OddQ, Layout.EvenQ)] Layout layout,
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy emptyPolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(8, 1, new VectorXY(13f, -21f), 2f, layout));
        var values = new int?[] { -7, null, 0, 42, -7, null, int.MinValue, int.MaxValue };
        var regions = new HexMap<int?>(centers.Topology, values);
        var sites = new[] { new Site(centers[1], float.PositiveInfinity), new Site(centers[0], 1f),
            new Site(centers[2], 1f), new Site(centers[3], 1f), new Site(centers[6], 1f),
            new Site(centers[7], 1f), new Site(new PointXY(-1000f, -1000f), 1f) };

        var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy));
        var expectedSites = new int?[] { 1, null, 2, 3, 1, null, 4, 5 };

        Assert.Multiple(() =>
        {
            Assert.That(map.Cells, Has.Count.EqualTo(emptyPolicy == EmptyCellPolicy.Exclude ? 5 : 7));
            for (int i = 0; i < values.Length; i++)
            {
                Assert.That(map.Participates(i), Is.EqualTo(values[i].HasValue));
                if (expectedSites[i].HasValue)
                    Assert.That(map.Cells[map[i]!.Value].Site, Is.EqualTo(sites[expectedSites[i]!.Value]));
                else
                    Assert.That(map[i], Is.Null);
            }
            Assert.That(map.Cells.Select(cell => cell.Id), Is.EqualTo(Enumerable.Range(0, map.Cells.Count)));
        });

        if (emptyPolicy == EmptyCellPolicy.LeaveAsIs)
        {
            var defaultPolicy = centers.PartitionByVoronoi(regions, sites);
            Assert.That(Enumerable.Range(0, 8).Select(i => defaultPolicy[i]),
                Is.EqualTo(Enumerable.Range(0, 8).Select(i => map[i])));
        }
    }

    [Test]
    public void Partition_WithNullableRegionsAndExclaves_MatchesSeparateMasks(
        [Values] ExclavePolicy exclavePolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(11, 1, VectorXY.Zero, 1f, Layout.OddR));
        var values = new int?[] { -7, null, -7, -7, -7, 42, -7, null, -7, null, 42 };
        var regions = new HexMap<int?>(centers.Topology, values);
        var participation = new BoolHexMap(centers.Topology, values.Select(region => region.HasValue).ToArray());
        var separateRegions = new IntHexMap(centers.Topology, values.Select(region => region.GetValueOrDefault()).ToArray());
        var sites = new[] { new Site(centers[0], 10f), new Site(centers[4], 1f), new Site(centers[5], 1f) };
        var expected = centers.PartitionByVoronoi(participation, separateRegions, sites, new VoronoiHexPartitionParameters(exclavePolicy: exclavePolicy));

        var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(exclavePolicy: exclavePolicy));

        Assert.Multiple(() =>
        {
            Assert.That(Enumerable.Range(0, 11).Select(i => map[i]),
                Is.EqualTo(Enumerable.Range(0, 11).Select(i => expected[i])));
            Assert.That(map.Cells.Select(cell => cell.Site), Is.EqualTo(expected.Cells.Select(cell => cell.Site)));
            Assert.That(map.Cells, Has.Count.EqualTo(exclavePolicy == ExclavePolicy.LeaveAsIs ? 3 : 6));
            if (exclavePolicy == ExclavePolicy.ReassignToClosestCell)
            {
                Assert.That(map[2], Is.EqualTo(1));
                Assert.That(map[3], Is.EqualTo(1));
                Assert.That(map.Cells[3].Center, Is.EqualTo(centers[6]));
                Assert.That(map.Cells[4].Center, Is.EqualTo(centers[8]));
                Assert.That(map.Cells[5].Center, Is.EqualTo(centers[10]));
            }
        });
    }

    [Test]
    public void Partition_WithNullRegionsOnly_AppliesEmptyCellPolicy(
        [Values] EmptyCellPolicy emptyPolicy, [Values] ExclavePolicy exclavePolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };
        var regions = new HexMap<int?>(centers.Topology);

        if (emptyPolicy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() =>
                centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy, exclavePolicy: exclavePolicy)));
            return;
        }

        var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy, exclavePolicy: exclavePolicy));
        Assert.That(map[0], Is.Null);
        Assert.That(map[1], Is.Null);
        Assert.That(map.Cells, Has.Count.EqualTo(emptyPolicy == EmptyCellPolicy.Exclude ? 0 : 1));
    }

    [TestCase(0, 3)]
    [TestCase(3, 0)]
    public void Partition_WithEmptyNullableRegionMap_ReturnsEmptyMaskedPartition(int width, int height)
    {
        var centers = new HexCenterMap(new HexMapGeometry(width, height, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new HexMap<int?>(centers.Topology);
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var map = centers.PartitionByVoronoi(regions, sites,
            new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude, exclavePolicy: ExclavePolicy.ReassignToClosestCell));

        Assert.That(map.Cells, Is.Empty);
        Assert.That(map.Topology.Count, Is.Zero);
    }

    [Test]
    public void Partition_WithNullableRegionMask_SkipsExcludedNonFiniteCenter([Values] ExclavePolicy exclavePolicy)
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(float.MaxValue, 0f),
            (float.MaxValue / 4f).ConvertHexApothemToRadius(), Layout.OddR);
        var centers = new HexCenterMap(geometry);
        var regions = new HexMap<int?>(centers.Topology, new int?[] { -7, null });
        var sites = new[] { new Site(centers[0], 1f) };

        var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(exclavePolicy: exclavePolicy));

        Assert.That(map[0], Is.EqualTo(0));
        Assert.That(map[1], Is.Null);
        regions[1] = -7;
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(exclavePolicy: exclavePolicy)))!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void Partition_WithNullableRegionMask_ValidatesInputsAndMissingRegionSites()
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };
        var regions = new HexMap<int?>(centers.Topology, new int?[] { null, 0 });

        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            centers.PartitionByVoronoi((IHexMap<int?>)null!, sites))!.ParamName, Is.EqualTo("regionsMask"));
        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            ((HexCenterMap)null!).PartitionByVoronoi(regions, sites))!.ParamName, Is.EqualTo("hexCenters"));
        Assert.That(Assert.Throws<ArgumentException>(() =>
            centers.PartitionByVoronoi(new HexMap<int?>(new HexMapTopology(2, 1, Layout.EvenR)), sites))!.ParamName,
            Is.EqualTo("regionsMask"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(exclavePolicy: (ExclavePolicy)123)))!.ParamName, Is.EqualTo("exclavePolicy"));
        Assert.Throws<InvalidOperationException>(() => centers.PartitionByVoronoi(regions, sites));
        regions[0] = -7;
        Assert.Throws<InvalidOperationException>(() => centers.PartitionByVoronoi(regions, sites));
    }

    [Test]
    public void Partition_WithParticipationExclaves_GrowsThroughMultipleHexes(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude, EmptyCellPolicy.ThrowException)] EmptyCellPolicy emptyPolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var mask = new BoolHexMap(centers.Topology, new[] { true, false, true, true, true });
        var sites = new[] { new Site(centers[0], 10f), new Site(centers[4], 1f) };

        var unchanged = centers.PartitionByVoronoi(mask, sites);
        var map = centers.PartitionByVoronoi(mask, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy, exclavePolicy: ExclavePolicy.ReassignToClosestCell));

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
        [Values] bool combinedMasks)
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(centers.Topology, new[] { -7, -7, 42, -7, -7, -7, -7 });
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 7).ToArray());
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[6], 1f), new Site(centers[2], 1f) };
        int[] assignments;
        IReadOnlyList<VoronoiHexPartitionCell> cells;
        if (combinedMasks)
        {
            var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.LeaveAsIs,
                    exclavePolicy: ExclavePolicy.ReassignToClosestCell));
            assignments = Enumerable.Range(0, 7).Select(i => map[i]!.Value).ToArray();
            cells = map.Cells;
        }
        else
        {
            var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(exclavePolicy: ExclavePolicy.ReassignToClosestCell));
            assignments = Enumerable.Range(0, 7).Select(i => map[i]).ToArray();
            cells = map.Cells;
        }

        Assert.That(assignments, Is.EqualTo(new[] { 0, 0, 2, 1, 1, 1, 1 }));
        Assert.That(cells, Has.Count.EqualTo(3));
        Assert.That(regions[3], Is.EqualTo(-7));
    }

    [Test]
    public void Partition_WithBlockedRegionExclaves_CreatesOneCellPerComponent(
        [Values] bool combinedMasks,
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy emptyPolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(centers.Topology, new[] { 0, 1, 0, 0, 1, 0, 0 });
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 7).ToArray());
        var sites = new[] { new Site(centers[0], 3f), new Site(centers[1], 1f), new Site(centers[4], 1f),
            new Site(new PointXY(-100f, -100f), 1f) };
        int[] assignments;
        IReadOnlyList<VoronoiHexPartitionCell> cells;
        if (combinedMasks)
        {
            var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy,
                    exclavePolicy: ExclavePolicy.ReassignToClosestCell));
            assignments = Enumerable.Range(0, 7).Select(i => map[i]!.Value).ToArray();
            cells = map.Cells;
        }
        else
        {
            var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: emptyPolicy, exclavePolicy: ExclavePolicy.ReassignToClosestCell));
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

        var map = centers.PartitionByVoronoi(mask, sites, new VoronoiHexPartitionParameters(exclavePolicy: ExclavePolicy.ReassignToClosestCell));

        Assert.Multiple(() =>
        {
            Assert.That(Enumerable.Range(0, 7).Select(i => map[i]), Is.EqualTo(new int?[] { 0, 0, null, 1, 1, null, 2 }));
            Assert.That(map.Cells[1].Center, Is.EqualTo(centers[3]));
            Assert.That(map.Cells[2].Center, Is.EqualTo(centers[6]));
            Assert.That(map.Cells, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Partition_WithInvalidExclavePolicy_Throws(
        [Values] PartitionOverload overload, [Values(-1, 123)] int invalidPolicy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            PartitionForOverload(overload, centers, sites, exclavePolicy: (ExclavePolicy)invalidPolicy))!.ParamName,
            Is.EqualTo("exclavePolicy"));
    }

    [Test]
    public void Partition_WithExclave_SelectsClosestAdjacentCenterWithoutWeights()
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(centers.Topology, new[] { 0, 1, 0, 0, 0 });
        var fartherPosition = new PointXY(centers[4].X + (centers[4].X - centers[3].X) * 0.25f, centers[4].Y);
        var sites = new[] { new Site(centers[0], 10f), new Site(centers[2], 0.01f),
            new Site(fartherPosition, 1f), new Site(centers[1], 1f) };

        var unchanged = centers.PartitionByVoronoi(regions, sites);
        var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(exclavePolicy: ExclavePolicy.ReassignToClosestCell));

        Assert.That(unchanged[3], Is.EqualTo(0));
        Assert.That(map[3], Is.EqualTo(1));
    }

    [Test]
    public void Partition_WithMaskedSite_KeepsComponentClosestToSite()
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var mask = new BoolHexMap(centers.Topology, new[] { true, true, false, false, true });
        var sites = new[] { new Site(centers[3], 1f) };

        var map = centers.PartitionByVoronoi(mask, sites, new VoronoiHexPartitionParameters(exclavePolicy: ExclavePolicy.ReassignToClosestCell));

        Assert.That(map[4], Is.EqualTo(0));
        Assert.That(map[0], Is.EqualTo(1));
        Assert.That(map[1], Is.EqualTo(1));
        Assert.That(map.Cells[1].Center, Is.EqualTo(centers[1]));
    }

    [Test]
    public void Partition_WithBothMasks_RestrictsSitesByRegionAndSkipsExcludedRegions(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(4, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { false, true, true, false });
        var regions = new IntHexMap(centers.Topology, new[] { -7, 0, -7, 42 });
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[1], 1f), new Site(centers[3], 1f) };

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
    public void PartitionByVoronoi_WithBothMasksAndNoParticipation_AppliesEmptyCellPolicy(EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology);
        var regions = new IntHexMap(centers.Topology);
        var sites = new[] { new Site(new PointXY(100f, 100f), 1f) };

        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() => centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy)));
            return;
        }

        var map = centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));
        Assert.That(map[0], Is.Null);
        Assert.That(map.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 1));
    }

    [Test]
    public void PartitionByVoronoi_WithBothMasks_ValidatesMasksAndMissingRegionSites()
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participation = new BoolHexMap(centers.Topology, new[] { true, true });
        var regions = new IntHexMap(centers.Topology, new[] { 0, 1 });
        var sites = new[] { new Site(centers[0], 1f) };
        var differentTopology = new HexMapTopology(1, 1, Layout.OddR);

        Assert.Throws<ArgumentNullException>(() => centers.PartitionByVoronoi(null!, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.LeaveAsIs)));
        Assert.Throws<ArgumentNullException>(() => centers.PartitionByVoronoi(participation, null!, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.LeaveAsIs)));
        Assert.Throws<ArgumentException>(() => centers.PartitionByVoronoi(new BoolHexMap(differentTopology), regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.LeaveAsIs)));
        Assert.Throws<ArgumentException>(() => centers.PartitionByVoronoi(participation, new IntHexMap(differentTopology), sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.LeaveAsIs)));
        Assert.Throws<InvalidOperationException>(() => centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.LeaveAsIs)));
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

        var map = hexCenters.PartitionByVoronoi(sites);

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
    public void Partition_ReturnsHexPartitionMapOfCellIds()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.EvenQ));

        var map = hexCenters.PartitionByVoronoi(sites);
        IHexMap<int> hexMap = map;

        HexPartitionMap<VoronoiHexPartitionCell> partitionMap = map;
        Assert.That(map, Is.InstanceOf<HexMap<int>>());
        Assert.That(partitionMap.Cells, Is.SameAs(map.Cells));
        Assert.That(map.Centers, Is.SameAs(hexCenters));
        Assert.That(hexMap.Topology.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
        Assert.That(hexMap.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Cells, Has.Count.EqualTo(1));
        Assert.That(map.Cells[0].HexIndexes, Has.Count.EqualTo(4));
    }

    [TestCase(3, 2, Layout.OddR)]
    [TestCase(3, 2, Layout.EvenR)]
    [TestCase(3, 2, Layout.OddQ)]
    [TestCase(3, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void ImplicitConversion_ToSpatialHexMap_SharesAssignmentsAndPreservesGeometry(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 2f, layout);
        var centers = new HexCenterMap(geometry);
        var sites = new[] { new Site(new PointXY(10f, -20f), 1f) };
        var map = centers.PartitionByVoronoi(sites);
        var cells = map.Cells;
        var originalIndexes = cells[0].HexIndexes.ToArray();
        var originalAssignments = Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();
        var independentCopy = map.ToMutableHexMap();

        SpatialHexMap<int> facade = map;

        Assert.Multiple(() =>
        {
            Assert.That(facade, Is.Not.SameAs(map));
            Assert.That(facade.Geometry, Is.EqualTo(geometry));
            Assert.That(facade.Topology, Is.EqualTo(map.Topology));
            Assert.That(Enumerable.Range(0, facade.Topology.Count).Select(index => facade[index]),
                Is.EqualTo(originalAssignments));
        });

        if (map.Topology.Count == 0)
            return;

        facade[new VectorXYInt(0, 0)] = 17;
        Assert.That(map[0], Is.EqualTo(17));
        map[1] = 23;
        Assert.That(facade[1], Is.EqualTo(23));
        Assert.That(map.Cells, Is.SameAs(cells));
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(originalIndexes));
        Assert.That(Enumerable.Range(0, independentCopy.Topology.Count).Select(index => independentCopy[index]),
            Is.EqualTo(originalAssignments));
    }

    [Test]
    public void ImplicitConversion_ToSpatialHexMap_WhenPartitionIsNull_ReturnsNull()
    {
        VoronoiHexPartitionMap? map = null;

        SpatialHexMap<int>? facade = map;

        Assert.That(facade, Is.Null);
    }

    [Test]
    public void PartitionByVoronoi_PartitionsHexCenters()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));

        var map = hexCenters.PartitionByVoronoi(sites);
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

        PartialVoronoiHexPartitionMap map = hexCenters.PartitionByVoronoi(participationMask, sites);
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

    [TestCase(3, 2, Layout.OddR)]
    [TestCase(3, 2, Layout.EvenR)]
    [TestCase(3, 2, Layout.OddQ)]
    [TestCase(3, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void PartialImplicitConversion_ToSpatialHexMap_SharesNullableAssignmentsAndPreservesGeometry(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 2f, layout);
        var centers = new HexCenterMap(geometry);
        var sites = new[] { new Site(new PointXY(10f, -20f), 1f) };
        var participation = Enumerable.Range(0, geometry.Topology.Count).Select(index => index % 2 == 0).ToArray();
        var mask = new BoolHexMap(geometry.Topology, participation);
        var map = centers.PartitionByVoronoi(mask, sites);
        var cells = map.Cells;
        var originalIndexes = cells.Select(cell => cell.HexIndexes.ToArray()).ToArray();
        var originalAssignments = Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();
        var independentCopy = map.ToMutableHexMap();

        SpatialHexMap<int?> facade = map;

        Assert.Multiple(() =>
        {
            Assert.That(facade, Is.Not.SameAs(map));
            Assert.That(facade.Geometry, Is.EqualTo(geometry));
            Assert.That(facade.Topology, Is.EqualTo(map.Topology));
            Assert.That(Enumerable.Range(0, facade.Topology.Count).Select(index => facade[index]),
                Is.EqualTo(originalAssignments));
        });

        if (map.Topology.Count == 0)
            return;

        Assert.That(facade[0], Is.Not.Null);
        Assert.That(facade[1], Is.Null);
        facade[new VectorXYInt(0, 0)] = null;
        Assert.That(map[0], Is.Null);
        map[1] = 23;
        Assert.That(facade[1], Is.EqualTo(23));
        map[2] = null;
        Assert.That(facade[2], Is.Null);
        Assert.That(map.Cells, Is.SameAs(cells));
        Assert.That(map.Cells.Select(cell => cell.HexIndexes.ToArray()), Is.EqualTo(originalIndexes));
        Assert.That(Enumerable.Range(0, map.Topology.Count).Select(map.Participates), Is.EqualTo(participation));
        Assert.That(Enumerable.Range(0, independentCopy.Topology.Count).Select(index => independentCopy[index]),
            Is.EqualTo(originalAssignments));
    }

    [Test]
    public void PartialImplicitConversion_ToSpatialHexMap_WhenPartitionIsNull_ReturnsNull()
    {
        PartialVoronoiHexPartitionMap? map = null;

        SpatialHexMap<int?>? facade = map;

        Assert.That(facade, Is.Null);
    }

    [Test]
    public void PartitionByVoronoi_WithParticipationMask_PartitionsOnlyParticipatingHexCenters()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, false, true });

        PartialVoronoiHexPartitionMap map = hexCenters.PartitionByVoronoi(participationMask, sites);

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

        PartialVoronoiHexPartitionMap map = hexCenters.PartitionByVoronoi(participationMask, sites);

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
        [Values(Layout.OddR, Layout.EvenR, Layout.OddQ, Layout.EvenQ)] Layout layout)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 2, new VectorXY(12f, -8f), 2f, layout));
        var regions = new IntHexMap(hexCenters.Topology, new[] { -7, 0, 42, 42, -7, 0 });
        var sites = new[]
        {
            new Site(hexCenters[0] + new VectorXY(0.1f, -0.1f), 1f),
            new Site(hexCenters[1] + new VectorXY(-0.1f, 0.1f), float.PositiveInfinity),
            new Site(hexCenters[2] + new VectorXY(0.1f, 0.1f), 1f)
        };

        var map = hexCenters.PartitionByVoronoi(regions, sites);

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

        var expected = hexCenters.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));
        var actual = hexCenters.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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

        var map = hexCenters.PartitionByVoronoi(regions, sites);

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

        var map = hexCenters.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
        var sites = new[] { new Site(hexCenters[0], 1f) };

        var exception = Assert.Throws<InvalidOperationException>(() => hexCenters.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy)));

        Assert.That(exception!.Message, Does.Contain("71"));
    }

    [Test]
    public void Partition_WithRegionMask_WhenRegionHasOnlyZeroWeightSiteAwayFromHex_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology, new[] { 10, 71, 71 });
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 0f) };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            hexCenters.PartitionByVoronoi(regions, sites));

        Assert.That(exception!.Message, Does.Contain("71"));
    }

    [Test]
    public void Partition_WithRegionMask_AppliesEmptyCellPolicyToDuplicateAndOutsideSites(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude, EmptyCellPolicy.ThrowException)] EmptyCellPolicy policy)
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
        VoronoiHexPartitionMap Partition() => hexCenters.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() => hexCenters.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy)));
            return;
        }

        var map = hexCenters.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
        var sites = new[] { new Site(hexCenters[0], 1f) };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => hexCenters.PartitionByVoronoi(regions, sites));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void Partition_WithRegionMask_WhenEitherMapIsNull_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(hexCenters.Topology);
        var sites = new[] { new Site(hexCenters[0], 1f) };

        Assert.That(Assert.Throws<ArgumentNullException>(() => ((HexCenterMap)null!).PartitionByVoronoi(regions, sites))!.ParamName,
            Is.EqualTo("hexCenters"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => hexCenters.PartitionByVoronoi((IHexMap<int>)null!, sites))!.ParamName,
            Is.EqualTo("regionsMask"));
    }

    [TestCase(1, 2, Layout.OddR)]
    [TestCase(2, 1, Layout.EvenR)]
    public void Partition_WithRegionMask_WhenTopologyDiffers_Throws(int width, int height, Layout layout)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new IntHexMap(new HexMapTopology(width, height, layout));
        var sites = new[] { new Site(hexCenters[0], 1f) };

        var exception = Assert.Throws<ArgumentException>(() => hexCenters.PartitionByVoronoi(regions, sites));

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
        PartialVoronoiHexPartitionMap map = hexCenters.PartitionByVoronoi(participationMask, sites);

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

        PartialHexPartitionMap<VoronoiHexPartitionCell> partitionMap = map;
        partitionMap[1] = map.Cells[1].Id;
        var updatedCopy = map.ToMutableHexMap();
        map[1] = null;
        Assert.That(updatedCopy[1], Is.EqualTo(map.Cells[1].Id));
        Assert.That(map.Participates(1), Is.False);
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
        var map = hexCenters.PartitionByVoronoi(sites);

        HexMap<int> mutableMap = map.ToMutableHexMap();
        mutableMap[0] = map.Cells[1].Id;

        Assert.Multiple(() =>
        {
            Assert.That(mutableMap[0], Is.EqualTo(map.Cells[1].Id));
            Assert.That(map[0], Is.EqualTo(map.Cells[0].Id));
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));
            Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
        });

        HexPartitionMap<VoronoiHexPartitionCell> partitionMap = map;
        partitionMap[0] = map.Cells[1].Id;
        var updatedCopy = map.ToMutableHexMap();
        map[0] = map.Cells[0].Id;
        Assert.That(updatedCopy[0], Is.EqualTo(map.Cells[1].Id));
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

        var map = hexCenters.PartitionByVoronoi(sites);

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

        var map = hexCenters.PartitionByVoronoi(sites);

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

        var map = hexCenters.PartitionByVoronoi(sites);

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

        var map = hexCenters.PartitionByVoronoi(sites);

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

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            hexCenters.PartitionByVoronoi(sites));

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

        PartialVoronoiHexPartitionMap map = hexCenters.PartitionByVoronoi(participationMask, sites);

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

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            hexCenters.PartitionByVoronoi(participationMask, sites));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void VoronoiHexPartitionCell_WhenSiteIndexIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VoronoiHexPartitionCell(-1, new Site(new PointXY(0f, 0f), 1f), Array.Empty<VectorXYInt>()));
    }

    [Test]
    public void VoronoiHexPartitionCell_WhenHexIndexesIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new VoronoiHexPartitionCell(0, new Site(new PointXY(0f, 0f), 1f), null!));
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

        var map = hexCenters.PartitionByVoronoi(sites);

        Assert.That(map.Cells, Has.Count.EqualTo(2));
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0) }));
        Assert.That(map.Cells[1].HexIndexes, Is.Empty);
    }

    [Test]
    public void Partition_WithEmptyCellPolicy_HandlesEmptyCellsAndAssignments(
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy)
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

        var map = hexCenters.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
        [Values(EmptyCellPolicy.LeaveAsIs, EmptyCellPolicy.Exclude)] EmptyCellPolicy policy)
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

        var map = hexCenters.PartitionByVoronoi(participationMask, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
    public void PartitionByVoronoi_WhenPolicyIsThrowExceptionAndCellIsEmpty_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[]
        {
            new Site(hexCenters[0], 1f),
            new Site(new PointXY(100f, 0f), 1f)
        };

        Assert.Throws<InvalidOperationException>(() =>
            hexCenters.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.ThrowException)));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void PartitionByVoronoi_WhenPolicyIsThrowExceptionAndMaskEmptiesCell_Throws(bool excludeAll)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 1f) };
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { !excludeAll, false });

        Assert.Throws<InvalidOperationException>(() =>
            hexCenters.PartitionByVoronoi(participationMask, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.ThrowException)));
    }

    [TestCase(EmptyCellPolicy.LeaveAsIs, 2)]
    [TestCase(EmptyCellPolicy.Exclude, 0)]
    public void PartitionByVoronoi_WithEmptyParticipationMask_AppliesEmptyCellPolicy(
        EmptyCellPolicy policy, int expectedCellCount)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 1f) };
        var participationMask = new BoolHexMap(hexCenters.Topology);

        var map = hexCenters.PartitionByVoronoi(participationMask, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
    public void PartitionByVoronoi_WhenAllCellsArePopulated_PreservesCells(EmptyCellPolicy policy)
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(hexCenters[0], 1f), new Site(hexCenters[1], 1f) };
        var participationMask = new BoolHexMap(hexCenters.Topology, new[] { true, true });

        var map = hexCenters.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));
        var maskedMap = hexCenters.PartitionByVoronoi(participationMask, sites, new VoronoiHexPartitionParameters(emptyCellPolicy: policy));

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
    public void PartitionExtensions_WhenSitesIsNull_Throws([Values] PartitionOverload overload)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));

        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            PartitionForOverload(overload, centers, null!))!.ParamName, Is.EqualTo("sites"));
    }

    [Test]
    public void PartitionExtensions_WhenSitesIsEmpty_Throws([Values] PartitionOverload overload)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            PartitionForOverload(overload, centers, Array.Empty<Site>()))!.ParamName, Is.EqualTo("sites"));
    }

    [Test]
    public void PartitionExtensions_WhenAllSiteWeightsAreZero_Throws(
        [Values] PartitionOverload overload, [Values] bool useDefaultSite)
    {
        var centers = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = useDefaultSite ? new Site[1] : new[]
        {
            new Site(new PointXY(0f, 0f), 0f),
            new Site(new PointXY(1f, 0f), 0f)
        };

        var exception = Assert.Throws<ArgumentException>(() => PartitionForOverload(overload, centers, sites));

        Assert.That(exception!.ParamName, Is.EqualTo("sites"));
    }

    [Test]
    public void PartitionExtensions_WhenHexCenterMapIsNull_Throws([Values] PartitionOverload overload)
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        Assert.That(Assert.Throws<ArgumentNullException>(() =>
            PartitionForOverload(overload, null!, sites))!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void Partition_WhenParticipationMaskIsNull_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            hexCenters.PartitionByVoronoi((IHexMap<bool>)null!, sites));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }

    [Test]
    public void Partition_WhenParticipationMaskHasDifferentTopology_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var participationMask = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentException>(() =>
            hexCenters.PartitionByVoronoi(participationMask, sites));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }

    [Test]
    public void PartitionByVoronoi_WhenHexCenterMapIsNull_Throws()
    {
        HexCenterMap hexCenters = null!;
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            hexCenters.PartitionByVoronoi(sites));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }

    [Test]
    public void PartitionByVoronoi_WhenParticipationMaskIsNull_Throws()
    {
        var hexCenters = new HexCenterMap(new HexMapGeometry(1, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            hexCenters.PartitionByVoronoi((IHexMap<bool>)null!, sites));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }

    [Test]
    public void PartitionExtensions_WithEmptyDimensions_AppliesEmptyPolicyAfterRelaxation(
        [Values] PartitionOverload overload, [Values] bool emptyWidth, [Values] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(
            emptyWidth ? 0 : 3, emptyWidth ? 3 : 0, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f), new Site(new PointXY(1f, 0f), 2f) };

        if (policy == EmptyCellPolicy.ThrowException)
        {
            Assert.Throws<InvalidOperationException>(() => PartitionForOverload(overload, centers, sites,
                relaxationIterations: 2, emptyCellPolicy: policy, exclavePolicy: ExclavePolicy.ReassignToClosestCell));
            return;
        }

        var result = PartitionForOverload(overload, centers, sites,
            relaxationIterations: 2, emptyCellPolicy: policy, exclavePolicy: ExclavePolicy.ReassignToClosestCell);

        Assert.That(result.Assignments, Is.Empty);
        Assert.That(result.Cells, Has.Count.EqualTo(policy == EmptyCellPolicy.Exclude ? 0 : 2));
        Assert.That(result.Cells.All(cell => cell.HexIndexes.Count == 0), Is.True);
        if (policy == EmptyCellPolicy.LeaveAsIs)
            Assert.That(result.Cells.Select(cell => cell.Site), Is.EqualTo(sites));
    }

    [Test]
    public void PartitionExtensions_WithEqualDistances_PrefersFirstSite(
        [Values] PartitionOverload overload, [Values(1f, float.PositiveInfinity)] float weight,
        [Values] bool reverseSites)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], weight), new Site(centers[2], weight) };
        if (reverseSites)
            Array.Reverse(sites);

        var result = PartitionForOverload(overload, centers, sites);

        Assert.That(result.Assignments, Is.EqualTo(reverseSites ? new int?[] { 1, 0, 0 } : new int?[] { 0, 0, 1 }));
        Assert.That(result.Cells[0].Site, Is.EqualTo(sites[0]));
        Assert.That(result.Cells[0].HexIndexes, Does.Contain(new VectorXYInt(1, 0)));
    }

    [Test]
    public void PartitionExtensions_WithInfiniteWeights_PrefersNearestInfiniteSiteAndExactFiniteSite(
        [Values] PartitionOverload overload, [Values(0f, 1f)] float finiteWeight)
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[]
        {
            new Site(centers[0], float.PositiveInfinity),
            new Site(centers[4], float.PositiveInfinity),
            new Site(centers[2], finiteWeight)
        };

        var result = PartitionForOverload(overload, centers, sites, emptyCellPolicy: EmptyCellPolicy.ThrowException);

        Assert.That(result.Assignments, Is.EqualTo(new int?[] { 0, 0, 2, 1, 1 }));
        Assert.That(result.Cells.Select(cell => cell.Site), Is.EqualTo(sites));
    }

    [Test]
    public void PartitionExtensions_WithReadOnlySites_RelaxesWithoutChangingInput(
        [Values] PartitionOverload overload)
    {
        var centers = new HexCenterMap(new HexMapGeometry(9, 1, new VectorXY(13f, -21f), 1f, Layout.OddR));
        var original = new[] { new Site(centers[0], 2f), new Site(centers[2], 2f) };
        var sites = Array.AsReadOnly(original.ToArray());

        var result = PartitionForOverload(overload, centers, sites, relaxationIterations: 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.Assignments, Is.EqualTo(new int?[] { 0, 0, 0, 0, 1, 1, 1, 1, 1 }));
            Assert.That(result.Cells[0].Center.X, Is.EqualTo(centers[1].X).Within(0.00001f));
            Assert.That(result.Cells[1].Center.X, Is.EqualTo((centers[5].X + centers[6].X) / 2f).Within(0.00001f));
            Assert.That(result.Cells.Select(cell => cell.Center.Y), Is.All.EqualTo(-21f));
            Assert.That(result.Cells.Select(cell => cell.Site.Weight), Is.EqualTo(new[] { 2f, 2f }));
            Assert.That(sites, Is.EqualTo(original));
            Assert.That(result.Cells[0].HexIndexes, Is.EqualTo(Enumerable.Range(0, 4).Select(x => new VectorXYInt(x, 0))));
            Assert.That(result.Cells[1].HexIndexes, Is.EqualTo(Enumerable.Range(4, 5).Select(x => new VectorXYInt(x, 0))));
        });
    }

    [Test]
    public void PartitionExtensions_WhenSiteListChanges_PreservesPreviousCellsAndUsesNewSites(
        [Values] PartitionOverload overload)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new List<Site> { new Site(centers[0], 1f), new Site(centers[2], 1f) };
        var original = sites.ToArray();
        var first = PartitionForOverload(overload, centers, sites);
        sites.Reverse();
        var second = PartitionForOverload(overload, centers, sites);

        Assert.Multiple(() =>
        {
            Assert.That(first.Assignments, Is.EqualTo(new int?[] { 0, 0, 1 }));
            Assert.That(first.Cells.Select(cell => cell.Site), Is.EqualTo(original));
            Assert.That(first.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));
            Assert.That(second.Assignments, Is.EqualTo(new int?[] { 1, 0, 0 }));
            Assert.That(second.Cells.Select(cell => cell.Site), Is.EqualTo(sites));
            Assert.That(second.Cells, Is.Not.SameAs(first.Cells));
            Assert.That(second.Cells[0], Is.Not.SameAs(first.Cells[0]));
        });
    }

    [Test]
    public void PartitionExtensions_WhenCalledRepeatedly_ReturnIndependentMutableMaps()
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f), new Site(centers[2], 1f) };
        var participation = new BoolHexMap(centers.Topology, new[] { true, true, true });
        var regions = new IntHexMap(centers.Topology);
        var nullableRegions = new HexMap<int?>(centers.Topology, new int?[] { 0, 0, 0 });

        var fullFactories = new Func<VoronoiHexPartitionMap>[]
        {
            () => centers.PartitionByVoronoi(sites),
            () => centers.PartitionByVoronoi(regions, sites)
        };
        foreach (var create in fullFactories)
        {
            var first = create();
            var second = create();
            first[0] = 1;
            Assert.That(second[0], Is.EqualTo(0));
            Assert.That(first.Cells, Is.Not.SameAs(second.Cells));
            Assert.That(first.Cells[0].HexIndexes, Does.Contain(new VectorXYInt(0, 0)));
            Assert.That(second.Cells[0].HexIndexes, Does.Contain(new VectorXYInt(0, 0)));
        }

        var partialFactories = new Func<PartialVoronoiHexPartitionMap>[]
        {
            () => centers.PartitionByVoronoi(participation, sites),
            () => centers.PartitionByVoronoi(nullableRegions, sites),
            () => centers.PartitionByVoronoi(participation, regions, sites)
        };
        foreach (var create in partialFactories)
        {
            var first = create();
            var second = create();
            first[0] = null;
            Assert.That(second[0], Is.EqualTo(0));
            Assert.That(second.Participates(0), Is.True);
            Assert.That(first.Cells, Is.Not.SameAs(second.Cells));
            Assert.That(first.Cells[0].HexIndexes, Does.Contain(new VectorXYInt(0, 0)));
            Assert.That(second.Cells[0].HexIndexes, Does.Contain(new VectorXYInt(0, 0)));
        }

        Assert.That(Enumerable.Range(0, 3).Select(i => participation[i]), Is.All.True);
        Assert.That(Enumerable.Range(0, 3).Select(i => regions[i]), Is.All.EqualTo(0));
        Assert.That(Enumerable.Range(0, 3).Select(i => nullableRegions[i]), Is.All.EqualTo(0));
    }

    [Test]
    public void PartitionExtensions_WithDefaultExclavePolicy_RetainsDisconnectedAssignments()
    {
        var centers = new HexCenterMap(new HexMapGeometry(7, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 10f), new Site(centers[3], 1f), new Site(centers[6], 1f) };
        var participation = new BoolHexMap(centers.Topology, Enumerable.Repeat(true, 7).ToArray());
        var regions = new IntHexMap(centers.Topology);
        var nullableRegions = new HexMap<int?>(centers.Topology, Enumerable.Repeat((int?)0, 7).ToArray());
        var expected = new int?[] { 0, 0, 0, 1, 0, 0, 2 };

        foreach (var map in new[]
        {
            centers.PartitionByVoronoi(sites),
            centers.PartitionByVoronoi(regions, sites)
        })
            Assert.That(Enumerable.Range(0, 7).Select(i => (int?)map[i]), Is.EqualTo(expected));

        foreach (var map in new[]
        {
            centers.PartitionByVoronoi(participation, sites),
            centers.PartitionByVoronoi(nullableRegions, sites),
            centers.PartitionByVoronoi(participation, regions, sites)
        })
            Assert.That(Enumerable.Range(0, 7).Select(i => map[i]), Is.EqualTo(expected));

        foreach (PartitionOverload overload in Enum.GetValues<PartitionOverload>())
            Assert.That(PartitionForOverload(overload, centers, sites,
                exclavePolicy: ExclavePolicy.ReassignToClosestCell).Assignments, Is.EqualTo(new int?[] { 0, 0, 0, 1, 1, 2, 2 }),
                $"Overload {overload}");
    }

    [Test]
    public void PartitionByVoronoi_WithNullableRegionsAndRelaxation_RetainsRegionWhenCentroidIsExcluded(
        [Values(1, 3)] int iterations, [Values] EmptyCellPolicy policy)
    {
        var centers = new HexCenterMap(new HexMapGeometry(5, 1, VectorXY.Zero, 1f, Layout.OddR));
        var regions = new HexMap<int?>(centers.Topology, new int?[] { -7, 42, null, 42, -7 });
        var sites = new[] { new Site(centers[0], 3f), new Site(centers[1], 1f) };
        var original = sites.ToArray();

        var map = centers.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(iterations, emptyCellPolicy: policy));

        Assert.Multiple(() =>
        {
            Assert.That(Enumerable.Range(0, 5).Select(i => map[i]), Is.EqualTo(new int?[] { 0, 1, null, 1, 0 }));
            Assert.That(map.Cells, Has.Count.EqualTo(2));
            Assert.That(map.Cells.Select(cell => cell.Center.X), Is.All.EqualTo(centers[2].X).Within(0.00001f));
            Assert.That(map.Cells.Select(cell => cell.Site.Weight), Is.EqualTo(new[] { 3f, 1f }));
            Assert.That(map.Participates(2), Is.False);
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(4, 0) }));
            Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(1, 0), new VectorXYInt(3, 0) }));
            Assert.That(Enumerable.Range(0, 5).Select(i => regions[i]), Is.EqualTo(new int?[] { -7, 42, null, 42, -7 }));
            Assert.That(sites, Is.EqualTo(original));
        });
    }

    [Test]
    public void PartitionExtensions_WithNonFiniteParticipatingCenter_Throws(
        [Values] PartitionOverload overload,
        [Values(float.PositiveInfinity, float.NegativeInfinity)] float coordinate,
        [Values] bool invalidY)
    {
        var centers = new HexCenterMap(new HexMapGeometry(2, 1, VectorXY.Zero, 1f, Layout.OddR));
        var sites = new[] { new Site(centers[0], 1f) };
        centers[1] = invalidY ? new PointXY(centers[1].X, coordinate) : new PointXY(coordinate, centers[1].Y);

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            PartitionForOverload(overload, centers, sites, relaxationIterations: 1))!.ParamName,
            Is.EqualTo("hexCenters"));
    }

    [Test]
    public void PartitionExtensions_WithZeroWeightSite_UsesGeometryTolerance(
        [Values] PartitionOverload overload, [Values] bool withinTolerance)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        float offset = GeometryConstants.GeometryEpsilon * (withinTolerance ? 0.5f : 2f);
        var sites = new[] { new Site(new PointXY(offset, 0f), 0f), new Site(centers[2], 1f) };

        var result = PartitionForOverload(overload, centers, sites);

        Assert.That(result.Assignments, Is.EqualTo(new int?[] { withinTolerance ? 0 : 1, 1, 1 }));
        Assert.That(result.Cells[0].HexIndexes, Has.Count.EqualTo(withinTolerance ? 1 : 0));
        Assert.That(result.Cells.Select(cell => cell.Site), Is.EqualTo(sites));
    }

    [Test]
    public void PartitionByVoronoi_WithRelaxation_SkipsNonFiniteExcludedCenterEvenWhenCentroidMovesIntoIt(
        [Values(1, 3)] int iterations)
    {
        var centers = new HexCenterMap(new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR));
        var expectedCentroid = centers[1];
        var sites = new[] { new Site(centers[0], 3f) };
        centers[1] = new PointXY(float.NegativeInfinity, float.PositiveInfinity);
        var participation = new BoolHexMap(centers.Topology, new[] { true, false, true });
        var regions = new IntHexMap(centers.Topology, new[] { -7, 42, -7 });
        var nullableRegions = new HexMap<int?>(centers.Topology, new int?[] { -7, null, -7 });

        var maps = new[]
        {
            centers.PartitionByVoronoi(participation, sites, new VoronoiHexPartitionParameters(iterations)),
            centers.PartitionByVoronoi(nullableRegions, sites, new VoronoiHexPartitionParameters(iterations)),
            centers.PartitionByVoronoi(participation, regions, sites, new VoronoiHexPartitionParameters(iterations))
        };
        foreach (var map in maps)
        {
            Assert.That(Enumerable.Range(0, 3).Select(i => map[i]), Is.EqualTo(new int?[] { 0, null, 0 }));
            Assert.That(map.Cells, Has.Count.EqualTo(1));
            Assert.That(map.Cells[0].Center, Is.EqualTo(expectedCentroid));
            Assert.That(map.Cells[0].Site.Weight, Is.EqualTo(3f));
            Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(2, 0) }));
            Assert.That(map.Participates(1), Is.False);
        }
        Assert.That(sites[0], Is.EqualTo(new Site(centers[0], 3f)));
        Assert.That(centers[1].X, Is.EqualTo(float.NegativeInfinity));
        Assert.That(centers[1].Y, Is.EqualTo(float.PositiveInfinity));
    }

    private static (int?[] Assignments, IReadOnlyList<VoronoiHexPartitionCell> Cells) PartitionForOverload(
        PartitionOverload overload, HexCenterMap centers, IReadOnlyList<Site> sites,
        int relaxationIterations = 0, EmptyCellPolicy emptyCellPolicy = EmptyCellPolicy.LeaveAsIs,
        ExclavePolicy exclavePolicy = ExclavePolicy.LeaveAsIs)
    {
        var topology = centers?.Topology ?? new HexMapTopology(1, 1, Layout.OddR);
        var regions = new IntHexMap(topology);
        if (overload == PartitionOverload.Full || overload == PartitionOverload.Regions)
        {
            var map = overload == PartitionOverload.Full
                ? centers!.PartitionByVoronoi(sites, new VoronoiHexPartitionParameters(relaxationIterations, emptyCellPolicy, exclavePolicy))
                : centers!.PartitionByVoronoi(regions, sites, new VoronoiHexPartitionParameters(relaxationIterations, emptyCellPolicy, exclavePolicy));
            return (Enumerable.Range(0, topology.Count).Select(i => (int?)map[i]).ToArray(), map.Cells);
        }

        var participation = new BoolHexMap(topology, Enumerable.Repeat(true, topology.Count).ToArray());
        var nullableRegions = new HexMap<int?>(topology, Enumerable.Repeat((int?)0, topology.Count).ToArray());
        var partial = overload switch
        {
            PartitionOverload.Participation => centers!.PartitionByVoronoi(
                participation, sites, new VoronoiHexPartitionParameters(relaxationIterations, emptyCellPolicy, exclavePolicy)),
            PartitionOverload.NullableRegions => centers!.PartitionByVoronoi(
                nullableRegions, sites, new VoronoiHexPartitionParameters(relaxationIterations, emptyCellPolicy, exclavePolicy)),
            PartitionOverload.CombinedMasks => centers!.PartitionByVoronoi(
                participation, regions, sites, new VoronoiHexPartitionParameters(relaxationIterations, emptyCellPolicy, exclavePolicy)),
            _ => throw new ArgumentOutOfRangeException(nameof(overload))
        };
        return (Enumerable.Range(0, topology.Count).Select(i => partial[i]).ToArray(), partial.Cells);
    }
}
