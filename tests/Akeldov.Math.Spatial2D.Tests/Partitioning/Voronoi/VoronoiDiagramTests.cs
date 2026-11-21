using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiDiagramTests
{
    private static CompositeContour Polygon(VoronoiCell cell) => (CompositeContour)cell.Region!.Contours.Single();

    [Test]
    public void EmptyCellPolicyControlsFinalMembership()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(10, 2));
        var sites = new[] { new PointXY(2, 1), new PointXY(40, 1), new PointXY(8, 1) };
        Assert.That(bounds.PartitionByVoronoi(sites, EmptyCellPolicy.LeaveAsIs)[1].Region, Is.Null);
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
        Assert.That(bounds.PartitionByVoronoi(sites)[0].Region, Is.Null);
        var cells = bounds.PartitionByVoronoi(sites, 1, policy);
        Assert.That(cells.Count, Is.EqualTo(3));
        Assert.That(cells[0].Site, Is.EqualTo(sites[0]));
        Assert.That(cells[0].Region, Is.Not.Null);
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
        Assert.That(Polygon(cells[0]).Curves.Max(edge => edge.StartPoint.X), Is.EqualTo(split));
        Assert.That(Polygon(cells[1]).Curves.Min(edge => edge.StartPoint.X), Is.EqualTo(split));
        Assert.That(cells[2].Region, Is.Null);
    }

    [Test]
    public void NegativeRelaxationCountThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(Rectangle).PartitionByVoronoi(Array.Empty<PointXY>(), -1));
    }

    [TestCase(1e20f)]
    [TestCase(1e-25f)]
    public void SingleSiteContourPreservesRepresentablePerimeter(float side)
    {
        var bounds = new Rectangle(default, new PointXY(side, side));
        var cell = bounds.PartitionByVoronoi(new[] { bounds.Center }).Single();

        Assert.That(cell.Region!.Contours.Single().Length, Is.EqualTo(4 * side));
        Assert.That(cell.GetCentroid(), Is.EqualTo(bounds.Center));
    }

    [Test]
    public void DiagonalBisectorSplitsSquareIntoTwoOrderedTriangles()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(4, 4));
        var sites = new[] { bounds.BottomLeft, bounds.TopRight };
        var cells = bounds.PartitionByVoronoi(sites);

        Assert.That(cells.Select(cell => cell.Site), Is.EqualTo(sites));
        Assert.That(Polygon(cells[0]).Curves.Select(edge => (edge.StartPoint, edge.EndPoint)), Is.EqualTo(new[]
        {
            (bounds.BottomLeft, bounds.BottomRight), (bounds.BottomRight, bounds.TopLeft), (bounds.TopLeft, bounds.BottomLeft)
        }));
        Assert.That(Polygon(cells[1]).Curves.Select(edge => (edge.StartPoint, edge.EndPoint)), Is.EqualTo(new[]
        {
            (bounds.BottomRight, bounds.TopRight), (bounds.TopRight, bounds.TopLeft), (bounds.TopLeft, bounds.BottomRight)
        }));
    }

    [TestCase(2)]
    [TestCase(4)]
    public void OutsideSiteWithZeroAreaRetainsItsIndex(int x)
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(1, 1));
        var cells = bounds.PartitionByVoronoi(new[] { new PointXY(x, 0), bounds.BottomLeft });
        Assert.That(cells[0].Region, Is.Null);
        Assert.Throws<InvalidOperationException>(() => cells[0].GetCentroid());
        Assert.That(Polygon(cells[1]).Curves.Count, Is.EqualTo(4));
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
        Assert.That(Polygon(cells[0]).Curves.Select(edge => edge.StartPoint), Is.EquivalentTo(new[] { new PointXY(-1, -1), new PointXY(1, -1),
            new PointXY(1, 1), new PointXY(-1, 1) }));
    }

    // These sites have an Apollonius circle centered at (0, 0) with radius 2.
    private static readonly Site[] DiskSites =
    {
        new Site(new PointXY(1, 0), 1), new Site(new PointXY(4, 0), 2)
    };

    [Test]
    public void UnequalWeightsProduceDiskAndRectangleWithHole()
    {
        var bounds = new Rectangle(new PointXY(-4, -4), new PointXY(8, 4));
        var cells = bounds.PartitionByVoronoi(DiskSites);
        var disk = (Circle)cells[0].Region!.Contours.Single();
        Assert.That(disk.Center, Is.EqualTo(default(PointXY)));
        Assert.That(disk.Radius, Is.EqualTo(2));
        Assert.That(cells[1].Region!.Contours.Count, Is.EqualTo(2));
        Assert.That(cells[1].Region!.Contains(default), Is.False);
        Assert.That(cells[1].Region!.Contains(new PointXY(3, 0)), Is.True);
        Assert.That(cells.All(c => c.Region!.Contains(new PointXY(2, 0))), Is.True);
        Assert.That(cells[0].GetCentroid(), Is.EqualTo(disk.Center));
        Assert.That(cells[1].GetCentroid().X, Is.EqualTo(192 / (96 - 4 * System.Math.PI)).Within(1e-6));

        var relaxed = bounds.PartitionByVoronoi(DiskSites, 1);
        Assert.That(relaxed.Select(c => c.Site), Is.EqualTo(cells.Select(c => c.GetCentroid())));
        Assert.That(relaxed.Select(c => c.Weight), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void ClippedSemicircleHasCurvedBoundaryAndAreaCentroid()
    {
        var bounds = new Rectangle(new PointXY(0, -4), new PointXY(6, 4));
        var cells = bounds.PartitionByVoronoi(DiskSites);
        var disk = cells[0].Region!;
        Assert.That(disk.Contours.Single().Length, Is.EqualTo(4 + 2 * System.Math.PI).Within(1e-5));
        Assert.That(disk.Contains(new PointXY(1.4f, 1.4f)), Is.True);
        Assert.That(disk.Contains(new PointXY(1.5f, 1.5f)), Is.False);
        Assert.That(cells[0].GetCentroid().X, Is.EqualTo(8 / (3 * System.Math.PI)).Within(1e-6));
        Assert.That(cells[0].GetCentroid().Y, Is.EqualTo(0).Within(1e-6));
    }

    [Test]
    public void CircleCrossingBothSidesOfStripSeparatesHeavierCell()
    {
        var bounds = new Rectangle(new PointXY(-6, -1), new PointXY(6, 1));
        var region = bounds.PartitionByVoronoi(DiskSites)[1].Region!;
        Assert.That(region.Contours.Count, Is.EqualTo(2));
        Assert.That(region.Contains(new PointXY(-3, 0)), Is.True);
        Assert.That(region.Contains(new PointXY(3, 0)), Is.True);
        Assert.That(region.Contains(default), Is.False);
    }

    [Test]
    public void ZeroAndInfiniteWeightsRespectEmptyCellPolicy()
    {
        var bounds = new Rectangle(default, new PointXY(10, 4));
        var sites = new[] { new Site(new PointXY(2, 2), float.PositiveInfinity),
            new Site(new PointXY(1, 2), 0), new Site(new PointXY(6, 2), 1),
            new Site(new PointXY(8, 2), float.PositiveInfinity) };
        var all = bounds.PartitionByVoronoi(sites);
        Assert.That(all[1].Region, Is.Null);
        Assert.That(all[2].Region, Is.Null);
        var kept = bounds.PartitionByVoronoi(sites, EmptyCellPolicy.Exclude);
        Assert.That(kept.Select(c => c.Site), Is.EqualTo(new[] { sites[0].Position, sites[3].Position }));
        Assert.That(kept.All(c => c.Region!.Contains(new PointXY(5, 2))), Is.True);
        Assert.Throws<InvalidOperationException>(() => bounds.PartitionByVoronoi(sites, EmptyCellPolicy.ThrowException));
        Assert.Throws<ArgumentException>(() => bounds.PartitionByVoronoi(new[] { sites[1] }));
        var finite = bounds.PartitionByVoronoi(new[] { sites[1], sites[2] });
        Assert.That(finite[0].Region, Is.Null);
        Assert.That(finite[1].GetCentroid(), Is.EqualTo(bounds.Center));
    }

    [TestCase(0f)]
    [TestCase(1000000f)]
    public void AdjacentFloatWeightsHaveAContinuousClippedBoundary(float offset)
    {
        var bounds = new Rectangle(new PointXY(offset - 5, -5), new PointXY(offset + 5, 5));
        var sites = new[] { new Site(new PointXY(offset - 1, 0), 1),
            new Site(new PointXY(offset + 1, 0), MathF.BitIncrement(1)) };
        var cells = bounds.PartitionByVoronoi(sites);
        Assert.That(cells[0].Region!.Contains(new PointXY(offset - 0.25f, 2)), Is.True);
        Assert.That(cells[0].Region!.Contains(new PointXY(offset + 0.25f, 2)), Is.False);
        Assert.That(cells[0].GetCentroid().X, Is.EqualTo(offset - 2.5f).Within(1e-5));
    }

    [Test]
    public void FourWeightedCellsMeetAtOneVertex()
    {
        var bounds = new Rectangle(new PointXY(-7, -7), new PointXY(7, 7));
        var sites = new[] { new Site(new PointXY(2, 0), 2), new Site(new PointXY(0, 3), 3),
            new Site(new PointXY(-4, 0), 4), new Site(new PointXY(0, -1), 1) };
        var cells = bounds.PartitionByVoronoi(sites);
        foreach (var cell in cells)
        {
            Assert.That(cell.Region!.Contains(default), Is.True);
            Assert.That(cell.Region.Contains(cell.Site), Is.True);
        }
    }

    [Test]
    public void ThreeNearlyEqualWeightsPreserveTheirCommonBoundary()
    {
        var bounds = new Rectangle(new PointXY(-10, -10), new PointXY(10, 10));
        var sites = new[] { new Site(new PointXY(-1, 0), 1),
            new Site(new PointXY(1, 0), MathF.BitIncrement(1)),
            new Site(new PointXY(0, 1), MathF.BitDecrement(1)) };
        var cells = bounds.PartitionByVoronoi(sites);
        foreach (var cell in cells)
            Assert.That(cell.Region!.Contains(cell.Site), Is.True);
        Assert.That(cells[2].Region!.Contains(new PointXY(0, 3)), Is.True);
    }

    [Test]
    public void SmallDiskClippedByLongSideRetainsBothIntersections()
    {
        var bounds = new Rectangle(new PointXY(-10, 0), new PointXY(10, 5));
        var sites = new[] { new Site(default, 1e-8f), new Site(new PointXY(1, 0), 1) };
        var cells = bounds.PartitionByVoronoi(sites);
        Assert.That(cells[0].Region!.Contains(new PointXY(0, 0.8e-8f)), Is.True);
        Assert.That(cells[0].GetCentroid().Y, Is.EqualTo(4e-8 / (3 * System.Math.PI)).Within(1e-14));
    }

    [Test]
    public void DiskTangentToRectangleFromOutsideHasNoArea()
    {
        var bounds = new Rectangle(new PointXY(2, -3), new PointXY(8, 3));
        var cells = bounds.PartitionByVoronoi(DiskSites);
        Assert.That(cells[0].Region, Is.Null);
        Assert.That(cells[1].GetCentroid(), Is.EqualTo(bounds.Center));
    }

    [TestCase(0f)]
    [TestCase(1f)]
    public void NearlyEqualWeightsSplitTinyWindowAtWeightedBoundary(float y)
    {
        var heavier = MathF.BitIncrement(1);
        // The internal division point is on the bisector for sites (-1,-y) and (1,y).
        float x = (float)((1d - heavier) / (1d + heavier));
        var center = new PointXY(x, y * x);
        const float half = 1e-10f;
        var bounds = new Rectangle(new PointXY(x - half, center.Y - half), new PointXY(x + half, center.Y + half));
        var sites = new[] { new Site(new PointXY(-1, -y), 1), new Site(new PointXY(1, y), heavier) };
        var cells = bounds.PartitionByVoronoi(sites, EmptyCellPolicy.ThrowException);

        var left = new PointXY(x - half / 2, center.Y);
        var right = new PointXY(x + half / 2, center.Y);
        Assert.That(cells[0].Region!.Contains(left), Is.True);
        Assert.That(cells[0].Region!.Contains(right), Is.False);
        Assert.That(cells[1].Region!.Contains(right), Is.True);
        Assert.That(cells[1].Region!.Contains(left), Is.False);
    }

    [Test]
    public void AdjacentFloatWeightsRetainCurvatureAwayFromTheAxis()
    {
        var bounds = new Rectangle(new PointXY(-3, -3), new PointXY(3, 3));
        var sites = new[] { new Site(new PointXY(-1, 0), 1), new Site(new PointXY(1, 0), MathF.BitIncrement(1)) };
        var cells = bounds.PartitionByVoronoi(sites);
        // At y=0 the boundary is near x=-6e-8. At y=2 it has curved left to x=-3e-7.
        var onAxis = new PointXY(-1.5e-7f, 0);
        var offAxis = new PointXY(-1.5e-7f, 2);
        Assert.That(cells[0].Region!.Contains(onAxis), Is.True);
        Assert.That(cells[0].Region!.Contains(offAxis), Is.False);
        Assert.That(cells[1].Region!.Contains(onAxis), Is.False);
        Assert.That(cells[1].Region!.Contains(offAxis), Is.True);
    }

    [Test]
    public void ThreeAlmostEqualWeightsMeetInsideTinyWindow()
    {
        var w1 = MathF.BitIncrement(1);
        var w2 = MathF.BitIncrement(w1);
        var sites = new[] { new Site(new PointXY(1, 0), 1), new Site(new PointXY(0, w1), w1),
            new Site(new PointXY(-w2, 0), w2) };
        var bounds = new Rectangle(new PointXY(-1e-10f, -1e-10f), new PointXY(1e-10f, 1e-10f));
        var cells = bounds.PartitionByVoronoi(sites, EmptyCellPolicy.ThrowException);
        PointXY[] samples = { new(5e-11f, 0), new(0, 5e-11f), new(-5e-11f, 0) };
        for (int i = 0; i < cells.Count; i++)
            for (int j = 0; j < samples.Length; j++)
                Assert.That(cells[i].Region!.Contains(samples[j]), Is.EqualTo(i == j));
    }

    [Test]
    public void QuarterCircleProjectsOntoItsCurvedBoundary()
    {
        var bounds = new Rectangle(new PointXY(0, -4), new PointXY(6, 4));
        var cells = bounds.PartitionByVoronoi(DiskSites);
        foreach (var cell in cells)
        {
            var arc = Polygon(cell).Curves.Single(curve =>
                (curve.StartPoint == new PointXY(2, 0) && curve.EndPoint == new PointXY(0, 2)) ||
                (curve.StartPoint == new PointXY(0, 2) && curve.EndPoint == new PointXY(2, 0)));
            var projection = arc.ProjectWithParameter(new PointXY(1, 1));
            float diagonal = MathF.Sqrt(2);
            Assert.That(projection.ProjectedPoint.X, Is.EqualTo(diagonal).Within(3e-7));
            Assert.That(projection.ProjectedPoint.Y, Is.EqualTo(diagonal).Within(3e-7));
            Assert.That(projection.CurveCoordinate, Is.EqualTo(MathF.PI / 2).Within(3e-7));
            Assert.That(arc.GetPoint(arc.Length / 2).Distance(projection.ProjectedPoint), Is.LessThan(3e-7));
        }
    }

    [Test]
    public void TranslatedIntersectionsPreserveClosedRegions()
    {
        var bounds = new Rectangle(new PointXY(8185, 8185), new PointXY(8199, 8199));
        var sites = new[] { new Site(new PointXY(8193, 8190), 1), new Site(new PointXY(8197, 8193), 1),
            new Site(new PointXY(8184, 8201), 4), new Site(new PointXY(8184, 8188), 3),
            new Site(new PointXY(8182, 8190), 3), new Site(new PointXY(8184, 8197), 5),
            new Site(new PointXY(8202, 8191), 1) };
        var cells = bounds.PartitionByVoronoi(sites);
        foreach (var sample in new[] { new PointXY(8195, 8191), new PointXY(8195, 8192), new PointXY(8196, 8191) })
        {
            var expected = sites.MinBy(site => sample.Distance(site.Position) / site.Weight);
            Assert.That(cells.Where(cell => cell.Region?.Contains(sample) == true).Select(cell => cell.Site),
                Is.EqualTo(new[] { expected.Position }));
        }
    }

    [TestCase(1.999f)]
    [TestCase(1.464f)]
    [TestCase(1.463f)]
    [TestCase(0.25f)]
    public void ClippedDiskAndItsComplementHaveAnalyticCentroids(float left)
    {
        var bounds = new Rectangle(new PointXY(left, -2), new PointXY(4, 2));
        var cells = bounds.PartitionByVoronoi(DiskSites);
        // Integral of x * 2 sqrt(4 - x²) from left to 2, divided by the cap area.
        double height = System.Math.Sqrt(4 - (double)left * left);
        double area = 4 * System.Math.Acos(left / 2d) - left * height;
        double moment = 2 * height * height * height / 3;
        double rectangleArea = (4d - left) * 4;
        double rectangleMoment = rectangleArea * (4d + left) / 2;
        Assert.That(cells[0].GetCentroid().X, Is.EqualTo(moment / area).Within(3e-7));
        Assert.That(cells[1].GetCentroid().X, Is.EqualTo((rectangleMoment - moment) / (rectangleArea - area)).Within(3e-7));
        Assert.That(cells.All(cell => MathF.Abs(cell.GetCentroid().Y) < 1e-6f), Is.True);
    }
}
