using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class PerlinNoiseFieldTests
{
    [Test]
    public void Sample_AtWorldCellCenters_MatchesSpatialRaster()
    {
        var grid = new RasterGeometry(new PointXY(-4f, 2f), new VectorXY(6f, 4f), new VectorXYInt(12, 4));
        var offset = new VectorXY(1.25f, -2.5f);
        IFloatField field = new PerlinNoiseField(42, 3.5f, 5, 0.4f, 1.75f, offset);
        var raster = grid.CreatePerlinNoise(42, 3.5f, 5, 0.4f, 1.75f, offset);

        Assert.That(field.Min, Is.EqualTo(0f));
        Assert.That(field.Max, Is.EqualTo(1f));
        for (int y = 0; y < grid.Resolution.Y; y++)
        {
            for (int x = 0; x < grid.Resolution.X; x++)
                Assert.That(field.Sample(grid.GetCellCenter(x, y)), Is.EqualTo(raster[x, y]));
        }
    }

    [Test]
    public void Sample_AtLatticePoint_ReturnsNormalizedMidpoint()
    {
        var field = new PerlinNoiseField(42, scale: 8f);

        Assert.That(field.Sample(new PointXY(-16f, 40f)), Is.EqualTo(0.5f));
    }

    [Test]
    public void Sample_RejectsNonFinitePointAndOutOfRangeLatticeCoordinate()
    {
        var field = new PerlinNoiseField(42, scale: 1f);

        var error = Assert.Throws<ArgumentOutOfRangeException>(() => field.Sample(new PointXY(0f, float.PositiveInfinity)));
        Assert.That(error!.ParamName, Is.EqualTo("point"));
        Assert.Throws<ArgumentOutOfRangeException>(() => field.Sample(new PointXY(float.MaxValue, 0f)));
    }

    [Test]
    public void CreatePerlinNoise_KeepsSubCellPrecisionAtLargeWorldOffset()
    {
        // Here a float cannot represent the half-cell offsets; the raster samples in double precision.
        const float originX = 16_777_216f;
        var grid = new RasterGeometry(new PointXY(originX, 0f), new VectorXY(4f, 1f), new VectorXYInt(4, 1));
        var raster = grid.CreatePerlinNoise(42, scale: 3f, offset: new VectorXY(-originX, 0f));
        var field = new PerlinNoiseField(42, scale: 3f);

        for (int x = 0; x < grid.Resolution.X; x++)
            Assert.That(raster[x, 0], Is.EqualTo(field.Sample(new PointXY(x + 0.5f, 0.5f))));
    }
}
