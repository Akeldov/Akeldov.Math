# Noise

A noise field generates values from world coordinates and a seed. Nearby points vary smoothly,
which makes noise useful for terrain height and temperature maps. Sampling the same position
with the same settings reproduces the same value, even when you sample points in a different order.

[PerlinNoiseField](perlin-noise-field.md) combines layers of Perlin noise. Use it to sample
individual positions or pass it to an algorithm that accepts an <xref:Akeldov.Math.Spatial2D.Fields.IFloatField>.
