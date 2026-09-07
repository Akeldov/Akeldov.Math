# Source indexing illustrations

Run from the repository root with the .NET 9 SDK:

```powershell
dotnet run --project docfx/examples/SourceIndexing/SourceIndexing.csproj --framework net9.0
```

The C# example uses `HalfPlaneInfluenceSourceIndex` and `DelaunayInfluenceSourceIndex`
with the five-source layout and colors from `docs/Spatial2D/fields/source-culling.md`.
It generates `indexing-half-plane.png` and `indexing-delaunay.png` in
`docfx/assets/spatial2d/fields` at 400 × 280 resolution. An optional first argument
overrides the output directory.

Both PNGs come directly from the library's `RasterizeCullingMap` and `SaveAsPng`,
without post-processing or external imaging dependencies. Each pixel averages the
selected source colors in linear RGB; it does not represent a sampled field value.
Positive Y points upwards.

The program checks source membership at the three points discussed in the EN/RU
article (inside a triangle, outside near an edge, and outside near a vertex), and
also samples a barycentric field using the same Delaunay index.
