# Get Map Geometry Bounds

Use `GetBoundingBox()` to obtain the axis-aligned rectangle that contains every hex in a finite
map, including the outer edges and vertices of its boundary cells.

## Get the bounding rectangle

Create a <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> with the map's topology, zero-hex
center, and radius, then call `GetBoundingBox()`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var geometry = new HexMapGeometry(
    width: 3,
    height: 2,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

Rectangle bounds = geometry.GetBoundingBox();

Console.WriteLine(FormattableString.Invariant(
    $"Min: ({bounds.Min.X:F3}, {bounds.Min.Y:F3})"));
Console.WriteLine(FormattableString.Invariant(
    $"Max: ({bounds.Max.X:F3}, {bounds.Max.Y:F3})"));
Console.WriteLine(FormattableString.Invariant(
    $"Size: ({bounds.Size.X:F3}, {bounds.Size.Y:F3})"));
```

The result is:

```text
Min: (8.268, 18.000)
Max: (20.392, 25.000)
Size: (12.124, 7.000)
```

`Origin` is the center of the zero hex, so it is not generally equal to `bounds.Min`. The
bounding rectangle extends from the outermost vertex or edge on each axis. The odd or even
offset rule can also move a shifted row or column toward a negative axis direction.

## Get only the size

If an operation needs the dimensions but not the rectangle position, call `GetBoundingBoxSize()`:

```csharp
VectorXY size = geometry.GetBoundingBoxSize();
```

The returned value equals `geometry.GetBoundingBox().Size`. It is useful when allocating an
output surface whose origin is managed separately. Use the complete rectangle when world-space
alignment matters.

If you already have a <xref:Akeldov.Math.Hexes.HexMapTopology> but do not need to retain a
`HexMapGeometry`, pass the placement values directly:

```csharp
Rectangle sameBounds = geometry.Topology.GetBoundingBox(
    geometry.Origin,
    geometry.Radius);
```

Both map dimensions must be greater than zero. An empty topology is valid for storage, but it has
no geometric bounds, so `GetBoundingBox()` and `GetBoundingBoxSize()` throw
`ArgumentOutOfRangeException` when the width or height is zero. The origin must have finite
components, the radius must be finite and positive, and the layout must be supported.

For cell centers and corner positions, see
[Get a Hex Center and Vertices](get-a-hex-center-and-vertices.md). For the formulas behind map
extents, see [Hex Grid Geometry](../../concepts/hex-grid-model/geometry.md).
