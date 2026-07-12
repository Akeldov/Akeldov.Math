# Agent Notes

## Hex Geometry Size

The primary unit for hex geometry size is the radius, measured from the center of a hex to a
vertex. Accept the radius as the source value and derive the apothem from it rather than using
the apothem as the constructor input value. The derived apothem may be stored or cached after
it has been calculated from the radius.
