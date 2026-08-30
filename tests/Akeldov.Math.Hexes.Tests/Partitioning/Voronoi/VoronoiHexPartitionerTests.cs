using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

namespace Akeldov.Math.Hexes.Tests.Partitioning.Voronoi;

public class VoronoiHexPartitionerTests
{
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

        Assert.That(map[0].Site, Is.EqualTo(sites[0]));
        Assert.That(map[0].SiteIndex, Is.EqualTo(0));
        Assert.That(map[0].Center, Is.EqualTo(sites[0].Position));
        Assert.That(map[1], Is.SameAs(map[0]));
        Assert.That(map[2].Site, Is.EqualTo(sites[1]));
        Assert.That(map[2].SiteIndex, Is.EqualTo(1));
        Assert.That(map[2].Center, Is.EqualTo(sites[1].Position));
        Assert.That(map.Cells, Has.Count.EqualTo(2));
        Assert.That(map.Cells[0], Is.SameAs(map[0]));
        Assert.That(map.Cells[0].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(0, 0), new VectorXYInt(1, 0) }));
        Assert.That(map.Cells[1], Is.SameAs(map[2]));
        Assert.That(map.Cells[1].HexIndexes, Is.EqualTo(new[] { new VectorXYInt(2, 0) }));
    }

    [Test]
    public void Partition_ReturnsReadOnlyHexMapOfVoronoiCells()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var hexCenters = new HexCenterMap(new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.EvenQ));
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);
        IHexMap<VoronoiCell> hexMap = map;

        Assert.That(map, Is.Not.InstanceOf<HexMap<VoronoiCell>>());
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
        ISpatialHexMap<VoronoiCell> spatialMap = map;

        Assert.That(map.Centers, Is.SameAs(hexCenters));
        Assert.That(spatialMap.Geometry, Is.EqualTo(hexCenters.Geometry));
        Assert.That(map[0].SiteIndex, Is.EqualTo(0));
        Assert.That(map[1].SiteIndex, Is.EqualTo(0));
        Assert.That(map[2].SiteIndex, Is.EqualTo(1));
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
        ISpatialHexMap<VoronoiCell?> spatialMap = map;

        Assert.Multiple(() =>
        {
            Assert.That(map.Centers, Is.SameAs(hexCenters));
            Assert.That(spatialMap.Geometry, Is.EqualTo(hexCenters.Geometry));
            Assert.That(map[0], Is.SameAs(map.Cells[0]));
            Assert.That(map[1], Is.Null);
            Assert.That(map[2], Is.SameAs(map.Cells[1]));
            Assert.That(map[new VectorXYInt(0, 0)], Is.SameAs(map.Cells[0]));
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

        Assert.That(map[0], Is.SameAs(map.Cells[0]));
        Assert.That(map[1], Is.Null);
        Assert.That(map[2], Is.SameAs(map.Cells[1]));
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

        HexMap<VoronoiCell?> mutableMap = map.ToMutableHexMap();
        BoolHexMap mutableMask = map.ToMutableParticipationMask();
        mutableMap[1] = map.Cells[1];
        mutableMask[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(mutableMap[1], Is.SameAs(map.Cells[1]));
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

        HexMap<VoronoiCell> mutableMap = map.ToMutableHexMap();
        mutableMap[0] = map.Cells[1];

        Assert.Multiple(() =>
        {
            Assert.That(mutableMap[0], Is.SameAs(map.Cells[1]));
            Assert.That(map[0], Is.SameAs(map.Cells[0]));
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

        Assert.That(map[0].Site, Is.EqualTo(sites[1]));
        Assert.That(map[0].SiteIndex, Is.EqualTo(1));
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

        Assert.That(map[0].Site, Is.EqualTo(sites[0]));
        Assert.That(map[0].SiteIndex, Is.EqualTo(0));
        Assert.That(map[1].Site, Is.EqualTo(sites[1]));
        Assert.That(map[1].SiteIndex, Is.EqualTo(1));
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

        Assert.That(map[0].Site, Is.EqualTo(sites[1]));
        Assert.That(map[0].SiteIndex, Is.EqualTo(1));
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

        Assert.That(map[0].SiteIndex, Is.EqualTo(1));
        Assert.That(map[0].Site, Is.EqualTo(sites[1]));
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

        Assert.That(map[0], Is.SameAs(map.Cells[0]));
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
            partitioner.Partition(hexCenters, null!));

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
            hexCenters.ToVoronoiHexPartitionMap(sites, null!));

        Assert.That(exception!.ParamName, Is.EqualTo("participationMask"));
    }
}
