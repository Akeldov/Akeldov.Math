# Create a Region with Holes

Use `ContourBasedRegion` when a filled area is bounded by more than one closed contour. With the
even-odd fill rule, an outer contour creates the filled area and each nested contour toggles that
area between filled and empty.

## Define the boundaries

Create the outer boundary and the hole as separate `IContour` instances. Contours can use
different concrete types; this example places a circular hole inside a rectangular composite
contour:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Regions;

var outerBoundary = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(12f, 0f),
    new PointXY(12f, 8f),
    new PointXY(0f, 8f));

var holeBoundary = new Circle(
    center: new PointXY(6f, 4f),
    radius: 1.5f);
```

List polygon vertices in boundary order. `CompositeContour` connects the last vertex back to the
first, so the first vertex does not need to be repeated.

## Apply the even-odd fill rule

Pass both contours to `ContourBasedRegion`:

```csharp
var region = new ContourBasedRegion(
    new IContour[] { outerBoundary, holeBoundary },
    FillRule.EvenOdd);
```

`EvenOdd` is the default and currently the only supported fill rule, so its argument can be
omitted. A point belongs to the region when it is enclosed by an odd number of contours. Contour
order and traversal direction do not affect the result.

## Check the result

Use `Contains` to distinguish the filled area from the hole and the exterior:

```csharp
bool inFilledArea = region.Contains(new PointXY(2f, 4f)); // true
bool inHole = region.Contains(new PointXY(6f, 4f));       // false
bool outside = region.Contains(new PointXY(14f, 4f));     // false
```

`Distance` returns the shortest distance to any defining contour. `SignedDistance` makes that
distance negative in the filled area and positive in a hole or outside the outer boundary:

```csharp
float filledDistance = region.SignedDistance(new PointXY(2f, 4f)); // -2
float holeDistance = region.SignedDistance(new PointXY(6f, 4f));   // 1.5
float boundaryDistance =
    region.SignedDistance(new PointXY(6f, 5.5f));                   // 0
```

This sign convention allows the same region to be used directly by signed-distance
rasterization and other `ISignedPointDistanceProvider` consumers.

## Add more holes or filled islands

Add one contour for each additional hole. If a contour is nested inside a hole, it toggles the
fill again and creates a filled island. Deeper nesting continues to alternate between filled and
empty areas at every boundary crossing.

Keep the contours nested or disjoint. The constructor accepts intersecting or touching contours,
but those inputs can make the intended boundary and distance behavior ambiguous. Every contour
must be non-null, and the collection must contain at least one contour.

The constructor copies the contour references into private storage. Its `Contours` property is
a read-only structural view, so callers cannot add, remove, or reorder the region's boundaries
through the public contract.

For more background, see [Regions](../../concepts/geometry-model/regions.md) and
[Contours](../../concepts/geometry-model/contours.md). To turn the result into an image, continue
with [Rasterize a signed-distance field](../rasterization/rasterize-a-signed-distance-field.md).
