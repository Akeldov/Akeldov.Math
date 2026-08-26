namespace Akeldov.Math.Hexes.Tests.Maps;

public class IntHexMapTests
{
    [Test]
    public void MinAndMax_ReturnCurrentValueRange()
    {
        var values = new[] { 3, -2, 7 };
        var map = new IntHexMap(new HexMapTopology(3, 1, Layout.OddR), values);
        IIntHexMap readOnlyMap = map;

        map[0] = -4;
        values[2] = 9;

        Assert.Multiple(() =>
        {
            Assert.That(readOnlyMap.Min, Is.EqualTo(-4));
            Assert.That(readOnlyMap.Max, Is.EqualTo(9));
        });
    }

    [Test]
    public void MinAndMax_WhenMapIsEmpty_Throw()
    {
        var map = new IntHexMap(new HexMapTopology(0, 0, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = map.Min);
            Assert.Throws<InvalidOperationException>(() => _ = map.Max);
        });
    }

    [Test]
    public void UnaryNegation_ReturnsElementWiseNegationWithoutChangingOperand()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var map = new IntHexMap(topology, new[] { 1, -2, 0 });

        IntHexMap result = -map;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(2));
            Assert.That(result[2], Is.Zero);
            Assert.That(map[0], Is.EqualTo(1));
        });
    }

    [Test]
    public void UnaryNegation_WhenCellContainsMinimumValue_ThrowsOverflowException()
    {
        var map = new IntHexMap(
            new HexMapTopology(1, 1, Layout.OddR),
            new[] { int.MinValue });

        Assert.Throws<OverflowException>(() => _ = -map);
    }

    [Test]
    public void UnaryNegation_WithNullOperand_Throws()
    {
        IntHexMap? missing = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => _ = -missing);
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }

    [Test]
    public void Addition_ReturnsElementWiseSumWithoutChangingOperands()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var left = new IntHexMap(topology, new[] { 1, -2, 4 });
        var right = new IntHexMap(topology, new[] { 2, 3, -1 });

        IntHexMap result = left + right;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(1));
            Assert.That(result[2], Is.EqualTo(3));
            Assert.That(left[0], Is.EqualTo(1));
            Assert.That(right[0], Is.EqualTo(2));
        });
    }

    [Test]
    public void Addition_WithEquivalentTopologies_ReturnsElementWiseSum()
    {
        var left = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 1, 2 });
        var right = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 3, 4 });

        IntHexMap result = left + right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(4));
            Assert.That(result[1], Is.EqualTo(6));
        });
    }

    [Test]
    public void Addition_WithDifferentTopologies_Throws()
    {
        var left = new IntHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new IntHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new IntHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left + differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left + differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Addition_WithNullOperand_Throws()
    {
        var map = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing + map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map + missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Addition_WithIntValue_AddsValueToEveryCellWithoutChangingMap()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var map = new IntHexMap(topology, new[] { 1, -2, 4 });

        IntHexMap rightValueResult = map + 2;
        IntHexMap leftValueResult = 2 + map;
        rightValueResult[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(rightValueResult.Topology, Is.EqualTo(topology));
            Assert.That(rightValueResult[0], Is.EqualTo(100));
            Assert.That(rightValueResult[1], Is.EqualTo(0));
            Assert.That(rightValueResult[2], Is.EqualTo(6));
            Assert.That(leftValueResult[0], Is.EqualTo(3));
            Assert.That(leftValueResult[1], Is.EqualTo(0));
            Assert.That(leftValueResult[2], Is.EqualTo(6));
            Assert.That(map[0], Is.EqualTo(1));
        });
    }

    [Test]
    public void Addition_WithIntValueAndNullMap_Throws()
    {
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing + 1)!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = 1 + missing)!.ParamName,
                Is.EqualTo("map"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Subtraction_ReturnsElementWiseDifferenceWithoutChangingOperands()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var left = new IntHexMap(topology, new[] { 1, -2, 4 });
        var right = new IntHexMap(topology, new[] { 2, 3, -1 });

        IntHexMap result = left - right;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(-5));
            Assert.That(result[2], Is.EqualTo(5));
            Assert.That(left[0], Is.EqualTo(1));
            Assert.That(right[0], Is.EqualTo(2));
        });
    }

    [Test]
    public void Subtraction_WithEquivalentTopologies_ReturnsElementWiseDifference()
    {
        var left = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 1, 2 });
        var right = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 3, 4 });

        IntHexMap result = left - right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(-2));
            Assert.That(result[1], Is.EqualTo(-2));
        });
    }

    [Test]
    public void Subtraction_WithDifferentTopologies_Throws()
    {
        var left = new IntHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new IntHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new IntHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left - differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left - differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Subtraction_WithNullOperand_Throws()
    {
        var map = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing - map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map - missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Subtraction_WithIntValue_AppliesBothOperandOrdersWithoutChangingMap()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var map = new IntHexMap(topology, new[] { 1, -2, 4 });

        IntHexMap mapMinusValue = map - 2;
        IntHexMap valueMinusMap = 2 - map;
        mapMinusValue[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(mapMinusValue.Topology, Is.EqualTo(topology));
            Assert.That(mapMinusValue[0], Is.EqualTo(100));
            Assert.That(mapMinusValue[1], Is.EqualTo(-4));
            Assert.That(mapMinusValue[2], Is.EqualTo(2));
            Assert.That(valueMinusMap[0], Is.EqualTo(1));
            Assert.That(valueMinusMap[1], Is.EqualTo(4));
            Assert.That(valueMinusMap[2], Is.EqualTo(-2));
            Assert.That(map[0], Is.EqualTo(1));
        });
    }

    [Test]
    public void Subtraction_WithIntValueAndNullMap_Throws()
    {
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing - 1)!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = 1 - missing)!.ParamName,
                Is.EqualTo("map"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Arithmetic_WhenCellValueOverflows_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var maximum = new IntHexMap(topology, new[] { int.MaxValue });
        var minimum = new IntHexMap(topology, new[] { int.MinValue });
        var one = new IntHexMap(topology, new[] { 1 });
        var two = new IntHexMap(topology, new[] { 2 });
        var greaterThanHalfMaximum = new IntHexMap(topology, new[] { (int.MaxValue / 2) + 1 });

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = maximum + one);
            Assert.Throws<OverflowException>(() => _ = maximum + 1);
            Assert.Throws<OverflowException>(() => _ = 1 + maximum);
            Assert.Throws<OverflowException>(() => _ = minimum - one);
            Assert.Throws<OverflowException>(() => _ = minimum - 1);
            Assert.Throws<OverflowException>(() => _ = 1 - minimum);
            Assert.Throws<OverflowException>(() => _ = greaterThanHalfMaximum * two);
            Assert.Throws<OverflowException>(() => _ = greaterThanHalfMaximum * 2);
            Assert.Throws<OverflowException>(() => _ = 2 * greaterThanHalfMaximum);
            Assert.Throws<OverflowException>(() => _ = minimum / -1);
        });
    }

    [Test]
    public void Multiplication_ReturnsElementWiseProductWithoutChangingOperands()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var left = new IntHexMap(topology, new[] { 3, -4, 8 });
        var right = new IntHexMap(topology, new[] { 2, 3, -1 });

        IntHexMap result = left * right;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(-12));
            Assert.That(result[2], Is.EqualTo(-8));
            Assert.That(left[0], Is.EqualTo(3));
            Assert.That(right[0], Is.EqualTo(2));
        });
    }

    [Test]
    public void Multiplication_WithEquivalentTopologies_ReturnsElementWiseProduct()
    {
        var left = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 2, 3 });
        var right = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 4, -2 });

        IntHexMap result = left * right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(8));
            Assert.That(result[1], Is.EqualTo(-6));
        });
    }

    [Test]
    public void Multiplication_WithDifferentTopologies_Throws()
    {
        var left = new IntHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new IntHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new IntHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left * differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left * differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Multiplication_WithNullOperand_Throws()
    {
        var map = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing * map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map * missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Multiplication_WithIntValue_MultipliesEveryCellWithoutChangingMap()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var map = new IntHexMap(topology, new[] { 3, -4, 8 });

        IntHexMap rightValueResult = map * 2;
        IntHexMap leftValueResult = 2 * map;
        rightValueResult[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(rightValueResult.Topology, Is.EqualTo(topology));
            Assert.That(rightValueResult[0], Is.EqualTo(100));
            Assert.That(rightValueResult[1], Is.EqualTo(-8));
            Assert.That(rightValueResult[2], Is.EqualTo(16));
            Assert.That(leftValueResult[0], Is.EqualTo(6));
            Assert.That(leftValueResult[1], Is.EqualTo(-8));
            Assert.That(leftValueResult[2], Is.EqualTo(16));
            Assert.That(map[0], Is.EqualTo(3));
        });
    }

    [Test]
    public void Multiplication_WithIntValueAndNullMap_Throws()
    {
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing * 2)!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = 2 * missing)!.ParamName,
                Is.EqualTo("map"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Division_ReturnsElementWiseIntegerQuotientWithoutChangingOperands()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var left = new IntHexMap(topology, new[] { 7, -5, 8 });
        var right = new IntHexMap(topology, new[] { 2, 2, -2 });

        IntHexMap result = left / right;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(-2));
            Assert.That(result[2], Is.EqualTo(-4));
            Assert.That(left[0], Is.EqualTo(7));
            Assert.That(right[0], Is.EqualTo(2));
        });
    }

    [Test]
    public void Division_WithEquivalentTopologies_ReturnsElementWiseIntegerQuotient()
    {
        var left = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 10, 6 });
        var right = new IntHexMap(
            new HexMapTopology(2, 1, Layout.EvenQ),
            new[] { 4, -2 });

        IntHexMap result = left / right;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo(-3));
        });
    }

    [Test]
    public void Division_WithDifferentTopologies_Throws()
    {
        var left = new IntHexMap(new HexMapTopology(2, 1, Layout.OddR));
        var differentResolution = new IntHexMap(new HexMapTopology(1, 2, Layout.OddR));
        var differentLayout = new IntHexMap(new HexMapTopology(2, 1, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left / differentResolution)!.ParamName,
                Is.EqualTo("right"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => _ = left / differentLayout)!.ParamName,
                Is.EqualTo("right"));
        });
    }

    [Test]
    public void Division_WithNullOperand_Throws()
    {
        var map = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR), new[] { 1 });
        IntHexMap? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = missing / map)!.ParamName,
                Is.EqualTo("left"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => _ = map / missing)!.ParamName,
                Is.EqualTo("right"));
#pragma warning restore CS8604
        });
    }

    [Test]
    public void Division_WithInvalidCellOperation_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var one = new IntHexMap(topology, new[] { 1 });
        var zero = new IntHexMap(topology, new[] { 0 });
        var minimum = new IntHexMap(topology, new[] { int.MinValue });
        var negativeOne = new IntHexMap(topology, new[] { -1 });

        Assert.Multiple(() =>
        {
            Assert.Throws<DivideByZeroException>(() => _ = one / zero);
            Assert.Throws<OverflowException>(() => _ = minimum / negativeOne);
        });
    }

    [Test]
    public void Division_WithIntValue_DividesEveryCellWithoutChangingMap()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var map = new IntHexMap(topology, new[] { 3, -4, 8 });

        IntHexMap result = map / 2;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(-2));
            Assert.That(result[2], Is.EqualTo(4));
            Assert.That(map[0], Is.EqualTo(3));
        });
    }

    [Test]
    public void Division_ByZero_Throws()
    {
        var map = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));

        Assert.Throws<DivideByZeroException>(() => _ = map / 0);
    }

    [Test]
    public void Division_WithIntValueAndNullMap_Throws()
    {
        IntHexMap? missing = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => _ = missing / 2);
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }

    [Test]
    public void Remainder_WithIntValue_ReturnsElementWiseRemainderWithoutChangingMap()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var map = new IntHexMap(topology, new[] { 7, -5, 8 });

        IntHexMap result = map % 3;
        result[0] = 100;

        Assert.Multiple(() =>
        {
            Assert.That(result.Topology, Is.EqualTo(topology));
            Assert.That(result[0], Is.EqualTo(100));
            Assert.That(result[1], Is.EqualTo(-2));
            Assert.That(result[2], Is.EqualTo(2));
            Assert.That(map[0], Is.EqualTo(7));
        });
    }

    [Test]
    public void Remainder_WithInvalidDivisor_Throws()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var one = new IntHexMap(topology, new[] { 1 });
        var minimum = new IntHexMap(topology, new[] { int.MinValue });

        Assert.Multiple(() =>
        {
            Assert.Throws<DivideByZeroException>(() => _ = one % 0);
            Assert.Throws<OverflowException>(() => _ = minimum % -1);
        });
    }

    [Test]
    public void Remainder_WithIntValueAndNullMap_Throws()
    {
        IntHexMap? missing = null;

#pragma warning disable CS8604
        var exception = Assert.Throws<ArgumentNullException>(() => _ = missing % 2);
#pragma warning restore CS8604

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }
}
