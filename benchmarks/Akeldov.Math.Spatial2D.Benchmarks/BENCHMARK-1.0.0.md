# Spatial2D 1.0.0 benchmark comparison

Status: passed after performance fixes

## Scope

The repository did not contain a complete accepted BenchmarkDotNet report for 0.9.0. The
baseline was therefore reconstructed from the last released 0.9.0 source commit
`6b09805135567699abc6eda931cf6fb603a27354` and measured immediately before the pre-fix 1.0.0
candidate commit `025cf98da25263a9531f6bffdc5ac2c911389830` on the same machine.

Both full suites completed all 45 cases with exit code 0. The comparison used BenchmarkDotNet
0.15.8, its `ShortRun` job, .NET 8.0.17, .NET SDK 9.0.301, concurrent workstation GC, and an AMD
Ryzen 9 9950X3D running Windows 11. The baseline suite took 5:50 and the candidate suite took
5:52. The review investigated every time increase above 10% and every allocation increase.

The 0.9.0 `DelaunayCuller.Build` and `CullQueries` cases were matched to the renamed 1.0.0
`DelaunayInfluenceSourceIndex.Build` and `SelectSources` cases. All other benchmark sources were
equivalent apart from API type renames.

## Full-suite screening

| Benchmark group | Cases | Time delta range | Cases above 10% | Allocation increases |
| --- | ---: | ---: | ---: | ---: |
| Barycentric float sampling | 3 | -0.1% to +3.6% | 0 | 0 |
| Contours | 4 | -4.8% to -2.5% | 0 | 0 |
| Delaunay influence index | 6 | -1.5% to +201.8% | 3 | 3 |
| PNG encoding | 4 | +5.7% to +9.0% | 0 | 0 |
| Poisson disk sampling | 8 | +0.8% to +7.2% | 0 | 0 |
| Regions | 4 | -1.6% to +0.1% | 0 | 0 |
| Signed-distance rasterization | 8 | -3.2% to +22.1% | 4 | 4 |
| Voronoi partitioning | 8 | -3.8% to +15.5% | 1 | 0 |

The apparent +15.5% Voronoi case was not classified as a confirmed regression: its baseline
ShortRun error was 74.9%, the confidence intervals overlapped, and allocations were unchanged.
The seven Delaunay and contour-rasterization cases had deterministic allocation increases and
were repeated separately for confirmation.

## Initial confirmed regressions

| Benchmark | Parameters | 0.9.0 mean | 1.0.0 mean | Time delta | 0.9.0 allocated | 1.0.0 allocated | Ratio |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Delaunay `SelectSources` | 32 sources, 1,000 queries | 23.29 us | 72.16 us | +209.8% | 76.07 KB | 144.46 KB | 1.90x |
| Delaunay `SelectSources` | 128 sources, 1,000 queries | 22.57 us | 54.14 us | +139.9% | 77.33 KB | 146.88 KB | 1.90x |
| Delaunay `SelectSources` | 512 sources, 1,000 queries | 22.39 us | 40.10 us | +79.1% | 77.98 KB | 148.14 KB | 1.90x |
| Contour Gray8 SDF | 128 x 128 | 5.173 ms | 7.081 ms | +36.9% | 2.10 MB | 16.21 MB | 7.72x |
| Contour Gray16 SDF | 128 x 128 | 5.304 ms | 6.488 ms | +22.3% | 2.11 MB | 16.22 MB | 7.69x |
| Contour Gray8 SDF | 256 x 256 | 20.658 ms | 28.352 ms | +37.2% | 8.42 MB | 65.06 MB | 7.73x |
| Contour Gray16 SDF | 256 x 256 | 24.106 ms | 28.491 ms | +18.2% | 8.48 MB | 65.12 MB | 7.68x |

The targeted rerun used the same host and `ShortRun` configuration as the full comparison.

## Investigation

- `DelaunayInfluenceSourceIndex.SelectSources` now creates a `List<int>` and then allocates and
  fills a second `List<TPointSource>`. The 0.9.0 implementation returned the source list directly.
  This extra per-query collection accounts for the approximately 70 KB increase across 1,000
  queries and contributes to the time regression.
- `ParameterizedArc.CountRightwardCrossings` now routes through the public intersection helpers.
  A crossing test creates an intersection list and passes it through multiple ordering operations
  with capturing comparison lambdas. Contour signed-distance rasterization repeats that path for
  every cell and every fillet arc, accounting for the resolution-scaled allocation increase.

## Resolution verification

`DelaunayInfluenceSourceIndex.SelectSources` was changed to create the required caller-owned source
list directly for the normal triangulation and convex-hull paths. `Arc` and `ParameterizedArc` now
count horizontal crossings analytically without constructing or ordering intersection lists.

The same seven cases were repeated after the fixes:

| Benchmark | Parameters | 0.9.0 mean | Fixed mean | Time delta | 0.9.0 allocated | Fixed allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Delaunay `SelectSources` | 32 sources, 1,000 queries | 23.29 us | 24.17 us | +3.8% | 76.07 KB | 76.07 KB |
| Delaunay `SelectSources` | 128 sources, 1,000 queries | 22.57 us | 23.17 us | +2.7% | 77.33 KB | 77.33 KB |
| Delaunay `SelectSources` | 512 sources, 1,000 queries | 22.39 us | 22.87 us | +2.1% | 77.98 KB | 77.98 KB |
| Contour Gray8 SDF | 128 x 128 | 5.173 ms | 5.403 ms | +4.4% | 2.10 MB | 16.09 KB |
| Contour Gray16 SDF | 128 x 128 | 5.304 ms | 5.282 ms | -0.4% | 2.11 MB | 32.09 KB |
| Contour Gray8 SDF | 256 x 256 | 20.658 ms | 20.656 ms | 0.0% | 8.42 MB | 64.09 KB |
| Contour Gray16 SDF | 256 x 256 | 24.106 ms | 20.479 ms | -15.0% | 8.48 MB | 128.10 KB |

Delaunay allocations returned exactly to the 0.9.0 baseline. Contour rasterization now allocates
essentially only its result array and is faster than or within 5% of the baseline. A final full
suite completed all 45 cases with exit code 0 in 5:36 on the same host and configuration.

The benchmark release gate is passed.
