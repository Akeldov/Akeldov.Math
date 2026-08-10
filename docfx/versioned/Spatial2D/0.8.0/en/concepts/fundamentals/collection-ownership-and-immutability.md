# Collection Ownership and Immutability

Collection contracts in Akeldov.Math.Spatial2D answer two separate questions: what the library
does with a collection passed into an API, and what ownership a caller receives when a
collection is returned. The collection type provides an initial signal; conceptual and member
documentation state stronger guarantees where they apply.

## Two groups of situations

| Direction | Main question | Possible contracts |
|---|---|---|
| Collection input | Does the library keep anything after the call? | Use only, retain a copy, or retain the original reference |
| Collection output | What does the returned collection represent? | A new caller-owned result, a read-only library-owned surface, or direct mutable access to existing state |

The input and output contracts are independent. For example, an API can copy an input
collection into its state and later expose that copy through `IReadOnlyList<T>`.

## Collection input

Passing a collection into a method or constructor does not by itself transfer ownership. An
input follows one of the three patterns below, but its parameter type alone does not identify
which pattern applies.

### Used only during the call

The library reads or enumerates the collection to perform the operation and does not retain the
collection after the call returns. The library does not acquire ownership, and the caller may
reuse or modify the collection afterward.

Do not modify a borrowed input while the operation is still running unless the API explicitly
supports concurrent mutation. When documentation states that an input is used only for the
operation or is not retained, the caller can rely on that lifetime guarantee.

### Copied into library-owned state

The library creates a structural copy and retains that copy as its own state. The original
collection remains caller-owned, and later structural changes to it do not affect the library's
copy.

<xref:Akeldov.Math.Spatial2D.Curves.BezierCurve> follows this contract for control points:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var input = new[]
{
    new PointXY(0f, 0f),
    new PointXY(1f, 2f),
    new PointXY(3f, 0f)
};

var curve = new BezierCurve(input);
input[1] = new PointXY(100f, 100f);

PointXY retained = curve.ControlPoints[1]; // Still (1, 2)
```

This is ownership of the copy, not ownership of the original collection. Unless documented
otherwise, a structural copy is shallow: reference-type elements can still be shared.

### Original reference retained: aggregation

The library stores a reference to the collection supplied by the caller. No structural copy is
made, so the caller and library share the same collection. Changes through either reference are
visible through the other.

`Raster<TValue>` aggregates the value array passed to its constructor:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var values = new[]
{
    1, 2,
    3, 4
};

var raster = new Raster<int>(new VectorXYInt(2, 2), values);

values[0] = 10;
int throughRaster = raster[0, 0]; // 10
```

Aggregation avoids a copy, but it couples lifetime and mutation. The caller must not make
changes that violate the documented invariants and must coordinate concurrent access. The API
documentation explicitly says that the input is retained as state.

The parameter type alone does not distinguish these cases. An `IReadOnlyList<T>` input can be
borrowed, copied, or retained as the same object.

## Collection output

A returned collection has one of three ownership contracts. Its public type is an important
signal; consult the member documentation for details that the type cannot express.

### New caller-owned result

A new array or `List<T>` returned as a computation result is `CallerOwned`. The library does not
retain that collection, and the caller may filter, append to, reorder, or reuse it. XML
documentation states that the result is new, mutable, and owned by the caller.

Ray intersections are one example:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var circle = new Circle(new PointXY(0f, 0f), 5f);
var ray = new Ray(new PointXY(-10f, 0f));

List<PointXY> intersections = circle.GetRayIntersections(ray);

intersections.RemoveAll(point => point.X < 0f);
intersections.Add(new PointXY(20f, 0f));
```

Changing this list does not modify the circle or later intersection results. Caller ownership
applies to the collection structure; it does not imply a deep copy of reference-type elements.

### Read-only library-owned surface

`IReadOnlyCollection<T>` and `IReadOnlyList<T>` indicate a `LibraryOwned` output contract. The
collection represents library state or a semantic result whose structure the caller should not
change through the returned reference.

There are two important underlying cases:

- the library copied the original collection and returns a read-only surface over its copy, as
  with `BezierCurve.ControlPoints`;
- the library retained the original collection or returns an existing collection as-is through
  a read-only interface, so another reference may still be able to change it.

Both cases look read-only at the output boundary. The member documentation states whether the
underlying collection is a copy or the original. `LibraryOwned` describes the public mutation
contract, not who allocated the backing object or whether the library has exclusive access to
it.

Do not cast a read-only result to a mutable runtime type and modify it. The concrete type is not
part of the contract, and bypassing the interface can violate result or state invariants.

### Direct mutable access to existing state

In rare performance-oriented APIs, a property returns an existing `List<T>` or array directly.
This is not a new caller-owned result: it is shared mutable access to library state. The member
documentation should make the retained, shared nature of the collection clear.

`Raster<TValue>.Values` returns the retained array itself:

```csharp
raster.Values[3] = 40;
int throughIndexer = raster[1, 1]; // 40
```

This contract is appropriate when changing elements is intentional and does not invalidate the
collection's structural invariants. A raster array cannot change its length, so callers can
replace cell values without breaking the relationship between the resolution and cell count.
The benefit is direct bulk access without another full-raster copy; the cost is shared mutable
state and caller-managed synchronization.

## Read-only is not immutable

`IReadOnlyCollection<T>` and `IReadOnlyList<T>` prevent mutation only through those interfaces.
They do not guarantee:

- an immutable backing collection;
- a defensive copy;
- the absence of another mutable reference;
- immutable reference-type elements;
- a deep copy of element state.

Treat collection structure and element state as separate concerns. A library-owned list can
retain a stable count and order while properties of a mutable element change.

Core coordinate and geometry values such as `PointXY` and `VectorXY` are commonly immutable
value types. Assigning them copies the value, but placing them in a read-only collection still
adds a separate contract for adding, removing, replacing, or reordering elements.

## Make a copy when isolation is required

Create a copy at your own boundary when:

- a borrowed or aggregated input must not observe later changes;
- a read-only output is needed as an independent snapshot;
- direct mutable state must be edited without affecting the library;
- element state also requires isolation from shared reference-type objects.

For an `IReadOnlyCollection<T>`, use a suitable collection constructor or LINQ operation. For
an array, use `Clone` or `Array.Copy`. These operations make shallow copies; copy the elements
as well when deep isolation is required.

Use the guarantees explicitly stated for a member. Look for phrases such as "used only during
the call," "copied into retained state," "retained as state," "new mutable result owned by the
caller," "read-only view of copied state," and "direct access to retained mutable state." If
the documentation does not say whether an input is copied or retained, do not infer that
behavior from the collection type alone.
