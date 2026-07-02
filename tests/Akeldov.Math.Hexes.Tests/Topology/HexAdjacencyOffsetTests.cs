using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexAdjacencyOffsetTests
{
    [Test]
    public void GetRelativeOffsets_ReturnsCallerOwnedCopy()
    {
        VectorXYInt[] offsets = true.GetRelativeOffsets(Layout.OddR);
        offsets[0] = new VectorXYInt(42, 42);

        VectorXYInt[] freshOffsets = true.GetRelativeOffsets(Layout.OddR);

        Assert.That(freshOffsets[0], Is.EqualTo(new VectorXYInt(1, 0)));
    }
}
