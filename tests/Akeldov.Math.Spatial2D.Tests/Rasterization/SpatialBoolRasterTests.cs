using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Spatial2D.Tests.Rasterization;

public class SpatialBoolRasterTests
{
    private static readonly RasterGeometry Geometry =
        new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(2, 2));

    private static IEnumerable<TestCaseData> SpatialOperators
    {
        get
        {
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left & right),
                new[] { false, false, false, true }).SetArgDisplayNames("& specialized");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => (SpatialRaster<bool>)left & right),
                new[] { false, false, false, true }).SetArgDisplayNames("& generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left & (SpatialRaster<bool>)right),
                new[] { false, false, false, true }).SetArgDisplayNames("& generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left | right),
                new[] { false, true, true, true }).SetArgDisplayNames("| specialized");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => (SpatialRaster<bool>)left | right),
                new[] { false, true, true, true }).SetArgDisplayNames("| generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left | (SpatialRaster<bool>)right),
                new[] { false, true, true, true }).SetArgDisplayNames("| generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left ^ right),
                new[] { false, true, true, false }).SetArgDisplayNames("^ specialized");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => (SpatialRaster<bool>)left ^ right),
                new[] { false, true, true, false }).SetArgDisplayNames("^ generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left ^ (SpatialRaster<bool>)right),
                new[] { false, true, true, false }).SetArgDisplayNames("^ generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left == right),
                new[] { true, false, false, true }).SetArgDisplayNames("== specialized");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => (SpatialRaster<bool>)left == right),
                new[] { true, false, false, true }).SetArgDisplayNames("== generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left == (SpatialRaster<bool>)right),
                new[] { true, false, false, true }).SetArgDisplayNames("== generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left != right),
                new[] { false, true, true, false }).SetArgDisplayNames("!= specialized");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => (SpatialRaster<bool>)left != right),
                new[] { false, true, true, false }).SetArgDisplayNames("!= generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster>((left, right) => left != (SpatialRaster<bool>)right),
                new[] { false, true, true, false }).SetArgDisplayNames("!= generic-right");
        }
    }

    private static IEnumerable<TestCaseData> NonSpatialOperators
    {
        get
        {
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial & plain),
                new[] { false, false, false, true }, false).SetArgDisplayNames("& specialized-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => plain & spatial),
                new[] { false, false, false, true }, true).SetArgDisplayNames("& specialized-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial & (Raster<bool>)plain),
                new[] { false, false, false, true }, false).SetArgDisplayNames("& generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => (Raster<bool>)plain & spatial),
                new[] { false, false, false, true }, true).SetArgDisplayNames("& generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial | plain),
                new[] { false, true, true, true }, false).SetArgDisplayNames("| specialized-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => plain | spatial),
                new[] { false, true, true, true }, true).SetArgDisplayNames("| specialized-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial | (Raster<bool>)plain),
                new[] { false, true, true, true }, false).SetArgDisplayNames("| generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => (Raster<bool>)plain | spatial),
                new[] { false, true, true, true }, true).SetArgDisplayNames("| generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial ^ plain),
                new[] { false, true, true, false }, false).SetArgDisplayNames("^ specialized-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => plain ^ spatial),
                new[] { false, true, true, false }, true).SetArgDisplayNames("^ specialized-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial ^ (Raster<bool>)plain),
                new[] { false, true, true, false }, false).SetArgDisplayNames("^ generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => (Raster<bool>)plain ^ spatial),
                new[] { false, true, true, false }, true).SetArgDisplayNames("^ generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial == plain),
                new[] { true, false, false, true }, false).SetArgDisplayNames("== specialized-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => plain == spatial),
                new[] { true, false, false, true }, true).SetArgDisplayNames("== specialized-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial == (Raster<bool>)plain),
                new[] { true, false, false, true }, false).SetArgDisplayNames("== generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => (Raster<bool>)plain == spatial),
                new[] { true, false, false, true }, true).SetArgDisplayNames("== generic-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial != plain),
                new[] { false, true, true, false }, false).SetArgDisplayNames("!= specialized-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => plain != spatial),
                new[] { false, true, true, false }, true).SetArgDisplayNames("!= specialized-left");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => spatial != (Raster<bool>)plain),
                new[] { false, true, true, false }, false).SetArgDisplayNames("!= generic-right");
            yield return new TestCaseData(
                new Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster>((spatial, plain) => (Raster<bool>)plain != spatial),
                new[] { false, true, true, false }, true).SetArgDisplayNames("!= generic-left");
        }
    }

    [Test]
    public void Constructor_RetainsArrayAndImplementsSpatialContract()
    {
        var values = new[] { false, true, false, true };
        var raster = new SpatialBoolRaster(Geometry, values);
        BoolRaster plain = raster;
        ISpatialRaster<bool> spatial = raster;

        plain[0] = true;
        values[1] = false;

        Assert.That(raster.Values, Is.SameAs(values));
        Assert.That(spatial.Geometry, Is.EqualTo(Geometry));
        Assert.That(spatial.Resolution, Is.EqualTo(Geometry.Resolution));
        Assert.That(spatial[0], Is.True);
        Assert.That(spatial[1], Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Constructor_WhenGeometryIsInvalidOrTooLarge_Throws(bool tooLarge)
    {
        var geometry = tooLarge
            ? new RasterGeometry(new PointXY(0, 0), VectorXY.One, new VectorXYInt(50_000, 50_000))
            : default;

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpatialBoolRaster(geometry, Array.Empty<bool>()))!.ParamName, Is.EqualTo("geometry"));
    }

    [Test]
    public void Constructor_WhenValuesAreNullOrHaveWrongLength_Throws()
    {
        Assert.That(Assert.Throws<ArgumentNullException>(() => new SpatialBoolRaster(Geometry, null!))!.ParamName,
            Is.EqualTo("values"));
        Assert.That(Assert.Throws<ArgumentException>(() => new SpatialBoolRaster(Geometry, new bool[3]))!.ParamName,
            Is.EqualTo("values"));
    }

    [Test]
    public void ImplicitConversion_SharesValuesAndPreservesGeometry()
    {
        var raster = new SpatialBoolRaster(Geometry, new bool[4]);

        SpatialRaster<bool> converted = raster;
        converted[0] = true;
        raster[1] = true;

        Assert.That(converted.Geometry, Is.EqualTo(Geometry));
        Assert.That(converted.Values, Is.SameAs(raster.Values));
        Assert.That(raster[0], Is.True);
        Assert.That(converted[1], Is.True);
        SpatialBoolRaster? missing = null;
        SpatialRaster<bool>? convertedNull = missing;
        Assert.That(convertedNull, Is.Null);
    }

    [TestCaseSource(nameof(SpatialOperators))]
    public void SpatialOperators_ApplyTruthTablesAndPreserveGeometry(
        Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster> operation, bool[] expected)
    {
        var left = new SpatialBoolRaster(Geometry, new[] { false, false, true, true });
        var right = new SpatialBoolRaster(Geometry, new[] { false, true, false, true });

        var result = operation(left, right);

        Assert.That(result.Geometry, Is.EqualTo(Geometry));
        Assert.That(result.Values, Is.EqualTo(expected));
        Assert.That(result.Values, Is.Not.SameAs(left.Values));
        Assert.That(result.Values, Is.Not.SameAs(right.Values));
        result[0] = !result[0];
        Assert.That(left.Values, Is.EqualTo(new[] { false, false, true, true }));
        Assert.That(right.Values, Is.EqualTo(new[] { false, true, false, true }));
    }

    [TestCaseSource(nameof(SpatialOperators))]
    public void SpatialOperators_WhenGeometriesDiffer_Throw(
        Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster> operation, bool[] expected)
    {
        var left = new SpatialBoolRaster(Geometry, new bool[4]);
        var differentGeometries = new[]
        {
            new RasterGeometry(new PointXY(8, -10), new VectorXY(16, 8), new VectorXYInt(2, 2)),
            new RasterGeometry(new PointXY(6, -8), new VectorXY(16, 8), new VectorXYInt(2, 2)),
            new RasterGeometry(new PointXY(6, -10), new VectorXY(8, 8), new VectorXYInt(2, 2)),
            new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 16), new VectorXYInt(2, 2)),
            new RasterGeometry(new PointXY(6, -10), new VectorXY(16, 8), new VectorXYInt(1, 4))
        };

        foreach (var geometry in differentGeometries)
        {
            var right = new SpatialBoolRaster(geometry, new bool[4]);
            Assert.That(Assert.Throws<ArgumentException>(() => operation(left, right))!.ParamName,
                Is.EqualTo("right"), $"Geometry: {geometry}");
        }
    }

    [TestCaseSource(nameof(SpatialOperators))]
    public void SpatialOperators_WhenEitherOperandIsNull_Throw(
        Func<SpatialBoolRaster, SpatialBoolRaster, SpatialBoolRaster> operation, bool[] expected)
    {
        var raster = new SpatialBoolRaster(Geometry, new bool[4]);

        Assert.That(Assert.Throws<ArgumentNullException>(() => operation(null!, raster))!.ParamName, Is.EqualTo("left"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => operation(raster, null!))!.ParamName, Is.EqualTo("right"));
    }

    [TestCaseSource(nameof(NonSpatialOperators))]
    public void NonSpatialOperators_ApplyTruthTablesAndPreserveSpatialGeometry(
        Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster> operation, bool[] expected, bool plainFirst)
    {
        var spatial = new SpatialBoolRaster(Geometry, new[] { false, false, true, true });
        var plain = new BoolRaster(Geometry.Resolution, new[] { false, true, false, true });

        var result = operation(spatial, plain);

        Assert.That(result.Geometry, Is.EqualTo(Geometry));
        Assert.That(result.Values, Is.EqualTo(expected));
        Assert.That(result.Values, Is.Not.SameAs(spatial.Values));
        Assert.That(result.Values, Is.Not.SameAs(plain.Values));
        result[0] = !result[0];
        Assert.That(spatial.Values, Is.EqualTo(new[] { false, false, true, true }));
        Assert.That(plain.Values, Is.EqualTo(new[] { false, true, false, true }));
    }

    [TestCaseSource(nameof(NonSpatialOperators))]
    public void NonSpatialOperators_WhenResolutionsDiffer_Throw(
        Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster> operation, bool[] expected, bool plainFirst)
    {
        var spatial = new SpatialBoolRaster(Geometry, new bool[4]);
        var plain = new BoolRaster(new VectorXYInt(1, 4), new bool[4]);

        Assert.That(Assert.Throws<ArgumentException>(() => operation(spatial, plain))!.ParamName, Is.EqualTo("right"));
    }

    [TestCaseSource(nameof(NonSpatialOperators))]
    public void NonSpatialOperators_WhenEitherOperandIsNull_Throw(
        Func<SpatialBoolRaster, BoolRaster, SpatialBoolRaster> operation, bool[] expected, bool plainFirst)
    {
        var spatial = new SpatialBoolRaster(Geometry, new bool[4]);
        var plain = new BoolRaster(Geometry.Resolution, new bool[4]);

        Assert.That(Assert.Throws<ArgumentNullException>(() => operation(null!, plain))!.ParamName,
            Is.EqualTo(plainFirst ? "right" : "left"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => operation(spatial, null!))!.ParamName,
            Is.EqualTo(plainFirst ? "left" : "right"));
    }

    [Test]
    public void Negation_PreservesGeometryAndCreatesIndependentValues()
    {
        var raster = new SpatialBoolRaster(Geometry, new[] { false, true, true, false });

        SpatialBoolRaster result = !raster;

        Assert.That(result.Geometry, Is.EqualTo(Geometry));
        Assert.That(result.Values, Is.EqualTo(new[] { true, false, false, true }));
        Assert.That(result.Values, Is.Not.SameAs(raster.Values));
        Assert.That(raster.Values, Is.EqualTo(new[] { false, true, true, false }));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = !(SpatialBoolRaster)null!)!.ParamName,
            Is.EqualTo("raster"));
    }

    [Test]
    public void ScalarEqualityOperators_PreserveGeometryAndCreateIndependentMasks(
        [Values] bool value, [Values] bool unequal, [Values] bool scalarFirst)
    {
        var raster = new SpatialBoolRaster(Geometry, new[] { false, true, false, true });

        SpatialBoolRaster result = unequal
            ? (scalarFirst ? value != raster : raster != value)
            : (scalarFirst ? value == raster : raster == value);

        bool[] expected = value == unequal ? new[] { true, false, true, false } : new[] { false, true, false, true };
        Assert.That(result.Geometry, Is.EqualTo(Geometry));
        Assert.That(result.Values, Is.EqualTo(expected));
        Assert.That(result.Values, Is.Not.SameAs(raster.Values));
        result[0] = !result[0];
        Assert.That(raster.Values, Is.EqualTo(new[] { false, true, false, true }));
    }

    [Test]
    public void ScalarEqualityOperators_WhenRasterIsNull_Throw()
    {
        SpatialBoolRaster raster = null!;

        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster == true)!.ParamName, Is.EqualTo("raster"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = true == raster)!.ParamName, Is.EqualTo("raster"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = raster != false)!.ParamName, Is.EqualTo("raster"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => _ = false != raster)!.ParamName, Is.EqualTo("raster"));
    }
}
