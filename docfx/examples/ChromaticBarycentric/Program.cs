using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

if (args.Length > 0)
{
    Directory.CreateDirectory(args[0]);
    Directory.SetCurrentDirectory(args[0]);
}

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

RasterGeometry sampling = geometry.ToRasterGeometry(pixelsPerApothem: 32f);

var weights = new ChromaticBarycentricTripletRaster(geometry, sampling);
SpatialRaster<RGBA8BitColor> blendedImage = weights.MapValues(
    w => RGBA8BitColor.FromNormalized(w.Index0, w.Index1, w.Index2, alpha: 1f));

var classes = new ChromaticIndexMap(geometry);
SpatialRaster<RGBA8BitColor> classImage = classes.Rasterize(
    pixelsPerApothem: 32f,
    margin: 0f,
    colorSelector: c => RGBA8BitColor.FromNormalized(
        c == 0 ? 1f : 0f,
        c == 1 ? 1f : 0f,
        c == 2 ? 1f : 0f,
        alpha: 1f));

classImage.SaveAsPng(Path.GetFullPath("chromatic-classes.png"));
blendedImage.SaveAsPng(Path.GetFullPath("chromatic-barycentric.png"));

var partialWeights = new ChromaticBarycentricPartialTripletRaster(
    geometry, sampling);

SpatialRaster<RGBA8BitColor> finiteImage = partialWeights.MapValues(ToFiniteRgb);
finiteImage.SaveAsPng(Path.GetFullPath("chromatic-barycentric-finite.png"));

static RGBA8BitColor ToFiniteRgb(PartialChromaticTriplet<float> w)
{
    float w0 = w.HasIndex0 ? w.Index0 : 0f;
    float w1 = w.HasIndex1 ? w.Index1 : 0f;
    float w2 = w.HasIndex2 ? w.Index2 : 0f;
    float sum = w0 + w1 + w2;

    return sum > 0f
        ? RGBA8BitColor.FromNormalized(w0 / sum, w1 / sum, w2 / sum, alpha: 1f)
        : default;
}
