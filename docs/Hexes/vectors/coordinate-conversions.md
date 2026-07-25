# Coordinate Conversions

Coordinate conversion helpers connect QRS coordinates, offset-storage indexes, and Spatial2D vectors. Conversions that depend on row or column offsets require a [`Layout`](../layouts.md).

## QRS and XY Indexes

`VectorQRSInt` is the layout-independent hex index. `VectorXYInt` is the corresponding row-and-column storage index:

```csharp
var qrsIndex = new VectorQRSInt(2, -1);

VectorXYInt storageIndex = qrsIndex.ToXYIndex(Layout.OddR);
VectorQRSInt restored = storageIndex.ToQRSIndex(Layout.OddR);
```

Use the same layout in both directions. `OddR` and `EvenR` offset alternating rows; `OddQ` and `EvenQ` offset alternating columns.

## Spatial2D Coordinates

`ToVectorXY` maps a fractional QRS vector onto the continuous axes of a unit-radius hex grid:

```csharp
var qrsOffset = new VectorQRS(1f, -0.5f);

VectorXY pointyTopOffset = qrsOffset.ToVectorXY(Layout.OddR);
VectorXY flatTopOffset = qrsOffset.ToVectorXY(Layout.OddQ);
```

The odd and even variants of one orientation produce the same continuous vector. Their difference matters only for offset-storage indexes.

`ToNormalizedAxial` removes a positive hex-radius scale from an already scaled QRS value:

```csharp
var scaled = new VectorQRS(24f, -12f);
VectorQRS normalized = scaled.ToNormalizedAxial(hexRadius: 12f);
```

## QRS Basis

The QRS axes are separated by 120 degrees. Red is `eQ`, green is `eR`, and blue is `eS`. Valid vectors satisfy:

```text
(Q, R, S) = Q * eQ + R * eR + S * eS
Q + R + S = 0
```

Individual basis axes are not themselves valid `VectorQRS` values because `(1, 0, 0)`, `(0, 1, 0)`, and `(0, 0, 1)` violate the zero-sum invariant. The valid differences `eQ - eS = (1, 0, -1)` and `eR - eS = (0, 1, -1)` define the plane.

### Pointy-top layouts

`OddR` and `EvenR` share the same pointy-top continuous basis:

![QRS basis for OddR and EvenR pointy-top layouts](../../assets/hexes/vectors/qrs-basis-pointy-top.png)

### Flat-top layouts

`OddQ` and `EvenQ` share the same flat-top continuous basis:

![QRS basis for OddQ and EvenQ flat-top layouts](../../assets/hexes/vectors/qrs-basis-flat-top.png)

For a unit-radius hexagon, each component axis has length `1`, while the difference between two axes has length `sqrt(3)`, the center-to-center distance between neighboring hexagons.
