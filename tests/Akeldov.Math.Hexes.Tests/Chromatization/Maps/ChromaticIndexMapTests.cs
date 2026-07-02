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
            ChromaticIndexMap chromatization = new ChromaticIndexMap(width, height, layout);

            Assert.That(chromatization.Width, Is.EqualTo(width));
            Assert.That(chromatization.Height, Is.EqualTo(height));
            Assert.That(chromatization.Layout, Is.EqualTo(layout));

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

        ChromaticIndexMap chromatization = new ChromaticIndexMap(resolution.X, resolution.Y, Layout.OddR);

        Assert.That(chromatization.Width, Is.EqualTo(2));
        Assert.That(chromatization.Height, Is.EqualTo(1));
        Assert.That(chromatization.Layout, Is.EqualTo(Layout.OddR));
        Assert.That(typeof(ChromaticIndexMap).GetProperty("ChromaticIndices"), Is.Null);
    }

    [Test]
    public void ChromaticIndexMap_ImplementsIHexMap()
    {
        var source = new ChromaticIndexMap(3, 2, Layout.OddR);
        IHexMap<byte> map = source;

        byte chromaticIndex = source[5];

        Assert.That(map.Width, Is.EqualTo(3));
        Assert.That(map.Height, Is.EqualTo(2));
        Assert.That(map[new VectorXYInt(2, 1)], Is.EqualTo(chromaticIndex));
        Assert.That(map[5], Is.EqualTo(chromaticIndex));
    }

    [Test]
    public void Indexer_WhenIndexIsOutsideChromatization_Throws()
    {
        var chromatization = new ChromaticIndexMap(3, 2, Layout.OddR);

        Assert.Throws<IndexOutOfRangeException>(() => _ = chromatization[new VectorXYInt(3, 0)]);
        Assert.Throws<IndexOutOfRangeException>(() => _ = chromatization[new VectorXYInt(0, 2)]);
    }

    [Test]
    public void Constructor_WhenWidthIsNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChromaticIndexMap(-1, 1, Layout.OddR));
    }

    [Test]
    public void Constructor_WhenWidthIsNegativeThroughResolution_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexMap(-1, 1, Layout.OddR));
    }

    [Test]
    public void Constructor_WhenLayoutIsUnsupported_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexMap(0, 0, (Layout)42));
    }
}
