namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapTests
{
    [Test]
    public void MinAndMax_ReturnCurrentValueRange()
    {
        var values = new[] { 3.5f, -2f, 7f };
        var map = new FloatHexMap(new HexMapTopology(3, 1, Layout.OddR), values);
        IFloatHexMap readOnlyMap = map;

        map[0] = -4f;
        values[2] = 9f;

        Assert.Multiple(() =>
        {
            Assert.That(readOnlyMap.Min, Is.EqualTo(-4f));
            Assert.That(readOnlyMap.Max, Is.EqualTo(9f));
        });
    }

    [Test]
    public void MinAndMax_WhenMapIsEmpty_Throw()
    {
        var map = new FloatHexMap(new HexMapTopology(0, 0, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = map.Min);
            Assert.Throws<InvalidOperationException>(() => _ = map.Max);
        });
    }
}
