using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Stress;

[Explicit("Stress tests are excluded from normal test runs.")]
[Category("Stress")]
public class RegionStressTests
{
    [TestCase(6101)]
    [TestCase(6102)]
    [TestCase(6103)]
    public void RandomRectangleRegions_WithManySeeds_KeepSignedDistanceSignConsistentAwayFromBoundary(int seed)
    {
        var random = new Random(seed);

        for (int i = 0; i < 1_000; i++)
        {
            Rectangle rectangle = CreateRectangle(random);
            IContour contour = rectangle.ToContour();
            PointXY point = NextPointAround(rectangle, random);
            float boundaryDistance = GetDistanceToBoundary(rectangle, point);

            if (boundaryDistance <= 0.01f)
                continue;

            bool contains = rectangle.Contains(point);
            float signedDistance = contour.SignedDistance(point);

            Assert.That(float.IsNaN(signedDistance), Is.False, $"Seed: {seed}, iteration: {i}.");
            Assert.That(float.IsInfinity(signedDistance), Is.False, $"Seed: {seed}, iteration: {i}.");
            Assert.That(signedDistance <= 0f, Is.EqualTo(contains), $"Seed: {seed}, iteration: {i}.");
        }
    }

    private static Rectangle CreateRectangle(Random random)
    {
        float minX = NextFloat(random, -1_000f, 1_000f);
        float minY = NextFloat(random, -1_000f, 1_000f);
        float width = NextFloat(random, 0.1f, 500f);
        float height = NextFloat(random, 0.1f, 500f);

        return new Rectangle(
            new PointXY(minX, minY),
            new PointXY(minX + width, minY + height));
    }

    private static PointXY NextPointAround(Rectangle rectangle, Random random)
    {
        float marginX = rectangle.Width * 0.5f + 10f;
        float marginY = rectangle.Height * 0.5f + 10f;

        return new PointXY(
            NextFloat(random, rectangle.Min.X - marginX, rectangle.Max.X + marginX),
            NextFloat(random, rectangle.Min.Y - marginY, rectangle.Max.Y + marginY));
    }

    private static float GetDistanceToBoundary(Rectangle rectangle, PointXY point)
    {
        if (rectangle.Contains(point))
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

    private static float NextFloat(Random random, float min, float max)
    {
        return min + (max - min) * random.NextSingle();
    }
}
