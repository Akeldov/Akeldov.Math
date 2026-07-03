# Field Parameters

Field parameter helpers reconstruct and validate hex geometry settings.

## Reconstruction

- Reconstruct apothem from field dimensions.
- Reconstruct radius from apothem.
- Reconstruct integer dimensions when possible.

## Parameters

- Hex radius.
- Hex apothem.
- Hex field origin.
- Raster or geometry dimensions.

## Validation

- Reject non-finite coordinates.
- Reject non-positive radius or apothem values.
- Detect integer overflow during dimension reconstruction.
