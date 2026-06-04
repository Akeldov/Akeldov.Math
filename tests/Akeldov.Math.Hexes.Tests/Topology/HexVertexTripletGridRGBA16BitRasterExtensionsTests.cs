using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexVertexTripletGridRGBA16BitRasterExtensionsTests
{
    [Test]
    public void IndexTripletGrid_ToRGBA16BitRaster_UsesGridGeometry()
    {
        var grid = new HexVertexIndexTripletGrid(
            1,
            1,
            Layout.OddR,
            VectorXY.Zero,
            2f,
            new VectorXY(-1f, -2f),
            new VectorXY(4f, 5f),
            new VectorXYInt(3, 2));

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(_ => new RGBA16BitColor(1, 2, 3, 4));

        Assert.That(raster.Grid, Is.EqualTo(new RasterGrid(
            new PointXY(-1f, -2f),
            new VectorXY(4f, 5f),
            new VectorXYInt(3, 2))));
        Assert.That(raster.Values, Has.Length.EqualTo(6));
    }

    [Test]
    public void IndexTripletGrid_ToRGBA16BitRaster_MapsHitCellsByIndexTriplet()
    {
        var grid = new HexVertexIndexTripletGrid(2, 1, Layout.OddR, VectorXY.Zero, 2f, new VectorXYInt(4, 1));
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            triplet => triplet.Main.X == 0 ? red : blue);

        Assert.That(raster.Values, Is.EqualTo(new[]
        {
            red,
            red,
            blue,
            blue
        }));
    }

    [Test]
    public void BarycentricGrid_ToRGBA16BitRaster_MapsHitCellsByBarycentricCoordinates()
    {
        var grid = new HexVertexBarycentricGrid(1, 1, Layout.OddR, VectorXY.Zero, 2f, VectorXYInt.One);
        var color = new RGBA16BitColor(1, 2, 3, 4);
        Triplet<float> mapped = default;

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            coordinates =>
            {
                mapped = coordinates;
                return color;
            });

        Assert.That(raster.Values, Is.EqualTo(new[] { color }));
        Assert.That(mapped.Main + mapped.Left + mapped.Right, Is.EqualTo(1f).Within(0.000001f));
    }

    [Test]
    public void ChromaticIndexTripletGrid_ToRGBA16BitRaster_MapsHitCellsByChromaticIndices()
    {
        var grid = new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 2f, VectorXYInt.One);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            triplet => triplet.Main == 0 ? red : blue);

        Assert.That(raster.Values, Is.EqualTo(new[] { red }));
    }

    [Test]
    public void ToRGBA16BitRaster_UsesEmptyColorForMissCells()
    {
        var grid = new HexVertexIndexTripletGrid(
            1,
            1,
            Layout.OddR,
            VectorXY.Zero,
            2f,
            new VectorXY(100f, 100f),
            VectorXY.One,
            VectorXYInt.One);
        var emptyColor = new RGBA16BitColor(1, 2, 3, 4);
        var mapperWasCalled = false;

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            _ =>
            {
                mapperWasCalled = true;
                return new RGBA16BitColor(9, 9, 9, 9);
            });

        Assert.That(mapperWasCalled, Is.False);
        Assert.That(raster.Values, Is.EqualTo(new[] { emptyColor }));
    }

    [Test]
    public void ToRGBA16BitRaster_WhenGridIsNull_Throws()
    {
        HexVertexIndexTripletGrid indexTripletGrid = null!;
        HexVertexBarycentricGrid barycentricGrid = null!;
        HexVertexChromaticIndexTripletGrid chromaticIndexTripletGrid = null!;

        Assert.Throws<ArgumentNullException>(() => indexTripletGrid.ToRGBA16BitRaster(_ => default));
        Assert.Throws<ArgumentNullException>(() => barycentricGrid.ToRGBA16BitRaster(_ => default));
        Assert.Throws<ArgumentNullException>(() => chromaticIndexTripletGrid.ToRGBA16BitRaster(_ => default));
    }

    [Test]
    public void ToRGBA16BitRaster_WhenMapperIsNull_Throws()
    {
        var indexTripletGrid = new HexVertexIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 2f, VectorXYInt.One);
        var barycentricGrid = new HexVertexBarycentricGrid(1, 1, Layout.OddR, VectorXY.Zero, 2f, VectorXYInt.One);
        var chromaticIndexTripletGrid = new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 2f, VectorXYInt.One);

        Assert.Throws<ArgumentNullException>(() => indexTripletGrid.ToRGBA16BitRaster(null!));
        Assert.Throws<ArgumentNullException>(() => barycentricGrid.ToRGBA16BitRaster(null!));
        Assert.Throws<ArgumentNullException>(() => chromaticIndexTripletGrid.ToRGBA16BitRaster(null!));
    }
}
