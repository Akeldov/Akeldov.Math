# Obtaining Edges and Vertices

In this part of the tutorial, you will obtain the six world-space vertices of one occupied cell
and connect consecutive vertices into edge segments. This is useful when an application needs
individual cell geometry rather than only the final polyhex outline.

## Build one cell's geometry

Add these namespace imports at the top of `Program.cs`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
```

Then add this code after the polyhex output:

```csharp
const Layout layout = Layout.OddR;
const float hexRadius = 1f;
const int sampleQ = 0;
const int sampleR = 1;

VectorXY[] vertices =
    Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexVertices(
        sampleQ,
        sampleR,
        hexRadius,
        layout);
var edges = new Segment[vertices.Length];

for (int index = 0; index < vertices.Length; index++)
{
    edges[index] = new Segment(
        (PointXY)vertices[index],
        (PointXY)vertices[(index + 1) % vertices.Length]);
}

Console.WriteLine($"Sample vertices: {vertices.Length}");
Console.WriteLine($"Sample edges: {edges.Length}");
```

Expected final lines:

```text
Sample vertices: 6
Sample edges: 6
```

`GetHexVertices` returns a new mutable array owned by the caller. The selected Q/R cell `[0, 1]`
is occupied, and `OddR` gives it a pointy-top orientation in XY space. The last edge wraps from
vertex 5 back to vertex 0.

These are the six edges of one cell. Repeating this for every occupied cell would include shared
internal edges twice. The region conversion later in the tutorial removes internal edges and
keeps only the polyhex boundary.

Continue with [Converting to Spatial2D Geometry](converting-to-spatial2d-geometry.md).
