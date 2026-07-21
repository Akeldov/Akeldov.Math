# Vectors

Hexes uses QRS axial coordinates as the main coordinate model for hex-grid math.

`VectorQRS` represents fractional QRS coordinates, corresponding to cube or
axial coordinates of a point in the hex coordinate plane. `VectorQRSInt` has two
closely related roles: it can represent rounded integer QRS coordinates, or a
hex index in QRS form. When it is used as a hex index, the mapping between that
QRS index and row/column storage indexes depends on `Layout`.

## QRS Basis Examples

QRS coordinates keep the cube-coordinate invariant `Q + R + S = 0`. The
diagrams show a symmetric embedding of the three component axes, separated by
120 degrees: red is `eQ`, green is `eR`, and blue is `eS`. A valid QRS vector is
the linear combination:

```text
(Q, R, S) = Q * eQ + R * eR + S * eS
```

The component axes cannot be represented individually by `VectorQRS` because
each valid vector must satisfy the zero-sum invariant. The diagrams reconstruct
them from the valid differences `eQ - eS = (1, 0, -1)` and
`eR - eS = (0, 1, -1)`, with `eQ + eR + eS = 0` selecting the symmetric
embedding. For a unit-radius hexagon, each component axis has length `1.0`,
while the difference between two axes has length `sqrt(3)`, the center-to-center
distance of neighboring hexagons.

### `OddR` and `EvenR`

`OddR` and `EvenR` are row-oriented, pointy-top layouts. Their continuous QRS
basis is the same; odd/even row offsets affect index conversion, not the
world-space directions of QRS vectors.

```csharp
var p0 = new PointXY(0, 0);
VectorXY qMinusS = new VectorQRS(1, 0).ToVectorXY(Layout.OddR);
VectorXY rMinusS = new VectorQRS(0, 1).ToVectorXY(Layout.OddR);
VectorXY s = (qMinusS + rMinusS) * (-1f / 3f);
VectorXY q = qMinusS + s;
VectorXY r = rMinusS + s;
TrueTypeFont font = TrueTypeFont.Load(
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"));

var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.White, RGBA16BitColor.AlphaOver);
var centered = new TextLayoutOptions { Anchor = TextAnchor.Center };
RGBA16BitColor xColor = RGBA16BitColor.FromNormalized(1f, 0f, 0f, 0.5f);
RGBA16BitColor yColor = RGBA16BitColor.FromNormalized(0f, 1f, 0f, 0.5f);

scene
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, new PointXY(1f, 0f)), xColor, 0.006f, 0.006f)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, new PointXY(0f, 1f)), yColor, 0.006f, 0.006f)
    .AddTextLayer(font, "+X", new PointXY(0.9f, -0.08f), 0.11f, xColor, 0.01f, centered)
    .AddTextLayer(font, "+Y", new PointXY(0.08f, 0.9f), 0.11f, yColor, 0.01f, centered)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + q), RGBA16BitColor.Red, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + r), RGBA16BitColor.Green, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + s), RGBA16BitColor.Blue, 0.01f, 0.01f)
    .AddTextLayer(font, "+Q", p0 + q * 1.12f, 0.13f, RGBA16BitColor.Red, 0.01f, centered)
    .AddTextLayer(font, "+R", p0 + r * 1.12f, 0.13f, RGBA16BitColor.Green, 0.01f, centered)
    .AddTextLayer(font, "+S", p0 + s * 1.12f, 0.13f, RGBA16BitColor.Blue, 0.01f, centered)
    .Rasterize(new RasterGeometry(new PointXY(-1.25f, -1.25f), new VectorXY(2.5f, 2.5f), new VectorXYInt(300, 300)))
    .SaveAsPng("qrs-basis-pointy-top.png");
```

![QRS basis for OddR and EvenR pointy-top layouts](../assets/hexes/vectors/qrs-basis-pointy-top.png)

### `OddQ` and `EvenQ`

`OddQ` and `EvenQ` are column-oriented, flat-top layouts. Their continuous QRS
basis is the same; odd/even column offsets affect index conversion, not the
world-space directions of QRS vectors.

```csharp
var p0 = new PointXY(0, 0);
VectorXY qMinusS = new VectorQRS(1, 0).ToVectorXY(Layout.OddQ);
VectorXY rMinusS = new VectorQRS(0, 1).ToVectorXY(Layout.OddQ);
VectorXY s = (qMinusS + rMinusS) * (-1f / 3f);
VectorXY q = qMinusS + s;
VectorXY r = rMinusS + s;
TrueTypeFont font = TrueTypeFont.Load(
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"));

var scene = new GeometryScene<RGBA16BitColor>(RGBA16BitColor.White, RGBA16BitColor.AlphaOver);
var centered = new TextLayoutOptions { Anchor = TextAnchor.Center };
RGBA16BitColor xColor = RGBA16BitColor.FromNormalized(1f, 0f, 0f, 0.5f);
RGBA16BitColor yColor = RGBA16BitColor.FromNormalized(0f, 1f, 0f, 0.5f);

scene
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, new PointXY(1f, 0f)), xColor, 0.006f, 0.006f)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, new PointXY(0f, 1f)), yColor, 0.006f, 0.006f)
    .AddTextLayer(font, "+X", new PointXY(0.9f, -0.08f), 0.11f, xColor, 0.01f, centered)
    .AddTextLayer(font, "+Y", new PointXY(0.08f, 0.9f), 0.11f, yColor, 0.01f, centered)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + q), RGBA16BitColor.Red, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + r), RGBA16BitColor.Green, 0.01f, 0.01f)
    .AddPointDistanceBasedLayer(new ParameterizedSegment(p0, p0 + s), RGBA16BitColor.Blue, 0.01f, 0.01f)
    .AddTextLayer(font, "+Q", p0 + q * 1.12f, 0.13f, RGBA16BitColor.Red, 0.01f, centered)
    .AddTextLayer(font, "+R", p0 + r * 1.12f, 0.13f, RGBA16BitColor.Green, 0.01f, centered)
    .AddTextLayer(font, "+S", p0 + s * 1.12f, 0.13f, RGBA16BitColor.Blue, 0.01f, centered)
    .Rasterize(new RasterGeometry(new PointXY(-1.25f, -1.25f), new VectorXY(2.5f, 2.5f), new VectorXYInt(300, 300)))
    .SaveAsPng("qrs-basis-flat-top.png");
```

![QRS basis for OddQ and EvenQ flat-top layouts](../assets/hexes/vectors/qrs-basis-flat-top.png)

## Topics

- [Types](vectors/types.md)
- [Coordinate Conversions](vectors/coordinate-conversions.md)
- [Discretization](vectors/discretization.md)
- [Transformations](vectors/transformations.md)
- [Serialization](vectors/serialization.md)
