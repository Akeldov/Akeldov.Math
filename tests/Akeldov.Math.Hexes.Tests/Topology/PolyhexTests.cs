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
    public void Constructor_WhenIntMaskIsNull_ThrowsArgumentNullException()
    {
        int[,]? source = null;

        var exception = Assert.Throws<ArgumentNullException>(() => _ = new Polyhex(source!));

        Assert.That(exception!.ParamName, Is.EqualTo("intMask"));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsidePolyhex_Throws()
    {
        var polyhex = new Polyhex(new bool[2, 3]);

        Assert.Multiple(() =>
        {
            Assert.Throws<IndexOutOfRangeException>(() => _ = polyhex[0, 3]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = polyhex[-1, 3]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = polyhex[new Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt(-1, 3)]);
        });
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
    public void PolyhexBuilder_Indexer_WhenIndexIsOutsidePolyhex_Throws()
    {
        var builder = new PolyhexBuilder(2, 3);

        Assert.Multiple(() =>
        {
            Assert.Throws<IndexOutOfRangeException>(() => _ = builder[0, 3]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = builder[-1, 3]);
            Assert.Throws<IndexOutOfRangeException>(() => builder[0, 3] = true);
            Assert.Throws<IndexOutOfRangeException>(() => builder[-1, 3] = true);
        });
    }

    [Test]
    public void GetExtended_ForSingleHex_AddsHexAndSixNeighbors()
    {
        var sourceMask = new bool[1, 1];
        sourceMask[0, 0] = true;
        var polyhex = new Polyhex(sourceMask);

        Polyhex extended = polyhex.GetExtended();

        var expected = new[,]
        {
            { false, true, true },
            { true, true, true },
            { true, true, false }
        };

        Assert.Multiple(() =>
        {
            Assert.That(extended.QRSResolution.Q, Is.EqualTo(3));
            Assert.That(extended.QRSResolution.R, Is.EqualTo(3));
            Assert.That(extended.HexCount, Is.EqualTo(7));
            Assert.That(extended.ToBoolArray(), Is.EqualTo(expected));
        });
    }

    [Test]
    public void GetExtended_ForInteriorHex_AddsInteriorNeighbors()
    {
        var sourceMask = new bool[3, 3];
        sourceMask[1, 1] = true;
        var polyhex = new Polyhex(sourceMask);

        Polyhex extended = polyhex.GetExtended();

        Assert.Multiple(() =>
        {
            Assert.That(extended.HexCount, Is.EqualTo(7));
            Assert.That(extended[2, 2], Is.True);
            Assert.That(extended[3, 2], Is.True);
            Assert.That(extended[1, 2], Is.True);
            Assert.That(extended[2, 3], Is.True);
            Assert.That(extended[2, 1], Is.True);
            Assert.That(extended[1, 3], Is.True);
            Assert.That(extended[3, 1], Is.True);
        });
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

    [Test]
    public void BinaryWriter_WhenNull_ThrowsArgumentNullException()
    {
        BinaryWriter? writer = null;

        var exception = Assert.Throws<ArgumentNullException>(() => writer!.Write((Polyhex?)null));

        Assert.That(exception!.ParamName, Is.EqualTo("binaryWriter"));
    }

    [Test]
    public void BinaryReader_WhenNull_ThrowsArgumentNullException()
    {
        BinaryReader? reader = null;

        var exception = Assert.Throws<ArgumentNullException>(() => reader!.ReadPolyhexStamp());

        Assert.That(exception!.ParamName, Is.EqualTo("binaryReader"));
    }
}
