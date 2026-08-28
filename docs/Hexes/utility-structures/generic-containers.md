# Generic Containers

Generic containers group related values used by topology, geometry, and chromatization APIs.

## Pairs and Triplets

- `Pair<T>`.
    - Stores two related values.
- `Triplet<T>`.
    - Stores a main value with left and right values.
- `PartialTriplet<T>`.
    - Stores a triplet with presence flags.

## Sextuplets and Septuplets

- `Sextuplet<T>`.
    - Stores the six values ordered by adjacent hex-edge position, without a main value.
- `PartialSextuplet<T>`.
    - Stores six adjacent values with presence flags.
- `Septuplet<T>`.
    - Stores a main value and six adjacent values.
- `PartialSeptuplet<T>`.
    - Stores a septuplet with presence flags.

## Usage

- Triplets model vertex-neighborhood data.
- Sextuplets model neighbor-only data.
- Septuplets model center-plus-neighbor data.
- Partial containers preserve boundary information.
