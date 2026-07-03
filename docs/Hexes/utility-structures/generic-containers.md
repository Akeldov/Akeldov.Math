# Generic Containers

Generic containers group related values used by topology, geometry, and chromatization APIs.

## Pairs and Triplets

- `Pair<T>`.
    - Stores two related values.
- `Triplet<T>`.
    - Stores a main value with left and right values.
- `PartialTriplet<T>`.
    - Stores a triplet with presence flags.

## Septuplets

- `Septuplet<T>`.
    - Stores a main value and six adjacent values.
- `PartialSeptuplet<T>`.
    - Stores a septuplet with presence flags.

## Usage

- Triplets model vertex-neighborhood data.
- Septuplets model center-plus-neighbor data.
- Partial containers preserve boundary information.
