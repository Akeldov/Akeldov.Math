using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Tests.Structures;

public class SextupletTests
{
    [Test]
    public void Constructor_StoresAdjacentValues()
    {
        var sextuplet = new Sextuplet<int>(1, 2, 3, 4, 5, 6);

        Assert.That(sextuplet.Adjacent0, Is.EqualTo(1));
        Assert.That(sextuplet.Adjacent1, Is.EqualTo(2));
        Assert.That(sextuplet.Adjacent2, Is.EqualTo(3));
        Assert.That(sextuplet.Adjacent3, Is.EqualTo(4));
        Assert.That(sextuplet.Adjacent4, Is.EqualTo(5));
        Assert.That(sextuplet.Adjacent5, Is.EqualTo(6));
    }

    [Test]
    public void Deconstruct_ReturnsAdjacentValues()
    {
        var sextuplet = new Sextuplet<int>(1, 2, 3, 4, 5, 6);

        var (adjacent0, adjacent1, adjacent2, adjacent3, adjacent4, adjacent5) = sextuplet;

        Assert.That(adjacent0, Is.EqualTo(1));
        Assert.That(adjacent1, Is.EqualTo(2));
        Assert.That(adjacent2, Is.EqualTo(3));
        Assert.That(adjacent3, Is.EqualTo(4));
        Assert.That(adjacent4, Is.EqualTo(5));
        Assert.That(adjacent5, Is.EqualTo(6));
    }
}
