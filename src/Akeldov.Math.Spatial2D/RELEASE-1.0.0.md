# Akeldov.Math.Spatial2D 1.0.0 Release Checklist

## Release candidate

- [x] Freeze the reviewed public API in `PublicAPI.Shipped.txt` and leave
  `PublicAPI.Unshipped.txt` empty apart from its nullable directive.
- [x] Review the 1.0.0 release notes and migration impact of removed and replaced APIs.
- [x] Build the package in Release configuration and verify that it contains the README, XML
  documentation, and both `netstandard2.1` and `net6.0` assets.
- [x] Verify package compatibility with a clean `net8.0` consumer build.
- [x] Build and run the normal Spatial2D test suite.
- [x] Run the explicit Spatial2D stress suite.
- [x] Build the complete DocFX wiki with zero errors and verify representative English, Russian,
  API, inherited, overridden, `latest`, and `upcoming` pages.
- [x] Confirm that `latest` resolves to 1.0.0 and that generated edit links target the inherited
  base or the 1.0.0 override as appropriate.
- [x] Confirm that temporary DocFX merge directories are removed after the build.
- [x] Bind publication to an annotated `spatial2d/v1.0.0` tag, reject duplicate publication, and
  compare the generated payload with the immutable release candidate before pushing it.

## Before tagging

- [x] Compare the BenchmarkDotNet suite with the last accepted baseline and investigate meaningful
  time or allocation regressions. See the
  [1.0.0 benchmark comparison](../../benchmarks/Akeldov.Math.Spatial2D.Benchmarks/BENCHMARK-1.0.0.md).
- [x] Resolve or explicitly accept the confirmed Delaunay query and contour signed-distance
  rasterization regressions recorded in the benchmark comparison.
- [ ] Confirm the supported-runtime and CI policy intended for the 1.0 support lifetime.

## Publication

- [ ] Review the final commit and create an annotated `spatial2d/v1.0.0` tag on it.
- [ ] Run the tag-bound NuGet publication workflow and approve the `nuget.org` environment.
- [ ] Verify the published package identity, version, contents, and SHA-256 against the release
  candidate stored in `docfx/versioned/Spatial2D/1.0.0/source`.
- [ ] Publish release notes and verify the public package and documentation links.
