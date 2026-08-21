# Use Specialized Hex Maps

Use `BoolHexMap`, `IntHexMap`, and `FloatHexMap` when a map stores masks or numeric fields. These
types keep the `HexMap<TValue>` indexing contract and add operations for their value type.

## Create the maps

```csharp
using Akeldov.Math.Hexes;

var topology = new HexMapTopology(3, 2, Layout.OddR);

var blocked = new BoolHexMap(topology, new[]
{
    false, true,  false,
    false, false, true,
});

var movementCost = new IntHexMap(topology, new[]
{
    1, 4, 2,
    3, 1, 5,
});

var elevation = new FloatHexMap(topology, new[]
{
    0.1f, 0.4f, 0.2f,
    0.7f, 0.5f, 0.9f,
});
```

As with `HexMap<TValue>`, the array is retained as backing storage. Clone it before construction
when later changes to the original array must not affect the map.

## Query and transform numeric maps

```csharp
Console.WriteLine($"Costs: {movementCost.Min}..{movementCost.Max}");
Console.WriteLine($"Elevation: {elevation.Min}..{elevation.Max}");

IntHexMap doubledCost = movementCost * 2;
FloatHexMap shiftedElevation = elevation + 0.25f;
```

Every operator returns a new mutable map. Two-map operations require equal topologies. `IntHexMap`
uses checked arithmetic, so overflow throws `OverflowException`.

## Copy an interface-typed map

```csharp
IHexMap<int> sourceCosts = movementCost;
IntHexMap editableCopy = sourceCosts.ToIntHexMap();

IHexMap<float> sourceElevation = elevation;
FloatHexMap editableFloatCopy = sourceElevation.ToFloatHexMap();
```

The conversion methods copy cell values into independent storage. Subsequent changes to the
source and result do not affect each other.

Next, [combine masks and select values](combine-and-select-hex-map-values.md).
