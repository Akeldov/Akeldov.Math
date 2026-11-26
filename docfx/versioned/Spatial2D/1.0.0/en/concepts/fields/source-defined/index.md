# Source-Defined Fields

To define one of these fields, supply the sources and a sampler. When you call `Sample(point)`, a point source supplies its value and the distance to the query. A curve source uses the projection of the query onto its curve. The sampler selects one source value or combines several.

| Field | Source | Result | Example |
|---|---|---|---|
| [FloatPointInfluenceField](float-point-influence-field.md) | Point | `float` | Temperature from sensor readings |
| [IntPointInfluenceField](int-point-influence-field.md) | Point | `int` | District ID from the nearest marker |
| [BoolPointInfluenceField](bool-point-influence-field.md) | Point | `bool` | Whether the nearest gate is open |
| [FloatCurveInfluenceField](float-curve-influence-field.md) | Curve | `float` | Road elevation by distance along the road |

See [Sampling Strategies](../sampling-strategies.md) for ways to combine source values and [Source Indexing](../source-indexing.md) for selecting a local group of sources.
