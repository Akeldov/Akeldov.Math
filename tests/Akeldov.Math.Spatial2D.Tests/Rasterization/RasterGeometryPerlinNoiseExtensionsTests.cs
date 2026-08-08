using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class RasterGeometryPerlinNoiseExtensionsTests
{
    [Test]
    public void CreatePerlinNoise_ReturnsNormalizedMutableSpatialRaster()
    {
        RasterGeometry grid = CreateGrid();

        SpatialRaster<float> raster = grid.CreatePerlinNoise(
            seed: 12345,
            scale: 3f,
            octaves: 5,
            persistence: 0.55f,
            lacunarity: 2.1f,
            offset: new VectorXY(3.25f, -7.5f));

        Assert.Multiple(() =>
        {
            Assert.That(raster.Geometry, Is.EqualTo(grid));
            Assert.That(raster.Values, Has.All.InRange(0f, 1f));
            Assert.That(raster.Values.Max(), Is.GreaterThan(raster.Values.Min()));
        });

        raster[0] = 0.75f;

        Assert.That(raster[0], Is.EqualTo(0.75f));
    }

    [Test]
    public void CreatePerlinNoise_WithSameArguments_ReturnsSameValues()
    {
        RasterGeometry grid = CreateGrid();

        SpatialRaster<float> first = grid.CreatePerlinNoise(37, 4.5f, 3, 0.4f, 2.25f, new VectorXY(-2f, 5f));
        SpatialRaster<float> second = grid.CreatePerlinNoise(37, 4.5f, 3, 0.4f, 2.25f, new VectorXY(-2f, 5f));

        Assert.That(second.Values, Is.EqualTo(first.Values));
    }

    [Test]
    public void CreatePerlinNoise_WithDifferentSeed_ChangesValues()
    {
        RasterGeometry grid = CreateGrid();

        SpatialRaster<float> first = grid.CreatePerlinNoise(seed: 1, scale: 4f);
        SpatialRaster<float> second = grid.CreatePerlinNoise(seed: 2, scale: 4f);

        Assert.That(second.Values, Is.Not.EqualTo(first.Values));
    }

    [Test]
    public void CreatePerlinNoise_ForAdjacentGrid_ContinuesSameNoiseField()
    {
        var largerGrid = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(6f, 4f),
            new VectorXYInt(6, 4));
        var adjacentGrid = new RasterGeometry(
            new PointXY(1f, 0f),
            new VectorXY(5f, 4f),
            new VectorXYInt(5, 4));

        SpatialRaster<float> larger = largerGrid.CreatePerlinNoise(seed: 91, scale: 5f);
        SpatialRaster<float> adjacent = adjacentGrid.CreatePerlinNoise(seed: 91, scale: 5f);

        for (int y = 0; y < adjacent.Resolution.Y; y++)
        {
            for (int x = 0; x < adjacent.Resolution.X; x++)
                Assert.That(adjacent[x, y], Is.EqualTo(larger[x + 1, y]));
        }
    }

    [Test]
    public void CreatePerlinNoise_WithOffset_ContinuesSameNoiseField()
    {
        var largerGrid = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(6f, 4f),
            new VectorXYInt(6, 4));
        var smallerGrid = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(5f, 4f),
            new VectorXYInt(5, 4));

        SpatialRaster<float> larger = largerGrid.CreatePerlinNoise(seed: 91, scale: 5f);
        SpatialRaster<float> shifted = smallerGrid.CreatePerlinNoise(
            seed: 91,
            scale: 5f,
            offset: new VectorXY(1f, 0f));

        for (int y = 0; y < shifted.Resolution.Y; y++)
        {
            for (int x = 0; x < shifted.Resolution.X; x++)
                Assert.That(shifted[x, y], Is.EqualTo(larger[x + 1, y]));
        }
    }

    [Test]
    public void CreatePerlinNoise_WithZeroPersistence_UsesOnlyFirstOctave()
    {
        RasterGeometry grid = CreateGrid();

        SpatialRaster<float> singleOctave = grid.CreatePerlinNoise(seed: 73, scale: 3f, octaves: 1);
        SpatialRaster<float> zeroPersistence = grid.CreatePerlinNoise(
            seed: 73,
            scale: 3f,
            octaves: 8,
            persistence: 0f,
            lacunarity: float.MaxValue);

        Assert.That(zeroPersistence.Values, Is.EqualTo(singleOctave.Values));
    }

    [Test]
    public void CreatePerlinNoise_WithInvalidGrid_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(RasterGeometry).CreatePerlinNoise(seed: 1, scale: 1f));

        Assert.That(exception!.ParamName, Is.EqualTo("grid"));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidScale_Throws(float scale)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateGrid().CreatePerlinNoise(1, scale));

        Assert.That(exception!.ParamName, Is.EqualTo("scale"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CreatePerlinNoise_WithInvalidOctaveCount_Throws(int octaves)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateGrid().CreatePerlinNoise(1, 1f, octaves));

        Assert.That(exception!.ParamName, Is.EqualTo("octaves"));
    }

    [TestCase(-0.1f)]
    [TestCase(1.1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidPersistence_Throws(float persistence)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGrid().CreatePerlinNoise(1, 1f, persistence: persistence));

        Assert.That(exception!.ParamName, Is.EqualTo("persistence"));
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void CreatePerlinNoise_WithInvalidLacunarity_Throws(float lacunarity)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGrid().CreatePerlinNoise(1, 1f, lacunarity: lacunarity));

        Assert.That(exception!.ParamName, Is.EqualTo("lacunarity"));
    }

    [TestCase(float.NaN, 0f)]
    [TestCase(0f, float.NegativeInfinity)]
    public void CreatePerlinNoise_WithNonFiniteOffset_Throws(float x, float y)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateGrid().CreatePerlinNoise(1, 1f, offset: new VectorXY(x, y)));

        Assert.That(exception!.ParamName, Is.EqualTo("offset"));
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(
            new PointXY(-3f, 2f),
            new VectorXY(11f, 7f),
            new VectorXYInt(11, 7));
    }
}
