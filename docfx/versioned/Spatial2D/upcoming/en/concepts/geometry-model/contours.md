# Contours

Contours are closed boundaries with finite length. Spatial2D provides circular and rectangular
contours and can join finite paths into a composite boundary. Contours support all common
[curve](curves.md) queries and add enclosure and signed-distance operations.

Contour types live in the <xref:Akeldov.Math.Spatial2D.Contours> namespace.

## Distinguish curves, contours, and regions

These abstractions describe related but different geometry:

| Abstraction | Meaning | Typical operation |
|---|---|---|
| [Curve](curves.md) | One-dimensional geometry that may be open, closed, finite, or infinite. | Project a point or intersect a ray. |
| Contour | A finite closed boundary. | Test enclosure or measure signed distance to the boundary. |
| [Region](regions.md) | A filled two-dimensional area, possibly bounded by several contours. | Test area membership, including holes. |

Use a contour when the boundary itself is the primary object. Use a region when fill semantics,
multiple boundaries, or holes matter. A contour's `Encloses` method still provides a convenient
single-boundary containment test.

## Understand the contour interfaces

The contour interfaces add closed-boundary capabilities to the curve model:

| Interface | Capability |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Contours.IContour> | A finite closed curve with `Length`, `Encloses`, unsigned `Distance`, and `SignedDistance`. |
| <xref:Akeldov.Math.Spatial2D.Contours.IParameterizedContour> | Adds a length-based curve coordinate and parameterized projection. |
| <xref:Akeldov.Math.Spatial2D.Curves.IContourPath> | A finite directed path with the fill-rule crossing query required by a composite contour. |
| <xref:Akeldov.Math.Spatial2D.Contours.ICompositeContour> | Exposes the contour paths forming a contour as a read-only structural view. |
| <xref:Akeldov.Math.Spatial2D.Contours.IParameterizedCompositeContour> | Combines a composite boundary with one continuous coordinate around it. |

All contours implement <xref:Akeldov.Math.Spatial2D.Curves.ICurve> for point distance and
projection. They separately implement
<xref:Akeldov.Math.Spatial2D.Curves.IRightwardCrossingProvider> for fill-rule queries. `IContour`
does not expose polymorphic ray intersections; supported non-composite contour types provide
those operations through extension methods. Composite contour types do not provide them.

## Choose a concrete contour

Choose a type from the boundary shape and whether it needs a traversal coordinate:

| Shape | Non-parameterized | Parameterized | Use when |
|---|---|---|---|
| Circle | <xref:Akeldov.Math.Spatial2D.Contours.Circle> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCircle> | The boundary is a full circumference. |
| Axis-aligned rectangle | <xref:Akeldov.Math.Spatial2D.Contours.RectangleContour> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedRectangleContour> | Rectangle edges remain parallel to the world axes. |
| Rotated rectangle | <xref:Akeldov.Math.Spatial2D.Contours.OrientedRectangleContour> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedOrientedRectangleContour> | The rectangle has a world-space rotation. |
| Paths or polygon vertices | <xref:Akeldov.Math.Spatial2D.Contours.CompositeContour> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCompositeContour> | The boundary is assembled from segments, arcs, Bezier curves, or other finite paths. |

The non-parameterized types describe only the boundary geometry. The parameterized types also
choose where coordinate zero lies and the direction in which coordinates increase.

## Create standard contours

Circular and rectangular contours expose their boundary dimensions directly:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 3f);

var rectangle = new RectangleContour(
    cornerA: new PointXY(-2f, -1f),
    cornerB: new PointXY(2f, 1f));

float circumference = circle.Length;
float perimeter = rectangle.Length; // 12
```

`RectangleContour` normalizes the two opposite corners into `Min` and `Max`, so their input
order does not matter. `OrientedRectangleContour` instead accepts a center, a size, and a
rotation in radians. See [Angles and Units](../fundamentals/angles-and-units.md) for the rotation
convention.

Rectangular dimensions may be zero but never negative. Positive width and height produce the
usual area and boundary; one zero dimension produces a segment; two zero dimensions produce a
point. All six rectangular region and contour structures have valid `default` values representing
the origin point.

A degenerate rectangular contour is a closed out-and-back traversal of its segment, so its
`Length` is twice the segment length. A point contour has `Length` equal to zero. Degenerate
rectangles have no interior: their `SignedDistance` is zero on the represented segment or point
and positive outside it. These geometry rules do not change the separate validation performed by
`RasterGeometry`.

## Build a composite contour

For a polygon, pass at least three vertices in boundary order. The constructor joins consecutive
vertices with parameterized segments and closes the last vertex back to the first:

```csharp
var triangle = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f),
    new PointXY(2f, 3f));
```

For a mixed boundary, pass an `IReadOnlyList<IContourPath>`. Every path must end where the next
path starts, and the final path must reconnect to the first:

```csharp
using System;
using Akeldov.Math.Spatial2D.Curves;

var curvedBoundary = new CompositeContour(new IContourPath[]
{
    new ParameterizedSegment(
        new PointXY(-2f, 0f),
        new PointXY(2f, 0f)),
    new ParameterizedArc(
        center: new PointXY(0f, 0f),
        radius: 2f,
        startAngle: 0f,
        endAngle: MathF.PI,
        angularDirection: AngularDirection.Counterclockwise)
});
```

Composite constructors copy the supplied path references into private storage. `Curves` exposes
a read-only structural view of that copy, so callers cannot change the contour's order or
cardinality after construction. Paths must have finite non-negative lengths, and their total
length must remain finite.

## Parameterize a closed boundary

An `IParameterizedContour` maps coordinates in `[0, Length]` to positions around the boundary.
Coordinates `0` and `Length` describe the same geometric location because the contour is closed:

```csharp
using Akeldov.Math.Spatial2D.Curves;

var directedCircle = new ParameterizedCircle(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: MathF.PI / 2f,
    contourDirection: ContourDirection.Counterclockwise);

PointXY start = directedCircle.GetPoint(0f); // Approximately (0, 2)
PointXY halfway = directedCircle.GetPoint(directedCircle.Length * 0.5f);
```

Parameterized rectangles can select a named boundary origin or a perimeter coordinate.
`ParameterizedCompositeContour` starts at the first path's `StartPoint` and advances through
the paths in list order, giving the whole chain one continuous length coordinate.

## Query enclosure and boundary distance

`Encloses` returns `true` for a point inside or on the closed boundary. `Distance` is always
non-negative, while `SignedDistance` conventionally returns a negative value inside:

```csharp
var sample = new PointXY(1f, 0f);

bool enclosed = circle.Encloses(sample);       // true
float distance = circle.Distance(sample);      // 2
float signedDistance = circle.SignedDistance(sample); // -2
CurveProjection projection = circle.Project(sample);
```

`SignedDistance` has no tolerance parameter. Its sign is determined directly from the contour's
boundary and enclosure calculations.

Supported non-composite contour types expose `GetPointIntersections` through intersection
extension methods; neither `IContour` nor `IContourPath` declares the operation.
`CompositeContour` and `ParameterizedCompositeContour` do not provide ray-intersection overloads
because their heterogeneous paths have no common binary-intersection contract.

## Smooth polygonal corners

`FilletCorners(radius)` returns a new `CompositeContour`: adjacent parameterized segments are
trimmed and tangent arcs are inserted between them. The source contour is unchanged.

```csharp
var square = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f),
    new PointXY(4f, 4f),
    new PointXY(0f, 4f));

CompositeContour rounded = square.FilletCorners(radius: 0.5f);
```

The radius is expressed in world coordinate units. Corners involving path types other than
`ParameterizedSegment` are preserved.

## Turn a boundary into a region

Circular and rectangular contour types provide their corresponding filled-region value through
`ToRegion`. Use <xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion> for arbitrary composite
boundaries or for several contours interpreted with a fill rule.

For practical examples, see:

- [Build a closed contour](../../how-to-guides/curves-and-contours/build-a-closed-contour.md)
- [Create a region with holes](../../how-to-guides/regions/create-a-region-with-holes.md)
- [Rasterization concepts](../rasterization.md)
