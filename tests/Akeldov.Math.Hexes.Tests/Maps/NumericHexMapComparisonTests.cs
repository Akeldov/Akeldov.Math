namespace Akeldov.Math.Hexes.Tests.Maps;

public class NumericHexMapComparisonTests
{
    [Test]
    public void Comparison_WithFloatMaps_ReturnsCellwiseMasks()
    {
        var topology = new HexMapTopology(4, 2, Layout.OddR);
        var left = new FloatHexMap(
            topology,
            new[]
            {
                1f, 2f, 3f, float.NaN,
                2f, float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity,
            });
        var right = new FloatHexMap(
            topology,
            new[]
            {
                2f, 2f, 2f, 2f,
                float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f,
            });

        BoolHexMap less = left < right;
        BoolHexMap lessOrEqual = left <= right;
        BoolHexMap greater = left > right;
        BoolHexMap greaterOrEqual = left >= right;

        AssertMask(less, topology, true, false, false, false, false, false, false, false);
        AssertMask(lessOrEqual, topology, true, true, false, false, false, true, true, false);
        AssertMask(greater, topology, false, false, true, false, false, false, false, true);
        AssertMask(greaterOrEqual, topology, false, true, true, false, false, true, true, true);

        less[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(lessOrEqual[0], Is.True);
            Assert.That(left[0], Is.EqualTo(1f));
            Assert.That(right[0], Is.EqualTo(2f));
        });
    }

    [Test]
    public void Comparison_WithIntMaps_ReturnsCellwiseMasks()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var left = new IntHexMap(topology, new[] { 1, 2, 3, int.MinValue, int.MaxValue, int.MinValue });
        var right = new IntHexMap(topology, new[] { 2, 2, 2, int.MaxValue, int.MinValue, int.MinValue });

        BoolHexMap less = left < right;
        BoolHexMap lessOrEqual = left <= right;
        BoolHexMap greater = left > right;
        BoolHexMap greaterOrEqual = left >= right;

        AssertMask(less, topology, true, false, false, true, false, false);
        AssertMask(lessOrEqual, topology, true, true, false, true, false, true);
        AssertMask(greater, topology, false, false, true, false, true, false);
        AssertMask(greaterOrEqual, topology, false, true, true, false, true, true);

        less[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(lessOrEqual[0], Is.True);
            Assert.That(left[0], Is.EqualTo(1));
            Assert.That(right[0], Is.EqualTo(2));
        });
    }

    [Test]
    public void Comparison_WithFloatLeftAndIntRight_ReturnsCellwiseMasks()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var left = new FloatHexMap(
            topology,
            new[] { 1f, 2f, 3f, float.NaN, 16_777_216f, float.PositiveInfinity });
        var right = new IntHexMap(topology, new[] { 2, 2, 2, 2, 16_777_217, int.MaxValue });

        BoolHexMap less = left < right;
        BoolHexMap lessOrEqual = left <= right;
        BoolHexMap greater = left > right;
        BoolHexMap greaterOrEqual = left >= right;

        AssertMask(less, topology, true, false, false, false, false, false);
        AssertMask(lessOrEqual, topology, true, true, false, false, true, false);
        AssertMask(greater, topology, false, false, true, false, false, true);
        AssertMask(greaterOrEqual, topology, false, true, true, false, true, true);

        less[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(lessOrEqual[0], Is.True);
            Assert.That(left[0], Is.EqualTo(1f));
            Assert.That(right[0], Is.EqualTo(2));
        });
    }

    [Test]
    public void Comparison_WithIntLeftAndFloatRight_ReturnsCellwiseMasks()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var left = new IntHexMap(topology, new[] { 1, 2, 3, 2, 16_777_217, int.MinValue });
        var right = new FloatHexMap(
            topology,
            new[] { 2f, 2f, 2f, float.NaN, 16_777_216f, float.NegativeInfinity });

        BoolHexMap less = left < right;
        BoolHexMap lessOrEqual = left <= right;
        BoolHexMap greater = left > right;
        BoolHexMap greaterOrEqual = left >= right;

        AssertMask(less, topology, true, false, false, false, false, false);
        AssertMask(lessOrEqual, topology, true, true, false, false, true, false);
        AssertMask(greater, topology, false, false, true, false, false, true);
        AssertMask(greaterOrEqual, topology, false, true, true, false, true, true);

        less[0] = false;

        Assert.Multiple(() =>
        {
            Assert.That(lessOrEqual[0], Is.True);
            Assert.That(left[0], Is.EqualTo(1));
            Assert.That(right[0], Is.EqualTo(2f));
        });
    }

    [Test]
    public void Comparison_WhenLastCellMatchesCondition_SetsLastMaskCell()
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var lowFloat = new FloatHexMap(topology, new[] { 0f, 0f, 0f, 1f });
        var highFloat = new FloatHexMap(topology, new[] { 0f, 0f, 0f, 2f });
        var lowInt = new IntHexMap(topology, new[] { 0, 0, 0, 1 });
        var highInt = new IntHexMap(topology, new[] { 0, 0, 0, 2 });

        Assert.Multiple(() =>
        {
            Assert.That((lowFloat < highFloat)[3], Is.True);
            Assert.That((lowFloat <= highFloat)[3], Is.True);
            Assert.That((highFloat > lowFloat)[3], Is.True);
            Assert.That((highFloat >= lowFloat)[3], Is.True);

            Assert.That((lowFloat < highInt)[3], Is.True);
            Assert.That((lowFloat <= highInt)[3], Is.True);
            Assert.That((highFloat > lowInt)[3], Is.True);
            Assert.That((highFloat >= lowInt)[3], Is.True);

            Assert.That((lowInt < highFloat)[3], Is.True);
            Assert.That((lowInt <= highFloat)[3], Is.True);
            Assert.That((highInt > lowFloat)[3], Is.True);
            Assert.That((highInt >= lowFloat)[3], Is.True);

            Assert.That((lowInt < highInt)[3], Is.True);
            Assert.That((lowInt <= highInt)[3], Is.True);
            Assert.That((highInt > lowInt)[3], Is.True);
            Assert.That((highInt >= lowInt)[3], Is.True);
        });
    }

    [Test]
    public void Comparison_WithEquivalentTopologies_ReturnsMasks()
    {
        var floatLeft = new FloatHexMap(new HexMapTopology(1, 1, Layout.EvenQ), new[] { 1f });
        var floatRight = new FloatHexMap(new HexMapTopology(1, 1, Layout.EvenQ), new[] { 2f });
        var intLeft = new IntHexMap(new HexMapTopology(1, 1, Layout.EvenQ), new[] { 1 });
        var intRight = new IntHexMap(new HexMapTopology(1, 1, Layout.EvenQ), new[] { 2 });

        Assert.Multiple(() =>
        {
            Assert.That((floatLeft < floatRight)[0], Is.True);
            Assert.That((floatLeft < intRight)[0], Is.True);
            Assert.That((intLeft < floatRight)[0], Is.True);
            Assert.That((intLeft < intRight)[0], Is.True);
        });
    }

    [Test]
    public void Comparison_WithEmptyMaps_ReturnsEmptyMasks()
    {
        var topology = new HexMapTopology(0, 0, Layout.OddR);
        var floatLeft = new FloatHexMap(topology);
        var floatRight = new FloatHexMap(topology);
        var intLeft = new IntHexMap(topology);
        var intRight = new IntHexMap(topology);

        Assert.Multiple(() =>
        {
            Assert.That((floatLeft < floatRight).Topology, Is.EqualTo(topology));
            Assert.That((floatLeft < floatRight).Topology.Count, Is.Zero);
            Assert.That((floatLeft < intRight).Topology.Count, Is.Zero);
            Assert.That((intLeft < floatRight).Topology.Count, Is.Zero);
            Assert.That((intLeft < intRight).Topology.Count, Is.Zero);
        });
    }

    [Test]
    public void Comparison_WithDifferentResolution_Throws()
    {
        var floatLeft = new FloatHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var floatRight = new FloatHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var intLeft = new IntHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var intRight = new IntHexMap(new HexMapTopology(1, 2, Layout.OddR));

        AssertArgumentException(
            "right",
            () => _ = floatLeft < floatRight,
            () => _ = floatLeft <= floatRight,
            () => _ = floatLeft > floatRight,
            () => _ = floatLeft >= floatRight,
            () => _ = floatLeft < intRight,
            () => _ = floatLeft <= intRight,
            () => _ = floatLeft > intRight,
            () => _ = floatLeft >= intRight,
            () => _ = intLeft < floatRight,
            () => _ = intLeft <= floatRight,
            () => _ = intLeft > floatRight,
            () => _ = intLeft >= floatRight,
            () => _ = intLeft < intRight,
            () => _ = intLeft <= intRight,
            () => _ = intLeft > intRight,
            () => _ = intLeft >= intRight);
    }

    [Test]
    public void Comparison_WithDifferentLayout_Throws()
    {
        var floatLeft = new FloatHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var floatRight = new FloatHexMap(new HexMapTopology(2, 1, Layout.EvenR));
        var intLeft = new IntHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var intRight = new IntHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        AssertArgumentException(
            "right",
            () => _ = floatLeft < floatRight,
            () => _ = floatLeft <= floatRight,
            () => _ = floatLeft > floatRight,
            () => _ = floatLeft >= floatRight,
            () => _ = floatLeft < intRight,
            () => _ = floatLeft <= intRight,
            () => _ = floatLeft > intRight,
            () => _ = floatLeft >= intRight,
            () => _ = intLeft < floatRight,
            () => _ = intLeft <= floatRight,
            () => _ = intLeft > floatRight,
            () => _ = intLeft >= floatRight,
            () => _ = intLeft < intRight,
            () => _ = intLeft <= intRight,
            () => _ = intLeft > intRight,
            () => _ = intLeft >= intRight);
    }

    [Test]
    public void Comparison_WithNullLeftOperand_Throws()
    {
        var floatMap = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR));
        var intMap = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));
        FloatHexMap? missingFloatMap = null;
        IntHexMap? missingIntMap = null;

#pragma warning disable CS8604
        AssertArgumentNull(
            "left",
            () => _ = missingFloatMap < floatMap,
            () => _ = missingFloatMap <= floatMap,
            () => _ = missingFloatMap > floatMap,
            () => _ = missingFloatMap >= floatMap,
            () => _ = missingFloatMap < intMap,
            () => _ = missingFloatMap <= intMap,
            () => _ = missingFloatMap > intMap,
            () => _ = missingFloatMap >= intMap,
            () => _ = missingIntMap < floatMap,
            () => _ = missingIntMap <= floatMap,
            () => _ = missingIntMap > floatMap,
            () => _ = missingIntMap >= floatMap,
            () => _ = missingIntMap < intMap,
            () => _ = missingIntMap <= intMap,
            () => _ = missingIntMap > intMap,
            () => _ = missingIntMap >= intMap);
#pragma warning restore CS8604
    }

    [Test]
    public void Comparison_WithNullRightOperand_Throws()
    {
        var floatMap = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR));
        var intMap = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));
        FloatHexMap? missingFloatMap = null;
        IntHexMap? missingIntMap = null;

#pragma warning disable CS8604
        AssertArgumentNull(
            "right",
            () => _ = floatMap < missingFloatMap,
            () => _ = floatMap <= missingFloatMap,
            () => _ = floatMap > missingFloatMap,
            () => _ = floatMap >= missingFloatMap,
            () => _ = floatMap < missingIntMap,
            () => _ = floatMap <= missingIntMap,
            () => _ = floatMap > missingIntMap,
            () => _ = floatMap >= missingIntMap,
            () => _ = intMap < missingFloatMap,
            () => _ = intMap <= missingFloatMap,
            () => _ = intMap > missingFloatMap,
            () => _ = intMap >= missingFloatMap,
            () => _ = intMap < missingIntMap,
            () => _ = intMap <= missingIntMap,
            () => _ = intMap > missingIntMap,
            () => _ = intMap >= missingIntMap);
#pragma warning restore CS8604
    }

    private static void AssertMask(BoolHexMap actual, HexMapTopology topology, params bool[] expected)
    {
        Assert.That(actual.Topology, Is.EqualTo(topology));
        Assert.That(actual.Topology.Count, Is.EqualTo(expected.Length));

        for (int index = 0; index < expected.Length; index++)
            Assert.That(actual[index], Is.EqualTo(expected[index]), $"Unexpected value at index {index}.");
    }

    private static void AssertArgumentNull(string parameterName, params TestDelegate[] actions)
    {
        Assert.Multiple(() =>
        {
            foreach (TestDelegate action in actions)
                Assert.That(Assert.Throws<ArgumentNullException>(action)!.ParamName, Is.EqualTo(parameterName));
        });
    }

    private static void AssertArgumentException(string parameterName, params TestDelegate[] actions)
    {
        Assert.Multiple(() =>
        {
            foreach (TestDelegate action in actions)
                Assert.That(Assert.Throws<ArgumentException>(action)!.ParamName, Is.EqualTo(parameterName));
        });
    }
}
