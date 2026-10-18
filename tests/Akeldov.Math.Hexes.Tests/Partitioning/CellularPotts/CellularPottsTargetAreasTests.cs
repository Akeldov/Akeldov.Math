using Akeldov.Math.Hexes.Partitioning.CellularPotts;

namespace Akeldov.Math.Hexes.Tests.Partitioning.CellularPotts;

[TestFixture(0)]
[TestFixture(1)]
[TestFixture(2)]
[TestFixture(3)]
[TestFixture(4)]
[TestFixture(5)]
[TestFixture(6)]
[TestFixture(7)]
public class CellularPottsTargetAreasTests
{
    private readonly int _overload;

    public CellularPottsTargetAreasTests(int overload) => _overload = overload;

    [Test]
    public void Apply_SameParametersAcceptDifferentTargetsWithoutChangingInputs()
    {
        var source = Source();
        var parameters = new CellularPottsParameters(1, 0, 0, 1, CellularPottsConnectivity.None);
        var grow = new Dictionary<int, float> { [7] = 0, [42] = 3 };
        var shrink = new Dictionary<int, float> { [7] = 3, [42] = 0 };

        Assert.That(Apply(source, grow, parameters, new FirstHexRandom())[0], Is.EqualTo(42));
        Assert.That(Apply(source, shrink, parameters, new FirstHexRandom())[0], Is.EqualTo(7));
        Assert.That(grow, Is.EquivalentTo(new Dictionary<int, float> { [7] = 0, [42] = 3 }));
        Assert.That(shrink, Is.EquivalentTo(new Dictionary<int, float> { [7] = 3, [42] = 0 }));
        Assert.That(CellularPottsTests.Values(source), Is.EqualTo(new[] { 7, 42, 42 }));
    }

    [Test]
    public void Apply_NullEmptyAndMissingTargetsUseInitialParticipatingAreas()
    {
        var source = Source();
        var parameters = new CellularPottsParameters(1, 0, 0, 1, CellularPottsConnectivity.None);
        var initial = new Dictionary<int, float> { [7] = 1, [42] = 2 };
        var expected = Values(Apply(source, initial, parameters, new FirstHexRandom()));
        Assert.That(Values(Apply(source, null, parameters, new FirstHexRandom())), Is.EqualTo(expected));
        Assert.That(Values(Apply(source, new Dictionary<int, float>(), parameters, new FirstHexRandom())), Is.EqualTo(expected));

        var missing = Apply(source, new Dictionary<int, float> { [7] = 0 }, parameters, new FirstHexRandom());
        var explicitTargets = Apply(source, new Dictionary<int, float> { [7] = 0, [42] = 2 }, parameters, new FirstHexRandom());
        Assert.That(Values(missing), Is.EqualTo(Values(explicitTargets)));
        Assert.That(missing[0], Is.EqualTo(42));
    }

    [Test]
    public void Apply_CapturesTargetsBeforeDrawingRandomNumbers()
    {
        var source = Source();
        var parameters = new CellularPottsParameters(1, 0, 0, 1, CellularPottsConnectivity.None);
        var targets = new Dictionary<int, float> { [7] = 0, [42] = 3 };
        var result = Apply(source, targets, parameters, new FirstHexRandom(() =>
        {
            targets[7] = 3;
            targets[42] = 0;
        }));

        Assert.That(result[0], Is.EqualTo(42));
        targets.Clear();
        Assert.That(result[0], Is.EqualTo(42));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Apply_ValidatesTargetsEvenAtZeroStepsOrWithNoParticipants(bool empty)
    {
        var source = empty
            ? CellularPottsTests.Partition(new HexMapTopology(0, 0, Layout.OddR), _ => 7, 7, 42)
            : Source();
        var parameters = new CellularPottsParameters(0);
        foreach (float value in new[] { -1f, float.NaN, float.NegativeInfinity, float.PositiveInfinity, (float)int.MaxValue })
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Apply(source,
                new Dictionary<int, float> { [7] = value }, parameters, new FirstHexRandom()))!.ParamName,
                Is.EqualTo("targetAreas"), $"value={value}");
        foreach (int id in new[] { -1, 99 })
            Assert.That(Assert.Throws<ArgumentException>(() => Apply(source,
                new Dictionary<int, float> { [id] = 1 }, parameters, new FirstHexRandom()))!.ParamName,
                Is.EqualTo("targetAreas"), $"id={id}");

        // Largest float below int.MaxValue is valid; (float)int.MaxValue above rounds out of range.
        Assert.DoesNotThrow(() => Apply(source, new Dictionary<int, float> { [7] = 2147483520f, [42] = 2.5f },
            parameters, new FirstHexRandom()));
    }

    private static HexPartitionMap<HexPartitionCell> Source() =>
        CellularPottsTests.Partition(new HexMapTopology(3, 1, Layout.OddR), p => p.X == 0 ? 7 : 42, 7, 42);

    private PartialHexPartitionMap<HexPartitionCell> Apply(HexPartitionMap<HexPartitionCell> full,
        IReadOnlyDictionary<int, float>? targets, CellularPottsParameters parameters, Random random)
    {
        var partial = new PartialHexPartitionMap<HexPartitionCell>(full.Topology, full.Cells);
        var mask = new BoolHexMap(full.Topology, Enumerable.Repeat(true, full.Topology.Count).ToArray());
        var regions = new IntHexMap(full.Topology);
        var nullable = new HexMap<int?>(full.Topology, Enumerable.Repeat((int?)0, full.Topology.Count).ToArray());
        return _overload switch
        {
            0 => new PartialHexPartitionMap<HexPartitionCell>(full.Topology, full.ApplyCellularPotts(targets, parameters, random).Cells),
            1 => full.ApplyCellularPotts(mask, targets, parameters, random),
            2 => full.ApplyCellularPotts(nullable, targets, parameters, random),
            3 => full.ApplyCellularPotts(mask, regions, targets, parameters, random),
            4 => partial.ApplyCellularPotts(targets, parameters, random),
            5 => partial.ApplyCellularPotts(mask, targets, parameters, random),
            6 => partial.ApplyCellularPotts(nullable, targets, parameters, random),
            7 => partial.ApplyCellularPotts(mask, regions, targets, parameters, random),
            _ => throw new ArgumentOutOfRangeException(nameof(_overload))
        };
    }

    private static int?[] Values(PartialHexPartitionMap<HexPartitionCell> map) => PartialCellularPottsTests.Values(map);

    private sealed class FirstHexRandom : Random
    {
        private readonly Action? _beforeFirstDraw;
        private bool _started;

        internal FirstHexRandom(Action? beforeFirstDraw = null) => _beforeFirstDraw = beforeFirstDraw;

        public override int Next(int maxValue)
        {
            if (!_started)
            {
                _started = true;
                _beforeFirstDraw?.Invoke();
            }
            return 0;
        }

        public override double NextDouble() => throw new AssertionException("Zero temperature must not draw an acceptance probability.");
    }
}
