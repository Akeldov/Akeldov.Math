using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapRasterizationExtensionsTests
{
    [Test]
    public void SpatialHexMapToRasterGeometry_ReturnsGridForMapGeometry()
    {
        var geometry = new HexMapGeometry(
            width: 2,
            height: 3,
            origin: new VectorXY(10f, 20f),
            radius: 2f,
            layout: Layout.EvenQ);
        ISpatialHexMap<PointXY> map = new HexCenterMap(geometry);

        RasterGeometry actual = map.ToRasterGeometry(pixelsPerApothem: 4f, margin: 1.5f);
        RasterGeometry expected = geometry.ToRasterGeometry(pixelsPerApothem: 4f, margin: 1.5f);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void SpatialHexMapToRasterGeometry_WhenMapIsNull_Throws()
    {
        ISpatialHexMap<PointXY> map = null!;

        var exception = Assert.Throws<ArgumentNullException>(() => map.ToRasterGeometry(3f));

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }

    [Test]
    public void SpatialHexMapRasterize_WithCustomRasterGeometry_UsesWorldCoordinates()
    {
        var geometry = new HexMapGeometry(
            width: 1,
            height: 1,
            origin: new VectorXY(10f, 20f),
            radius: 2f,
            layout: Layout.OddR);
        var map = new SpatialHexMap<int>(geometry, new[] { 7 });
        var mapCenterGrid = new RasterGeometry(
            new PointXY(9.5f, 19.5f),
            VectorXY.One,
            VectorXYInt.One);
        var outsideGrid = new RasterGeometry(
            new PointXY(100f, 100f),
            VectorXY.One,
            VectorXYInt.One);

        SpatialRaster<int> mapCenterRaster = map.Rasterize(mapCenterGrid, value => value);
        SpatialRaster<int> outsideRaster = map.Rasterize(outsideGrid, value => value);

        Assert.Multiple(() =>
        {
            Assert.That(mapCenterRaster.Geometry, Is.EqualTo(mapCenterGrid));
            Assert.That(mapCenterRaster[0], Is.EqualTo(7));
            Assert.That(outsideRaster.Geometry, Is.EqualTo(outsideGrid));
            Assert.That(outsideRaster[0], Is.Zero);
        });
    }

    [Test]
    public void SpatialHexMapRasterize_WhenArgumentsAreNull_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, radius: 1f, layout: Layout.OddR);
        ISpatialHexMap<int> map = new SpatialHexMap<int>(geometry, new[] { 7 });
        ISpatialHexMap<int> nullMap = null!;
        RasterGeometry rasterGeometry = geometry.ToRasterGeometry(pixelsPerApothem: 1f);

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.Rasterize(1f, 0f, value => value))!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => nullMap.Rasterize(rasterGeometry, value => value))!.ParamName,
                Is.EqualTo("map"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => map.Rasterize<int, int>(rasterGeometry, null!))!.ParamName,
                Is.EqualTo("colorSelector"));
        });
    }

    [Test]
    public void SpatialHexMapRasterize_WhenTopologyDiffersFromGeometry_Throws()
    {
        var geometry = new HexMapGeometry(1, 1, radius: 1f, layout: Layout.OddR);
        ISpatialHexMap<int> map = new InconsistentSpatialHexMap(
            geometry,
            new HexMapTopology(1, 1, Layout.EvenR));
        RasterGeometry rasterGeometry = geometry.ToRasterGeometry(pixelsPerApothem: 1f);

        var exception = Assert.Throws<ArgumentException>(
            () => map.Rasterize(rasterGeometry, value => value));

        Assert.That(exception!.ParamName, Is.EqualTo("map"));
    }

    [Test]
    public void HexMapTopologyRasterize_WithMargin_ExpandsGridByMarginOnEachSide()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        SpatialRaster<Gray8BitColor> raster = topology.Rasterize(
            radius: 2f,
            options: new HexMapTopologyRasterizationOptions(
                margin: 1f,
                curveWidth: 0.5f,
                fadeDistance: 0.5f,
                curveColor: Gray8BitColor.White,
                backgroundColor: Gray8BitColor.Black,
                pixelsPerApothem: 3));

        Assert.Multiple(() =>
        {
            Assert.That(raster.Geometry.Origin.X, Is.EqualTo(-2.7321f).Within(0.0001f));
            Assert.That(raster.Geometry.Origin.Y, Is.EqualTo(-3f).Within(0.0001f));
            Assert.That(raster.Geometry.Size.X, Is.EqualTo(5.4641f).Within(0.0001f));
            Assert.That(raster.Geometry.Size.Y, Is.EqualTo(6f).Within(0.0001f));
            Assert.That(raster.Geometry.Resolution, Is.EqualTo(new VectorXYInt(10, 11)));
        });
    }

    [Test]
    public void HexMapTopologyRasterize_WithOrigin_ReturnsSameRasterAsGeometry()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var origin = new VectorXY(10f, 20f);
        var geometry = new HexMapGeometry(topology, origin, radius: 2f.ConvertHexApothemToRadius());

        SpatialRaster<Gray8BitColor> actual = topology.Rasterize(
            radius: 2f.ConvertHexApothemToRadius(),
            origin: origin,
            options: new HexMapTopologyRasterizationOptions(
                margin: 0f,
                curveWidth: 1f,
                fadeDistance: 1f,
                curveColor: Gray8BitColor.White,
                backgroundColor: Gray8BitColor.Black,
                pixelsPerApothem: 3));
        SpatialRaster<Gray8BitColor> expected = geometry.Rasterize(
            curveWidth: 1f,
            fadeDistance: 1f,
            curveColor: new Gray8BitColor(255),
            backgroundColor: Gray8BitColor.Black,
            pixelsPerApothem: 3);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Geometry, Is.EqualTo(expected.Geometry));
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        });
    }

    [Test]
    public void HexMapGeometryRasterize_UsesGeometryOrigin()
    {
        var geometry = new HexMapGeometry(
            width: 1,
            height: 1,
            origin: new VectorXY(10f, 20f),
            radius: 2f.ConvertHexApothemToRadius(),
            layout: Layout.OddR);
        RasterGeometry grid = geometry.ToRasterGeometry(pixelsPerApothem: 3f);

        SpatialRaster<Gray8BitColor> actual = geometry.Rasterize(
            curveWidth: 1f,
            fadeDistance: 1f,
            curveColor: Gray8BitColor.White,
            backgroundColor: Gray8BitColor.Black,
            pixelsPerApothem: 3);
        SpatialRaster<Gray8BitColor> expected = geometry
            .ToHexEdgeSegments()
            .Rasterize(
                curveWidth: 1f,
                fadeDistance: 1f,
                curveColor: Gray8BitColor.White,
                backgroundColor: Gray8BitColor.Black,
                rasterGeometry: grid);

        Assert.Multiple(() =>
        {
            Assert.That(expected.Values.Any(value => value != Gray8BitColor.Black), Is.True);
            Assert.That(actual.Geometry, Is.EqualTo(expected.Geometry));
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        });
    }

    [Test]
    public void HexMapTopologyRasterize_WithXYLabels_AddsLabelPixels()
    {
        TrueTypeFont? font = LoadSystemArial();
        if (font == null)
        {
            Assert.Ignore("Arial is not available on this machine.");
            return;
        }

        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var options = new HexMapTopologyRasterizationOptions(1f, 0.1f, 0f, Gray8BitColor.Black, Gray8BitColor.White, 20);
        SpatialRaster<Gray8BitColor> plain = topology.Rasterize(10f, options);
        SpatialRaster<Gray8BitColor> labeled = topology.Rasterize(
            10f,
            options,
            new HexMapTopologyXYLabelsRasterizationOptions(font, 4f, Gray8BitColor.Black, 0.2f));

        Assert.That(labeled.Values, Is.Not.EqualTo(plain.Values));
    }

    [Test]
    public void HexMapTopologyRasterize_WithQRSLabels_AddsLabelPixels()
    {
        TrueTypeFont? font = LoadSystemArial();
        if (font == null)
        {
            Assert.Ignore("Arial is not available on this machine.");
            return;
        }

        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var options = new HexMapTopologyRasterizationOptions(1f, 0.1f, 0f, Gray8BitColor.Black, Gray8BitColor.White, 20);
        SpatialRaster<Gray8BitColor> plain = topology.Rasterize(10f, options);
        SpatialRaster<Gray8BitColor> labeled = topology.Rasterize(
            10f,
            options,
            new HexMapTopologyQRSLabelsRasterizationOptions(font, 3f, Gray8BitColor.Black, 0.2f));

        Assert.That(labeled.Values, Is.Not.EqualTo(plain.Values));
    }

    [Test]
    public void HexMapTopologyRasterize_WithXYAndQRSLabels_AddsBothLabelLayers()
    {
        TrueTypeFont? font = LoadSystemArial();
        if (font == null)
        {
            Assert.Ignore("Arial is not available on this machine.");
            return;
        }

        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var options = new HexMapTopologyRasterizationOptions(1f, 0.1f, 0f, Gray8BitColor.Black, Gray8BitColor.White, 20);
        SpatialRaster<Gray8BitColor> plain = topology.Rasterize(10f, options);
        SpatialRaster<Gray8BitColor> labeled = topology.Rasterize(
            10f,
            options,
            new HexMapTopologyXYLabelsRasterizationOptions(
                font, 3f, Gray8BitColor.Black, 0.2f, new VectorXY(0f, 2f)),
            new HexMapTopologyQRSLabelsRasterizationOptions(
                font, 2f, Gray8BitColor.Black, 0.2f, new VectorXY(0f, -2f)));

        Assert.That(labeled.Values, Is.Not.EqualTo(plain.Values));
    }

    private static TrueTypeFont? LoadSystemArial()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "arial.ttf");
        return File.Exists(path) ? TrueTypeFont.Load(path) : null;
    }

    private sealed class InconsistentSpatialHexMap : ISpatialHexMap<int>
    {
        public InconsistentSpatialHexMap(HexMapGeometry geometry, HexMapTopology topology)
        {
            Geometry = geometry;
            Topology = topology;
        }

        public HexMapGeometry Geometry { get; }

        public HexMapTopology Topology { get; }

        public int this[VectorXYInt index] => default;

        public int this[int index] => default;
    }
}
