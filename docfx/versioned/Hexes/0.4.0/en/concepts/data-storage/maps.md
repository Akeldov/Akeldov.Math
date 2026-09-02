# Maps

A hex map associates one value with every cell of a rectangular
<xref:Akeldov.Math.Hexes.HexMapTopology>. The topology fixes the valid X/Y indices, layout, and
cell count; the map supplies the data. Use maps for terrain, masks, costs, labels, or any other
per-cell state.

## Choose a Map Type

| Type | Purpose |
|---|---|
| `IHexMap<TValue>` | Read-only access to any topology-backed map |
| `HexMap<TValue>` | Mutable storage for arbitrary values |
| `ISpatialHexMap<TValue>` | Read-only access to values with world-space geometry |
| `SpatialHexMap<TValue>` | Mutable generic values with retained world-space geometry |
| `BoolHexMap` / `SpatialBoolHexMap` | Boolean masks, logical operators, morphology, and connectivity |
| `IntHexMap` / `SpatialIntHexMap` | Integer fields with extrema, arithmetic, comparisons, and range transforms |
| `FloatHexMap` / `SpatialFloatHexMap` | Floating-point fields with numeric, noise, blur, and range operations |

The specialized spatial maps inherit `SpatialHexMap<TValue>` and retain a
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>. Their topology-only counterparts inherit
`HexMap<TValue>`. Every operator and transformation described below creates a new mutable map and
leaves its inputs unchanged.

## Create and Index a Map

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var terrain = new HexMap<string?>(topology);
var cell = new VectorXYInt(2, 1);

terrain[cell] = "forest";
string? value = terrain[cell];
```

The topology-only constructor allocates `Topology.Count` cells initialized with
`default(TValue)`. A `VectorXYInt` indexer checks X/Y coordinates, while the flat indexer uses
row-major order. An invalid index throws `IndexOutOfRangeException`.

Array constructors retain the supplied array without copying it. Clone caller-owned data before
construction when the map must have independent storage. Conversion methods described below do
copy their source values.

## Transform Values and Neighborhoods

`MapValues` creates a new mutable map by applying a selector to every source value. Selectors that
return `bool`, `int`, or `float` produce the matching specialized map; other result types produce
`HexMap<TResult>`. Spatial sources use the same overloads and preserve their geometry.

A second overload family passes a <xref:Akeldov.Math.Hexes.Topology.PartialSextuplet`1> containing
the six edge-adjacent values to the selector. Its `Adjacent0` through `Adjacent5` positions follow
`HexEdge.Edge0` through `HexEdge.Edge5`; missing boundary neighbors are marked absent and contain
`default(TValue)`.

## Query Numeric Extrema

`IntHexMap` and `FloatHexMap` expose `Min` and `Max`. Each property scans the map. Use `GetMinMax`
to obtain both extrema in one pass, or `TryGetMinMax` when the map may be empty. Reading `Min`,
`Max`, or `GetMinMax` from an empty map throws `InvalidOperationException`.

## Apply Cell-Wise Operators

Numeric maps support unary negation and the following binary operator families:

| Operators | Operands | Result |
|---|---|---|
| `+`, `-`, `*`, `/`, `%` | Two integer maps | Integer map |
| `+`, `-`, `*`, `/`, `%` | Any integer/floating-point map pair | Floating-point map |
| `+`, `-`, `*`, `/`, `%` | Numeric map and matching scalar, in either order | Same numeric map type |
| `<`, `>`, `<=`, `>=` | Numeric maps, including mixed integer/floating-point pairs | Boolean map |
| `<`, `>`, `<=`, `>=` | Numeric map and matching scalar, in either order | Boolean map |

The library intentionally does not overload `==` or `!=` for cell-wise comparison. Integer
division and remainder follow C# integer rules; floating-point operations follow IEEE 754 rules.

```csharp
var costs = new IntHexMap(topology, new[]
{
    1, 2, 3, 4,
    5, 6, 7, 8,
    9, 10, 11, 12,
});

IntHexMap adjusted = -(costs + 2) * 3;
BoolHexMap expensive = costs >= 8;

var elevation = new FloatHexMap(topology, new float[topology.Count]);
FloatHexMap weighted = (elevation + costs) * 0.5f;
BoolHexMap aboveCost = elevation > costs;
```

Two topology-only operands must have equal topologies. Two spatial operands must have equal
geometry, including topology, origin, and radius. A spatial and topology-only pair may also be
combined: their topologies must match, the result is spatial, and it retains the spatial
operand's geometry. The same rules apply to `&`, `|`, and `^` on Boolean maps; unary `!` negates a
Boolean map.

## Clamp or Rescale a Numeric Range

`Clamp(min, max)` restricts every value to an inclusive interval. `Rescale(newMin, newMax)` maps
the source minimum and maximum linearly to a new inclusive interval:

```csharp
IntHexMap boundedCosts = costs.Clamp(2, 8);
FloatHexMap normalizedElevation = elevation.Rescale(0f, 1f);
```

Both methods require an ordered target interval and return a new map. Rescaling a constant map
fills it with `newMin`; integer results use midpoint-to-even rounding. Spatial overloads preserve
the source geometry.

## Process Boolean Regions

Boolean morphology uses a cell and its six edge-adjacent neighbors. Missing neighbors outside the
finite topology are ignored.

```csharp
var land = new BoolHexMap(topology, new bool[topology.Count]);

BoolHexMap expanded = land.Dilate();
BoolHexMap contracted = land.Erode();
BoolHexMap opened = land.Open();
BoolHexMap closed = land.Close();
BoolHexMap outline = land.Outline();
```

`Open` applies erosion followed by dilation, while `Close` applies dilation followed by erosion.
Both execute two direct passes through pooled scratch storage rather than allocating an
intermediate public map.

Connectivity methods also use six-neighbor adjacency:

```csharp
BoolHexMap region = land.FloodFill(new VectorXYInt(1, 1));
(IntHexMap labels, int count) = land.ConnectedComponents();
IntHexMap distanceToWater = land.DistanceTransform(targetValue: false);
```

`FloodFill` selects the connected region having the same Boolean value as its seed.
`ConnectedComponents` assigns deterministic positive labels to `true` components; label zero
represents `false`. `DistanceTransform` stores the minimum number of hex steps to the requested
value, or `int.MaxValue` when that value is absent. Spatial overloads return the corresponding
spatial map and preserve geometry.

## Convert Between Specialized Maps

`ToBoolHexMap`, `ToIntHexMap`, and `ToFloatHexMap` create independent topology-only copies from
compatible interface-typed maps. `ToSpatialHexMap(geometry)` copies a topology-only map into its
specialized spatial form; the supplied geometry must have the same topology. `ToHexMap()` copies
a spatial map back to its topology-only specialization. `ToSpatialFloatHexMap()` converts a
spatial integer map to floating point, while `ToSpatialIntHexMap()` truncates spatial floating-point
values toward zero. Integer maps also provide `ToValueMask(values)` for selecting an explicit set
of values as a Boolean mask.

These methods always allocate independent storage. Later writes to the source and result do not
affect each other.

## Sample Spatial2D Fields

Call `ToSpatialHexMap` on an `IFloatField` or `IIntField` to sample the field at each hex center.
Pass a <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>, or reuse a precomputed
<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> when several fields share one geometry. The method
returns a new mutable specialized spatial map.

`FloatFieldRange` and `IntFieldRange` provide matching overloads that sample pointwise bounds and
draw one value per center with a caller-supplied `Random`. Cells are processed in row-major order.
Floating-point values use the interpolation factor returned by `Random.NextDouble`; integer bounds
are inclusive. Non-finite floating-point bounds and reversed ranges are rejected at the affected
cell.

## Generate and Smooth Floating-Point Fields

Generate deterministic fractal Perlin noise directly from a topology, then smooth it when needed:

```csharp
FloatHexMap noise = topology.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f);

FloatHexMap smoothNoise = noise.GaussianBlur(sigma: 1.25f);
```

`CreatePerlinNoise` samples unit-radius hex centers and returns values in `[0, 1]`.
`GaussianBlur` returns a new map and normalizes its kernel at map boundaries.

Continue with [Complete and Partial Neighborhoods](complete-and-partial-neighborhoods.md) for
fixed-size neighbor values and layout-aware sampling.
