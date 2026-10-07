# Generating Poisson Points

Randomly chosen points often cluster together. A Poisson disk sampler avoids those clusters by
maintaining a minimum distance between accepted points, making the result a useful starting layout
for procedural regions.

Add this code to `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var pointSampler = new PoissonDiskPointSampler(
    new Random(12345),
    maxAttempts: 30);

var samples = pointSampler.Sample(fieldSize, minimalDistance: 14f);
var sites = samples
    .Select(sample => new Site(sample.Point, weight: 1f))
    .ToArray();

Console.WriteLine($"Generated sites: {sites.Length}");
```

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> starts at a
random point and tries up to `maxAttempts` candidates around each active sample. A larger attempt
count can fill gaps more thoroughly, at the cost of additional work.

The fixed `Random` seed makes the layout reproducible. Change the seed when you want another
layout; use the same seed when investigating or testing a generated map.

`Sample` returns a new mutable list owned by the caller. Each
<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSample> stores its point
and the minimum distance used when accepting it. Here, every sample becomes an equally weighted
<xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.Site>.

Continue with [Creating Voronoi Cells](creating-voronoi-cells.md).
