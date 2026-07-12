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
        var hexCenters = new HexCenterMap(3, 1, VectorXY.Zero, 1f, Layout.OddR);
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
        var hexCenters = new HexCenterMap(2, 2, VectorXY.Zero, 1f, Layout.EvenQ);
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);
        IHexMap<VoronoiCell> hexMap = map;

        Assert.That(map, Is.Not.InstanceOf<HexMap<VoronoiCell>>());
        Assert.That(map.Centers, Is.SameAs(hexCenters));
        Assert.That(hexMap.Resolution, Is.EqualTo(new VectorXYInt(2, 2)));
        Assert.That(hexMap.Layout, Is.EqualTo(Layout.EvenQ));
        Assert.That(map.Layout, Is.EqualTo(Layout.EvenQ));
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
        var hexCenters = new HexCenterMap(3, 1, VectorXY.Zero, 1f, Layout.OddR);

        var map = hexCenters.ToVoronoiHexPartitionMap(sites);

        Assert.That(map.Centers, Is.SameAs(hexCenters));
        Assert.That(map[0].SiteIndex, Is.EqualTo(0));
        Assert.That(map[1].SiteIndex, Is.EqualTo(0));
        Assert.That(map[2].SiteIndex, Is.EqualTo(1));
        Assert.That(map.Cells, Has.Count.EqualTo(2));
    }

    [Test]
    public void ToMutableHexMap_ReturnsCallerOwnedAssignmentCopy()
    {
        var sites = new[]
        {
            new Site(new PointXY(0f, 0f), 1f),
            new Site(new PointXY(4f, 0f), 1f)
        };
        var hexCenters = new HexCenterMap(3, 1, VectorXY.Zero, 1f, Layout.OddR);
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
        var hexCenters = new HexCenterMap(1, 1, new VectorXY(3f, 0f), 1f, Layout.OddR);
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
        var hexCenters = new HexCenterMap(2, 1, VectorXY.Zero, 1f, Layout.OddR);
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
        var hexCenters = new HexCenterMap(1, 1, new VectorXY(-100f, 0f), 1f, Layout.OddR);
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
        var hexCenters = new HexCenterMap(1, 1, VectorXY.Zero, 1f, Layout.OddR);
        var partitioner = new VoronoiHexPartitioner(sites);

        var map = partitioner.Partition(hexCenters);

        Assert.That(map[0].SiteIndex, Is.EqualTo(1));
        Assert.That(map[0].Site, Is.EqualTo(sites[1]));
    }

    [Test]
    public void Partition_WhenHexCenterCoordinateIsNotFinite_Throws()
    {
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };
        var hexCenters = new HexCenterMap(
            width: 2,
            height: 1,
            origin: new VectorXY(float.MaxValue, 0f),
            radius: (float.MaxValue / 4f).ConvertHexApothemToRadius(),
            layout: Layout.OddR);
        var partitioner = new VoronoiHexPartitioner(sites);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            partitioner.Partition(hexCenters));

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
        var hexCenters = new HexCenterMap(1, 1, VectorXY.Zero, 1f, Layout.OddR);
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
    public void ToVoronoiHexPartitionMap_WhenHexCenterMapIsNull_Throws()
    {
        HexCenterMap hexCenters = null!;
        var sites = new[] { new Site(new PointXY(0f, 0f), 1f) };

        var exception = Assert.Throws<ArgumentNullException>(() =>
            hexCenters.ToVoronoiHexPartitionMap(sites));

        Assert.That(exception!.ParamName, Is.EqualTo("hexCenters"));
    }
}
