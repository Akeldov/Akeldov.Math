using Akeldov.Math.Hexes.Vectors.QRS;
using System.IO;

namespace Akeldov.Math.Hexes.Tests.VectorsQRS;

public class SerializationTests
{
    [Test]
    public void BinaryWriterAndReader_RoundTripQRSValues()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(new VectorQRSInt(2, -7));
            writer.Write(new VectorQRS(1.5f, -2.25f));
            writer.Write(SixfoldAngle.Deg240);
        }

        stream.Position = 0;

        using var reader = new BinaryReader(stream);
        var intVector = reader.ReadVectorQRSInt();
        var floatVector = reader.ReadVectorQRS();
        var angle = reader.ReadSixfoldAngle();

        Assert.Multiple(() =>
        {
            Assert.That(intVector, Is.EqualTo(new VectorQRSInt(2, -7)));
            Assert.That(floatVector, Is.EqualTo(new VectorQRS(1.5f, -2.25f)));
            Assert.That(angle, Is.EqualTo(SixfoldAngle.Deg240));
        });
    }

    [Test]
    public void BinaryWriter_WhenNull_ThrowsArgumentNullException()
    {
        BinaryWriter? writer = null;

        Assert.Multiple(() =>
        {
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => writer!.Write(new VectorQRSInt(2, -7)))!.ParamName,
                Is.EqualTo("writer"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => writer!.Write(new VectorQRS(1.5f, -2.25f)))!.ParamName,
                Is.EqualTo("writer"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => writer!.Write(SixfoldAngle.Deg240))!.ParamName,
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
                Assert.Throws<ArgumentNullException>(() => reader!.ReadVectorQRSInt())!.ParamName,
                Is.EqualTo("reader"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => reader!.ReadVectorQRS())!.ParamName,
                Is.EqualTo("reader"));
            Assert.That(
                Assert.Throws<ArgumentNullException>(() => reader!.ReadSixfoldAngle())!.ParamName,
                Is.EqualTo("reader"));
        });
    }
}
