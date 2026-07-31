# Collection Ownership and Immutability

Akeldov.Math.Spatial2D uses collection types and XML documentation to communicate who may mutate
a collection and whether the library keeps it as state. Value types such as `PointXY`,
`VectorXY`, and most small geometry primitives are immutable, but collections and the objects
stored in them can have separate mutability rules.

## Two ownership types

Computed collection results and read-only structural surfaces use two ownership contracts:

| Ownership type | Public shape | Contract |
|---|---|---|
| Caller Ownership | A newly returned `List<T>` or array | The caller receives a new mutable collection that the library does not retain |
| Library Ownership | A returned `IReadOnlyList<T>` | The library preserves structural state or semantic invariants; the caller receives read-only access |

These contracts describe the collection returned through the API. Whether a constructor or
method copies or retains an input collection is a separate question described later on this
page. An API that explicitly exposes retained mutable storage, such as `Raster<TValue>.Values`,
shares existing state rather than returning a new result or transferring ownership.

## Caller Ownership

Spatial2D returns `List<T>` or arrays for newly computed transient results that it does not
retain. Their XML documentation states that the collection is new, mutable, and owned by the
caller.

Ray intersections are one example:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(new PointXY(0f, 0f), 5f);
var ray = new Ray(new PointXY(-10f, 0f));

List<PointXY> intersections = circle.GetRayIntersections(ray);

intersections.RemoveAll(point => point.X < 0f);
intersections.Add(new PointXY(20f, 0f));
```

Changing this list does not modify the circle or affect later intersection calls. The same
caller-ownership pattern is used for values such as culling results, Poisson disk samples,
flattened curve segments, scaled item copies, and derived Voronoi site arrays.

Caller ownership applies to the collection structure. Unless an API explicitly promises a deep
copy, reference-type elements can still refer to objects shared with the input or library state.

## Library Ownership

Library Ownership means that the library keeps control of structural mutation through its public
contract. `IReadOnlyList<T>` is used when order, cardinality, adjacency, or another invariant
forms part of retained state or of a semantic algorithm result. For example, the control-point
sequence determines a Bezier curve:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Curves;

var input = new[]
{
    new PointXY(0f, 0f),
    new PointXY(1f, 2f),
    new PointXY(3f, 0f)
};

var curve = new BezierCurve(input);
IReadOnlyList<PointXY> controlPoints = curve.ControlPoints;

PointXY middle = controlPoints[1]; // (1, 2)
```

<xref:Akeldov.Math.Spatial2D.Curves.BezierCurve> copies the input points and exposes a read-only
structural view of that copy. Replacing an element in `input` after construction does not alter
the curve.

Contour curve lists, region contours, partition items, influence sources, and distinct field
values use similar read-only surfaces when their structure belongs to retained state or a
semantic algorithm result.

Library Ownership describes the returned API surface rather than who originally allocated the
underlying collection. A validation operation may return its input as-is without granting
structural mutation through the returned `IReadOnlyList<T>` contract.

Do not cast a returned `IReadOnlyList<T>` to a mutable implementation and modify it. The concrete
runtime type is not part of the contract, and bypassing the read-only surface can break
invariants even if a cast happens to succeed.

### Read-only is not deeply immutable

`IReadOnlyList<T>` prevents mutation through that interface. It does not by itself guarantee any
of the following:

- that the underlying collection is an immutable collection type;
- that the library made a defensive copy;
- that another reference cannot change the backing collection;
- that reference-type elements are themselves immutable;
- that a structural copy recursively clones its elements.

Spatial2D documents the stronger guarantee when it exists. For example, `BezierCurve` describes
its control points as copied state, while a validation helper can return its input list as-is
because validation does not transfer ownership.

Treat collection immutability and element immutability as separate questions. A stable
read-only list of mutable objects can retain its order and count while an object's properties
change.

## Copied and retained inputs

Input handling is separate from the two return ownership types. Some APIs, such as
`BezierCurve`, copy an input collection before retaining state. The caller may then change the
original collection without affecting the created object.

Other APIs intentionally share mutable storage. `Raster<TValue>` retains the array supplied to
its constructor and exposes it through `Values`. The array and raster indexers address the same
cells:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var values = new[]
{
    1, 2,
    3, 4
};

var raster = new Raster<int>(new VectorXYInt(2, 2), values);

values[0] = 10;
int throughRaster = raster[0, 0]; // 10

raster[1, 1] = 40;
int throughArray = values[3]; // 40
```

This sharing avoids an extra full-raster copy and allows direct bulk access. It also means the
caller must coordinate mutations. The raster does not synchronize access, and a reference
obtained from `Values` is not a snapshot.

Copy the array before construction when the raster must be isolated from subsequent changes to
the source:

```csharp
var isolatedRaster = new Raster<int>(
    new VectorXYInt(2, 2),
    (int[])values.Clone());
```

An `IReadOnlyList<T>` parameter does not by itself say whether the input will be copied, retained,
or used only during the call. Read the parameter and property documentation when later mutation
matters.

## Understand immutable value types

Core coordinate and geometry values are commonly declared as `readonly struct`. Their public
state cannot change after construction, and assignments copy the value:

```csharp
var original = new VectorXY(2f, 3f);
VectorXY translated = original + new VectorXY(1f, -1f);

// original is still (2, 3); translated is (3, 2).
```

Operations return new values instead of mutating the receiver. This is different from a
read-only collection: a value such as `VectorXY` is itself immutable, while
`IReadOnlyList<VectorXY>` additionally controls whether vectors can be added, removed, replaced,
or reordered through that collection reference.

A `readonly struct` can still contain a reference to a mutable object, so `readonly` is not a
general promise of deep immutability. Use the documented contract of the specific type.

## Choose when to make your own copy

Make a copy at your boundary when:

- you need a snapshot that cannot reflect later changes elsewhere;
- you want to mutate a read-only result without affecting its invariants;
- you pass mutable storage to an API that documents retaining it but require isolation;
- you need ownership that survives independently of another component's lifetime or policy.

For an `IReadOnlyList<T>`, a shallow mutable copy can be created with `new List<T>(source)`.
For an array, use `Clone`, `Array.Copy`, or another explicit copy operation. If `T` is a mutable
reference type and isolation must include element state, perform an appropriate deep copy as
well.

The API reference is the authority for each member. Look for phrases such as "new mutable list
owned by the caller," "read-only structural view," "copied into retained state," "retained as
state," and "returned as-is."
