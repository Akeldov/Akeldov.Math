using System.Reflection;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapCrossOperatorTests
{
    private static readonly Type[] SpecializedTypes = { typeof(FloatHexMap), typeof(IntHexMap), typeof(BoolHexMap) };
    private static readonly Type[] GenericTypes = { typeof(HexMap<float>), typeof(HexMap<int>), typeof(HexMap<bool>) };

    [TestCase(2, 2, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    [TestCase(2, 2, Layout.OddQ)]
    [TestCase(2, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void MixedOperators_MatchSpecializedOperatorsWithoutChangingSources(int width, int height, Layout layout)
    {
        var topology = new HexMapTopology(width, height, layout);
        var floatLeft = (FloatHexMap)Argument(typeof(FloatHexMap), 0, topology);
        var floatRight = (FloatHexMap)Argument(typeof(FloatHexMap), 1, topology);
        var genericFloatLeft = (HexMap<float>)Argument(typeof(HexMap<float>), 0, topology);
        var genericFloatRight = (HexMap<float>)Argument(typeof(HexMap<float>), 1, topology);
        var intLeft = (IntHexMap)Argument(typeof(IntHexMap), 0, topology);
        var intRight = (IntHexMap)Argument(typeof(IntHexMap), 1, topology);
        var genericIntLeft = (HexMap<int>)Argument(typeof(HexMap<int>), 0, topology);
        var genericIntRight = (HexMap<int>)Argument(typeof(HexMap<int>), 1, topology);
        var boolLeft = (BoolHexMap)Argument(typeof(BoolHexMap), 0, topology);
        var boolRight = (BoolHexMap)Argument(typeof(BoolHexMap), 1, topology);
        var genericBoolLeft = (HexMap<bool>)Argument(typeof(HexMap<bool>), 0, topology);
        var genericBoolRight = (HexMap<bool>)Argument(typeof(HexMap<bool>), 1, topology);

        AssertIndependentMap(genericFloatLeft + floatRight, floatLeft + floatRight);
        AssertIndependentMap(floatLeft + genericFloatRight, floatLeft + floatRight);
        AssertIndependentMap(genericFloatLeft - floatRight, floatLeft - floatRight);
        AssertIndependentMap(floatLeft - genericFloatRight, floatLeft - floatRight);
        AssertIndependentMap(genericFloatLeft * floatRight, floatLeft * floatRight);
        AssertIndependentMap(floatLeft * genericFloatRight, floatLeft * floatRight);
        AssertIndependentMap(genericFloatLeft * intRight, floatLeft * intRight);
        AssertIndependentMap(floatLeft * genericIntRight, floatLeft * intRight);
        AssertIndependentMap(genericIntLeft * floatRight, intLeft * floatRight);
        AssertIndependentMap(intLeft * genericFloatRight, intLeft * floatRight);
        AssertIndependentMap(genericFloatLeft / floatRight, floatLeft / floatRight);
        AssertIndependentMap(floatLeft / genericFloatRight, floatLeft / floatRight);
        AssertIndependentMap(genericFloatLeft / intRight, floatLeft / intRight);
        AssertIndependentMap(floatLeft / genericIntRight, floatLeft / intRight);
        AssertIndependentMap(genericIntLeft / floatRight, intLeft / floatRight);
        AssertIndependentMap(intLeft / genericFloatRight, intLeft / floatRight);
        AssertIndependentMap(genericFloatLeft % floatRight, floatLeft % floatRight);
        AssertIndependentMap(floatLeft % genericFloatRight, floatLeft % floatRight);
        AssertIndependentMap(genericFloatLeft % intRight, floatLeft % intRight);
        AssertIndependentMap(floatLeft % genericIntRight, floatLeft % intRight);
        AssertIndependentMap(genericIntLeft % floatRight, intLeft % floatRight);
        AssertIndependentMap(intLeft % genericFloatRight, intLeft % floatRight);
        AssertIndependentMap(genericFloatLeft < floatRight, floatLeft < floatRight);
        AssertIndependentMap(floatLeft < genericFloatRight, floatLeft < floatRight);
        AssertIndependentMap(genericFloatLeft < intRight, floatLeft < intRight);
        AssertIndependentMap(floatLeft < genericIntRight, floatLeft < intRight);
        AssertIndependentMap(genericIntLeft < floatRight, intLeft < floatRight);
        AssertIndependentMap(intLeft < genericFloatRight, intLeft < floatRight);
        AssertIndependentMap(genericFloatLeft > floatRight, floatLeft > floatRight);
        AssertIndependentMap(floatLeft > genericFloatRight, floatLeft > floatRight);
        AssertIndependentMap(genericFloatLeft > intRight, floatLeft > intRight);
        AssertIndependentMap(floatLeft > genericIntRight, floatLeft > intRight);
        AssertIndependentMap(genericIntLeft > floatRight, intLeft > floatRight);
        AssertIndependentMap(intLeft > genericFloatRight, intLeft > floatRight);
        AssertIndependentMap(genericFloatLeft <= floatRight, floatLeft <= floatRight);
        AssertIndependentMap(floatLeft <= genericFloatRight, floatLeft <= floatRight);
        AssertIndependentMap(genericFloatLeft <= intRight, floatLeft <= intRight);
        AssertIndependentMap(floatLeft <= genericIntRight, floatLeft <= intRight);
        AssertIndependentMap(genericIntLeft <= floatRight, intLeft <= floatRight);
        AssertIndependentMap(intLeft <= genericFloatRight, intLeft <= floatRight);
        AssertIndependentMap(genericFloatLeft >= floatRight, floatLeft >= floatRight);
        AssertIndependentMap(floatLeft >= genericFloatRight, floatLeft >= floatRight);
        AssertIndependentMap(genericFloatLeft >= intRight, floatLeft >= intRight);
        AssertIndependentMap(floatLeft >= genericIntRight, floatLeft >= intRight);
        AssertIndependentMap(genericIntLeft >= floatRight, intLeft >= floatRight);
        AssertIndependentMap(intLeft >= genericFloatRight, intLeft >= floatRight);
        AssertIndependentMap(genericFloatLeft == floatRight, floatLeft == floatRight);
        AssertIndependentMap(floatLeft == genericFloatRight, floatLeft == floatRight);
        AssertIndependentMap(genericFloatLeft != floatRight, floatLeft != floatRight);
        AssertIndependentMap(floatLeft != genericFloatRight, floatLeft != floatRight);
        AssertIndependentMap(genericFloatLeft == intRight, floatLeft == intRight);
        AssertIndependentMap(floatLeft == genericIntRight, floatLeft == intRight);
        AssertIndependentMap(genericIntLeft == floatRight, intLeft == floatRight);
        AssertIndependentMap(intLeft == genericFloatRight, intLeft == floatRight);
        AssertIndependentMap(genericFloatLeft != intRight, floatLeft != intRight);
        AssertIndependentMap(floatLeft != genericIntRight, floatLeft != intRight);
        AssertIndependentMap(genericIntLeft != floatRight, intLeft != floatRight);
        AssertIndependentMap(intLeft != genericFloatRight, intLeft != floatRight);
        AssertIndependentMap(genericFloatLeft + intRight, floatLeft + intRight);
        AssertIndependentMap(floatLeft + genericIntRight, floatLeft + intRight);
        AssertIndependentMap(genericIntLeft + floatRight, intLeft + floatRight);
        AssertIndependentMap(intLeft + genericFloatRight, intLeft + floatRight);
        AssertIndependentMap(genericFloatLeft - intRight, floatLeft - intRight);
        AssertIndependentMap(floatLeft - genericIntRight, floatLeft - intRight);
        AssertIndependentMap(genericIntLeft - floatRight, intLeft - floatRight);
        AssertIndependentMap(intLeft - genericFloatRight, intLeft - floatRight);
        AssertIndependentMap(genericIntLeft + intRight, intLeft + intRight);
        AssertIndependentMap(intLeft + genericIntRight, intLeft + intRight);
        AssertIndependentMap(genericIntLeft - intRight, intLeft - intRight);
        AssertIndependentMap(intLeft - genericIntRight, intLeft - intRight);
        AssertIndependentMap(genericIntLeft * intRight, intLeft * intRight);
        AssertIndependentMap(intLeft * genericIntRight, intLeft * intRight);
        AssertIndependentMap(genericIntLeft / intRight, intLeft / intRight);
        AssertIndependentMap(intLeft / genericIntRight, intLeft / intRight);
        AssertIndependentMap(genericIntLeft % intRight, intLeft % intRight);
        AssertIndependentMap(intLeft % genericIntRight, intLeft % intRight);
        AssertIndependentMap(genericIntLeft < intRight, intLeft < intRight);
        AssertIndependentMap(intLeft < genericIntRight, intLeft < intRight);
        AssertIndependentMap(genericIntLeft > intRight, intLeft > intRight);
        AssertIndependentMap(intLeft > genericIntRight, intLeft > intRight);
        AssertIndependentMap(genericIntLeft <= intRight, intLeft <= intRight);
        AssertIndependentMap(intLeft <= genericIntRight, intLeft <= intRight);
        AssertIndependentMap(genericIntLeft >= intRight, intLeft >= intRight);
        AssertIndependentMap(intLeft >= genericIntRight, intLeft >= intRight);
        AssertIndependentMap(genericIntLeft == intRight, intLeft == intRight);
        AssertIndependentMap(intLeft == genericIntRight, intLeft == intRight);
        AssertIndependentMap(genericIntLeft != intRight, intLeft != intRight);
        AssertIndependentMap(intLeft != genericIntRight, intLeft != intRight);
        AssertIndependentMap(genericBoolLeft & boolRight, boolLeft & boolRight);
        AssertIndependentMap(boolLeft & genericBoolRight, boolLeft & boolRight);
        AssertIndependentMap(genericBoolLeft | boolRight, boolLeft | boolRight);
        AssertIndependentMap(boolLeft | genericBoolRight, boolLeft | boolRight);
        AssertIndependentMap(genericBoolLeft ^ boolRight, boolLeft ^ boolRight);
        AssertIndependentMap(boolLeft ^ genericBoolRight, boolLeft ^ boolRight);
        AssertIndependentMap(genericBoolLeft == boolRight, boolLeft == boolRight);
        AssertIndependentMap(boolLeft == genericBoolRight, boolLeft == boolRight);
        AssertIndependentMap(genericBoolLeft != boolRight, boolLeft != boolRight);
        AssertIndependentMap(boolLeft != genericBoolRight, boolLeft != boolRight);

        foreach (object map in new object[]
        {
            floatLeft, floatRight, genericFloatLeft, genericFloatRight,
            intLeft, intRight, genericIntLeft, genericIntRight,
            boolLeft, boolRight, genericBoolLeft, genericBoolRight
        })
        {
            bool isRight = ReferenceEquals(map, floatRight) || ReferenceEquals(map, genericFloatRight) ||
                ReferenceEquals(map, intRight) || ReferenceEquals(map, genericIntRight) ||
                ReferenceEquals(map, boolRight) || ReferenceEquals(map, genericBoolRight);
            Assert.That(BoxedValues(map), Is.EqualTo(BoxedValues(Argument(map.GetType(), isRight ? 1 : 0, topology))),
                $"Source {map.GetType()}, right operand: {isRight}.");
        }
    }

    [Test]
    public void OperatorSurface_AcceptsGenericMapOnEitherSideOfEverySpecializedMapPair()
    {
        MethodInfo[] ordinary = GetOperators().Where(method =>
            method.GetParameters().All(parameter => SpecializedTypes.Contains(parameter.ParameterType))).ToArray();
        MethodInfo[] mixed = GetMixedOperators();
        Assert.That(ordinary, Has.Length.EqualTo(49));
        Assert.That(mixed, Has.Length.EqualTo(98));

        foreach (MethodInfo original in ordinary)
        {
            Type[] parameters = original.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            for (int genericIndex = 0; genericIndex < parameters.Length; genericIndex++)
            {
                Type[] expected = (Type[])parameters.Clone();
                expected[genericIndex] = GenericTypes[Array.IndexOf(SpecializedTypes, expected[genericIndex])];
                MethodInfo[] matches = mixed.Where(method => method.Name == original.Name &&
                    method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(expected)).ToArray();
                Assert.That(matches, Has.Length.EqualTo(1), Signature(original));
                Assert.That(matches[0].ReturnType, Is.EqualTo(original.ReturnType), Signature(matches[0]));
            }
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MixedOperators_ValidateTopologyAndNullOperands(bool changeLayout)
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var differentTopology = changeLayout
            ? new HexMapTopology(2, 2, Layout.EvenR)
            : new HexMapTopology(4, 1, Layout.OddR);

        foreach (MethodInfo method in GetMixedOperators())
        {
            ParameterInfo[] parameters = method.GetParameters();
            var mismatch = new object?[]
            {
                Argument(parameters[0].ParameterType, 0, topology),
                Argument(parameters[1].ParameterType, 1, differentTopology)
            };
            var topologyError = (ArgumentException)InvokeForError(method, mismatch, typeof(ArgumentException));
            Assert.That(topologyError.ParamName, Is.EqualTo("right"), Signature(method));

            for (int nullIndex = 0; nullIndex < 2; nullIndex++)
            {
                var arguments = new object?[]
                {
                    Argument(parameters[0].ParameterType, 0, topology),
                    Argument(parameters[1].ParameterType, 1, topology)
                };
                arguments[nullIndex] = null;
                var nullError = (ArgumentNullException)InvokeForError(method, arguments, typeof(ArgumentNullException));
                Assert.That(nullError.ParamName, Is.EqualTo(parameters[nullIndex].Name), Signature(method));
            }
        }
    }

    [Test]
    public void IntegerArithmetic_PreservesOverflowAndDivisionErrors()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var maximum = new IntHexMap(topology, new[] { int.MaxValue });
        var minimum = new IntHexMap(topology, new[] { int.MinValue });
        var one = new IntHexMap(topology, new[] { 1 });
        var negativeOne = new IntHexMap(topology, new[] { -1 });
        var genericMaximum = new HexMap<int>(topology, new[] { int.MaxValue });
        var genericMinimum = new HexMap<int>(topology, new[] { int.MinValue });
        var genericOne = new HexMap<int>(topology, new[] { 1 });
        var genericNegativeOne = new HexMap<int>(topology, new[] { -1 });
        var genericZero = new HexMap<int>(topology, new[] { 0 });

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = maximum + genericOne);
            Assert.Throws<OverflowException>(() => _ = genericMaximum + one);
            Assert.Throws<OverflowException>(() => _ = minimum - genericOne);
            Assert.Throws<OverflowException>(() => _ = genericMinimum - one);
            Assert.Throws<OverflowException>(() => _ = maximum * genericMaximum);
            Assert.Throws<OverflowException>(() => _ = genericMaximum * maximum);
            Assert.Throws<OverflowException>(() => _ = minimum / genericNegativeOne);
            Assert.Throws<OverflowException>(() => _ = genericMinimum / negativeOne);
            Assert.Throws<OverflowException>(() => _ = minimum % genericNegativeOne);
            Assert.Throws<OverflowException>(() => _ = genericMinimum % negativeOne);
            Assert.Throws<DivideByZeroException>(() => _ = maximum / genericZero);
            Assert.Throws<DivideByZeroException>(() => _ = maximum % genericZero);
            Assert.Throws<DivideByZeroException>(() => _ = genericOne / new IntHexMap(topology));
            Assert.Throws<DivideByZeroException>(() => _ = genericOne % new IntHexMap(topology));
        });
    }

    [Test]
    public void FloatingPointArithmetic_PreservesNaNInfinityAndMixedPrecision()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var nan = new FloatHexMap(topology, new[] { float.NaN });
        var zero = new FloatHexMap(topology);
        var genericZero = new HexMap<float>(topology);
        var genericNaN = new HexMap<float>(topology, new[] { float.NaN });
        var integerOne = new IntHexMap(topology, new[] { 1 });
        var genericIntegerOne = new HexMap<int>(topology, new[] { 1 });
        var roundedFloat = new FloatHexMap(topology, new[] { 16_777_216f });
        var roundedInteger = new IntHexMap(topology, new[] { 16_777_217 });
        var genericRoundedFloat = new HexMap<float>(topology, new[] { 16_777_216f });
        var genericRoundedInteger = new HexMap<int>(topology, new[] { 16_777_217 });

        Assert.Multiple(() =>
        {
            Assert.That((nan / genericZero)[0], Is.NaN);
            Assert.That((genericNaN % zero)[0], Is.NaN);
            Assert.That((integerOne / genericZero)[0], Is.EqualTo(float.PositiveInfinity));
            Assert.That((genericIntegerOne / zero)[0], Is.EqualTo(float.PositiveInfinity));
            Assert.That((integerOne == genericNaN)[0], Is.False);
            Assert.That((genericIntegerOne != nan)[0], Is.True);
            Assert.That((roundedFloat == genericRoundedInteger)[0], Is.True);
            Assert.That((roundedInteger == genericRoundedFloat)[0], Is.True);
        });
    }

    private static MethodInfo[] GetOperators() => SpecializedTypes
        .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        .Where(method => method.IsSpecialName && method.Name.StartsWith("op_", StringComparison.Ordinal))
        .Where(method => method.GetParameters().Length == 2)
        .ToArray();

    private static MethodInfo[] GetMixedOperators() => GetOperators().Where(method =>
        method.GetParameters().Any(parameter => GenericTypes.Contains(parameter.ParameterType))).ToArray();

    private static string Signature(MethodInfo method) =>
        $"{method.DeclaringType!.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType))})";

    private static object Argument(Type type, int index, HexMapTopology topology)
    {
        bool left = index == 0;
        if (type == typeof(FloatHexMap) || type == typeof(HexMap<float>))
        {
            float[] values = topology.Count == 0 ? Array.Empty<float>() :
                left ? new[] { 6f, -4f, float.NaN, float.NegativeInfinity } : new[] { 2f, 2f, 1f, float.PositiveInfinity };
            return type == typeof(FloatHexMap) ? new FloatHexMap(topology, values) : new HexMap<float>(topology, values);
        }
        if (type == typeof(IntHexMap) || type == typeof(HexMap<int>))
        {
            int[] values = topology.Count == 0 ? Array.Empty<int>() :
                left ? new[] { 3, -6, 0, 4 } : new[] { 2, 3, 2, -1 };
            return type == typeof(IntHexMap) ? new IntHexMap(topology, values) : new HexMap<int>(topology, values);
        }
        if (type == typeof(BoolHexMap) || type == typeof(HexMap<bool>))
        {
            bool[] values = topology.Count == 0 ? Array.Empty<bool>() :
                left ? new[] { true, true, false, false } : new[] { true, false, true, false };
            return type == typeof(BoolHexMap) ? new BoolHexMap(topology, values) : new HexMap<bool>(topology, values);
        }

        throw new InvalidOperationException($"Unexpected operator argument {type}.");
    }

    private static T[] Values<T>(IHexMap<T> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private static object[] BoxedValues(object map) => map switch
    {
        IHexMap<float> values => Values(values).Cast<object>().ToArray(),
        IHexMap<int> values => Values(values).Cast<object>().ToArray(),
        IHexMap<bool> values => Values(values).Cast<object>().ToArray(),
        _ => throw new InvalidOperationException($"Unexpected map {map.GetType()}.")
    };

    private static void AssertIndependentMap<T>(HexMap<T> actual, HexMap<T> expected)
    {
        Assert.That(actual.GetType(), Is.EqualTo(expected.GetType()));
        Assert.That(actual.Topology, Is.EqualTo(expected.Topology));
        T[] expectedValues = Values(expected);
        Assert.That(Values(actual), Is.EqualTo(expectedValues));
        if (actual.Topology.Count > 0)
        {
            switch (actual)
            {
                case FloatHexMap map:
                    map[0] = 123.25f;
                    break;
                case IntHexMap map:
                    map[0] = 123;
                    break;
                case BoolHexMap map:
                    map[0] = !map[0];
                    break;
            }
        }
        Assert.That(Values(expected), Is.EqualTo(expectedValues));
    }

    private static Exception InvokeForError(MethodInfo method, object?[] arguments, Type expectedType)
    {
        try
        {
            method.Invoke(null, arguments);
        }
        catch (TargetInvocationException wrapper) when (wrapper.InnerException?.GetType() == expectedType)
        {
            return wrapper.InnerException;
        }

        throw new AssertionException($"{Signature(method)} did not throw {expectedType.Name}.");
    }
}
