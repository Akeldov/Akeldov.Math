# Regions

`IRegion` represents filled area membership through `Contains`, unsigned boundary distance through `Distance`, and signed boundary distance through `SignedDistance`.
Negative signed distances are inside the region; positive signed distances are outside.
`IContourBasedRegion` adds contours and a fill rule for regions defined by closed contours.
`ContourBasedRegion` is the built-in contour-backed implementation.

Regions live in the `Akeldov.Math.Spatial2D.Regions` namespace.

Contours describe boundaries. Regions describe area membership and distance to the filled area's boundary.
Use `Contour.Encloses` for a single boundary and `ContourBasedRegion.Contains` for the filled area.

## Topics

- [Rectangle](rectangle.md)
- [OrientedRectangle](oriented-rectangle.md)
- [Contour-Based Regions](contour-based-regions.md)
