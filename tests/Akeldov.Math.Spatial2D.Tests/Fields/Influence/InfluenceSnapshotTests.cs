using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields.Influence;

public class InfluenceSnapshotTests
{
    private static readonly VectorXY FieldSize = new VectorXY(100f, 60f);
    private static readonly VectorXY CullerFieldSize = new VectorXY(100f, 70f);

    [Test]
    public void FloatPointInfluenceField_WithNearestSampler_MatchesApprovedImage()
    {
        FloatPointInfluenceSource[] sources = CreateFieldSources();
        var sampler = new NearestFloatInfluenceSampler<FloatPointInfluenceSource>();
        var field = new FloatPointInfluenceField(
            sampler,
            sources);

        Assert.That(field.Sample(sources[0].Position), Is.EqualTo(0f));
        Assert.That(field.Sample(sources[1].Position), Is.EqualTo(100f));
        AssertMatchesApprovedSamplerWeightsPng("nearest-sampler.png", sampler, sources, FieldSize);
    }

    [Test]
    public void FloatPointInfluenceField_WithInverseDistanceWeightedSampler_MatchesApprovedImage()
    {
        FloatPointInfluenceSource[] sources = CreateFieldSources();
        var sampler = new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();
        var field = new FloatPointInfluenceField(
            sampler,
            sources);

        Assert.That(field.Sample(sources[0].Position), Is.EqualTo(0f));
        Assert.That(field.Sample(sources[1].Position), Is.EqualTo(100f));
        AssertMatchesApprovedSamplerWeightsPng("inverse-distance-weighted-sampler.png", sampler, sources, FieldSize);
    }

    [Test]
    public void FloatPointInfluenceField_WithBarycentricSampler_MatchesApprovedImage()
    {
        FloatPointInfluenceSource[] sources = CreateBarycentricFieldSources();
        var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
        var field = new FloatPointInfluenceField(
            sampler,
            sources);

        Assert.That(field.Sample(new PointXY(50f, 52f)), Is.EqualTo(100f).Within(0.0001f));
        AssertMatchesApprovedSamplerWeightsPng("barycentric-sampler.png", sampler, sources, FieldSize);
    }

    [Test]
    public void DelaunayCuller_MatchesApprovedImage()
    {
        FloatPointInfluenceSource[] sources = CreateCullerSources();
        var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);

        List<FloatPointInfluenceSource> selectedSources = culler.Cull(new PointXY(50f, 34f));

        Assert.That(selectedSources, Has.Count.EqualTo(3));
        AssertMatchesApprovedCullerPng("delaunay-culler.png", culler, sources, CullerFieldSize);
    }

    private static FloatPointInfluenceSource[] CreateFieldSources()
    {
        return new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
            new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
        };
    }

    private static FloatPointInfluenceSource[] CreateBarycentricFieldSources()
    {
        return new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 25f),
            new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 100f)
        };
    }

    private static FloatPointInfluenceSource[] CreateCullerSources()
    {
        return new[]
        {
            new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),
            new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),
            new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),
            new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),
            new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)
        };
    }

    private static void AssertMatchesApprovedSamplerWeightsPng(
        string approvedFileName,
        IInfluenceSampler<FloatPointInfluenceSource, float> sampler,
        IReadOnlyList<FloatPointInfluenceSource> sources,
        VectorXY fieldSize)
    {
        byte[] actual = InfluencePngRenderer.RenderSamplerWeights(sampler, sources, fieldSize, width: 320, height: 192);
        AssertMatchesApprovedPng(approvedFileName, actual, "Actual influence field image");
    }

    private static void AssertMatchesApprovedCullerPng(
        string approvedFileName,
        IInfluenceSourceCuller<FloatPointInfluenceSource> culler,
        IReadOnlyList<FloatPointInfluenceSource> sources,
        VectorXY fieldSize)
    {
        byte[] actual = InfluencePngRenderer.RenderCullerSelection(culler, sources, fieldSize, width: 320, height: 224);
        AssertMatchesApprovedPng(approvedFileName, actual, "Actual influence culler image");
    }

    private static void AssertMatchesApprovedPng(
        string approvedFileName,
        byte[] actual,
        string attachmentDescription)
    {
        string approvedPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fields", "Influence", "Approved", approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, approvedFileName.Replace(".png", ".actual.png"));
            File.WriteAllBytes(actualPath, actual);
            TestContext.AddTestAttachment(actualPath, attachmentDescription);
            Assert.Fail($"Influence approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, approvedFileName.Replace(".png", ".actual.png"));
            File.WriteAllBytes(actualPath, actual);
            TestContext.AddTestAttachment(actualPath, attachmentDescription);
            Assert.Fail($"Influence snapshot changed. Actual image: {actualPath}");
        }
    }

    private static bool BytesEqual(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
            return false;

        for (int i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
                return false;
        }

        return true;
    }
}
