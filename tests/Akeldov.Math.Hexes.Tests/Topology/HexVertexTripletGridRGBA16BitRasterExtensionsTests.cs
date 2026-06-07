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
        var grid = new IndexTripletGrid(
            1,
            1,
            Layout.OddR,
            VectorXY.Zero,
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
        var grid = new IndexTripletGrid(2, 1, Layout.OddR, VectorXY.Zero, new VectorXYInt(4, 1));
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
    public void ChromaticIndexTripletGrid_ToRGBA16BitRaster_MapsHitCellsByChromaticIndices()
    {
        var grid = new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        RGBA16BitRaster raster = grid.ToRGBA16BitRaster(
            triplet => triplet.Main == 0 ? red : blue);

        Assert.That(raster.Values, Is.EqualTo(new[] { red }));
    }

    [Test]
    public void ToRGBA16BitRaster_WhenGridIsNull_Throws()
    {
        IndexTripletGrid indexTripletGrid = null!;
        BarycentricTripletGrid barycentricGrid = null!;
        HexVertexChromaticIndexTripletGrid chromaticIndexTripletGrid = null!;

        Assert.Throws<ArgumentNullException>(() => indexTripletGrid.ToRGBA16BitRaster(_ => default));
        Assert.Throws<ArgumentNullException>(() => barycentricGrid.ToRGBA16BitRaster(_ => default));
        Assert.Throws<ArgumentNullException>(() => chromaticIndexTripletGrid.ToRGBA16BitRaster(_ => default));
    }
}
