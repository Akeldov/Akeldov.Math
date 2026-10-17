using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class BoolRasterTests
{
    private static IEnumerable<TestCaseData> BinaryOperators
    {
        get
        {
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left & right),
                new[] { false, false, false, true }).SetArgDisplayNames("& specialized");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => (Raster<bool>)left & right),
                new[] { false, false, false, true }).SetArgDisplayNames("& base-left");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left & (Raster<bool>)right),
                new[] { false, false, false, true }).SetArgDisplayNames("& base-right");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left | right),
                new[] { false, true, true, true }).SetArgDisplayNames("| specialized");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => (Raster<bool>)left | right),
                new[] { false, true, true, true }).SetArgDisplayNames("| base-left");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left | (Raster<bool>)right),
                new[] { false, true, true, true }).SetArgDisplayNames("| base-right");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left ^ right),
                new[] { false, true, true, false }).SetArgDisplayNames("^ specialized");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => (Raster<bool>)left ^ right),
                new[] { false, true, true, false }).SetArgDisplayNames("^ base-left");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left ^ (Raster<bool>)right),
                new[] { false, true, true, false }).SetArgDisplayNames("^ base-right");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left == right),
                new[] { true, false, false, true }).SetArgDisplayNames("== specialized");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => (Raster<bool>)left == right),
                new[] { true, false, false, true }).SetArgDisplayNames("== base-left");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left == (Raster<bool>)right),
                new[] { true, false, false, true }).SetArgDisplayNames("== base-right");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left != right),
                new[] { false, true, true, false }).SetArgDisplayNames("!= specialized");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => (Raster<bool>)left != right),
                new[] { false, true, true, false }).SetArgDisplayNames("!= base-left");
            yield return new TestCaseData(
                new Func<BoolRaster, BoolRaster, BoolRaster>((left, right) => left != (Raster<bool>)right),
                new[] { false, true, true, false }).SetArgDisplayNames("!= base-right");
        }
    }

    [Test]
    public void Constructor_RetainsArrayAndSupportsRowMajorIndexers()
    {
        var values = new[] { false, true, false, true, false, true };
        var resolution = new VectorXYInt(3, 2);
        var raster = new BoolRaster(resolution, values);
        Raster<bool> baseRaster = raster;
        IRaster<bool> view = raster;

        Assert.That(raster.Resolution, Is.EqualTo(resolution));
        Assert.That(raster.Values, Is.SameAs(values));
        Assert.That(view[new VectorXYInt(0, 1)], Is.True);
        values[0] = true;
        Assert.That(raster[0, 0], Is.True);
        raster[new VectorXYInt(1, 1)] = true;
        Assert.That(values[4], Is.True);
        baseRaster[5] = false;
        Assert.That(raster[2, 1], Is.False);
    }

    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(-1, 1)]
    [TestCase(1, -1)]
    [TestCase(int.MaxValue, 2)]
    public void Constructor_WhenResolutionIsInvalid_Throws(int width, int height)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BoolRaster(new VectorXYInt(width, height), Array.Empty<bool>()));

        Assert.That(exception!.ParamName, Is.EqualTo("resolution"));
    }

    [Test]
    public void Constructor_WhenValuesAreNullOrHaveWrongLength_Throws()
    {
        var resolution = new VectorXYInt(2, 2);

        Assert.That(Assert.Throws<ArgumentNullException>(() => new BoolRaster(resolution, null!))!.ParamName,
            Is.EqualTo("values"));
        Assert.That(Assert.Throws<ArgumentException>(() => new BoolRaster(resolution, new bool[3]))!.ParamName,
            Is.EqualTo("values"));
        Assert.That(Assert.Throws<ArgumentException>(() => new BoolRaster(resolution, new bool[5]))!.ParamName,
            Is.EqualTo("values"));
    }

    [TestCaseSource(nameof(BinaryOperators))]
    public void BinaryOperators_ApplyTruthTableAndReturnIndependentStorage(
        Func<BoolRaster, BoolRaster, BoolRaster> operation, bool[] expected)
    {
        var resolution = new VectorXYInt(2, 2);
        var left = new BoolRaster(resolution, new[] { false, false, true, true });
        var right = new BoolRaster(resolution, new[] { false, true, false, true });

        var result = operation(left, right);

        Assert.That(result.Resolution, Is.EqualTo(resolution));
        Assert.That(result.Values, Is.EqualTo(expected));
        Assert.That(result.Values, Is.Not.SameAs(left.Values));
        Assert.That(result.Values, Is.Not.SameAs(right.Values));
        result[0] = !result[0];
        Assert.That(left.Values, Is.EqualTo(new[] { false, false, true, true }));
        Assert.That(right.Values, Is.EqualTo(new[] { false, true, false, true }));
    }

    [TestCaseSource(nameof(BinaryOperators))]
    public void BinaryOperators_WhenResolutionDiffers_Throw(
        Func<BoolRaster, BoolRaster, BoolRaster> operation, bool[] expected)
    {
        var left = new BoolRaster(new VectorXYInt(2, 2), new bool[4]);
        // Equal lengths do not imply equal raster shapes.
        var right = new BoolRaster(new VectorXYInt(4, 1), new bool[4]);

        var exception = Assert.Throws<ArgumentException>(() => operation(left, right));

        Assert.That(exception!.ParamName, Is.EqualTo("right"));
    }

    [TestCaseSource(nameof(BinaryOperators))]
    public void BinaryOperators_WhenEitherRasterIsNull_Throw(
        Func<BoolRaster, BoolRaster, BoolRaster> operation, bool[] expected)
    {
        var raster = new BoolRaster(new VectorXYInt(1, 1), new bool[1]);

        Assert.That(Assert.Throws<ArgumentNullException>(() => operation(null!, raster))!.ParamName,
            Is.EqualTo("left"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => operation(raster, null!))!.ParamName,
            Is.EqualTo("right"));
    }

    [Test]
    public void Negation_CreatesIndependentInvertedRaster()
    {
        var raster = new BoolRaster(new VectorXYInt(2, 1), new[] { false, true });

        BoolRaster result = !raster;

        Assert.That(result.Resolution, Is.EqualTo(raster.Resolution));
        Assert.That(result.Values, Is.EqualTo(new[] { true, false }));
        Assert.That(result.Values, Is.Not.SameAs(raster.Values));
        result[1] = true;
        Assert.That(raster.Values, Is.EqualTo(new[] { false, true }));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = !(BoolRaster)null!)!.ParamName,
            Is.EqualTo("raster"));
    }

    [TestCase(false, false, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, false)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public void ScalarEqualityOperators_ReturnIndependentMasks(bool value, bool unequal, bool scalarFirst)
    {
        var raster = new BoolRaster(new VectorXYInt(2, 1), new[] { false, true });

        BoolRaster result = unequal
            ? (scalarFirst ? value != raster : raster != value)
            : (scalarFirst ? value == raster : raster == value);

        bool[] expected = value == unequal ? new[] { true, false } : new[] { false, true };
        Assert.That(result.Resolution, Is.EqualTo(raster.Resolution));
        Assert.That(result.Values, Is.EqualTo(expected));
        Assert.That(result.Values, Is.Not.SameAs(raster.Values));
        result[0] = !result[0];
        Assert.That(raster.Values, Is.EqualTo(new[] { false, true }));
    }

    [Test]
    public void ScalarEqualityOperators_WhenRasterIsNull_Throw()
    {
        BoolRaster raster = null!;

        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster == true)!.ParamName, Is.EqualTo("raster"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = true == raster)!.ParamName, Is.EqualTo("raster"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster != false)!.ParamName, Is.EqualTo("raster"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = false != raster)!.ParamName, Is.EqualTo("raster"));
    }
}

