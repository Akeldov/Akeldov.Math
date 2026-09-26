using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class IntHexMapSerializationTests
{
    // Version 1, topology-only Int32 map, width 3, height 2, EvenQ, row-major values.
    private static byte[] GoldenRecord => new byte[]
    {
        0x48, 0x4D, 0x41, 0x50, 0x01, 0x00, 0x02,
        0x03, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00,
        0x03,
        0x00, 0x00, 0x00, 0x80, // Int32.MinValue.
        0xFF, 0xFF, 0xFF, 0x7F, // Int32.MaxValue.
        0x00, 0x00, 0x00, 0x00, // Zero.
        0xFF, 0xFF, 0xFF, 0xFF, // Minus one.
        0x78, 0x56, 0x34, 0x12, // 0x12345678.
        0xFE, 0xFF, 0xFF, 0xFF, // Minus two.
    };

    [TestCase(Layout.OddR, 0)]
    [TestCase(Layout.EvenR, 1)]
    [TestCase(Layout.OddQ, 2)]
    [TestCase(Layout.EvenQ, 3)]
    public void WriteHexMap_WritesVersionOneBytes(Layout layout, byte layoutCode)
    {
        var map = new IntHexMap(new HexMapTopology(3, 2, layout),
            new[] { int.MinValue, int.MaxValue, 0, -1, 0x12345678, -2 });
        byte[] expected = GoldenRecord;
        expected[15] = layoutCode;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.WriteHexMap(map);

        Assert.That(stream.ToArray(), Is.EqualTo(expected));
    }

    [TestCase(0, Layout.OddR)]
    [TestCase(1, Layout.EvenR)]
    [TestCase(2, Layout.OddQ)]
    [TestCase(3, Layout.EvenQ)]
    public void ReadIntHexMap_ReadsVersionOneBytes(byte layoutCode, Layout layout)
    {
        byte[] bytes = GoldenRecord;
        bytes[15] = layoutCode;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        IntHexMap map = reader.ReadIntHexMap(maxCellCount: 6);

        Assert.Multiple(() =>
        {
            Assert.That(map.Topology, Is.EqualTo(new HexMapTopology(3, 2, layout)));
            Assert.That(map[0], Is.EqualTo(int.MinValue));
            Assert.That(map[1], Is.EqualTo(int.MaxValue));
            Assert.That(map[2], Is.Zero);
            Assert.That(map[new VectorXYInt(0, 1)], Is.EqualTo(-1));
            Assert.That(map[new VectorXYInt(1, 1)], Is.EqualTo(0x12345678));
            Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(-2));
            Assert.That(stream.Position, Is.EqualTo(bytes.Length));
        });
    }

    [Test]
    public void RoundTrip_ReturnsIndependentMutableStorage()
    {
        var originalValues = new[] { int.MinValue, -42, 0, int.MaxValue };
        var original = new IntHexMap(new HexMapTopology(2, 2, Layout.OddQ), originalValues);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.WriteHexMap(original);
        originalValues[0] = 7;
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        IntHexMap restored = reader.ReadIntHexMap();
        restored[1] = 10;

        Assert.Multiple(() =>
        {
            Assert.That(restored.Topology, Is.EqualTo(original.Topology));
            Assert.That(restored[0], Is.EqualTo(int.MinValue));
            Assert.That(restored[1], Is.EqualTo(10));
            Assert.That(restored[2], Is.Zero);
            Assert.That(restored[3], Is.EqualTo(int.MaxValue));
            Assert.That(original[0], Is.EqualTo(7));
            Assert.That(original[1], Is.EqualTo(-42));
        });
    }

    [TestCase(0, 0, Layout.OddR)]
    [TestCase(0, 4, Layout.EvenR)]
    [TestCase(3, 0, Layout.OddQ)]
    [TestCase(int.MaxValue, 0, Layout.EvenQ)]
    public void RoundTrip_EmptyMapPreservesDimensionsAndLayout(int width, int height, Layout layout)
    {
        var map = new IntHexMap(new HexMapTopology(width, height, layout));
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.WriteHexMap(map);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        IntHexMap restored = reader.ReadIntHexMap(maxCellCount: 0);

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
    public void WriteHexMap_InterfaceSourceSerializesOnlyTopologyAndValues(bool spatial)
    {
        var topology = new HexMapTopology(3, 2, Layout.EvenQ);
        var values = new[] { int.MinValue, int.MaxValue, 0, -1, 0x12345678, -2 };
        IHexMap<int> source = spatial
            ? new SpatialIntHexMap(new HexMapGeometry(topology, new VectorXY(10f, 20f), 3f), values)
            : new HexMap<int>(topology, values);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.WriteHexMap(source);

        Assert.That(stream.ToArray(), Is.EqualTo(GoldenRecord));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MixedRecords_PreserveBoundariesAndLeaveStreamOpen(bool nonSeekable)
    {
        var mask = new BoolHexMap(new HexMapTopology(2, 1, Layout.EvenR), new[] { true, false });
        var costs = new IntHexMap(new HexMapTopology(2, 1, Layout.OddQ), new[] { -1, int.MaxValue });
        using var buffer = new MemoryStream();
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(123456);
        writer.WriteHexMap(mask);
        writer.WriteHexMap(costs);
        writer.WriteHexMap(mask);
        writer.Write(654321);
        buffer.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        Assert.That(reader.ReadInt32(), Is.EqualTo(123456));
        BoolHexMap first = reader.ReadBoolHexMap();
        IntHexMap second = reader.ReadIntHexMap();
        BoolHexMap third = reader.ReadBoolHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(first.Topology, Is.EqualTo(mask.Topology));
            Assert.That(first[0], Is.True);
            Assert.That(first[1], Is.False);
            Assert.That(second.Topology, Is.EqualTo(costs.Topology));
            Assert.That(second[0], Is.EqualTo(-1));
            Assert.That(second[1], Is.EqualTo(int.MaxValue));
            Assert.That(third.Topology, Is.EqualTo(mask.Topology));
            Assert.That(third[0], Is.True);
            Assert.That(third[1], Is.False);
            Assert.That(reader.ReadInt32(), Is.EqualTo(654321));
            Assert.That(buffer.Position, Is.EqualTo(buffer.Length));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ReadMap_RejectsTheOtherValueKindBeforeReadingDimensions(bool readAsBoolean)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        if (readAsBoolean)
            writer.WriteHexMap(new IntHexMap(topology, new[] { 1 }));
        else
            writer.WriteHexMap(new BoolHexMap(topology, new[] { true }));
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        if (readAsBoolean)
            Assert.Throws<InvalidDataException>(() => reader.ReadBoolHexMap());
        else
            Assert.Throws<InvalidDataException>(() => reader.ReadIntHexMap());
        Assert.That(stream.Position, Is.EqualTo(7));
    }

    [TestCase(0, 0)] // Signature.
    [TestCase(4, 0)] // Format version.
    [TestCase(4, 2)]
    [TestCase(5, 1)] // Map kind.
    [TestCase(5, 255)]
    [TestCase(6, 0)] // Value kind.
    [TestCase(6, 1)]
    [TestCase(6, 255)]
    [TestCase(15, 4)] // Layout.
    [TestCase(15, 255)]
    public void ReadIntHexMap_InvalidHeaderThrowsInvalidDataException(int offset, byte value)
    {
        byte[] bytes = GoldenRecord;
        bytes[offset] = value;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadIntHexMap());
    }

    [TestCase(-1, 2)]
    [TestCase(3, -1)]
    [TestCase(-1, 0)]
    [TestCase(65536, 65536)]
    [TestCase(int.MaxValue, 2)]
    public void ReadIntHexMap_InvalidDimensionsAreRejectedBeforePayloadRead(int width, int height)
    {
        byte[] header = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(7, 4), width);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(11, 4), height);
        using var stream = new MemoryStream(header);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadIntHexMap());
        Assert.That(stream.Position, Is.EqualTo(16));
    }

    [TestCase(0)]
    [TestCase(5)]
    public void ReadIntHexMap_CellCountLimitIsCheckedBeforePayloadRead(int maxCellCount)
    {
        using var stream = new NonSeekableStream(new MemoryStream(GoldenRecord.Take(16).ToArray()));
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadIntHexMap(maxCellCount));
    }

    [TestCase(1_073_741_824)] // The payload size exceeds UInt32 capacity.
    [TestCase(int.MaxValue)]
    public void ReadIntHexMap_LargeTruncatedRecordIsRejectedBeforeAllocation(int width)
    {
        byte[] header = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(7, 4), width);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(11, 4), 1);
        using var stream = new MemoryStream(header);
        using var reader = new BinaryReader(stream);

        Assert.Throws<EndOfStreamException>(() => reader.ReadIntHexMap());
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
    public void ReadIntHexMap_TruncatedRecordThrowsEndOfStreamException(int length, bool nonSeekable)
    {
        using var buffer = new MemoryStream(GoldenRecord.Take(length).ToArray());
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var reader = new BinaryReader(stream);

        Assert.Throws<EndOfStreamException>(() => reader.ReadIntHexMap());
    }

    [Test]
    public void WriteHexMap_NullArgumentsAreRejectedBeforeWriting()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var map = new IntHexMap(new HexMapTopology(1, 1, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                HexMapBinaryWriterExtensions.WriteHexMap(null!, map))!.ParamName, Is.EqualTo("writer"));
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                writer.WriteHexMap((IHexMap<int>)null!))!.ParamName, Is.EqualTo("map"));
            Assert.That(stream.Length, Is.Zero);
        });
    }

    [Test]
    public void ReadIntHexMap_InvalidArgumentsAreRejectedBeforeReading()
    {
        using var stream = new MemoryStream(GoldenRecord);
        using var reader = new BinaryReader(stream);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                HexMapBinaryReaderExtensions.ReadIntHexMap(null!))!.ParamName, Is.EqualTo("reader"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
                reader.ReadIntHexMap(-1))!.ParamName, Is.EqualTo("maxCellCount"));
            Assert.That(stream.Position, Is.Zero);
        });
    }
}
