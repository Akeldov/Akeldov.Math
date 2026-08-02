# Regions

Regions represent filled two-dimensional areas. Spatial2D provides circular and rectangular
regions directly and can interpret one or more closed [contours](contours.md) as a filled area.
Regions answer membership queries and measure unsigned or signed distance to their boundary.

Region types live in the <xref:Akeldov.Math.Spatial2D.Regions> namespace.

## Distinguish a region from its boundary

A contour describes a closed one-dimensional boundary. A region describes the two-dimensional
area selected by that boundary:

| Question | Contour | Region |
|---|---:|---:|
| Is this point enclosed by one boundary? | `Encloses(point)` | — |
| Does this point belong to the filled area? | — | `Contains(point)` |
| What is the distance to the boundary? | `Distance(point)` | `Distance(point)` |
| Is the point inside or outside? | `SignedDistance(point)` | `SignedDistance(point)` |
| Can several boundaries create holes? | No fill semantics | Yes, through `ContourBasedRegion` |

For a single simple shape, contour enclosure and region membership usually select the same
points. The distinction becomes important when several nested boundaries define holes or filled
islands.

## Understand the region interfaces

Spatial2D exposes two region interfaces:

| Interface | Capability |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Regions.IRegion> | Tests filled-area membership with `Contains` and provides unsigned and signed boundary distance. |
| <xref:Akeldov.Math.Spatial2D.Regions.IContourBasedRegion> | Adds a read-only structural view of the defining contours and the rule used to fill them. |

`IRegion` extends <xref:Akeldov.Math.Spatial2D.ISignedPointDistanceProvider>, so regions can be
used by distance-field and rasterization APIs without depending on a concrete shape.

## Choose a concrete region

Choose the most direct representation of the filled shape:

| Type | Shape model | Use when |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Regions.Disk> | Center and non-negative radius | The area is circular. |
| <xref:Akeldov.Math.Spatial2D.Regions.Rectangle> | Two opposite corners, normalized to axis-aligned bounds | The area is an axis-aligned rectangle. |
| <xref:Akeldov.Math.Spatial2D.Regions.OrientedRectangle> | Center, size, and rotation | The rectangle follows a rotated local coordinate frame. |
| <xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion> | One or more arbitrary closed contours | The boundary is composite, contains holes, or has nested filled areas. |

`Disk`, `Rectangle`, and `OrientedRectangle` are immutable value types. They avoid the
indirection of a general contour list and expose shape-specific properties such as radius,
corners, local axes, and size.

## Create standard regions

Create a region directly from the parameters of its shape:

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var disk = new Disk(
    center: new PointXY(0f, 0f),
    radius: 5f);

var rectangle = new Rectangle(
    cornerA: new PointXY(-4f, -2f),
    cornerB: new PointXY(4f, 2f));

var orientedRectangle = new OrientedRectangle(
    center: new PointXY(0f, 0f),
    size: new VectorXY(8f, 3f),
    rotation: MathF.PI / 6f);
```

`Rectangle` accepts its opposite corners in either order and exposes normalized `Min` and `Max`
bounds. `OrientedRectangle` requires positive size components; its rotation is the
counterclockwise angle of the local X axis in radians. See
[Angles and Units](../fundamentals/angles-and-units.md) for the rotation convention.

## Test membership and measure distance

`Contains` includes the boundary. `Distance` is the non-negative distance to that boundary,
including for a point inside the region. `SignedDistance` adds the side of the boundary:

- a negative value represents a point inside;
- zero represents a point on the boundary;
- a positive value represents a point outside.

```csharp
var sample = new PointXY(3f, 0f);

bool contained = disk.Contains(sample);              // true
float distance = disk.Distance(sample);              // 2
float signedDistance = disk.SignedDistance(sample);  // -2

float outside = disk.SignedDistance(new PointXY(7f, 0f)); // 2
```

Use `Distance` when only proximity to the boundary matters. Use `SignedDistance` for signed
distance fields, inside/outside effects, and smooth boundary falloff. Its optional
`geometryEpsilon` is measured in world coordinate units and controls classification close to
the boundary; it does not change the exact `Contains` contract.

## Create a region from contours

`ContourBasedRegion` applies a fill rule to one or more `IContour` instances. A single arbitrary
boundary becomes a filled region without requiring a specialized region type:

```csharp
using Akeldov.Math.Spatial2D.Contours;

var triangleBoundary = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(6f, 0f),
    new PointXY(3f, 4f));

var triangle = new ContourBasedRegion(
    new IContour[] { triangleBoundary });

bool insideTriangle = triangle.Contains(new PointXY(3f, 2f));
```

The constructor copies the contour references into private storage. `Contours` exposes a
read-only structural view of that copy, so the region's contour order and cardinality cannot be
changed through the public contract.

Defining contours should not intersect or touch one another. Use nested, disjoint boundaries for
holes and islands so membership and boundary distance remain unambiguous.

## Define holes with the even-odd rule

Spatial2D currently supports <xref:Akeldov.Math.Spatial2D.Regions.FillRule.EvenOdd>. A point is
inside when it lies inside an odd number of contours. Contour traversal direction does not
affect this result.

Two nested circles therefore create a disk-shaped area with a circular hole:

```csharp
var regionWithHole = new ContourBasedRegion(new IContour[]
{
    new Circle(center: new PointXY(0f, 0f), radius: 5f),
    new Circle(center: new PointXY(0f, 0f), radius: 2f)
});

bool inFilledArea = regionWithHole.Contains(new PointXY(3f, 0f)); // true
bool inHole = regionWithHole.Contains(new PointXY(1f, 0f));       // false

float holeDistance = regionWithHole.SignedDistance(
    new PointXY(1f, 0f)); // Positive: the hole is outside the filled region
```

A third contour nested inside the hole creates another filled island. Each boundary crossing
toggles membership between filled and unfilled space.

## Convert between standard regions and contours

Use `ToContour` when an algorithm needs the boundary of a `Disk`, `Rectangle`, or
`OrientedRectangle`. `Rectangle` and `OrientedRectangle` also provide `ToRegion`, which wraps
their boundary in a `ContourBasedRegion` while preserving the same filled area.

```csharp
Circle diskBoundary = disk.ToContour();
RectangleContour rectangleBoundary = rectangle.ToContour();
ContourBasedRegion contourRegion = rectangle.ToRegion();
```

A general `ContourBasedRegion` does not convert to one contour because its fill can depend on
several boundaries.

## Use regions in spatial output

Regions are suitable sources for signed-distance rasterization and geometry-scene layers. Their
negative-inside convention makes the same region usable for filled coverage, outlines, falloff,
and threshold-based masks.

For practical examples, see:

- [Create a region with holes](../../how-to-guides/regions/create-a-region-with-holes.md)
- [Rasterize a signed distance field](../../how-to-guides/rasterization/rasterize-a-signed-distance-field.md)
- [Rasterization concepts](../rasterization.md)
