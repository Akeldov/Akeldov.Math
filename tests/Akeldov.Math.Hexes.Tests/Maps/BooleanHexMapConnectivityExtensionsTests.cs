using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class BooleanHexMapConnectivityExtensionsTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void FloodFill_FromTrueCell_ReturnsOnlyItsConnectedComponent(Layout layout)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var seed = new VectorXYInt(3, 3);
        VectorXYInt adjacent = seed.GetAdjacents(layout)[0];
        var source = Map(topology, seed, adjacent, new VectorXYInt(6, 6));

        BoolHexMap result = source.FloodFill(seed);

        Assert.That(ReadValues(result), Is.EqualTo(Values(topology, seed, adjacent)));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void FloodFill_FromFalseCell_ReturnsConnectedRegionOfFalseCells(Layout layout)
    {
        var topology = new HexMapTopology(5, 5, layout);
        bool[] values = Enumerable.Repeat(true, topology.Count).ToArray();
        var seed = new VectorXYInt(2, 2);
        VectorXYInt adjacent = seed.GetAdjacents(layout)[0];
        values[FlatIndex(seed, topology)] = false;
        values[FlatIndex(adjacent, topology)] = false;
        var source = new BoolHexMap(topology, values);

        BoolHexMap result = source.FloodFill(seed);

        Assert.That(ReadValues(result), Is.EqualTo(Values(topology, seed, adjacent)));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(2, 0)]
    [TestCase(0, 2)]
    public void FloodFill_WhenSeedIsOutsideMap_Throws(int x, int y)
    {
        var source = new BoolHexMap(new HexMapTopology(2, 2, Layout.OddR));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => source.FloodFill(new VectorXYInt(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("seed"));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void ConnectedComponents_LabelsTrueComponentsInRowMajorDiscoveryOrder(Layout layout)
    {
        var topology = new HexMapTopology(7, 7, layout);
        var first = new VectorXYInt(0, 0);
        var second = new VectorXYInt(3, 3);
        VectorXYInt secondAdjacent = second.GetAdjacents(layout)[0];
        var third = new VectorXYInt(6, 6);
        var source = Map(topology, first, second, secondAdjacent, third);

        (IntHexMap labels, int count) = source.ConnectedComponents();

        Assert.Multiple(() =>
        {
            Assert.That(count, Is.EqualTo(3));
            Assert.That(labels[FlatIndex(first, topology)], Is.EqualTo(1));
            Assert.That(labels[FlatIndex(second, topology)], Is.EqualTo(2));
            Assert.That(labels[FlatIndex(secondAdjacent, topology)], Is.EqualTo(2));
            Assert.That(labels[FlatIndex(third, topology)], Is.EqualTo(3));
            Assert.That(labels[FlatIndex(new VectorXYInt(1, 1), topology)], Is.Zero);
        });
    }

    [Test]
    public void ConnectedComponents_ForUniformMaps_ReturnsExpectedCounts()
    {
        var topology = new HexMapTopology(4, 3, Layout.EvenR);
        var empty = new BoolHexMap(topology);
        var full = new BoolHexMap(topology, Enumerable.Repeat(true, topology.Count).ToArray());

        (IntHexMap emptyLabels, int emptyCount) = empty.ConnectedComponents();
        (IntHexMap fullLabels, int fullCount) = full.ConnectedComponents();

        Assert.Multiple(() =>
        {
            Assert.That(emptyCount, Is.Zero);
            Assert.That(ReadValues(emptyLabels), Is.All.Zero);
            Assert.That(fullCount, Is.EqualTo(1));
            Assert.That(ReadValues(fullLabels), Is.All.EqualTo(1));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void DistanceTransform_ReturnsGraphDistanceToNearestTarget(Layout layout)
    {
        var topology = new HexMapTopology(7, 6, layout);
        var target = new VectorXYInt(3, 2);
        bool[] values = Enumerable.Repeat(true, topology.Count).ToArray();
        values[FlatIndex(target, topology)] = false;
        var source = new BoolHexMap(topology, values);

        IntHexMap result = source.DistanceTransform();

        Assert.That(ReadValues(result), Is.EqualTo(ReferenceDistances(source, targetValue: false)));
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void DistanceTransform_CanMeasureDistanceToTrueCells(Layout layout)
    {
        var topology = new HexMapTopology(6, 7, layout);
        var source = Map(topology, new VectorXYInt(2, 3), new VectorXYInt(5, 0));

        IntHexMap result = source.DistanceTransform(targetValue: true);

        Assert.That(ReadValues(result), Is.EqualTo(ReferenceDistances(source, targetValue: true)));
    }

    [Test]
    public void DistanceTransform_WhenTargetDoesNotExist_ReturnsMaxValue()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddQ);
        var source = new BoolHexMap(topology, Enumerable.Repeat(true, topology.Count).ToArray());

        IntHexMap result = source.DistanceTransform(targetValue: false);

        Assert.That(ReadValues(result), Is.All.EqualTo(int.MaxValue));
    }

    [Test]
    public void ConnectivityOperations_WithEmptyMap_ReturnEmptyResults()
    {
        var source = new BoolHexMap(new HexMapTopology(0, 0, Layout.OddR));

        (IntHexMap labels, int count) = source.ConnectedComponents();
        IntHexMap distances = source.DistanceTransform();

        Assert.Multiple(() =>
        {
            Assert.That(count, Is.Zero);
            Assert.That(labels.Topology.Count, Is.Zero);
            Assert.That(distances.Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void SpatialConnectivityOperations_PreserveGeometryAndReturnSpecializations()
    {
        var geometry = new HexMapGeometry(
            4,
            3,
            new VectorXY(10f, -20f),
            2f,
            Layout.EvenQ);
        bool[] sourceValues = Values(geometry.Topology, new VectorXYInt(1, 1));
        var source = new SpatialBoolHexMap(geometry, sourceValues);

        SpatialBoolHexMap fill = source.FloodFill(new VectorXYInt(1, 1));
        (SpatialIntHexMap labels, int count) = source.ConnectedComponents();
        SpatialIntHexMap distances = source.DistanceTransform(targetValue: true);

        Assert.Multiple(() =>
        {
            Assert.That(fill, Is.TypeOf<SpatialBoolHexMap>());
            Assert.That(labels, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(distances, Is.TypeOf<SpatialIntHexMap>());
            Assert.That(fill.Geometry, Is.EqualTo(geometry));
            Assert.That(labels.Geometry, Is.EqualTo(geometry));
            Assert.That(distances.Geometry, Is.EqualTo(geometry));
            Assert.That(count, Is.EqualTo(1));
        });

        fill[0] = !fill[0];
        labels[0] = 99;
        distances[0] = 99;
        Assert.That(ReadValues(source), Is.EqualTo(sourceValues));
    }

    [Test]
    public void ConnectivityOperations_WhenMapIsNull_Throw()
    {
        IHexMap<bool> ordinary = null!;
        ISpatialHexMap<bool> spatial = null!;
        var seed = VectorXYInt.Zero;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.FloodFill(seed))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.ConnectedComponents())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => ordinary.DistanceTransform())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.FloodFill(seed))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.ConnectedComponents())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => spatial.DistanceTransform())!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void SpatialConnectivityOperations_WhenTopologyAndGeometryDiffer_Throw()
    {
        ISpatialHexMap<bool> map = new InconsistentSpatialBoolMap();

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => map.FloodFill(VectorXYInt.Zero))!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.ConnectedComponents())!.ParamName, Is.EqualTo("map"));
            Assert.That(Assert.Throws<ArgumentException>(() => map.DistanceTransform())!.ParamName, Is.EqualTo("map"));
        });
    }

    [Test]
    public void ConnectivityOperations_ReadSourceALinearNumberOfTimes()
    {
        var source = new CountingBoolHexMap(new HexMapTopology(32, 24, Layout.EvenR));

        source.ResetReadCount();
        _ = source.FloodFill(VectorXYInt.Zero);
        Assert.That(source.ReadCount, Is.LessThanOrEqualTo(source.Topology.Count * 7 + 1));

        source.ResetReadCount();
        _ = source.ConnectedComponents();
        Assert.That(source.ReadCount, Is.LessThanOrEqualTo(source.Topology.Count * 7));

        source.ResetReadCount();
        _ = source.DistanceTransform();
        Assert.That(source.ReadCount, Is.LessThanOrEqualTo(source.Topology.Count));
    }

    private static int[] ReferenceDistances(IHexMap<bool> map, bool targetValue)
    {
        int count = map.Topology.Count;
        int width = map.Topology.Resolution.X;
        var distances = Enumerable.Repeat(int.MaxValue, count).ToArray();
        var queue = new Queue<int>();

        for (int index = 0; index < count; index++)
        {
            if (map[index] != targetValue)
                continue;

            distances[index] = 0;
            queue.Enqueue(index);
        }

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            var coordinate = new VectorXYInt(current % width, current / width);
            foreach (VectorXYInt adjacent in coordinate.GetAdjacents(map.Topology.Layout))
            {
                if (!Contains(adjacent, map.Topology))
                    continue;

                int adjacentIndex = FlatIndex(adjacent, map.Topology);
                if (distances[adjacentIndex] != int.MaxValue)
                    continue;

                distances[adjacentIndex] = distances[current] + 1;
                queue.Enqueue(adjacentIndex);
            }
        }

        return distances;
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

    private static T[] ReadValues<T>(IHexMap<T> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private static bool Contains(VectorXYInt index, HexMapTopology topology) =>
        index.X >= 0 && index.X < topology.Resolution.X &&
        index.Y >= 0 && index.Y < topology.Resolution.Y;

    private static int FlatIndex(VectorXYInt index, HexMapTopology topology) =>
        index.Y * topology.Resolution.X + index.X;

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
