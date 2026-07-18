using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class ChromaticBarycentricTripletGridTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Values_AreOrderedByChromaticIndex(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(3, 2, 1f, layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            new VectorXYInt(32, 24));
        var barycentricGrid = new BarycentricTripletGrid(hexMapGeometry, rasterGeometry);
        var chromaticIndexGrid = new ChromaticIndexTripletGrid(hexMapGeometry, rasterGeometry);
        var grid = new ChromaticBarycentricTripletGrid(hexMapGeometry, rasterGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(grid, Is.AssignableTo<ISpatialRaster<ChromaticTriplet<float>>>());
            Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
            Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
            Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
        });

        for (int index = 0; index < rasterGeometry.Resolution.X * rasterGeometry.Resolution.Y; index++)
        {
            Triplet<float> barycentric = barycentricGrid[index];
            Triplet<byte> chromaticIndices = chromaticIndexGrid[index];
            ChromaticTriplet<float> actual = grid[index];

            Assert.Multiple(() =>
            {
                Assert.That(actual.Index0, Is.EqualTo(GetCoordinate(0, barycentric, chromaticIndices)), $"{layout}, cell {index}, Index0");
                Assert.That(actual.Index1, Is.EqualTo(GetCoordinate(1, barycentric, chromaticIndices)), $"{layout}, cell {index}, Index1");
                Assert.That(actual.Index2, Is.EqualTo(GetCoordinate(2, barycentric, chromaticIndices)), $"{layout}, cell {index}, Index2");
            });
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void PartialValues_ReorderCoordinatesAndPresenceByChromaticIndex(Layout layout)
    {
        var hexMapGeometry = new HexMapGeometry(2, 2, 1f, layout);
        var rasterGeometry = new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            new VectorXYInt(32, 32));
        var barycentricGrid = new BarycentricPartialTripletGrid(hexMapGeometry, rasterGeometry);
        var chromaticIndexGrid = new ChromaticIndexTripletGrid(hexMapGeometry, rasterGeometry);
        var grid = new ChromaticBarycentricPartialTripletGrid(hexMapGeometry, rasterGeometry);
        bool foundPartialValue = false;

        Assert.Multiple(() =>
        {
            Assert.That(grid, Is.AssignableTo<ISpatialRaster<PartialChromaticTriplet<float>>>());
            Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
            Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
            Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
        });

        for (int index = 0; index < rasterGeometry.Resolution.X * rasterGeometry.Resolution.Y; index++)
        {
            PartialTriplet<float> barycentric = barycentricGrid[index];
            Triplet<byte> chromaticIndices = chromaticIndexGrid[index];
            PartialChromaticTriplet<float> actual = grid[index];
            (float Coordinate, bool IsPresent) expected0 = GetCoordinate(0, barycentric, chromaticIndices);
            (float Coordinate, bool IsPresent) expected1 = GetCoordinate(1, barycentric, chromaticIndices);
            (float Coordinate, bool IsPresent) expected2 = GetCoordinate(2, barycentric, chromaticIndices);

            foundPartialValue |= actual.Presence != ChromaticTripletPresenceFlags.All;

            Assert.Multiple(() =>
            {
                Assert.That(actual.Index0, Is.EqualTo(expected0.Coordinate), $"{layout}, cell {index}, Index0");
                Assert.That(actual.Index1, Is.EqualTo(expected1.Coordinate), $"{layout}, cell {index}, Index1");
                Assert.That(actual.Index2, Is.EqualTo(expected2.Coordinate), $"{layout}, cell {index}, Index2");
                Assert.That(actual.HasIndex0, Is.EqualTo(expected0.IsPresent), $"{layout}, cell {index}, HasIndex0");
                Assert.That(actual.HasIndex1, Is.EqualTo(expected1.IsPresent), $"{layout}, cell {index}, HasIndex1");
                Assert.That(actual.HasIndex2, Is.EqualTo(expected2.IsPresent), $"{layout}, cell {index}, HasIndex2");
            });
        }

        Assert.That(foundPartialValue, Is.True, $"{layout} did not exercise a partial triplet.");
    }

    [Test]
    public void TryGetValue_WhenIndexIsOutside_ReturnsFalse()
    {
        var hexMapGeometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var rasterGeometry = new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, VectorXYInt.One);
        var grid = new ChromaticBarycentricTripletGrid(hexMapGeometry, rasterGeometry);
        var partialGrid = new ChromaticBarycentricPartialTripletGrid(hexMapGeometry, rasterGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(grid.TryGetValue(new VectorXYInt(-1, 0), out _), Is.False);
            Assert.That(partialGrid.TryGetValue(new VectorXYInt(1, 0), out _), Is.False);
        });
    }

    [Test]
    public void Constructors_WhenArgumentsAreInvalid_Throw()
    {
        var hexMapGeometry = new HexMapGeometry(1, 1, 1f, Layout.OddR);
        var emptyTopology = new HexMapTopology(0, 0, Layout.OddR);
        var emptyHexMapGeometry = new HexMapGeometry(emptyTopology, 1f);
        var rasterGeometry = new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, VectorXYInt.One);

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticBarycentricTripletGrid(hexMapGeometry, default));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticBarycentricPartialTripletGrid(hexMapGeometry, default));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticBarycentricTripletGrid(emptyHexMapGeometry, rasterGeometry));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticBarycentricPartialTripletGrid(emptyHexMapGeometry, rasterGeometry));
        });
    }

    [Test]
    public void ChromaticTripletTypes_ExposeChromaticOrderAndPresence()
    {
        var triplet = new ChromaticTriplet<int>(10, 20, 30);
        var partial = new PartialChromaticTriplet<int>(
            triplet,
            ChromaticTripletPresenceFlags.Index0 | ChromaticTripletPresenceFlags.Index2);

        (int index0, int index1, int index2) = triplet;

        Assert.Multiple(() =>
        {
            Assert.That((index0, index1, index2), Is.EqualTo((10, 20, 30)));
            Assert.That(partial.ToTriplet(), Is.EqualTo(triplet));
            Assert.That(partial.HasIndex0, Is.True);
            Assert.That(partial.HasIndex1, Is.False);
            Assert.That(partial.HasIndex2, Is.True);
        });
    }

    private static float GetCoordinate(
        byte chromaticIndex,
        Triplet<float> barycentric,
        Triplet<byte> chromaticIndices)
    {
        if (chromaticIndices.Main == chromaticIndex)
            return barycentric.Main;

        if (chromaticIndices.Left == chromaticIndex)
            return barycentric.Left;

        if (chromaticIndices.Right == chromaticIndex)
            return barycentric.Right;

        throw new InvalidOperationException();
    }

    private static (float Coordinate, bool IsPresent) GetCoordinate(
        byte chromaticIndex,
        PartialTriplet<float> barycentric,
        Triplet<byte> chromaticIndices)
    {
        if (chromaticIndices.Main == chromaticIndex)
            return (barycentric.Main, barycentric.HasMain);

        if (chromaticIndices.Left == chromaticIndex)
            return (barycentric.Left, barycentric.HasLeft);

        if (chromaticIndices.Right == chromaticIndex)
            return (barycentric.Right, barycentric.HasRight);

        throw new InvalidOperationException();
    }
}
