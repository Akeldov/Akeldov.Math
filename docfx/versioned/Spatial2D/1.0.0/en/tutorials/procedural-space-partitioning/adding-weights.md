# Adding Weights

Equal weights produce ordinary nearest-site regions. A larger positive weight lets a site compete
over a wider area because weighted Voronoi assignment compares squared distance divided by
squared weight.

Replace the creation of `sites` with this indexed projection:

```csharp
var sites = samples
    .Select((sample, index) => new Site(
        sample.Point,
        weight: index % 5 == 0 ? 1.8f : 1f))
    .ToArray();
```

Every fifth site now has weight `1.8`; all others retain weight `1`. Run the program again and
compare the reported cell counts. Heavier sites generally receive more cells, although the exact
size also depends on their neighbors and proximity to the map boundary.

A site weight must be non-negative and cannot be `NaN`, and at least one configured site must
have a positive weight. A zero-weight site only wins an item located exactly at that site. Positive
infinity is supported as a dominant weight; when several sites have infinite weight, the nearest
one wins.

Weights describe relative reach rather than a guaranteed cell area. Doubling a weight does not
guarantee twice as many assigned grid cells.

Keep the weighted `sites` array and continue with [Relaxing the Cells](relaxing-the-cells.md).
