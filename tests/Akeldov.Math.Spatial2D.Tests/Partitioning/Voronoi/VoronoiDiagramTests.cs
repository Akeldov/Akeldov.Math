using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiDiagramTests
{
    [Test]
    public void DiagonalBisectorSplitsSquareIntoTwoOrderedTriangles()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(4, 4));
        var sites = new[] { bounds.BottomLeft, bounds.TopRight };
        var cells = VoronoiDiagram.PartitionByVoronoi(bounds, sites);

        Assert.That(cells.Select(cell => cell.Site), Is.EqualTo(sites));
        Assert.That(cells[0].Vertices, Is.EqualTo(new[] { bounds.BottomLeft, bounds.BottomRight, bounds.TopLeft }));
        Assert.That(cells[1].Vertices, Is.EqualTo(new[] { bounds.BottomRight, bounds.TopRight, bounds.TopLeft }));
    }

    [TestCase(2)]
    [TestCase(4)]
    public void OutsideSiteWithZeroAreaRetainsItsIndex(int x)
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(1, 1));
        var cells = VoronoiDiagram.PartitionByVoronoi(bounds, new[] { new PointXY(x, 0), bounds.BottomLeft });
        Assert.That(cells[0].Vertices, Is.Empty);
        Assert.That(cells[1].Vertices.Count, Is.EqualTo(4));
    }

    [Test]
    public void InteriorSiteHasFourBisectorEdges()
    {
        var bounds = new Rectangle(new PointXY(-5, -5), new PointXY(5, 5));
        var sites = new[] { new PointXY(0, 0), new PointXY(-2, 0), new PointXY(2, 0),
            new PointXY(0, -2), new PointXY(0, 2) };
        var cells = VoronoiDiagram.PartitionByVoronoi(bounds, sites);
        Assert.That(cells[0].Vertices, Is.EquivalentTo(new[] { new PointXY(-1, -1), new PointXY(1, -1),
            new PointXY(1, 1), new PointXY(-1, 1) }));
    }
}
