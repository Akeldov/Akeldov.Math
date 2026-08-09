using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Regions;

public class ContourBasedRegionTests
{
    [Test]
    public void Constructor_WhenContoursIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ContourBasedRegion(null!));
    }

    [Test]
    public void Constructor_WhenContoursIsEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ContourBasedRegion(Array.Empty<IContour>()));
    }

    [Test]
    public void Constructor_WhenContoursContainsNull_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ContourBasedRegion(new IContour[] { null! }));
    }

    [Test]
    public void Constructor_WhenFillRuleIsUnsupported_Throws()
    {
        var contour = CreateSquareContour(0f, 0f, 1f, 1f);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContourBasedRegion(new IContour[] { contour }, (FillRule)42));

        Assert.That(exception!.ParamName, Is.EqualTo("fillRule"));
    }

    [Test]
    public void Constructor_WhenContoursIntersect_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
            new ContourBasedRegion(new IContour[]
            {
                CreateSquareContour(0f, 0f, 4f, 4f),
                CreateSquareContour(2f, -1f, 5f, 2f)
            }));
    }

    [Test]
    public void Constructor_WhenContoursTouch_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
            new ContourBasedRegion(new IContour[]
            {
                CreateSquareContour(0f, 0f, 4f, 4f),
                CreateSquareContour(4f, 1f, 5f, 3f)
            }));
    }

    [Test]
    public void Contours_WhenAccessed_ReturnsReadOnlyView()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 1f, 1f)
        });

        Assert.That(region.Contours, Is.Not.InstanceOf<IContour[]>());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<IContour>)region.Contours)[0] = CreateSquareContour(1f, 1f, 2f, 2f));
    }

    [Test]
    public void IRegion_ExposesSignedPointDistanceProviderContract()
    {
        IRegion region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f)
        });

        Assert.That(region, Is.InstanceOf<ISignedPointDistanceProvider>());
        Assert.That(region, Is.InstanceOf<IPointDistanceProvider>());
    }

    [Test]
    public void Contains_WhenPointIsInsideOuterContourAndOutsideHole_ReturnsTrue()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });

        Assert.That(region.Contains(new PointXY(0.5f, 0.5f)), Is.True);
    }

    [Test]
    public void Contains_WhenPointIsInsideHole_ReturnsFalse()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });

        Assert.That(region.Contains(new PointXY(2f, 2f)), Is.False);
    }

    [Test]
    public void Contains_WhenPointIsOnHoleBoundary_ReturnsFalse()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });

        Assert.That(region.Contains(new PointXY(1f, 2f)), Is.False);
    }

    [Test]
    public void Contains_UsesContourRightwardCrossings()
    {
        var contour = new CrossingAwareContour();
        IRegion region = new ContourBasedRegion(new IContour[] { contour });

        bool contains = region.Contains(new PointXY(0f, 0f));

        Assert.That(contains, Is.True);
        Assert.That(contour.CountRightwardCrossingsCallCount, Is.EqualTo(1));
    }

    [Test]
    public void Contains_WhenRegionIsSquareWithSquareHole_ClassifiesPoints()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });

        Assert.That(region.Contains(new PointXY(-0.5f, 2f)), Is.False);
        Assert.That(region.Contains(new PointXY(0.5f, 0.5f)), Is.True);
        Assert.That(region.Contains(new PointXY(2f, 2f)), Is.False);
        Assert.That(region.Contains(new PointXY(0f, 2f)), Is.True);
        Assert.That(region.Contains(new PointXY(1f, 2f)), Is.False);
    }

    [Test]
    public void Distance_ReturnsShortestDistanceToAnyContourBoundary()
    {
        IRegion region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });

        Assert.That(region.Distance(new PointXY(0.5f, 2f)), Is.EqualTo(0.5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(region.Distance(new PointXY(2f, 2f)), Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void SignedDistance_ReturnsNegativeInsideRegionAndPositiveOutsideOrInsideHole()
    {
        IRegion region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 4f, 4f),
            CreateSquareContour(1f, 1f, 3f, 3f)
        });

        Assert.That(region.SignedDistance(new PointXY(0.5f, 2f)), Is.EqualTo(-0.5f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(region.SignedDistance(new PointXY(2f, 2f)), Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
        Assert.That(region.SignedDistance(new PointXY(5f, 2f)), Is.EqualTo(1f).Within(GeometryConstants.GeometryEpsilon));
    }

    [Test]
    public void Contains_WhenContoursAreNested_AlternatesInsideAndOutside()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 8f, 8f),
            CreateSquareContour(1f, 1f, 7f, 7f),
            CreateSquareContour(2f, 2f, 6f, 6f),
            CreateSquareContour(3f, 3f, 5f, 5f)
        });

        Assert.That(region.Contains(new PointXY(0.5f, 0.5f)), Is.True);
        Assert.That(region.Contains(new PointXY(1.5f, 1.5f)), Is.False);
        Assert.That(region.Contains(new PointXY(2.5f, 2.5f)), Is.True);
        Assert.That(region.Contains(new PointXY(4f, 4f)), Is.False);
        Assert.That(region.Contains(new PointXY(8.5f, 8.5f)), Is.False);
    }

    [Test]
    public void Contains_WhenPointCoordinateIsInvalid_Throws()
    {
        var region = new ContourBasedRegion(new IContour[]
        {
            CreateSquareContour(0f, 0f, 1f, 1f)
        });

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            region.Contains(new PointXY(float.PositiveInfinity, 0f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    private static CompositeContour CreateSquareContour(float left, float bottom, float right, float top)
    {
        return new CompositeContour(new IFinitePath[]
        {
            new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
            new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
            new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
            new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
        });
    }

    private sealed class CrossingAwareContour : IContour
    {
        private static readonly IFinitePath[] ContourCurves =
        {
            new DistantBoundaryCurve()
        };

        public int CountRightwardCrossingsCallCount { get; private set; }

        public IReadOnlyList<IFinitePath> Curves => ContourCurves;

        public float Length => Curves[0].Length;

        public int CountRightwardCrossings(PointXY origin)
        {
            CountRightwardCrossingsCallCount++;
            return 1;
        }

        public bool Encloses(PointXY point)
        {
            return false;
        }

        public float Distance(PointXY point)
        {
            float minDistance = float.MaxValue;

            for (int i = 0; i < Curves.Count; i++)
            {
                float distance = Curves[i].Distance(point);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        public float SignedDistance(PointXY point, float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            float distance = Distance(point);
            return Encloses(point) ? -distance : distance;
        }

        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return new List<PointXY>();
        }

        public List<PointXY> GetPointIntersections(Ray ray) => new List<PointXY>();

        public CurveProjection Project(PointXY point)
        {
            return Curves[0].Project(point);
        }
    }

    private sealed class DistantBoundaryCurve : IFinitePath
    {
        public PointXY StartPoint => new PointXY(0f, 0f);

        public PointXY EndPoint => new PointXY(0f, 0f);

        public PointXY EndpointA => StartPoint;

        public PointXY EndpointB => EndPoint;

        public float Length => 0f;

        public PointXY GetPoint(float curveCoordinate) => new PointXY(0f, 0f);

        public int CountRightwardCrossings(PointXY origin) => 0;

        public List<PointXY> GetRayIntersections(
            Ray ray,
            float geometryEpsilon = GeometryConstants.GeometryEpsilon)
        {
            return new List<PointXY>();
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
