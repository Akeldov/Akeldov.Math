using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class ContourSmoothingExtensionsTests
{
    [Test]
    public void FilletCorners_WhenContourIsSegmentSquare_InsertsArcAtEachCorner()
    {
        var contour = new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f)),
            new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(2f, 2f)),
            new ParameterizedSegment(new PointXY(2f, 2f), new PointXY(0f, 2f)),
            new ParameterizedSegment(new PointXY(0f, 2f), new PointXY(0f, 0f))
        });

        CompositeContour smoothed = contour.FilletCorners(0.25f);

        Assert.That(smoothed.Curves, Has.Count.EqualTo(8));
        Assert.That(smoothed.Curves.OfType<ParameterizedSegment>().Count(), Is.EqualTo(4));
        Assert.That(smoothed.Curves.OfType<ParameterizedArc>().Count(), Is.EqualTo(4));
        Assert.That(smoothed.Curves.OfType<ParameterizedArc>().Select(arc => arc.Radius), Is.All.EqualTo(0.25f));

        var firstSegment = (ParameterizedSegment)smoothed.Curves[0];
        AssertVector(firstSegment.StartPoint, 0.25f, 0f);
        AssertVector(firstSegment.EndPoint, 1.75f, 0f);
    }

    [Test]
    public void FilletCorners_WhenOnlySomeAdjacentCurvesAreSegments_InsertsArcsOnlyAtSegmentSegmentCorners()
    {
        var originalArc = new ParameterizedArc(
            new PointXY(2f, 1f),
            1f,
            -0.5f * MathF.PI,
            0.5f * MathF.PI,
            AngularDirection.Counterclockwise);

        var contour = new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f)),
            originalArc,
            new ParameterizedSegment(new PointXY(2f, 2f), new PointXY(0f, 2f)),
            new ParameterizedSegment(new PointXY(0f, 2f), new PointXY(0f, 0f))
        });

        CompositeContour smoothed = contour.FilletCorners(0.25f);

        Assert.That(smoothed.Curves, Has.Count.EqualTo(6));
        Assert.That(smoothed.Curves.OfType<ParameterizedArc>().Count(), Is.EqualTo(3));
        Assert.That(smoothed.Curves.OfType<ParameterizedArc>().Count(arc => arc.Radius.Equals(0.25f)), Is.EqualTo(2));
        Assert.That(smoothed.Curves[1], Is.EqualTo(originalArc));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void FilletCorners_WhenRadiusIsInvalid_Throws(float radius)
    {
        var contour = new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 0f)),
            new ParameterizedSegment(new PointXY(1f, 0f), new PointXY(0f, 1f)),
            new ParameterizedSegment(new PointXY(0f, 1f), new PointXY(0f, 0f))
        });

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => contour.FilletCorners(radius));

        Assert.That(exception!.ParamName, Is.EqualTo("radius"));
    }

    private static void AssertVector(PointXY actual, float expectedX, float expectedY)
    {
        Assert.That(actual.X, Is.EqualTo(expectedX).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(actual.Y, Is.EqualTo(expectedY).Within(GeometryConstants.GeometryEpsilon));
    }
}
