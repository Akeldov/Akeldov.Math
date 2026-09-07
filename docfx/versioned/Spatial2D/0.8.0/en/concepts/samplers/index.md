# Samplers

Point samplers create new positions for procedural generation, test data, and initial partition
sites. Their parameters control the generation bounds and spacing between points.

## Available samplers

- [Poisson Disk Sampler](poisson-disk.md) — irregular point sets with either a constant minimum
  spacing or spacing supplied by a field.

Point generation is different from sampling a [field](../fields.md): a field evaluates a value
at an existing query point, while a point sampler creates the positions themselves.
