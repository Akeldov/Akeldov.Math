using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class VectorXYIntPerlinNoiseExtensionsTests
{
    [Test]
    public void CreatePerlinNoise_ReturnsNormalizedMutableRaster()
    {
        var resolution = new VectorXYInt(11, 7);

        Raster<float> raster = resolution.CreatePerlinNoise(
            seed: 12345,
            scale: 3f,
            octaves: 5,
            persistence: 0.55f,
            lacunarity: 2.1f,
            offset: new VectorXY(3.25f, -7.5f));

        Assert.Multiple(() =>
        {
            Assert.That(raster.Resolution, Is.EqualTo(resolution));
            Assert.That(raster.Values, Has.All.InRange(0f, 1f));
            Assert.That(raster.Values.Max(), Is.GreaterThan(raster.Values.Min()));
        });

        raster[0] = 0.75f;

        Assert.That(raster[0], Is.EqualTo(0.75f));
    }

    [Test]
    public void CreatePerlinNoise_MatchesEquivalentUnitCellRasterGeometry()
    {
        var resolution = new VectorXYInt(9, 6);
        var grid = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(resolution.X, resolution.Y),
            resolution);
        var offset = new VectorXY(-3.5f, 7.25f);

        Raster<float> raster = resolution.CreatePerlinNoise(
            seed: 81,
            scale: 4.5f,
            octaves: 5,
            persistence: 0.45f,
            lacunarity: 2.25f,
            offset: offset);
        SpatialRaster<float> spatialRaster = grid.CreatePerlinNoise(
            seed: 81,
            scale: 4.5f,
            octaves: 5,
            persistence: 0.45f,
            lacunarity: 2.25f,
            offset: offset);

        Assert.That(raster.Values, Is.EqualTo(spatialRaster.Values));
    }

    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(-1, 1)]
    [TestCase(1, -1)]
    public void CreatePerlinNoise_WithInvalidResolution_Throws(int width, int height)
    {
        var resolution = new VectorXYInt(width, height);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            resolution.CreatePerlinNoise(seed: 1, scale: 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }

    [Test]
    public void CreatePerlinNoise_WhenCellCountExceedsArrayLength_Throws()
    {
        var resolution = new VectorXYInt(int.MaxValue, 2);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            resolution.CreatePerlinNoise(seed: 1, scale: 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidScale_Throws(float scale)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VectorXYInt.One.CreatePerlinNoise(1, scale));

        Assert.That(exception!.ParamName, Is.EqualTo("scale"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CreatePerlinNoise_WithInvalidOctaveCount_Throws(int octaves)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VectorXYInt.One.CreatePerlinNoise(1, 1f, octaves));

        Assert.That(exception!.ParamName, Is.EqualTo("octaves"));
    }

    [TestCase(-0.1f)]
    [TestCase(1.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidPersistence_Throws(float persistence)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VectorXYInt.One.CreatePerlinNoise(1, 1f, persistence: persistence));

        Assert.That(exception!.ParamName, Is.EqualTo("persistence"));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidLacunarity_Throws(float lacunarity)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VectorXYInt.One.CreatePerlinNoise(1, 1f, lacunarity: lacunarity));

        Assert.That(exception!.ParamName, Is.EqualTo("lacunarity"));
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void CreatePerlinNoise_WithNonFiniteOffset_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            VectorXYInt.One.CreatePerlinNoise(1, 1f, offset: new VectorXY(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("offset"));
    }
}
