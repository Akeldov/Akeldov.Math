# Vectors

Hexes uses QRS axial coordinates as the main coordinate model for hex-grid math.

## QRS Basis Examples

QRS coordinates keep the cube-coordinate invariant `Q + R + S = 0`, so the
three visible coordinate axes are drawn as directions in that two-dimensional
plane. These examples build the basis of the QRS coordinate system: it is
equivalent to a QR basis drawn with axes separated by 120 degrees instead of
the 90 degrees used by a Cartesian XY basis.

### `OddR` and `EvenR`

`OddR` and `EvenR` are row-oriented, pointy-top layouts. Their continuous QRS
basis is the same; odd/even row offsets affect index conversion, not the
world-space directions of QRS vectors.

```csharp
var p0 = new PointXY(0, 0);
var segmentQ = new ParameterizedSegment(p0, p0 + new VectorQRS(1, 0).ToVectorXY(Layout.OddR));
var segmentR = new ParameterizedSegment(p0, p0 + new VectorQRS(-1, 1).ToVectorXY(Layout.OddR));
var segmentS = new ParameterizedSegment(p0, p0 + new VectorQRS(0, -1).ToVectorXY(Layout.OddR));

var gameScene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

gameScene
    .AddPointDistanceBasedLayer(segmentQ, RGBA16BitColor.Red, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(segmentR, RGBA16BitColor.Green, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(segmentS, RGBA16BitColor.Blue, 0.01f, 0.01f)
    .Rasterize(new SpatialRasterGrid(new PointXY(-1, -1), new VectorXY(2, 2), new VectorXYInt(300, 300)))
    .SaveAsPng("qrsBasis.png");
```

![QRS basis for OddR and EvenR pointy-top layouts](../assets/hexes/vectors/qrs-basis-pointy-top.png)

### `OddQ` and `EvenQ`

`OddQ` and `EvenQ` are column-oriented, flat-top layouts. Their continuous QRS
basis is the same; odd/even column offsets affect index conversion, not the
world-space directions of QRS vectors.

```csharp
var p0 = new PointXY(0, 0);
var segmentQ = new ParameterizedSegment(p0, p0 + new VectorQRS(1, 0).ToVectorXY(Layout.OddQ));
var segmentR = new ParameterizedSegment(p0, p0 + new VectorQRS(-1, 1).ToVectorXY(Layout.OddQ));
var segmentS = new ParameterizedSegment(p0, p0 + new VectorQRS(0, -1).ToVectorXY(Layout.OddQ));

var gameScene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.AlphaOver);

gameScene
    .AddPointDistanceBasedLayer(segmentQ, RGBA16BitColor.Red, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(segmentR, RGBA16BitColor.Green, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(segmentS, RGBA16BitColor.Blue, 0.01f, 0.01f)
    .Rasterize(new SpatialRasterGrid(new PointXY(-1, -1), new VectorXY(2, 2), new VectorXYInt(300, 300)))
    .SaveAsPng("qrsBasis.png");
```

![QRS basis for OddQ and EvenQ flat-top layouts](../assets/hexes/vectors/qrs-basis-flat-top.png)

## Topics

- [Types](vectors/types.md)
- [Coordinate Conversions](vectors/coordinate-conversions.md)
- [Discretization](vectors/discretization.md)
- [Transformations](vectors/transformations.md)
- [Serialization](vectors/serialization.md)
