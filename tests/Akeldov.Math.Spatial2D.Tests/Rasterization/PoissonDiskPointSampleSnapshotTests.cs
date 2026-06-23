using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class PoissonDiskPointSampleSnapshotTests
{
    private static readonly VectorXY FieldSize = new VectorXY(120f, 80f);
    private static readonly RasterGrid SnapshotGrid = new RasterGrid(
        origin: new PointXY(0f, 0f),
        size: FieldSize,
        resolution: new VectorXYInt(180, 120));

    [Test]
    public void Rasterize_WithVariableDistancePoissonDiskSamples_MatchesApprovedImage()
    {
        var sampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);
        var field = new HorizontalGradientFloatField(min: 5f, max: 13f, width: FieldSize.X);
        List<PoissonDiskPointSample> samples = sampler.Sample(FieldSize, field);

        Raster<RGBA16BitColor> raster = samples.Rasterize(SnapshotGrid, ToSnapshotColor);
        byte[] actual = SaveToPngBytes(raster, "poisson-disk-samples-rgba16.png");

        Assert.That(samples, Has.Count.EqualTo(104));
        AssertMatchesApprovedPng("poisson-disk-samples-rgba16.png", actual);
    }

    [Test]
    public void RasterizeGray16_WithVariableDistancePoissonDiskSamplesAndRings_MatchesApprovedImage()
    {
        var sampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);
        var field = new HorizontalGradientFloatField(min: 5f, max: 13f, width: FieldSize.X);
        List<PoissonDiskPointSample> samples = sampler.Sample(FieldSize, field);

        Raster<ushort> raster = samples.Rasterize(
            SnapshotGrid,
            new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(
                pointRadius: 1.45f,
                ringThickness: 0.18f,
                backgroundGrayLevel: 0x1010,
                ringGrayLevel: 0x8a8a,
                pointGrayLevel: ushort.MaxValue));
        byte[] actual = SaveToPngBytes(raster, "poisson-disk-samples-rings-gray16.png");

        Assert.That(samples, Has.Count.EqualTo(104));
        AssertMatchesApprovedPng("poisson-disk-samples-rings-gray16.png", actual);
    }

    private static RGBA16BitColor ToSnapshotColor(PoissonDiskPointSample sample, float distance)
    {
        float distanceT = MathF.Min(distance / sample.MinimalDistance, 1f);
        float distanceFill = (1f - distanceT) * 0.55f;
        float radiusT = MathF.Min(MathF.Max((sample.MinimalDistance - 5f) / 8f, 0f), 1f);

        Rgb background = new Rgb(0.972f, 0.980f, 0.988f);
        Rgb smallDistance = new Rgb(0.125f, 0.510f, 0.965f);
        Rgb largeDistance = new Rgb(0.961f, 0.620f, 0.043f);
        Rgb diskColor = Blend(smallDistance, largeDistance, radiusT);
        Rgb color = Blend(background, diskColor, distanceFill);

        float pointAmount = MathF.Max(0f, 1f - distance / 1.15f);
        color = Blend(color, new Rgb(0.058f, 0.090f, 0.165f), pointAmount);

        return color.ToRGBA16BitColor();
    }

    private static Rgb Blend(Rgb from, Rgb to, float amount)
    {
        amount = MathF.Min(MathF.Max(amount, 0f), 1f);
        float inverseAmount = 1f - amount;

        return new Rgb(
            from.Red * inverseAmount + to.Red * amount,
            from.Green * inverseAmount + to.Green * amount,
            from.Blue * inverseAmount + to.Blue * amount);
    }

    private static byte[] SaveToPngBytes(Raster<ushort> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static byte[] SaveToPngBytes(Raster<RGBA16BitColor> raster, string approvedFileName)
    {
        string actualPath = GetActualPath(approvedFileName);
        raster.SaveAsPng(actualPath);
        return File.ReadAllBytes(actualPath);
    }

    private static void AssertMatchesApprovedPng(string approvedFileName, byte[] actual)
    {
        string approvedPath = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "Rasterization",
            "Approved",
            approvedFileName);

        if (!File.Exists(approvedPath))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual Poisson disk sample raster snapshot");
            Assert.Fail($"Poisson disk sample approved image is missing. Actual image: {actualPath}");
        }

        byte[] approved = File.ReadAllBytes(approvedPath);

        if (!BytesEqual(actual, approved))
        {
            string actualPath = GetActualPath(approvedFileName);
            TestContext.AddTestAttachment(actualPath, "Actual Poisson disk sample raster snapshot");
            Assert.Fail($"Poisson disk sample snapshot changed. Actual image: {actualPath}");
        }
    }

    private static string GetActualPath(string approvedFileName)
    {
        return Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            approvedFileName.Replace(".png", ".actual.png"));
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

    private readonly struct Rgb
    {
        public Rgb(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public float Red { get; }

        public float Green { get; }

        public float Blue { get; }

        public RGBA16BitColor ToRGBA16BitColor()
        {
            return new RGBA16BitColor(ToChannel(Red), ToChannel(Green), ToChannel(Blue), ushort.MaxValue);
        }

        private static ushort ToChannel(float value)
        {
            value = MathF.Min(MathF.Max(value, 0f), 1f);
            return (ushort)MathF.Round(value * ushort.MaxValue);
        }
    }

    private sealed class HorizontalGradientFloatField : IFloatField
    {
        private readonly float _min;
        private readonly float _max;
        private readonly float _width;

        public HorizontalGradientFloatField(float min, float max, float width)
        {
            _min = min;
            _max = max;
            _width = width;
        }

        public float Min => _min;

        public float Max => _max;

        public float Sample(PointXY point)
        {
            float t = point.X / _width;
            t = MathF.Max(0f, MathF.Min(1f, t));
            return _min + (_max - _min) * t;
        }
    }
}
