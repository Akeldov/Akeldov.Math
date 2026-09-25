# Fundamentals

This section explains the basic conventions used throughout Akeldov.Math.Spatial2D.

## Coordinate System

Use `PointXY` for positions, `VectorXY` for floating-point vectors, and `VectorXYInt` for
integer vectors. Like `VectorXY`, `VectorXYInt` can represent directions, offsets, and sizes;
APIs also use it for values such as raster indices and resolutions.

See [Coordinate System](coordinate-system/index.md) for Cartesian axes and the relationship
between continuous coordinates and discrete indices. The [Points](../geometry-model/points.md)
and [Vectors](../geometry-model/vectors.md) pages in Geometry Model describe the types and their
operations.

## Angles and Units

Spatial2D angles are expressed in radians by default. Distances, sizes, and radii use the same
world unit as the geometry around them.

See [Angles and Units](angles-and-units.md) for rotation direction, degree conversion, and unit
rules.

## Collections and Immutability

A mutable list or array returned as a new result belongs to the caller. An `IReadOnlyList<T>` is
used when the library needs to preserve collection structure or result invariants.

See [Collection Ownership and Immutability](collection-ownership-and-immutability.md) for input
copying, retained storage, and immutable value types.

Continue to the [Geometry Model](../geometry-model/index.md) after reviewing the topics relevant
to your code.
