using Akeldov.Math.Spatial2D;
using Akeldov.Math.Hexes.Pathfinding;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexTransferCostMapTests
{
    [Test]
    public void Constructor_RetainsCostMapsAndExposesTheirTopology()
    {
        var topology = new HexMapTopology(3, 2, Layout.OddR);
        var exitCosts = new HexMap<float>(topology);
        var entryCosts = new HexMap<float>(topology);

        var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);

        Assert.Multiple(() =>
        {
            Assert.That(transferCosts.ExitCosts, Is.SameAs(exitCosts));
            Assert.That(transferCosts.EntryCosts, Is.SameAs(entryCosts));
            Assert.That(transferCosts.Topology, Is.EqualTo(topology));
        });
    }

    [Test]
    public void Constructor_WhenCostMapIsNull_Throws()
    {
        var map = new HexMap<float>(new HexMapTopology(1, 1, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => new HexTransferCostMap(null!, map))!.ParamName,
                Is.EqualTo("exitCosts"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => new HexTransferCostMap(map, null!))!.ParamName,
                Is.EqualTo("entryCosts"));
        });
    }

    [Test]
    public void Constructor_WhenCostMapTopologiesDiffer_Throws()
    {
        var exitCosts = new HexMap<float>(new HexMapTopology(2, 2, Layout.OddR));
        var differentResolution = new HexMap<float>(new HexMapTopology(3, 2, Layout.OddR));
        var differentLayout = new HexMap<float>(new HexMapTopology(2, 2, Layout.EvenR));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentException>(() => new HexTransferCostMap(exitCosts, differentResolution))!.ParamName,
                Is.EqualTo("entryCosts"));
            Assert.That(
                Assert.Throws<ArgumentException>(() => new HexTransferCostMap(exitCosts, differentLayout))!.ParamName,
                Is.EqualTo("entryCosts"));
        });
    }

    [Test]
    public void GetTransferCost_ReturnsSourceExitCostPlusDestinationEntryCost()
    {
        var topology = new HexMapTopology(4, 3, Layout.EvenQ);
        var exitCosts = new HexMap<float>(topology);
        var entryCosts = new HexMap<float>(topology);
        var from = new VectorXYInt(0, 0);
        var to = new VectorXYInt(3, 2);
        exitCosts[from] = 2.5f;
        entryCosts[to] = 4.25f;
        exitCosts[to] = 100f;
        entryCosts[from] = 200f;
        var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);

        float cost = transferCosts.GetTransferCost(from, to);

        Assert.That(cost, Is.EqualTo(6.75f));
    }

    [Test]
    public void GetTransferCost_WhenSourceAndDestinationAreTheSame_StillCombinesBothCosts()
    {
        var topology = new HexMapTopology(1, 1, Layout.OddQ);
        var exitCosts = new HexMap<float>(topology, new[] { 1.5f });
        var entryCosts = new HexMap<float>(topology, new[] { 2.25f });
        var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);

        float cost = transferCosts.GetTransferCost(VectorXYInt.Zero, VectorXYInt.Zero);

        Assert.That(cost, Is.EqualTo(3.75f));
    }

    [Test]
    public void GetTransferCost_WhenIndexIsOutsideTopology_Throws()
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var transferCosts = new HexTransferCostMap(
            new HexMap<float>(topology),
            new HexMap<float>(topology));

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    transferCosts.GetTransferCost(new VectorXYInt(-1, 0), VectorXYInt.Zero))!.ParamName,
                Is.EqualTo("from"));
            Assert.That(
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    transferCosts.GetTransferCost(VectorXYInt.Zero, new VectorXYInt(2, 0)))!.ParamName,
                Is.EqualTo("to"));
        });
    }
}
