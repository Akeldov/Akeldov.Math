using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.IO;

namespace Akeldov.Math.Hexes.Tests.Maps;

[TestFixture(typeof(bool), 1)]
[TestFixture(typeof(int), 2)]
[TestFixture(typeof(float), 3)]
public class HexMapFileSpatialTests<T>
{
    private readonly int _kind;
    private string _path = null!;

    public HexMapFileSpatialTests(int kind) => _kind = kind;

    [SetUp]
    public void SetUp() =>
        _path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "HexMapFileSpatialTests-" + Guid.NewGuid().ToString("N") + ".hmap");

    [TearDown]
    public void TearDown() => File.Delete(_path);

    private static HexMapGeometry Geometry => new(2, 1, new VectorXY(-2, 3), 0.5f, Layout.EvenR);

    [Test]
    public void Write_ConcreteSpatialTypeSelectsGeometryPreservingOverload()
    {
        if (_kind == 1) HexMapFile.Write(_path, new SpatialBoolHexMap(Geometry));
        else if (_kind == 2) HexMapFile.Write(_path, new SpatialIntHexMap(Geometry));
        else HexMapFile.Write(_path, new SpatialFloatHexMap(Geometry));

        byte[] bytes = File.ReadAllBytes(_path);
        Assert.That(bytes[5], Is.EqualTo(1));
        Assert.That(bytes[6], Is.EqualTo(_kind));
        HexMapGeometry restored = _kind switch
        {
            1 => HexMapFile.ReadSpatialBoolHexMap(_path).Geometry,
            2 => HexMapFile.ReadSpatialIntHexMap(_path).Geometry,
            _ => HexMapFile.ReadSpatialFloatHexMap(_path).Geometry,
        };
        Assert.That(restored, Is.EqualTo(Geometry));
    }

    [Test]
    public void Write_TopologyInterfaceSelectsTopologyOnlyOverload()
    {
        if (_kind == 1) HexMapFile.Write(_path, (IHexMap<bool>)new SpatialBoolHexMap(Geometry));
        else if (_kind == 2) HexMapFile.Write(_path, (IHexMap<int>)new SpatialIntHexMap(Geometry));
        else HexMapFile.Write(_path, (IHexMap<float>)new SpatialFloatHexMap(Geometry));

        Assert.That(File.ReadAllBytes(_path)[5], Is.Zero);
        HexMapTopology restored = _kind switch
        {
            1 => HexMapFile.ReadBoolHexMap(_path).Topology,
            2 => HexMapFile.ReadIntHexMap(_path).Topology,
            _ => HexMapFile.ReadFloatHexMap(_path).Topology,
        };
        Assert.That(restored, Is.EqualTo(Geometry.Topology));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Write_InvalidSpatialSourceDoesNotTruncateFile(bool mismatchedTopology)
    {
        byte[] original = { 11, 22, 33 };
        File.WriteAllBytes(_path, original);
        ISpatialHexMap<T> map = mismatchedTopology
            ? new CustomMap(new HexMapTopology(1, 2, Layout.EvenR), Geometry)
            : new CustomMap(default, default);

        if (mismatchedTopology)
            Assert.Throws<ArgumentException>(() => Write(map));
        else
            Assert.Throws<ArgumentOutOfRangeException>(() => Write(map));

        Assert.That(File.ReadAllBytes(_path), Is.EqualTo(original));
    }

    private void Write(ISpatialHexMap<T> map)
    {
        if (_kind == 1) HexMapFile.Write(_path, (ISpatialHexMap<bool>)(object)map);
        else if (_kind == 2) HexMapFile.Write(_path, (ISpatialHexMap<int>)(object)map);
        else HexMapFile.Write(_path, (ISpatialHexMap<float>)(object)map);
    }

    private sealed class CustomMap : HexMap<T>, ISpatialHexMap<T>
    {
        public CustomMap(HexMapTopology topology, HexMapGeometry geometry) : base(topology) => Geometry = geometry;
        public HexMapGeometry Geometry { get; }
    }
}
