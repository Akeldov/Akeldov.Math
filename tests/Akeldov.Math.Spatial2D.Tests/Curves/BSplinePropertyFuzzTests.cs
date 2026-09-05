using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class BSplinePropertyFuzzTests
{
    [TestCase(1709)]
    [TestCase(81173)]
    [TestCase(20260905)]
    public void CurveOperations_MatchNurbsWithUnitWeights(int seed)
    {
        var random = new Random(seed);
        for (int scenario = 0; scenario < 60; scenario++)
        {
            int degree = random.Next(1, 6);
            int count = degree + 1 + random.Next(6);
            PointXY[] points = Enumerable.Range(0, count)
                .Select(_ => new PointXY((float)(20 * random.NextDouble() - 10), (float)(20 * random.NextDouble() - 10))).ToArray();
            float[] knots;
            if (scenario % 2 == 0)
            {
                float[] interior = Enumerable.Range(0, count - degree - 1)
                    .Select(_ => (float)(0.01 + 0.98 * random.NextDouble())).OrderBy(x => x).ToArray();
                knots = Enumerable.Repeat(0f, degree + 1).Concat(interior).Concat(Enumerable.Repeat(1f, degree + 1)).ToArray();
            }
            else
            {
                knots = new float[count + degree + 1];
                knots[0] = -5f;
                for (int i = 1; i < knots.Length; i++)
                    knots[i] = knots[i - 1] + (float)(0.01 + random.NextDouble());
            }

            var curve = new BSpline(degree, points, knots, 8);
            var nurbs = new Nurbs(degree, points, Enumerable.Repeat(1f, count).ToArray(), knots, 8);
            string context = $"Seed {seed}, scenario {scenario}, degree {degree}";
            Assert.That(curve.Length, Is.EqualTo(nurbs.Length).Within(1e-5f), context);
            Assert.That(curve.Flatten(), Is.EqualTo(nurbs.Flatten()), context);

            for (int sample = 0; sample <= 20; sample++)
            {
                float t = sample / 20f;
                PointXY actual = curve.GetPointAt(t);
                PointXY expected = nurbs.GetPointAt(t);
                string sampleContext = $"{context}, sample {sample}";
                Assert.That(actual.Distance(expected), Is.LessThan(1e-6f), sampleContext);
                Assert.That(actual.X, Is.InRange(points.Min(p => p.X) - 1e-6f, points.Max(p => p.X) + 1e-6f), sampleContext);
                Assert.That(actual.Y, Is.InRange(points.Min(p => p.Y) - 1e-6f, points.Max(p => p.Y) + 1e-6f), sampleContext);

                var query = new PointXY((float)(30 * random.NextDouble() - 15), (float)(30 * random.NextDouble() - 15));
                ParameterizedCurveProjection projection = curve.ProjectWithParameter(query);
                ParameterizedCurveProjection reference = nurbs.ProjectWithParameter(query);
                Assert.That(projection.ProjectedPoint.Distance(reference.ProjectedPoint), Is.LessThan(1e-5f), sampleContext);
                Assert.That(projection.Distance, Is.EqualTo(reference.Distance).Within(1e-5f), sampleContext);
                Assert.That(projection.CurveCoordinate, Is.EqualTo(reference.CurveCoordinate).Within(1e-5f), sampleContext);
                Assert.That(curve.GetPoint(projection.CurveCoordinate).Distance(projection.ProjectedPoint), Is.LessThan(2e-5f), sampleContext);
                Assert.That(curve.CountRightwardCrossings(query), Is.EqualTo(nurbs.CountRightwardCrossings(query)), sampleContext);
            }
        }
    }
}
