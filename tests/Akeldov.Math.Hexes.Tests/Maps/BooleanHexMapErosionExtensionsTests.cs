using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class BooleanHexMapErosionExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Erode_WithRingCount_ExpandsFalseCellToHexDisk(Layout layout)
    {
        var topology = new HexMapTopology(17, 17, layout);
        var source = Map(topology, new VectorXYInt(8, 8));
        for (int ringsCount = 0; ringsCount <= 4; ringsCount++)
        {
            BoolHexMap result = source.Erode(ringsCount);
            Assert.That(ReadValues(result).Count(value => !value), Is.EqualTo(1 + 3 * ringsCount * (ringsCount + 1)),
                $"Rings: {ringsCount}");
        }
    }

    [Test]
    public void Erode_WithDistanceLimits_BlocksCellsBeyondAnInsufficientLimit()
    {
        var topology = new HexMapTopology(5, 1, Layout.OddR);
        var source = Map(topology, VectorXYInt.Zero);
        var limits = new IntHexMap(topology, new[] { 0, 1, 1, 50, 50 });

        Assert.That(ReadValues(source.Erode(limits)), Is.EqualTo(new[] { false, false, true, true, true }));
    }

    [TestCase(2, true)]
    [TestCase(3, false)]
    public void Erode_WithDistanceLimits_CountsFullDetourLength(int targetLimit, bool expectedTarget)
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var source = Map(topology, VectorXYInt.Zero);
        var limits = new HexMap<int>(topology, Enumerable.Repeat(int.MaxValue, topology.Count).ToArray());
        limits[new VectorXYInt(1, 0)] = 0;
        limits[new VectorXYInt(2, 0)] = targetLimit;

        BoolHexMap result = source.Erode(limits);

        Assert.Multiple(() =>
        {
            Assert.That(result[new VectorXYInt(1, 0)], Is.True);
            Assert.That(result[new VectorXYInt(2, 0)], Is.EqualTo(expectedTarget));
            Assert.That(result[new VectorXYInt(1, 1)], Is.False);
        });
    }

    [Test]
    public void Erode_WithDistanceLimits_UsesNearestAdmissibleFalseSource()
    {
        var topology = new HexMapTopology(7, 1, Layout.OddR);
        var source = Map(topology, VectorXYInt.Zero, new VectorXYInt(6, 0));
        IHexMap<int> limits = new IntHexMap(topology, new[] { int.MinValue, 1, 2, 3, 2, 1, 0 });

        Assert.That(ReadValues(source.Erode<IHexMap<int>>(limits)), Is.All.False);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Erode_WithUniformDistanceLimits_MatchesRingCount(Layout layout)
    {
        var topology = new HexMapTopology(11, 9, layout);
        var source = Map(topology, VectorXYInt.Zero, new VectorXYInt(8, 6));
        foreach (int ringsCount in new[] { 0, 1, 2, 5, int.MaxValue })
        {
            BoolHexMap result = source.Erode(new UniformIntMap(topology, ringsCount));
            Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source.Erode(ringsCount))), $"Rings: {ringsCount}");
        }
        Assert.That(ReadValues(source.Erode(int.MaxValue)), Is.All.False);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Erode_WithNonPositiveDistanceLimits_PreservesBothTrueAndFalseCells(int limit)
    {
        var topology = new HexMapTopology(7, 5, Layout.EvenQ);
        var source = Map(topology, VectorXYInt.Zero, new VectorXYInt(3, 2));
        BoolHexMap result = source.Erode(new UniformIntMap(topology, limit));

        Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source)));
    }

    [TestCase(0, 0)]
    [TestCase(0, 7)]
    [TestCase(7, 0)]
    [TestCase(1, 1)]
    [TestCase(7, 5)]
    public void Erode_WithEmptyOrUniformMaps_ReturnsIndependentCopies(int width, int height)
    {
        var geometry = new HexMapGeometry(width, height, VectorXY.Zero, 1f, Layout.EvenR);
        foreach (bool value in new[] { false, true })
        {
            var source = new SpatialBoolHexMap(geometry, Enumerable.Repeat(value, geometry.Topology.Count).ToArray());
            IHexMap<bool> ordinary = source;
            foreach (int ringsCount in new[] { 0, 1, 4, int.MaxValue })
            {
                var limits = new UniformIntMap(geometry.Topology, ringsCount);
                BoolHexMap[] ordinaryResults = { ordinary.Erode(ringsCount), ordinary.Erode(limits) };
                SpatialBoolHexMap[] spatialResults = { source.Erode(ringsCount), source.Erode(limits) };
                foreach (IHexMap<bool> result in ordinaryResults.Cast<IHexMap<bool>>().Concat(spatialResults))
                {
                    Assert.That(result, Is.Not.SameAs(source));
                    Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
                    Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source)));
                }
                foreach (SpatialBoolHexMap result in spatialResults)
                    Assert.That(result.Geometry, Is.EqualTo(geometry));
            }
        }
    }

    [TestCase(Layout.OddR, 1729)]
    [TestCase(Layout.EvenR, 2718)]
    [TestCase(Layout.OddQ, 31415)]
    [TestCase(Layout.EvenQ, 65537)]
    public void Erode_OverloadsMatchSynchronousExpansion(Layout layout, int seed)
    {
        var random = new Random(seed);
        for (int scenario = 0; scenario < 48; scenario++)
        {
            var geometry = new HexMapGeometry(random.Next(1, 12), random.Next(1, 12), new VectorXY(10f, -20f), 2f, layout);
            int count = geometry.Topology.Count;
            double falseDensity = (scenario % 4) switch { 0 => 0.01, 1 => 0.05, 2 => 0.5, _ => 0.9 };
            bool[] original = Enumerable.Range(0, count).Select(_ => random.NextDouble() >= falseDensity).ToArray();
            int[] originalLimits = Enumerable.Range(0, count).Select(_ => random.Next(-1, 12)).ToArray();
            var source = new BoolHexMap(geometry.Topology, (bool[])original.Clone());
            ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry, (bool[])original.Clone());
            var limits = new IntHexMap(geometry.Topology, (int[])originalLimits.Clone());
            int ringsCount = scenario % 8;
            BoolHexMap expectedRings = source;
            for (int ring = 0; ring < ringsCount; ring++)
                expectedRings = expectedRings.Erode();
            var expectedLimits = new BoolHexMap(geometry.Topology, (bool[])original.Clone());
            for (int distance = 1; distance < count; distance++)
            {
                BoolHexMap candidates = expectedLimits.Erode();
                for (int index = 0; index < count; index++)
                    expectedLimits[index] &= candidates[index] || originalLimits[index] < distance;
            }

            BoolHexMap[] results = { source.Erode(ringsCount), source.Erode(limits) };
            SpatialBoolHexMap[] spatialResults = { spatial.Erode(ringsCount), spatial.Erode(limits) };
            BoolHexMap[] expected = { expectedRings, expectedLimits };
            string context = $"Seed: {seed}, scenario: {scenario}, rings: {ringsCount}";
            for (int operation = 0; operation < results.Length; operation++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(ReadValues(results[operation]), Is.EqualTo(ReadValues(expected[operation])), context);
                    Assert.That(ReadValues(spatialResults[operation]), Is.EqualTo(ReadValues(expected[operation])), context);
                    Assert.That(spatialResults[operation].Geometry, Is.EqualTo(geometry), context);
                    Assert.That(ReadValues(source), Is.EqualTo(original), context);
                    Assert.That(ReadValues(spatial), Is.EqualTo(original), context);
                    Assert.That(Enumerable.Range(0, count).Select(index => limits[index]), Is.EqualTo(originalLimits), context);
                });
                results[operation][0] = !results[operation][0];
                spatialResults[operation][0] = !spatialResults[operation][0];
            }
            Assert.That(ReadValues(source), Is.EqualTo(original), context);
            Assert.That(ReadValues(spatial), Is.EqualTo(original), context);
        }
    }

    [Test]
    public void Erode_WithSpatialDistanceLimits_MatchesByIndexAndPreservesSourceGeometry()
    {
        var geometry = new HexMapGeometry(4, 3, new VectorXY(5f, 7f), 2f, Layout.EvenQ);
        var limitGeometry = new HexMapGeometry(4, 3, new VectorXY(-10f, 3f), 5f, Layout.EvenQ);
        var source = new SpatialBoolHexMap(geometry, ReadValues(Map(geometry.Topology, VectorXYInt.Zero)));
        ISpatialIntHexMap limits = new SpatialIntHexMap(limitGeometry, Enumerable.Repeat(2, geometry.Topology.Count).ToArray());

        SpatialBoolHexMap result = source.Erode(limits);

        Assert.That(result.Geometry, Is.EqualTo(geometry));
        Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source.Erode(2))));
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Erode_WithNegativeRingCount_Throws(int ringsCount)
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        IHexMap<bool> source = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry);
        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => source.Erode(ringsCount))!.ParamName, Is.EqualTo("ringsCount"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => spatial.Erode(ringsCount))!.ParamName, Is.EqualTo("ringsCount"));
        });
    }

    [Test]
    public void Erode_WithDistanceLimits_ValidatesNullMaps()
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        IHexMap<bool> source = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry);
        IHexMap<int> nullLimits = null!;
        IHexMap<bool> nullSource = null!;
        ISpatialHexMap<bool> nullSpatial = null!;
        var limits = new UniformIntMap(geometry.Topology, 2);
        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => source.Erode(nullLimits))!.ParamName, Is.EqualTo("maxErodeDistanceMap"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Erode(nullLimits))!.ParamName, Is.EqualTo("maxErodeDistanceMap"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => nullSource.Erode(limits))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => nullSpatial.Erode(limits))!.ParamName, Is.EqualTo("map"));
        });
    }

    [TestCase(3, 2, Layout.OddR)]
    [TestCase(1, 4, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    public void Erode_WithDistanceLimits_RejectsDifferentTopologies(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        IHexMap<bool> source = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry);
        var limits = new UniformIntMap(new HexMapTopology(width, height, layout), 1);
        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => source.Erode(limits))!.ParamName, Is.EqualTo("maxErodeDistanceMap"));
            Assert.That(Assert.Throws<ArgumentException>(() => spatial.Erode(limits))!.ParamName, Is.EqualTo("maxErodeDistanceMap"));
        });
    }

    private static BoolHexMap Map(HexMapTopology topology, params VectorXYInt[] falseCells)
    {
        var values = Enumerable.Repeat(true, topology.Count).ToArray();
        foreach (VectorXYInt index in falseCells)
            values[index.Y * topology.Resolution.X + index.X] = false;
        return new BoolHexMap(topology, values);
    }

    private static bool[] ReadValues(IHexMap<bool> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private readonly struct UniformIntMap : IHexMap<int>
    {
        private readonly int _value;

        public UniformIntMap(HexMapTopology topology, int value)
        {
            Topology = topology;
            _value = value;
        }

        public HexMapTopology Topology { get; }

        public int this[VectorXYInt index] => _value;

        public int this[int index] => _value;
    }
}
