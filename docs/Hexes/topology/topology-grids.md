# Topology Grids

Topology grids sample index relationships into regular grid coordinates.

## Triplet Grids

- `IndexTripletGrid`.
    - Samples vertex triplets as hex indexes.
- `IndexPartialTripletGrid`.
    - Samples vertex triplets with presence flags.

## Septuplet Grids

- `IndexSeptupletGrid`.
    - Samples full neighborhood septuplets.
- `IndexPartialSeptupletGrid`.
    - Samples partial neighborhood septuplets with presence flags.

## Rasterization Support

- Topology grids can feed topology rasterization.
- Raster helpers can map index relationships into image-space samples.
