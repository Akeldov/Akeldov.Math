using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using System.Globalization;

namespace Akeldov.Math.Hexes.Tests.Geometry.HexEdgesGeneration;

public class HexMapTopologyHexEdgesGenerationExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToHexEdgeSegments_ForSingleHex_ReturnsSixSegments(Layout layout)
    {
        var topology = new HexMapTopology(1, 1, layout);

        List<Segment> segments = topology.ToHexEdgeSegments(2f);

        Assert.That(segments, Has.Count.EqualTo(6));
        Assert.That(segments.Select(segment => segment.Length), Is.All.EqualTo(2f).Within(0.0001f));
        AssertSegmentsAreUnique(segments);
    }

    [TestCase(Layout.OddR, 2, 1)]
    [TestCase(Layout.EvenR, 2, 1)]
    [TestCase(Layout.OddQ, 2, 1)]
    [TestCase(Layout.EvenQ, 2, 1)]
    [TestCase(Layout.OddR, 1, 2)]
    [TestCase(Layout.EvenR, 1, 2)]
    [TestCase(Layout.OddQ, 1, 2)]
    [TestCase(Layout.EvenQ, 1, 2)]
    public void ToHexEdgeSegments_WhenTwoHexesShareEdge_ReturnsSharedSegmentOnce(
        Layout layout,
        int width,
        int height)
    {
        var topology = new HexMapTopology(width, height, layout);

        List<Segment> segments = topology.ToHexEdgeSegments(2f);

        Assert.That(segments, Has.Count.EqualTo(11));
        AssertSegmentsAreUnique(segments);
    }

    [Test]
    public void ToHexEdgeSegments_WhenTopologyIsEmpty_ReturnsEmptyList()
    {
        var topology = new HexMapTopology(0, 3, Layout.OddR);

        List<Segment> segments = topology.ToHexEdgeSegments(2f);

        Assert.That(segments, Is.Empty);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToHexEdgeSegments_ForGeometry_UsesGeometryOrigin(Layout layout)
    {
        var topology = new HexMapTopology(2, 1, layout);
        var origin = new VectorXY(10f, 20f);
        float radius = 2f.ConvertHexApothemToRadius();
        VectorXY defaultOrigin = VectorXYInt.Zero.GetHexCenter(radius, layout);
        VectorXY translation = origin - defaultOrigin;
        var geometry = new HexMapGeometry(topology, origin, radius);

        List<Segment> geometrySegments = geometry.ToHexEdgeSegments();
        List<Segment> expectedSegments = topology
            .ToHexEdgeSegments(radius)
            .Select(segment => segment + translation)
            .ToList();

        Assert.That(
            geometrySegments.Select(CreateSegmentKey),
            Is.EquivalentTo(expectedSegments.Select(CreateSegmentKey)));
    }

    [Test]
    public void ToHexEdgeSegments_WhenGeometryIsEmpty_ReturnsEmptyList()
    {
        var geometry = new HexMapGeometry(
            new HexMapTopology(0, 3, Layout.OddR),
            VectorXY.Zero,
            2f);

        List<Segment> segments = geometry.ToHexEdgeSegments();

        Assert.That(segments, Is.Empty);
    }

    [Test]
    public void ToHexEdgeSegments_WhenGeometryIsDefault_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(HexMapGeometry).ToHexEdgeSegments());

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [TestCase(0f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void ToHexEdgeSegments_WhenRadiusIsInvalid_Throws(float radius)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.ToHexEdgeSegments(radius));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    private static void AssertSegmentsAreUnique(IReadOnlyCollection<Segment> segments)
    {
        var keys = new HashSet<string>(segments.Select(CreateSegmentKey));

        Assert.That(keys, Has.Count.EqualTo(segments.Count));
    }

    private static string CreateSegmentKey(Segment segment)
    {
        string endpointA = CreatePointKey(segment.EndpointA);
        string endpointB = CreatePointKey(segment.EndpointB);

        return string.CompareOrdinal(endpointA, endpointB) <= 0
            ? endpointA + "|" + endpointB
            : endpointB + "|" + endpointA;
    }

    private static string CreatePointKey(PointXY point)
    {
        return Quantize(point.X).ToString(CultureInfo.InvariantCulture) +
            "," +
            Quantize(point.Y).ToString(CultureInfo.InvariantCulture);
    }

    private static int Quantize(float value) => (int)MathF.Round(value * 10000f);
}
