using System.Globalization;

using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Spatial2D.Tests.Rectangles;

public class NormalizedSubrectangleTests
{
    [Test]
    public void Constructor_WhenCornersAreNormalized_StoresMinAndMaxCorners()
    {
        var cornerA = new PointXY(0.75f, 0.5f);
        var cornerB = new PointXY(0.25f, 1f);

        var subrectangle = new NormalizedSubrectangle(cornerA, cornerB);

        Assert.That(subrectangle.Min, Is.EqualTo(new PointXY(0.25f, 0.5f)));
        Assert.That(subrectangle.Max, Is.EqualTo(new PointXY(0.75f, 1f)));
    }

    [Test]
    public void Size_ReturnsNormalizedSize()
    {
        var subrectangle = new NormalizedSubrectangle(
            new PointXY(0.25f, 0.5f),
            new PointXY(0.75f, 1f));

        Assert.That(subrectangle.Size, Is.EqualTo(new VectorXY(0.5f, 0.5f)));
    }

    [Test]
    public void CornerProperties_ReturnNormalizedCorners()
    {
        var subrectangle = new NormalizedSubrectangle(
            new PointXY(0.25f, 0.5f),
            new PointXY(0.75f, 1f));

        Assert.That(subrectangle.BottomLeft, Is.EqualTo(new PointXY(0.25f, 0.5f)));
        Assert.That(subrectangle.BottomRight, Is.EqualTo(new PointXY(0.75f, 0.5f)));
        Assert.That(subrectangle.TopLeft, Is.EqualTo(new PointXY(0.25f, 1f)));
        Assert.That(subrectangle.TopRight, Is.EqualTo(new PointXY(0.75f, 1f)));
    }

    [Test]
    public void Center_ReturnsNormalizedCenter()
    {
        var subrectangle = new NormalizedSubrectangle(
            new PointXY(0.25f, 0.5f),
            new PointXY(0.75f, 1f));

        Assert.That(subrectangle.Center, Is.EqualTo(new PointXY(0.5f, 0.75f)));
    }

    [Test]
    public void Full_ReturnsWholeNormalizedRectangle()
    {
        var full = NormalizedSubrectangle.Full;

        Assert.That(full.Min, Is.EqualTo(new PointXY(0f, 0f)));
        Assert.That(full.Max, Is.EqualTo(new PointXY(1f, 1f)));
    }

    [TestCase(-0.01f, 0.5f, "cornerA")]
    [TestCase(1.01f, 0.5f, "cornerA")]
    [TestCase(0.5f, -0.01f, "cornerA")]
    [TestCase(0.5f, 1.01f, "cornerA")]
    [TestCase(float.PositiveInfinity, 0.5f, "cornerA")]
    [TestCase(0.5f, float.NegativeInfinity, "cornerA")]
    [TestCase(-0.01f, 0.5f, "cornerB")]
    [TestCase(1.01f, 0.5f, "cornerB")]
    [TestCase(0.5f, -0.01f, "cornerB")]
    [TestCase(0.5f, 1.01f, "cornerB")]
    [TestCase(float.PositiveInfinity, 0.5f, "cornerB")]
    [TestCase(0.5f, float.NegativeInfinity, "cornerB")]
    public void Constructor_WhenCoordinateIsNotNormalized_Throws(float x, float y, string parameterName)
    {
        var point = new PointXY(x, y);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = parameterName == "cornerA"
                ? new NormalizedSubrectangle(point, new PointXY(1f, 1f))
                : new NormalizedSubrectangle(new PointXY(0f, 0f), point));

        Assert.That(exception!.ParamName, Is.EqualTo(parameterName));
    }

    [TestCase(float.NaN, 0.5f, "x")]
    [TestCase(0.5f, float.NaN, "y")]
    public void Constructor_WhenCoordinateIsNaN_ThrowsBeforeSubrectangleIsCreated(float x, float y, string parameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                var corner = new PointXY(x, y);
                _ = new NormalizedSubrectangle(corner, new PointXY(1f, 1f));
            });

        Assert.That(exception!.ParamName, Is.EqualTo(parameterName));
    }

    [TestCase(0.25f, 0.25f, true)]
    [TestCase(0.75f, 0.75f, true)]
    [TestCase(0.5f, 0.5f, true)]
    [TestCase(0.1f, 0.5f, false)]
    [TestCase(0.5f, 0.9f, false)]
    public void Contains_WhenPointIsNormalized_ReturnsWhetherPointIsInsideSubrectangle(float x, float y, bool expected)
    {
        var subrectangle = new NormalizedSubrectangle(
            new PointXY(0.25f, 0.25f),
            new PointXY(0.75f, 0.75f));

        bool contains = subrectangle.Contains(new PointXY(x, y));

        Assert.That(contains, Is.EqualTo(expected));
    }

    [Test]
    public void Contains_WhenPointIsNotNormalized_Throws()
    {
        var subrectangle = NormalizedSubrectangle.Full;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => _ = subrectangle.Contains(new PointXY(1.01f, 0.5f)));

        Assert.That(exception!.ParamName, Is.EqualTo("point"));
    }

    [Test]
    public void Deconstruct_ReturnsCorners()
    {
        var min = new PointXY(0.25f, 0.5f);
        var max = new PointXY(0.75f, 1f);
        var subrectangle = new NormalizedSubrectangle(min, max);

        var (actualMin, actualMax) = subrectangle;

        Assert.That(actualMin, Is.EqualTo(min));
        Assert.That(actualMax, Is.EqualTo(max));
    }

    [Test]
    public void Equality_WhenCornersMatch_ReturnsTrue()
    {
        var left = new NormalizedSubrectangle(new PointXY(0.25f, 0.5f), new PointXY(0.75f, 1f));
        var right = new NormalizedSubrectangle(new PointXY(0.25f, 0.5f), new PointXY(0.75f, 1f));

        Assert.That(left.Equals(right), Is.True);
        Assert.That(left == right, Is.True);
        Assert.That(left != right, Is.False);
    }

    [Test]
    public void ToString_UsesInvariantCulture()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        try
        {
            var subrectangle = new NormalizedSubrectangle(
                new PointXY(0.25f, 0.5f),
                new PointXY(0.75f, 1f));

            Assert.That(subrectangle.ToString(), Is.EqualTo("[(0.25, 0.5), (0.75, 1)]"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
