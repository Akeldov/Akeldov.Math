using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Reflection;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class CrossSpatialHexMapOperatorTests
{
    private static readonly Type[] OrdinaryTypes =
        { typeof(BoolHexMap), typeof(FloatHexMap), typeof(IntHexMap) };

    private static readonly Type[] SpatialTypes =
        { typeof(SpatialBoolHexMap), typeof(SpatialFloatHexMap), typeof(SpatialIntHexMap) };

    [Test]
    public void OperatorSurface_SpatializesEitherOperandOfEveryOrdinaryMapPair()
    {
        MethodInfo[] ordinary = GetOrdinaryMapPairOperators();
        MethodInfo[] cross = GetCrossOperators();
        ILookup<string, MethodInfo> crossBySignature = cross.ToLookup(SemanticSignature);

        Assert.Multiple(() =>
        {
            Assert.That(ordinary, Has.Length.EqualTo(39));
            Assert.That(cross, Has.Length.EqualTo(78));
            Assert.That(
                cross.Count(method => method.DeclaringType == typeof(SpatialBoolHexMap)),
                Is.EqualTo(6));
            Assert.That(
                cross.Count(method => method.DeclaringType == typeof(SpatialFloatHexMap)),
                Is.EqualTo(36));
            Assert.That(
                cross.Count(method => method.DeclaringType == typeof(SpatialIntHexMap)),
                Is.EqualTo(36));
            Assert.That(cross.Any(method => method.Name is "op_Equality" or "op_Inequality"), Is.False);
        });

        foreach (MethodInfo ordinaryOperator in ordinary)
        {
            MethodInfo[] counterparts = crossBySignature[SemanticSignature(ordinaryOperator)].ToArray();

            Assert.That(counterparts, Has.Length.EqualTo(2), SemanticSignature(ordinaryOperator));
            Assert.That(
                counterparts.Select(method => Array.FindIndex(
                    method.GetParameters(),
                    parameter => SpatialTypes.Contains(parameter.ParameterType))).OrderBy(index => index),
                Is.EqualTo(new[] { 0, 1 }),
                SemanticSignature(ordinaryOperator));
        }

        foreach (MethodInfo crossOperator in cross)
        {
            ParameterInfo[] parameters = crossOperator.GetParameters();
            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Length.EqualTo(2), DisplaySignature(crossOperator));
                Assert.That(
                    parameters.Count(parameter => OrdinaryTypes.Contains(parameter.ParameterType)),
                    Is.EqualTo(1),
                    DisplaySignature(crossOperator));
                Assert.That(
                    parameters.Count(parameter => SpatialTypes.Contains(parameter.ParameterType)),
                    Is.EqualTo(1),
                    DisplaySignature(crossOperator));
                Assert.That(SpatialTypes.Contains(crossOperator.ReturnType), Is.True, DisplaySignature(crossOperator));
            });
        }
    }

    [Test]
    public void EveryCrossOperator_MatchesOrdinaryCellwiseSemanticsAndRetainsSpatialGeometry()
    {
        HexMapGeometry geometry = Geometry();
        Dictionary<string, MethodInfo> ordinary = GetOrdinaryMapPairOperators()
            .ToDictionary(SemanticSignature);

        foreach (MethodInfo crossOperator in GetCrossOperators())
        {
            string signature = SemanticSignature(crossOperator);
            MethodInfo ordinaryOperator = ordinary[signature];
            object?[] crossArguments = Arguments(crossOperator, geometry);
            object[][] originalInputValues = crossArguments.Select(argument => BoxedValues(argument!)).ToArray();
            object ordinaryResult = ordinaryOperator.Invoke(null, Arguments(ordinaryOperator, geometry))!;
            object crossResult = crossOperator.Invoke(null, crossArguments)!;

            Assert.Multiple(() =>
            {
                Assert.That(BoxedValues(crossResult), Is.EqualTo(BoxedValues(ordinaryResult)), signature);
                Assert.That(ResultGeometry(crossResult), Is.EqualTo(geometry), signature);
                Assert.That(
                    Normalize(crossResult.GetType()),
                    Is.EqualTo(ordinaryOperator.ReturnType),
                    signature);
                for (int index = 0; index < crossArguments.Length; index++)
                    Assert.That(BoxedValues(crossArguments[index]!), Is.EqualTo(originalInputValues[index]), signature);
            });

            MutateFirstValue(crossResult);
            for (int index = 0; index < crossArguments.Length; index++)
                Assert.That(BoxedValues(crossArguments[index]!), Is.EqualTo(originalInputValues[index]), signature);
        }
    }

    [Test]
    public void EveryCrossOperator_ValidatesTopologyAndNullOperands()
    {
        HexMapGeometry geometry = Geometry();
        var differentGeometry = new HexMapGeometry(
            new HexMapTopology(2, 1, Layout.EvenR),
            new VectorXY(-100f, 200f),
            7f);

        foreach (MethodInfo crossOperator in GetCrossOperators())
        {
            string signature = DisplaySignature(crossOperator);
            ParameterInfo[] parameters = crossOperator.GetParameters();
            object?[] mismatchedArguments = Arguments(crossOperator, geometry);
            mismatchedArguments[1] = Argument(parameters[1].ParameterType, 1, differentGeometry);
            var topologyError = (ArgumentException)InvokeForError(
                crossOperator,
                mismatchedArguments,
                typeof(ArgumentException));

            Assert.That(topologyError.ParamName, Is.EqualTo("right"), signature);

            for (int index = 0; index < parameters.Length; index++)
            {
                object?[] nullArguments = Arguments(crossOperator, geometry);
                nullArguments[index] = null;
                var nullError = (ArgumentNullException)InvokeForError(
                    crossOperator,
                    nullArguments,
                    typeof(ArgumentNullException));

                Assert.That(nullError.ParamName, Is.EqualTo(parameters[index].Name), signature);
            }
        }
    }

    [Test]
    public void NumericCrossOperators_PreserveCheckedIntAndIeeeFloatSemantics()
    {
        HexMapGeometry geometry = new(1, 1, new VectorXY(3f, -2f), 2f, Layout.OddR);
        var spatialMaximum = new SpatialIntHexMap(geometry, new[] { int.MaxValue });
        var spatialMinimum = new SpatialIntHexMap(geometry, new[] { int.MinValue });
        var spatialNegativeOne = new SpatialIntHexMap(geometry, new[] { -1 });
        var spatialZero = new SpatialIntHexMap(geometry, new[] { 0 });
        var ordinaryMaximum = new IntHexMap(geometry.Topology, new[] { int.MaxValue });
        var ordinaryMinimum = new IntHexMap(geometry.Topology, new[] { int.MinValue });
        var ordinaryNegativeOne = new IntHexMap(geometry.Topology, new[] { -1 });
        var ordinaryOne = new IntHexMap(geometry.Topology, new[] { 1 });
        var ordinaryZero = new IntHexMap(geometry.Topology, new[] { 0 });

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = spatialMaximum + ordinaryOne);
            Assert.Throws<OverflowException>(() => _ = ordinaryOne + spatialMaximum);
            Assert.Throws<OverflowException>(() => _ = spatialMinimum - ordinaryOne);
            Assert.Throws<OverflowException>(() => _ = ordinaryMinimum - spatialMaximum);
            Assert.Throws<OverflowException>(() => _ = spatialMaximum * ordinaryMaximum);
            Assert.Throws<OverflowException>(() => _ = ordinaryMaximum * spatialMaximum);
            Assert.Throws<OverflowException>(() => _ = spatialMinimum / ordinaryNegativeOne);
            Assert.Throws<OverflowException>(() => _ = ordinaryMinimum / spatialNegativeOne);
            Assert.Throws<OverflowException>(() => _ = spatialMinimum % ordinaryNegativeOne);
            Assert.Throws<OverflowException>(() => _ = ordinaryMinimum % spatialNegativeOne);
            Assert.Throws<DivideByZeroException>(() => _ = spatialMaximum / ordinaryZero);
            Assert.Throws<DivideByZeroException>(() => _ = ordinaryOne % spatialZero);
        });

        var spatialNaN = new SpatialFloatHexMap(geometry, new[] { float.NaN });
        var spatialFloatZero = new SpatialFloatHexMap(geometry, new[] { 0f });
        var ordinaryFloatZero = new FloatHexMap(geometry.Topology, new[] { 0f });

        Assert.Multiple(() =>
        {
            Assert.That((spatialNaN / ordinaryFloatZero)[0], Is.NaN);
            Assert.That((ordinaryFloatZero / spatialFloatZero)[0], Is.NaN);
            Assert.That((ordinaryOne / spatialFloatZero)[0], Is.EqualTo(float.PositiveInfinity));
            Assert.That((spatialZero / ordinaryFloatZero)[0], Is.NaN);
            Assert.That((spatialNaN % ordinaryOne)[0], Is.NaN);
            Assert.That((ordinaryOne < spatialNaN)[0], Is.False);
            Assert.That((spatialNaN >= ordinaryOne)[0], Is.False);
        });
    }

    private static MethodInfo[] GetOrdinaryMapPairOperators() => GetOperators(OrdinaryTypes)
        .Where(method => method.GetParameters().Length == 2)
        .Where(method => method.GetParameters().All(parameter => OrdinaryTypes.Contains(parameter.ParameterType)))
        .ToArray();

    private static MethodInfo[] GetCrossOperators() => GetOperators(SpatialTypes)
        .Where(method =>
        {
            Type[] operands = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
            return operands.Any(OrdinaryTypes.Contains) && operands.Any(SpatialTypes.Contains);
        })
        .ToArray();

    private static MethodInfo[] GetOperators(Type[] types) => types
        .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        .Where(method => method.IsSpecialName && method.Name.StartsWith("op_", StringComparison.Ordinal))
        .ToArray();

    private static string SemanticSignature(MethodInfo method) =>
        $"{method.Name}" +
        $"({string.Join(",", method.GetParameters().Select(parameter => Normalize(parameter.ParameterType).Name))})" +
        $"->{Normalize(method.ReturnType).Name}";

    private static string DisplaySignature(MethodInfo method) =>
        $"{method.DeclaringType!.Name}.{method.Name}" +
        $"({string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.Name))})" +
        $"->{method.ReturnType.Name}";

    private static Type Normalize(Type type) => type == typeof(SpatialBoolHexMap) ? typeof(BoolHexMap) :
        type == typeof(SpatialFloatHexMap) ? typeof(FloatHexMap) :
        type == typeof(SpatialIntHexMap) ? typeof(IntHexMap) : type;

    private static HexMapGeometry Geometry() =>
        new(2, 1, new VectorXY(10f, -20f), 2f, Layout.OddR);

    private static object?[] Arguments(MethodInfo method, HexMapGeometry geometry) => method.GetParameters()
        .Select((parameter, index) => Argument(parameter.ParameterType, index, geometry))
        .ToArray();

    private static object Argument(Type type, int index, HexMapGeometry geometry)
    {
        bool left = index == 0;
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

    private static object[] BoxedValues(object map) => map switch
    {
        IHexMap<bool> values => Values(values).Cast<object>().ToArray(),
        IHexMap<float> values => Values(values).Cast<object>().ToArray(),
        IHexMap<int> values => Values(values).Cast<object>().ToArray(),
        _ => throw new InvalidOperationException($"Unexpected operator result {map.GetType()}.")
    };

    private static T[] Values<T>(IHexMap<T> map) =>
        Enumerable.Range(0, map.Topology.Count).Select(index => map[index]).ToArray();

    private static HexMapGeometry ResultGeometry(object result) => result switch
    {
        SpatialBoolHexMap map => map.Geometry,
        SpatialFloatHexMap map => map.Geometry,
        SpatialIntHexMap map => map.Geometry,
        _ => throw new InvalidOperationException($"Unexpected operator result {result.GetType()}.")
    };

    private static void MutateFirstValue(object map)
    {
        switch (map)
        {
            case SpatialBoolHexMap values:
                values[0] = !values[0];
                break;
            case SpatialFloatHexMap values:
                values[0] += 123.25f;
                break;
            case SpatialIntHexMap values:
                values[0] = values[0] == int.MaxValue ? int.MinValue : values[0] + 1;
                break;
            default:
                throw new InvalidOperationException($"Unexpected operator result {map.GetType()}.");
        }
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

        throw new AssertionException($"{DisplaySignature(method)} did not throw {expectedType.Name}.");
    }
}
