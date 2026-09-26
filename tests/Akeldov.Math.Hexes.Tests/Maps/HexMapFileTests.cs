using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using System.IO;
using System.Text;

namespace Akeldov.Math.Hexes.Tests.Maps;

[TestFixture(typeof(bool), 1, false)]
[TestFixture(typeof(int), 2, false)]
[TestFixture(typeof(float), 3, false)]
[TestFixture(typeof(bool), 1, true)]
[TestFixture(typeof(int), 2, true)]
[TestFixture(typeof(float), 3, true)]
public class HexMapFileTests<T>
{
    private readonly int _kind;
    private readonly bool _spatial;
    private string _directory = null!;
    private string _path = null!;

    public HexMapFileTests(int kind, bool spatial)
    {
        _kind = kind;
        _spatial = spatial;
    }

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "HexMapFileTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "\u043a\u0430\u0440\u0442\u0430 terrain.hmap");
    }

    [TearDown]
    public void TearDown()
    {
        foreach (string file in Directory.EnumerateFiles(_directory))
            File.Delete(file);
        Directory.Delete(_directory);
    }

    private static HexMapGeometry Geometry => new(
        new HexMapTopology(2, 2, Layout.EvenQ), new VectorXY(-2.25f, 1.5f), 0.5f);

    private T[] Values => (T[])(_kind switch
    {
        1 => (object)new[] { true, false, true, false },
        2 => new[] { int.MinValue, int.MaxValue, -42, 0 },
        _ => new[]
        {
            BitConverter.Int32BitsToSingle(unchecked((int)0xFFC12345)),
            BitConverter.Int32BitsToSingle(int.MinValue),
            float.PositiveInfinity, float.Epsilon,
        },
    });

    [TestCase(false)]
    [TestCase(true)]
    public void RoundTrip_SupportsAbsoluteAndRelativePaths(bool relative)
    {
        string path = relative ? Path.GetRelativePath(Environment.CurrentDirectory, _path) : _path;
        Assert.That(Path.IsPathFullyQualified(path), Is.EqualTo(!relative));
        IHexMap<T> map = CreateMap();

        Write(path, map);
        HexMap<T> restored = Read(path, 4);

        AssertMap(restored);
        Assert.That(restored, Is.Not.SameAs(map));
        restored[0] = Values[1];
        Assert.That(map[0], Is.EqualTo(Values[0]));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(exclusive.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Write_UsesExistingBinaryFormatAndTruncatesOldFile()
    {
        IHexMap<T> map = CreateMap();
        byte[] expected = Encode(map);
        File.WriteAllBytes(_path, new byte[expected.Length + 100]);

        Write(_path, map);

        Assert.That(File.ReadAllBytes(_path), Is.EqualTo(expected));
    }

    [Test]
    public void Read_AcceptsStreamRecordAndIgnoresTrailingData()
    {
        File.WriteAllBytes(_path, Encode(CreateMap()).Concat(new byte[] { 1, 2, 3 }).ToArray());

        AssertMap(Read(_path));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(exclusive.CanWrite, Is.True);
    }

    [Test]
    public void RoundTrip_EmptyMapAcceptsZeroCellLimit()
    {
        var geometry = new HexMapGeometry(new HexMapTopology(0, 7, Layout.OddQ), new VectorXY(-8, 10), 2f);
        IHexMap<T> map = _spatial
            ? new SpatialHexMap<T>(geometry)
            : new HexMap<T>(geometry.Topology);

        Write(_path, map);
        HexMap<T> restored = Read(_path, 0);

        Assert.That(restored.Topology, Is.EqualTo(geometry.Topology));
        if (_spatial)
            Assert.That(((ISpatialHexMap<T>)restored).Geometry, Is.EqualTo(geometry));
        Assert.That(new FileInfo(_path).Length, Is.EqualTo(_spatial ? 28 : 16));
    }

    [Test]
    public void Read_CellLimitFailureClosesFile()
    {
        Write(_path, CreateMap());

        Assert.Throws<InvalidDataException>(() => Read(_path, 3));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(exclusive.CanWrite, Is.True);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Read_MalformedRecordClosesFile(bool truncated)
    {
        byte[] bytes = Encode(CreateMap());
        if (truncated)
            bytes = bytes.Take(bytes.Length - 1).ToArray();
        else
            bytes[0] = 0;
        File.WriteAllBytes(_path, bytes);

        if (truncated)
            Assert.Throws<EndOfStreamException>(() => Read(_path));
        else
            Assert.Throws<InvalidDataException>(() => Read(_path));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(exclusive.CanWrite, Is.True);
    }

    [TestCase(5)] // Map kind.
    [TestCase(6)] // Value kind.
    public void Read_RejectsOtherRecordKinds(int offset)
    {
        byte[] bytes = Encode(CreateMap());
        bytes[offset] = offset == 5 ? (byte)(_spatial ? 0 : 1) : (byte)(_kind == 1 ? 2 : 1);
        File.WriteAllBytes(_path, bytes);

        Assert.Throws<InvalidDataException>(() => Read(_path));
    }

    [Test]
    public void Read_MissingFilePropagatesFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() => Read(_path));
        Assert.That(File.Exists(_path), Is.False);
    }

    [Test]
    public void Write_MissingParentIsNotCreated()
    {
        string parent = Path.Combine(_directory, "missing");

        Assert.Throws<DirectoryNotFoundException>(() => Write(Path.Combine(parent, "map.hmap"), CreateMap()));
        Assert.That(Directory.Exists(parent), Is.False);
    }

    [Test]
    public void Read_InvalidArgumentsAreRejectedBeforeOpeningFile()
    {
        Assert.That(Assert.Throws<ArgumentNullException>(() => Read(null!))!.ParamName, Is.EqualTo("path"));
        Assert.That(Assert.Throws<ArgumentException>(() => Read(""))!.ParamName, Is.EqualTo("path"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => Read(_path, -1))!.ParamName, Is.EqualTo("maxCellCount"));
        Assert.That(File.Exists(_path), Is.False);
    }

    [Test]
    public void Write_InvalidArgumentsDoNotTruncateExistingFile()
    {
        byte[] original = { 10, 20, 30, 40 };
        File.WriteAllBytes(_path, original);

        Assert.That(Assert.Throws<ArgumentNullException>(() => Write(null!, CreateMap()))!.ParamName, Is.EqualTo("path"));
        Assert.That(Assert.Throws<ArgumentException>(() => Write("", CreateMap()))!.ParamName, Is.EqualTo("path"));
        Assert.That(Assert.Throws<ArgumentNullException>(() => Write(_path, null!))!.ParamName, Is.EqualTo("map"));
        Assert.That(File.ReadAllBytes(_path), Is.EqualTo(original));
    }

    [Test]
    public void Write_ValueAccessFailureClosesFile()
    {
        IHexMap<T> map = new ThrowingMap(Geometry);

        Assert.Throws<InvalidOperationException>(() => Write(_path, map));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.That(exclusive.CanWrite, Is.True);
    }

    private IHexMap<T> CreateMap() => _spatial
        ? new SpatialHexMap<T>(Geometry, Values)
        : new HexMap<T>(Geometry.Topology, Values);

    private void Write(string path, IHexMap<T> map)
    {
        if (_spatial)
        {
            if (_kind == 1) HexMapFile.Write(path, (ISpatialHexMap<bool>)(object)map);
            else if (_kind == 2) HexMapFile.Write(path, (ISpatialHexMap<int>)(object)map);
            else HexMapFile.Write(path, (ISpatialHexMap<float>)(object)map);
        }
        else
        {
            if (_kind == 1) HexMapFile.Write(path, (IHexMap<bool>)(object)map);
            else if (_kind == 2) HexMapFile.Write(path, (IHexMap<int>)(object)map);
            else HexMapFile.Write(path, (IHexMap<float>)(object)map);
        }
    }

    private HexMap<T> Read(string path, int maxCellCount = int.MaxValue) => (HexMap<T>)(_kind switch
    {
        1 => _spatial ? (object)HexMapFile.ReadSpatialBoolHexMap(path, maxCellCount) : HexMapFile.ReadBoolHexMap(path, maxCellCount),
        2 => _spatial ? (object)HexMapFile.ReadSpatialIntHexMap(path, maxCellCount) : HexMapFile.ReadIntHexMap(path, maxCellCount),
        _ => _spatial ? (object)HexMapFile.ReadSpatialFloatHexMap(path, maxCellCount) : HexMapFile.ReadFloatHexMap(path, maxCellCount),
    });

    private byte[] Encode(IHexMap<T> map)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        if (_spatial)
        {
            if (_kind == 1) writer.Write((ISpatialHexMap<bool>)(object)map);
            else if (_kind == 2) writer.Write((ISpatialHexMap<int>)(object)map);
            else writer.Write((ISpatialHexMap<float>)(object)map);
        }
        else
        {
            if (_kind == 1) writer.Write((IHexMap<bool>)(object)map);
            else if (_kind == 2) writer.Write((IHexMap<int>)(object)map);
            else writer.Write((IHexMap<float>)(object)map);
        }
        return stream.ToArray();
    }

    private void AssertMap(HexMap<T> map)
    {
        Type expectedType = _kind switch
        {
            1 => _spatial ? typeof(SpatialBoolHexMap) : typeof(BoolHexMap),
            2 => _spatial ? typeof(SpatialIntHexMap) : typeof(IntHexMap),
            _ => _spatial ? typeof(SpatialFloatHexMap) : typeof(FloatHexMap),
        };
        Assert.That(map, Is.TypeOf(expectedType));
        Assert.That(map.Topology, Is.EqualTo(Geometry.Topology));
        if (_spatial)
            Assert.That(((ISpatialHexMap<T>)map).Geometry, Is.EqualTo(Geometry));
        T[] expected = Values;
        for (int i = 0; i < expected.Length; i++)
        {
            if (_kind == 3)
                Assert.That(BitConverter.SingleToInt32Bits((float)(object)map[i]!),
                    Is.EqualTo(BitConverter.SingleToInt32Bits((float)(object)expected[i]!)), $"Cell {i}");
            else
                Assert.That(map[i], Is.EqualTo(expected[i]), $"Cell {i}");
        }
    }

    private sealed class ThrowingMap : SpatialHexMap<T>, ISpatialHexMap<T>
    {
        public ThrowingMap(HexMapGeometry geometry) : base(geometry) { }
        T IHexMap<T>.this[int index] => throw new InvalidOperationException("Simulated source failure.");
    }
}
