# Regions

Regions represent filled two-dimensional areas. They live in the `Akeldov.Math.Spatial2D.Regions` namespace and provide exact membership through `Contains`, unsigned boundary distance through `Distance`, and signed boundary distance through `SignedDistance`.

Signed distance is negative inside a region, zero on its boundary, and positive outside. `IContourBasedRegion` additionally exposes the contours and fill rule used to define a region.

The thumbnails below show filled-region rasterization with a short falloff outside each boundary.

## [Circular](circular/index.md)

| Region | Shape model | Notes |
|---|---|---|
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Filled disk region raster" src="../../assets/spatial2d/regions/disk-region.png"><br><span class="curve-overview-link">[`Disk`](disk.md)</span> | Center and radius | Circular filled area; converts to a `Circle` boundary. |

## [Rectangular](rectangular/index.md)

| Region | Shape model | Notes |
|---|---|---|
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Filled rectangle region raster" src="../../assets/spatial2d/regions/rectangle-region.png"><br><span class="curve-overview-link">[`Rectangle`](rectangle.md)</span> | Two opposite corners | Axis-aligned filled rectangle with normalized bounds. |
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Filled oriented rectangle region raster" src="../../assets/spatial2d/regions/oriented-rectangle-region.png"><br><span class="curve-overview-link">[`OrientedRectangle`](oriented-rectangle.md)</span> | Center, size, and rotation | Filled rectangle whose local axes may be rotated in world space. |

## [Contour Based](contour-based/index.md)

| Region | Shape model | Notes |
|---|---|---|
| <img class="curve-overview-thumbnail" style="width: 160px; height: 160px; max-width: none; object-fit: contain;" alt="Contour-based region with a hole raster" src="../../assets/spatial2d/regions/contour-based-region.png"><br><span class="curve-overview-link">[`ContourBasedRegion`](contour-based-regions.md)</span> | One or more closed contours | Even-odd filling supports holes and nested areas. |

Use [contours](../contours/index.md) when only a closed boundary is needed. Use regions when area membership or a signed distance field is required.
