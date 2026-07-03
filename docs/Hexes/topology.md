# Topology

Topology APIs describe adjacency, neighborhood structure, and index relationships without requiring world-space geometry.

## Edges and Vertices

- `HexEdge`.
    - Represents the six edges of a hex.
    - Used to select adjacent hex indexes.
- `HexVertex`.
    - Represents the six vertices of a hex.
    - Used to select adjacent edge pairs and vertex neighborhoods.

## Adjacency

- Neighboring hex indexes by edge.
- Adjacent edge lookup for a vertex.
- Vertex pair adjacency.
- Vertex triplet adjacency.
- Ring offsets around a hex.

## Topology Maps

- `IndexSeptupletMap`.
    - Stores each hex index and its six adjacent indexes.
- `IndexPartialSeptupletMap`.
    - Stores adjacency with presence flags.

## Topology Grids

- `IndexTripletGrid`.
    - Samples vertex triplets as hex indexes.
- `IndexPartialTripletGrid`.
    - Samples vertex triplets with presence flags.
- `IndexSeptupletGrid`.
    - Samples full neighborhood septuplets.
- `IndexPartialSeptupletGrid`.
    - Samples partial neighborhood septuplets.

## Polyhex Topology

- Boolean and integer mask conversion.
- Polyhex builders.
- Polyhex extension and contour extraction.
- Binary serialization helpers for polyhex stamps.
