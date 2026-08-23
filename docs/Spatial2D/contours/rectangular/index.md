# Rectangular Contours

Rectangular contours represent closed four-edge boundaries. Choose between axis-aligned and oriented geometry, then choose whether a length-based boundary coordinate is required.

## Choosing a Type

| Type | Orientation | Curve coordinate |
| --- | --- | --- |
| [`RectangleContour`](../rectangle-contour.md) | Axis-aligned | None |
| [`ParameterizedRectangleContour`](../parameterized-rectangle-contour.md) | Axis-aligned | `[0, Length]` |
| [`OrientedRectangleContour`](../oriented-rectangle-contour.md) | Rotated in world space | None |
| [`ParameterizedOrientedRectangleContour`](../parameterized-oriented-rectangle-contour.md) | Rotated in world space | `[0, Length]` |

The non-parameterized types expose geometric boundary operations without selecting a traversal origin. Parameterized variants add a boundary point where coordinate `0` lies and a `ContourDirection` that controls whether coordinates increase clockwise or counterclockwise.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var contour = new ParameterizedRectangleContour(
    cornerA: new PointXY(0f, 0f),
    cornerB: new PointXY(4f, 2f),
    contourDirection: ContourDirection.Counterclockwise);

float perimeter = contour.Length;       // 12
PointXY origin = contour.GetPoint(0f);   // (4, 1), the default right-edge midpoint
PointXY next = contour.GetPoint(1f);     // (4, 2)
```

Oriented variants use a center, size, and rotation. Rotation is counterclockwise in radians. Parameter origins may be selected with a named rectangular boundary point or an explicit perimeter coordinate.

Rectangle dimensions may be zero but never negative. One zero dimension produces a closed segment
contour whose traversal goes out and back and whose `Length` is twice the segment length. Two zero
dimensions produce a point contour with `Length` equal to zero. All four rectangular contour
structures therefore have valid `default` values representing the origin point. On a degenerate
contour, `SignedDistance` is zero on the represented set and positive outside it.

Use the corresponding types in [Regions](../../regions/index.md) when filled-area behavior is the primary abstraction.
