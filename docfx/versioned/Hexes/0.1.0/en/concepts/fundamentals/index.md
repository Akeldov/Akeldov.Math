# Fundamentals

This section explains the coordinate conventions behind Akeldov.Math.Hexes. The key distinction
is between logical hex coordinates, rectangular storage indices, and continuous world-space
coordinates.

## Coordinate Systems

Use <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> for integer hex coordinates and offsets,
and <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> for fractional coordinates used in continuous
calculations. QRS coordinates satisfy `Q + R + S = 0`, so only two components are independent.
The types expose all three components, and their constructors derive or validate `S`.

Use `VectorXYInt` for row-and-column indices in rectangular maps. Its correspondence with a QRS
coordinate depends on <xref:Akeldov.Math.Hexes.Layout>. `PointXY` and `VectorXY` represent
positions and offsets in continuous space; conversions involving geometry also require a hex
radius and a world-space origin.

See [Coordinate Systems](coordinate-systems/index.md) for the roles of QRS, offset, and spatial
coordinates.

## Layouts

<xref:Akeldov.Math.Hexes.Layout> combines hex orientation with the offset rule used by rectangular
storage. `OddR` and `EvenR` use pointy-top hexes and stagger odd or even rows. `OddQ` and `EvenQ`
use flat-top hexes and stagger odd or even columns.

The odd and even variants of one orientation share the same continuous QRS axes but produce
different row-and-column indices. Use one layout consistently when converting coordinates and
when creating topology, maps, or geometry.

See [Layouts](layouts.md) for the orientation and offset conventions.

## Coordinate Discretization

Keep coordinates fractional until an operation needs a specific cell. `ToQRSIndex(Layout)` rounds
a <xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> to the nearest valid
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> while preserving the QRS invariant. A
world-space `PointXY` can be mapped directly to the offset index of its containing hex with
`ToXYIndex(hexRadius, hexFieldOrigin, layout)`.

An explicit conversion from `VectorQRS` to `VectorQRSInt` truncates `Q` and `R` toward zero; it
does not perform nearest-hex rounding.

See [Coordinate Discretization](coordinate-discretization.md) for fractional QRS rounding and
world-space point conversion.

## Rotations and Transformations

<xref:Akeldov.Math.Hexes.SixfoldAngle> represents counterclockwise rotations in 60-degree steps.
These rotations preserve the integer hex lattice, so rotating a `VectorQRSInt` by a
`SixfoldAngle` returns another integer QRS coordinate. Arbitrary-angle QRS rotations use radians
and return fractional coordinates. Spatial-vector helpers also support scaling, rotation, and
translation.

See [Rotations and Transformations](rotations-and-transformations.md) for the available rotation
and affine transformation operations.

Continue to the [Hex Grid Model](../hex-grid-model/index.md) to see how these conventions are used
by topology, geometry, and polyhex APIs.
