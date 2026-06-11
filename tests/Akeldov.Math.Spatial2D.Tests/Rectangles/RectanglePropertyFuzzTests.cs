using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Rectangles;

public class RectanglePropertyFuzzTests
{
    private const int IterationCount = 500;

    [TestCase(31415)]
    [TestCase(27182)]
    public void Contains_WithGeneratedRectangles_MatchesExpandedBounds(int seed)
    {
        var random = new Random(seed);

        for (int iteration = 0; iteration < IterationCount; iteration++)
        {
            float minX = NextCoordinate(random);
            float minY = NextCoordinate(random);
            float width = NextPositiveFloat(random, 1e-4f, 500f);
            float height = NextPositiveFloat(random, 1e-4f, 500f);
            var rectangle = new Rectangle(
                new PointXY(minX, minY),
                new PointXY(minX + width, minY + height));
            float geometryEpsilon = NextGeometryEpsilon(random);
            PointXY point = NextPointAroundRectangle(random, rectangle, geometryEpsilon);

            bool expected = point.X >= rectangle.Min.X - geometryEpsilon &&
                point.X <= rectangle.Max.X + geometryEpsilon &&
                point.Y >= rectangle.Min.Y - geometryEpsilon &&
                point.Y <= rectangle.Max.Y + geometryEpsilon;

            Assert.That(
                rectangle.Contains(point, geometryEpsilon),
                Is.EqualTo(expected),
                $"Seed: {seed}, iteration: {iteration}, rectangle: {rectangle}, point: {point}, epsilon: {geometryEpsilon}.");
        }
    }

    [TestCase(16180)]
    [TestCase(14142)]
    public void ToContourSignedDistance_WithGeneratedPoints_HasSignConsistentWithContainsAwayFromBoundary(int seed)
    {
        var random = new Random(seed);

        for (int iteration = 0; iteration < IterationCount; iteration++)
        {
            float minX = NextCoordinate(random);
            float minY = NextCoordinate(random);
            float width = NextPositiveFloat(random, 1f, 500f);
            float height = NextPositiveFloat(random, 1f, 500f);
            var rectangle = new Rectangle(
                new PointXY(minX, minY),
                new PointXY(minX + width, minY + height));
            IContourBasedRegion region = rectangle.ToRegion();
            PointXY point = NextPointAroundRectangle(random, rectangle, geometryEpsilon: 0f);

            float distanceToBoundary = GetDistanceToBoundary(rectangle, point);
            if (distanceToBoundary <= 0.01f)
                continue;

            bool contains = rectangle.Contains(point, geometryEpsilon: 0f);
            float signedDistance = region.Contours[0].SignedDistance(point, geometryEpsilon: 0f);

            Assert.That(
                signedDistance <= 0f,
                Is.EqualTo(contains),
                $"Seed: {seed}, iteration: {iteration}, rectangle: {rectangle}, point: {point}, signed distance: {signedDistance}.");
        }
    }

    private static PointXY NextPointAroundRectangle(Random random, Rectangle rectangle, float geometryEpsilon)
    {
        if (random.Next(4) == 0)
        {
            float x = random.Next(2) == 0
                ? rectangle.Min.X + NextSignedFloat(random, 0f, geometryEpsilon * 2f + 0.01f)
                : rectangle.Max.X + NextSignedFloat(random, 0f, geometryEpsilon * 2f + 0.01f);
            float y = NextPositiveFloat(random, rectangle.Min.Y, rectangle.Max.Y);
            return new PointXY(x, y);
        }

        if (random.Next(4) == 0)
        {
            float x = NextPositiveFloat(random, rectangle.Min.X, rectangle.Max.X);
            float y = random.Next(2) == 0
                ? rectangle.Min.Y + NextSignedFloat(random, 0f, geometryEpsilon * 2f + 0.01f)
                : rectangle.Max.Y + NextSignedFloat(random, 0f, geometryEpsilon * 2f + 0.01f);
            return new PointXY(x, y);
        }

        float marginX = rectangle.Width * 0.5f + geometryEpsilon + 0.01f;
        float marginY = rectangle.Height * 0.5f + geometryEpsilon + 0.01f;
        return new PointXY(
            NextPositiveFloat(random, rectangle.Min.X - marginX, rectangle.Max.X + marginX),
            NextPositiveFloat(random, rectangle.Min.Y - marginY, rectangle.Max.Y + marginY));
    }

    private static float GetDistanceToBoundary(Rectangle rectangle, PointXY point)
    {
        if (rectangle.Contains(point, geometryEpsilon: 0f))
        {
            return MathF.Min(
                MathF.Min(point.X - rectangle.Min.X, rectangle.Max.X - point.X),
                MathF.Min(point.Y - rectangle.Min.Y, rectangle.Max.Y - point.Y));
        }

        float dx = point.X < rectangle.Min.X
            ? rectangle.Min.X - point.X
            : MathF.Max(0f, point.X - rectangle.Max.X);
        float dy = point.Y < rectangle.Min.Y
            ? rectangle.Min.Y - point.Y
            : MathF.Max(0f, point.Y - rectangle.Max.Y);

        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float NextGeometryEpsilon(Random random)
    {
        return random.Next(4) == 0
            ? 0f
            : NextPositiveFloat(random, 0f, 0.25f);
    }

    private static float NextCoordinate(Random random)
    {
        return random.Next(6) switch
        {
            0 => 0f,
            1 => NextSignedFloat(random, 0f, GeometryConstants.GeometryEpsilon * 2f),
            2 => NextSignedFloat(random, 0f, 1f),
            3 => NextSignedFloat(random, 100f, 1_000f),
            _ => NextSignedFloat(random, 0f, 1_000f)
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
}
