# Rasterization

Rasterization cost scales with grid resolution and per-cell sampling work.

Signed-distance rasterization over contours and regions can be expensive because each cell may require curve distance and containment checks.

Use lower resolutions for previews and 16-bit output only when the extra precision matters.
