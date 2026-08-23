using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

namespace Akeldov.Math.Spatial2D.Tests.Curves;

public class CompositeContourTests
{
    [Test]
    public void Constructor_WhenCurvesIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeContour((IReadOnlyList<IContourPath>)null!));
    }

    [Test]
    public void Constructor_WhenCurvesIsEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CompositeContour(Array.Empty<IContourPath>()));
    }

    [Test]
    public void Constructor_WhenCurvesContainsNull_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CompositeContour(new IContourPath[] { null! }));
    }

    [Test]
    public void Constructor_WhenCurvesAreDisconnected_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new CompositeContour(new IContourPath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 0f)),
            new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(2f, 1f)),
            new ParameterizedSegment(new PointXY(2f, 1f), new PointXY(0f, 0f))
        }));

        Assert.That(exception!.ParamName, Is.EqualTo("curves"));
    }

    [Test]
    public void Constructor_WhenCurvesDoNotClose_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => new CompositeContour(new IContourPath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(1f, 0f)),
            new ParameterizedSegment(new PointXY(1f, 0f), new PointXY(1f, 1f)),
            new ParameterizedSegment(new PointXY(1f, 1f), new PointXY(0f, 1f))
        }));

        Assert.That(exception!.ParamName, Is.EqualTo("curves"));
    }

    [Test]
    public void Constructor_WhenPointsAreOpen_CreatesClosedSegmentContour()
    {
        var contour = new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f));

        Assert.That(contour.Curves, Has.Count.EqualTo(4));
        Assert.That(contour.Length, Is.EqualTo(8f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(contour.Encloses(new PointXY(1f, 1f)), Is.True);
        Assert.That(contour.Curves[3].EndPoint.AlmostEquals(contour.Curves[0].StartPoint), Is.True);
    }

    [Test]
    public void Constructor_WhenPointListIsProvided_CreatesClosedSegmentContourAndDoesNotRetainList()
    {
        var points = new List<PointXY>
        {
            new PointXY(0f, 0f),
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f)
        };

        var contour = new CompositeContour((IReadOnlyList<PointXY>)points);
        points[1] = new PointXY(20f, 0f);

        Assert.That(contour.Curves, Has.Count.EqualTo(4));
        Assert.That(contour.Length, Is.EqualTo(8f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(contour.Curves[0].EndPoint.AlmostEquals(new PointXY(2f, 0f)), Is.True);
        Assert.That(contour.Encloses(new PointXY(1f, 1f)), Is.True);
    }

    [Test]
    public void Constructor_WhenPointsAreExplicitlyClosed_DoesNotCreateZeroClosingSegment()
    {
        var contour = new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(2f, 0f),
            new PointXY(2f, 2f),
            new PointXY(0f, 2f),
            new PointXY(0f, 0f));

        Assert.That(contour.Curves, Has.Count.EqualTo(4));
        Assert.That(contour.Length, Is.EqualTo(8f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Constructor_WhenPointsAreInvalid_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeContour((PointXY[])null!));
        Assert.Throws<ArgumentNullException>(() => new CompositeContour((IReadOnlyList<PointXY>)null!));
        Assert.Throws<ArgumentException>(() => new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f)));
        Assert.Throws<ArgumentException>(() => new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f),
            new PointXY(0f, 0f)));
        Assert.Throws<ArgumentException>(() => new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f),
            new PointXY(1f, 0f),
            new PointXY(0f, 1f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CompositeContour(
            new PointXY(0f, 0f),
            new PointXY(1f, 0f),
            new PointXY(float.PositiveInfinity, 1f)));
    }

    [Test]
    public void Curves_WhenAccessed_ReturnsReadOnlyView()
    {
        var contour = new CompositeContour(new IContourPath[]
        {
            CreateUnitCirclePath()
        });

        Assert.That(contour.Curves, Is.Not.InstanceOf<IContourPath[]>());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<IContourPath>)contour.Curves)[0] = new ParameterizedSegment(
                new PointXY(0f, 0f),
                new PointXY(1f, 1f)));
    }

    [Test]
    public void IContour_ImplementsPointDistanceProviderContracts()
    {
        IContour contour = CreateSquareContour();

        Assert.That(contour, Is.InstanceOf<ICurve>());
        Assert.That(contour, Is.InstanceOf<IFiniteCurve>());
        Assert.That(contour, Is.InstanceOf<IPointDistanceProvider>());
        Assert.That(contour, Is.InstanceOf<ISignedPointDistanceProvider>());
        Assert.That(contour.Length, Is.EqualTo(8f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void CompositeContour_ImplementsCompositeContourContract()
    {
        ICompositeContour contour = CreateSquareContour();

        Assert.That(contour, Is.InstanceOf<IContour>());
        Assert.That(contour.Curves, Has.Count.EqualTo(4));
    }

    [Test]
    public void GetPointIntersections_ReturnsBoundaryIntersections()
    {
        IContour contour = CreateSquareContour();
        var ray = new Ray(new PointXY(-1f, 1f));

        List<PointXY> intersections = contour.GetPointIntersections(ray);

        Assert.That(intersections, Has.Count.EqualTo(2));
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(0f, 1f))), Is.True);
        Assert.That(intersections.Exists(point => point.AlmostEquals(new PointXY(2f, 1f))), Is.True);
    }

    [Test]
    public void Project_ReturnsClosestBoundaryProjection()
    {
        IContour contour = CreateSquareContour();

        CurveProjection projection = contour.Project(new PointXY(3f, 0.5f));

        Assert.That(projection.ProjectedPoint.AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void ParameterizedCompositeContour_ImplementsContourAndParameterizedCurveContracts()
    {
        var contour = new ParameterizedCompositeContour(CreateSquareCurves());

        Assert.That(contour, Is.InstanceOf<IParameterizedContour>());
        Assert.That(contour, Is.InstanceOf<IParameterizedCompositeContour>());
        Assert.That(contour, Is.InstanceOf<IContour>());
        Assert.That(contour, Is.InstanceOf<ICompositeContour>());
        Assert.That(contour, Is.InstanceOf<IParameterizedCurve>());
        Assert.That(contour, Is.InstanceOf<IFiniteCurve>());
        Assert.That(contour.Length, Is.EqualTo(8f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(contour.StartPoint.AlmostEquals(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.EndPoint.AlmostEquals(contour.StartPoint), Is.True);
    }

    [Test]
    public void ParameterizedCompositeContour_GetPoint_UsesLengthCoordinateAroundBoundary()
    {
        var contour = new ParameterizedCompositeContour(CreateSquareCurves());

        Assert.That(contour.GetPoint(0f).AlmostEquals(new PointXY(0f, 0f)), Is.True);
        Assert.That(contour.GetPoint(1.5f).AlmostEquals(new PointXY(1.5f, 0f)), Is.True);
        Assert.That(contour.GetPoint(2.5f).AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(contour.GetPoint(contour.Length).AlmostEquals(new PointXY(0f, 0f)), Is.True);
    }

    [TestCase(-1e-6f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NegativeInfinity)]
    public void ParameterizedCompositeContour_GetPoint_WhenCurveCoordinateIsInvalid_Throws(float curveCoordinate)
    {
        var contour = new ParameterizedCompositeContour(CreateSquareCurves());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            contour.GetPoint(curveCoordinate));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void ParameterizedCompositeContour_GetPoint_WhenCurveCoordinateExceedsLength_Throws()
    {
        var contour = new ParameterizedCompositeContour(CreateSquareCurves());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            contour.GetPoint(contour.Length + 0.001f));

        Assert.That(exception!.ParamName, Is.EqualTo("curveCoordinate"));
    }

    [Test]
    public void ParameterizedCompositeContour_ProjectWithParameter_ReturnsClosestBoundaryCoordinate()
    {
        var contour = new ParameterizedCompositeContour(CreateSquareCurves());

        ParameterizedCurveProjection projection = contour.ProjectWithParameter(new PointXY(3f, 0.5f));

        Assert.That(projection.ProjectedPoint.AlmostEquals(new PointXY(2f, 0.5f)), Is.True);
        Assert.That(projection.CurveCoordinate, Is.EqualTo(2.5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(projection.Distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Encloses_WhenPointIsInsideSegmentContour_ReturnsTrue()
    {
        var contour = new CompositeContour(new IContourPath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f)),
            new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(2f, 2f)),
            new ParameterizedSegment(new PointXY(2f, 2f), new PointXY(0f, 2f)),
            new ParameterizedSegment(new PointXY(0f, 2f), new PointXY(0f, 0f))
        });

        Assert.That(contour.Encloses(new PointXY(1f, 1f)), Is.True);
    }

    [TestCase(0f, 0f)]
    [TestCase(1f, 0f)]
    [TestCase(2f, 0f)]
    [TestCase(2f, 1f)]
    [TestCase(2f, 2f)]
    [TestCase(1f, 2f)]
    [TestCase(0f, 2f)]
    [TestCase(0f, 1f)]
    public void Encloses_WhenPointIsOnSquareBoundary_ReturnsTrue(float x, float y)
    {
        IContour contour = CreateSquareContour();

        Assert.That(contour.Encloses(new PointXY(x, y)), Is.True);
    }

    [Test]
    public void Encloses_WhenPointIsLessThanGeometryEpsilonOutside_ReturnsFalse()
    {
        IContour contour = CreateSquareContour();
        var point = new PointXY(2f + GeometryConstants.GeometryEpsilon / 2f, 1f);

        Assert.That(contour.Distance(point), Is.GreaterThan(0f));
        Assert.That(contour.Encloses(point), Is.False);
    }

    [Test]
    public void Encloses_WhenPointIsOutsideContour_ReturnsFalse()
    {
        IContour contour = new CompositeContour(new IContourPath[]
        {
            CreateUnitCirclePath()
        });

        bool isInside = contour.Encloses(new PointXY(2f, 0f));

        Assert.That(isInside, Is.False);
    }

    [Test]
    public void Encloses_WhenPointIsOnRightmostBoundary_ReturnsTrue()
    {
        var contour = new CompositeContour(new IContourPath[]
        {
            CreateUnitCirclePath()
        });

        Assert.That(contour.Encloses(new PointXY(1f, 0f)), Is.True);
    }

    [Test]
    public void Encloses_WhenRayPassesThroughSharedVertex_UsesHalfOpenCrossingRule()
    {
        var contour = new CompositeContour(new IContourPath[]
        {
            new ParameterizedSegment(new PointXY(0f, 1f), new PointXY(1f, 0f)),
            new ParameterizedSegment(new PointXY(1f, 0f), new PointXY(2f, 1f)),
            new ParameterizedSegment(new PointXY(2f, 1f), new PointXY(1f, 2f)),
            new ParameterizedSegment(new PointXY(1f, 2f), new PointXY(0f, 1f))
        });

        Assert.That(contour.Encloses(new PointXY(-1f, 0f)), Is.False);
        Assert.That(contour.Encloses(new PointXY(-1f, 2f)), Is.False);
        Assert.That(contour.Encloses(new PointXY(1f, 1f)), Is.True);
    }

    [Test]
    public void Distance_WhenPointIsInsideContour_ReturnsShortestBoundaryDistance()
    {
        IContour contour = CreateSquareContour();

        float distance = contour.Distance(new PointXY(1f, 1f));

        Assert.That(distance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WhenPointIsInsideContour_ReturnsNegativeDistance()
    {
        IContour contour = CreateSquareContour();

        float signedDistance = contour.SignedDistance(new PointXY(1f, 1f));

        Assert.That(signedDistance, Is.EqualTo(-1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WhenPointIsExactlyOnBoundary_ReturnsZeroAndEnclosesPoint()
    {
        IContour contour = CreateSquareContour();
        var boundaryPoint = new PointXY(0f, 0.5f);

        float signedDistance = contour.SignedDistance(boundaryPoint);

        Assert.That(contour.Encloses(boundaryPoint), Is.True);
        Assert.That(signedDistance, Is.EqualTo(0f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WhenPointIsOutsideContour_ReturnsPositiveDistance()
    {
        IContour contour = CreateSquareContour();

        float signedDistance = contour.SignedDistance(new PointXY(3f, 1f));

        Assert.That(signedDistance, Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_WhenPointIsImmediatelyOutside_ReturnsPositiveDistance()
    {
        IContour contour = new CompositeContour(new IContourPath[]
        {
            CreateUnitCirclePath()
        });

        float signedDistance = contour.SignedDistance(new PointXY(1.0005f, 0f));

        Assert.That(signedDistance, Is.EqualTo(0.0005f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Encloses_UsesCurveRightwardCrossings()
    {
        var curve = new CrossingAwareCurve();
        IContour contour = new CompositeContour(new IContourPath[] { curve });

        bool encloses = contour.Encloses(new PointXY(0f, 0f));

        Assert.That(encloses, Is.True);
        Assert.That(curve.CountRightwardCrossingsCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Encloses_WhenPointCoordinateIsInvalid_Throws()
    {
        var contour = new CompositeContour(new IContourPath[]
        {
            CreateUnitCirclePath()
        });

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            contour.Encloses(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Distance_WhenPointCoordinateIsInvalid_Throws()
    {
        IContour contour = CreateSquareContour();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            contour.Distance(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void SignedDistance_WhenPointCoordinateIsInvalid_Throws()
    {
        IContour contour = CreateSquareContour();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            contour.SignedDistance(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    private static CompositeContour CreateSquareContour()
    {
        return new CompositeContour(CreateSquareCurves());
    }

    private static IContourPath[] CreateSquareCurves()
    {
        return new IContourPath[]
        {
            new ParameterizedSegment(new PointXY(0f, 0f), new PointXY(2f, 0f)),
            new ParameterizedSegment(new PointXY(2f, 0f), new PointXY(2f, 2f)),
            new ParameterizedSegment(new PointXY(2f, 2f), new PointXY(0f, 2f)),
            new ParameterizedSegment(new PointXY(0f, 2f), new PointXY(0f, 0f))
        };
    }

    private static ParameterizedArc CreateUnitCirclePath()
    {
        return new ParameterizedArc(
            new PointXY(0f, 0f),
            1f,
            0f,
            2f * MathF.PI,
            AngularDirection.Counterclockwise);
    }

    private sealed class CrossingAwareCurve : IContourPath
    {
        public int CountRightwardCrossingsCallCount { get; private set; }

        public PointXY StartPoint => new PointXY(0f, 0f);

        public PointXY EndPoint => new PointXY(0f, 0f);

        public PointXY EndpointA => StartPoint;

        public PointXY EndpointB => EndPoint;

        public float Length => 0f;

        public PointXY GetPoint(float curveCoordinate) => new PointXY(0f, 0f);

        public int CountRightwardCrossings(PointXY origin)
        {
            CountRightwardCrossingsCallCount++;
            return 1;
        }

        public List<PointXY> GetPointIntersections(Ray ray) => new List<PointXY>();

        public float Distance(PointXY point) => 1f;

        public CurveProjection Project(PointXY point) => new(new PointXY(0f, 0f), Distance(point));

        public ParameterizedCurveProjection ProjectWithParameter(PointXY point) => new(
            new PointXY(0f, 0f),
            0f,
            Distance(point));
    }
}
