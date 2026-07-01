using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Hexes.Tests.Geometry.Contours;

public class HexMatrixApothemOffsetContourTests
{
    private const float Apothem = 1.25f;
    private const float Epsilon = 1e-4f;

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToApothemOffsetContour_PlacesEndpointsAtApothemDistanceFromSourceContour(Layout layout)
    {
        PolyhexGeometry geometry = CreateGeometry();

        Segment[] contour = geometry.ToContour(layout);
        Segment[] extendedContour = geometry.ToApothemOffsetContour(layout);

        Assert.That(extendedContour, Is.Not.Empty);

        for (int i = 0; i < extendedContour.Length; i++)
        {
            Segment extended = extendedContour[i];

            Assert.That(
                HasSourceContourAtApothemDistance(extended.EndpointA, contour),
                Is.True,
                $"Extended segment {i} endpoint A must be at apothem distance from the source contour.");

            Assert.That(
                HasSourceContourAtApothemDistance(extended.EndpointB, contour),
                Is.True,
                $"Extended segment {i} endpoint B must be at apothem distance from the source contour.");
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToApothemOffsetContour_ReturnsClosedContour(Layout layout)
    {
        var geometry = new PolyhexGeometry(new[,] { { true } }, Apothem);

        Segment[] extendedContour = geometry.ToApothemOffsetContour(layout);

        Assert.That(extendedContour, Is.Not.Empty);
        AssertContourIsClosed(extendedContour);
    }

    [Test]
    public void ToApothemOffsetContour_WithoutLayout_UsesOddR()
    {
        PolyhexGeometry geometry = CreateGeometry();

        Assert.That(
            geometry.ToApothemOffsetContour(),
            Is.EqualTo(geometry.ToApothemOffsetContour(Layout.OddR)));
    }

    [Test]
    public void ToApothemOffsetContour_WhenPolyhexIsEmpty_ReturnsEmptyArray()
    {
        var geometry = new PolyhexGeometry(new[,] { { false } }, Apothem);

        Assert.That(geometry.ToApothemOffsetContour(), Is.Empty);
    }

    private static PolyhexGeometry CreateGeometry()
    {
        return new PolyhexGeometry(
            new bool[,]
            {
                { false, true,  true,  false },
                { true,  true,  true,  false },
                { true,  false, true,  true  },
                { false, true,  true,  true  },
                { false, false, true,  false }
            },
            Apothem);
    }

    private static bool HasSourceContourAtApothemDistance(
        PointXY point,
        Segment[] sourceContour)
    {
        for (int i = 0; i < sourceContour.Length; i++)
        {
            Line sourceLine = new Line(sourceContour[i].EndpointA, sourceContour[i].EndpointB);
            if (sourceLine.Distance(point).AlmostEquals(Apothem, Epsilon))
                return true;
        }

        return false;
    }

    private static void AssertContourIsClosed(Segment[] contour)
    {
        var endpointGroups = new List<List<PointXY>>();

        for (int i = 0; i < contour.Length; i++)
        {
            AddEndpoint(endpointGroups, contour[i].EndpointA);
            AddEndpoint(endpointGroups, contour[i].EndpointB);
        }

        for (int i = 0; i < endpointGroups.Count; i++)
        {
            Assert.That(
                endpointGroups[i],
                Has.Count.EqualTo(2),
                $"Contour endpoint group {i} must have degree 2.");
        }
    }

    private static void AddEndpoint(List<List<PointXY>> endpointGroups, PointXY endpoint)
    {
        for (int i = 0; i < endpointGroups.Count; i++)
        {
            if (endpointGroups[i][0].AlmostEquals(endpoint, Epsilon))
            {
                endpointGroups[i].Add(endpoint);
                return;
            }
        }

        endpointGroups.Add(new List<PointXY> { endpoint });
    }
}
