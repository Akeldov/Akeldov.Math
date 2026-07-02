using System.IO;

namespace Akeldov.Math.Spatial2D.Tests.Vectors;

public class SerializationTests
{
    [Test]
    public void BinaryWriterAndReader_RoundTripXYValues()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(new VectorXYInt(2, -7));
            writer.Write(new VectorXY(1.5f, -2.25f));
        }

        stream.Position = 0;

        using var reader = new BinaryReader(stream);
        var intVector = reader.ReadVectorXYInt();
        var floatVector = reader.ReadVectorXY();

        Assert.Multiple(() =>
        {
            Assert.That(intVector, Is.EqualTo(new VectorXYInt(2, -7)));
            Assert.That(floatVector, Is.EqualTo(new VectorXY(1.5f, -2.25f)));
        });
    }

    [Test]
    public void BinaryWriter_WhenNull_ThrowsArgumentNullException()
    {
        BinaryWriter? writer = null;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => writer!.Write(new VectorXYInt(2, -7)))!.ParamName,
                Is.EqualTo("writer"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => writer!.Write(new VectorXY(1.5f, -2.25f)))!.ParamName,
                Is.EqualTo("writer"));
        });
    }

    [Test]
    public void BinaryReader_WhenNull_ThrowsArgumentNullException()
    {
        BinaryReader? reader = null;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => reader!.ReadVectorXYInt())!.ParamName,
                Is.EqualTo("reader"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => reader!.ReadVectorXY())!.ParamName,
                Is.EqualTo("reader"));
        });
    }
}
