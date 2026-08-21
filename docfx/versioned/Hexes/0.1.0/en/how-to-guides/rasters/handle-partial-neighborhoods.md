# Handle Partial Neighborhoods

Use partial index rasters when samples touch or extend beyond a finite hex map. Their presence
flags distinguish usable in-map indices from logical neighbors on the surrounding infinite grid.
Always check a position's `Has...` property before using its stored index with a bounded map.

## Create a partial triplet raster

The following sampling geometry includes a margin, so some raster cells can refer to positions
outside the `3 × 2` source topology:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var mapGeometry = new HexMapGeometry(
    width: 3,
    height: 2,
    radius: 1f,
    layout: Layout.OddR);

var values = new HexMap<int>(mapGeometry.Topology, new[]
{
    10, 20, 30,
    40, 50, 60
});

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 8f,
    margin: mapGeometry.Radius);

var indexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Each raster cell stores a `PartialTriplet<VectorXYInt>` with `Main`, `Left`, and `Right` positions.
Their `HasMain`, `HasLeft`, and `HasRight` properties are independent: a sample whose `Main` lies
outside the map can still have an in-map left or right neighbor.

## Consume only present triplet positions

`TryGetValue` returns `true` only when the raster coordinate is valid and at least one of the three
hex positions belongs to the source topology. This makes it convenient for skipping empty samples:

```csharp
var sample = new VectorXYInt(
    indexRaster.Resolution.X / 2,
    indexRaster.Resolution.Y / 2);

if (!indexRaster.TryGetValue(
        sample,
        out PartialTriplet<VectorXYInt> indices))
{
    Console.WriteLine("The sample has no source-map values.");
    return;
}

int total = 0;
int count = 0;

if (indices.HasMain)
{
    total += values[indices.Main];
    count++;
}

if (indices.HasLeft)
{
    total += values[indices.Left];
    count++;
}

if (indices.HasRight)
{
    total += values[indices.Right];
    count++;
}

float average = (float)total / count;
Console.WriteLine($"Average of present values: {average}");
```

Because a successful `TryGetValue` guarantees at least one present position, `count` is positive.
When code has already validated the raster coordinate and needs to distinguish an empty sample,
read the indexer and compare `Presence` with `TripletPresenceFlags.None` instead.

## Handle a partial seven-index neighborhood

Use <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster> when the operation needs `Main`
and all six edge-adjacent positions:

```csharp
var septupletRaster = new IndexPartialSeptupletRaster(
    mapGeometry,
    rasterGeometry);

PartialSeptuplet<VectorXYInt> neighborhood = septupletRaster[sample];

if (!neighborhood.HasMain)
{
    Console.WriteLine("The sample lies outside the source map.");
    return;
}

int neighborhoodTotal = values[neighborhood.Main];

if (neighborhood.HasAdjacent0)
    neighborhoodTotal += values[neighborhood.Adjacent0];
if (neighborhood.HasAdjacent1)
    neighborhoodTotal += values[neighborhood.Adjacent1];
if (neighborhood.HasAdjacent2)
    neighborhoodTotal += values[neighborhood.Adjacent2];
if (neighborhood.HasAdjacent3)
    neighborhoodTotal += values[neighborhood.Adjacent3];
if (neighborhood.HasAdjacent4)
    neighborhoodTotal += values[neighborhood.Adjacent4];
if (neighborhood.HasAdjacent5)
    neighborhoodTotal += values[neighborhood.Adjacent5];

Console.WriteLine($"Present-neighborhood total: {neighborhoodTotal}");
```

For a partial septuplet, `HasMain == false` implies that all seven flags are clear. When `Main` is
present, each `HasAdjacentN` independently reports whether the corresponding edge neighbor is
inside the topology. Septuplet rasters have no `TryGetValue`, so validate the
raster coordinate or use the checked `[VectorXYInt]` indexer as above.

## Do not infer presence from stored values

The two partial raster families have different absent-payload details:

| Raster | Presence behavior | Payload in an absent position |
|---|---|---|
| `IndexPartialTripletRaster` | Checks `Main`, `Left`, and `Right` independently | `default(VectorXYInt)` |
| `IndexPartialSeptupletRaster` | Clears every flag when `Main` is absent; otherwise checks each neighbor | Retains the computed logical index |

For both types, the flag is the source of truth. Never compare an index with `VectorXYInt.Zero`:
`(0, 0)` is a valid source-map cell. `ToTriplet()` and `ToSeptuplet()` preserve the stored payloads
but discard all presence information, so call them only after absence has already been handled.
Partial rasters do not clamp missing indices to the nearest map cell.

See [Create an Index Triplet Raster](create-an-index-triplet-raster.md) and
[Create an Index Septuplet Raster](create-an-index-septuplet-raster.md) for the complete variants.
For partial interpolation weights, continue with
[Create a Barycentric Raster](create-a-barycentric-raster.md). The underlying value types are
described in
[Complete and Partial Neighborhoods](../../concepts/data-storage/complete-and-partial-neighborhoods.md).
