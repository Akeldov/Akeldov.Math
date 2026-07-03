# Discretization

Discretization rounds fractional coordinates into stable hex indexes.

## Fractional QRS

- Fractional QRS coordinates can be rounded to the nearest hex.
- Rounding preserves the QRS invariant.
- Large component values are checked before integer conversion.

## Point Sampling

- Spatial2D points can be converted to the containing or nearest hex index.
- Hex radius and origin are part of the conversion.
- Invalid radius, non-finite points, and unsupported layouts are rejected.

## Offset Separation

- QRS rounding does not bake odd/even row or column offsets into QRS values.
- Layout offsets are applied only when converting between QRS and XY indexes.
