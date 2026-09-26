using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class SpatialHexMapSerializationPayloadTests
{
    [TestCase(0x00000000u)]
    [TestCase(0x80000000u)]
    [TestCase(0x00000001u)]
    [TestCase(0x80000001u)]
    [TestCase(0x007FFFFFu)]
    [TestCase(0x807FFFFFu)]
    [TestCase(0x00800000u)]
    [TestCase(0x80800000u)]
    [TestCase(0x7F7FFFFFu)]
    [TestCase(0xFF7FFFFFu)]
    [TestCase(0x7F800000u)]
    [TestCase(0xFF800000u)]
    [TestCase(0x7FC00000u)]
    [TestCase(0xFFC00000u)]
    [TestCase(0x7FC12345u)]
    [TestCase(0xFFC12345u)]
    [TestCase(0x7F800001u)]
    [TestCase(0xFF800001u)]
    [TestCase(0x7FA12345u)]
    [TestCase(0xFFA12345u)]
    public void FloatRoundTrip_PreservesEverySpecialValueBit(uint bits)
    {
        int expectedBits = unchecked((int)bits);
        var geometry = new HexMapGeometry(1, 1, new VectorXY(-4, 7), 2f, Layout.OddR);
        var map = new SpatialFloatHexMap(geometry, new[] { BitConverter.Int32BitsToSingle(expectedBits) });
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.WriteSpatialHexMap(map);
        Assert.That(BinaryPrimitives.ReadInt32LittleEndian(stream.ToArray().AsSpan(28)), Is.EqualTo(expectedBits));
        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        Assert.That(BitConverter.SingleToInt32Bits(reader.ReadSpatialFloatHexMap()[0]), Is.EqualTo(expectedBits));
    }

    [TestCase(2)]
    [TestCase(128)]
    [TestCase(255)]
    public void BooleanReader_RejectsNonCanonicalPayloadBytes(byte invalidValue)
    {
        // 1 x 1 spatial Boolean map, origin (0, 0), radius 1, then an invalid Boolean byte.
        byte[] bytes =
        {
            0x48, 0x4D, 0x41, 0x50, 1, 1, 1,
            1, 0, 0, 0, 1, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0x3F,
            invalidValue,
        };
        using var reader = new BinaryReader(new MemoryStream(bytes));

        Assert.Throws<InvalidDataException>(() => reader.ReadSpatialBoolHexMap());
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MixedRecords_PreserveAllSixMapKindsAndStreamBoundaries(bool nonSeekable)
    {
        var geometry = new HexMapGeometry(2, 1, new VectorXY(-10, 8), 1.25f, Layout.OddQ);
        var mask = new SpatialBoolHexMap(geometry, new[] { true, false });
        var costs = new SpatialIntHexMap(geometry, new[] { int.MinValue, int.MaxValue });
        int nanBits = unchecked((int)0xFFC12345);
        var heights = new SpatialFloatHexMap(geometry, new[]
        {
            BitConverter.Int32BitsToSingle(nanBits),
            BitConverter.Int32BitsToSingle(int.MinValue),
        });
        using var buffer = new MemoryStream();
        using Stream stream = nonSeekable ? new NonSeekableStream(buffer) : buffer;
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(123456);
        writer.WriteSpatialHexMap(costs);
        writer.WriteHexMap(mask);
        writer.WriteSpatialHexMap(heights);
        writer.WriteHexMap(costs);
        writer.WriteSpatialHexMap(mask);
        writer.WriteHexMap(heights);
        writer.Write(654321);
        buffer.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        Assert.That(reader.ReadInt32(), Is.EqualTo(123456));
        SpatialIntHexMap spatialInt = reader.ReadSpatialIntHexMap();
        BoolHexMap plainBool = reader.ReadBoolHexMap();
        SpatialFloatHexMap spatialFloat = reader.ReadSpatialFloatHexMap();
        IntHexMap plainInt = reader.ReadIntHexMap();
        SpatialBoolHexMap spatialBool = reader.ReadSpatialBoolHexMap();
        FloatHexMap plainFloat = reader.ReadFloatHexMap();

        Assert.Multiple(() =>
        {
            Assert.That(spatialInt.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialFloat.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialBool.Geometry, Is.EqualTo(geometry));
            Assert.That(plainBool.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(plainInt.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(plainFloat.Topology, Is.EqualTo(geometry.Topology));
            Assert.That(spatialInt[0], Is.EqualTo(int.MinValue));
            Assert.That(spatialInt[1], Is.EqualTo(int.MaxValue));
            Assert.That(plainInt[0], Is.EqualTo(int.MinValue));
            Assert.That(plainInt[1], Is.EqualTo(int.MaxValue));
            Assert.That(spatialBool[0], Is.True);
            Assert.That(spatialBool[1], Is.False);
            Assert.That(plainBool[0], Is.True);
            Assert.That(plainBool[1], Is.False);
            Assert.That(BitConverter.SingleToInt32Bits(spatialFloat[0]), Is.EqualTo(nanBits));
            Assert.That(BitConverter.SingleToInt32Bits(spatialFloat[1]), Is.EqualTo(int.MinValue));
            Assert.That(BitConverter.SingleToInt32Bits(plainFloat[0]), Is.EqualTo(nanBits));
            Assert.That(BitConverter.SingleToInt32Bits(plainFloat[1]), Is.EqualTo(int.MinValue));
            Assert.That(reader.ReadInt32(), Is.EqualTo(654321));
            Assert.That(buffer.Position, Is.EqualTo(buffer.Length));
        });
    }
}
