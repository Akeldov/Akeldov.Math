# Spatial Coordinates

Spatial coordinates place the logical hex lattice in the continuous Cartesian space provided by
Akeldov.Math.Spatial2D. Hex APIs use `VectorXY` for world-space offsets, centers, origins, sizes,
and vertices. They use `PointXY` for positions that are sampled or tested for their containing hex.

Keep these values separate from `VectorXYInt` row-and-column indices. A pair of integer storage
coordinates does not describe a physical position until a layout, hex radius, and origin are
known.

## World-space axes and units

Spatial2D uses a positive X axis to the right and a positive Y axis upward. Akeldov.Math.Hexes
does not prescribe a concrete unit: coordinates can represent pixels, metres, game units, or any
other consistent world unit.

The hex radius is the distance from its center to a vertex. The apothem is the distance from the
center to an edge:

```text
apothem = radius * sqrt(3) / 2
```

Neighboring centers are `sqrt(3) * radius` units apart. APIs that accept a radius require it to
be finite and greater than zero.

## Map QRS onto Cartesian axes

The world-space QRS basis depends on the layout orientation. Odd and even variants of one
orientation use the same continuous basis:

| Layouts | Orientation | Offset X | Offset Y |
|---|---|---|---|
| `OddR`, `EvenR` | Pointy-top | `sqrt(3) * radius * (Q + R / 2)` | `3 * radius * R / 2` |
| `OddQ`, `EvenQ` | Flat-top | `3 * radius * Q / 2` | `sqrt(3) * radius * (R + Q / 2)` |

The formulas produce an offset from the center of the zero hex. Add the chosen origin to obtain a
world-space center.

Use `GetHexOffset(hexRadius, layout)` for an integer QRS coordinate:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
float radius = 10f;
var qrsIndex = new VectorQRSInt(q: 2, r: -1);

VectorXY offset = qrsIndex.GetHexOffset(radius, layout);
var origin = new VectorXY(100f, 50f);
VectorXY center = origin + offset;
```

`GetHexOffset` does not add an origin. This makes it suitable for composing translations or
placing the same logical grid at different world-space locations.

## Convert fractional QRS and XY vectors

`ToVectorXY(Layout)` maps a fractional
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> to a `VectorXY` using a unit-radius hex basis.
`ToVectorQRS(Layout)` performs the inverse:

```csharp
Layout layout = Layout.OddR;
const float radius = 10f;
var fractionalQrs = new VectorQRS(q: 1.5f, r: -0.25f);

VectorXY unitRadiusOffset = fractionalQrs.ToVectorXY(layout);
VectorQRS restored = unitRadiusOffset.ToVectorQRS(layout);
VectorXY physicalOffset = unitRadiusOffset * radius;
```

Use the same orientation in both calls. A floating-point round trip can differ by a small
rounding error, so compare calculated values with an application-appropriate tolerance.

`ToNormalizedAxial(hexRadius)` has a different purpose: it divides QRS coordinates that are already
expressed in radius-scaled QRS units by the radius. It does not rotate axes or convert an XY vector:

```csharp
var scaledQrs = new VectorQRS(15f, -2.5f);
VectorQRS normalized = scaledQrs.ToNormalizedAxial(10f);

// normalized is (1.5, -0.25, -1.25)
```

## Get a center from a storage index

`VectorXYInt.GetHexCenter` combines the offset layout, radius, and zero-hex origin:

```csharp
Layout layout = Layout.OddR;
const float radius = 10f;
var origin = new VectorXY(100f, 50f);
var qrsIndex = new VectorQRSInt(q: 2, r: -1);

VectorXY center = origin + qrsIndex.GetHexOffset(radius, layout);
VectorXYInt index = qrsIndex.ToXYIndex(layout);
VectorXY sameCenter = index.GetHexCenter(
    radius,
    origin,
    layout);
```

`sameCenter` equals `center` apart from possible floating-point rounding. The origin parameter is
the world-space center of storage index `(0, 0)`, which is also the zero QRS hex when the same
layout is used.

An overload without an origin uses the library's default zero-hex center:

| Orientation | Default origin |
|---|---|
| Pointy-top | `(apothem, radius)` |
| Flat-top | `(radius, apothem)` |

The static QRS-based `GetHexCenter(q, r, hexRadius, layout)` helper uses the same defaults.
Specify an origin explicitly when a map must align with an existing world coordinate system.

## Keep topology and geometry together

<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> packages a
<xref:Akeldov.Math.Hexes.HexMapTopology> with its origin, radius, and derived apothem. Use it when
multiple operations must agree on storage dimensions and physical placement:

```csharp
var topology = new HexMapTopology(
    width: 8,
    height: 6,
    layout: Layout.OddR);
var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 10f);

VectorXY mapOrigin = geometry.Origin;
float mapRadius = geometry.Radius;
float mapApothem = geometry.Apothem;
```

The geometry constructor requires a finite origin and a finite positive radius. Keeping one
`HexMapGeometry` value avoids accidental combinations of a topology from one map with the origin
or radius of another.

See [Coordinate Discretization](../coordinate-discretization.md) to map a world-space `PointXY`
to its containing hex, and [Hex Grid Geometry](../../hex-grid-model/geometry.md) for centers,
vertices, and map bounds.
