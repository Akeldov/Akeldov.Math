using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexAdjacencyOffsetTests
{
    [TestCase(Layout.OddR, 3, 2, 2, 3, 1, 3, 1, 2, 1, 1, 2, 1)]
    [TestCase(Layout.EvenR, 3, 2, 3, 3, 2, 3, 1, 2, 2, 1, 3, 1)]
    [TestCase(Layout.OddQ, 3, 2, 2, 3, 1, 2, 1, 1, 2, 1, 3, 1)]
    [TestCase(Layout.EvenQ, 3, 3, 2, 3, 1, 3, 1, 2, 2, 1, 3, 2)]
    public void GetAdjacents_ReturnsLayoutOrderedNeighbors(
        Layout layout,
        int x0,
        int y0,
        int x1,
        int y1,
        int x2,
        int y2,
        int x3,
        int y3,
        int x4,
        int y4,
        int x5,
        int y5)
    {
        VectorXYInt[] actual = new VectorXYInt(2, 2).GetAdjacents(layout);

        Assert.That(actual, Is.EqualTo(new[]
        {
            new VectorXYInt(x0, y0),
            new VectorXYInt(x1, y1),
            new VectorXYInt(x2, y2),
            new VectorXYInt(x3, y3),
            new VectorXYInt(x4, y4),
            new VectorXYInt(x5, y5)
        }));
    }

    [Test]
    public void GetAdjacents_ReturnsCallerOwnedArray()
    {
        var index = new VectorXYInt(2, 2);
        VectorXYInt[] adjacents = index.GetAdjacents(Layout.OddR);
        adjacents[0] = new VectorXYInt(42, 42);

        VectorXYInt[] freshAdjacents = index.GetAdjacents(Layout.OddR);

        Assert.That(freshAdjacents[0], Is.EqualTo(new VectorXYInt(3, 2)));
    }

    [Test]
    public void GetAdjacents_WhenLayoutIsUnsupported_Throws()
    {
        Assert.That(
            () => VectorXYInt.Zero.GetAdjacents((Layout)42),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void GetRelativeOffsets_ReturnsCallerOwnedCopy()
    {
        VectorXYInt[] offsets = true.GetRelativeOffsets(Layout.OddR);
        offsets[0] = new VectorXYInt(42, 42);

        VectorXYInt[] freshOffsets = true.GetRelativeOffsets(Layout.OddR);

        Assert.That(freshOffsets[0], Is.EqualTo(new VectorXYInt(1, 0)));
    }
}
