using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Akeldov.Math.Hexes.Tests.Maps;

[TestFixture(typeof(bool), 1)]
[TestFixture(typeof(int), 2)]
[TestFixture(typeof(float), 3)]
public class SpatialHexMapSerializationTests<T>
{
    private readonly int _kind;

    public SpatialHexMapSerializationTests(int kind) => _kind = kind;

    private HexMapGeometry GoldenGeometry(Layout layout = Layout.EvenQ) =>
        new(new HexMapTopology(3, 2, layout), new VectorXY(-2.25f, 1.5f), 0.5f);

    private T[] GoldenValues => (T[])(_kind switch
    {
        1 => (object)new[] { true, false, false, true, true, false },
        2 => new[] { int.MinValue, int.MaxValue, 0, -1, 0x12345678, -2 },
        _ => new[] { 1.5f, -2.25f, 0f, BitConverter.Int32BitsToSingle(int.MinValue),
            float.PositiveInfinity, BitConverter.Int32BitsToSingle(unchecked((int)0xFFC12345)) },
    });

    // Independent version-1 wire bytes: spatial map, 3 x 2, EvenQ, origin (-2.25, 1.5), radius 0.5.
    private byte[] GoldenRecord => new byte[]
    {
        0x48, 0x4D, 0x41, 0x50, 0x01, 0x01, (byte)_kind,
        0x03, 0x00, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00,
        0x03,
        0x00, 0x00, 0x10, 0xC0,
        0x00, 0x00, 0xC0, 0x3F,
        0x00, 0x00, 0x00, 0x3F,
    }.Concat(_kind switch
    {
        1 => new byte[] { 1, 0, 0, 1, 1, 0 },
        2 => new byte[]
        {
            0x00, 0x00, 0x00, 0x80,
            0xFF, 0xFF, 0xFF, 0x7F,
            0x00, 0x00, 0x00, 0x00,
            0xFF, 0xFF, 0xFF, 0xFF,
            0x78, 0x56, 0x34, 0x12,
            0xFE, 0xFF, 0xFF, 0xFF,
        },
        _ => new byte[]
        {
            0x00, 0x00, 0xC0, 0x3F,
            0x00, 0x00, 0x10, 0xC0,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x80,
            0x00, 0x00, 0x80, 0x7F,
            0x45, 0x23, 0xC1, 0xFF,
        },
    }).ToArray();

    [TestCase(Layout.OddR, 0)]
    [TestCase(Layout.EvenR, 1)]
    [TestCase(Layout.OddQ, 2)]
    [TestCase(Layout.EvenQ, 3)]
    public void Write_WritesVersionOneBytes(Layout layout, byte layoutCode)
    {
        var map = CreateMap(GoldenGeometry(layout), GoldenValues);
        byte[] expected = GoldenRecord;
        expected[15] = layoutCode;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        Write(writer, map);

        Assert.That(stream.ToArray(), Is.EqualTo(expected));
    }

    [TestCase(0, Layout.OddR)]
    [TestCase(1, Layout.EvenR)]
    [TestCase(2, Layout.OddQ)]
    [TestCase(3, Layout.EvenQ)]
    public void ReadSpatialHexMap_ReadsVersionOneBytes(byte layoutCode, Layout layout)
    {
        byte[] bytes = GoldenRecord;
        bytes[15] = layoutCode;
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        ISpatialHexMap<T> map = Read(reader, 6);

        AssertGeometry(map.Geometry, GoldenGeometry(layout));
        Assert.That(map.Topology, Is.EqualTo(map.Geometry.Topology));
        AssertValues(map, GoldenValues);
        Assert.That(map[new VectorXYInt(1, 1)], Is.EqualTo(GoldenValues[4]));
        Assert.That(stream.Position, Is.EqualTo(bytes.Length));
    }

    [Test]
    public void Write_AcceptsGenericInterfaceSource()
    {
        ISpatialHexMap<T> map = new SpatialHexMap<T>(GoldenGeometry(), GoldenValues);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        Write(writer, map);

        Assert.That(stream.ToArray(), Is.EqualTo(GoldenRecord));
    }

    [Test]
    public void RoundTrip_ReturnsIndependentMutableSpecializedMap()
    {
        T[] originalValues = GoldenValues;
        var original = CreateMap(GoldenGeometry(), originalValues);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        Write(writer, original);
        originalValues[0] = originalValues[1];
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        ISpatialHexMap<T> restored = Read(reader);

        Type expectedType = _kind switch
        {
            1 => typeof(SpatialBoolHexMap),
            2 => typeof(SpatialIntHexMap),
            _ => typeof(SpatialFloatHexMap),
        };
        Assert.That(restored, Is.TypeOf(expectedType));
        AssertGeometry(restored.Geometry, original.Geometry);
        AssertValues(restored, GoldenValues);
        ((HexMap<T>)restored)[1] = GoldenValues[0];
        Assert.That(original[1], Is.EqualTo(GoldenValues[1]));
    }

    [TestCase(0, 0, Layout.OddR)]
    [TestCase(0, 4, Layout.EvenR)]
    [TestCase(3, 0, Layout.OddQ)]
    [TestCase(int.MaxValue, 0, Layout.EvenQ)]
    [TestCase(0, int.MaxValue, Layout.OddR)]
    public void RoundTrip_EmptyMapRetainsGeometry(int width, int height, Layout layout)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(width, height, layout),
            new VectorXY(-10, 20), 3f);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        Write(writer, CreateMap(geometry, Array.Empty<T>()));
        Assert.That(stream.Length, Is.EqualTo(28));
        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        var restored = Read(reader, 0);

        AssertGeometry(restored.Geometry, geometry);
        Assert.That(restored.Topology.Count, Is.Zero);
    }

    [TestCase(0x80000000u, 0x00000001u, 0x00000001u)] // Signed zero and minimum subnormals.
    [TestCase(0x00000000u, 0x80000000u, 0x00800000u)] // Minimum normal radius.
    [TestCase(0x7F7FFFFFu, 0xFF7FFFFFu, 0x7F7FFFFFu)] // Finite extrema.
    public void RoundTrip_PreservesGeometryBits(uint x, uint y, uint radius)
    {
        var geometry = new HexMapGeometry(new HexMapTopology(3, 2, Layout.OddR),
            new VectorXY(BitConverter.Int32BitsToSingle((int)x), BitConverter.Int32BitsToSingle((int)y)),
            BitConverter.Int32BitsToSingle((int)radius));
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        Write(writer, CreateMap(geometry, GoldenValues));
        byte[] bytes = stream.ToArray();
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(16)), Is.EqualTo(x));
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(20)), Is.EqualTo(y));
        Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24)), Is.EqualTo(radius));
        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        AssertGeometry(Read(reader).Geometry, geometry);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RoundTrip_ConsumesOneRecordAndLeavesStreamUsable(bool nonSeekable)
    {
        var map = CreateMap(GoldenGeometry(), GoldenValues);
        using var buffer = new MemoryStream();
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(123456);
        Write(writer, map);
        Write(writer, map);
        writer.Write(654321);
        buffer.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        Assert.That(reader.ReadInt32(), Is.EqualTo(123456));
        var first = Read(reader);
        Assert.That(buffer.Position, Is.EqualTo(4 + GoldenRecord.Length));
        var second = Read(reader);
        AssertGeometry(first.Geometry, map.Geometry);
        AssertGeometry(second.Geometry, map.Geometry);
        AssertValues(first, GoldenValues);
        AssertValues(second, GoldenValues);
        Assert.That(reader.ReadInt32(), Is.EqualTo(654321));
        Assert.That(buffer.Position, Is.EqualTo(buffer.Length));
        Assert.That(stream.CanWrite, Is.True);
    }

    [TestCase(0, 0)]
    [TestCase(4, 0)]
    [TestCase(4, 2)]
    [TestCase(5, 0)]
    [TestCase(5, 255)]
    [TestCase(6, 0)]
    [TestCase(6, 255)]
    [TestCase(15, 4)]
    [TestCase(15, 255)]
    public void ReadSpatialHexMap_InvalidHeaderIsRejected(int offset, byte value)
    {
        byte[] bytes = GoldenRecord;
        bytes[offset] = value;
        using var reader = new BinaryReader(new MemoryStream(bytes));

        Assert.Throws<InvalidDataException>(() => Read(reader));
    }

    [TestCase(-1, 2)]
    [TestCase(3, -1)]
    [TestCase(-1, 0)]
    [TestCase(65536, 65536)]
    [TestCase(int.MaxValue, 2)]
    public void ReadSpatialHexMap_InvalidDimensionsAreRejectedBeforeGeometry(int width, int height)
    {
        byte[] bytes = GoldenRecord.Take(16).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(7), width);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(11), height);
        using var stream = new MemoryStream(bytes);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() => Read(reader));
        Assert.That(stream.Position, Is.EqualTo(16));
    }

    [TestCase(0)]
    [TestCase(5)]
    public void ReadSpatialHexMap_CellLimitIsCheckedBeforeGeometry(int maxCellCount)
    {
        using var reader = new BinaryReader(new NonSeekableStream(new MemoryStream(GoldenRecord.Take(16).ToArray())));

        Assert.Throws<InvalidDataException>(() => Read(reader, maxCellCount));
    }

    [TestCase(1_073_741_824)]
    [TestCase(int.MaxValue)]
    public void ReadSpatialHexMap_LargeTruncatedPayloadIsRejectedBeforeAllocation(int width)
    {
        byte[] bytes = GoldenRecord.Take(28).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(7), width);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(11), 1);
        using var reader = new BinaryReader(new MemoryStream(bytes));

        Assert.Throws<EndOfStreamException>(() => Read(reader));
    }

    [TestCase(16, 0x7F800000u)]
    [TestCase(16, 0xFF800000u)]
    [TestCase(16, 0x7FC12345u)]
    [TestCase(20, 0x7F800000u)]
    [TestCase(20, 0xFF800000u)]
    [TestCase(20, 0xFFC12345u)]
    [TestCase(24, 0x00000000u)]
    [TestCase(24, 0x80000000u)]
    [TestCase(24, 0xBF800000u)]
    [TestCase(24, 0x7F800000u)]
    [TestCase(24, 0xFF800000u)]
    [TestCase(24, 0x7FC12345u)]
    public void ReadSpatialHexMap_InvalidGeometryIsRejectedBeforeValues(int offset, uint bits)
    {
        byte[] bytes = GoldenRecord.Take(28).ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), bits);
        using var reader = new BinaryReader(new NonSeekableStream(new MemoryStream(bytes)));

        Assert.Throws<InvalidDataException>(() => Read(reader));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ReadSpatialHexMap_EveryTruncatedPrefixIsRejected(bool nonSeekable)
    {
        byte[] bytes = GoldenRecord;
        for (int length = 0; length < bytes.Length; length++)
        {
            using var buffer = new MemoryStream(bytes.Take(length).ToArray());
            using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
            using var reader = new BinaryReader(stream);

            Assert.Throws<EndOfStreamException>(() => Read(reader), $"Truncated at byte {length}");
        }
    }

    [Test]
    public void ReadSpatialHexMap_RejectsOtherValueKindsBeforeDimensions()
    {
        foreach (byte kind in new byte[] { 1, 2, 3 }.Where(kind => kind != _kind))
        {
            byte[] bytes = GoldenRecord;
            bytes[6] = kind;
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            Assert.Throws<InvalidDataException>(() => Read(reader), $"Value kind {kind}");
            Assert.That(stream.Position, Is.EqualTo(7));
        }
    }

    [Test]
    public void TopologyReader_RejectsSpatialRecordBeforeValues()
    {
        using var stream = new MemoryStream(GoldenRecord);
        using var reader = new BinaryReader(stream);

        Assert.Throws<InvalidDataException>(() =>
        {
            if (_kind == 1) reader.ReadBoolHexMap();
            else if (_kind == 2) reader.ReadIntHexMap();
            else reader.ReadFloatHexMap();
        });
        Assert.That(stream.Position, Is.EqualTo(6));
    }

    [Test]
    public void Write_NullArgumentsAreRejectedBeforeWriting()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var map = CreateMap(GoldenGeometry(), GoldenValues);

        Assert.That(Assert.Throws<ArgumentNullException>(() => Write(null!, map))!.ParamName, Is.EqualTo("writer"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => Write(writer, null!))!.ParamName, Is.EqualTo("map"));
        Assert.That(stream.Length, Is.Zero);
    }

    [Test]
    public void Write_DefaultGeometryIsRejectedBeforeWriting()
    {
        var map = new CustomSpatialMap(default, default);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Write(writer, map))!.ParamName, Is.EqualTo("map"));
        Assert.That(stream.Length, Is.Zero);
    }

    [TestCase(2, 3, Layout.EvenQ)]
    [TestCase(3, 2, Layout.OddR)]
    public void Write_InconsistentTopologyIsRejectedBeforeWriting(int width, int height, Layout layout)
    {
        var map = new CustomSpatialMap(new HexMapTopology(width, height, layout), GoldenGeometry());
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        Assert.That(Assert.Throws<ArgumentException>(() => Write(writer, map))!.ParamName, Is.EqualTo("map"));
        Assert.That(stream.Length, Is.Zero);
    }

    [Test]
    public void ReadSpatialHexMap_InvalidArgumentsAreRejectedBeforeReading()
    {
        using var stream = new MemoryStream(GoldenRecord);
        using var reader = new BinaryReader(stream);

        Assert.That(Assert.Throws<ArgumentNullException>(() => Read(null!))!.ParamName, Is.EqualTo("reader"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Read(reader, -1))!.ParamName, Is.EqualTo("maxCellCount"));
        Assert.That(stream.Position, Is.Zero);
    }

    private ISpatialHexMap<T> CreateMap(HexMapGeometry geometry, T[] values) => (ISpatialHexMap<T>)(_kind switch
    {
        1 => (object)new SpatialBoolHexMap(geometry, (bool[])(object)values),
        2 => new SpatialIntHexMap(geometry, (int[])(object)values),
        _ => new SpatialFloatHexMap(geometry, (float[])(object)values),
    });

    private void Write(BinaryWriter writer, ISpatialHexMap<T> map)
    {
        if (_kind == 1) writer.Write((ISpatialHexMap<bool>)(object)map);
        else if (_kind == 2) writer.Write((ISpatialHexMap<int>)(object)map);
        else writer.Write((ISpatialHexMap<float>)(object)map);
    }

    private ISpatialHexMap<T> Read(BinaryReader reader, int maxCellCount = int.MaxValue) => (ISpatialHexMap<T>)(_kind switch
    {
        1 => (object)reader.ReadSpatialBoolHexMap(maxCellCount),
        2 => reader.ReadSpatialIntHexMap(maxCellCount),
        _ => reader.ReadSpatialFloatHexMap(maxCellCount),
    });

    private void AssertValues(IHexMap<T> map, T[] expected)
    {
        for (int index = 0; index < expected.Length; index++)
        {
            if (_kind == 3)
                Assert.That(BitConverter.SingleToInt32Bits((float)(object)map[index]!),
                    Is.EqualTo(BitConverter.SingleToInt32Bits((float)(object)expected[index]!)), $"Cell {index}");
            else
                Assert.That(map[index], Is.EqualTo(expected[index]), $"Cell {index}");
        }
    }

    private static void AssertGeometry(HexMapGeometry actual, HexMapGeometry expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.Topology, Is.EqualTo(expected.Topology));
            Assert.That(BitConverter.SingleToInt32Bits(actual.Origin.X), Is.EqualTo(BitConverter.SingleToInt32Bits(expected.Origin.X)));
            Assert.That(BitConverter.SingleToInt32Bits(actual.Origin.Y), Is.EqualTo(BitConverter.SingleToInt32Bits(expected.Origin.Y)));
            Assert.That(BitConverter.SingleToInt32Bits(actual.Radius), Is.EqualTo(BitConverter.SingleToInt32Bits(expected.Radius)));
            Assert.That(actual.Apothem, Is.EqualTo(expected.Apothem));
        });
    }

    private sealed class CustomSpatialMap : HexMap<T>, ISpatialHexMap<T>
    {
        public CustomSpatialMap(HexMapTopology topology, HexMapGeometry geometry) : base(topology) => Geometry = geometry;
        public HexMapGeometry Geometry { get; }
    }
}
