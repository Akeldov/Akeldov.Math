# Sampling strategy illustrations

Run from the repository root with the .NET 9 SDK:

```powershell
dotnet run --project docfx/examples/SamplingStrategies/SamplingStrategies.csproj --framework net9.0
```

The C# example evaluates the three library samplers, checks the documented values at
`P(4, 3)`, and calls `RasterizeHeatMap` and `SaveAsPng` to produce the three checked-in
400 × 400 PNGs in `docfx/assets/spatial2d/fields`.

All images use the same sources, bounds, orientation (positive Y upwards), and the
library's blue-cyan-green-yellow-red temperature palette. They are saved directly
by the library's PNG encoder without post-processing or additional imaging dependencies.

The articles reference shared images with `~/assets/...`. The main DocFX build resolves
that path directly; the versioned build rebases it to the main asset directory before
rendering each independent version adapter.
