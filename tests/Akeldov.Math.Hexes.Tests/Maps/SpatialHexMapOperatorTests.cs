using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Reflection;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialHexMapOperatorTests
{
    private static readonly Type[] OrdinaryTypes =
        { typeof(BoolHexMap), typeof(FloatHexMap), typeof(IntHexMap) };

    private static readonly Type[] SpatialTypes =
        { typeof(SpatialBoolHexMap), typeof(SpatialFloatHexMap), typeof(SpatialIntHexMap) };

    [Test]
    public void ConcreteMaps_InheritSpatialHexMapAndRetainBackingArrays()
    {
        HexMapGeometry geometry = Geometry();
        bool[] boolValues = { false, true };
        float[] floatValues = { -2f, 4f };
        int[] intValues = { -3, 5 };
        SpatialHexMap<bool> boolMap = new SpatialBoolHexMap(geometry, boolValues);
        SpatialHexMap<float> floatMap = new SpatialFloatHexMap(geometry, floatValues);
        SpatialHexMap<int> intMap = new SpatialIntHexMap(geometry, intValues);

        boolValues[0] = true;
        floatMap[1] = 7f;
        intMap[0] = -8;

        Assert.Multiple(() =>
        {
            Assert.That(boolMap.Geometry, Is.EqualTo(geometry));
            Assert.That(boolMap[0], Is.True);
            Assert.That(floatValues[1], Is.EqualTo(7f));
            Assert.That(intValues[0], Is.EqualTo(-8));
            Assert.That(((IFloatHexMap)floatMap).Min, Is.EqualTo(-2f));
            Assert.That(((IFloatHexMap)floatMap).Max, Is.EqualTo(7f));
            Assert.That(((IIntHexMap)intMap).Min, Is.EqualTo(-8));
            Assert.That(((IIntHexMap)intMap).Max, Is.EqualTo(5));
        });
    }

    [Test]
    public void NativeOperatorSurface_ExactlyMatchesOrdinaryMaps()
    {
        MethodInfo[] ordinary = GetOperators(OrdinaryTypes);
        MethodInfo[] spatial = GetNativeSpatialOperators();

        Assert.Multiple(() =>
        {
            Assert.That(ordinary, Has.Length.EqualTo(62));
            Assert.That(spatial, Has.Length.EqualTo(62));
            Assert.That(
                spatial.Select(Signature).OrderBy(value => value),
                Is.EqualTo(ordinary.Select(Signature).OrderBy(value => value)));
            Assert.That(spatial.Any(method => method.Name is "op_Equality" or "op_Inequality"), Is.False);
        });
    }

    [Test]
    public void EveryOperator_MatchesOrdinaryCellwiseSemantics()
    {
        HexMapGeometry geometry = Geometry();
        MethodInfo[] ordinary = GetOperators(OrdinaryTypes);
        Dictionary<string, MethodInfo> spatial = GetNativeSpatialOperators().ToDictionary(Signature);

        foreach (MethodInfo ordinaryOperator in ordinary)
        {
            string signature = Signature(ordinaryOperator);
            MethodInfo spatialOperator = spatial[signature];
            object ordinaryResult = ordinaryOperator.Invoke(null, Arguments(ordinaryOperator, geometry))!;
            object?[] spatialArguments = Arguments(spatialOperator, geometry);
            object[] spatialInputs = spatialArguments
                .Where(argument => argument != null && SpatialTypes.Contains(argument.GetType()))
                .Cast<object>()
                .ToArray();
            object[][] inputValues = spatialInputs.Select(BoxedValues).ToArray();
            object spatialResult = spatialOperator.Invoke(null, spatialArguments)!;

            Assert.Multiple(() =>
            {
                Assert.That(BoxedValues(spatialResult), Is.EqualTo(BoxedValues(ordinaryResult)), signature);
                Assert.That(ResultGeometry(spatialResult), Is.EqualTo(geometry), signature);
                for (int index = 0; index < spatialInputs.Length; index++)
                    Assert.That(BoxedValues(spatialInputs[index]), Is.EqualTo(inputValues[index]), signature);
            });

            MutateFirstValue(spatialResult);
            for (int index = 0; index < spatialInputs.Length; index++)
                Assert.That(BoxedValues(spatialInputs[index]), Is.EqualTo(inputValues[index]), signature);
        }
    }

    [Test]
    public void Operators_ReturnSpatialResultsWithSourceGeometry()
    {
        HexMapGeometry geometry = Geometry();
        var boolLeft = new SpatialBoolHexMap(geometry, new[] { true, false });
        var boolRight = new SpatialBoolHexMap(geometry, new[] { false, false });
        var floatLeft = new SpatialFloatHexMap(geometry, new[] { 6f, -4f });
        var floatRight = new SpatialFloatHexMap(geometry, new[] { 2f, 2f });
        var intLeft = new SpatialIntHexMap(geometry, new[] { 3, -6 });
        var intRight = new SpatialIntHexMap(geometry, new[] { 2, 3 });

        object[] results =
        {
            !boolLeft, boolLeft & boolRight, boolLeft | boolRight, boolLeft ^ boolRight,
            -floatLeft, floatLeft + floatRight, floatLeft - floatRight,
            floatLeft * floatRight, floatLeft / floatRight, floatLeft % floatRight,
            floatLeft + 2f, 2f - floatLeft, floatLeft * 2f, 2f / floatLeft, floatLeft % 2f,
            floatLeft * intRight, intLeft * floatRight,
            floatLeft / intRight, intLeft / floatRight,
            floatLeft % intRight, intLeft % floatRight,
            -intLeft, intLeft + intRight, intLeft - intRight,
            intLeft * intRight, intLeft / intRight, intLeft % intRight,
            intLeft + 2, 2 - intLeft, intLeft * 2, 2 / intLeft, intLeft % 2,
            floatLeft < floatRight, floatLeft <= intRight,
            intLeft > floatRight, intLeft >= intRight,
        };

        foreach (object result in results)
            Assert.That(ResultGeometry(result), Is.EqualTo(geometry), result.GetType().Name);

        Assert.Multiple(() =>
        {
            Assert.That(Values(!boolLeft), Is.EqualTo(new[] { false, true }));
            Assert.That(Values(floatLeft * intRight), Is.EqualTo(new[] { 12f, -12f }));
            Assert.That(Values(intLeft / intRight), Is.EqualTo(new[] { 1, -2 }));
            Assert.That(Values(floatLeft <= intRight), Is.EqualTo(new[] { false, true }));
            Assert.That(results.All(result => SpatialTypes.Contains(result.GetType())), Is.True);
        });
    }

    [Test]
    public void Operators_ValidateGeometryAndNullMapOperands()
    {
        HexMapGeometry geometry = Geometry();
        var equivalent = new HexMapGeometry(
            new HexMapTopology(2, 1, Layout.OddR),
            new VectorXY(10f, -20f),
            2f);
        HexMapGeometry[] incompatible =
        {
            new(geometry.Topology, new VectorXY(geometry.Origin.X + 1f, geometry.Origin.Y), geometry.Radius),
            new(geometry.Topology, geometry.Origin, geometry.Radius + 1f),
            new(new HexMapTopology(2, 1, Layout.EvenR), geometry.Origin, geometry.Radius),
            new(new HexMapTopology(1, 2, Layout.OddR), geometry.Origin, geometry.Radius),
        };
        MethodInfo[] operators = GetNativeSpatialOperators();
        MethodInfo[] binaryMapOperators = operators.Where(method =>
            method.GetParameters().Length == 2 &&
            method.GetParameters().All(parameter => SpatialTypes.Contains(parameter.ParameterType))).ToArray();

        Assert.That(binaryMapOperators, Has.Length.EqualTo(39));
        foreach (MethodInfo method in binaryMapOperators)
        {
            object?[] equalArguments = Arguments(method, geometry);
            equalArguments[1] = Argument(method.GetParameters()[1].ParameterType, 1, equivalent);
            Assert.DoesNotThrow(() => method.Invoke(null, equalArguments), Signature(method));

            foreach (HexMapGeometry differentGeometry in incompatible)
            {
                object?[] arguments = Arguments(method, geometry);
                arguments[1] = Argument(method.GetParameters()[1].ParameterType, 1, differentGeometry);
                var error = (ArgumentException)InvokeForError(method, arguments, typeof(ArgumentException));
                Assert.That(error.ParamName, Is.EqualTo("right"), Signature(method));
            }
        }

        foreach (MethodInfo method in operators)
        {
            ParameterInfo[] parameters = method.GetParameters();
            for (int index = 0; index < parameters.Length; index++)
            {
                if (!SpatialTypes.Contains(parameters[index].ParameterType))
                    continue;

                object?[] arguments = Arguments(method, geometry);
                arguments[index] = null;
                var error = (ArgumentNullException)InvokeForError(method, arguments, typeof(ArgumentNullException));
                Assert.That(error.ParamName, Is.EqualTo(parameters[index].Name), Signature(method));
            }
        }
    }

    [Test]
    public void NumericOperators_PreserveCheckedIntAndIeeeFloatSemantics()
    {
        HexMapGeometry geometry = new(1, 1, new VectorXY(3f, -2f), 2f, Layout.OddR);
        var minimum = new SpatialIntHexMap(geometry, new[] { int.MinValue });
        var maximum = new SpatialIntHexMap(geometry, new[] { int.MaxValue });
        var negativeOne = new SpatialIntHexMap(geometry, new[] { -1 });
        var one = new SpatialIntHexMap(geometry, new[] { 1 });
        var zero = new SpatialIntHexMap(geometry, new[] { 0 });

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = -minimum);
            Assert.Throws<OverflowException>(() => _ = maximum + one);
            Assert.Throws<OverflowException>(() => _ = minimum / negativeOne);
            Assert.Throws<OverflowException>(() => _ = minimum % negativeOne);
            Assert.Throws<DivideByZeroException>(() => _ = one / zero);
            Assert.Throws<DivideByZeroException>(() => _ = one / 0);
            Assert.Throws<DivideByZeroException>(() => _ = 1 % zero);
        });

        var special = new SpatialFloatHexMap(geometry, new[] { float.NaN });
        var floatZero = new SpatialFloatHexMap(geometry, new[] { 0f });
        var intValue = new SpatialIntHexMap(geometry, new[] { 1 });

        Assert.Multiple(() =>
        {
            Assert.That((special / floatZero)[0], Is.NaN);
            Assert.That((floatZero / floatZero)[0], Is.NaN);
            Assert.That((1f / floatZero)[0], Is.EqualTo(float.PositiveInfinity));
            Assert.That((special % floatZero)[0], Is.NaN);
            Assert.That((special < intValue)[0], Is.False);
            Assert.That((special <= intValue)[0], Is.False);
            Assert.That((intValue > special)[0], Is.False);
            Assert.That((intValue >= special)[0], Is.False);
        });
    }

    private static MethodInfo[] GetOperators(Type[] types) => types
        .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        .Where(method => method.IsSpecialName && method.Name.StartsWith("op_", StringComparison.Ordinal))
        .ToArray();

    private static MethodInfo[] GetNativeSpatialOperators() => GetOperators(SpatialTypes)
        .Where(method => method.GetParameters().All(parameter => !OrdinaryTypes.Contains(parameter.ParameterType)))
        .ToArray();

    private static string Signature(MethodInfo method) =>
        $"{Normalize(method.DeclaringType!).Name}.{method.Name}" +
        $"({string.Join(",", method.GetParameters().Select(parameter => Normalize(parameter.ParameterType).Name))})" +
        $"->{Normalize(method.ReturnType).Name}";

    private static Type Normalize(Type type) => type == typeof(SpatialBoolHexMap) ? typeof(BoolHexMap) :
        type == typeof(SpatialFloatHexMap) ? typeof(FloatHexMap) :
        type == typeof(SpatialIntHexMap) ? typeof(IntHexMap) : type;

    private static HexMapGeometry Geometry() =>
        new(2, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);

    private static HexMapGeometry ResultGeometry(object result) => result switch
    {
        SpatialBoolHexMap map => map.Geometry,
        SpatialFloatHexMap map => map.Geometry,
        SpatialIntHexMap map => map.Geometry,
        _ => throw new InvalidOperationException($"Unexpected operator result {result.GetType()}.")
    };

    private static object?[] Arguments(MethodInfo method, HexMapGeometry geometry) => method.GetParameters()
        .Select((parameter, index) => Argument(parameter.ParameterType, index, geometry)).ToArray();

    private static T[] Values<T>(IHexMap<T> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private static object[] BoxedValues(object map) => map switch
    {
        IHexMap<bool> values => Values(values).Cast<object>().ToArray(),
        IHexMap<float> values => Values(values).Cast<object>().ToArray(),
        IHexMap<int> values => Values(values).Cast<object>().ToArray(),
        _ => throw new InvalidOperationException($"Unexpected operator result {map.GetType()}.")
    };

    private static void MutateFirstValue(object map)
    {
        switch (map)
        {
            case HexMap<bool> values:
                values[0] = !values[0];
                break;
            case HexMap<float> values:
                values[0] += 123.25f;
                break;
            case HexMap<int> values:
                values[0] = values[0] == int.MaxValue ? int.MinValue : values[0] + 1;
                break;
            default:
                throw new InvalidOperationException($"Unexpected operator result {map.GetType()}.");
        }
    }

    private static object Argument(Type type, int index, HexMapGeometry geometry)
    {
        bool left = index == 0;
        if (type == typeof(float)) return 2f;
        if (type == typeof(int)) return 2;
        if (type == typeof(BoolHexMap))
            return new BoolHexMap(geometry.Topology, left ? new[] { true, false } : new[] { false, true });
        if (type == typeof(FloatHexMap))
            return new FloatHexMap(geometry.Topology, left ? new[] { 6f, -4f } : new[] { 2f, 2f });
        if (type == typeof(IntHexMap))
            return new IntHexMap(geometry.Topology, left ? new[] { 3, -6 } : new[] { 2, 3 });
        if (type == typeof(SpatialBoolHexMap))
            return new SpatialBoolHexMap(geometry, left ? new[] { true, false } : new[] { false, true });
        if (type == typeof(SpatialFloatHexMap))
            return new SpatialFloatHexMap(geometry, left ? new[] { 6f, -4f } : new[] { 2f, 2f });
        if (type == typeof(SpatialIntHexMap))
            return new SpatialIntHexMap(geometry, left ? new[] { 3, -6 } : new[] { 2, 3 });
        throw new InvalidOperationException($"Unexpected operator argument {type}.");
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
