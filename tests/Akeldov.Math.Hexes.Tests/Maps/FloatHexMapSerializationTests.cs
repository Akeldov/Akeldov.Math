using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class FloatHexMapSerializationTests
{
    // Version 1, topology-only Single map, width 3, height 2, EvenQ, row-major values.
    private static byte[] GoldenRecord => new byte[]
    {
        0x48, 0x4D, 0x41, 0x50, 0x01, 0x00, 0x03,
        0x03, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00,
        0x03,
        0x00, 0x00, 0xC0, 0x3F, // 1.5.
        0x00, 0x00, 0x10, 0xC0, // -2.25.
        0x00, 0x00, 0x00, 0x00, // Positive zero.
        0x00, 0x00, 0x00, 0x80, // Negative zero.
        0x00, 0x00, 0x80, 0x7F, // Positive infinity.
        0x45, 0x23, 0xC1, 0xFF, // Negative NaN with a non-default payload.
    };

    private static float[] GoldenValues => new[]
    {
        1.5f, -2.25f, 0f,
        BitConverter.Int32BitsToSingle(int.MinValue),
        float.PositiveInfinity,
        BitConverter.Int32BitsToSingle(unchecked((int)0xFFC12345)),
    };

    [TestCase(Layout.OddR, 0)]
    [TestCase(Layout.EvenR, 1)]
    [TestCase(Layout.OddQ, 2)]
    [TestCase(Layout.EvenQ, 3)]
    public void Write_WritesVersionOneBytes(Layout layout, byte layoutCode)
    {
        var map = new FloatHexMap(new HexMapTopology(3, 2, layout),
            GoldenValues);
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
    public void ReadFloatHexMap_ReadsVersionOneBytes(byte layoutCode, Layout layout)
    {
        byte[] bytes = GoldenRecord;
        bytes[15] = layoutCode;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        FloatHexMap map = reader.ReadFloatHexMap(maxCellCount: 6);

        Assert.Multiple(() =>
        {
            Assert.That(map.Topology, Is.EqualTo(new HexMapTopology(3, 2, layout)));
            Assert.That(map[0], Is.EqualTo(1.5f));
            Assert.That(map[1], Is.EqualTo(-2.25f));
            Assert.That(BitConverter.SingleToInt32Bits(map[2]), Is.Zero);
            Assert.That(BitConverter.SingleToInt32Bits(map[new VectorXYInt(0, 1)]), Is.EqualTo(int.MinValue));
            Assert.That(map[new VectorXYInt(1, 1)], Is.EqualTo(float.PositiveInfinity));
            Assert.That(BitConverter.SingleToInt32Bits(map[new VectorXYInt(2, 1)]), Is.EqualTo(unchecked((int)0xFFC12345)));
            Assert.That(stream.Position, Is.EqualTo(bytes.Length));
        });
    }

    [TestCase(0x00000000u)] // Positive zero.
    [TestCase(0x80000000u)] // Negative zero.
    [TestCase(0x00000001u)] // Smallest positive subnormal.
    [TestCase(0x80000001u)] // Smallest negative subnormal in magnitude.
    [TestCase(0x007FFFFFu)] // Largest positive subnormal.
    [TestCase(0x807FFFFFu)] // Largest negative subnormal in magnitude.
    [TestCase(0x00800000u)] // Smallest positive normal.
    [TestCase(0x80800000u)] // Smallest negative normal in magnitude.
    [TestCase(0x7F7FFFFFu)] // Largest finite positive value.
    [TestCase(0xFF7FFFFFu)] // Most negative finite value.
    [TestCase(0x7F800000u)] // Positive infinity.
    [TestCase(0xFF800000u)] // Negative infinity.
    [TestCase(0x7FC00000u)] // Quiet NaNs, both signs and varying payloads.
    [TestCase(0xFFC00000u)]
    [TestCase(0x7FC12345u)]
    [TestCase(0xFFC12345u)]
    [TestCase(0x7F800001u)] // Signaling NaNs, both signs and varying payloads.
    [TestCase(0xFF800001u)]
    [TestCase(0x7FA12345u)]
    [TestCase(0xFFA12345u)]
    public void RoundTrip_PreservesSpecialValueBits(uint bits)
    {
        int expectedBits = unchecked((int)bits);
        var map = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR),
            new[] { BitConverter.Int32BitsToSingle(expectedBits) });
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(map);
        Assert.That(BinaryPrimitives.ReadInt32LittleEndian(stream.ToArray().AsSpan(16, 4)), Is.EqualTo(expectedBits));
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        FloatHexMap restored = reader.ReadFloatHexMap();

        Assert.That(BitConverter.SingleToInt32Bits(restored[0]), Is.EqualTo(expectedBits));
    }

    [Test]
    public void RoundTrip_ReturnsIndependentMutableStorage()
    {
        var originalValues = new[] { float.MinValue, -42f, 0f, float.MaxValue };
        var original = new FloatHexMap(new HexMapTopology(2, 2, Layout.OddQ), originalValues);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(original);
        originalValues[0] = 7;
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        FloatHexMap restored = reader.ReadFloatHexMap();
        restored[1] = 10;

        Assert.Multiple(() =>
        {
            Assert.That(restored.Topology, Is.EqualTo(original.Topology));
            Assert.That(restored[0], Is.EqualTo(float.MinValue));
            Assert.That(restored[1], Is.EqualTo(10));
            Assert.That(restored[2], Is.Zero);
            Assert.That(restored[3], Is.EqualTo(float.MaxValue));
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
        var map = new FloatHexMap(new HexMapTopology(width, height, layout));
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(map);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        FloatHexMap restored = reader.ReadFloatHexMap(maxCellCount: 0);

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
        var values = GoldenValues;
        IHexMap<float> source = spatial
            ? new SpatialFloatHexMap(new HexMapGeometry(topology, new VectorXY(10f, 20f), 3f), values)
            : new HexMap<float>(topology, values);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(source);

        Assert.That(stream.ToArray(), Is.EqualTo(GoldenRecord));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MixedRecords_PreserveBoundariesAndLeaveStreamOpen(bool nonSeekable)
    {
        var mask = new BoolHexMap(new HexMapTopology(2, 1, Layout.EvenR), new[] { true, false });
        var costs = new IntHexMap(new HexMapTopology(2, 1, Layout.OddQ), new[] { -1, int.MaxValue });
        var heights = new FloatHexMap(new HexMapTopology(3, 2, Layout.EvenQ), GoldenValues);
        using var buffer = new MemoryStream();
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(123456);
        writer.Write(mask);
        writer.Write(heights);
        writer.Write(costs);
        writer.Write(heights);
        writer.Write(654321);
        buffer.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        Assert.That(reader.ReadInt32(), Is.EqualTo(123456));
        BoolHexMap first = reader.ReadBoolHexMap();
        FloatHexMap second = reader.ReadFloatHexMap();
        IntHexMap third = reader.ReadIntHexMap();
        FloatHexMap fourth = reader.ReadFloatHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(first.Topology, Is.EqualTo(mask.Topology));
            Assert.That(first[0], Is.True);
            Assert.That(first[1], Is.False);
            Assert.That(second.Topology, Is.EqualTo(heights.Topology));
            Assert.That(third.Topology, Is.EqualTo(costs.Topology));
            Assert.That(third[0], Is.EqualTo(-1));
            Assert.That(third[1], Is.EqualTo(int.MaxValue));
            Assert.That(fourth.Topology, Is.EqualTo(heights.Topology));
            for (int index = 0; index < heights.Topology.Count; index++)
            {
                Assert.That(BitConverter.SingleToInt32Bits(second[index]),
                    Is.EqualTo(BitConverter.SingleToInt32Bits(heights[index])), $"First float record, cell {index}");
                Assert.That(BitConverter.SingleToInt32Bits(fourth[index]),
                    Is.EqualTo(BitConverter.SingleToInt32Bits(heights[index])), $"Second float record, cell {index}");
            }
            Assert.That(reader.ReadInt32(), Is.EqualTo(654321));
            Assert.That(buffer.Position, Is.EqualTo(buffer.Length));
        });
    }

    [TestCase(3, 1)]
    [TestCase(3, 2)]
    [TestCase(1, 3)]
    [TestCase(2, 3)]
    public void ReadMap_RejectsTheOtherValueKindBeforeReadingDimensions(byte writtenKind, byte readKind)
    {
        var topology = new HexMapTopology(1, 1, Layout.OddR);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        if (writtenKind == 1)
            writer.Write(new BoolHexMap(topology, new[] { true }));
        else if (writtenKind == 2)
            writer.Write(new IntHexMap(topology, new[] { 1 }));
        else
            writer.Write(new FloatHexMap(topology, new[] { 1f }));
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        if (readKind == 1)
            Assert.Throws<InvalidDataException>(() => reader.ReadBoolHexMap());
        else if (readKind == 2)
            Assert.Throws<InvalidDataException>(() => reader.ReadIntHexMap());
        else
            Assert.Throws<InvalidDataException>(() => reader.ReadFloatHexMap());
        Assert.That(stream.Position, Is.EqualTo(7));
    }

    [TestCase(0, 0)] // Signature.
    [TestCase(4, 0)] // Format version.
    [TestCase(4, 2)]
    [TestCase(5, 1)] // Map kind.
    [TestCase(5, 255)]
    [TestCase(6, 0)] // Value kind.
    [TestCase(6, 1)]
    [TestCase(6, 2)]
    [TestCase(6, 255)]
    [TestCase(15, 4)] // Layout.
    [TestCase(15, 255)]
    public void ReadFloatHexMap_InvalidHeaderThrowsInvalidDataException(int offset, byte value)
    {
        byte[] bytes = GoldenRecord;
        bytes[offset] = value;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadFloatHexMap());
    }

    [TestCase(-1, 2)]
    [TestCase(3, -1)]
    [TestCase(-1, 0)]
    [TestCase(65536, 65536)]
    [TestCase(int.MaxValue, 2)]
    public void ReadFloatHexMap_InvalidDimensionsAreRejectedBeforePayloadRead(int width, int height)
    {
        byte[] header = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(7, 4), width);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(11, 4), height);
        using var stream = new MemoryStream(header);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadFloatHexMap());
        Assert.That(stream.Position, Is.EqualTo(16));
    }

    [TestCase(0)]
    [TestCase(5)]
    public void ReadFloatHexMap_CellCountLimitIsCheckedBeforePayloadRead(int maxCellCount)
    {
        using var stream = new NonSeekableStream(new MemoryStream(GoldenRecord.Take(16).ToArray()));
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => reader.ReadFloatHexMap(maxCellCount));
    }

    [TestCase(1_073_741_824)] // The payload size exceeds UInt32 capacity.
    [TestCase(int.MaxValue)]
    public void ReadFloatHexMap_LargeTruncatedRecordIsRejectedBeforeAllocation(int width)
    {
        byte[] header = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(7, 4), width);
        BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(11, 4), 1);
        using var stream = new MemoryStream(header);
        using var reader = new BinaryReader(stream);

        Assert.Throws<EndOfStreamException>(() => reader.ReadFloatHexMap());
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
    public void ReadFloatHexMap_TruncatedRecordThrowsEndOfStreamException(int length, bool nonSeekable)
    {
        using var buffer = new MemoryStream(GoldenRecord.Take(length).ToArray());
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var reader = new BinaryReader(stream);

        Assert.Throws<EndOfStreamException>(() => reader.ReadFloatHexMap());
    }

    [Test]
    public void Write_NullArgumentsAreRejectedBeforeWriting()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var map = new FloatHexMap(new HexMapTopology(1, 1, Layout.OddR));

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                HexMapBinaryWriterExtensions.Write(null!, map))!.ParamName, Is.EqualTo("writer"));
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                writer.Write((IHexMap<float>)null!))!.ParamName, Is.EqualTo("map"));
            Assert.That(stream.Length, Is.Zero);
        });
    }

    [Test]
    public void ReadFloatHexMap_InvalidArgumentsAreRejectedBeforeReading()
    {
        using var stream = new MemoryStream(GoldenRecord);
        using var reader = new BinaryReader(stream);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() =>
                HexMapBinaryReaderExtensions.ReadFloatHexMap(null!))!.ParamName, Is.EqualTo("reader"));
            Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
                reader.ReadFloatHexMap(-1))!.ParamName, Is.EqualTo("maxCellCount"));
            Assert.That(stream.Position, Is.Zero);
        });
    }
}
