# Discrete Indices

A discrete index identifies an element in a two-dimensional integer grid. In
Akeldov.Math.Spatial2D, <xref:Akeldov.Math.Spatial2D.VectorXYInt> carries the two integer
components used for raster cells, resolutions, dimensions, and discrete offsets.

The type stores the numbers but does not assign them a role. The API that receives a
`VectorXYInt` determines whether its value is an index, an extent, or an offset and which values
are valid.

## Keep the role visible

The same value type serves several related purposes:

| Role | Meaning | Typical constraints |
|---|---|---|
| Index | Address of one grid element | Zero-based and inside a resolution |
| Resolution | Number of columns and rows | Both components are positive |
| Offset | Relative movement between indices | Components may be negative |
| Dimensions | Integral width and height | Constraints depend on the receiving API |

Use names such as `index`, `resolution`, and `offset` to keep the role clear:

```csharp
using Akeldov.Math.Spatial2D;

var index = new VectorXYInt(3, 2);
var resolution = new VectorXYInt(5, 4);
var offset = new VectorXYInt(-1, 1);

VectorXYInt neighbor = index + offset; // (2, 3)
```

Although `index + index` is numerically valid, it usually has no useful addressing meaning.
Treat the role imposed by the surrounding API as part of the value's contract.

## Use zero-based raster indices

Spatial2D rasters use zero-based two-dimensional indices. `X` selects the column and `Y` selects
the row. For a resolution `(width, height)`, valid indices occupy half-open ranges:

```text
0 <= X < width
0 <= Y < height
```

For example, a raster with resolution `(5, 4)` has first index `(0, 0)` and last index `(4, 3)`.
The resolution itself is not a valid index.

```csharp
var resolution = new VectorXYInt(5, 4);
var index = new VectorXYInt(4, 3);

bool isInside =
    index.X >= 0 && index.X < resolution.X &&
    index.Y >= 0 && index.Y < resolution.Y; // true
```

The built-in `Raster<TValue>` validates indices at access time. A negative component or a
component equal to or greater than the corresponding resolution causes
`ArgumentOutOfRangeException`.

## Access raster cells

The raster contracts support either a `VectorXYInt` index or separate `x` and `y` components:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var raster = new Raster<char>(
    resolution: new VectorXYInt(3, 2),
    values: new[]
    {
        'a', 'b', 'c',
        'd', 'e', 'f'
    });

char first = raster[new VectorXYInt(0, 0)]; // 'a'
char secondRowFirst = raster[0, 1];         // 'd'
char last = raster[new VectorXYInt(2, 1)];  // 'f'
```

`Raster<TValue>` stores values in row-major order. Its two-dimensional index `(x, y)` maps to:

```text
flatIndex = y * resolution.X + x
```

Therefore the `X` index changes fastest in the retained value array. The flat `int` index on the
general `IRaster<TValue>` contract has implementation-defined ordering, however. Do not assume
row-major flat indexing when working with an arbitrary implementation through that interface.

## Iterate within a resolution

Loop over `Y` and then `X` to visit a concrete `Raster<TValue>` in its row-major order:

```csharp
VectorXYInt resolution = raster.Resolution;

for (int y = 0; y < resolution.Y; y++)
{
    for (int x = 0; x < resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        char value = raster[index];
    }
}
```

Raster resolutions must have positive components. `Raster<TValue>` also requires the cell count
to fit in a one-dimensional array and the supplied value array to contain exactly
`resolution.X * resolution.Y` elements.

## Build neighboring indices with offsets

The integer basis vectors are convenient one-cell offsets along the Cartesian axes:

```csharp
var index = new VectorXYInt(2, 2);

VectorXYInt right = index + VectorXYInt.BasisX; // (3, 2)
VectorXYInt left = index - VectorXYInt.BasisX;  // (1, 2)
VectorXYInt up = index + VectorXYInt.BasisY;    // (2, 3)
VectorXYInt down = index - VectorXYInt.BasisY;  // (2, 1)
```

Arithmetic does not know the target grid's resolution. Check each derived index before using it;
an offset from an edge cell can produce a negative or upper-bound index.

## Map cells to world space

An index is not a world-space position. <xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>
connects a discrete raster resolution to continuous bounds. Its origin is the lower-left corner,
and `GetCellCenter` returns the world-space center of a cell:

```csharp
var geometry = new RasterGeometry(
    origin: new PointXY(1f, 2f),
    size: new VectorXY(10f, 6f),
    resolution: new VectorXYInt(5, 3));

VectorXY cellSize = geometry.CellSize; // (2, 2)
PointXY first = geometry.GetCellCenter(0, 0); // (2, 3)
PointXY last = geometry.GetCellCenter(new VectorXYInt(4, 2)); // (10, 7)
```

The center is offset by half a cell from its boundaries. In each axis, the mapping is:

```text
center = origin + (index + 0.5) * cellSize
```

Increasing `X` moves along the positive world X axis, and increasing `Y` moves along the
positive world Y axis. Image-file row conventions are an encoding concern and do not change this
world-space grid definition.

## Define world-to-index behavior explicitly

Mapping a world-space point back to a cell requires a boundary policy. A point can lie outside
the raster, on the outer boundary, or exactly between cells. `RasterGeometry` deliberately
provides the unambiguous index-to-center mapping; code performing the reverse mapping must decide
how those cases are handled.

A common half-open-cell policy first expresses the point in cell coordinates, rejects values
outside `[0, resolution)`, and then applies `MathF.Floor`:

```csharp
PointXY point = new PointXY(4.5f, 5.5f);

float cellX = (point.X - geometry.Origin.X) / geometry.CellSize.X;
float cellY = (point.Y - geometry.Origin.Y) / geometry.CellSize.Y;

if (cellX < 0f || cellX >= geometry.Resolution.X ||
    cellY < 0f || cellY >= geometry.Resolution.Y)
{
    throw new ArgumentOutOfRangeException(nameof(point));
}

var index = new VectorXYInt(
    (int)MathF.Floor(cellX),
    (int)MathF.Floor(cellY));
```

Do not replace `Floor` with a direct cast when negative coordinates are possible: integer casts
truncate toward zero and can incorrectly move a point just below the origin into index zero.
Use rounding only when the desired rule is nearest grid coordinate rather than containing cell.
See [Vectors](vectors.md) for conversion and rounding semantics.

## Compare indices exactly

Discrete indices use structural exact equality. This makes `VectorXYInt` suitable as a key in a
dictionary or as an element of a set:

```csharp
using System.Collections.Generic;

var visited = new HashSet<VectorXYInt>();
visited.Add(new VectorXYInt(3, 2));

bool alreadyVisited = visited.Contains(new VectorXYInt(3, 2)); // true
```

Tolerance-based comparison belongs to continuous points and vectors, not to discrete indices.

For complete member lists, see the API references for
<xref:Akeldov.Math.Spatial2D.VectorXYInt> and
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>.
