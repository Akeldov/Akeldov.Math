using Akeldov.Math.Hexes.Topology;

namespace Akeldov.Math.Hexes.Tests.Structures;

public class PartialPairTests
{
    [Test]
    public void Constructor_WhenPresenceIsProvided_StoresValuesAndPresence()
    {
        var pair = new PartialPair<int>(1, 2, PairPresenceFlags.Left);

        Assert.That(pair.Left, Is.EqualTo(1));
        Assert.That(pair.Right, Is.EqualTo(2));
        Assert.That(pair.Presence, Is.EqualTo(PairPresenceFlags.Left));
        Assert.That(pair.HasLeft, Is.True);
        Assert.That(pair.HasRight, Is.False);
    }

    [Test]
    public void Constructor_WhenBooleanPresenceIsProvided_CreatesPresenceFlags()
    {
        var pair = new PartialPair<int>(1, 2, hasLeft: false, hasRight: true);

        Assert.That(pair.Presence, Is.EqualTo(PairPresenceFlags.Right));
        Assert.That(pair.HasLeft, Is.False);
        Assert.That(pair.HasRight, Is.True);
    }

    [Test]
    public void Constructor_WhenPairIsProvided_CopiesValuesAndPresence()
    {
        var source = new Pair<int>(1, 2);

        var pair = new PartialPair<int>(source, PairPresenceFlags.All);

        Assert.That(pair.Left, Is.EqualTo(1));
        Assert.That(pair.Right, Is.EqualTo(2));
        Assert.That(pair.Presence, Is.EqualTo(PairPresenceFlags.All));
    }

    [Test]
    public void ToPair_ReturnsFullPair()
    {
        var partialPair = new PartialPair<int>(1, 2, PairPresenceFlags.Left);

        Pair<int> pair = partialPair.ToPair();

        Assert.That(pair.Left, Is.EqualTo(1));
        Assert.That(pair.Right, Is.EqualTo(2));
    }

    [Test]
    public void Deconstruct_ReturnsValuesAndPresence()
    {
        var pair = new PartialPair<int>(1, 2, PairPresenceFlags.All);

        var (left, right) = pair;
        pair.Deconstruct(out int leftWithPresence, out int rightWithPresence, out PairPresenceFlags presence);

        Assert.That(left, Is.EqualTo(1));
        Assert.That(right, Is.EqualTo(2));
        Assert.That(leftWithPresence, Is.EqualTo(1));
        Assert.That(rightWithPresence, Is.EqualTo(2));
        Assert.That(presence, Is.EqualTo(PairPresenceFlags.All));
    }
}

