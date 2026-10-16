using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class IntRasterTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void BinaryOperators_WhenLaterCellOverflows_ThrowAndPreserveSources(bool subtract)
    {
        int boundary = subtract ? int.MinValue : int.MaxValue;
        var left = new IntRaster(new VectorXYInt(2, 1), new[] { 5, boundary });
        var right = new IntRaster(left.Resolution, new[] { 1, 1 });
        Raster<int> baseLeft = left;
        Raster<int> baseRight = right;

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = subtract ? left - right : left + right);
            Assert.Throws<OverflowException>(() => _ = subtract ? baseLeft - right : baseLeft + right);
            Assert.Throws<OverflowException>(() => _ = subtract ? left - baseRight : left + baseRight);
            Assert.That(left.Values, Is.EqualTo(new[] { 5, boundary }));
            Assert.That(right.Values, Is.EqualTo(new[] { 1, 1 }));
        });
    }

    [Test]
    public void ScalarOperators_WhenLaterCellOverflows_ThrowAndPreserveSources()
    {
        var maximum = new IntRaster(new VectorXYInt(2, 1), new[] { 5, int.MaxValue });
        var minimum = new IntRaster(maximum.Resolution, new[] { 5, int.MinValue });

        Assert.Multiple(() =>
        {
            Assert.Throws<OverflowException>(() => _ = maximum + 1);
            Assert.Throws<OverflowException>(() => _ = 1 + maximum);
            Assert.Throws<OverflowException>(() => _ = minimum - 1);
            Assert.Throws<OverflowException>(() => _ = 1 - minimum);
            Assert.That(maximum.Values, Is.EqualTo(new[] { 5, int.MaxValue }));
            Assert.That(minimum.Values, Is.EqualTo(new[] { 5, int.MinValue }));
        });
    }

    [Test]
    public void UnaryMinus_WhenLaterCellIsIntMinValue_ThrowsAndPreservesSource()
    {
        var raster = new IntRaster(new VectorXYInt(2, 1), new[] { 5, int.MinValue });

        Assert.Throws<OverflowException>(() => _ = -raster);

        Assert.That(raster.Values, Is.EqualTo(new[] { 5, int.MinValue }));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void BinaryOperators_CreateIndependentResult(bool subtract)
    {
        var resolution = new VectorXYInt(2, 2);
        var left = new IntRaster(resolution, new[] { 3, -4, 0, 8 });
        var right = new IntRaster(resolution, new[] { 1, 6, -2, 8 });

        IntRaster result = subtract ? left - right : left + right;

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(resolution));
            Assert.That(result.Values, Is.EqualTo(subtract
                ? new[] { 2, -10, 2, 0 }
                : new[] { 4, 2, -2, 16 }));
            Assert.That(result.Values, Is.Not.SameAs(left.Values));
            Assert.That(result.Values, Is.Not.SameAs(right.Values));
            Assert.That(left.Values, Is.EqualTo(new[] { 3, -4, 0, 8 }));
            Assert.That(right.Values, Is.EqualTo(new[] { 1, 6, -2, 8 }));
        });
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void MixedBinaryOperators_PreserveOperandOrderAndSources(bool subtract, bool baseFirst)
    {
        var resolution = new VectorXYInt(2, 2);
        var raster = new IntRaster(resolution, new[] { 3, -4, 0, 8 });
        var baseRaster = new Raster<int>(resolution, new[] { 1, 6, -2, 8 });

        IntRaster result = subtract
            ? (baseFirst ? baseRaster - raster : raster - baseRaster)
            : (baseFirst ? baseRaster + raster : raster + baseRaster);

        int[] expected = subtract
            ? (baseFirst ? new[] { -2, 10, -2, 0 } : new[] { 2, -10, 2, 0 })
            : new[] { 4, 2, -2, 16 };

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(resolution));
            Assert.That(result.Values, Is.EqualTo(expected));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(result.Values, Is.Not.SameAs(baseRaster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4, 0, 8 }));
            Assert.That(baseRaster.Values, Is.EqualTo(new[] { 1, 6, -2, 8 }));
        });
    }

    [TestCase(false, false, 7, 0)]
    [TestCase(false, true, 7, 0)]
    [TestCase(true, false, -1, -8)]
    [TestCase(true, true, 1, 8)]
    public void ScalarOperators_PreserveOperandOrderAndSource(
        bool subtract, bool scalarFirst, int first, int second)
    {
        var raster = new IntRaster(new VectorXYInt(2, 1), new[] { 3, -4 });

        IntRaster result = subtract
            ? (scalarFirst ? 4 - raster : raster - 4)
            : (scalarFirst ? 4 + raster : raster + 4);

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(raster.Resolution));
            Assert.That(result.Values, Is.EqualTo(new[] { first, second }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4 }));
        });
    }

    [Test]
    public void UnaryMinus_CreatesIndependentNegatedRaster()
    {
        var raster = new IntRaster(new VectorXYInt(2, 1), new[] { 3, -4 });

        IntRaster result = -raster;

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(raster.Resolution));
            Assert.That(result.Values, Is.EqualTo(new[] { -3, 4 }));
            Assert.That(result.Values, Is.Not.SameAs(raster.Values));
            Assert.That(raster.Values, Is.EqualTo(new[] { 3, -4 }));
        });
    }

    [TestCase(1, 4)]
    [TestCase(4, 1)]
    [TestCase(1, 1)]
    public void BinaryOperators_WhenResolutionsDiffer_Throw(int width, int height)
    {
        var left = new IntRaster(new VectorXYInt(2, 2), new int[4]);
        var right = new IntRaster(new VectorXYInt(width, height), new int[width * height]);

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (Raster<int>)left + right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left + (Raster<int>)right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = (Raster<int>)left - right)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentException>(() => _ = left - (Raster<int>)right)!.ParamName, Is.EqualTo("right"));
        });
    }

    [Test]
    public void Operators_WhenRasterIsNull_Throw()
    {
        var raster = new IntRaster(new VectorXYInt(1, 1), new int[1]);
        IntRaster missing = null!;
        var baseRaster = new Raster<int>(raster.Resolution, new int[1]);
        Raster<int> missingBase = null!;

        Assert.Multiple(() =>
        {
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster + missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster - missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + 4)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = 4 + missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - 4)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = 4 - missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = -missing)!.ParamName, Is.EqualTo("raster"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing + baseRaster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = baseRaster + missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingBase + raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster + missingBase)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missing - baseRaster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = baseRaster - missing)!.ParamName, Is.EqualTo("right"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = missingBase - raster)!.ParamName, Is.EqualTo("left"));
            Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster - missingBase)!.ParamName, Is.EqualTo("right"));
        });
    }
}
