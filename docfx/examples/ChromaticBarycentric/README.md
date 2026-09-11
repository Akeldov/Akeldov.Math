# Chromatic barycentric interpolation illustrations

Run from the repository root with the .NET 9 SDK:

```powershell
dotnet run --project docfx/examples/ChromaticBarycentric/ChromaticBarycentric.csproj -- docfx/assets/hexes/chromatization
```

The example uses the immutable Hexes 0.1.0 package, the article base version, and generates the
three shared PNGs directly through the library's rasterization and PNG APIs. It compares discrete
RGB class colors with barycentric weights ordered by class on the same 5 by 4 OddR geometry.
The complete weight raster includes the implied infinite lattice at the rectangular image edges.

The partial weight raster generates a third image using only present weights, normalized by
their sum. It returns transparent black when no positive contribution remains.

Without an output-directory argument, the images are saved in the working directory.
The EN/RU tutorial uses the same rendering code. Assets are referenced via `~/assets/...`
so DocFX's existing asset-link rebasing handles stable, upcoming, and language paths.
