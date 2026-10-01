using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class BooleanHexMapMorphologyExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Dilate_WithSingleInteriorCell_ExpandsToCellAndSixNeighbors(Layout layout)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var center = new VectorXYInt(3, 3);
        var source = Map(topology, center);

        BoolHexMap result = source.Dilate();

        bool[] expected = Values(topology, center);
        foreach (VectorXYInt adjacent in center.GetAdjacents(layout))
            expected[FlatIndex(adjacent, topology)] = true;

        Assert.That(ReadValues(result), Is.EqualTo(expected));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Dilate_WithRingCount_ExpandsSingleCellToHexDisk(Layout layout)
    {
        var topology = new HexMapTopology(17, 17, layout);
        var source = Map(topology, new VectorXYInt(8, 8));

        for (int ringsCount = 0; ringsCount <= 4; ringsCount++)
        {
            BoolHexMap result = source.Dilate(ringsCount);
            Assert.That(ReadValues(result).Count(value => value), Is.EqualTo(1 + 3 * ringsCount * (ringsCount + 1)),
                $"Rings: {ringsCount}");
        }
    }

    [TestCase(Layout.OddR, 1729)]
    [TestCase(Layout.EvenR, 2718)]
    [TestCase(Layout.OddQ, 31415)]
    [TestCase(Layout.EvenQ, 65537)]
    public void Dilate_WithRingCount_MatchesRepeatedDilation(Layout layout, int seed)
    {
        var random = new Random(seed);
        for (int scenario = 0; scenario < 60; scenario++)
        {
            // Include empty and one-cell-wide maps, both parity axes, and overlapping fronts.
            int width = scenario % 10 == 0 ? 1 : random.Next(0, 18);
            int height = scenario % 10 == 1 ? 1 : random.Next(0, 18);
            var geometry = new HexMapGeometry(width, height, new VectorXY(10f, -20f), 2f, layout);
            double density = (scenario % 8) switch
            {
                0 => 0,
                1 => 0.01,
                2 => 0.05,
                3 => 0.125,
                4 => 0.25,
                5 => 0.5,
                6 => 0.75,
                _ => 1,
            };
            bool[] original = Enumerable.Range(0, geometry.Topology.Count)
                .Select(_ => random.NextDouble() < density).ToArray();
            var source = new BoolHexMap(geometry.Topology, (bool[])original.Clone());
            ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry, (bool[])original.Clone());
            int ringsCount = scenario % 9;
            BoolHexMap expected = source;
            for (int ring = 0; ring < ringsCount; ring++)
                expected = expected.Dilate();

            BoolHexMap result = source.Dilate(ringsCount);
            SpatialBoolHexMap spatialResult = spatial.Dilate(ringsCount);
            string context = $"Seed: {seed}, scenario: {scenario}, size: {width}x{height}, rings: {ringsCount}";
            Assert.Multiple(() =>
            {
                Assert.That(ReadValues(result), Is.EqualTo(ReadValues(expected)), context);
                Assert.That(result.Topology, Is.EqualTo(source.Topology), context);
                Assert.That(ReadValues(spatialResult), Is.EqualTo(ReadValues(expected)), context);
                Assert.That(spatialResult.Geometry, Is.EqualTo(geometry), context);
                Assert.That(ReadValues(source), Is.EqualTo(original), context);
                Assert.That(ReadValues(spatial), Is.EqualTo(original), context);
            });
        }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    public void Dilate_WithRingCount_ReturnsIndependentMap(int ringsCount)
    {
        var topology = new HexMapTopology(9, 9, Layout.OddR);
        var source = Map(topology, new VectorXYInt(4, 4));
        bool[] original = ReadValues(source);

        BoolHexMap result = source.Dilate(ringsCount);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.SameAs(source));
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(ReadValues(source), Is.EqualTo(original));
            if (ringsCount == 0)
                Assert.That(ReadValues(result), Is.EqualTo(original));
        });
        result[0] = !result[0];
        Assert.That(ReadValues(source), Is.EqualTo(original));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Dilate_WithMaximumRingCount_FillsMapOnlyWhenTrueCellsExist(Layout layout)
    {
        var geometry = new HexMapGeometry(11, 7, VectorXY.Zero, 1f, layout);
        var source = Map(geometry.Topology, VectorXYInt.Zero);
        var empty = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry, ReadValues(source));
        ISpatialHexMap<bool> spatialEmpty = new SpatialBoolHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(ReadValues(source.Dilate(int.MaxValue)), Is.All.True);
            Assert.That(ReadValues(empty.Dilate(int.MaxValue)), Is.All.False);
            Assert.That(ReadValues(spatial.Dilate(int.MaxValue)), Is.All.True);
            Assert.That(ReadValues(spatialEmpty.Dilate(int.MaxValue)), Is.All.False);
        });
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Dilate_WithNegativeRingCount_Throws(int ringsCount)
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        IHexMap<bool> ordinary = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => ordinary.Dilate(ringsCount))!.ParamName,
                Is.EqualTo("ringsCount"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => spatial.Dilate(ringsCount))!.ParamName,
                Is.EqualTo("ringsCount"));
        });
    }

    [Test]
    public void Dilate_WithDistanceLimits_BlocksCellsBeyondAnInsufficientLimit()
    {
        var topology = new HexMapTopology(5, 1, Layout.OddR);
        var source = Map(topology, VectorXYInt.Zero);
        var limits = new IntHexMap(topology, new[] { 0, 1, 1, 50, 50 });

        BoolHexMap result = source.Dilate(limits);

        Assert.That(ReadValues(result), Is.EqualTo(new[] { true, true, false, false, false }));
    }

    [TestCase(2, false)]
    [TestCase(3, true)]
    public void Dilate_WithDistanceLimits_CountsFullDetourLength(int targetLimit, bool expectedTarget)
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var source = Map(topology, VectorXYInt.Zero);
        var limits = new HexMap<int>(topology, Enumerable.Repeat(int.MaxValue, topology.Count).ToArray());
        limits[new VectorXYInt(1, 0)] = 0;
        limits[new VectorXYInt(2, 0)] = targetLimit;

        BoolHexMap result = source.Dilate(limits);

        Assert.Multiple(() =>
        {
            Assert.That(result[new VectorXYInt(1, 0)], Is.False);
            Assert.That(result[new VectorXYInt(2, 0)], Is.EqualTo(expectedTarget));
            Assert.That(result[new VectorXYInt(1, 1)], Is.True);
        });
    }

    [Test]
    public void Dilate_WithDistanceLimits_UsesNearestAdmissibleSource()
    {
        var topology = new HexMapTopology(7, 1, Layout.OddR);
        var source = Map(topology, VectorXYInt.Zero, new VectorXYInt(6, 0));
        IHexMap<int> limits = new IntHexMap(topology, new[] { 0, 1, 2, 3, 2, 1, int.MinValue });

        Assert.That(ReadValues(source.Dilate<IHexMap<int>>(limits)), Is.All.True);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void Dilate_WithNonPositiveDistanceLimits_PreservesOriginalTrueCells(int limit)
    {
        var topology = new HexMapTopology(7, 5, Layout.EvenQ);
        var source = Map(topology, VectorXYInt.Zero, new VectorXYInt(3, 2));
        var limits = new UniformIntMap(topology, limit);

        BoolHexMap result = source.Dilate(limits);

        Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source)));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Dilate_WithUniformDistanceLimits_MatchesRingCount(Layout layout)
    {
        var topology = new HexMapTopology(11, 9, layout);
        var source = Map(topology, VectorXYInt.Zero, new VectorXYInt(8, 6));
        foreach (int ringsCount in new[] { 0, 1, 2, 5, int.MaxValue })
        {
            BoolHexMap result = source.Dilate(new UniformIntMap(topology, ringsCount));
            Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source.Dilate(ringsCount))), $"Rings: {ringsCount}");
        }
    }

    [TestCase(0, 0)]
    [TestCase(0, 7)]
    [TestCase(7, 0)]
    [TestCase(1, 1)]
    public void Dilate_WithDistanceLimits_HandlesEmptyDomainsAndUniformMasks(int width, int height)
    {
        var geometry = new HexMapGeometry(width, height, VectorXY.Zero, 1f, Layout.EvenR);
        foreach (bool value in new[] { false, true })
        {
            var source = new SpatialBoolHexMap(geometry, Enumerable.Repeat(value, geometry.Topology.Count).ToArray());
            SpatialBoolHexMap result = source.Dilate(new UniformIntMap(geometry.Topology, int.MaxValue));
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.SameAs(source));
                Assert.That(result.Geometry, Is.EqualTo(geometry));
                Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source)));
            });
        }
    }

    [TestCase(Layout.OddR, 1729)]
    [TestCase(Layout.EvenR, 2718)]
    [TestCase(Layout.OddQ, 31415)]
    [TestCase(Layout.EvenQ, 65537)]
    public void Dilate_WithDistanceLimits_MatchesSynchronousExpansion(Layout layout, int seed)
    {
        var random = new Random(seed);
        for (int scenario = 0; scenario < 50; scenario++)
        {
            var geometry = new HexMapGeometry(random.Next(1, 12), random.Next(1, 12), new VectorXY(10f, -20f), 2f, layout);
            int count = geometry.Topology.Count;
            bool[] original = Enumerable.Range(0, count).Select(_ => random.NextDouble() < 0.1).ToArray();
            int[] originalLimits = Enumerable.Range(0, count).Select(_ => random.Next(-1, 12)).ToArray();
            var source = new BoolHexMap(geometry.Topology, (bool[])original.Clone());
            ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry, (bool[])original.Clone());
            var limits = new IntHexMap(geometry.Topology, (int[])originalLimits.Clone());
            // Whole-map synchronous steps are an independent oracle for the queued traversal.
            var expected = new BoolHexMap(geometry.Topology, (bool[])original.Clone());
            for (int distance = 1; distance < count; distance++)
            {
                BoolHexMap candidates = expected.Dilate();
                for (int index = 0; index < count; index++)
                    expected[index] |= candidates[index] && originalLimits[index] >= distance;
            }

            BoolHexMap result = source.Dilate(limits);
            SpatialBoolHexMap spatialResult = spatial.Dilate(limits);
            string context = $"Seed: {seed}, scenario: {scenario}";
            Assert.Multiple(() =>
            {
                Assert.That(ReadValues(result), Is.EqualTo(ReadValues(expected)), context);
                Assert.That(result.Topology, Is.EqualTo(geometry.Topology), context);
                Assert.That(ReadValues(spatialResult), Is.EqualTo(ReadValues(expected)), context);
                Assert.That(spatialResult.Geometry, Is.EqualTo(geometry), context);
                Assert.That(ReadValues(source), Is.EqualTo(original), context);
                Assert.That(ReadValues(spatial), Is.EqualTo(original), context);
                Assert.That(Enumerable.Range(0, count).Select(index => limits[index]), Is.EqualTo(originalLimits), context);
            });

            result[0] = !result[0];
            spatialResult[0] = !spatialResult[0];
            Assert.That(source[0], Is.EqualTo(original[0]), context);
            Assert.That(spatial[0], Is.EqualTo(original[0]), context);
        }
    }

    [Test]
    public void Dilate_WithSpatialDistanceLimits_MatchesByIndexAndPreservesSourceGeometry()
    {
        var geometry = new HexMapGeometry(4, 3, new VectorXY(5f, 7f), 2f, Layout.EvenQ);
        var limitGeometry = new HexMapGeometry(4, 3, new VectorXY(-10f, 3f), 5f, Layout.EvenQ);
        var source = new SpatialBoolHexMap(geometry, Values(geometry.Topology, VectorXYInt.Zero));
        ISpatialIntHexMap limits = new SpatialIntHexMap(limitGeometry, Enumerable.Repeat(2, geometry.Topology.Count).ToArray());

        SpatialBoolHexMap result = source.Dilate(limits);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(ReadValues(result), Is.EqualTo(ReadValues(source.Dilate(2))));
        });
    }

    [Test]
    public void Dilate_WithDistanceLimits_ValidatesNullMaps()
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        IHexMap<bool> source = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry);
        var limits = new IntHexMap(geometry.Topology);
        IHexMap<int> nullLimits = null!;
        IHexMap<bool> nullSource = null!;
        ISpatialHexMap<bool> nullSpatial = null!;
        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => source.Dilate(nullLimits))!.ParamName, Is.EqualTo("maxDilateDistanceMap"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Dilate(nullLimits))!.ParamName, Is.EqualTo("maxDilateDistanceMap"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => nullSource.Dilate(limits))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => nullSpatial.Dilate(limits))!.ParamName, Is.EqualTo("map"));
        });
    }

    [TestCase(3, 2, Layout.OddR)]
    [TestCase(2, 3, Layout.OddR)]
    [TestCase(1, 4, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    public void Dilate_WithDistanceLimits_RejectsDifferentTopologies(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(2, 2, VectorXY.Zero, 1f, Layout.OddR);
        IHexMap<bool> source = new BoolHexMap(geometry.Topology);
        ISpatialHexMap<bool> spatial = new SpatialBoolHexMap(geometry);
        var limits = new UniformIntMap(new HexMapTopology(width, height, layout), 1);
        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => source.Dilate(limits))!.ParamName, Is.EqualTo("maxDilateDistanceMap"));
            Assert.That(Assert.Throws<ArgumentException>(() => spatial.Dilate(limits))!.ParamName, Is.EqualTo("maxDilateDistanceMap"));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Erode_WithCellAndSixNeighbors_LeavesCenter(Layout layout)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var center = new VectorXYInt(3, 3);
        VectorXYInt[] cluster = center.GetAdjacents(layout).Append(center).ToArray();
        var source = Map(topology, cluster);

        BoolHexMap result = source.Erode();

        Assert.That(ReadValues(result), Is.EqualTo(Values(topology, center)));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Open_RemovesIsolatedCellWithoutAllocatingAResultMapForTheIntermediateStage(Layout layout)
    {
        var topology = new HexMapTopology(8, 8, layout);
        var center = new VectorXYInt(3, 3);
        VectorXYInt[] cluster = center.GetAdjacents(layout).Append(center).ToArray();
        VectorXYInt isolated = new(7, 7);
        var source = Map(topology, cluster.Append(isolated).ToArray());

        BoolHexMap result = source.Open();

        Assert.That(ReadValues(result), Is.EqualTo(Values(topology, cluster)));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Close_FillsSingleCellHole(Layout layout)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var hole = new VectorXYInt(3, 3);
        bool[] values = Enumerable.Repeat(true, topology.Count).ToArray();
        values[FlatIndex(hole, topology)] = false;
        var source = new BoolHexMap(topology, values);

        BoolHexMap result = source.Close();

        Assert.That(ReadValues(result), Is.All.True);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Outline_ReturnsInnerBoundary(Layout layout)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var center = new VectorXYInt(3, 3);
        VectorXYInt[] ring = center.GetAdjacents(layout);
        var source = Map(topology, ring.Append(center).ToArray());

        BoolHexMap result = source.Outline();

        Assert.That(ReadValues(result), Is.EqualTo(Values(topology, ring)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Morphology_OnSingleCellTopology_IgnoresMissingNeighbors(bool value)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var source = new BoolHexMap(topology, new[] { value });

        Assert.Multiple(() =>
        {
            Assert.That(source.Dilate()[0], Is.EqualTo(value));
            Assert.That(source.Dilate(int.MaxValue)[0], Is.EqualTo(value));
            Assert.That(source.Erode()[0], Is.EqualTo(value));
            Assert.That(source.Open()[0], Is.EqualTo(value));
            Assert.That(source.Close()[0], Is.EqualTo(value));
            Assert.That(source.Outline()[0], Is.False);
        });
    }

    [Test]
    public void Morphology_WithEmptyMap_ReturnsEmptyIndependentMaps()
    {
        var source = new BoolHexMap(new HexMapTopology(0, 0, Layout.EvenQ));

        BoolHexMap[] results =
        {
            source.Dilate(), source.Erode(), source.Open(), source.Close(), source.Outline(),
            source.Dilate(0), source.Dilate(1), source.Dilate(int.MaxValue),
        };

        Assert.Multiple(() =>
        {
            foreach (BoolHexMap result in results)
            {
                Assert.That(result, Is.Not.SameAs(source));
                Assert.That(result.Topology, Is.EqualTo(source.Topology));
                Assert.That(result.Topology.Count, Is.Zero);
            }
        });
    }

    [Test]
    public void SpatialMorphology_PreservesGeometryAndReturnsIndependentSpecializations()
    {
        var geometry = new HexMapGeometry(
            4,
            3,
            new VectorXY(10f, -20f),
            2f,
            Layout.EvenQ);
        bool[] values = Values(geometry.Topology, new VectorXYInt(1, 1));
        var source = new SpatialBoolHexMap(geometry, values);

        SpatialBoolHexMap[] results =
        {
            source.Dilate(), source.Erode(), source.Open(), source.Close(), source.Outline(),
            source.Dilate(0), source.Dilate(1), source.Dilate(3),
        };

        values[0] = !values[0];
        foreach (SpatialBoolHexMap result in results)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<SpatialBoolHexMap>());
                Assert.That(result.Geometry, Is.EqualTo(geometry));
            });

            bool sourceValue = source[0];
            result[0] = !result[0];
            Assert.That(source[0], Is.EqualTo(sourceValue));
        }
    }

    [Test]
    public void Morphology_WhenMapIsNull_Throws()
    {
        IHexMap<bool> ordinary = null!;
        ISpatialHexMap<bool> spatial = null!;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Dilate())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Dilate(0))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Dilate(3))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Erode())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Open())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Close())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.Outline())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Dilate())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Dilate(0))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Dilate(3))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Erode())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Open())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Close())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.Outline())!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void SpatialMorphology_WhenTopologyAndGeometryDiffer_Throws()
    {
        ISpatialHexMap<bool> map = new InconsistentSpatialBoolMap();

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => map.Dilate())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Dilate(0))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Dilate(3))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Dilate(new UniformIntMap(map.Topology, 1)))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Erode())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Open())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Close())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.Outline())!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void Morphology_ReadsSourceALinearNumberOfTimes()
    {
        var source = new CountingBoolHexMap(new HexMapTopology(32, 24, Layout.OddR));
        Func<IHexMap<bool>, BoolHexMap>[] operations =
        {
            map => map.Dilate(),
            map => map.Dilate(0),
            map => map.Dilate(1),
            map => map.Dilate(8),
            map => map.Dilate(int.MaxValue),
            map => map.Dilate(new UniformIntMap(map.Topology, int.MaxValue)),
            map => map.Erode(),
            map => map.Open(),
            map => map.Close(),
            map => map.Outline(),
        };

        foreach (Func<IHexMap<bool>, BoolHexMap> operation in operations)
        {
            source.ResetReadCount();
            _ = operation(source);
            Assert.That(source.ReadCount, Is.LessThanOrEqualTo(source.Topology.Count * 7));
        }
    }

    private static BoolHexMap Map(HexMapTopology topology, params VectorXYInt[] trueCells) =>
        new(topology, Values(topology, trueCells));

    private static bool[] Values(HexMapTopology topology, params VectorXYInt[] trueCells)
    {
        var values = new bool[topology.Count];
        foreach (VectorXYInt index in trueCells)
            values[FlatIndex(index, topology)] = true;

        return values;
    }

    private static bool[] ReadValues(IHexMap<bool> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private static int FlatIndex(VectorXYInt index, HexMapTopology topology) =>
        index.Y * topology.Resolution.X + index.X;

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

    private sealed class InconsistentSpatialBoolMap : ISpatialHexMap<bool>
    {
        public HexMapTopology Topology { get; } = new(1, 1, Layout.OddR);

        public HexMapGeometry Geometry { get; } = new(2, 1, VectorXY.Zero, 1f, Layout.OddR);

        public bool this[VectorXYInt index] => false;

        public bool this[int index] => false;
    }

    private sealed class CountingBoolHexMap : IHexMap<bool>
    {
        public CountingBoolHexMap(HexMapTopology topology)
        {
            Topology = topology;
        }

        public HexMapTopology Topology { get; }

        public int ReadCount { get; private set; }

        public bool this[VectorXYInt index]
        {
            get
            {
                ReadCount++;
                return ((index.X + index.Y) & 3) == 0;
            }
        }

        public bool this[int index]
        {
            get
            {
                ReadCount++;
                return (index & 3) == 0;
            }
        }

        public void ResetReadCount() => ReadCount = 0;
    }
}
