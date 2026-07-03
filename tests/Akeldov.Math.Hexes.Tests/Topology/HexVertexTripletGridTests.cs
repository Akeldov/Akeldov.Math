using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexVertexTripletGridTests
{
    [Test]
    public void IndexTripletGrid_ExposesGeometryResolutionAndSampledValues()
    {
        var origin = new VectorXY(10f, -20f);

        var grid = new IndexTripletGrid(2, 1, Layout.OddR, origin, new VectorXYInt(4, 2));

        Assert.That(grid.HexResolution, Is.EqualTo(new VectorXYInt(2, 1)));
        Assert.That(grid.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(4, 2)));
        Assert.That(grid.ResolutionX, Is.EqualTo(4));
        Assert.That(grid.ResolutionY, Is.EqualTo(2));
        Assert.That(grid.Count, Is.EqualTo(8));
        Assert.That(grid[0], Is.EqualTo(grid[VectorXYInt.Zero]));
    }

    [Test]
    public void GridTypes_DoNotExposeBackingCollections()
    {
        Assert.That(typeof(IndexTripletGrid).GetProperty("IndexTriplets"), Is.Null);
        Assert.That(typeof(IndexPartialTripletGrid).GetProperty("IndexTriplets"), Is.Null);
        Assert.That(typeof(BarycentricTripletGrid).GetProperty("BarycentricCoordinates"), Is.Null);
        Assert.That(typeof(BarycentricPartialTripletGrid).GetProperty("BarycentricCoordinates"), Is.Null);
        Assert.That(typeof(ChromaticIndexTripletGrid).GetProperty("ChromaticIndices"), Is.Null);
        Assert.That(typeof(ChromaticIndexPartialTripletGrid).GetProperty("ChromaticIndices"), Is.Null);
    }

    [Test]
    public void IndexTripletGrid_UsesNearestVertexAndLeftRightOrder()
    {
        VectorXY point = GetPointNearOddRVertex0();
        var expected = new Triplet<VectorXYInt>(
            new VectorXYInt(0, 0),
            new VectorXYInt(0, 1),
            new VectorXYInt(1, 0));
        var grid = CreateSingleSampleIndexTripletGrid(point);

        Triplet<VectorXYInt> actual = grid[VectorXYInt.Zero];

        AssertTriplet(actual, expected);
        Assert.That(grid.TryGetValue(VectorXYInt.Zero, out Triplet<VectorXYInt> fromTry), Is.True);
        AssertTriplet(fromTry, expected);
    }

    [TestCase(Layout.OddR, 0, 1, 1, 0)]
    [TestCase(Layout.EvenR, 1, 1, 1, 0)]
    [TestCase(Layout.OddQ, 1, 0, 1, -1)]
    [TestCase(Layout.EvenQ, 1, 1, 1, 0)]
    public void GetAdjacentTriplet_UsesLayoutSpecificVertexEdges(
        Layout layout,
        int expectedLeftX,
        int expectedLeftY,
        int expectedRightX,
        int expectedRightY)
    {
        Triplet<VectorXYInt> triplet = VectorXYInt.Zero.GetAdjacentTriplet(HexVertex.Vertex0, layout);

        AssertTriplet(
            triplet,
            new Triplet<VectorXYInt>(
                VectorXYInt.Zero,
                new VectorXYInt(expectedLeftX, expectedLeftY),
                new VectorXYInt(expectedRightX, expectedRightY)));
    }

    [Test]
    public void GetAdjacentEdges_WhenVertexIsInvalid_Throws()
    {
        var invalid = (HexVertex)6;

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetAdjacentEdges());
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetAdjacentEdges(Layout.OddR));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = VectorXYInt.Zero.GetAdjacentPair(invalid, Layout.OddR));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = VectorXYInt.Zero.GetAdjacentTriplet(invalid, Layout.OddR));
        });
    }

    [Test]
    public void ChromaticIndexTripletGrid_UsesSameVertexTripletOrder()
    {
        VectorXY point = GetPointNearOddRVertex0();
        var indexTriplet = new Triplet<VectorXYInt>(
            new VectorXYInt(0, 0),
            new VectorXYInt(0, 1),
            new VectorXYInt(1, 0));
        Triplet<byte> expected = indexTriplet.GetChromaticTriplet(Layout.OddR);
        var grid = CreateSingleSampleChromaticIndexGrid(point);

        Triplet<byte> actual = grid[VectorXYInt.Zero];

        AssertTriplet(actual, expected);
        Assert.That(grid.TryGetValue(VectorXYInt.Zero, out Triplet<byte> fromTry), Is.True);
        AssertTriplet(fromTry, expected);
    }

    [Test]
    public void TryGetMethods_WhenGridIndexIsOutside_ReturnFalse()
    {
        var adjacency = new IndexSeptupletMap(2, 2, Layout.OddR);
        var resolution = VectorXYInt.One;
        var indexGrid = new IndexTripletGrid(adjacency, resolution);
        var partialIndexGrid = new IndexPartialTripletGrid(adjacency, resolution);
        var barycentricGrid = new BarycentricTripletGrid(adjacency, resolution);
        var partialBarycentricGrid = new BarycentricPartialTripletGrid(adjacency, resolution);
        var chromaticGrid = new ChromaticIndexTripletGrid(adjacency, resolution);
        var outsideIndex = new VectorXYInt(1, 0);

        Assert.That(indexGrid.TryGetValue(outsideIndex, out Triplet<VectorXYInt> indexTriplet), Is.False);
        Assert.That(indexTriplet, Is.EqualTo(default(Triplet<VectorXYInt>)));
        Assert.That(partialIndexGrid.TryGetValue(outsideIndex, out PartialTriplet<VectorXYInt> partialIndexTriplet), Is.False);
        Assert.That(partialIndexTriplet, Is.EqualTo(default(PartialTriplet<VectorXYInt>)));
        Assert.That(barycentricGrid.TryGetValue(outsideIndex, out Triplet<float> barycentricCoordinates), Is.False);
        Assert.That(barycentricCoordinates, Is.EqualTo(default(Triplet<float>)));
        Assert.That(partialBarycentricGrid.TryGetValue(outsideIndex, out PartialTriplet<float> partialBarycentricCoordinates), Is.False);
        Assert.That(partialBarycentricCoordinates, Is.EqualTo(default(PartialTriplet<float>)));
        Assert.That(chromaticGrid.TryGetValue(outsideIndex, out Triplet<byte> chromaticIndices), Is.False);
        Assert.That(chromaticIndices, Is.EqualTo(default(Triplet<byte>)));
    }

    [Test]
    public void Constructors_WhenArgumentsAreInvalid_Throw()
    {
        var emptyAdjacency = new IndexSeptupletMap(0, 1, Layout.OddR);
        var emptyPartialAdjacency = new IndexPartialSeptupletMap(0, 1, Layout.OddR);

        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexTripletGrid(0, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletGrid(0, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, new VectorXYInt(0, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, new VectorXY(float.PositiveInfinity, 0f), VectorXY.One, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarycentricTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, VectorXY.Zero, new VectorXY(0f, 1f), VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, VectorXY.Zero, new VectorXY(float.PositiveInfinity, 1f), VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSeptupletGrid(emptyAdjacency, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSeptupletGrid(new IndexSeptupletMap(1, 1, Layout.OddR), VectorXYInt.One, default));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialSeptupletGrid(emptyPartialAdjacency, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialTripletGrid(emptyAdjacency, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarycentricTripletGrid(emptyAdjacency, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarycentricPartialTripletGrid(emptyAdjacency, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletGrid(emptyAdjacency, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexPartialTripletGrid(emptyAdjacency, VectorXYInt.One));
    }

    private static VectorXY GetPointNearOddRVertex0()
    {
        const float hexRadius = 1f;
        return new VectorXY(
            Akeldov.Math.Hexes.Geometry.Constants.Cos30Deg * hexRadius * 0.75f,
            Akeldov.Math.Hexes.Geometry.Constants.Sin30Deg * hexRadius * 0.75f);
    }

    private static IndexTripletGrid CreateSingleSampleIndexTripletGrid(VectorXY point)
    {
        return CreateSingleSampleIndexTripletGrid(point, 2, 2);
    }

    private static IndexTripletGrid CreateSingleSampleIndexTripletGrid(
        VectorXY point,
        int hexWidth,
        int hexHeight)
    {
        return new IndexTripletGrid(
            hexWidth,
            hexHeight,
            Layout.OddR,
            VectorXY.Zero,
            point - new VectorXY(0.5f, 0.5f),
            VectorXY.One,
            VectorXYInt.One);
    }

    private static ChromaticIndexTripletGrid CreateSingleSampleChromaticIndexGrid(VectorXY point)
    {
        return CreateSingleSampleChromaticIndexGrid(point, 2, 2);
    }

    private static ChromaticIndexTripletGrid CreateSingleSampleChromaticIndexGrid(
        VectorXY point,
        int hexWidth,
        int hexHeight)
    {
        return new ChromaticIndexTripletGrid(
            hexWidth,
            hexHeight,
            Layout.OddR,
            VectorXY.Zero,
            point - new VectorXY(0.5f, 0.5f),
            VectorXY.One,
            VectorXYInt.One);
    }

    private static void AssertTriplet<T>(Triplet<T> actual, Triplet<T> expected)
    {
        Assert.That(actual.Main, Is.EqualTo(expected.Main));
        Assert.That(actual.Left, Is.EqualTo(expected.Left));
        Assert.That(actual.Right, Is.EqualTo(expected.Right));
    }

}
