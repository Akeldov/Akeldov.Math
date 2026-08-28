using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Tests.Structures;

public class PartialSextupletTests
{
    [Test]
    public void Default_HasNoPresentAdjacentValues()
    {
        var sextuplet = default(PartialSextuplet<int>);

        Assert.Multiple(() =>
        {
            Assert.That(sextuplet.Presence, Is.EqualTo(SextupletPresenceFlags.None));
            Assert.That(sextuplet.HasAdjacent0, Is.False);
            Assert.That(sextuplet.HasAdjacent1, Is.False);
            Assert.That(sextuplet.HasAdjacent2, Is.False);
            Assert.That(sextuplet.HasAdjacent3, Is.False);
            Assert.That(sextuplet.HasAdjacent4, Is.False);
            Assert.That(sextuplet.HasAdjacent5, Is.False);
        });
    }

    [Test]
    public void Constructor_WhenPresenceIsProvided_StoresValuesAndPresence()
    {
        SextupletPresenceFlags presence =
            SextupletPresenceFlags.Adjacent0 |
            SextupletPresenceFlags.Adjacent2 |
            SextupletPresenceFlags.Adjacent5;

        var sextuplet = new PartialSextuplet<int>(1, 2, 3, 4, 5, 6, presence);

        Assert.That(sextuplet.Adjacent0, Is.EqualTo(1));
        Assert.That(sextuplet.Adjacent1, Is.EqualTo(2));
        Assert.That(sextuplet.Adjacent2, Is.EqualTo(3));
        Assert.That(sextuplet.Adjacent3, Is.EqualTo(4));
        Assert.That(sextuplet.Adjacent4, Is.EqualTo(5));
        Assert.That(sextuplet.Adjacent5, Is.EqualTo(6));
        Assert.That(sextuplet.Presence, Is.EqualTo(presence));
        Assert.That(sextuplet.HasAdjacent0, Is.True);
        Assert.That(sextuplet.HasAdjacent1, Is.False);
        Assert.That(sextuplet.HasAdjacent2, Is.True);
        Assert.That(sextuplet.HasAdjacent3, Is.False);
        Assert.That(sextuplet.HasAdjacent4, Is.False);
        Assert.That(sextuplet.HasAdjacent5, Is.True);
    }

    [Test]
    public void Constructor_WhenBooleanPresenceIsProvided_CreatesPresenceFlags()
    {
        var sextuplet = new PartialSextuplet<int>(
            1,
            2,
            3,
            4,
            5,
            6,
            hasAdjacent0: false,
            hasAdjacent1: true,
            hasAdjacent2: false,
            hasAdjacent3: true,
            hasAdjacent4: false,
            hasAdjacent5: true);

        SextupletPresenceFlags expected =
            SextupletPresenceFlags.Adjacent1 |
            SextupletPresenceFlags.Adjacent3 |
            SextupletPresenceFlags.Adjacent5;

        Assert.That(sextuplet.Presence, Is.EqualTo(expected));
        Assert.That(sextuplet.HasAdjacent0, Is.False);
        Assert.That(sextuplet.HasAdjacent1, Is.True);
        Assert.That(sextuplet.HasAdjacent2, Is.False);
        Assert.That(sextuplet.HasAdjacent3, Is.True);
        Assert.That(sextuplet.HasAdjacent4, Is.False);
        Assert.That(sextuplet.HasAdjacent5, Is.True);
    }

    [Test]
    public void Constructor_WhenSextupletIsProvided_CopiesValuesAndPresence()
    {
        var source = new Sextuplet<int>(1, 2, 3, 4, 5, 6);

        var sextuplet = new PartialSextuplet<int>(source, SextupletPresenceFlags.All);

        Assert.That(sextuplet.Adjacent0, Is.EqualTo(1));
        Assert.That(sextuplet.Adjacent1, Is.EqualTo(2));
        Assert.That(sextuplet.Adjacent2, Is.EqualTo(3));
        Assert.That(sextuplet.Adjacent3, Is.EqualTo(4));
        Assert.That(sextuplet.Adjacent4, Is.EqualTo(5));
        Assert.That(sextuplet.Adjacent5, Is.EqualTo(6));
        Assert.That(sextuplet.Presence, Is.EqualTo(SextupletPresenceFlags.All));
    }

    [Test]
    public void ToSextuplet_ReturnsFullSextuplet()
    {
        var partialSextuplet = new PartialSextuplet<int>(
            1,
            2,
            3,
            4,
            5,
            6,
            SextupletPresenceFlags.Adjacent0);

        Sextuplet<int> sextuplet = partialSextuplet.ToSextuplet();

        Assert.That(sextuplet.Adjacent0, Is.EqualTo(1));
        Assert.That(sextuplet.Adjacent1, Is.EqualTo(2));
        Assert.That(sextuplet.Adjacent2, Is.EqualTo(3));
        Assert.That(sextuplet.Adjacent3, Is.EqualTo(4));
        Assert.That(sextuplet.Adjacent4, Is.EqualTo(5));
        Assert.That(sextuplet.Adjacent5, Is.EqualTo(6));
    }

    [Test]
    public void Deconstruct_ReturnsValuesAndPresence()
    {
        var sextuplet = new PartialSextuplet<int>(
            1,
            2,
            3,
            4,
            5,
            6,
            SextupletPresenceFlags.All);

        var (adjacent0, adjacent1, adjacent2, adjacent3, adjacent4, adjacent5) = sextuplet;
        sextuplet.Deconstruct(
            out int adjacent0WithPresence,
            out int adjacent1WithPresence,
            out int adjacent2WithPresence,
            out int adjacent3WithPresence,
            out int adjacent4WithPresence,
            out int adjacent5WithPresence,
            out SextupletPresenceFlags presence);

        Assert.That(adjacent0, Is.EqualTo(1));
        Assert.That(adjacent1, Is.EqualTo(2));
        Assert.That(adjacent2, Is.EqualTo(3));
        Assert.That(adjacent3, Is.EqualTo(4));
        Assert.That(adjacent4, Is.EqualTo(5));
        Assert.That(adjacent5, Is.EqualTo(6));
        Assert.That(adjacent0WithPresence, Is.EqualTo(1));
        Assert.That(adjacent1WithPresence, Is.EqualTo(2));
        Assert.That(adjacent2WithPresence, Is.EqualTo(3));
        Assert.That(adjacent3WithPresence, Is.EqualTo(4));
        Assert.That(adjacent4WithPresence, Is.EqualTo(5));
        Assert.That(adjacent5WithPresence, Is.EqualTo(6));
        Assert.That(presence, Is.EqualTo(SextupletPresenceFlags.All));
    }
}
