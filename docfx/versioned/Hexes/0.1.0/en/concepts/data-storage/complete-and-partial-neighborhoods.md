# Complete and Partial Neighborhoods

Hex-grid operations often return a small, fixed set of related values: two neighbors beside a
vertex, three hexes meeting at a vertex, or a hex together with its six edge neighbors.
Akeldov.Math.Hexes represents these sets with ordered value types instead of variable-length
collections.

A **complete** value says that every logical position participates in the result. A **partial**
value stores the same positions and adds a bit mask that says which ones are semantically
present. This distinction matters at the boundary of a finite <xref:Akeldov.Math.Hexes.HexMapTopology>:
the surrounding infinite hex grid still has neighbors, but a bounded map may not contain them.

## Know the Slot Order

Each type has a fixed order with a specific meaning. The names are part of the contract; these
types do not expose a variable-length neighborhood to enumerate.

| Complete type | Partial type | Ordered positions | Typical meaning |
|---|---|---|---|
| <xref:Akeldov.Math.Hexes.Topology.Pair`1> | <xref:Akeldov.Math.Hexes.Topology.PartialPair`1> | `Left`, `Right` | The two neighbors beside a selected hex vertex |
| <xref:Akeldov.Math.Hexes.Topology.Triplet`1> | <xref:Akeldov.Math.Hexes.Topology.PartialTriplet`1> | `Main`, `Left`, `Right` | A source hex and the two neighbors meeting it at a vertex |
| <xref:Akeldov.Math.Hexes.Topology.Septuplet`1> | <xref:Akeldov.Math.Hexes.Topology.PartialSeptuplet`1> | `Main`, `Adjacent0` … `Adjacent5` | A source hex followed by its six edge neighbors |
| <xref:Akeldov.Math.Hexes.Topology.ChromaticTriplet`1> | <xref:Akeldov.Math.Hexes.Topology.PartialChromaticTriplet`1> | `Index0`, `Index1`, `Index2` | Three values reordered by chromatic class |

For a septuplet, `Adjacent0` through `Adjacent5` correspond to `HexEdge.Edge0` through
`HexEdge.Edge5`. Their physical directions depend on the layout, but the edge-number order does
not. For a normal triplet, `Left` and `Right` are relative to the selected vertex and layout; they
are not the first two members of a septuplet.

The complete types can be constructed and deconstructed directly:

```csharp
using Akeldov.Math.Hexes.Topology;

var values = new Triplet<string>(
    main: "center",
    left: "left neighbor",
    right: "right neighbor");

var (main, left, right) = values;
```

`Pair<T>`, `Triplet<T>`, `Septuplet<T>`, and `ChromaticTriplet<T>` are generic carriers. Their
constructors do not decide whether a value is a valid map index, interpolation weight, or
application datum. The operation producing the value gives its slots their domain meaning.

## Choose Complete or Partial Values

Use a complete value when all logical positions are meaningful and the consumer can handle an
unbounded grid. Use a partial value when the result will be applied to a finite source and each
slot must be checked before use.

| Question | Complete form | Partial form |
|---|---|---|
| Are all slots semantically present? | Yes | Only the flagged slots |
| Can an index lie outside a finite map? | Yes | It may be stored, but its flag is clear |
| Is `default(T)` automatically missing? | No | No; presence is independent of the payload |
| Is a missing slot automatically cleared? | Not applicable | No; inspect the presence flag |

“Complete” does **not** mean “inside this map.” For example, all six logical neighbors of a
corner hex exist on the infinite grid, so a complete septuplet contains all six indices even
when five or six of them are outside a small rectangular topology.

Conversely, a partial type does not use `null`, `default(T)`, or a sentinel value to encode
absence. A present slot may legitimately contain `default(T)`, and an absent slot may retain a
non-default logical value. Always test `Has...` or `Presence` before consuming a partial slot.

## Read and Combine Presence Flags

Every partial family has its own `[Flags]` enum and convenience properties:

| Partial type | Presence enum | Individual flags and properties |
|---|---|---|
| `PartialPair<T>` | <xref:Akeldov.Math.Hexes.Topology.PairPresenceFlags> | `Left`, `Right`; `HasLeft`, `HasRight` |
| `PartialTriplet<T>` | <xref:Akeldov.Math.Hexes.Topology.TripletPresenceFlags> | `Main`, `Left`, `Right`; `HasMain`, `HasLeft`, `HasRight` |
| `PartialSeptuplet<T>` | <xref:Akeldov.Math.Hexes.Topology.SeptupletPresenceFlags> | `Main`, `Adjacent0` … `Adjacent5`; matching `Has...` properties |
| `PartialChromaticTriplet<T>` | <xref:Akeldov.Math.Hexes.Topology.ChromaticTripletPresenceFlags> | `Index0`, `Index1`, `Index2`; matching `Has...` properties |

Every enum defines `None` and `All`. Combine individual positions with bitwise OR, or use the
constructor overload that accepts one Boolean per position:

```csharp
using Akeldov.Math.Hexes.Topology;

var byFlags = new PartialTriplet<int>(
    main: 10,
    left: 20,
    right: 30,
    presence: TripletPresenceFlags.Main | TripletPresenceFlags.Right);

var byBooleans = new PartialTriplet<int>(
    main: 10,
    left: 20,
    right: 30,
    hasMain: true,
    hasLeft: false,
    hasRight: true);

bool samePresence = byFlags.Presence == byBooleans.Presence; // true

if (byFlags.HasRight)
{
    int value = byFlags.Right;
}
```

You can also construct a partial value from its complete counterpart and a presence mask. The
constructors store the supplied values and flags as-is; they do not validate unknown enum bits,
clear absent payloads, or check relationships between positions. Use only the declared flags and
keep the mask consistent with the payload's meaning.

Each partial type has two deconstruction forms: one returns the stored values, and the other also
returns `Presence`. `ToPair()`, `ToTriplet()`, and `ToSeptuplet()` return all stored payloads in a
complete value and deliberately discard the mask. They do not fill, filter, or validate missing
positions, so convert only when the caller has already handled absence.

The default value of any partial struct has `Presence == None` and `default(T)` in every slot.

## Keep Chromatic and Geometric Order Separate

A <xref:Akeldov.Math.Hexes.Topology.Triplet`1> uses geometric `Main`, `Left`, and `Right` order.
A <xref:Akeldov.Math.Hexes.Topology.ChromaticTriplet`1> instead uses `Index0`, `Index1`, and
`Index2`: each slot belongs to one of the three chromatic classes, regardless of which geometric
slot supplied it.

This reordering lets chromatic barycentric rasters expose a stable class-to-weight mapping. Its
partial counterpart carries `ChromaticTripletPresenceFlags`, because a weight belonging to a hex
outside the finite map is unavailable even though the chromatic position remains defined.

```csharp
using Akeldov.Math.Hexes.Topology;

var weights = new PartialChromaticTriplet<float>(
    index0: 0.25f,
    index1: 0f,
    index2: 0.75f,
    hasIndex0: true,
    hasIndex1: false,
    hasIndex2: true);

float presentWeight = weights.HasIndex2 ? weights.Index2 : 0f;
```

Do not interpret `Index0` as `Main`, `Index1` as `Left`, or `Index2` as `Right`. Convert or reorder
only through an operation that knows the chromatic class of each participating hex.

## Precompute Per-Hex Neighborhoods

Two read-only maps precompute a septuplet for every cell of a topology:

| Map | Value per hex | Boundary behavior |
|---|---|---|
| <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletMap> | `Septuplet<VectorXYInt>` | Keeps the six logical neighbor indices, including out-of-bounds indices |
| <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletMap> | `PartialSeptuplet<VectorXYInt>` | Keeps the same logical indices and clears flags for neighbors outside the topology |

Each map can be constructed from either a topology or a
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>. The topology overload creates a unit-radius
geometry; the geometry overload retains its origin and radius so the neighborhood map can also
participate in spatial-map operations. Neighborhoods themselves depend only on resolution and
layout.

`IndexPartialSeptupletMap` also exposes `Width`, `Height`, and `Count` convenience properties.
For `IndexSeptupletMap`, read the same dimensions and cell count from `Topology`.

The following `1 x 1` map makes the difference explicit:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(1, 1, Layout.OddR);

var completeMap = new IndexSeptupletMap(topology);
Septuplet<VectorXYInt> complete = completeMap[VectorXYInt.Zero];

// complete.Main      == ( 0,  0)
// complete.Adjacent0 == ( 1,  0)
// complete.Adjacent1 == ( 0,  1)
// complete.Adjacent2 == (-1,  1)
// complete.Adjacent3 == (-1,  0)
// complete.Adjacent4 == (-1, -1)
// complete.Adjacent5 == ( 0, -1)

var partialMap = new IndexPartialSeptupletMap(topology);
PartialSeptuplet<VectorXYInt> partial = partialMap[VectorXYInt.Zero];

// partial stores the same seven coordinates.
// partial.Presence == SeptupletPresenceFlags.Main
```

For any valid map-cell lookup, `Main` is present in the partial map. `AdjacentN` is present exactly
when that neighbor falls inside `0 <= X < Width` and `0 <= Y < Height`. The neighbor order is the
same in complete and partial maps and matches the corresponding `HexEdge` number for every
layout.

Use the flags before indexing another bounded map:

```csharp
var costs = new HexMap<int>(topology);
var neighborhood = partialMap[VectorXYInt.Zero];

int total = costs[neighborhood.Main];

if (neighborhood.HasAdjacent0)
    total += costs[neighborhood.Adjacent0];

// Apply the same check to Adjacent1 through Adjacent5.
```

These specialized maps own their precomputed arrays and expose no value setters. A
`VectorXYInt` map indexer checks both coordinates and throws `IndexOutOfRangeException` outside
the topology. The integer indexer uses zero-based row-major order and follows array bounds.
An empty topology is accepted, but naturally has no valid cell to index. Construction from a
geometry rejects a non-finite origin or a radius that is non-finite or not positive with
`ArgumentOutOfRangeException`; the topology itself validates its dimensions and layout.

## Sample Neighborhoods into Rasters

Index rasters answer a different question: for the point at the center of each raster cell,
which source-grid hexes participate? The raster has its own resolution and world-space geometry;
its cell coordinates are not hex-map indices.

| Raster | Stored value | Selection at each sample point |
|---|---|---|
| <xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster> | `Triplet<VectorXYInt>` | The containing hex as `Main`, plus the two neighbors meeting it at the closest vertex as `Left` and `Right` |
| <xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster> | `PartialTriplet<VectorXYInt>` | The same three positions, independently marked when they are inside the source topology |
| <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletRaster> | `Septuplet<VectorXYInt>` | The containing hex as `Main`, plus all six edge neighbors |
| <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster> | `PartialSeptuplet<VectorXYInt>` | The containing hex and its in-bounds neighbors, but only when `Main` is inside the source topology |

Complete rasters use the infinite-grid interpretation. Sampling outside the finite source map is
allowed and still produces logical hex indices. Partial rasters compare those indices with the
source topology, but their absent payload convention differs:

- `IndexPartialTripletRaster` checks `Main`, `Left`, and `Right` independently and stores
  `default(VectorXYInt)` for each absent position. A sample whose `Main` is outside can still have
  a present `Left` or `Right`.
- `IndexPartialSeptupletRaster` stores the computed logical coordinates even in absent slots. If
  `Main` is outside, every flag is clear; if `Main` is inside, each adjacent flag reflects that
  neighbor's bounds.

This is why portable consumer code must treat the mask—not the stored coordinate—as the source of
truth for every partial family.

Each raster can derive a default sampling geometry from the source
`HexMapGeometry.ToRasterGeometry(1f)` or accept an explicit `RasterGeometry`. Construction
requires both source-map dimensions and both raster-resolution components to be positive. It
throws `ArgumentOutOfRangeException` otherwise, and the checked raster cell-count allocation can
throw `OverflowException`.

The `VectorXYInt` raster indexer validates both raster coordinates and throws
`IndexOutOfRangeException` outside its own resolution. Flat integer indices are row-major. The
`[x, y]` overload calculates that flat index without validating each component; an invalid pair
can therefore alias another row or throw. Check both coordinates before using it.

Only the triplet index rasters expose `TryGetValue`:

- `IndexTripletRaster.TryGetValue` returns `false` only when the requested raster coordinate is
  outside the raster; produced hex indices may still be outside the source map.
- `IndexPartialTripletRaster.TryGetValue` also returns `false` for an in-raster sample whose
  `Presence` is `None`. Otherwise it returns the partial triplet and `true`.

The septuplet raster types have indexers but no `TryGetValue` method in this version.

## Understand Immutability and Ownership

All complete and partial neighborhood containers are `readonly struct` values with get-only
properties. Passing one around copies its fixed set of fields, and changing one variable cannot
replace the fields in another copy. This is structural, not deep, immutability: when `T` is a
mutable reference type, the referenced object can still be changed.

The specialized index maps and rasters are classes, but their precomputed storage is owned
internally and exposed only through read-only indexers. Each lookup returns a neighborhood struct
by value; callers cannot replace entries in the internal array. There is no caller-supplied array
whose later mutation could change these lookup tables.

The generic neighborhood constructors perform no map, geometry, payload, or flag validation.
Selection and clipping happen only in the map or raster that produces the neighborhood. Keep the
producing topology, layout, and geometry with the data, and never infer presence from a payload
alone.

Continue with [Maps](maps.md) for general per-cell storage and [Rasters](rasters.md) for the full
sampling-raster families.
