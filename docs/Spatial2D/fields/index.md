# Fields

Fields sample values at 2D points.

Influence fields sample values from positioned point or curve sources. They are useful for heat maps, procedural masks, control maps, and other continuous 2D value fields.

An influence field combines two kinds of behavior:

- Sampling defines how selected sources contribute to the final value.
- Culling defines which local subset of sources should be sampled at a point.

## Topics

- [Field Interfaces](field-interfaces.md)
- [Point Influence Fields](point-influence-fields.md)
- [Curve Influence Fields](curve-influence-fields.md)
- [Sampling Strategies](sampling-strategies.md)
- [Source Culling](source-culling.md)

## Clamping and Validation

Influence sources validate weights and values so invalid source sets fail early.

Field implementations can expose `Min` and `Max` values for downstream workflows such as sampling and rasterization.

When composing fields with samplers and cullers, validate:

- source weights are finite and meaningful for the sampler;
- cullers receive enough sources for their algorithm;
- sampled values stay inside the expected field range;
- empty source sets are handled deliberately.
