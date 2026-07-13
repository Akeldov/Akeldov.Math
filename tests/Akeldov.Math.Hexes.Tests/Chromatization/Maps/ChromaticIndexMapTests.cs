using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Chromatization;

public class ChromaticIndexMapTests
{
    [Test]
    public void Constructor_MatchesSingleHexChromaticClass_ForEveryLayout()
    {
        const int width = 5;
        const int height = 4;

        foreach (Layout layout in Enum.GetValues(typeof(Layout)))
        {
            var topology = new HexMapTopology(width, height, layout);
            ChromaticIndexMap chromatization = new ChromaticIndexMap(topology);

            Assert.That(chromatization.Topology.Resolution, Is.EqualTo(new VectorXYInt(width, height)));
            Assert.That(chromatization.Topology.Layout, Is.EqualTo(layout));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte expected = (byte)new VectorXYInt(x, y).GetChromaticClass(layout);

                    Assert.That(chromatization[new VectorXYInt(x, y)], Is.EqualTo(expected));
                }
            }
        }
    }

    [Test]
    public void Constructor_ExposesDimensionsAndLayout()
    {
        var resolution = new VectorXYInt(2, 1);

        var topology = new HexMapTopology(resolution, Layout.OddR);
        ChromaticIndexMap chromatization = new ChromaticIndexMap(topology);

        Assert.That(chromatization.Topology.Resolution, Is.EqualTo(resolution));
        Assert.That(chromatization.Topology.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(typeof(ChromaticIndexMap).GetProperty("ChromaticIndices"), Is.Null);
    }

    [Test]
    public void ChromaticIndexMap_ImplementsIHexMap()
    {
        var source = new ChromaticIndexMap(new HexMapTopology(3, 2, Layout.OddR));
        IHexMap<byte> map = source;

        byte chromaticIndex = source[5];

        Assert.That(map.Topology.Resolution, Is.EqualTo(new VectorXYInt(3, 2)));
        Assert.That(map.Topology.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(chromaticIndex));
        Assert.That(map[5], Is.EqualTo(chromaticIndex));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideChromatization_Throws()
    {
        var chromatization = new ChromaticIndexMap(new HexMapTopology(3, 2, Layout.OddR));

        Assert.Throws<IndexOutOfRangeException>(() => _ = chromatization[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = chromatization[new VectorXYInt(0, 2)]);
    }

    [Test]
    public void Constructor_WhenWidthIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChromaticIndexMap(new HexMapTopology(-1, 1, Layout.OddR)));
    }

    [Test]
    public void Constructor_WhenWidthIsNegativeThroughResolution_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChromaticIndexMap(new HexMapTopology(new VectorXYInt(-1, 1), Layout.OddR)));
    }

    [Test]
    public void Constructor_WhenLayoutIsUnsupported_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChromaticIndexMap(new HexMapTopology(0, 0, (Layout)42)));
    }
}
