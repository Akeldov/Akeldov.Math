using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapRasterizationExtensionsTests
{
    [Test]
    public void HexMapTopologyRasterize_WithMargin_ExpandsGridByMarginOnEachSide()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);

        SpatialRaster<byte> raster = topology.Rasterize(
            apothem: 2f,
            options: new HexMapTopologyRasterizationOptions(
                margin: 1f,
                curveWidth: 0.5f,
                fadeDistance: 0.5f,
                curveColor: 255,
                backgroundColor: 0,
                pixelsPerApothem: 3));

        Assert.Multiple(() =>
        {
            Assert.That(raster.Grid.Origin.X, Is.EqualTo(-3f).Within(0.0001f));
            Assert.That(raster.Grid.Origin.Y, Is.EqualTo(-3.3094f).Within(0.0001f));
            Assert.That(raster.Grid.Size.X, Is.EqualTo(6f).Within(0.0001f));
            Assert.That(raster.Grid.Size.Y, Is.EqualTo(6.6188f).Within(0.0001f));
            Assert.That(raster.Grid.Resolution, Is.EqualTo(new VectorXYInt(9, 10)));
        });
    }

    [Test]
    public void HexMapTopologyRasterize_WithOrigin_ReturnsSameRasterAsGeometry()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        var origin = new VectorXY(10f, 20f);
        var geometry = new HexMapGeometry(topology, origin, apothem: 2f);

        SpatialRaster<byte> actual = topology.Rasterize(
            apothem: 2f,
            origin: origin,
            options: new HexMapTopologyRasterizationOptions(
                margin: 0f,
                curveWidth: 1f,
                fadeDistance: 1f,
                curveColor: 255,
                backgroundColor: 0,
                pixelsPerApothem: 3));
        SpatialRaster<byte> expected = geometry.Rasterize(
            curveWidth: 1f,
            fadeDistance: 1f,
            curveColor: 255,
            backgroundColor: 0,
            pixelsPerApothem: 3);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Grid, Is.EqualTo(expected.Grid));
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
            apothem: 2f,
            layout: Layout.OddR);
        SpatialRasterGrid grid = geometry.ToSpatialRasterGrid(pixelsPerApothem: 3f);

        SpatialRaster<byte> actual = geometry.Rasterize(
            curveWidth: 1f,
            fadeDistance: 1f,
            curveColor: 255,
            backgroundColor: 0,
            pixelsPerApothem: 3);
        SpatialRaster<byte> expected = geometry
            .ToHexEdgeSegments()
            .Rasterize(
                curveWidth: 1f,
                fadeDistance: 1f,
                curveColor: (byte)255,
                backgroundColor: 0,
                spatialRasterGrid: grid);

        Assert.Multiple(() =>
        {
            Assert.That(expected.Values.Any(value => value != 0), Is.True);
            Assert.That(actual.Grid, Is.EqualTo(expected.Grid));
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
        var options = new HexMapTopologyRasterizationOptions(1f, 0.1f, 0f, 0, 255, 20);
        SpatialRaster<byte> plain = topology.Rasterize(10f, options);
        SpatialRaster<byte> labeled = topology.Rasterize(
            10f,
            options,
            new HexMapTopologyXYLabelsRasterizationOptions(font, 4f, 0, 0.2f));

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
        var options = new HexMapTopologyRasterizationOptions(1f, 0.1f, 0f, 0, 255, 20);
        SpatialRaster<byte> plain = topology.Rasterize(10f, options);
        SpatialRaster<byte> labeled = topology.Rasterize(
            10f,
            options,
            new HexMapTopologyQRSLabelsRasterizationOptions(font, 3f, 0, 0.2f));

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
        var options = new HexMapTopologyRasterizationOptions(1f, 0.1f, 0f, 0, 255, 20);
        SpatialRaster<byte> plain = topology.Rasterize(10f, options);
        SpatialRaster<byte> labeled = topology.Rasterize(
            10f,
            options,
            new HexMapTopologyXYLabelsRasterizationOptions(
                font, 3f, 0, 0.2f, new VectorXY(0f, 2f)),
            new HexMapTopologyQRSLabelsRasterizationOptions(
                font, 2f, 0, 0.2f, new VectorXY(0f, -2f)));

        Assert.That(labeled.Values, Is.Not.EqualTo(plain.Values));
    }

    private static TrueTypeFont? LoadSystemArial()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
            "arial.ttf");
        return File.Exists(path) ? TrueTypeFont.Load(path) : null;
    }
}
