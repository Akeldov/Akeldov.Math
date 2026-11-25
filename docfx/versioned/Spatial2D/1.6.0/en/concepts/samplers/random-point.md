# Random Point Sampler

<xref:Akeldov.Math.Spatial2D.Sampling.Point.UniformRandom.RandomPointSampler> draws a fixed number
of points uniformly and independently within a rectangle. Clusters and gaps occur naturally,
and two points can coincide. Use the [Poisson Disk Sampler](poisson-disk.md) when you need a
minimum distance between neighbors.

## Sample a rectangle

This example draws 128 points in world coordinates. The rectangle's minimum corner is at
`(-56, 32)`, so the returned points are already positioned in the scene.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;
using Akeldov.Math.Spatial2D.Sampling.Point.UniformRandom;

var bounds = new Rectangle(new PointXY(-56, 32), new PointXY(248, 224));
var sampler = new RandomPointSampler(new Random(42));
List<PointXY> points = sampler.Sample(bounds, count: 128);
```

![128 random points inside the sample rectangle, with small clusters and gaps](~/assets/spatial2d/samplers/uniform-random-points.png)

The image shows the sample generated with seed `42`. A second call on the same sampler continues
the sequence. To repeat the example in the same runtime, create a new sampler with the same seed.

For a rectangle starting at `(0, 0)`, pass its size directly, for example
`sampler.Sample(new VectorXY(304, 192), count: 128)`. Both dimensions must be positive.
Each call returns a new, mutable `List<PointXY>` owned by the caller. A count of zero returns an
empty list.
