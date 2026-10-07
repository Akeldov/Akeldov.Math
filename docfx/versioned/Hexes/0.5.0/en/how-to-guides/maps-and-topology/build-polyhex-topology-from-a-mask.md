# Build Polyhex Topology from a Mask

Use `Polyhex` to turn a rectangular Q/R mask into an immutable set of occupied hexes. A `true`
element belongs to the shape.

```csharp
using Akeldov.Math.Hexes.Topology;

bool[,] mask =
{
    { false, true,  false }, // q = 0, r = 0..2
    { true,  true,  true  }, // q = 1, r = 0..2
    { false, true,  false }, // q = 2, r = 0..2
};

var polyhex = new Polyhex(mask);

int hexCount = polyhex.HexCount;         // 5
bool centerIsPresent = polyhex[1, 1];    // true
int qSize = polyhex.QRSResolution.Q;     // 3
int rSize = polyhex.QRSResolution.R;     // 3
```

The first array dimension is Q and the second is R. The derived S coordinate is `-Q - R`, so the
mask does not have a third dimension and does not use `Layout`.

The constructor copies the mask. Later changes to `mask` do not affect `polyhex`. An `int[,]` is
also accepted: zero means absent and any nonzero value means present. Mask dimensions must be
positive; empty shapes, holes, and disconnected components are allowed.

For the complete data model, see [Polyhexes](../../concepts/hex-grid-model/polyhexes.md). To add
physical size and produce a boundary, continue with
[Convert a polyhex to a Spatial2D contour](../geometry-and-polyhexes/convert-a-polyhex-to-a-spatial2d-contour.md).
