using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class ISpatialRasterTests
{
    [Test]
    public void SpatialRaster_WhenGeometryHasDefaultValue_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialRaster<int>(default, Array.Empty<int>()));

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void SpatialRaster_WhenCellCountExceedsArrayCapacity_Throws()
    {
        var geometry = new RasterGeometry(
            new PointXY(0f, 0f),
            VectorXY.One,
            new VectorXYInt(50_000, 50_000));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialRaster<int>(geometry, Array.Empty<int>()));

        Assert.That(exception!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void SpatialRaster_ImplementsSpatialRasterContract()
    {
        var geometry = new RasterGeometry(
            new PointXY(0f, 0f),
            new VectorXY(2f, 1f),
            new VectorXYInt(2, 1));
        ISpatialRaster<int> raster = new SpatialRaster<int>(geometry, new[] { 10, 20 });

        Assert.Multiple(() =>
        {
            Assert.That(raster.Geometry, Is.EqualTo(geometry));
            Assert.That(raster.Resolution, Is.EqualTo(geometry.Resolution));
            Assert.That(raster[0], Is.EqualTo(10));
            Assert.That(raster[1], Is.EqualTo(20));
        });
    }
}
