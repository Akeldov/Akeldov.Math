using Akeldov.Math.Spatial3D.Curves;

namespace Akeldov.Math.Spatial3D.Tests.Curves;

public class NurbsPropertyFuzzTests
{
    [TestCase(1709)]
    [TestCase(81173)]
    [TestCase(20260905)]
    public void Evaluation_MatchesIndependentBasisSumAndWeightScaling(int seed)
    {
        var random = new Random(seed);
        for (int scenario = 0; scenario < 60; scenario++)
        {
            int degree = random.Next(1, 6);
            int count = degree + 1 + random.Next(6);
            PointXYZ[] points = Enumerable.Range(0, count)
                .Select(_ => new PointXYZ(
                    (float)(20 * random.NextDouble() - 10),
                    (float)(20 * random.NextDouble() - 10),
                    (float)(20 * random.NextDouble() - 10)))
                .ToArray();
            float[] weights = Enumerable.Range(0, count)
                .Select(_ => (float)(0.1 + random.NextDouble() * 5))
                .ToArray();
            float[] interior = Enumerable.Range(0, count - degree - 1)
                .Select(_ => (float)(0.01 + 0.98 * random.NextDouble()))
                .OrderBy(value => value)
                .ToArray();
            float[] knots = Enumerable.Repeat(0f, degree + 1)
                .Concat(interior)
                .Concat(Enumerable.Repeat(1f, degree + 1))
                .ToArray();
            var curve = new Nurbs(degree, points, weights, knots, 8);
            var rescaled = new Nurbs(
                degree,
                points,
                weights.Select(weight => weight * 16f).ToArray(),
                knots,
                8);

            for (int sample = 0; sample < 30; sample++)
            {
                float t = sample == 0 ? 0f : (float)random.NextDouble();
                PointXYZ actual = curve.GetPointAt(t);
                PointXYZ expected = EvaluateBasisSum(points, weights, knots, degree, t);
                string context =
                    $"Seed {seed}, scenario {scenario}, sample {sample}, degree {degree}, t {t}";

                Assert.Multiple(() =>
                {
                    Assert.That(actual.X, Is.EqualTo(expected.X).Within(2e-6f), context);
                    Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(2e-6f), context);
                    Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(2e-6f), context);
                    Assert.That(rescaled.GetPointAt(t), Is.EqualTo(actual), context);
                    Assert.That(
                        actual.X,
                        Is.InRange(points.Min(point => point.X) - 1e-6f, points.Max(point => point.X) + 1e-6f),
                        context);
                    Assert.That(
                        actual.Y,
                        Is.InRange(points.Min(point => point.Y) - 1e-6f, points.Max(point => point.Y) + 1e-6f),
                        context);
                    Assert.That(
                        actual.Z,
                        Is.InRange(points.Min(point => point.Z) - 1e-6f, points.Max(point => point.Z) + 1e-6f),
                        context);
                });

                ParameterizedCurveProjection projection = curve.ProjectWithParameter(actual);
                PointXYZ roundTrip = curve.GetPoint(projection.CurveCoordinate);
                Assert.That(
                    GetDistance(roundTrip, projection.ProjectedPoint),
                    Is.LessThan(2e-5f),
                    context);
            }

            Assert.That(
                curve.GetPointAt(1f),
                Is.EqualTo(points[count - 1]),
                $"Seed {seed}, scenario {scenario}, endpoint");
        }
    }

    // Cox-de Boor basis recurrence provides an oracle independent of homogeneous point evaluation.
    private static PointXYZ EvaluateBasisSum(
        PointXYZ[] points,
        float[] weights,
        float[] knots,
        int degree,
        float t)
    {
        var basis = new double[knots.Length - 1];
        for (int i = 0; i < basis.Length; i++)
            basis[i] = knots[i] <= t && t < knots[i + 1] ? 1d : 0d;

        for (int order = 1; order <= degree; order++)
        {
            for (int i = 0; i < basis.Length - order; i++)
            {
                double leftWidth = (double)knots[i + order] - knots[i];
                double rightWidth = (double)knots[i + order + 1] - knots[i + 1];
                basis[i] =
                    (leftWidth == 0d ? 0d : (t - (double)knots[i]) / leftWidth * basis[i]) +
                    (rightWidth == 0d
                        ? 0d
                        : (knots[i + order + 1] - (double)t) / rightWidth * basis[i + 1]);
            }
        }

        double x = 0d;
        double y = 0d;
        double z = 0d;
        double weightSum = 0d;
        for (int i = 0; i < points.Length; i++)
        {
            double amount = weights[i] * basis[i];
            x += points[i].X * amount;
            y += points[i].Y * amount;
            z += points[i].Z * amount;
            weightSum += amount;
        }

        return new PointXYZ(
            (float)(x / weightSum),
            (float)(y / weightSum),
            (float)(z / weightSum));
    }

    private static float GetDistance(PointXYZ left, PointXYZ right)
    {
        double dx = (double)right.X - left.X;
        double dy = (double)right.Y - left.Y;
        double dz = (double)right.Z - left.Z;
        return (float)global::System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
