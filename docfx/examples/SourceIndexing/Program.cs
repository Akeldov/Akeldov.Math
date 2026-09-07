using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

// Run from the repository root. PNGs are produced entirely by Spatial2D.
var outputDirectory = Path.GetFullPath(args.Length == 0
    ? "docfx/assets/spatial2d/fields"
    : args[0]);
Directory.CreateDirectory(outputDirectory);

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(400, 280));
var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),    // A
    new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),   // B
    new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),   // C
    new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),   // D
    new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)   // E
};
var sourceColors = new Dictionary<PointXY, RGBA16BitColor>
{
    { sources[0].Position, new RGBA16BitColor(0xefef, 0x4444, 0x4444, 0xffff) }, // Red
    { sources[1].Position, new RGBA16BitColor(0x2222, 0xc5c5, 0x5e5e, 0xffff) }, // Green
    { sources[2].Position, new RGBA16BitColor(0x3b3b, 0x8282, 0xf6f6, 0xffff) }, // Blue
    { sources[3].Position, new RGBA16BitColor(0xf5f5, 0x9e9e, 0x0b0b, 0xffff) }, // Orange
    { sources[4].Position, new RGBA16BitColor(0xa8a8, 0x5555, 0xf7f7, 0xffff) }  // Purple
};

var halfPlaneIndex = new HalfPlaneInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
halfPlaneIndex.RasterizeCullingMap(grid, point => sourceColors[point])
    .SaveAsPng(Path.Combine(outputDirectory, "indexing-half-plane.png"));

var delaunayIndex = new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
delaunayIndex.RasterizeCullingMap(grid, point => sourceColors[point])
    .SaveAsPng(Path.Combine(outputDirectory, "indexing-delaunay.png"));

// Validate membership, not the order returned by the index.
var checks = new[]
{
    (Query: new PointXY(50f, 5f), HalfPlane: "ABE", Delaunay: "AB"),
    (Query: new PointXY(50f, 20f), HalfPlane: "ABE", Delaunay: "ABE"),
    (Query: new PointXY(0f, 0f), HalfPlane: "A", Delaunay: "A")
};
foreach (var check in checks)
{
    var indexes = new (string Name, IInfluenceSourceIndex<FloatPointInfluenceSource> Index, string Expected)[]
    {
        ("half-plane", halfPlaneIndex, check.HalfPlane),
        ("delaunay", delaunayIndex, check.Delaunay)
    };
    foreach (var (name, index, expected) in indexes)
    {
        var selected = index.SelectSources(check.Query);
        var names = new string(selected
            .Select(source => (char)('A' + Array.IndexOf(sources, source)))
            .OrderBy(label => label).ToArray());
        if (names != expected)
            throw new InvalidOperationException($"Unexpected {name} selection at {check.Query}: {names}, expected {expected}.");

        Console.WriteLine($"{name} at {check.Query}: {names}");
    }
}

var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, delaunayIndex);
float value = field.Sample(new PointXY(50f, 20f));
if (!float.IsFinite(value) || value < field.Min || value > field.Max)
    throw new InvalidOperationException("The indexed field must return a finite value within its bounds.");
Console.WriteLine(FormattableString.Invariant($"Indexed field at (50, 20): {value:F6}"));
