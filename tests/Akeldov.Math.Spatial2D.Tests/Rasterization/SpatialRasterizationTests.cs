using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class SpatialRasterizationTests
{
    [Test]
    public void Rasterize_SamplesCellCentersInRowMajorOrder()
    {
        var sampledPoints = new List<PointXY>();
        var grid = new RasterGeometry(
            origin: new PointXY(10f, 20f),
            size: new VectorXY(4f, 6f),
            resolution: new VectorXYInt(2, 3));

        SpatialRaster<PointXY> raster = grid.Rasterize(point =>
        {
            sampledPoints.Add(point);
            return point;
        });

        var expectedPoints = new[]
        {
            new PointXY(11f, 21f),
            new PointXY(13f, 21f),
            new PointXY(11f, 23f),
            new PointXY(13f, 23f),
            new PointXY(11f, 25f),
            new PointXY(13f, 25f)
        };

        Assert.Multiple(() =>
        {
            Assert.That(raster.Geometry, Is.EqualTo(grid));
            Assert.That(raster.Values, Is.EqualTo(expectedPoints));
            Assert.That(sampledPoints, Is.EqualTo(expectedPoints));
        });
    }

    [Test]
    public void Rasterize_SupportsReferenceValues()
    {
        var first = new object();
        var second = new object();
        int sampleIndex = 0;
        var grid = new RasterGeometry(default, VectorXY.One, new VectorXYInt(2, 1));

        SpatialRaster<object> raster = grid.Rasterize(_ => sampleIndex++ == 0 ? first : second);

        Assert.Multiple(() =>
        {
            Assert.That(raster.Values[0], Is.SameAs(first));
            Assert.That(raster.Values[1], Is.SameAs(second));
        });
    }

    [Test]
    public void Rasterize_InvokesSamplerOncePerCell()
    {
        int sampleCount = 0;
        var grid = new RasterGeometry(default, VectorXY.One, new VectorXYInt(3, 4));

        SpatialRaster<int> raster = grid.Rasterize(_ => ++sampleCount);

        Assert.Multiple(() =>
        {
            Assert.That(sampleCount, Is.EqualTo(12));
            Assert.That(raster.Values, Is.EqualTo(Enumerable.Range(1, 12)));
        });
    }

    [Test]
    public void Rasterize_PreservesExistingCellCenterFloatArithmetic()
    {
        var sampledPoints = new List<PointXY>();
        var grid = new RasterGeometry(
            origin: new PointXY(0.1f, 0f),
            size: new VectorXY(0.2f, 1f),
            resolution: new VectorXYInt(7, 1));

        grid.Rasterize(point =>
        {
            sampledPoints.Add(point);
            return 0;
        });

        float firstX = grid.Origin.X + grid.CellSize.X * 0.5f;
        float expectedX = firstX + 4 * grid.CellSize.X;

        Assert.Multiple(() =>
        {
            Assert.That(
                BitConverter.SingleToInt32Bits(sampledPoints[4].X),
                Is.EqualTo(BitConverter.SingleToInt32Bits(expectedX)));
            Assert.That(sampledPoints[4].X, Is.Not.EqualTo(grid.GetCellCenter(4, 0).X));
        });
    }

    [Test]
    public void Rasterize_WhenSampleIsNull_Throws()
    {
        RasterGeometry grid = CreateGrid();

        var exception = Assert.Throws<ArgumentNullException>(() => grid.Rasterize((Func<PointXY, int>)null!));

        Assert.That(exception!.ParamName, Is.EqualTo("sample"));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        int sampleCount = 0;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            default(RasterGeometry).Rasterize(_ => ++sampleCount));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.ParamName, Is.EqualTo("grid"));
            Assert.That(sampleCount, Is.Zero);
        });
    }

    [Test]
    public void Rasterize_WhenCellCountExceedsArrayLength_Throws()
    {
        var grid = new RasterGeometry(default, VectorXY.One, new VectorXYInt(int.MaxValue, 2));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => grid.Rasterize(_ => 1));

        Assert.That(exception!.ParamName, Is.EqualTo("grid"));
    }

    [Test]
    public void Rasterize_WhenSamplerThrows_PropagatesException()
    {
        var expected = new InvalidOperationException("Sampling failed.");
        RasterGeometry grid = CreateGrid();

        InvalidOperationException? actual = Assert.Throws<InvalidOperationException>(() => grid.Rasterize<int>(_ => throw expected));

        Assert.That(actual, Is.SameAs(expected));
    }

    private static RasterGeometry CreateGrid() => new RasterGeometry(default, VectorXY.One, VectorXYInt.One);
}
