using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiDiagramTests
{
    [Test]
    public void EmptyCellPolicyControlsFinalMembership()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(10, 2));
        var sites = new[] { new PointXY(2, 1), new PointXY(40, 1), new PointXY(8, 1) };
        Assert.That(bounds.PartitionByVoronoi(sites, EmptyCellPolicy.LeaveAsIs)[1].Vertices, Is.Empty);
        var kept = bounds.PartitionByVoronoi(sites, EmptyCellPolicy.Exclude);
        Assert.That(kept.Select(cell => cell.Site), Is.EqualTo(new[] { sites[0], sites[2] }));
        Assert.Throws<InvalidOperationException>(() =>
            bounds.PartitionByVoronoi(sites, EmptyCellPolicy.ThrowException));
    }

    [TestCase(EmptyCellPolicy.Exclude)]
    [TestCase(EmptyCellPolicy.ThrowException)]
    public void InitiallyEmptyCellCanGainAreaBeforePolicyIsApplied(EmptyCellPolicy policy)
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(10, 2));
        var sites = new[] { new PointXY(-2, 1), new PointXY(0, 1), new PointXY(10, 1) };
        Assert.That(bounds.PartitionByVoronoi(sites)[0].Vertices, Is.Empty);
        var cells = bounds.PartitionByVoronoi(sites, 1, policy);
        Assert.That(cells.Count, Is.EqualTo(3));
        Assert.That(cells[0].Site, Is.EqualTo(sites[0]));
        Assert.That(cells[0].Vertices, Is.Not.Empty);
    }

    [TestCase(0, 0, 4, 2)]
    [TestCase(1, 1, 7, 4)]
    [TestCase(2, 2, 8, 5)]
    public void RelaxationRebuildsCellsAndPreservesEmptySite(int iterations, int left, int right, int split)
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(12, 2));
        var sites = new[] { new PointXY(0, 1), new PointXY(4, 1), new PointXY(50, 1) };
        var cells = bounds.PartitionByVoronoi(sites, iterations);

        Assert.That(cells.Select(cell => cell.Site), Is.EqualTo(new[] { new PointXY(left, 1), new PointXY(right, 1), sites[2] }));
        Assert.That(cells[0].Vertices.Max(p => p.X), Is.EqualTo(split));
        Assert.That(cells[1].Vertices.Min(p => p.X), Is.EqualTo(split));
        Assert.That(cells[2].Vertices, Is.Empty);
    }

    [Test]
    public void NegativeRelaxationCountThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Rectangle).PartitionByVoronoi(Array.Empty<PointXY>(), -1));
    }

    [Test]
    public void DiagonalBisectorSplitsSquareIntoTwoOrderedTriangles()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(4, 4));
        var sites = new[] { bounds.BottomLeft, bounds.TopRight };
        var cells = bounds.PartitionByVoronoi(sites);

        Assert.That(cells.Select(cell => cell.Site), Is.EqualTo(sites));
        Assert.That(cells[0].Vertices, Is.EqualTo(new[] { bounds.BottomLeft, bounds.BottomRight, bounds.TopLeft }));
        Assert.That(cells[1].Vertices, Is.EqualTo(new[] { bounds.BottomRight, bounds.TopRight, bounds.TopLeft }));
    }

    [TestCase(2)]
    [TestCase(4)]
    public void OutsideSiteWithZeroAreaRetainsItsIndex(int x)
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(1, 1));
        var cells = bounds.PartitionByVoronoi(new[] { new PointXY(x, 0), bounds.BottomLeft });
        Assert.That(cells[0].Vertices, Is.Empty);
        Assert.Throws<InvalidOperationException>(() => cells[0].GetCentroid());
        Assert.That(cells[1].Vertices.Count, Is.EqualTo(4));
    }

    [Test]
    public void GetCentroid_ReturnsAreaCentroidOfClippedTrapezoid()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(4, 2));
        var sites = new[] { new PointXY(1, 1), new PointXY(3, 3) };
        var cells = bounds.PartitionByVoronoi(sites);

        // The diagonal x + y = 4 cuts a right triangle of area 2 off the rectangle.
        Assert.That(cells[0].GetCentroid(), Is.EqualTo(new PointXY(14f / 9, 8f / 9)));
    }

    [Test]
    public void InteriorSiteHasFourBisectorEdges()
    {
        var bounds = new Rectangle(new PointXY(-5, -5), new PointXY(5, 5));
        var sites = new[] { new PointXY(0, 0), new PointXY(-2, 0), new PointXY(2, 0),
            new PointXY(0, -2), new PointXY(0, 2) };
        var cells = bounds.PartitionByVoronoi(sites);
        Assert.That(cells[0].Vertices, Is.EquivalentTo(new[] { new PointXY(-1, -1), new PointXY(1, -1),
            new PointXY(1, 1), new PointXY(-1, 1) }));
    }
}
