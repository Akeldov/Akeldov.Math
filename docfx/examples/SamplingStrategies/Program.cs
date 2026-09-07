using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

// Run from the repository root. PNGs are produced entirely by Spatial2D.
var outputDirectory = Path.GetFullPath(args.Length == 0
    ? "docfx/assets/spatial2d/fields"
    : args[0]);
Directory.CreateDirectory(outputDirectory);

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(0f, 10f), 50f)
};
var query = new PointXY(4f, 3f);
var geometry = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(10f, 10f), new VectorXYInt(400, 400));

var samplers = new (string Name, IInfluenceSampler<FloatPointInfluenceSource, float> Sampler)[]
{
    ("nearest", new NearestFloatInfluenceSampler<FloatPointInfluenceSource>()),
    ("inverse-distance-weighted", new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>()),
    ("barycentric", new BarycentricFloatSampler<FloatPointInfluenceSource>())
};

double inverseDistanceExpected = (100d / Math.Sqrt(45d) + 50d / Math.Sqrt(65d))
    / (1d / 5d + 1d / Math.Sqrt(45d) + 1d / Math.Sqrt(65d));
var expectedValues = new[] { 0d, inverseDistanceExpected, 55d };
for (int strategy = 0; strategy < samplers.Length; strategy++)
{
    var (name, sampler) = samplers[strategy];
    var field = new FloatPointInfluenceField(sampler, sources);
    float value = field.Sample(query);
    if (Math.Abs(value - expectedValues[strategy]) > 0.0001d)
        throw new InvalidOperationException($"The documented {name} result changed: {value}.");

    var heatMap = field.RasterizeHeatMap(geometry);
    heatMap.SaveAsPng(Path.Combine(outputDirectory, $"sampling-{name}.png"));
    Console.WriteLine(FormattableString.Invariant($"{name}: f(4, 3) = {value:F6}"));
}
