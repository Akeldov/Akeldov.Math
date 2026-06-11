using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class CurveIntersectionFuzzTests
{
    private const int IterationCount = 200;
    private const float IntersectionEpsilon = 1e-3f;
    private const float AssertionEpsilon = 2e-3f;

    [TestCase(15277)]
    [TestCase(89431)]
    public void SegmentRayIntersections_WithGeneratedInputs_ReturnPointsOnSegmentAndRay(int seed)
    {
        var random = new Random(seed);

        for (int iteration = 0; iteration < IterationCount; iteration++)
        {
            var segment = CreateSegment(random);
            var ray = CreateRay(random);

            var intersections = segment.GetRayIntersections(ray, IntersectionEpsilon);

            Assert.That(
                intersections,
                Has.Count.LessThanOrEqualTo(1),
                CaseMessage(seed, iteration, "A segment and a ray should produce at most one returned point."));

            for (int i = 0; i < intersections.Count; i++)
            {
                PointXY point = intersections[i];

                Assert.That(IsFinite(point), Is.True, CaseMessage(seed, iteration, "Intersection must be finite."));
                Assert.That(
                    segment.Distance(point),
                    Is.LessThanOrEqualTo(AssertionEpsilon),
                    CaseMessage(seed, iteration, "Intersection must lie on the segment."));
                Assert.That(
                    IsPointOnRay(point, ray, AssertionEpsilon),
                    Is.True,
                    CaseMessage(seed, iteration, "Intersection must lie on the ray."));
            }
        }
    }

    [TestCase(26513)]
    [TestCase(78121)]
    public void CircularRayIntersections_WithGeneratedInputs_ReturnPointsOnCurveAndRay(int seed)
    {
        var random = new Random(seed);

        for (int iteration = 0; iteration < IterationCount; iteration++)
        {
            PointXY center = NextPoint(random);
            float radius = NextPositiveFloat(random, 0.01f, 100f);
            var circle = new Circle(center, radius);
            var arc = CreateArc(random, center, radius);
            var ray = CreateRay(random);

            AssertIntersectionsLieOnCurveAndRay(
                circle.GetRayIntersections(ray, IntersectionEpsilon),
                circle.Distance,
                ray,
                seed,
                iteration);

            AssertIntersectionsLieOnCurveAndRay(
                arc.GetRayIntersections(ray, IntersectionEpsilon),
                arc.Distance,
                ray,
                seed,
                iteration);
        }
    }

    private static void AssertIntersectionsLieOnCurveAndRay(
        IReadOnlyList<PointXY> intersections,
        Func<PointXY, float> distanceToCurve,
        Ray ray,
        int seed,
        int iteration)
    {
        Assert.That(
            intersections,
            Has.Count.LessThanOrEqualTo(2),
            CaseMessage(seed, iteration, "A circular curve and a ray should produce at most two returned points."));

        for (int i = 0; i < intersections.Count; i++)
        {
            PointXY point = intersections[i];

            Assert.That(IsFinite(point), Is.True, CaseMessage(seed, iteration, "Intersection must be finite."));
            Assert.That(
                distanceToCurve(point),
                Is.LessThanOrEqualTo(AssertionEpsilon),
                CaseMessage(seed, iteration, "Intersection must lie on the circular curve."));
            Assert.That(
                IsPointOnRay(point, ray, AssertionEpsilon),
                Is.True,
                CaseMessage(seed, iteration, "Intersection must lie on the ray."));
        }
    }

    private static Segment CreateSegment(Random random)
    {
        PointXY start = NextPoint(random);

        if (random.Next(5) == 0)
        {
            float dx = NextSignedFloat(random, 0f, GeometryConstants.GeometryEpsilon * 0.75f);
            float dy = NextSignedFloat(random, 0f, GeometryConstants.GeometryEpsilon * 0.75f);
            return new Segment(start, start + new VectorXY(dx, dy));
        }

        return new Segment(start, NextPoint(random));
    }

    private static Arc CreateArc(Random random, PointXY center, float radius)
    {
        float startAngle = NextAngle(random);
        float endAngle = random.Next(5) == 0
            ? startAngle + NextSignedFloat(random, 0f, GeometryConstants.GeometryEpsilon * 0.75f)
            : startAngle + NextSignedFloat(random, 0f, 2f * MathF.PI);

        return new Arc(center, radius, startAngle, endAngle);
    }

    private static Ray CreateRay(Random random)
    {
        return new Ray(NextPoint(random), NextAngle(random));
    }

    private static PointXY NextPoint(Random random)
    {
        return new PointXY(NextCoordinate(random), NextCoordinate(random));
    }

    private static float NextCoordinate(Random random)
    {
        return random.Next(8) switch
        {
            0 => 0f,
            1 => NextSignedFloat(random, 0f, GeometryConstants.GeometryEpsilon * 2f),
            2 => NextSignedFloat(random, 0f, 1e-3f),
            3 => NextSignedFloat(random, 0.5f, 2f),
            4 => NextSignedFloat(random, 50f, 100f),
            _ => NextSignedFloat(random, 0f, 100f)
        };
    }

    private static float NextAngle(Random random)
    {
        return random.Next(8) switch
        {
            0 => 0f,
            1 => MathF.PI / 2f,
            2 => MathF.PI,
            3 => 2f * MathF.PI,
            _ => NextSignedFloat(random, 0f, 2f * MathF.PI)
        };
    }

    private static float NextPositiveFloat(Random random, float min, float max)
    {
        return min + (max - min) * random.NextSingle();
    }

    private static float NextSignedFloat(Random random, float minMagnitude, float maxMagnitude)
    {
        float magnitude = NextPositiveFloat(random, minMagnitude, maxMagnitude);
        return random.Next(2) == 0 ? magnitude : -magnitude;
    }

    private static bool IsPointOnRay(PointXY point, Ray ray, float epsilon)
    {
        VectorXY delta = point - ray.Origin;
        float rayCoordinate = VectorXY.Dot(delta, ray.Direction);

        return rayCoordinate >= -epsilon &&
            MathF.Abs(VectorXY.Cross(delta, ray.Direction)) <= epsilon;
    }

    private static bool IsFinite(PointXY point)
    {
        return !float.IsNaN(point.X) &&
            !float.IsNaN(point.Y) &&
            !float.IsInfinity(point.X) &&
            !float.IsInfinity(point.Y);
    }

    private static string CaseMessage(int seed, int iteration, string message)
    {
        return $"{message} Seed: {seed}, iteration: {iteration}.";
    }
}
