using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Pathfinding;

public class HexTransferCostMapExtensionsTests
{
    [Test]
    public void FindShortestPath_BetweenNonAdjacentHexes_ReturnsPathAndTotalCost()
    {
        var topology = new HexMapTopology(3, 1, Layout.OddR);
        var exitCosts = new HexMap<float>(topology, new[] { 1f, 2f, 100f });
        var entryCosts = new HexMap<float>(topology, new[] { 100f, 3f, 4f });
        var costs = new HexTransferCostMap(exitCosts, entryCosts);

        HexPath? result = costs.FindShortestPath(new VectorXYInt(0, 0), new VectorXYInt(2, 0));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.HexIndexes, Is.EqualTo(new[]
            {
                new VectorXYInt(0, 0),
                new VectorXYInt(1, 0),
                new VectorXYInt(2, 0)
            }));
            Assert.That(result.TotalCost, Is.EqualTo(10f));
        });
    }

    [Test]
    public void FindShortestPath_ChoosesLowerCostPathInsteadOfFewerSteps()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var exitCosts = new HexMap<float>(topology, new[]
        {
            1f, 100f, 1f,
            1f, 1f, 1f
        });
        var entryCosts = new HexMap<float>(topology, new[]
        {
            1f, 100f, 1f,
            1f, 1f, 1f
        });
        var costs = new HexTransferCostMap(exitCosts, entryCosts);

        HexPath? result = costs.FindShortestPath(new VectorXYInt(0, 0), new VectorXYInt(2, 0));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.HexIndexes, Is.EqualTo(new[]
            {
                new VectorXYInt(0, 0),
                new VectorXYInt(0, 1),
                new VectorXYInt(1, 1),
                new VectorXYInt(2, 0)
            }));
            Assert.That(result.TotalCost, Is.EqualTo(6f));
        });
    }

    [Test]
    public void FindShortestPath_WhenSourceEqualsDestination_ReturnsZeroCostSingleHexPath()
    {
        var topology = new HexMapTopology(1, 1, Layout.EvenQ);
        var costs = new HexTransferCostMap(
            new HexMap<float>(topology, new[] { 2f }),
            new HexMap<float>(topology, new[] { 3f }));

        HexPath? result = costs.FindShortestPath(VectorXYInt.Zero, VectorXYInt.Zero);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.HexIndexes, Is.EqualTo(new[] { VectorXYInt.Zero }));
            Assert.That(result.TotalCost, Is.Zero);
        });
    }

    [Test]
    public void FindShortestPath_WhenRouteIsImpassable_ReturnsNull()
    {
        var topology = new HexMapTopology(1, 3, Layout.OddR);
        var costs = new HexTransferCostMap(
            new HexMap<float>(topology, new[] { 1f, 1f, 1f }),
            new HexMap<float>(topology, new[] { 1f, float.PositiveInfinity, 1f }));

        HexPath? result = costs.FindShortestPath(new VectorXYInt(0, 0), new VectorXYInt(0, 2));

        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindShortestPath_WhenArgumentsAreInvalid_Throws()
    {
        var topology = new HexMapTopology(2, 2, Layout.OddQ);
        var costs = new HexTransferCostMap(
            new HexMap<float>(topology),
            new HexMap<float>(topology));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() =>
                    HexTransferCostMapExtensions.FindShortestPath(null!, VectorXYInt.Zero, VectorXYInt.Zero))!.ParamName,
                Is.EqualTo("costs"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    costs.FindShortestPath(new VectorXYInt(-1, 0), VectorXYInt.Zero))!.ParamName,
                Is.EqualTo("from"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    costs.FindShortestPath(VectorXYInt.Zero, new VectorXYInt(2, 0)))!.ParamName,
                Is.EqualTo("to"));
        });
    }

    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.NegativeInfinity)]
    public void FindShortestPath_WhenCostIsUnsupported_Throws(float invalidCost)
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var exitCosts = new HexMap<float>(topology, new[] { 1f, invalidCost });
        var entryCosts = new HexMap<float>(topology, new[] { 1f, 1f });
        var costs = new HexTransferCostMap(exitCosts, entryCosts);

        Assert.Throws<InvalidOperationException>(() =>
            costs.FindShortestPath(VectorXYInt.Zero, new VectorXYInt(1, 0)));
    }

    [Test]
    public void HexPath_HexIndexesCannotBeMutatedThroughPublicContract()
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var costs = new HexTransferCostMap(
            new HexMap<float>(topology),
            new HexMap<float>(topology));
        HexPath result = costs.FindShortestPath(VectorXYInt.Zero, new VectorXYInt(1, 0))!;

        Assert.That(result.HexIndexes, Is.Not.InstanceOf<VectorXYInt[]>());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<VectorXYInt>)result.HexIndexes)[0] = new VectorXYInt(1, 0));
    }
}
