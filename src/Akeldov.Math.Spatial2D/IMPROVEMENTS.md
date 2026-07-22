# Akeldov.Math.Spatial2D Improvements

This document is a prioritized backlog for reliability, API consistency, performance, and
documentation work. Check an item only after its tests and documentation have been updated.

## High Priority

- [x] **Add compressed PNG output.** Replace the current uncompressed DEFLATE blocks with real
  PNG compression and expose a sensible compression-level option without breaking the existing
  `SaveAsPng` overloads. Verify round trips for every supported color format and benchmark the
  file-size and encoding-time tradeoff on representative rasters.
- [ ] **Add a floating-point signed-distance field.** Provide direct SDF rasterization to a
  `SpatialRaster<float>` so callers can retain signed distances without mapping them to grayscale
  or RGBA values. Support individual providers and provider collections, document the union/minimum
  rule, and test sign, units, bounds, and non-finite input behavior.
- [ ] **Expose Delaunay triangulation as a standalone operation.** Move the triangulation result
  behind a public, independently callable API instead of making it available only as an internal
  step of `DelaunayCuller<TPointSource>`. Define triangle/index ownership, deterministic behavior,
  duplicate and collinear-point handling, and reuse the same implementation in the culler.
- [ ] **Support point-matched TrueType composite glyphs.** Implement composite component
  placement when arguments identify parent and component points instead of an XY offset. Add a
  licensed font fixture that exercises the feature, reject invalid point indexes with a clear
  format error, and preserve the existing composite nesting limit.
- [ ] **Complete the numerical-contract audit.** Apply consistent finite-value validation and
  tolerance handling to public geometry boundaries. Document whether degenerate curves,
  contours, rectangles, sampling distances, and raster bounds are rejected or represented, and
  cover the chosen behavior with focused regression tests.
- [ ] **Extend curve intersection and projection fuzzing.** Add fixed-seed cases for tangencies,
  shared endpoints, collinear overlap, nearly degenerate Bezier curves, very small and very large
  coordinates, and reversed parameterization. Verify symmetry, point-on-curve, and parameter-range
  invariants, reporting the seed and iteration on failure.
- [ ] **Strengthen contour and region invariants.** Test curve connectivity, closure, orientation,
  nested holes, boundary points, and signed-distance consistency across composite, circular, and
  rectangular contours. Add deterministic behavior for ambiguous boundary cases and document it.
- [ ] **Audit angle units across the public API.** Ensure every angle parameter and property says
  that it uses radians, and give every non-radian member an explicit unit suffix such as `Deg`.
  Add API-level checks that prevent undocumented or ambiguous angle units from returning.

## Medium Priority

- [ ] **Finish the collection ownership audit.** Preserve mutable caller-owned results where
  mutation is part of the contract, and use read-only lists for retained state or semantic results
  with invariants. State ownership explicitly in XML comments and add mutation-isolation tests.
- [ ] **Broaden stress coverage for numeric extremes.** Extend the existing explicit stress
  fixtures with near-degenerate geometry, large coordinate magnitudes, dense influence sources,
  complex nested regions, and high-resolution rasterization. Use fixed seeds and actionable
  failure messages.
- [ ] **Extend performance baselines.** Keep the existing Delaunay, barycentric, Voronoi, Poisson
  disk, contour, region, and signed-distance rasterization benchmarks. Add curve intersection,
  Bezier projection, text layout, and geometry-scene composition workloads, tracking allocations
  as well as execution time.
- [ ] **Reduce avoidable hot-path allocations.** Profile before changing APIs, then optimize
  temporary storage in intersection, sampling, partitioning, and rasterization loops. Preserve the
  documented caller-ownership contract and record benchmark evidence for every optimization.
- [ ] **Document underrepresented subsystems.** Add focused guides and runnable examples for
  centroid helpers, scaling, imaging/export constraints, text layout limitations, and coordinate
  conventions. Keep examples synchronized through compile, snapshot, or documentation tests.

## Release Readiness

- [ ] Run focused edge-case tests, all seeded fuzz/property tests, the normal suite, and explicit
  stress fixtures before a release candidate.
- [ ] Compare BenchmarkDotNet results with the last recorded baseline and investigate meaningful
  regressions in time or allocations.
- [ ] Pack the NuGet package and verify XML documentation, README rendering, both target
  frameworks, and a clean consumer build before publishing.
