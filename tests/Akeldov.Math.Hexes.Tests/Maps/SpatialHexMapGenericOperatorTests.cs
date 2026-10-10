using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Reflection;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialHexMapGenericOperatorTests
{
    private static readonly Type[] SpecializedTypes = { typeof(SpatialFloatHexMap), typeof(SpatialIntHexMap), typeof(SpatialBoolHexMap) };
    private static readonly Type[] GenericTypes = { typeof(HexMap<float>), typeof(HexMap<int>), typeof(HexMap<bool>) };

    [TestCase(2, 2, Layout.OddR)]
    [TestCase(2, 2, Layout.EvenR)]
    [TestCase(2, 2, Layout.OddQ)]
    [TestCase(2, 2, Layout.EvenQ)]
    [TestCase(0, 0, Layout.OddR)]
    public void MixedOperators_MatchSpecializedOperatorsWithoutChangingSources(int width, int height, Layout layout)
    {
        var topology = new HexMapTopology(width, height, layout);
        var geometry = new HexMapGeometry(topology, new VectorXY(10f, -20f), 2f);
        var floatLeft = (SpatialFloatHexMap)Argument(typeof(SpatialFloatHexMap), 0, geometry);
        var floatRight = (SpatialFloatHexMap)Argument(typeof(SpatialFloatHexMap), 1, geometry);
        var genericFloatLeft = (HexMap<float>)Argument(typeof(HexMap<float>), 0, geometry);
        var genericFloatRight = (HexMap<float>)Argument(typeof(HexMap<float>), 1, geometry);
        var intLeft = (SpatialIntHexMap)Argument(typeof(SpatialIntHexMap), 0, geometry);
        var intRight = (SpatialIntHexMap)Argument(typeof(SpatialIntHexMap), 1, geometry);
        var genericIntLeft = (HexMap<int>)Argument(typeof(HexMap<int>), 0, geometry);
        var genericIntRight = (HexMap<int>)Argument(typeof(HexMap<int>), 1, geometry);
        var boolLeft = (SpatialBoolHexMap)Argument(typeof(SpatialBoolHexMap), 0, geometry);
        var boolRight = (SpatialBoolHexMap)Argument(typeof(SpatialBoolHexMap), 1, geometry);
        var genericBoolLeft = (HexMap<bool>)Argument(typeof(HexMap<bool>), 0, geometry);
        var genericBoolRight = (HexMap<bool>)Argument(typeof(HexMap<bool>), 1, geometry);

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
            Assert.That(BoxedValues(map), Is.EqualTo(BoxedValues(Argument(map.GetType(), isRight ? 1 : 0, geometry))),
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
        var geometry = new HexMapGeometry(topology, new VectorXY(10f, -20f), 2f);
        var differentTopology = changeLayout
            ? new HexMapTopology(2, 2, Layout.EvenR)
            : new HexMapTopology(4, 1, Layout.OddR);

        var differentGeometry = new HexMapGeometry(differentTopology, new VectorXY(-30f, 40f), 3f);

        foreach (MethodInfo method in GetMixedOperators())
        {
            ParameterInfo[] parameters = method.GetParameters();
            var mismatch = new object?[]
            {
                Argument(parameters[0].ParameterType, 0, geometry),
                Argument(parameters[1].ParameterType, 1, differentGeometry)
            };
            var topologyError = (ArgumentException)InvokeForError(method, mismatch, typeof(ArgumentException));
            Assert.That(topologyError.ParamName, Is.EqualTo("right"), Signature(method));

            for (int nullIndex = 0; nullIndex < 2; nullIndex++)
            {
                var arguments = new object?[]
                {
                    Argument(parameters[0].ParameterType, 0, geometry),
                    Argument(parameters[1].ParameterType, 1, geometry)
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
        var geometry = new HexMapGeometry(topology, new VectorXY(10f, -20f), 2f);
        var maximum = new SpatialIntHexMap(geometry, new[] { int.MaxValue });
        var minimum = new SpatialIntHexMap(geometry, new[] { int.MinValue });
        var one = new SpatialIntHexMap(geometry, new[] { 1 });
        var negativeOne = new SpatialIntHexMap(geometry, new[] { -1 });
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
            Assert.Throws<DivideByZeroException>(() => _ = genericOne / new SpatialIntHexMap(geometry));
            Assert.Throws<DivideByZeroException>(() => _ = genericOne % new SpatialIntHexMap(geometry));
        });
    }

    [Test]
    public void FloatingPointArithmetic_PreservesNaNInfinityAndMixedPrecision()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var geometry = new HexMapGeometry(topology, new VectorXY(10f, -20f), 2f);
        var nan = new SpatialFloatHexMap(geometry, new[] { float.NaN });
        var zero = new SpatialFloatHexMap(geometry);
        var genericZero = new HexMap<float>(topology);
        var genericNaN = new HexMap<float>(topology, new[] { float.NaN });
        var integerOne = new SpatialIntHexMap(geometry, new[] { 1 });
        var genericIntegerOne = new HexMap<int>(topology, new[] { 1 });
        var roundedFloat = new SpatialFloatHexMap(geometry, new[] { 16_777_216f });
        var roundedInteger = new SpatialIntHexMap(geometry, new[] { 16_777_217 });
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

    private static object Argument(Type type, int index, HexMapGeometry geometry)
    {
        var topology = geometry.Topology;
        bool left = index == 0;
        if (type == typeof(SpatialFloatHexMap) || type == typeof(HexMap<float>))
        {
            float[] values = topology.Count == 0 ? Array.Empty<float>() :
                left ? new[] { 6f, -4f, float.NaN, float.NegativeInfinity } : new[] { 2f, 2f, 1f, float.PositiveInfinity };
            return type == typeof(SpatialFloatHexMap) ? new SpatialFloatHexMap(geometry, values) : new HexMap<float>(topology, values);
        }
        if (type == typeof(SpatialIntHexMap) || type == typeof(HexMap<int>))
        {
            int[] values = topology.Count == 0 ? Array.Empty<int>() :
                left ? new[] { 3, -6, 0, 4 } : new[] { 2, 3, 2, -1 };
            return type == typeof(SpatialIntHexMap) ? new SpatialIntHexMap(geometry, values) : new HexMap<int>(topology, values);
        }
        if (type == typeof(SpatialBoolHexMap) || type == typeof(HexMap<bool>))
        {
            bool[] values = topology.Count == 0 ? Array.Empty<bool>() :
                left ? new[] { true, true, false, false } : new[] { true, false, true, false };
            return type == typeof(SpatialBoolHexMap) ? new SpatialBoolHexMap(geometry, values) : new HexMap<bool>(topology, values);
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
        Assert.That(((ISpatialHexMap<T>)actual).Geometry, Is.EqualTo(((ISpatialHexMap<T>)expected).Geometry));
        T[] expectedValues = Values(expected);
        Assert.That(Values(actual), Is.EqualTo(expectedValues));
        if (actual.Topology.Count > 0)
        {
            switch (actual)
            {
                case SpatialFloatHexMap map:
                    map[0] = 123.25f;
                    break;
                case SpatialIntHexMap map:
                    map[0] = 123;
                    break;
                case SpatialBoolHexMap map:
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

