# Get Edges and Vertices

Use `HexEdge` to select the neighbor across a side and `HexVertex` to select the three hexes that
meet at a corner. Always pass the layout used by the map.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(5, 5, Layout.OddR);
var hex = new VectorXYInt(2, 2);

// The two hexes sharing Edge0 are hex and acrossEdge.
VectorXYInt acrossEdge = hex.GetAdjacent(
    HexEdge.Edge0,
    topology.Layout); // (3, 2)

// Three hexes meet at Vertex0.
Triplet<VectorXYInt> atVertex = hex.GetAdjacentTriplet(
    HexVertex.Vertex0,
    topology.Layout);

VectorXYInt main = atVertex.Main;   // (2, 2)
VectorXYInt left = atVertex.Left;   // (2, 3)
VectorXYInt right = atVertex.Right; // (3, 2)
```

Use `GetAdjacentPair(vertex, layout)` when only the two neighboring hexes are needed. Use
`vertex.GetAdjacentEdges(layout)` to obtain the two `HexEdge` values incident to that vertex.

Edges and vertices are numbered from `0` through `5` counterclockwise. Their physical orientation
depends on the layout. These helpers operate on the infinite grid, so check returned indices
against finite-map bounds before using them in a `HexMap<TValue>`.

The methods above describe topological relationships rather than world-space points or segments.
For spatial coordinates, see [Geometry](../../concepts/hex-grid-model/geometry.md). Continue with
[Build polyhex topology from a mask](build-polyhex-topology-from-a-mask.md) to describe an
arbitrary finite shape.
