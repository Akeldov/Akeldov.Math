using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapSextupletSamplingExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SampleSextuplet_ReturnsNeighborsInHexEdgeOrder(Layout layout)
    {
        IHexMap<int> map = CreateMap(5, 5, layout);

        foreach (VectorXYInt index in new[] { new VectorXYInt(1, 1), new VectorXYInt(2, 2) })
        {
            Sextuplet<int> sample = map.SampleSextuplet(index);

            Assert.Multiple(() =>
            {
                Assert.That(sample.Adjacent0, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge0, layout)]));
                Assert.That(sample.Adjacent1, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge1, layout)]));
                Assert.That(sample.Adjacent2, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge2, layout)]));
                Assert.That(sample.Adjacent3, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge3, layout)]));
                Assert.That(sample.Adjacent4, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge4, layout)]));
                Assert.That(sample.Adjacent5, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge5, layout)]));
            });
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SamplePartialSextuplet_ForInteriorCenter_ReturnsAllNeighbors(Layout layout)
    {
        IHexMap<int> map = CreateMap(5, 5, layout);

        foreach (VectorXYInt index in new[] { new VectorXYInt(1, 1), new VectorXYInt(2, 2) })
        {
            PartialSextuplet<int> sample = map.SamplePartialSextuplet(index);

            Assert.Multiple(() =>
            {
                Assert.That(sample.Adjacent0, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge0, layout)]));
                Assert.That(sample.Adjacent1, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge1, layout)]));
                Assert.That(sample.Adjacent2, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge2, layout)]));
                Assert.That(sample.Adjacent3, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge3, layout)]));
                Assert.That(sample.Adjacent4, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge4, layout)]));
                Assert.That(sample.Adjacent5, Is.EqualTo(map[index.GetAdjacent(HexEdge.Edge5, layout)]));
                Assert.That(sample.Presence, Is.EqualTo(SextupletPresenceFlags.All));
            });
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SamplePartialSextuplet_AtBoundary_DefaultsMissingNeighborsAndSetsPresence(Layout layout)
    {
        IHexMap<int> map = CreateMap(3, 3, layout);
        var index = new VectorXYInt(0, 0);

        PartialSextuplet<int> sample = map.SamplePartialSextuplet(index);

        AssertPartialSample(map, index, sample);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SamplePartialSextuplet_ForSingleCellMap_ReturnsNoNeighbors(Layout layout)
    {
        IHexMap<int> map = CreateMap(1, 1, layout);
        var index = new VectorXYInt(0, 0);

        PartialSextuplet<int> sample = map.SamplePartialSextuplet(index);

        Assert.Multiple(() =>
        {
            Assert.That(sample.Adjacent0, Is.Zero);
            Assert.That(sample.Adjacent1, Is.Zero);
            Assert.That(sample.Adjacent2, Is.Zero);
            Assert.That(sample.Adjacent3, Is.Zero);
            Assert.That(sample.Adjacent4, Is.Zero);
            Assert.That(sample.Adjacent5, Is.Zero);
            Assert.That(sample.Presence, Is.EqualTo(SextupletPresenceFlags.None));
        });
    }

    [Test]
    public void SamplingMethods_SupportSpatialInterfaceReceiver()
    {
        var geometry = new HexMapGeometry(5, 5, 2f, Layout.EvenQ);
        int[] values = Enumerable.Range(1, geometry.Topology.Count).ToArray();
        ISpatialHexMap<int> map = new SpatialIntHexMap(geometry, values);
        var index = new VectorXYInt(2, 2);

        Sextuplet<int> fullSample = map.SampleSextuplet(index);
        PartialSextuplet<int> partialSample = map.SamplePartialSextuplet(index);

        Assert.Multiple(() =>
        {
            Assert.That(partialSample.Adjacent0, Is.EqualTo(fullSample.Adjacent0));
            Assert.That(partialSample.Adjacent1, Is.EqualTo(fullSample.Adjacent1));
            Assert.That(partialSample.Adjacent2, Is.EqualTo(fullSample.Adjacent2));
            Assert.That(partialSample.Adjacent3, Is.EqualTo(fullSample.Adjacent3));
            Assert.That(partialSample.Adjacent4, Is.EqualTo(fullSample.Adjacent4));
            Assert.That(partialSample.Adjacent5, Is.EqualTo(fullSample.Adjacent5));
            Assert.That(partialSample.Presence, Is.EqualTo(SextupletPresenceFlags.All));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SampleSextuplet_ForInteriorCenter_ReadsExactlySixNeighborValues(Layout layout)
    {
        foreach (VectorXYInt index in new[] { new VectorXYInt(1, 1), new VectorXYInt(2, 2) })
        {
            var map = new TrackingHexMap<int>(CreateMap(5, 5, layout));

            _ = map.SampleSextuplet(index);

            int[] expectedReadIndices = Enumerable.Range(0, 6)
                .Select(edgeIndex => index.GetAdjacent((HexEdge)edgeIndex, layout))
                .Select(adjacent => adjacent.Y * map.Topology.Resolution.X + adjacent.X)
                .ToArray();
            int centerFlatIndex = index.Y * map.Topology.Resolution.X + index.X;

            Assert.Multiple(() =>
            {
                Assert.That(map.ReadIndices, Has.Count.EqualTo(6));
                Assert.That(map.ReadIndices, Is.EqualTo(expectedReadIndices));
                Assert.That(map.ReadIndices, Does.Not.Contain(centerFlatIndex));
            });
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void SampleSextuplet_WhenCenterLacksCompleteNeighborhood_ThrowsBeforeReadingMapValues(Layout layout)
    {
        var map = new TrackingHexMap<int>(CreateMap(3, 3, layout));
        var index = new VectorXYInt(0, 0);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => map.SampleSextuplet(index));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ParamName, Is.EqualTo("index"));
            Assert.That(map.ReadIndices, Is.Empty);
        });
    }

    [Test]
    public void SamplingMethods_WhenMapIsNull_Throw()
    {
        IHexMap<int> map = null!;
        var index = new VectorXYInt(1, 1);

        var fullException = Assert.Throws<ArgumentNullException>(() => map.SampleSextuplet(index));
        var partialException = Assert.Throws<ArgumentNullException>(() => map.SamplePartialSextuplet(index));

        Assert.Multiple(() =>
        {
            Assert.That(fullException!.ParamName, Is.EqualTo("map"));
            Assert.That(partialException!.ParamName, Is.EqualTo("map"));
        });
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(3, 0)]
    [TestCase(0, 3)]
    public void SamplingMethods_WhenIndexIsOutsideMap_Throw(int x, int y)
    {
        IHexMap<int> map = CreateMap(3, 3, Layout.OddR);
        var index = new VectorXYInt(x, y);

        var fullException = Assert.Throws<ArgumentOutOfRangeException>(() => map.SampleSextuplet(index));
        var partialException = Assert.Throws<ArgumentOutOfRangeException>(() => map.SamplePartialSextuplet(index));

        Assert.Multiple(() =>
        {
            Assert.That(fullException!.ParamName, Is.EqualTo("index"));
            Assert.That(partialException!.ParamName, Is.EqualTo("index"));
        });
    }

    private static HexMap<int> CreateMap(int width, int height, Layout layout)
    {
        int[] values = Enumerable.Range(1, checked(width * height)).ToArray();
        return new HexMap<int>(new HexMapTopology(width, height, layout), values);
    }

    private static void AssertPartialSample(
        IHexMap<int> map,
        VectorXYInt index,
        PartialSextuplet<int> sample)
    {
        int[] actualValues =
        {
            sample.Adjacent0,
            sample.Adjacent1,
            sample.Adjacent2,
            sample.Adjacent3,
            sample.Adjacent4,
            sample.Adjacent5
        };

        Assert.Multiple(() =>
        {
            for (int edgeIndex = 0; edgeIndex < 6; edgeIndex++)
            {
                var edge = (HexEdge)edgeIndex;
                VectorXYInt adjacent = index.GetAdjacent(edge, map.Topology.Layout);
                bool isPresent =
                    (uint)adjacent.X < (uint)map.Topology.Resolution.X &&
                    (uint)adjacent.Y < (uint)map.Topology.Resolution.Y;
                int expectedValue = isPresent ? map[adjacent] : default;
                var flag = (SextupletPresenceFlags)(1 << edgeIndex);
                SextupletPresenceFlags expectedPresence = isPresent
                    ? flag
                    : SextupletPresenceFlags.None;

                Assert.That(actualValues[edgeIndex], Is.EqualTo(expectedValue), $"Value for {edge}");
                Assert.That(sample.Presence & flag, Is.EqualTo(expectedPresence), $"Presence for {edge}");
            }
        });
    }

    private sealed class TrackingHexMap<T> : IHexMap<T>
    {
        private readonly IHexMap<T> _source;

        public TrackingHexMap(IHexMap<T> source)
        {
            _source = source;
        }

        public HexMapTopology Topology => _source.Topology;

        public List<int> ReadIndices { get; } = new();

        public T this[VectorXYInt index]
        {
            get
            {
                ReadIndices.Add(index.Y * Topology.Resolution.X + index.X);
                return _source[index];
            }
        }

        public T this[int index]
        {
            get
            {
                ReadIndices.Add(index);
                return _source[index];
            }
        }
    }
}
