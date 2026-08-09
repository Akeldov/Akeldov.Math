using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization.Filtering;

public class FloatRasterBlurExtensionsTests
{
    [Test]
    public void GaussianBlur_WithRadiusOne_UsesSeparableGaussianKernel()
    {
        var resolution = new VectorXYInt(5, 5);
        var values = new float[25];
        values[12] = 1f;
        var raster = new Raster<float>(resolution, values);

        Raster<float> result = raster.GaussianBlur(1f, 1);

        double adjacentWeight = System.Math.Exp(-0.5d);
        double weightSum = 1d + 2d * adjacentWeight;
        float expectedCenter = (float)(1d / (weightSum * weightSum));
        float expectedAdjacent = (float)(adjacentWeight / (weightSum * weightSum));
        float expectedDiagonal = (float)(adjacentWeight * adjacentWeight / (weightSum * weightSum));

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(resolution));
            Assert.That(result[2, 2], Is.EqualTo(expectedCenter).Within(1e-7f));
            Assert.That(result[3, 2], Is.EqualTo(expectedAdjacent).Within(1e-7f));
            Assert.That(result[3, 3], Is.EqualTo(expectedDiagonal).Within(1e-7f));
            Assert.That(raster[2, 2], Is.EqualTo(1f));
        });
    }

    [Test]
    public void GaussianBlur_AtBoundaries_RenormalizesPresentWeights()
    {
        var resolution = new VectorXYInt(4, 3);
        var raster = new Raster<float>(resolution, CreateValues(12, 7f));

        Raster<float> result = raster.GaussianBlur(1.25f, 2);

        for (int index = 0; index < result.Values.Length; index++)
            Assert.That(result[index], Is.EqualTo(7f).Within(1e-6f), $"Unexpected value at flat index {index}.");
    }

    [Test]
    public void GaussianBlur_WithoutRadius_UsesThreeSigmaTruncation()
    {
        var resolution = new VectorXYInt(7, 6);
        var values = new float[42];
        for (int index = 0; index < values.Length; index++)
            values[index] = index % 5;

        var raster = new Raster<float>(resolution, values);

        Raster<float> automatic = raster.GaussianBlur(0.6f);
        Raster<float> explicitRadius = raster.GaussianBlur(0.6f, 2);

        for (int index = 0; index < values.Length; index++)
            Assert.That(automatic[index], Is.EqualTo(explicitRadius[index]).Within(1e-7f));
    }

    [Test]
    public void GaussianBlur_WithZeroRadius_ReturnsIndependentCopy()
    {
        var raster = new Raster<float>(new VectorXYInt(2, 1), new[] { 2f, -3f });

        Raster<float> result = raster.GaussianBlur(1f, 0);
        result[0] = 100f;

        Assert.Multiple(() =>
        {
            Assert.That(result[0], Is.EqualTo(100f));
            Assert.That(result[1], Is.EqualTo(-3f));
            Assert.That(raster[0], Is.EqualTo(2f));
        });
    }

    [Test]
    public void GaussianBlur_WithSpatialRaster_PreservesGeometryAndReturnsIndependentValues()
    {
        var geometry = new RasterGeometry(
            new PointXY(-2f, 3f),
            new VectorXY(6f, 4f),
            new VectorXYInt(3, 2));
        ISpatialRaster<float> raster = new SpatialRaster<float>(
            geometry,
            new[] { 1f, 2f, 3f, 4f, 5f, 6f });

        SpatialRaster<float> result = raster.GaussianBlur(1f, 1);
        Raster<float> expected = ((IRaster<float>)raster).GaussianBlur(1f, 1);
        result[0] = 100f;

        Assert.Multiple(() =>
        {
            Assert.That(result.Geometry, Is.EqualTo(geometry));
            Assert.That(result[0], Is.EqualTo(100f));
            Assert.That(result[1], Is.EqualTo(expected[1]).Within(1e-7f));
            Assert.That(raster[0], Is.EqualTo(1f));
        });
    }

    [Test]
    public void GaussianBlur_WithInvalidSpatialArguments_Throws()
    {
        var geometry = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(1f, 1f),
            new VectorXYInt(1, 1));
        ISpatialRaster<float> raster = new SpatialRaster<float>(geometry, new float[1]);
        ISpatialRaster<float>? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => missing.GaussianBlur(1f))!.ParamName,
                Is.EqualTo("raster"));
#pragma warning restore CS8604
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(0f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(1f, -1))!.ParamName,
                Is.EqualTo("radius"));
        });
    }

    [Test]
    public void GaussianBlur_WithInvalidArguments_Throws()
    {
        var raster = new Raster<float>(new VectorXYInt(1, 1), new float[1]);
        IRaster<float>? missing = null;

        Assert.Multiple(() =>
        {
#pragma warning disable CS8604
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => missing.GaussianBlur(1f))!.ParamName,
                Is.EqualTo("raster"));
#pragma warning restore CS8604
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(0f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(-1f))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(float.NaN))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(float.PositiveInfinity))!.ParamName,
                Is.EqualTo("sigma"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() => raster.GaussianBlur(1f, -1))!.ParamName,
                Is.EqualTo("radius"));
        });
    }

    private static float[] CreateValues(int count, float value)
    {
        var values = new float[count];
        Array.Fill(values, value);
        return values;
    }
}
