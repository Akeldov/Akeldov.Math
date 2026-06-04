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

        var grid = new HexVertexIndexTripletGrid(2, 1, Layout.OddR, origin, 2f, new VectorXYInt(4, 2));

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
    public void BarycentricGrid_UsesSameVertexTripletCenters()
    {
        const float hexApothem = 2f;
        float hexRadius = hexApothem.ConvertHexApothemToRadius();
        VectorXY point = GetPointNearOddRVertex0();
        var mainIndex = new VectorXYInt(0, 0);
        var leftIndex = new VectorXYInt(0, 1);
        var rightIndex = new VectorXYInt(1, 0);
        Triplet<float> expected = point.BarycentricCoordinates(
            mainIndex.GetHexCenter(hexApothem, hexRadius, VectorXY.Zero, Layout.OddR),
            leftIndex.GetHexCenter(hexApothem, hexRadius, VectorXY.Zero, Layout.OddR),
            rightIndex.GetHexCenter(hexApothem, hexRadius, VectorXY.Zero, Layout.OddR));
        var grid = CreateSingleSampleBarycentricGrid(point);

        Triplet<float> actual = grid[VectorXYInt.Zero];

        Assert.That(actual.Main, Is.EqualTo(expected.Main).Within(0.000001f));
        Assert.That(actual.Left, Is.EqualTo(expected.Left).Within(0.000001f));
        Assert.That(actual.Right, Is.EqualTo(expected.Right).Within(0.000001f));
        Assert.That(actual.Main + actual.Left + actual.Right, Is.EqualTo(1f).Within(0.000001f));
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
    public void Grids_WhenVertexNeighborsAreOutside_UseMainOnly()
    {
        VectorXY point = GetPointNearOddRVertex0();
        var expectedIndexTriplet = new Triplet<VectorXYInt>(
            VectorXYInt.Zero,
            VectorXYInt.Zero,
            VectorXYInt.Zero);

        var indexGrid = CreateSingleSampleIndexTripletGrid(point, 1, 1);
        var barycentricGrid = CreateSingleSampleBarycentricGrid(point, 1, 1);
        var chromaticGrid = CreateSingleSampleChromaticGrid(point, 1, 1);

        AssertTriplet(indexGrid[VectorXYInt.Zero], expectedIndexTriplet);
        AssertTriplet(chromaticGrid[VectorXYInt.Zero], expectedIndexTriplet.GetChromaticTriplet(Layout.OddR));
        AssertBarycentric(barycentricGrid[VectorXYInt.Zero], 1f, 0f, 0f);
    }

    [Test]
    public void Grids_WhenLeftVertexNeighborIsOutside_UseRemainingWeights()
    {
        VectorXY point = GetPointNearOddRVertex0();
        var expectedIndexTriplet = new Triplet<VectorXYInt>(
            VectorXYInt.Zero,
            VectorXYInt.Zero,
            new VectorXYInt(1, 0));

        var indexGrid = CreateSingleSampleIndexTripletGrid(point, 2, 1);
        var barycentricGrid = CreateSingleSampleBarycentricGrid(point, 2, 1);
        var chromaticGrid = CreateSingleSampleChromaticGrid(point, 2, 1);

        AssertTriplet(indexGrid[VectorXYInt.Zero], expectedIndexTriplet);
        AssertTriplet(chromaticGrid[VectorXYInt.Zero], expectedIndexTriplet.GetChromaticTriplet(Layout.OddR));
        AssertBarycentric(barycentricGrid[VectorXYInt.Zero], 0.6666667f, 0f, 0.3333333f);
    }

    [Test]
    public void Grids_WhenRightVertexNeighborIsOutside_UseRemainingWeights()
    {
        VectorXY point = GetPointNearOddRVertex0();
        var expectedIndexTriplet = new Triplet<VectorXYInt>(
            VectorXYInt.Zero,
            new VectorXYInt(0, 1),
            VectorXYInt.Zero);

        var indexGrid = CreateSingleSampleIndexTripletGrid(point, 1, 2);
        var barycentricGrid = CreateSingleSampleBarycentricGrid(point, 1, 2);
        var chromaticGrid = CreateSingleSampleChromaticGrid(point, 1, 2);

        AssertTriplet(indexGrid[VectorXYInt.Zero], expectedIndexTriplet);
        AssertTriplet(chromaticGrid[VectorXYInt.Zero], expectedIndexTriplet.GetChromaticTriplet(Layout.OddR));
        AssertBarycentric(barycentricGrid[VectorXYInt.Zero], 0.6666667f, 0.3333333f, 0f);
    }

    [Test]
    public void Grids_WithFillEmptyCells_WhenCellMissesFieldButTripletTouchesField_FillRemainingWeights()
    {
        VectorXY point = new VectorXY(2.1f, 0.6f);
        var expectedIndexTriplet = new Triplet<VectorXYInt>(
            VectorXYInt.Zero,
            VectorXYInt.Zero,
            VectorXYInt.Zero);

        var defaultGrid = CreateSingleSampleIndexTripletGrid(point, 1, 1);
        var indexGrid = CreateSingleSampleIndexTripletGrid(point, 1, 1, HexVertexTripletGridFillMode.FillEmptyCells);
        var barycentricGrid = CreateSingleSampleBarycentricGrid(point, 1, 1, HexVertexTripletGridFillMode.FillEmptyCells);
        var chromaticGrid = CreateSingleSampleChromaticGrid(point, 1, 1, HexVertexTripletGridFillMode.FillEmptyCells);

        Assert.That(defaultGrid.FillMode, Is.EqualTo(HexVertexTripletGridFillMode.HitHexesOnly));
        Assert.That(defaultGrid.TryGetIndexTriplet(VectorXYInt.Zero, out _), Is.False);
        Assert.That(indexGrid.FillMode, Is.EqualTo(HexVertexTripletGridFillMode.FillEmptyCells));
        Assert.That(barycentricGrid.FillMode, Is.EqualTo(HexVertexTripletGridFillMode.FillEmptyCells));
        Assert.That(chromaticGrid.FillMode, Is.EqualTo(HexVertexTripletGridFillMode.FillEmptyCells));
        Assert.That(barycentricGrid.HasHexAt(VectorXYInt.Zero), Is.True);
        Assert.That(chromaticGrid.HasHexAt(VectorXYInt.Zero), Is.True);
        AssertTriplet(indexGrid[VectorXYInt.Zero], expectedIndexTriplet);
        AssertTriplet(chromaticGrid[VectorXYInt.Zero], expectedIndexTriplet.GetChromaticTriplet(Layout.OddR));
        AssertBarycentric(barycentricGrid[VectorXYInt.Zero], 0f, 1f, 0f);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Grids_WithFillEmptyCells_FillEveryCellInDefaultBounds(Layout layout)
    {
        var indexGrid = new HexVertexIndexTripletGrid(
            5,
            4,
            layout,
            VectorXY.Zero,
            8f,
            new VectorXYInt(64, 64),
            HexVertexTripletGridFillMode.FillEmptyCells);
        var barycentricGrid = new HexVertexBarycentricGrid(
            5,
            4,
            layout,
            VectorXY.Zero,
            8f,
            new VectorXYInt(64, 64),
            HexVertexTripletGridFillMode.FillEmptyCells);
        var chromaticGrid = new HexVertexChromaticIndexTripletGrid(
            5,
            4,
            layout,
            VectorXY.Zero,
            8f,
            new VectorXYInt(64, 64),
            HexVertexTripletGridFillMode.FillEmptyCells);

        AssertAllCellsHaveHex(barycentricGrid.HasHex);
        AssertAllCellsHaveHex(chromaticGrid.HasHex);
    }

    [Test]
    public void Grids_WhenCellDoesNotHitHex_ReturnFalseAndThrowOnIndexer()
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

        Assert.That(grid.TryGetIndexTriplet(VectorXYInt.Zero, out Triplet<VectorXYInt> triplet), Is.False);
        Assert.That(triplet.Main, Is.EqualTo(new VectorXYInt(-1, -1)));
        Assert.Throws<InvalidOperationException>(() => _ = grid[VectorXYInt.Zero]);
    }

    [Test]
    public void Constructors_WhenArgumentsAreInvalid_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexIndexTripletGrid(0, 1, Layout.OddR, VectorXY.Zero, 1f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexBarycentricGrid(1, 0, Layout.OddR, VectorXY.Zero, 1f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 0f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexChromaticIndexTripletGrid(1, 1, Layout.OddR, VectorXY.Zero, 1f, new VectorXYInt(0, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexVertexIndexTripletGrid(
            1,
            1,
            Layout.OddR,
            VectorXY.Zero,
            1f,
            VectorXYInt.One,
            (HexVertexTripletGridFillMode)int.MaxValue));
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
        int hexHeight,
        HexVertexTripletGridFillMode fillMode = HexVertexTripletGridFillMode.HitHexesOnly)
    {
        return new HexVertexIndexTripletGrid(
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

    private static HexVertexBarycentricGrid CreateSingleSampleBarycentricGrid(VectorXY point)
    {
        return CreateSingleSampleBarycentricGrid(point, 2, 2);
    }

    private static HexVertexBarycentricGrid CreateSingleSampleBarycentricGrid(
        VectorXY point,
        int hexWidth,
        int hexHeight,
        HexVertexTripletGridFillMode fillMode = HexVertexTripletGridFillMode.HitHexesOnly)
    {
        return new HexVertexBarycentricGrid(
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
