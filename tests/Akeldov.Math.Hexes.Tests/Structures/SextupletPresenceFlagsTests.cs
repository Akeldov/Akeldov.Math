using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Tests.Structures;

public class SextupletPresenceFlagsTests
{
    [Test]
    public void Members_UseIndependentAdjacentBits()
    {
        Assert.That((byte)SextupletPresenceFlags.None, Is.EqualTo(0));
        Assert.That((byte)SextupletPresenceFlags.Adjacent0, Is.EqualTo(1 << 0));
        Assert.That((byte)SextupletPresenceFlags.Adjacent1, Is.EqualTo(1 << 1));
        Assert.That((byte)SextupletPresenceFlags.Adjacent2, Is.EqualTo(1 << 2));
        Assert.That((byte)SextupletPresenceFlags.Adjacent3, Is.EqualTo(1 << 3));
        Assert.That((byte)SextupletPresenceFlags.Adjacent4, Is.EqualTo(1 << 4));
        Assert.That((byte)SextupletPresenceFlags.Adjacent5, Is.EqualTo(1 << 5));
    }

    [Test]
    public void All_CombinesEveryAdjacentPosition()
    {
        SextupletPresenceFlags expected =
            SextupletPresenceFlags.Adjacent0 |
            SextupletPresenceFlags.Adjacent1 |
            SextupletPresenceFlags.Adjacent2 |
            SextupletPresenceFlags.Adjacent3 |
            SextupletPresenceFlags.Adjacent4 |
            SextupletPresenceFlags.Adjacent5;

        Assert.That(SextupletPresenceFlags.All, Is.EqualTo(expected));
    }
}
