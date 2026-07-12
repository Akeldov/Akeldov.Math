using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Hexes.Tests.Geometry.Contours;

public class HexMatrixApothemOffsetRegionTests
{
    private const float Radius = 1.25f;
    private static readonly float Apothem = Radius.ConvertHexRadiusToApothem();
    private const float Epsilon = 1e-4f;

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToApothemOffsetRegion_StaysAtApothemDistanceFromSourceRegion(Layout layout)
    {
        PolyhexGeometry geometry = CreateGeometry();

        ContourBasedRegion sourceRegion = geometry.ToRegion(layout);
        ContourBasedRegion offsetRegion = geometry.ToApothemOffsetRegion(layout);

        Assert.That(offsetRegion.Contours, Is.Not.Empty);
        AssertEveryCurvePointStaysAtApothemDistance(sourceRegion, offsetRegion);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToApothemOffsetRegion_UsesApothemRadiusForConvexJoins(Layout layout)
    {
        PolyhexGeometry geometry = CreateGeometry();

        ContourBasedRegion offsetRegion = geometry.ToApothemOffsetRegion(layout);
        ParameterizedArc[] offsetArcs = GetArcs(offsetRegion);

        Assert.That(offsetArcs, Is.Not.Empty);
        for (int i = 0; i < offsetArcs.Length; i++)
            Assert.That(offsetArcs[i].Radius, Is.EqualTo(Apothem).Within(Epsilon));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToApothemOffsetRegion_ReturnsClosedContours(Layout layout)
    {
        PolyhexGeometry geometry = CreateGeometry();

        ContourBasedRegion offsetRegion = geometry.ToApothemOffsetRegion(layout);

        Assert.That(offsetRegion.Contours, Is.Not.Empty);
        AssertRegionContoursAreClosed(offsetRegion);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ToApothemOffsetRegion_ReturnsSelfIntersectionFreeContours(Layout layout)
    {
        PolyhexGeometry geometry = CreateGeometry();

        ContourBasedRegion offsetRegion = geometry.ToApothemOffsetRegion(layout);

        AssertRegionContoursHaveNoNonAdjacentIntersections(offsetRegion);
    }

    [Test]
    public void ToApothemOffsetRegion_WithoutLayout_UsesOddR()
    {
        PolyhexGeometry geometry = CreateGeometry();

        AssertContoursAreEqual(
            geometry.ToApothemOffsetRegion(Layout.OddR),
            geometry.ToApothemOffsetRegion());
    }

    [Test]
    public void ToApothemOffsetRegion_WhenPolyhexIsEmpty_Throws()
    {
        var geometry = new PolyhexGeometry(new[,] { { false } }, Radius);

        Assert.Throws<InvalidOperationException>(() => geometry.ToApothemOffsetRegion());
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
            Radius);
    }

    private static void AssertEveryCurvePointStaysAtApothemDistance(
        ContourBasedRegion sourceRegion,
        ContourBasedRegion offsetRegion)
    {
        int curveIndex = 0;

        for (int contourIndex = 0; contourIndex < offsetRegion.Contours.Count; contourIndex++)
        {
            ICompositeContour contour = GetCompositeContour(offsetRegion.Contours[contourIndex]);

            for (int i = 0; i < contour.Curves.Count; i++)
            {
                IFinitePath curve = contour.Curves[i];
                int sampleCount = System.Math.Max(2, (int)MathF.Ceiling(curve.Length / (Apothem * 0.25f)));

                for (int j = 0; j <= sampleCount; j++)
                {
                    float coordinate = j == sampleCount ? curve.Length : curve.Length * j / sampleCount;
                    PointXY point = curve.GetPoint(coordinate);

                    Assert.That(
                        sourceRegion.Distance(point),
                        Is.EqualTo(Apothem).Within(Epsilon),
                        $"Offset curve {curveIndex} sample {j} must be at apothem distance from the source region.");
                }

                curveIndex++;
            }
        }
    }

    private static void AssertRegionContoursAreClosed(ContourBasedRegion region)
    {
        for (int i = 0; i < region.Contours.Count; i++)
            AssertContourIsClosed(GetCompositeContour(region.Contours[i]));
    }

    private static void AssertContourIsClosed(ICompositeContour contour)
    {
        for (int i = 0; i < contour.Curves.Count; i++)
        {
            IFinitePath current = contour.Curves[i];
            IFinitePath next = contour.Curves[(i + 1) % contour.Curves.Count];

            Assert.That(
                current.EndPoint.AlmostEquals(next.StartPoint, Epsilon),
                Is.True,
                $"Contour curve {i} end point must match the next curve start point.");
        }
    }

    private static void AssertRegionContoursHaveNoNonAdjacentIntersections(ContourBasedRegion region)
    {
        for (int i = 0; i < region.Contours.Count; i++)
            AssertContourHasNoNonAdjacentIntersections(GetCompositeContour(region.Contours[i]));
    }

    private static void AssertContourHasNoNonAdjacentIntersections(ICompositeContour contour)
    {
        for (int i = 0; i < contour.Curves.Count; i++)
        {
            for (int j = i + 1; j < contour.Curves.Count; j++)
            {
                if (AreAdjacent(i, j, contour.Curves.Count))
                    continue;

                List<PointXY> intersections = GetIntersections(contour.Curves[i], contour.Curves[j]);

                Assert.That(
                    intersections,
                    Is.Empty,
                    $"Offset contour curves {i} and {j} must not intersect.");
            }
        }
    }

    private static bool AreAdjacent(int firstIndex, int secondIndex, int count)
    {
        return secondIndex == firstIndex + 1 || (firstIndex == 0 && secondIndex == count - 1);
    }

    private static List<PointXY> GetIntersections(IFinitePath first, IFinitePath second)
    {
        var intersections = new List<PointXY>();

        if (first is ParameterizedSegment firstSegment &&
            second is ParameterizedSegment secondSegment)
        {
            AddSegmentSegmentIntersections(intersections, firstSegment, secondSegment);
        }
        else if (first is ParameterizedSegment segment &&
            second is ParameterizedArc arc)
        {
            AddSegmentArcIntersections(intersections, segment, arc);
        }
        else if (first is ParameterizedArc firstArc &&
            second is ParameterizedSegment segmentB)
        {
            AddSegmentArcIntersections(intersections, segmentB, firstArc);
        }
        else if (first is ParameterizedArc arcA &&
            second is ParameterizedArc arcB)
        {
            AddArcArcIntersections(intersections, arcA, arcB);
        }

        return intersections;
    }

    private static void AddSegmentSegmentIntersections(
        List<PointXY> intersections,
        ParameterizedSegment first,
        ParameterizedSegment second)
    {
        VectorXY firstDirection = first.EndPoint - first.StartPoint;
        VectorXY secondDirection = second.EndPoint - second.StartPoint;
        float cross = VectorXY.Cross(firstDirection, secondDirection);

        if (cross.IsAlmostZero(Epsilon))
        {
            AddIfPointIsOnSegment(intersections, first.StartPoint, second);
            AddIfPointIsOnSegment(intersections, first.EndPoint, second);
            AddIfPointIsOnSegment(intersections, second.StartPoint, first);
            AddIfPointIsOnSegment(intersections, second.EndPoint, first);
            return;
        }

        VectorXY originDelta = second.StartPoint - first.StartPoint;
        float firstCoordinate = VectorXY.Cross(originDelta, secondDirection) / cross;
        float secondCoordinate = VectorXY.Cross(originDelta, firstDirection) / cross;

        if (IsNormalizedCoordinateInside(firstCoordinate) &&
            IsNormalizedCoordinateInside(secondCoordinate))
        {
            AddDistinct(intersections, first.StartPoint + firstDirection * firstCoordinate);
        }
    }

    private static void AddIfPointIsOnSegment(
        List<PointXY> intersections,
        PointXY point,
        ParameterizedSegment segment)
    {
        VectorXY segmentVector = segment.EndPoint - segment.StartPoint;
        VectorXY startToPoint = point - segment.StartPoint;

        if (!VectorXY.Cross(segmentVector, startToPoint).IsAlmostZero(Epsilon))
            return;

        float dot = VectorXY.Dot(startToPoint, segmentVector);
        if (dot < -Epsilon || dot > segmentVector.SquaredLength + Epsilon)
            return;

        AddDistinct(intersections, point);
    }

    private static void AddSegmentArcIntersections(
        List<PointXY> intersections,
        ParameterizedSegment segment,
        ParameterizedArc arc)
    {
        VectorXY direction = segment.EndPoint - segment.StartPoint;
        VectorXY startToCenter = segment.StartPoint - arc.Center;

        float a = VectorXY.Dot(direction, direction);
        if (a <= Epsilon * Epsilon)
            return;

        float b = 2f * VectorXY.Dot(startToCenter, direction);
        float c = startToCenter.SquaredLength - arc.Radius * arc.Radius;
        float discriminant = b * b - 4f * a * c;

        if (discriminant < -Epsilon)
            return;

        if (discriminant < 0f)
            discriminant = 0f;

        float sqrtDiscriminant = MathF.Sqrt(discriminant);
        AddSegmentArcIntersection(intersections, segment, arc, (-b - sqrtDiscriminant) / (2f * a));
        AddSegmentArcIntersection(intersections, segment, arc, (-b + sqrtDiscriminant) / (2f * a));
    }

    private static void AddSegmentArcIntersection(
        List<PointXY> intersections,
        ParameterizedSegment segment,
        ParameterizedArc arc,
        float normalizedCoordinate)
    {
        if (!IsNormalizedCoordinateInside(normalizedCoordinate))
            return;

        PointXY point = segment.StartPoint + (segment.EndPoint - segment.StartPoint) * normalizedCoordinate;
        if (arc.IsWithinAngularRegion(point))
            AddDistinct(intersections, point);
    }

    private static void AddArcArcIntersections(
        List<PointXY> intersections,
        ParameterizedArc first,
        ParameterizedArc second)
    {
        VectorXY centerDelta = second.Center - first.Center;
        float centerDistance = centerDelta.Length;

        if (centerDistance <= Epsilon)
            return;

        if (centerDistance > first.Radius + second.Radius + Epsilon)
            return;

        if (centerDistance < MathF.Abs(first.Radius - second.Radius) - Epsilon)
            return;

        float a = (first.Radius * first.Radius -
            second.Radius * second.Radius +
            centerDistance * centerDistance) / (2f * centerDistance);
        float hSquared = first.Radius * first.Radius - a * a;

        if (hSquared < -Epsilon)
            return;

        if (hSquared < 0f)
            hSquared = 0f;

        VectorXY direction = centerDelta / centerDistance;
        PointXY basePoint = first.Center + direction * a;
        VectorXY perpendicular = new VectorXY(-direction.Y, direction.X) * MathF.Sqrt(hSquared);

        AddArcArcIntersection(intersections, first, second, basePoint + perpendicular);
        AddArcArcIntersection(intersections, first, second, basePoint - perpendicular);
    }

    private static void AddArcArcIntersection(
        List<PointXY> intersections,
        ParameterizedArc first,
        ParameterizedArc second,
        PointXY point)
    {
        if (first.IsWithinAngularRegion(point) && second.IsWithinAngularRegion(point))
            AddDistinct(intersections, point);
    }

    private static void AddDistinct(List<PointXY> points, PointXY point)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].AlmostEquals(point, Epsilon))
                return;
        }

        points.Add(point);
    }

    private static bool IsNormalizedCoordinateInside(float coordinate)
    {
        return coordinate >= -Epsilon && coordinate <= 1f + Epsilon;
    }

    private static ParameterizedArc[] GetArcs(ContourBasedRegion region)
    {
        return region.Contours
            .SelectMany(contour => GetCompositeContour(contour).Curves)
            .OfType<ParameterizedArc>()
            .ToArray();
    }

    private static ICompositeContour GetCompositeContour(IContour contour)
    {
        Assert.That(contour, Is.InstanceOf<ICompositeContour>());
        return (ICompositeContour)contour;
    }

    private static void AssertContoursAreEqual(ContourBasedRegion expected, ContourBasedRegion actual)
    {
        Assert.That(actual.Contours, Has.Count.EqualTo(expected.Contours.Count));

        for (int contourIndex = 0; contourIndex < expected.Contours.Count; contourIndex++)
        {
            ICompositeContour expectedContour = GetCompositeContour(expected.Contours[contourIndex]);
            ICompositeContour actualContour = GetCompositeContour(actual.Contours[contourIndex]);

            Assert.That(actualContour.Curves, Has.Count.EqualTo(expectedContour.Curves.Count));

            for (int i = 0; i < expectedContour.Curves.Count; i++)
                Assert.That(actualContour.Curves[i], Is.EqualTo(expectedContour.Curves[i]));

            Assert.That(actualContour.Length, Is.EqualTo(expectedContour.Length).Within(Epsilon));
        }
    }
}
