# Rasterizing a Hex Map

Turn values stored at hex centers into a smooth PNG heat map. You will build one shared sampling
grid, look up the three hexes around every pixel, interpolate their values with barycentric
weights, handle the finite map boundary, and map the result to color.

This tutorial uses a 9×7 `OddR` map and the 8-bit RGBA image types from Akeldov.Math.Spatial2D.
Complete [Creating a Hex Map](../creating-a-hex-map/index.md) first, or start with a .NET 6 or
later console project that references `Akeldov.Math.Hexes`.

The steps build on one another and should be completed in order:

1. [Prepare the topology and source map](preparing-topology-and-source-map.md).
2. [Create an index raster](creating-an-index-raster.md) for spatial lookups.
3. [Handle partial boundary neighborhoods](handling-partial-boundary-neighborhoods.md).
4. [Create a barycentric raster](creating-a-barycentric-raster.md) on the same sampling grid.
5. [Map hex values to pixels](mapping-map-values-to-pixels.md) with interpolation.
6. [Map interpolated values to colors](mapping-values-to-colors.md).
7. [Export the image](exporting-the-image.md) as a PNG.

The result has smooth transitions between hex-center values and a transparent background beyond
the finite map. Start with [Preparing Topology and the Source Map](preparing-topology-and-source-map.md).
