# Building an Influence Map

In this tutorial, you will build an influence map: a continuous numeric field whose value is
determined by several point sources. These fields are useful for danger maps, movement costs,
target attraction, and other spatial scores.

The result will be a PNG file in which low values use cool colors and high values use warm
colors.

## What You Will Build

You will:

1. define the positions, weights, and values of the sources;
2. assemble them into a field;
3. choose an interpolation strategy;
4. limit the sources considered at each point;
5. define the raster geometry;
6. save the heatmap as a PNG file.

Each step continues from the previous one. The final page includes the complete `Program.cs`.

## Create the Project

Install the .NET 6 SDK or later, then run:

```powershell
dotnet new console --name Spatial2D.InfluenceMap
Set-Location Spatial2D.InfluenceMap
dotnet add package Akeldov.Math.Spatial2D --version 0.9.0
```

Start with an empty `Program.cs`, then continue with [Influence Sources](influence-sources.md).
