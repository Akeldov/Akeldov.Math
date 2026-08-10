# Geometry Model

The Spatial2D geometry model builds complex shapes from a small set of roles: points locate
geometry, vectors describe displacement, curves form one-dimensional paths, contours close those
paths into boundaries, and regions give boundaries fill semantics.

This section explains how those roles relate and which abstraction to use for a spatial task.

## Model at a glance

Each layer adds meaning while reusing the operations of the layer below it where appropriate:

| Layer | Represents | Main abstractions | Typical queries |
|---|---|---|---|
| Coordinates | Positions, directions, offsets, and sizes | <xref:Akeldov.Math.Spatial2D.PointXY>, <xref:Akeldov.Math.Spatial2D.VectorXY> | Distance between points, vector length, dot product, rotation |
| [Curves](curves.md) | One-dimensional geometry, open or closed, finite or infinite | <xref:Akeldov.Math.Spatial2D.Curves.ICurve>, <xref:Akeldov.Math.Spatial2D.Curves.IParameterizedCurve> | Projection, boundary distance, ray intersections, point at a curve coordinate |
| [Contours](contours.md) | Finite closed boundaries | <xref:Akeldov.Math.Spatial2D.Contours.IContour>, <xref:Akeldov.Math.Spatial2D.Contours.IParameterizedContour> | Enclosure, signed distance, perimeter length |
| [Regions](regions.md) | Filled two-dimensional areas | <xref:Akeldov.Math.Spatial2D.Regions.IRegion>, <xref:Akeldov.Math.Spatial2D.Regions.IContourBasedRegion> | Area membership and signed distance to the boundary |

Every contour is a finite curve, so curve projection and intersection operations also work on
contours. A region is not a curve: it represents filled area rather than a path. A
contour-based region retains one or more contours and interprets them with a fill rule.

## Choose an abstraction

Start from the question the code needs to answer:

- Use `PointXY` for a position and `VectorXY` for a direction, offset, displacement, or size.
- Use `ICurve` when the geometry may be open or infinite, or when only projection, distance, and
  intersections matter.
- Use `IParameterizedCurve` when a position must also have a coordinate along the curve.
- Use `IContour` when the primary object is one finite closed boundary.
- Use `IParameterizedContour` when that closed boundary also needs an origin and traversal
  direction.
- Use `IRegion` when filled-area membership or an inside/outside distance is the goal.
- Use `IContourBasedRegion` when several boundaries define holes or nested filled areas.

Prefer a concrete shape such as `Segment`, `Circle`, or `Disk` when the geometry has that exact
form. Use composite types when the shape is assembled from heterogeneous pieces or multiple
boundaries.

## Match boundaries to filled shapes

Several common shapes have separate boundary and filled-area types:

| Shape | Boundary | Filled area |
|---|---|---|
| Circle | <xref:Akeldov.Math.Spatial2D.Contours.Circle> | <xref:Akeldov.Math.Spatial2D.Regions.Disk> |
| Axis-aligned rectangle | <xref:Akeldov.Math.Spatial2D.Contours.RectangleContour> | <xref:Akeldov.Math.Spatial2D.Regions.Rectangle> |
| Rotated rectangle | <xref:Akeldov.Math.Spatial2D.Contours.OrientedRectangleContour> | <xref:Akeldov.Math.Spatial2D.Regions.OrientedRectangle> |
| Arbitrary closed shape | <xref:Akeldov.Math.Spatial2D.Contours.CompositeContour> | <xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion> |

Choose the boundary type for ray intersections, perimeter traversal, or contour rasterization.
Choose the region type for containment, filled rasterization, masks, and signed distance fields.
Standard region types provide `ToContour`; rectangular regions can also be wrapped as a general
contour-based region.

## Build geometry by composition

A polygon demonstrates how the layers fit together. Points define vertices, parameterized
segments connect them, a composite contour closes the boundary, and a contour-based region fills
it:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Regions;

var boundary = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(6f, 0f),
    new PointXY(3f, 4f));

var region = new ContourBasedRegion(
    new IContour[] { boundary });

var sample = new PointXY(3f, 2f);

CurveProjection nearestBoundaryPoint = boundary.Project(sample);
bool inside = region.Contains(sample);              // true
float signedDistance = region.SignedDistance(sample); // negative
```

The point-based `CompositeContour` constructor creates and closes the segments automatically.
For mixed boundaries, supply an ordered list of `IFinitePath` instances such as parameterized
segments, arcs, and Bezier curves. Adjacent endpoints must meet, including the last path and the
first.

## Use the common distance convention

Curve and contour `Distance` and region `Distance` measure non-negative distance to the relevant
one-dimensional geometry or boundary. Types that implement
<xref:Akeldov.Math.Spatial2D.ISignedPointDistanceProvider> additionally follow this convention:

- negative inside a contour or region;
- zero on its boundary;
- positive outside.

This shared contract lets contours and regions participate in the same signed-distance
rasterizers and geometry-scene layers. `Contains` and `Encloses` answer membership directly when
the numeric distance is not needed.

The optional `geometryEpsilon` accepted by geometric comparisons is measured in world coordinate
units. It controls behavior near tangencies, shared endpoints, and boundaries; choose it in the
same scale as the surrounding coordinates.

## Keep parameterization separate from shape

Parameterization adds a coordinate and traversal direction without changing the underlying
geometric set. Curve coordinates are measured in world units along a curve, not normalized to
`[0, 1]`:

- finite paths and parameterized contours use `[0, Length]`;
- rays use `[0, +infinity)`;
- parameterized infinite lines use signed coordinates.

For a closed parameterized contour, coordinates `0` and `Length` identify the same position.
The chosen origin and direction still matter for animation, sampling, text placement, and any
value that varies along the boundary.

## Preserve geometry structure

Primitive geometry types are generally immutable value types: transforming or converting them
produces new values. Composite contours and contour-based regions copy their input references
into private storage and expose read-only structural views. This prevents callers from changing
the order or number of retained parts after construction.

New mutable collections returned by intersection and sampling operations are caller-owned. See
[Collection Ownership and Immutability](../fundamentals/collection-ownership-and-immutability.md)
for the full collection contract.

## Apply shared coordinate conventions

All linear measurements use the world coordinate unit selected by the caller. Unsuffixed angles
are expressed in radians, with positive rotation counterclockwise from the positive X axis.
Public geometry constructors and queries that require finite coordinates reject `NaN` and
infinity at their boundaries.

Review [Fundamentals](../fundamentals/index.md) for points, vectors, units, and ownership rules
before building larger geometry.

## Continue through the model

- [Curves](curves.md) explains curve capabilities, concrete curve families, projection, and
  parameterization.
- [Contours](contours.md) explains closed boundaries, composite paths, enclosure, and smoothing.
- [Regions](regions.md) explains filled shapes, holes, fill rules, and signed distance.

The geometry model is consumed by [Fields](../fields.md),
[Spatial Algorithms](../spatial-algorithms.md), and [Rasterization](../rasterization.md).
