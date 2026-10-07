# Get a Hex Center and Vertices

Use the map geometry's origin, radius, and layout to convert a storage index into a world-space
center. Then expand that center into the six vertices of the hex.

## Get the center

Create a <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> and select a cell by its
<xref:Akeldov.Math.Spatial2D.VectorXYInt> storage index:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var index = new VectorXYInt(2, 1);

VectorXY center = index.GetHexCenter(
    geometry.Radius,
    geometry.Origin,
    geometry.Topology.Layout);
```

For this geometry, `center` is approximately `(18.660, 23.000)`. `Origin` is the center of the
zero hex, not the minimum corner of the map. Keep the radius, origin, and complete layout from the
same geometry together; mixing values from different grids produces a plausible but incorrect
position.

## Get the six vertices

Call `GetHexVertices` on the center and pass the same radius and layout:

```csharp
VectorXY[] vertices = center.GetHexVertices(
    geometry.Radius,
    geometry.Topology.Layout);

for (int i = 0; i < vertices.Length; i++)
{
    VectorXY vertex = vertices[i];
    Console.WriteLine(FormattableString.Invariant(
        $"Vertex {i}: ({vertex.X:F3}, {vertex.Y:F3})"));
}
```

The result is:

```text
Vertex 0: (20.392, 24.000)
Vertex 1: (18.660, 25.000)
Vertex 2: (16.928, 24.000)
Vertex 3: (16.928, 22.000)
Vertex 4: (18.660, 21.000)
Vertex 5: (20.392, 22.000)
```

The returned array is new, mutable, and owned by the caller. Its six vertices are ordered
counterclockwise. For `OddR` and `EvenR`, vertex `0` is at 30 degrees from the positive X axis;
for `OddQ` and `EvenQ`, it is on the positive X axis.

The radius must be finite and greater than zero. The origin and center must have finite
components, and the layout must be `OddR`, `EvenR`, `OddQ`, or `EvenQ`.

For QRS coordinates instead of storage indices, see
[Convert QRS to Spatial2D coordinates](../coordinates-and-layouts/convert-qrs-to-spatial2d-coordinates.md).
For the formulas and orientation rules, see
[Hex Grid Geometry](../../concepts/hex-grid-model/geometry.md).
