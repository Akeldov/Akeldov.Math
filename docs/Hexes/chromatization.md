# Chromatization

Chromatization APIs assign stable chromatic classes to hex indexes and sampled vertex neighborhoods.

## Chromatic Indexes

- Chromatic class calculation for hex indexes.
- Layout-aware chromatic values.
- Vertex triplet chromatization.

## Chromatic Maps

- `ChromaticIndexMap`.
    - Maps hex indexes to chromatic classes.
    - Implements the common hex map contract.

## Chromatic Grids

- `ChromaticIndexTripletGrid`.
    - Samples chromatic index triplets.
- `ChromaticIndexPartialTripletGrid`.
    - Samples chromatic index triplets with presence flags.

## Rasterization

- RGBA16 rasterization for chromatic data.
- Color mapping from chromatic values.
- Layout-aware raster output for chromatic maps and grids.
