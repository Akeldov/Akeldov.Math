# Generate Poisson Disk Points

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var sampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);

List<PoissonDiskPointSample> samples =
    sampler.Sample(new VectorXY(120f, 80f), minimalDistance: 9f);
```

The returned list is new, mutable, and owned by the caller.
