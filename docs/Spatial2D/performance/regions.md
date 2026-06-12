# Regions

Rectangle and oriented rectangle checks are direct membership tests.

Contour-based regions depend on the number of contours and the number of curves inside each contour.

Region rasterization multiplies region membership and distance work by raster resolution, so grid size is often the dominant cost.
