# Influence Field Culling

Culling can reduce the number of sources passed to a sampler, but it has its own setup and query costs.

`DelaunayCuller<TPointSource>` builds triangulation up front, then checks triangles for sampled points.
It is intended for moderate source counts; benchmark very large source sets before relying on it in a hot path.
