using Akeldov.Math.Hexes.Topology;
using System.IO;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class PolyhexTests
{
    [Test]
    public void Constructor_CopiesSourceArray()
    {
        var source = new bool[2, 2];
        source[0, 1] = true;

        var polyhex = new Polyhex(source);
        source[0, 1] = false;

        Assert.That(polyhex[0, 1], Is.True);
        Assert.That(polyhex.HexCount, Is.EqualTo(1));
    }

    [Test]
    public void ToBoolArray_ReturnsCopy()
    {
        var polyhex = new Polyhex(new[,]
        {
            { true, false },
            { false, true }
        });

        bool[,] copy = polyhex.ToBoolArray();
        copy[0, 0] = false;

        Assert.That(polyhex[0, 0], Is.True);
    }

    [Test]
    public void PolyhexBuilder_CreatesIndependentPolyhex()
    {
        var builder = new PolyhexBuilder(2, 3);
        builder[1, 2] = true;

        Polyhex polyhex = builder.ToPolyhex();
        builder[1, 2] = false;

        Assert.That(polyhex[1, 2], Is.True);
        Assert.That(polyhex.HexCount, Is.EqualTo(1));
    }

    [Test]
    public void GetExtended_ReturnsPolyhex()
    {
        var sourceMask = new bool[1, 1];
        sourceMask[0, 0] = true;
        var polyhex = new Polyhex(sourceMask);

        Polyhex extended = polyhex.GetExtended();

        Assert.That(extended.QRSResolution.Q, Is.EqualTo(3));
        Assert.That(extended.QRSResolution.R, Is.EqualTo(3));
        Assert.That(extended[1, 1], Is.True);
    }

    [Test]
    public void BinaryWriterAndReader_RoundTripRectangularPolyhex()
    {
        var sourceMask = new bool[2, 3];
        sourceMask[0, 0] = true;
        sourceMask[1, 2] = true;
        var source = new Polyhex(sourceMask);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(source);
        }

        stream.Position = 0;

        using var reader = new BinaryReader(stream);
        Polyhex? result = reader.ReadPolyhexStamp();

        Assert.That(result, Is.Not.Null);
        Polyhex polyhex = result!;

        Assert.Multiple(() =>
        {
            Assert.That(polyhex.QRSResolution.Q, Is.EqualTo(2));
            Assert.That(polyhex.QRSResolution.R, Is.EqualTo(3));
            Assert.That(polyhex, Is.EqualTo(source));
            Assert.That(polyhex[0, 0], Is.True);
            Assert.That(polyhex[1, 2], Is.True);
        });
    }
}
