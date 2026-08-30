using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapValueMappingExtensionsTests
{
    [Test]
    public void MapValues_MapsSourceValuesInRowMajorOrder()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        IHexMap<int> source = new HexMap<int>(topology, new[] { 3, 1, 4, 1, 5, 9 });
        var selectedValues = new List<int>();

        HexMap<string> result = source.MapValues((int value) =>
        {
            selectedValues.Add(value);
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(selectedValues, Is.EqualTo(new[] { 3, 1, 4, 1, 5, 9 }));
            Assert.That(
                Enumerable.Range(0, topology.Count).Select(index => result[index]),
                Is.EqualTo(new[] { "3", "1", "4", "1", "5", "9" }));
        });
    }

    [Test]
    public void MapValues_WithSourceValues_FromSpatialMap_PreservesGeometry()
    {
        var geometry = new HexMapGeometry(3, 1, new(4f, -2f), 1.5f, Layout.EvenQ);
        ISpatialHexMap<int> source = new SpatialHexMap<int>(geometry, new[] { 2, 4, 8 });

        SpatialHexMap<int> result = source.MapValues((int value) => value * 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(
                Enumerable.Range(0, geometry.Topology.Count).Select(index => result[index]),
                Is.EqualTo(new[] { 4, 8, 16 }));
        });
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void MapValues_MapsPartialSextupletsInRowMajorOrder(Layout layout)
    {
        var topology = new HexMapTopology(3, 2, layout);
        IHexMap<int> source = new HexMap<int>(topology, Enumerable.Range(1, topology.Count).ToArray());
        var samples = new List<PartialSextuplet<int>>();

        HexMap<int> result = source.MapValues(sample =>
        {
            samples.Add(sample);
            return GetPresentValueSum(sample);
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(samples, Has.Count.EqualTo(topology.Count));

            for (int index = 0; index < topology.Count; index++)
            {
                PartialSextuplet<int> expected = source.SamplePartialSextuplet(
                    new(index % topology.Resolution.X, index / topology.Resolution.X));
                Assert.That(samples[index], Is.EqualTo(expected), $"Sample at flat index {index}");
                Assert.That(result[index], Is.EqualTo(GetPresentValueSum(expected)), $"Result at flat index {index}");
            }
        });
    }

    [Test]
    public void MapValues_FromSpatialMap_PreservesGeometry()
    {
        var geometry = new HexMapGeometry(3, 2, new(4f, -2f), 1.5f, Layout.EvenQ);
        ISpatialHexMap<int> source = new SpatialHexMap<int>(
            geometry,
            Enumerable.Range(1, geometry.Topology.Count).ToArray());

        SpatialHexMap<int> result = source.MapValues(GetPresentValueSum);

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(result[0], Is.EqualTo(GetPresentValueSum(source.SamplePartialSextuplet(new(0, 0)))));
        });
    }

    [Test]
    public void MapValues_ForSingleCell_PassesEmptyPartialSextuplet()
    {
        IHexMap<int> source = new HexMap<int>(new HexMapTopology(1, 1, Layout.OddR), new[] { 42 });
        PartialSextuplet<int> actual = default;

        HexMap<int> result = source.MapValues(sample =>
        {
            actual = sample;
            return 7;
        });

        Assert.Multiple(() =>
        {
            Assert.That(actual.Presence, Is.EqualTo(SextupletPresenceFlags.None));
            Assert.That(actual.ToSextuplet(), Is.EqualTo(default(Sextuplet<int>)));
            Assert.That(result[0], Is.EqualTo(7));
        });
    }

    [Test]
    public void MapValues_ForEmptyMap_ReturnsEmptyMapWithoutCallingSelector()
    {
        var topology = new HexMapTopology(0, 0, Layout.OddR);
        IHexMap<int> source = new HexMap<int>(topology);
        int selectorCallCount = 0;

        HexMap<int> result = source.MapValues((PartialSextuplet<int> _) => selectorCallCount++);

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result.Topology.Count, Is.Zero);
            Assert.That(selectorCallCount, Is.Zero);
        });
    }

    [Test]
    public void MapValues_WhenArgumentsAreInvalid_Throws()
    {
        IHexMap<int> nullMap = null!;
        ISpatialHexMap<int> nullSpatialMap = null!;
        var source = new HexMap<int>(new HexMapTopology(1, 1, Layout.OddR));
        var spatialSource = new SpatialHexMap<int>(new HexMapGeometry(1, 1, 1f, Layout.OddR));
        Func<int, int> nullValueSelector = null!;
        Func<PartialSextuplet<int>, int> nullSelector = null!;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.MapValues((PartialSextuplet<int> sample) => 0))!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => source.MapValues(nullValueSelector))!.ParamName,
                Is.EqualTo("selector"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => source.MapValues(nullSelector))!.ParamName,
                Is.EqualTo("selector"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullSpatialMap.MapValues((PartialSextuplet<int> sample) => 0))!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => spatialSource.MapValues(nullValueSelector))!.ParamName,
                Is.EqualTo("selector"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => spatialSource.MapValues(nullSelector))!.ParamName,
                Is.EqualTo("selector"));
        });
    }

    [Test]
    public void MapValues_WhenSpatialTopologyDoesNotMatchGeometry_Throws()
    {
        ISpatialHexMap<int> source = new InconsistentSpatialMap<int>(
            new HexMapTopology(1, 1, Layout.OddR),
            new HexMapGeometry(2, 1, 1f, Layout.OddR));

        var partialException = Assert.Throws<ArgumentException>(
            () => source.MapValues((PartialSextuplet<int> sample) => 0));
        var valueException = Assert.Throws<ArgumentException>(() => source.MapValues((int value) => 0));

        Assert.Multiple(() =>
        {
            Assert.That(partialException!.ParamName, Is.EqualTo("map"));
            Assert.That(valueException!.ParamName, Is.EqualTo("map"));
        });
    }

    private static int GetPresentValueSum(PartialSextuplet<int> sample)
    {
        int sum = 0;
        if (sample.HasAdjacent0) sum += sample.Adjacent0;
        if (sample.HasAdjacent1) sum += sample.Adjacent1;
        if (sample.HasAdjacent2) sum += sample.Adjacent2;
        if (sample.HasAdjacent3) sum += sample.Adjacent3;
        if (sample.HasAdjacent4) sum += sample.Adjacent4;
        if (sample.HasAdjacent5) sum += sample.Adjacent5;
        return sum;
    }

    private sealed class InconsistentSpatialMap<T> : ISpatialHexMap<T>
    {
        public InconsistentSpatialMap(HexMapTopology topology, HexMapGeometry geometry)
        {
            Topology = topology;
            Geometry = geometry;
        }

        public HexMapTopology Topology { get; }

        public HexMapGeometry Geometry { get; }

        public T this[Akeldov.Math.Spatial2D.VectorXYInt index] => default!;

        public T this[int index] => default!;
    }
}
