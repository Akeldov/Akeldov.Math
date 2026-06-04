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

        var grid = new HexVertexIndexTripletGrid(2, 1, Layout.OddR, origin, new VectorXYInt(4, 2));

        Assert.That(grid.HexResolution, Is.EqualTo(new VectorXYInt(2, 1)));
        Assert.That(grid.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(4, 2)));
        Assert.That(grid.ResolutionX, Is.EqualTo(4));
        Assert.That(grid.ResolutionY, Is.EqualTo(2));
        Assert.That(grid.Count, Is.EqualTo(8));
        Assert.That(grid.IndexTriplets, Has.Length.EqualTo(8));
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
        var grid = CreateSingleSampleChromaticGrid(point);

        Triplet<byte> actual = grid[VectorXYInt.Zero];

        AssertTriplet(actual, expected);
        Assert.That(grid.TryGetChromaticIndices(VectorXYInt.Zero, out Triplet<byte> fromTry), Is.True);
        AssertTriplet(fromTry, expected);
    }

    [Test]
    public void Constructors_WhenArgumentsAreInvalid_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexIndexTripletGrid(0, 1, Layout.OddR, VectorXY.Zero, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 0f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 1f, new VectorXYInt(0, 1)));
    }

    private static VectorXY GetPointNearOddRVertex0()
    {
        const float hexApothem = 2f;
        float hexRadius = hexApothem.ConvertHexApothemToRadius();
        return new VectorXY(
            Akeldov.Math.Hexes.Geometry.Constants.Cos30Deg * hexRadius * 0.75f,
            Akeldov.Math.Hexes.Geometry.Constants.Sin30Deg * hexRadius * 0.75f);
    }

    private static HexVertexIndexTripletGrid CreateSingleSampleIndexTripletGrid(VectorXY point)
    {
        return CreateSingleSampleIndexTripletGrid(point, 2, 2);
    }

    private static HexVertexIndexTripletGrid CreateSingleSampleIndexTripletGrid(
        VectorXY point,
        int hexWidth,
        int hexHeight)
    {
        return new HexVertexIndexTripletGrid(
            hexWidth,
            hexHeight,
            Layout.OddR,
            VectorXY.Zero,
            point - new VectorXY(0.5f, 0.5f),
            VectorXY.One,
            VectorXYInt.One);
    }

    private static HexVertexChromaticIndexTripletGrid CreateSingleSampleChromaticGrid(VectorXY point)
    {
        return CreateSingleSampleChromaticGrid(point, 2, 2);
    }

    private static HexVertexChromaticIndexTripletGrid CreateSingleSampleChromaticGrid(
        VectorXY point,
        int hexWidth,
        int hexHeight,
        HexVertexTripletGridFillMode fillMode = HexVertexTripletGridFillMode.HitHexesOnly)
    {
        return new HexVertexChromaticIndexTripletGrid(
            hexWidth,
            hexHeight,
            Layout.OddR,
            VectorXY.Zero,
            2f,
            point - new VectorXY(0.5f, 0.5f),
            VectorXY.One,
            VectorXYInt.One,
            fillMode);
    }

    private static void AssertTriplet<T>(Triplet<T> actual, Triplet<T> expected)
    {
        Assert.That(actual.Main, Is.EqualTo(expected.Main));
        Assert.That(actual.Left, Is.EqualTo(expected.Left));
        Assert.That(actual.Right, Is.EqualTo(expected.Right));
    }

    private static void AssertAllCellsHaveHex(bool[] hasHex)
    {
        for (int i = 0; i < hasHex.Length; i++)
            Assert.That(hasHex[i], Is.True, $"Expected cell {i} to be filled.");
    }

    private static void AssertBarycentric(Triplet<float> actual, float main, float left, float right)
    {
        Assert.That(actual.Main, Is.EqualTo(main).Within(0.000001f));
        Assert.That(actual.Left, Is.EqualTo(left).Within(0.000001f));
        Assert.That(actual.Right, Is.EqualTo(right).Within(0.000001f));
        Assert.That(actual.Main + actual.Left + actual.Right, Is.EqualTo(1f).Within(0.000001f));
    }
}
