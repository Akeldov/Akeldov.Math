using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Imaging;

public class DistanceColorDrawingTests
{
    private static SpatialRaster<RGBA8BitColor> CreateRaster(RGBA8BitColor background = default) =>
        new(new RasterGeometry(new PointXY(10, 20), new VectorXY(2, 4), new VectorXYInt(1, 1)), new[] { background });

    [Test]
    public void Draw_DistanceOverloadsUseSignedOrUnsignedContract()
    {
        var disk = new Disk(new PointXY(11, 22), 10);
        var signed = CreateRaster();
        var unsigned = CreateRaster();
        signed.Draw((ISignedPointDistanceProvider)disk, d => d < 0 ? RGBA8BitColor.Black : default);
        unsigned.Draw((IPointDistanceProvider)disk, d => d < 0 ? RGBA8BitColor.Black : default);
        Assert.That(signed.Values[0], Is.EqualTo(RGBA8BitColor.Black));
        Assert.That(unsigned.Values[0], Is.EqualTo(default(RGBA8BitColor)));
    }

    [Test]
    public void Draw_ParameterizedCurvePassesDistanceThenWorldLengthCoordinate()
    {
        var raster = CreateRaster();
        var curve = new ParameterizedSegment(new PointXY(0, 18), new PointXY(30, 18));
        var samples = new List<(float Distance, float Parameter)>();
        raster.Draw(curve, (distance, parameter) =>
        {
            samples.Add((distance, parameter));
            return RGBA8BitColor.Black;
        });
        var expected = new List<(float, float)>();
        for (int sy = 0; sy < 4; sy++)
            for (int sx = 0; sx < 4; sx++)
                expected.Add((2.5f + sy, 10.25f + sx * 0.5f));
        Assert.That(samples, Is.EqualTo(expected));
        Assert.That(raster.Values[0], Is.EqualTo(RGBA8BitColor.Black));
    }

    [Test]
    public void Draw_AveragesPremultipliedSamplesAndCompositesOnce()
    {
        var raster = CreateRaster(new RGBA8BitColor(0, 0, 255, 128));
        IParameterizedCurve curve = new ParameterizedSegment(new PointXY(0, 18), new PointXY(30, 18));
        raster.Draw(curve, (distance, parameter) => parameter < 11
            ? new RGBA8BitColor(255, 0, 0, 128)
            : new RGBA8BitColor(0, 255, 0, 0));
        Assert.That(raster.Values[0], Is.EqualTo(new RGBA8BitColor(102, 0, 153, 160)));
    }

    [Test]
    public void Draw_TransparentSamplesPreserveBackground()
    {
        var background = new RGBA8BitColor(23, 45, 67, 0);
        var raster = CreateRaster(background);
        raster.Draw((IPointDistanceProvider)new Disk(default, 1), _ => new RGBA8BitColor(255, 0, 0, 0));
        Assert.That(raster.Values[0], Is.EqualTo(background));
    }

    [Test]
    public void Draw_RejectsNullArgumentsForEveryOverload()
    {
        var raster = CreateRaster();
        IPointDistanceProvider unsigned = new Disk(default, 1);
        ISignedPointDistanceProvider signed = new Disk(default, 1);
        IParameterizedCurve curve = new ParameterizedSegment(default, new PointXY(1, 0));
        Assert.Throws<ArgumentNullException>(() => raster.Draw((IPointDistanceProvider)null!, _ => default));
        Assert.Throws<ArgumentNullException>(() => raster.Draw((ISignedPointDistanceProvider)null!, _ => default));
        Assert.Throws<ArgumentNullException>(() => raster.Draw((IParameterizedCurve)null!, (_, _) => default));
        Assert.Throws<ArgumentNullException>(() => raster.Draw(unsigned, null!));
        Assert.Throws<ArgumentNullException>(() => raster.Draw(signed, null!));
        Assert.Throws<ArgumentNullException>(() => raster.Draw(curve, (Func<float, float, RGBA8BitColor>)null!));
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.Draw(null!, unsigned, _ => default));
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.Draw(null!, signed, _ => default));
        Assert.Throws<ArgumentNullException>(() => SpatialRasterDrawingExtensions.Draw(null!, curve, (_, _) => default));
    }
}
