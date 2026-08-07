namespace Akeldov.Math.Hexes.Tests.Maps;

public class IntHexMapTests
{
    [Test]
    public void MinAndMax_ReturnCurrentValueRange()
    {
        var values = new[] { 3, -2, 7 };
        var map = new IntHexMap(new HexMapTopology(3, 1, Layout.OddR), values);
        IIntHexMap readOnlyMap = map;

        map[0] = -4;
        values[2] = 9;

        Assert.Multiple(() =>
        {
            Assert.That(readOnlyMap.Min, Is.EqualTo(-4));
            Assert.That(readOnlyMap.Max, Is.EqualTo(9));
        });
    }

    [Test]
    public void MinAndMax_WhenMapIsEmpty_Throw()
    {
        var map = new IntHexMap(new HexMapTopology(0, 0, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = map.Min);
            Assert.Throws<InvalidOperationException>(() => _ = map.Max);
        });
    }
}
