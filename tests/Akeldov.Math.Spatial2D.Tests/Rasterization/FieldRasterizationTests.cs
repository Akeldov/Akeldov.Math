using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class FieldRasterizationTests
{
    [Test]
    public void Rasterize_SamplesCellCentersInRowMajorOrderAndMapsValues()
    {
        var field = new RecordingField();
        var grid = new RasterGeometry(
            origin: new PointXY(10f, 20f),
            size: new VectorXY(4f, 2f),
            resolution: new VectorXYInt(2, 2));

        SpatialRaster<int> raster = field.Rasterize(grid, value => value * 10);

        Assert.Multiple(() =>
        {
            Assert.That(raster.Geometry, Is.EqualTo(grid));
            Assert.That(raster.Values, Is.EqualTo(new[] { 10, 20, 30, 40 }));
            Assert.That(field.SampledPoints, Is.EqualTo(new[]
            {
                new PointXY(11f, 20.5f),
                new PointXY(13f, 20.5f),
                new PointXY(11f, 21.5f),
                new PointXY(13f, 21.5f)
            }));
        });
    }

    [Test]
    public void Rasterize_ReturnsRasterWithCallerOwnedMutableValues()
    {
        var field = new ConstantField(3);

        SpatialRaster<string> raster = field.Rasterize(CreateGrid(), value => value.ToString());

        raster[0, 0] = "changed";

        Assert.That(raster[0, 0], Is.EqualTo("changed"));
    }

    [Test]
    public void Rasterize_WhenFieldIsNull_Throws()
    {
        IField<int> field = null!;

        var exception = Assert.Throws<ArgumentNullException>(() =>
            field.Rasterize(CreateGrid(), value => value));

        Assert.That(exception!.ParamName, Is.EqualTo("field"));
    }

    [Test]
    public void Rasterize_WhenSelectorIsNull_Throws()
    {
        var field = new ConstantField(1);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            field.Rasterize(CreateGrid(), (Func<int, int>)null!));

        Assert.That(exception!.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void Rasterize_WhenGridHasDefaultValue_Throws()
    {
        var field = new ConstantField(1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            field.Rasterize(default, value => value));

        Assert.That(exception!.ParamName, Is.EqualTo("grid"));
    }

    [Test]
    public void Rasterize_WhenCellCountExceedsArrayLength_Throws()
    {
        var field = new ConstantField(1);
        var grid = new RasterGeometry(
            new PointXY(0f, 0f),
            VectorXY.One,
            new VectorXYInt(int.MaxValue, 2));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            field.Rasterize(grid, value => value));

        Assert.That(exception!.ParamName, Is.EqualTo("grid"));
    }

    private static RasterGeometry CreateGrid()
    {
        return new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, VectorXYInt.One);
    }

    private sealed class ConstantField : IField<int>
    {
        private readonly int _value;

        public ConstantField(int value)
        {
            _value = value;
        }

        public int Sample(PointXY point)
        {
            return _value;
        }
    }

    private sealed class RecordingField : IField<int>
    {
        public List<PointXY> SampledPoints { get; } = new List<PointXY>();

        public int Sample(PointXY point)
        {
            SampledPoints.Add(point);
            return SampledPoints.Count;
        }
    }
}
