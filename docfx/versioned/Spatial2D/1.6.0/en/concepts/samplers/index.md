# Samplers

Point samplers create new positions for procedural generation, test data, and initial partition
sites. Choose a sampler based on the number of points or the spacing your application needs.

## Available samplers

- [Random Point Sampler](random-point.md) — a fixed number of independent, uniformly distributed points.
- [Poisson Disk Sampler](poisson-disk.md) — irregular point sets with either a constant minimum
  spacing or spacing supplied by a field.

Point generation is different from sampling a [field](../fields.md): a field evaluates a value
at an existing query point, while a point sampler creates the positions themselves.
