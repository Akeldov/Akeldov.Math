# `IHexMap<TValue>`

`IHexMap<TValue>` is the common contract for hex-indexed value maps.

## Metadata

- `Topology`, including width, height, and layout.

## Access

- Coordinate-based value access.
- Flat hex-indexed lookup.
- Bounds-aware implementations.

## Role

- Keeps map consumers independent from specific value domains.
- Allows topology, geometry, chromatization, and partitioning maps to share a common shape.
- Provides read-only access without promising immutable or snapshot state.

Mutable implementations may change while they are observed through `IHexMap<TValue>`. Algorithms
that need to write values use the separate mutable-map capability rather than assuming every map
implementation owns writable storage.

## Rasterization

- `IHexMap<TValue>` can be rasterized with an explicit `RasterGeometry`.
- The raster grid resolution must match the map width and height.
- This rasterization maps each hex-map value to one raster value; geometry-aware hex-field rasterizers can draw hex cells at arbitrary pixel density.

## Neighborhood sampling

`SampleSextuplet(index)` reads the six edge-adjacent values without reading the center value.
`Adjacent0` through `Adjacent5` always correspond to `HexEdge.Edge0` through `HexEdge.Edge5`;
the concrete offset coordinates are selected from the map layout and row or column parity.

```csharp
Sextuplet<int> neighbors = map.SampleSextuplet(new VectorXYInt(2, 1));
PartialSextuplet<int> edgeNeighbors = map.SamplePartialSextuplet(VectorXYInt.Zero);
```

The center index must be inside the map for both methods. The complete form additionally requires all
six neighbors to be inside the map. The partial form supports boundary cells: absent positions contain
`default(TValue)` and are excluded by `SextupletPresenceFlags`. Both methods work through
`IHexMap<TValue>`, including spatial maps, and allocate no arrays.
