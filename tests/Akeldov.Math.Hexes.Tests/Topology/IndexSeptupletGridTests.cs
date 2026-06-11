using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class IndexSeptupletGridTests
{
    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_WithRectanglePart_SamplesCorrespondingPartOfFullGrid(Layout layout)
    {
        var map = new IndexSeptupletMap(
            width: 12,
            height: 8,
            layout: layout);
        var fullGrid = new IndexSeptupletGrid(
            map,
            new VectorXYInt(4, 4));
        var partGrid = new IndexSeptupletGrid(
            map,
            new VectorXYInt(2, 2),
            new NormalizedRectanglePart(
                new PointXY(0.25f, 0.25f),
                new PointXY(0.75f, 0.75f)));

        AssertSeptuplet(partGrid[new VectorXYInt(0, 0)], fullGrid[new VectorXYInt(1, 1)]);
        AssertSeptuplet(partGrid[new VectorXYInt(1, 0)], fullGrid[new VectorXYInt(2, 1)]);
        AssertSeptuplet(partGrid[new VectorXYInt(0, 1)], fullGrid[new VectorXYInt(1, 2)]);
        AssertSeptuplet(partGrid[new VectorXYInt(1, 1)], fullGrid[new VectorXYInt(2, 2)]);
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void Constructor_WithFullRectanglePart_MatchesDefaultConstructor(Layout layout)
    {
        var map = new IndexSeptupletMap(
            width: 12,
            height: 8,
            layout: layout);
        var defaultGrid = new IndexSeptupletGrid(
            map,
            new VectorXYInt(4, 4));
        var fullPartGrid = new IndexSeptupletGrid(
            map,
            new VectorXYInt(4, 4),
            NormalizedRectanglePart.Full);

        for (int y = 0; y < defaultGrid.Height; y++)
        {
            for (int x = 0; x < defaultGrid.Width; x++)
            {
                var index = new VectorXYInt(x, y);
                AssertSeptuplet(fullPartGrid[index], defaultGrid[index]);
            }
        }
    }

    private static void AssertSeptuplet(
        Septuplet<VectorXYInt> actual,
        Septuplet<VectorXYInt> expected)
    {
        Assert.That(actual.Main, Is.EqualTo(expected.Main));
        Assert.That(actual.Adjacent0, Is.EqualTo(expected.Adjacent0));
        Assert.That(actual.Adjacent1, Is.EqualTo(expected.Adjacent1));
        Assert.That(actual.Adjacent2, Is.EqualTo(expected.Adjacent2));
        Assert.That(actual.Adjacent3, Is.EqualTo(expected.Adjacent3));
        Assert.That(actual.Adjacent4, Is.EqualTo(expected.Adjacent4));
        Assert.That(actual.Adjacent5, Is.EqualTo(expected.Adjacent5));
    }
}
