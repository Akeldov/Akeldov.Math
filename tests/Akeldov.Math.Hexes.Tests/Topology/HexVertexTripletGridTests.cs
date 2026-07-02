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
        Assert.That(grid.IndexTriplets, Has.Count.EqualTo(8));
        Assert.That(grid.IndexTriplets, Is.Not.InstanceOf<Triplet<VectorXYInt>[]>());
    }

    [Test]
    public void RetainedGridCollections_DoNotExposeBackingArrays()
    {
        var adjacency = new IndexSeptupletMap(2, 2, Layout.OddR);
        var resolution = new VectorXYInt(2, 1);
        var indexGrid = new IndexTripletGrid(adjacency, resolution);
        var partialIndexGrid = new IndexPartialTripletGrid(adjacency, resolution);
        var barycentricGrid = new BarycentricTripletGrid(adjacency, resolution);
        var partialBarycentricGrid = new BarycentricPartialTripletGrid(adjacency, resolution);
        var chromaticGrid = new ChromaticIndexTripletGrid(adjacency, resolution);
        var partialChromaticGrid = new ChromaticIndexPartialTripletGrid(adjacency, resolution);

        Assert.That(indexGrid.IndexTriplets, Has.Count.EqualTo(indexGrid.Count));
        Assert.That(indexGrid.IndexTriplets, Is.Not.InstanceOf<Triplet<VectorXYInt>[]>());
        Assert.That(partialIndexGrid.IndexTriplets, Has.Count.EqualTo(partialIndexGrid.Count));
        Assert.That(partialIndexGrid.IndexTriplets, Is.Not.InstanceOf<PartialTriplet<VectorXYInt>[]>());
        Assert.That(barycentricGrid.BarycentricCoordinates, Has.Count.EqualTo(barycentricGrid.Count));
        Assert.That(barycentricGrid.BarycentricCoordinates, Is.Not.InstanceOf<Triplet<float>[]>());
        Assert.That(partialBarycentricGrid.BarycentricCoordinates, Has.Count.EqualTo(partialBarycentricGrid.Count));
        Assert.That(partialBarycentricGrid.BarycentricCoordinates, Is.Not.InstanceOf<PartialTriplet<float>[]>());
        Assert.That(chromaticGrid.ChromaticIndices, Has.Count.EqualTo(chromaticGrid.Count));
        Assert.That(chromaticGrid.ChromaticIndices, Is.Not.InstanceOf<Triplet<byte>[]>());
        Assert.That(partialChromaticGrid.ChromaticIndices, Has.Count.EqualTo(partialChromaticGrid.Count));
        Assert.That(partialChromaticGrid.ChromaticIndices, Is.Not.InstanceOf<PartialTriplet<byte>[]>());
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
        Assert.That(grid.TryGetIndexTriplet(VectorXYInt.Zero, out Triplet<VectorXYInt> fromTry), Is.True);
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
        Assert.That(grid.TryGetChromaticIndices(VectorXYInt.Zero, out Triplet<byte> fromTry), Is.True);
        AssertTriplet(fromTry, expected);
    }

    [Test]
    public void Constructors_WhenArgumentsAreInvalid_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexTripletGrid(0, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletGrid(0, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, new VectorXYInt(0, 1)));
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
