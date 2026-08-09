# Curve Influence Fields

Curve influence fields sample values from curve sources.

Use `FloatCurveInfluenceSource` when influence should be attached to a parameterized curve rather than a single point.

Curve sources are useful for road-like, river-like, boundary-like, and path-like value fields where the nearest point on a curve drives the result.

The source curve coordinate is measured in world coordinate units along the curve.
