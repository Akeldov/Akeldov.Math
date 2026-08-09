# Curves and Contours

Curve operations are usually local and inexpensive.

Contour operations can become more expensive because they aggregate many bounded curves and often require ray-intersection checks across the full boundary.

Benchmark contour-heavy workloads when:

- contours contain many segments;
- containment checks are performed per raster cell;
- smoothing or filleting creates many additional curves.
