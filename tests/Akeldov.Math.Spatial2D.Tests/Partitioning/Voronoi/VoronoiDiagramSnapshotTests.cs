using System.Xml.Linq;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Partitioning.Voronoi;

public class VoronoiDiagramSnapshotTests
{
    [Test]
    public void PartitionByVoronoi_InteriorAndBoundaryCells_MatchApprovedSvg()
    {
        var bounds = new Rectangle(new PointXY(0, 0), new PointXY(100, 100));
        PointXY[] sites = { new(25, 25), new(75, 25), new(75, 75), new(25, 75), new(50, 50) };
        string[] colors = { "#a8d8ea", "#ffd3b6", "#d4c1ec", "#b8dfba", "#ffe59b" };
        var cells = VoronoiDiagram.PartitionByVoronoi(bounds, sites);
        XNamespace ns = "http://www.w3.org/2000/svg";
        var svg = new XElement(ns + "svg", new XAttribute("viewBox", "-2 -2 104 104"));
        for (int i = 0; i < cells.Count; i++)
        {
            var vertices = cells[i].Vertices;
            Assert.That(vertices, Is.Not.Empty);
            // A polygon may begin at any vertex. Keep the winding and normalize only the start.
            int start = Enumerable.Range(0, vertices.Count)
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
