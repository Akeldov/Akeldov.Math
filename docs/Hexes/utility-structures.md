# Utility Structures

Utility structures carry repeated grouped values used by topology, geometry, grids, and chromatization APIs.

## Generic Containers

- `Pair<T>`.
    - Stores two related values.
- `Triplet<T>`.
    - Stores a main value with left and right values.
- `PartialTriplet<T>`.
    - Stores a triplet with presence flags.
- `Septuplet<T>`.
    - Stores a main value and six adjacent values.
- `PartialSeptuplet<T>`.
    - Stores a septuplet with presence flags.

## Presence Flags

- `TripletPresenceFlags`.
    - Describes which triplet positions are present.
- `SeptupletPresenceFlags`.
    - Describes which septuplet positions are present.

## Sixfold Angles

- `SixfoldAngle`.
    - Represents one of the six 60-degree hex-grid directions.
- `SixfoldAngles`.
    - Provides ordered angle values.
- Sixfold angle helpers.
    - Sine and cosine lookup.
    - Radian and degree conversion.
    - 60, 120, 180, 240, and 300 degree rotation helpers.

## Binary Helpers

- Binary reader extension helpers.
- Binary writer extension helpers.
- Shared serialization support for hex-grid utility values.
