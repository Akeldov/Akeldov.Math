using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

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
            PointXY[] points = Enumerable.Range(0, count)
                .Select(_ => new PointXY((float)(20 * random.NextDouble() - 10), (float)(20 * random.NextDouble() - 10))).ToArray();
            float[] weights = Enumerable.Range(0, count).Select(_ => (float)(0.1 + random.NextDouble() * 5)).ToArray();
            float[] interior = Enumerable.Range(0, count - degree - 1).Select(_ => (float)(0.01 + 0.98 * random.NextDouble())).OrderBy(x => x).ToArray();
            float[] knots = Enumerable.Repeat(0f, degree + 1).Concat(interior).Concat(Enumerable.Repeat(1f, degree + 1)).ToArray();
            var curve = new Nurbs(degree, points, weights, knots, 8);
            var rescaled = new Nurbs(degree, points, weights.Select(w => w * 16f).ToArray(), knots, 8);

            for (int sample = 0; sample < 30; sample++)
            {
                float t = sample == 0 ? 0f : (float)random.NextDouble();
                PointXY actual = curve.GetPointAt(t);
                PointXY expected = EvaluateBasisSum(points, weights, knots, degree, t);
                string context = $"Seed {seed}, scenario {scenario}, sample {sample}, degree {degree}, t {t}";
                Assert.That(actual.X, Is.EqualTo(expected.X).Within(2e-6f), context);
                Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(2e-6f), context);
                Assert.That(rescaled.GetPointAt(t), Is.EqualTo(actual), context);
                Assert.That(actual.X, Is.InRange(points.Min(p => p.X) - 1e-6f, points.Max(p => p.X) + 1e-6f), context);
                Assert.That(actual.Y, Is.InRange(points.Min(p => p.Y) - 1e-6f, points.Max(p => p.Y) + 1e-6f), context);

                ParameterizedCurveProjection projection = curve.ProjectWithParameter(actual);
                PointXY roundTrip = curve.GetPoint(projection.CurveCoordinate);
                Assert.That(roundTrip.Distance(projection.ProjectedPoint), Is.LessThan(2e-5f), context);
            }

            Assert.That(curve.GetPointAt(1f), Is.EqualTo(points[count - 1]), $"Seed {seed}, scenario {scenario}, endpoint");
        }
    }

    // Cox-de Boor basis recurrence provides an oracle independent of homogeneous point evaluation.
    private static PointXY EvaluateBasisSum(PointXY[] points, float[] weights, float[] knots, int degree, float t)
    {
        var basis = new double[knots.Length - 1];
        for (int i = 0; i < basis.Length; i++)
            basis[i] = knots[i] <= t && t < knots[i + 1] ? 1.0 : 0.0;

        for (int order = 1; order <= degree; order++)
        {
            for (int i = 0; i < basis.Length - order; i++)
            {
                double leftWidth = (double)knots[i + order] - knots[i];
                double rightWidth = (double)knots[i + order + 1] - knots[i + 1];
                basis[i] = (leftWidth == 0.0 ? 0.0 : (t - (double)knots[i]) / leftWidth * basis[i])
                    + (rightWidth == 0.0 ? 0.0 : (knots[i + order + 1] - (double)t) / rightWidth * basis[i + 1]);
            }
        }

        double x = 0.0;
        double y = 0.0;
        double weight = 0.0;
        for (int i = 0; i < points.Length; i++)
        {
            double amount = weights[i] * basis[i];
            x += points[i].X * amount;
            y += points[i].Y * amount;
            weight += amount;
        }

        return new PointXY((float)(x / weight), (float)(y / weight));
    }
}
