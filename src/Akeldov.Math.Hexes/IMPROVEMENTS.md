# Akeldov.Math.Hexes Improvements

This document is a prioritized backlog for reliability, API consistency, performance, and
documentation work. Check an item only after its tests and documentation have been updated.

## High Priority

- [ ] **Audit public boundary validation.** Apply consistent checks for positive finite hex
  radii, valid map dimensions and indexes, supported layouts, finite coordinates, and compatible
  topology/geometry in operations that combine maps. Use consistent exception types and cover
  every rejected input with focused tests.
- [ ] **Add seeded property tests for coordinate and layout operations.** Exercise all four
  layouts and verify QRS invariants, QRS/offset/world-coordinate round trips, rotations,
  discretization near cell boundaries, and neighbor symmetry. Every failure must report its seed
  and iteration.
- [ ] **Verify pathfinding against a reference implementation.** Compare small randomized maps
  with a simple Dijkstra oracle and cover unreachable targets, identical start and end cells,
  infinite transfer costs, equal-cost alternatives, and invalid negative or non-finite costs.
  Define and test deterministic tie behavior where the public result exposes a chosen path.
- [ ] **Harden polyhex boundary generation.** Add regression and property tests for isolated
  cells, holes, disconnected masks, narrow bridges, concave boundaries, and all supported layouts.
  Verify that generated contours are closed, have consistent orientation, and produce the same
  occupied area as the source mask.
- [ ] **Define the binary compatibility contract.** Document byte order, field order, and version
  policy for QRS and polyhex serialization. Add golden-file round-trip tests plus tests for
  truncated, malformed, and unsupported data before treating the format as stable.

## Medium Priority

- [ ] **Add explicit stress suites.** Cover large pathfinding maps, Voronoi partitions, polyhex
  contours, and rasterization workloads with fixed seeds. Keep stress fixtures out of normal test
  runs and include the seed and scenario in failure messages.
- [ ] **Expand benchmark coverage.** Retain the existing geometry-map, adjacency-map, and
  rasterization benchmarks, then add representative pathfinding, Voronoi, polyhex, and map
  operation cases. Record throughput and allocation baselines for small and large inputs.
- [ ] **Audit collection ownership and mutability.** Make it clear which map, partition, path,
  raster, and polyhex collections are retained views and which are new caller-owned results.
  Align return types with those contracts and document the behavior in XML comments.
- [ ] **Complete public API documentation.** Document coordinate systems, layout assumptions,
  radius-based geometry sizing, boundary behavior, complexity where it is non-obvious, and all
  thrown exceptions. Enable a release check that prevents new public members with missing XML
  documentation.
- [ ] **Add end-to-end recipes.** Provide runnable examples for constructing a spatial map,
  finding a weighted path, extracting a polyhex region, rasterizing a map, and creating a weighted
  Voronoi partition. Keep each recipe covered by a compile or snapshot test.

## Release Readiness

- [ ] Run the complete normal test suite for every target framework and run explicit stress tests
  before a release candidate.
- [ ] Compare benchmark results with the last recorded baseline and investigate meaningful
  regressions in time or allocations.
- [ ] Pack the NuGet package and verify its README, XML documentation, dependency range, and a
  clean consumer build before publishing.
