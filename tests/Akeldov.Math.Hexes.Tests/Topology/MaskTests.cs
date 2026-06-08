using Akeldov.Math.Hexes.Topology;
using System.IO;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class MaskTests
{
    [Test]
    public void Constructor_CopiesSourceArray()
    {
        var source = new bool[2, 2];
        source[0, 1] = true;

        var mask = new Mask(source);
        source[0, 1] = false;

        Assert.That(mask[0, 1], Is.True);
        Assert.That(mask.PositiveSize, Is.EqualTo(1));
    }

    [Test]
    public void ToBoolArray_ReturnsCopy()
    {
        var mask = new Mask(new[,]
        {
            { true, false },
            { false, true }
        });

        bool[,] copy = mask.ToBoolArray();
        copy[0, 0] = false;

        Assert.That(mask[0, 0], Is.True);
    }

    [Test]
    public void MaskBuilder_CreatesIndependentMask()
    {
        var builder = new MaskBuilder(2, 3);
        builder[1, 2] = true;

        Mask mask = builder.ToMask();
        builder[1, 2] = false;

        Assert.That(mask[1, 2], Is.True);
        Assert.That(mask.PositiveSize, Is.EqualTo(1));
    }

    [Test]
    public void BinaryWriterAndReader_RoundTripRectangularMask()
    {
        var sourceMask = new bool[2, 3];
        sourceMask[0, 0] = true;
        sourceMask[1, 2] = true;
        var source = new Mask(sourceMask);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(source);
        }

        stream.Position = 0;

        using var reader = new BinaryReader(stream);
        Mask result = reader.ReadMask();

        Assert.That(result, Is.EqualTo(source));
    }

    [Test]
    public void BinaryWriterAndReader_RoundTripRectangularPolyhexMask()
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
        Polyhex result = reader.ReadPolyhexStamp();

        Assert.Multiple(() =>
        {
            Assert.That(result.Dimension.Q, Is.EqualTo(2));
            Assert.That(result.Dimension.R, Is.EqualTo(3));
            Assert.That(result.Mask, Is.EqualTo(source.Mask));
            Assert.That(result.Mask[0, 0], Is.True);
            Assert.That(result.Mask[1, 2], Is.True);
        });
    }
}
