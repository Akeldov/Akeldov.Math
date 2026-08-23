# Influence Field Culling

Culling can reduce the number of sources passed to a sampler, but it has its own setup and query costs.

`DelaunayInfluenceSourceIndex<TPointSource>` builds its immutable source snapshot, triangulation, and spatial lookup structure up front, then returns selected sources for sampled points.
It is intended for moderate source counts; benchmark very large source sets before relying on it in a hot path.
