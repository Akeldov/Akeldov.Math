# Use Specialized Hex Maps

Use Boolean, integer, and floating-point specializations when a map needs cell-wise operations.
Choose a spatial specialization when results must retain world-space placement.

## Create the Maps

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

As with `HexMap<TValue>`, each constructor retains its supplied array. Clone the array first when
the source and map must not share mutable storage.

## Transform Numeric Values

```csharp
IntHexMap doubledCost = movementCost * 2;
IntHexMap pairwiseCost = movementCost * movementCost;
FloatHexMap weightedElevation = (elevation + movementCost) / 2f;

IntHexMap boundedCost = movementCost.Clamp(2, 4);
FloatHexMap normalizedElevation = elevation.Rescale(0f, 1f);
```

Operators and range methods create new maps. Map operands require equal topologies. A mixed
integer/floating-point operation returns a floating-point map. `Rescale` maps the current extrema
to the requested range; a constant map is filled with the requested minimum.

## Build Masks with Comparisons

```csharp
BoolHexMap high = elevation >= 0.5f;
BoolHexMap affordable = 3 >= movementCost;
BoolHexMap higherThanCost = elevation > movementCost;
BoolHexMap usable = !blocked & affordable;
```

`<`, `>`, `<=`, and `>=` compare cells and return a Boolean map. Scalar comparisons work in
either operand order. `==` and `!=` are not cell-wise operators.

## Process Connected Regions

```csharp
BoolHexMap expanded = usable.Dilate();
BoolHexMap cleaned = usable.Open();
BoolHexMap boundary = usable.Outline();

BoolHexMap selected = usable.FloodFill(new VectorXYInt(0, 0));
(IntHexMap labels, int componentCount) = usable.ConnectedComponents();
IntHexMap distance = usable.DistanceTransform(targetValue: true);
```

Morphology and connectivity use six-neighbor hex adjacency. `FloodFill` follows the Boolean value
at the seed, component labels are deterministic, and an unreachable distance is `int.MaxValue`.

## Preserve Geometry

```csharp
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 8f);

SpatialFloatHexMap spatialElevation = elevation.ToSpatialHexMap(geometry);
SpatialFloatHexMap spatialWeighted = spatialElevation + movementCost;
SpatialBoolHexMap spatialHigh = spatialWeighted > 2f;

FloatHexMap detachedCopy = spatialWeighted.ToHexMap();
```

A spatial/topology-only pair requires equal topologies and returns a spatial map with the spatial
operand's geometry. Two spatial operands require equal geometry. Conversion methods copy values
into independent storage.

Next, [combine masks and select values](combine-and-select-hex-map-values.md).
