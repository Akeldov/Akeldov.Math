# Agent Notes

## Small Validation Guards

Do not extract small argument or state checks into separate helper methods just for reuse.
Keep simple guard checks inline at the public API or constructor boundary where the contract is
readable in place. Extract validation only when the logic is complex enough that a named helper
improves clarity, or when an existing local pattern already requires it.

## Spatial2D Angle Units

In `Akeldov.Math.Spatial2D`, angles are expressed in radians by default.
Angle parameters and properties must state their units in XML comments.
Non-radian members must use an explicit suffix, such as `Deg`, and document their unit.

## MkDocs Image Paths

MkDocs rewrites normal Markdown links, but raw HTML image tags such as `<img src="...">`
are emitted as-is.
When using raw HTML images, calculate `src` relative to the published page URL directory,
not relative to the source `.md` file.

For example, `docs/Spatial2D/learn/curves.md` is published at
`Spatial2D/learn/curves/`, so root-level assets are reached with
`../../../assets/...`.
`docs/Spatial2D/learn/curves/linear.md` is published at
`Spatial2D/learn/curves/linear/`, so root-level assets are reached with
`../../../../assets/...`.

Prefer Markdown image syntax when fixed sizing or HTML layout is not needed.
Before running MkDocs locally, check whether it is available in the active Python environment:

```powershell
python -m mkdocs --version
```

If that fails with `No module named mkdocs`, create or refresh the local virtual environment
from the repository requirements:

```powershell
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
.\.venv\Scripts\python -m mkdocs --version
```

Use the `.venv` Python for local documentation checks when the global Python does not have
MkDocs installed.
After changing raw HTML image paths, verify with:

```powershell
.\.venv\Scripts\python -m mkdocs build --strict --site-dir .mkdocs-site-temp
```

Then check the generated HTML image `src` values resolve to files under
`.mkdocs-site-temp\assets\...`, and remove `.mkdocs-site-temp` after verification.

## Spatial2D README

Keep `src\Akeldov.Math.Spatial2D\README.md` short and close to its current shape:
a brief package description, a compact feature overview, and a link to the full documentation.

Do not expand it with release notes, long examples, documentation maps, benchmark details,
or API guides. Put detailed material in `docs\Spatial2D\...`.

## Spatial2D Benchmarks

`Akeldov.Math.Spatial2D` has a BenchmarkDotNet project at
`benchmarks\Akeldov.Math.Spatial2D.Benchmarks\Akeldov.Math.Spatial2D.Benchmarks.csproj`.
When assessing Spatial2D performance coverage, account for this project before suggesting
that benchmarks are missing.

The benchmark project includes coverage for Delaunay culling, barycentric sampling,
Voronoi partitioning, Poisson disk sampling, contours, regions, and signed-distance rasterization.

## Spatial2D Collection Ownership

In `Akeldov.Math.Spatial2D`, collection return types express ownership semantics.

Use caller-owned mutable collections, such as `List<T>` or arrays, for newly computed transient
results that the library does not retain and callers may reasonably filter, append to, or reuse.
Examples include ray intersections, influence culling results, Poisson disk samples, scaled item
copies, and derived Voronoi site arrays.

Use `IReadOnlyList<T>` for structural views, retained data, copied immutable-facing state,
validated pass-through inputs, or semantic algorithm results whose order, cardinality, or
other invariants should not be mutated through the public contract. Examples include contour
curves, region contours, partition items, partitioner results, validation helpers that return
the input list after validation, influence sources, and distinct field values.

When returning a mutable caller-owned collection from a public Spatial2D API, XML comments must
state that the returned collection is new, mutable, and owned by the caller. For caller-owned
arrays, state that the returned array is new and owned by the caller.

When documenting an `IReadOnlyList<T>` surface, XML comments should state the reason when
ownership is not obvious: structural view/state, validated input returned as-is, or semantic
result with invariants.

Do not change a caller-owned mutable return to `IReadOnlyList<T>` solely for consistency. Treat
the return type as part of the ownership contract.

## Tests

When sandboxing is active, request elevated access immediately for .NET build and test commands.
The repository's .NET build and test commands write under `bin` and `obj`, which may fail with sandbox `Access denied` errors.

### Spatial2D Test Layout

`Akeldov.Math.Spatial2D.Tests` has three layers of geometry robustness tests:

- Edge-case regression tests live next to the API they cover, in the existing domain folders
  such as `Curves`, `Rectangles`, `Regions`, and `Partitioning\Voronoi`. Add focused cases to
  the existing `*Tests` class when the case belongs to one type, for example `SegmentTests`,
  `ArcTests`, `ContourTests`, or `RectangleTests`.
- Seeded property/fuzz tests live in the same domain folder as the behavior they exercise.
  Name them with `*PropertyFuzzTests` when they check broader invariants, or `*FuzzTests` when
  the domain name already makes the property obvious. Current examples include
  `Curves\CurveIntersectionFuzzTests.cs`,
  `Rectangles\RectanglePropertyFuzzTests.cs`, and
  `Partitioning\Voronoi\VoronoiPartitionPropertyFuzzTests.cs`.
- Heavy stress tests live under `tests\Akeldov.Math.Spatial2D.Tests\Stress`.
  Stress fixtures must use both `Category("Stress")` and `Explicit` so normal test runs stay fast.
  Name them by subsystem, for example `PoissonDiskStressTests`, `VoronoiStressTests`,
  `DelaunayCullerStressTests`, `RasterizationStressTests`, or `RegionStressTests`.

For seeded fuzz and stress tests, use fixed seeds in `[TestCase]` and include the seed and
iteration/scenario in failure messages so failures are reproducible.

For the Spatial2D NUnit tests, build the test project first, then run the test assembly directly:

```powershell
dotnet build tests\Akeldov.Math.Spatial2D.Tests\Akeldov.Math.Spatial2D.Tests.csproj --framework net6.0 --no-restore --disable-build-servers /maxcpucount:1
```

The explicit framework and single MSBuild node avoid intermittent empty `Build FAILED` results when resolving the multi-targeted Spatial2D project reference.

```powershell
dotnet vstest tests\Akeldov.Math.Spatial2D.Tests\bin\Debug\net6.0\Akeldov.Math.Spatial2D.Tests.dll "--logger:console;verbosity=detailed"
```

Spatial2D stress tests are marked with NUnit `Category("Stress")` and `Explicit`, so they are
excluded from normal test runs. Run them separately before releases or deep algorithm changes:

```powershell
dotnet vstest tests\Akeldov.Math.Spatial2D.Tests\bin\Debug\net6.0\Akeldov.Math.Spatial2D.Tests.dll --TestCaseFilter:"TestCategory=Stress" "--logger:console;verbosity=detailed"
```

For the Hexes NUnit tests, build the test project first, then run the test assembly directly:

```powershell
dotnet build tests\Akeldov.Math.Hexes.Tests\Akeldov.Math.Hexes.Tests.csproj --framework net6.0 --no-restore --disable-build-servers /maxcpucount:1
```

```powershell
dotnet vstest tests\Akeldov.Math.Hexes.Tests\bin\Debug\net6.0\Akeldov.Math.Hexes.Tests.dll "--logger:console;verbosity=detailed"
```
