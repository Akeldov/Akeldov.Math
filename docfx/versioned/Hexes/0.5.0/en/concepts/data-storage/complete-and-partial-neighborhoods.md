# Complete and Partial Neighborhoods

Hex-grid operations often return a small fixed set of related values. Akeldov.Math.Hexes uses
ordered value types rather than variable-length collections so every slot keeps a stable domain
meaning.

A complete value always stores every logical slot. A partial value stores the same payload slots
and adds flags describing which ones are semantically present. This distinction matters at the
boundary of a finite <xref:Akeldov.Math.Hexes.HexMapTopology>.

## Know the Slot Order

| Complete type | Partial type | Ordered positions | Typical meaning |
|---|---|---|---|
| `Pair<T>` | `PartialPair<T>` | `Left`, `Right` | Two neighbors beside a selected vertex |
| `Triplet<T>` | `PartialTriplet<T>` | `Main`, `Left`, `Right` | A source hex and the two neighbors meeting it at a vertex |
| `Sextuplet<T>` | `PartialSextuplet<T>` | `Adjacent0` … `Adjacent5` | Six edge neighbors without the center value |
| `Septuplet<T>` | `PartialSeptuplet<T>` | `Main`, `Adjacent0` … `Adjacent5` | A source hex followed by its six edge neighbors |
| `ChromaticTriplet<T>` | `PartialChromaticTriplet<T>` | `Index0`, `Index1`, `Index2` | Three values reordered by chromatic class |

For sextuplets and septuplets, `Adjacent0` through `Adjacent5` correspond exactly to
<xref:Akeldov.Math.Hexes.Topology.HexEdge.Edge0> through
<xref:Akeldov.Math.Hexes.Topology.HexEdge.Edge5>. Their
physical directions depend on the map layout and row or column parity, but their edge-number order
does not.

The complete containers can be constructed and deconstructed directly:

```csharp
using Akeldov.Math.Hexes.Topology;

var values = new Sextuplet<string>(
    "edge 0", "edge 1", "edge 2",
    "edge 3", "edge 4", "edge 5");

var (edge0, edge1, edge2, edge3, edge4, edge5) = values;
```

These `readonly struct` types are generic carriers. Their constructors do not validate map
bounds or assign geometric meaning; the producing operation defines what each payload represents.

## Interpret Partial Values

Each partial type exposes a `Presence` bit mask and matching `Has...` properties. The mask, not a
stored payload, is the source of truth: a missing value may still equal a valid value by accident.

| Partial type | Presence flags |
|---|---|
| `PartialPair<T>` | `PairPresenceFlags` |
| `PartialTriplet<T>` | `TripletPresenceFlags` |
| `PartialSextuplet<T>` | `SextupletPresenceFlags` |
| `PartialSeptuplet<T>` | `SeptupletPresenceFlags` |
| `PartialChromaticTriplet<T>` | `ChromaticTripletPresenceFlags` |

Every presence enum defines `None` and `All`. `ToPair`, `ToTriplet`, `ToSextuplet`, and
`ToSeptuplet` return all stored payloads and deliberately discard the mask; convert only after the
caller has handled absence.

“Complete” does not mean “inside this map.” All six logical neighbors exist on the infinite hex
grid, so a complete index neighborhood may contain coordinates outside a bounded topology.

## Sample Six Neighbor Values

Use `SampleSextuplet` to read the six values around an interior cell without reading its center:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(3, 3, Layout.OddR);
var map = new IntHexMap(topology, new int[topology.Count]);

Sextuplet<int> neighbors =
    map.SampleSextuplet(new VectorXYInt(1, 1));
```

The center and all six neighbors must be inside the map. The method validates every coordinate
before reading any value and throws `ArgumentOutOfRangeException` when the complete neighborhood
does not fit.

Use `SamplePartialSextuplet` at a boundary:

```csharp
PartialSextuplet<int> corner =
    map.SamplePartialSextuplet(VectorXYInt.Zero);

if (corner.HasAdjacent0)
{
    int edge0Value = corner.Adjacent0;
}
```

Only the center must be inside the map. Missing neighbors store `default(TValue)` and have their
flags cleared. Both methods work through `IHexMap<TValue>`, including spatial maps, choose offsets
from layout and parity, and allocate no arrays.

## Precompute Index Neighborhoods

Use `IndexSeptupletMap` or `IndexPartialSeptupletMap` when the same index neighborhood will be read
many times for every source cell. Unlike neighbor-value sampling, these maps store the center index
as `Main` together with six adjacent indices.

`IndexSeptupletMap` keeps all infinite-grid coordinates, even when an adjacent index is outside the
bounded topology. `IndexPartialSeptupletMap` stores the same logical coordinates and clears flags
for out-of-bounds neighbors. For every valid lookup, `Main` is present.

```csharp
var neighborhoods = new IndexPartialSeptupletMap(topology);
PartialSeptuplet<VectorXYInt> neighborhood =
    neighborhoods[VectorXYInt.Zero];

int total = map[neighborhood.Main];
if (neighborhood.HasAdjacent0)
    total += map[neighborhood.Adjacent0];
```

## Sample Neighborhoods into Rasters

Index rasters answer a spatial sampling question rather than a per-cell lookup question:

- `IndexTripletRaster` and `IndexPartialTripletRaster` store the containing hex and the two hexes
  meeting it at the closest vertex;
- `IndexSeptupletRaster` and `IndexPartialSeptupletRaster` store the containing hex and all six
  edge neighbors.

Complete rasters use infinite-grid coordinates. Partial rasters preserve slot order and use flags
when the finite source topology does not contain a position. See [Rasters](rasters.md) for geometry,
indexing, and boundary behavior.

## Keep Ownership Explicit

Complete and partial containers are value types with get-only properties. Passing one copies its
fields. Generic payloads may still refer to mutable objects; the containers do not clone them.

Specialized neighborhood maps and rasters own their precomputed storage and expose read-only
indexers. Each lookup returns a neighborhood struct by value, so callers cannot replace entries in
the internal storage.
