using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexVertexTripletGridRGBA16BitRasterExtensionsTests
{
    [Test]
    public void IndexTripletGrid_ToRGBA16BitRaster_MapsHitCellsByIndexTriplet()
    {
        var grid = new IndexTripletGrid(new HexMapTopology(2, 1, Layout.OddR), new VectorXYInt(4, 1));
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        var raster = grid.Rasterize((Triplet<VectorXYInt> triplet) => triplet.Main.X == 0 ? red : blue);

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
        var grid = new ChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One);
        var red = new RGBA16BitColor(ushort.MaxValue, 0, 0, ushort.MaxValue);
        var blue = new RGBA16BitColor(0, 0, ushort.MaxValue, ushort.MaxValue);

        SpatialRaster<RGBA16BitColor> raster = grid.Rasterize(
            triplet => triplet.Main == 0 ? red : blue);

        Assert.That(raster.Values, Is.EqualTo(new[] { red }));
    }

    [Test]
    public void ToRGBA16BitRaster_WhenGridIsNull_Throws()
    {
        IndexTripletGrid indexTripletGrid = null!;
        BarycentricTripletGrid barycentricGrid = null!;
        ChromaticIndexTripletGrid chromaticIndexTripletGrid = null!;

        Assert.Throws<ArgumentNullException>(() => indexTripletGrid.Rasterize((Triplet<VectorXYInt> _) => (RGBA16BitColor)default));
        Assert.Throws<ArgumentNullException>(() => barycentricGrid.MapValues((Triplet<float> _) => (RGBA16BitColor)default));
        Assert.Throws<ArgumentNullException>(() => chromaticIndexTripletGrid.Rasterize((Triplet<byte> _) => (RGBA16BitColor)default));
    }
}
