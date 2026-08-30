using Akeldov.Math.Spatial2D.Fields;

namespace Akeldov.Math.Spatial2D.Tests.Fields;

public class FieldRangeTests
{
    [Test]
    public void FloatFieldRange_Constructor_StoresFields()
    {
        var minField = new TestFloatField(-2f);
        var maxField = new TestFloatField(3f);

        var range = new FloatFieldRange(minField, maxField);
        var (deconstructedMinField, deconstructedMaxField) = range;

        Assert.Multiple(() =>
        {
            Assert.That(range.MinField, Is.SameAs(minField));
            Assert.That(range.MaxField, Is.SameAs(maxField));
            Assert.That(deconstructedMinField, Is.SameAs(minField));
            Assert.That(deconstructedMaxField, Is.SameAs(maxField));
        });
    }

    [Test]
    public void FloatFieldRange_Constructor_WhenFieldIsNull_Throws()
    {
        var field = new TestFloatField(0f);

        var minException = Assert.Throws<ArgumentNullException>(() => new FloatFieldRange(null!, field));
        var maxException = Assert.Throws<ArgumentNullException>(() => new FloatFieldRange(field, null!));

        Assert.Multiple(() =>
        {
            Assert.That(minException!.ParamName, Is.EqualTo("minField"));
            Assert.That(maxException!.ParamName, Is.EqualTo("maxField"));
        });
    }

    [Test]
    public void IntFieldRange_Constructor_StoresFields()
    {
        var minField = new TestIntField(-2);
        var maxField = new TestIntField(3);

        var range = new IntFieldRange(minField, maxField);
        var (deconstructedMinField, deconstructedMaxField) = range;

        Assert.Multiple(() =>
        {
            Assert.That(range.MinField, Is.SameAs(minField));
            Assert.That(range.MaxField, Is.SameAs(maxField));
            Assert.That(deconstructedMinField, Is.SameAs(minField));
            Assert.That(deconstructedMaxField, Is.SameAs(maxField));
        });
    }

    [Test]
    public void IntFieldRange_Constructor_WhenFieldIsNull_Throws()
    {
        var field = new TestIntField(0);

        var minException = Assert.Throws<ArgumentNullException>(() => new IntFieldRange(null!, field));
        var maxException = Assert.Throws<ArgumentNullException>(() => new IntFieldRange(field, null!));

        Assert.Multiple(() =>
        {
            Assert.That(minException!.ParamName, Is.EqualTo("minField"));
            Assert.That(maxException!.ParamName, Is.EqualTo("maxField"));
        });
    }

    private sealed class TestFloatField : IFloatField
    {
        public TestFloatField(float value)
        {
            Min = value;
            Max = value;
        }

        public float Min { get; }

        public float Max { get; }

        public float Sample(PointXY point) => Min;
    }

    private sealed class TestIntField : IIntField
    {
        public TestIntField(int value)
        {
            Min = value;
            Max = value;
        }

        public int Min { get; }

        public int Max { get; }

        public int Sample(PointXY point) => Min;
    }
}
