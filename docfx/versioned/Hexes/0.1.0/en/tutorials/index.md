# Tutorials

Tutorials introduce Hexes through sequential practical projects. Follow a tutorial from the first
page to the last to create a hex map, find a path, build a polyhex, or prepare a map for
rasterization. The chromatization tutorial is a shorter, self-contained exercise that partitions
a map into three stable classes.

[Complete and Partial Index Maps](creating-index-neighborhood-maps.md) provides two standalone
examples that visualize neighbor indices and compare boundary handling.

For pixel-based triplets, compare [IndexTripletRaster](visualizing-index-triplet-raster.md) and
[IndexPartialTripletRaster](visualizing-index-partial-triplet-raster.md): both examples encode
the three selected hex indices in RGB channels.

[Visualizing BarycentricTripletRaster](visualizing-barycentric-triplet-raster.md) displays
the main interpolation weight in grayscale and explains how weights differ from indices.
Compare it with [BarycentricPartialTripletRaster](visualizing-barycentric-partial-triplet-raster.md)
to see the effect of presence flags at finite-map boundaries.
