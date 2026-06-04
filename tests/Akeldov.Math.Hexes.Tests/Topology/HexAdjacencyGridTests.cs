using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using HexAdjacencyGrid = Akeldov.Math.Hexes.Geometry.HexAdjacencyGrid;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexAdjacencyGridTests
{
    [Test]
    public void Constructor_ExposesGeometryResolutionAndSampledValues()
    {
        var map = new HexAdjacencyMap(2, 1, Layout.OddR);
        var origin = new VectorXY(10f, -20f);

        var grid = new HexAdjacencyGrid(map, origin, 2f, new VectorXYInt(4, 2));

        Assert.That(grid.HexResolution, Is.EqualTo(new VectorXYInt(2, 1)));
        Assert.That(grid.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(grid.HexOrigin, Is.EqualTo(origin));
        Assert.That(grid.HexApothem, Is.EqualTo(2f));
        Assert.That(grid.HexRadius, Is.EqualTo(2f.ConvertHexApothemToRadius()));
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(4, 2)));
        Assert.That(grid.ResolutionX, Is.EqualTo(4));
        Assert.That(grid.ResolutionY, Is.EqualTo(2));
        Assert.That(grid.Count, Is.EqualTo(8));
        Assert.That(grid.Adjacent, Has.Length.EqualTo(8));
        Assert.That(grid.HexIndices, Has.Length.EqualTo(8));
        Assert.That(grid.HasHex, Has.Length.EqualTo(8));
    }

    [Test]
    public void Constructor_WithoutAdjacencyMap_ExposesGeometryResolutionAndSampledValues()
    {
        var origin = new VectorXY(10f, -20f);

        var grid = new HexAdjacencyGrid(2, 1, Layout.OddR, origin, 2f, new VectorXYInt(4, 2));

        Assert.That(grid.HexResolution, Is.EqualTo(new VectorXYInt(2, 1)));
        Assert.That(grid.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(grid.HexOrigin, Is.EqualTo(origin));
        Assert.That(grid.HexApothem, Is.EqualTo(2f));
        Assert.That(grid.HexRadius, Is.EqualTo(2f.ConvertHexApothemToRadius()));
        Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(4, 2)));
        Assert.That(grid.ResolutionX, Is.EqualTo(4));
        Assert.That(grid.ResolutionY, Is.EqualTo(2));
        Assert.That(grid.Count, Is.EqualTo(8));
        Assert.That(grid.Adjacent, Has.Length.EqualTo(8));
        Assert.That(grid.HexIndices, Has.Length.EqualTo(8));
        Assert.That(grid.HasHex, Has.Length.EqualTo(8));
    }

    [Test]
    public void Indexer_WhenGridIsDenserThanMap_ReusesHexAdjacency()
    {
        var map = new HexAdjacencyMap(2, 1, Layout.OddR);

        var grid = new HexAdjacencyGrid(map, VectorXY.Zero, 2f, new VectorXYInt(4, 1));

        Assert.That(grid.GetHexIndex(new VectorXYInt(0, 0)), Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(grid.GetHexIndex(new VectorXYInt(1, 0)), Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(grid.GetHexIndex(new VectorXYInt(2, 0)), Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(grid.GetHexIndex(new VectorXYInt(3, 0)), Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(grid.GetHexFlatIndex(new VectorXYInt(0, 0)), Is.EqualTo(0));
        Assert.That(grid.GetHexFlatIndex(new VectorXYInt(1, 0)), Is.EqualTo(0));
        Assert.That(grid.GetHexFlatIndex(new VectorXYInt(2, 0)), Is.EqualTo(1));
        Assert.That(grid.GetHexFlatIndex(new VectorXYInt(3, 0)), Is.EqualTo(1));
        AssertIndexedAdjacency(grid[new VectorXYInt(0, 0)], 0, map[new VectorXYInt(0, 0)]);
        AssertIndexedAdjacency(grid[new VectorXYInt(1, 0)], 0, map[new VectorXYInt(0, 0)]);
        AssertIndexedAdjacency(grid[new VectorXYInt(2, 0)], 1, map[new VectorXYInt(1, 0)]);
        AssertIndexedAdjacency(grid[new VectorXYInt(3, 0)], 1, map[new VectorXYInt(1, 0)]);
    }

    [Test]
    public void Indexer_WithoutAdjacencyMap_ReusesIndexedHexAdjacency()
    {
        var map = new IndexedHexAdjacencyMap(2, 1, Layout.OddR);

        var grid = new HexAdjacencyGrid(2, 1, Layout.OddR, VectorXY.Zero, 2f, new VectorXYInt(4, 1));

        Assert.That(grid.GetHexIndex(new VectorXYInt(0, 0)), Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(grid.GetHexIndex(new VectorXYInt(1, 0)), Is.EqualTo(new VectorXYInt(0, 0)));
        Assert.That(grid.GetHexIndex(new VectorXYInt(2, 0)), Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(grid.GetHexIndex(new VectorXYInt(3, 0)), Is.EqualTo(new VectorXYInt(1, 0)));
        Assert.That(grid[new VectorXYInt(0, 0)], Is.EqualTo(map[new VectorXYInt(0, 0)]));
        Assert.That(grid[new VectorXYInt(1, 0)], Is.EqualTo(map[new VectorXYInt(0, 0)]));
        Assert.That(grid[new VectorXYInt(2, 0)], Is.EqualTo(map[new VectorXYInt(1, 0)]));
        Assert.That(grid[new VectorXYInt(3, 0)], Is.EqualTo(map[new VectorXYInt(1, 0)]));
    }

    [Test]
    public void Constructor_UsesPointToXYIndexForStaggeredHexHit()
    {
        var map = new HexAdjacencyMap(2, 2, Layout.OddR);
        var hexOrigin = new VectorXY(10f, -20f);
        const float hexApothem = 2f;
        float hexRadius = hexApothem.ConvertHexApothemToRadius();
        var shiftedRowCenter = new VectorXY(
            hexOrigin.X + hexApothem,
            hexOrigin.Y + 1.5f * hexRadius);

        var grid = new HexAdjacencyGrid(
            map,
            hexOrigin,
            hexApothem,
            shiftedRowCenter - new VectorXY(0.5f, 0.5f),
            VectorXY.One,
            VectorXYInt.One);

        Assert.That(grid.GetCellCenter(VectorXYInt.Zero), Is.EqualTo(shiftedRowCenter));
        Assert.That(grid.GetHexIndex(VectorXYInt.Zero), Is.EqualTo(new VectorXYInt(0, 1)));
        Assert.That(grid.GetHexFlatIndex(VectorXYInt.Zero), Is.EqualTo(2));
        AssertIndexedAdjacency(grid[VectorXYInt.Zero], 2, map[new VectorXYInt(0, 1)]);
    }

    [Test]
    public void TryGetHexIndex_WhenGridCellDoesNotHitMap_ReturnsFalse()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);
        var grid = new HexAdjacencyGrid(
            map,
            VectorXY.Zero,
            2f,
            new VectorXY(100f, 100f),
            VectorXY.One,
            VectorXYInt.One);

        bool hasHex = grid.TryGetHexIndex(VectorXYInt.Zero, out VectorXYInt hexIndex);

        Assert.That(hasHex, Is.False);
        Assert.That(hexIndex, Is.EqualTo(new VectorXYInt(-1, -1)));
        Assert.That(grid.TryGetHexFlatIndex(VectorXYInt.Zero, out int hexFlatIndex), Is.False);
        Assert.That(hexFlatIndex, Is.EqualTo(-1));
        Assert.That(grid.HasHexAt(VectorXYInt.Zero), Is.False);
        Assert.That(grid.Adjacent[0].Index, Is.EqualTo(0));
        Assert.That(grid.Adjacent[0].Flags, Is.EqualTo(IndexedHexAdjacencyFlags.None));
        Assert.That(grid.Adjacent[0].HasOwnIndex, Is.False);
        Assert.Throws<InvalidOperationException>(() => _ = grid.GetHexIndex(VectorXYInt.Zero));
        Assert.Throws<InvalidOperationException>(() => _ = grid.GetHexFlatIndex(VectorXYInt.Zero));
        Assert.Throws<InvalidOperationException>(() => _ = grid[VectorXYInt.Zero]);
    }

    [Test]
    public void Indexer_UsesGridResolutionForFlatIndex()
    {
        var map = new HexAdjacencyMap(2, 2, Layout.OddR);
        var grid = new HexAdjacencyGrid(map, VectorXY.Zero, 2f, new VectorXYInt(4, 4));

        if (grid.HasHexAt(new VectorXYInt(3, 2)))
            Assert.That(grid[11], Is.EqualTo(grid[new VectorXYInt(3, 2)]));
        else
            Assert.Throws<InvalidOperationException>(() => _ = grid[11]);
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideGrid_Throws()
    {
        var map = new HexAdjacencyMap(2, 2, Layout.OddR);
        var grid = new HexAdjacencyGrid(map, VectorXY.Zero, 2f, new VectorXYInt(4, 4));

        Assert.Throws<IndexOutOfRangeException>(() => _ = grid[new VectorXYInt(4, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = grid[new VectorXYInt(0, 4)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = grid.GetHexIndex(new VectorXYInt(-1, 0)));
    }

    [Test]
    public void Constructor_WhenHexAdjacencyMapIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HexAdjacencyGrid(null!, VectorXY.Zero, 1f, VectorXYInt.One));
    }

    [Test]
    public void Constructor_WhenHexAdjacencyMapIsEmpty_Throws()
    {
        var map = new HexAdjacencyMap(0, 1, Layout.OddR);

        Assert.Throws<ArgumentException>(() => new HexAdjacencyGrid(map, VectorXY.Zero, 1f, VectorXYInt.One));
    }

    [Test]
    public void Constructor_WhenHexDimensionIsNotPositive_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(0, 1, Layout.OddR, VectorXY.Zero, 1f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(1, 0, Layout.OddR, VectorXY.Zero, 1f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(-1, 1, Layout.OddR, VectorXY.Zero, 1f, VectorXYInt.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(1, -1, Layout.OddR, VectorXY.Zero, 1f, VectorXYInt.One));
    }

    [Test]
    public void Constructor_WhenHexApothemIsNotPositive_Throws()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);

        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(map, VectorXY.Zero, 0f, VectorXYInt.One));
    }

    [Test]
    public void Constructor_WhenGridSizeIsNotPositive_Throws()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);

        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(
            map,
            VectorXY.Zero,
            1f,
            VectorXY.Zero,
            new VectorXY(0f, 1f),
            VectorXYInt.One));
    }

    [Test]
    public void Constructor_WhenResolutionIsNotPositive_Throws()
    {
        var map = new HexAdjacencyMap(1, 1, Layout.OddR);

        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(map, VectorXY.Zero, 1f, new VectorXYInt(0, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new HexAdjacencyGrid(map, VectorXY.Zero, 1f, new VectorXYInt(1, 0)));
    }

    private static void AssertIndexedAdjacency(IndexedHexAdjacency actual, int index, HexAdjacency expected)
    {
        Assert.That(actual.Index, Is.EqualTo(index));
        Assert.That(actual.Flags, Is.EqualTo((IndexedHexAdjacencyFlags)expected.Flags | IndexedHexAdjacencyFlags.OwnIndex));
        Assert.That(actual.HasOwnIndex, Is.True);
        Assert.That(actual.Adjacent0Index, Is.EqualTo(expected.Adjacent0Index));
        Assert.That(actual.Adjacent1Index, Is.EqualTo(expected.Adjacent1Index));
        Assert.That(actual.Adjacent2Index, Is.EqualTo(expected.Adjacent2Index));
        Assert.That(actual.Adjacent3Index, Is.EqualTo(expected.Adjacent3Index));
        Assert.That(actual.Adjacent4Index, Is.EqualTo(expected.Adjacent4Index));
        Assert.That(actual.Adjacent5Index, Is.EqualTo(expected.Adjacent5Index));
    }
}
