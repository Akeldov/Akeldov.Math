using System.Globalization;

namespace Akeldov.Math.Spatial3D.Tests.Points;

public class PointXYZTests
{
    private const float Tolerance = 1e-6f;

    [Test]
    public void Constructor_WhenCoordinatesAreValid_StoresCoordinates()
    {
        var point = new PointXYZ(1f, -2f, 3f);

        Assert.Multiple(() =>
        {
            Assert.That(point.X, Is.EqualTo(1f));
            Assert.That(point.Y, Is.EqualTo(-2f));
            Assert.That(point.Z, Is.EqualTo(3f));
        });
    }

    [Test]
    public void Constructor_WhenCoordinatesAreInfinite_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new PointXYZ(float.PositiveInfinity, 0f, 0f));
        Assert.DoesNotThrow(() => new PointXYZ(0f, float.NegativeInfinity, 0f));
        Assert.DoesNotThrow(() => new PointXYZ(0f, 0f, float.PositiveInfinity));
    }

    [TestCase(float.NaN, 0f, 0f, "x")]
    [TestCase(0f, float.NaN, 0f, "y")]
    [TestCase(0f, 0f, float.NaN, "z")]
    public void Constructor_WhenCoordinateIsNaN_Throws(float x, float y, float z, string paramName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new PointXYZ(x, y, z));

        Assert.That(exception!.ParamName, Is.EqualTo(paramName));
    }

    [Test]
    public void Distance_ReturnsEuclideanDistance()
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(3f, 5f, 9f);

        Assert.That(source.Distance(target), Is.EqualTo(7f).Within(Tolerance));
    }

    [Test]
    public void Subtract_WhenTwoPointsAreUsed_ReturnsVectorFromRightToLeft()
    {
        var source = new PointXYZ(1f, 2f, 3f);
        var target = new PointXYZ(4f, 6f, 8f);

        VectorXYZ vector = target - source;

        Assert.That(vector, Is.EqualTo(new VectorXYZ(3f, 4f, 5f)));
    }

    [Test]
    public void Add_WhenPointAndVectorAreUsed_ReturnsTranslatedPoint()
    {
        var point = new PointXYZ(1f, 2f, 3f);
        var vector = new VectorXYZ(3f, 4f, 5f);

        Assert.Multiple(() =>
        {
            Assert.That(point + vector, Is.EqualTo(new PointXYZ(4f, 6f, 8f)));
            Assert.That(vector + point, Is.EqualTo(new PointXYZ(4f, 6f, 8f)));
        });
    }

    [Test]
    public void Subtract_WhenPointAndVectorAreUsed_ReturnsTranslatedPoint()
    {
        var point = new PointXYZ(4f, 6f, 8f);
        var vector = new VectorXYZ(3f, 4f, 5f);

        Assert.That(point - vector, Is.EqualTo(new PointXYZ(1f, 2f, 3f)));
    }

    [Test]
    public void ExplicitConversions_ConvertBetweenPointAndCoordinateVector()
    {
        var point = new PointXYZ(1f, 2f, 3f);
        var vector = new VectorXYZ(4f, 5f, 6f);

        Assert.Multiple(() =>
        {
            Assert.That((VectorXYZ)point, Is.EqualTo(new VectorXYZ(1f, 2f, 3f)));
            Assert.That((PointXYZ)vector, Is.EqualTo(new PointXYZ(4f, 5f, 6f)));
        });
    }

    [Test]
    public void Equality_UsesExactCoordinateEquality()
    {
        var point = new PointXYZ(1f, 2f, 3f);
        var equal = new PointXYZ(1f, 2f, 3f);
        var different = new PointXYZ(1f, 2f, 4f);

        Assert.Multiple(() =>
        {
            Assert.That(point.Equals(equal), Is.True);
            Assert.That(point == equal, Is.True);
            Assert.That(point != different, Is.True);
            Assert.That(point.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
        });
    }

    [Test]
    public void Deconstruct_ReturnsCoordinates()
    {
        var (x, y, z) = new PointXYZ(1f, 2f, 3f);

        Assert.Multiple(() =>
        {
            Assert.That(x, Is.EqualTo(1f));
            Assert.That(y, Is.EqualTo(2f));
            Assert.That(z, Is.EqualTo(3f));
        });
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            Assert.That(new PointXYZ(1.5f, 2.25f, -3.75f).ToString(), Is.EqualTo("(1.5, 2.25, -3.75)"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
