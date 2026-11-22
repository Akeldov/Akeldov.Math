using System.Xml.Linq;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Tests.Rasterization;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiDiagramSnapshotTests
{
    private static readonly Rectangle Bounds = new(new PointXY(32, 16), new PointXY(336, 208));

    [Test]
    public void PartitionByVoronoi_WithExteriorSite_MatchesApprovedPng()
    {
        PointXY[] sites = { new(18, 110), new(90, 165), new(112, 54), new(186, 117), new(261, 169), new(280, 63) };
        AssertMatchesApprovedPng(Bounds.PartitionByVoronoi(sites), "bounded-exterior-site.png");
    }

    [TestCase(1f, 1.5f, "weighted-strong-center.png")]
    [TestCase(2f, 1f, "weighted-strong-outer-sites.png")]
    public void PartitionByVoronoi_WithMixedWeights_MatchesApprovedPng(float outerWeight, float centerWeight, string name)
    {
        Site[] sites =
        {
            new(new PointXY(56, 106), outerWeight), new(new PointXY(90, 165), 1),
            new(new PointXY(112, 54), 0.75f), new(new PointXY(186, 117), centerWeight),
            new(new PointXY(261, 169), 1), new(new PointXY(280, 63), outerWeight)
        };
        AssertMatchesApprovedPng(Bounds.PartitionByVoronoi(sites), name);
    }

    [Test]
    public void PartitionByVoronoi_WithWidelyDifferentWeights_MatchesApprovedPng()
    {
        Site[] sites =
        {
            new(new PointXY(62, 65), 1), new(new PointXY(110, 164), 2),
            new(new PointXY(290, 67), 3), new(new PointXY(205, 112), 8)
        };
        AssertMatchesApprovedPng(Bounds.PartitionByVoronoi(sites), "weighted-sites-1-2-3-8.png");
    }

    [Test]
    public void PartitionByVoronoi_WithCircularHole_MatchesApprovedPng()
    {
        Site[] sites = { new(new PointXY(149, 112), 1), new(new PointXY(233, 112), 2) };
        var cells = Bounds.PartitionByVoronoi(sites);
        Assert.That(cells[0].Region!.Contours.Single(), Is.TypeOf<Circle>());
        Assert.That(cells[1].Region!.Contours.Count, Is.EqualTo(2));
        AssertMatchesApprovedPng(cells, "weighted-circular-hole.png");
    }

    [Test]
    public void PartitionByVoronoi_WithDisconnectedCell_MatchesApprovedPng()
    {
        Site[] sites = { new(new PointXY(249, 112), 3), new(new PointXY(312, 112), 4) };
        var cells = Bounds.PartitionByVoronoi(sites);
        var split = cells[1].Region!;
        Assert.That(split.Contours.Count, Is.EqualTo(2));
        Assert.That(split.Contains(new PointXY(48, 112)), Is.True);
        Assert.That(split.Contains(new PointXY(168, 112)), Is.False);
        Assert.That(split.Contains(new PointXY(312, 112)), Is.True);
        AssertMatchesApprovedPng(cells, "weighted-disconnected-cell.png");
    }

    private static void AssertMatchesApprovedPng(IReadOnlyList<VoronoiCell> cells, string name)
    {
        RGBA8BitColor[] colors = { new(173, 204, 231, 255), new(247, 202, 157, 255),
            new(177, 221, 196, 255), new(246, 225, 148, 255), new(203, 187, 226, 255), new(232, 177, 185, 255) };
        var pixels = new RGBA8BitColor[352 * 224];
        Array.Fill(pixels, new RGBA8BitColor(248, 250, 252, 255));
        var raster = new SpatialRaster<RGBA8BitColor>(
            new RasterGeometry(default(PointXY), new VectorXY(352, 224), new VectorXYInt(352, 224)), pixels);
        var ink = new RGBA8BitColor(40, 53, 68, 255);
        for (int i = 0; i < cells.Count; i++)
        {
            raster.Draw(cells[i].Region!, colors[i]);
        }

        foreach (var cell in cells)
        {
            foreach (var contour in cell.Region!.Contours)
                raster.Draw(contour, 1.5f, ink);
            raster.FillCircle(cell.Site, 3.5f, ink);
        }

        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Partitioning", "Voronoi");
        string actual = Path.Combine(Directory.CreateDirectory(Path.Combine(directory, "Actual")).FullName, name);
        raster.SaveAsPng(actual);
        TestContext.AddTestAttachment(actual, "Bounded Voronoi diagram");
        string approved = Path.Combine(directory, "Approved", name);
        Assert.That(File.Exists(approved), Is.True, $"Missing approved PNG. Actual: {actual}");
        Assert.That(PngSnapshotComparer.AreEquivalent(File.ReadAllBytes(actual), File.ReadAllBytes(approved)),
            Is.True, $"Voronoi snapshot changed. Actual: {actual}");
    }

    [Test]
    public void PartitionByVoronoi_InteriorAndBoundaryCells_MatchApprovedSvg()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(100, 100));
        PointXY[] sites = { new(25, 25), new(75, 25), new(75, 75), new(25, 75), new(50, 50) };
        string[] colors = { "#a8d8ea", "#ffd3b6", "#d4c1ec", "#b8dfba", "#ffe59b" };
        var cells = bounds.PartitionByVoronoi(sites);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var svg = new XElement(ns + "svg", new XAttribute("viewBox", "-2 -2 104 104"));
        for (int i = 0; i < cells.Count; i++)
        {
            var vertices = ((CompositeContour)cells[i].Region!.Contours.Single()).Curves.Select(edge => edge.StartPoint).ToArray();
            // A polygon may begin at any vertex. Keep the winding and normalize only the start.
            int start = Enumerable.Range(0, vertices.Length)
                .OrderBy(j => vertices[j].X).ThenBy(j => vertices[j].Y).First();
            var points = vertices.Skip(start).Concat(vertices.Take(start))
                .Select(p => FormattableString.Invariant($"{p.X},{p.Y}"));
            svg.Add(new XElement(ns + "polygon", new XAttribute("points", string.Join(" ", points)),
                new XAttribute("fill", colors[i]), new XAttribute("stroke", "#334155")));
        }

        foreach (var cell in cells)
            svg.Add(new XElement(ns + "circle", new XAttribute("cx", cell.Site.X),
                new XAttribute("cy", cell.Site.Y), new XAttribute("r", 2)));

        string approved = Path.Combine(TestContext.CurrentContext.TestDirectory,
            "Partitioning", "Voronoi", "Approved", "bounded-cells.svg");
        Assert.That(svg.ToString(), Is.EqualTo(XElement.Load(approved).ToString()));
    }
}
