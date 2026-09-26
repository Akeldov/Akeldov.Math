using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class BoolHexMapSerializationTests
{
    // Version 1, topology-only Boolean map, width 3, height 2, EvenQ, row-major values.
    private static byte[] GoldenRecord => new byte[]
    {
        0x48, 0x4D, 0x41, 0x50, 0x01, 0x00, 0x01,
        0x03, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00,
        0x03,
        0x01, 0x00, 0x00, 0x01, 0x01, 0x00,
    };

    [TestCase(Layout.OddR, 0)]
    [TestCase(Layout.EvenR, 1)]
    [TestCase(Layout.OddQ, 2)]
    [TestCase(Layout.EvenQ, 3)]
    public void Write_WritesVersionOneBytes(Layout layout, byte layoutCode)
    {
        var map = new BoolHexMap(new HexMapTopology(3, 2, layout),
            new[] { true, false, false, true, true, false });
        byte[] expected = GoldenRecord;
        expected[15] = layoutCode;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(map);

        Assert.That(stream.ToArray(), Is.EqualTo(expected));
    }

    [TestCase(0, Layout.OddR)]
    [TestCase(1, Layout.EvenR)]
    [TestCase(2, Layout.OddQ)]
    [TestCase(3, Layout.EvenQ)]
    public void ReadBoolHexMap_ReadsVersionOneBytes(byte layoutCode, Layout layout)
    {
        byte[] bytes = GoldenRecord;
        bytes[15] = layoutCode;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        BoolHexMap map = reader.ReadBoolHexMap(maxCellCount: 6);

        Assert.Multiple(() =>
        {
            Assert.That(map.Topology, Is.EqualTo(new HexMapTopology(3, 2, layout)));
            Assert.That(map[0], Is.True);
            Assert.That(map[1], Is.False);
            Assert.That(map[2], Is.False);
            Assert.That(map[new VectorXYInt(0, 1)], Is.True);
            Assert.That(map[new VectorXYInt(1, 1)], Is.True);
            Assert.That(map[new VectorXYInt(2, 1)], Is.False);
            Assert.That(stream.Position, Is.EqualTo(bytes.Length));
        });
    }

    [Test]
    public void RoundTrip_ReturnsIndependentMutableStorage()
    {
        var originalValues = new[] { true, false, true, false };
        var original = new BoolHexMap(new HexMapTopology(2, 2, Layout.OddQ), originalValues);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(original);
        originalValues[0] = false;
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        BoolHexMap restored = reader.ReadBoolHexMap();
        restored[1] = true;

        Assert.Multiple(() =>
        {
            Assert.That(restored.Topology, Is.EqualTo(original.Topology));
            Assert.That(restored[0], Is.True);
            Assert.That(restored[1], Is.True);
            Assert.That(restored[2], Is.True);
            Assert.That(restored[3], Is.False);
            Assert.That(original[0], Is.False);
            Assert.That(original[1], Is.False);
        });
    }

    [TestCase(0, 0, Layout.OddR)]
    [TestCase(0, 4, Layout.EvenR)]
    [TestCase(3, 0, Layout.OddQ)]
    [TestCase(int.MaxValue, 0, Layout.EvenQ)]
    public void RoundTrip_EmptyMapPreservesDimensionsAndLayout(int width, int height, Layout layout)
    {
        var map = new BoolHexMap(new HexMapTopology(width, height, layout));
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(map);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        BoolHexMap restored = reader.ReadBoolHexMap(maxCellCount: 0);

        Assert.Multiple(() =>
        {
            Assert.That(restored.Topology, Is.EqualTo(map.Topology));
            Assert.That(restored.Topology.Count, Is.Zero);
            Assert.That(stream.Length, Is.EqualTo(16));
            Assert.That(stream.Position, Is.EqualTo(stream.Length));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Write_InterfaceSourceSerializesOnlyTopologyAndValues(bool spatial)
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var values = new[] { true, false, false, true, true, false };
        IHexMap<bool> source = spatial
            ? new SpatialBoolHexMap(new HexMapGeometry(topology, new VectorXY(10f, 20f), 3f), values)
            : new HexMap<bool>(topology, values);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(source);

        Assert.That(stream.ToArray(), Is.EqualTo(GoldenRecord));
    }

    [Test]
    public void Records_CanBeEmbeddedAndConcatenatedWithoutClosingTheStream()
    {
        var first = new BoolHexMap(new HexMapTopology(1, 1, Layout.EvenR), new[] { true });
        var second = new BoolHexMap(new HexMapTopology(2, 1, Layout.OddQ), new[] { false, true });
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(123456);
        writer.Write(first);
        writer.Write(second);
        writer.Write(654321);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        Assert.That(reader.ReadInt32(), Is.EqualTo(123456));
        BoolHexMap restoredFirst = reader.ReadBoolHexMap();
        BoolHexMap restoredSecond = reader.ReadBoolHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(restoredFirst.Topology, Is.EqualTo(first.Topology));
            Assert.That(restoredFirst[0], Is.True);
            Assert.That(restoredSecond.Topology, Is.EqualTo(second.Topology));
            Assert.That(restoredSecond[0], Is.False);
            Assert.That(restoredSecond[1], Is.True);
            Assert.That(reader.ReadInt32(), Is.EqualTo(654321));
            Assert.That(stream.Position, Is.EqualTo(stream.Length));
        });
    }

    [Test]
    public void Records_WorkWithNonSeekableStreamsAndShortReads()
    {
        var map = new BoolHexMap(new HexMapTopology(3, 2, Layout.EvenQ),
            new[] { true, false, false, true, true, false });
        using var buffer = new MemoryStream();
        using var stream = new NonSeekableStream(buffer);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(map);
        writer.Write((byte)42);
        buffer.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        BoolHexMap restored = reader.ReadBoolHexMap();

        Assert.That(restored.Topology, Is.EqualTo(map.Topology));
        for (int index = 0; index < map.Topology.Count; index++)
            Assert.That(restored[index], Is.EqualTo(map[index]), $"Cell {index}");
        Assert.That(reader.ReadByte(), Is.EqualTo(42));
    }

    [TestCase(0, 0)] // Signature.
    [TestCase(4, 0)] // Old or unknown format version.
    [TestCase(4, 2)]
    [TestCase(5, 1)] // Spatial or unknown map kind.
    [TestCase(5, 255)]
    [TestCase(6, 0)] // Non-Boolean value kind.
    [TestCase(6, 2)]
    [TestCase(15, 4)] // Layout.
    [TestCase(15, 255)]
    [TestCase(16, 2)] // First Boolean payload byte.
    [TestCase(21, 255)] // Last Boolean payload byte.
    public void ReadBoolHexMap_InvalidRecordThrowsInvalidDataException(int offset, byte value)
    {
        byte[] bytes = GoldenRecord;
        bytes[offset] = value;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadBoolHexMap());
    }

    [TestCase(-1, 2)]
    [TestCase(3, -1)]
    [TestCase(-1, 0)]
    [TestCase(65536, 65536)]
    [TestCase(int.MaxValue, 2)]
    public void ReadBoolHexMap_InvalidDimensionsAreRejectedBeforePayloadRead(int width, int height)
    {
        byte[] header = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(7, 4), width);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(11, 4), height);
        using var stream = new MemoryStream(header);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadBoolHexMap());
        Assert.That(stream.Position, Is.EqualTo(16));
    }

    [TestCase(0)]
    [TestCase(5)]
    public void ReadBoolHexMap_CellCountLimitIsCheckedBeforePayloadRead(int maxCellCount)
    {
        using var stream = new NonSeekableStream(new MemoryStream(GoldenRecord.Take(16).ToArray()));
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadBoolHexMap(maxCellCount));
    }

    [Test]
    public void ReadBoolHexMap_LargeTruncatedSeekableRecordIsRejectedBeforeAllocation()
    {
        byte[] header = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(7, 4), int.MaxValue);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(11, 4), 1);
        using var stream = new MemoryStream(header);
        using var reader = new BinaryReader(stream);

        Assert.Throws<EndOfStreamException>(() => reader.ReadBoolHexMap());
    }

    private static IEnumerable<TestCaseData> TruncatedRecords()
    {
        for (int length = 0; length < GoldenRecord.Length; length++)
        {
            yield return new TestCaseData(length, false);
            yield return new TestCaseData(length, true);
        }
    }

    [TestCaseSource(nameof(TruncatedRecords))]
    public void ReadBoolHexMap_TruncatedRecordThrowsEndOfStreamException(int length, bool nonSeekable)
    {
        using var buffer = new MemoryStream(GoldenRecord.Take(length).ToArray());
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var reader = new BinaryReader(stream);

        Assert.Throws<EndOfStreamException>(() => reader.ReadBoolHexMap());
    }

    [Test]
    public void Write_NullArgumentsAreRejectedBeforeWriting()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var map = new BoolHexMap(new HexMapTopology(1, 1, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                HexMapBinaryWriterExtensions.Write(null!, map))!.ParamName, Is.EqualTo("writer"));
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                writer.Write((IHexMap<bool>)null!))!.ParamName, Is.EqualTo("map"));
            Assert.That(stream.Length, Is.Zero);
        });
    }

    [Test]
    public void ReadBoolHexMap_InvalidArgumentsAreRejectedBeforeReading()
    {
        using var stream = new MemoryStream(GoldenRecord);
        using var reader = new BinaryReader(stream);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                HexMapBinaryReaderExtensions.ReadBoolHexMap(null!))!.ParamName, Is.EqualTo("reader"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
                reader.ReadBoolHexMap(-1))!.ParamName, Is.EqualTo("maxCellCount"));
            Assert.That(stream.Position, Is.Zero);
        });
    }
}
