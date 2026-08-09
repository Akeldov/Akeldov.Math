namespace Akeldov.Math.Hexes.Tests.Maps;

public class BoolHexMapTests
{
    [Test]
    public void Constructor_InitializesCellsToFalse()
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);

        var map = new BoolHexMap(topology);
        IHexMap<bool> mapView = map;

        Assert.Multiple(() =>
        {
            Assert.That(map.Topology, Is.EqualTo(topology));
            Assert.That(mapView, Is.SameAs(map));
            for (int index = 0; index < topology.Count; index++)
                Assert.That(map[index], Is.False);
        });
    }

    [Test]
    public void ValuesConstructor_RetainsBackingArray()
    {
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var values = new[] { true, false, false, true };
        var map = new BoolHexMap(topology, values);

        values[1] = true;
        map[2] = true;

        Assert.Multiple(() =>
        {
            Assert.That(map[1], Is.True);
            Assert.That(values[2], Is.True);
        });
    }
}
