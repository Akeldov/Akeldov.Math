using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

namespace Akeldov.Math.Hexes.Tests.Topology;

public class HexVertexTripletRasterTests
{
    [Test]
    public void RasterTypes_ExposeGeometryConstructors()
    {
        Type[] gridTypes =
        {
            typeof(IndexTripletRaster),
            typeof(IndexPartialTripletRaster),
            typeof(IndexSeptupletRaster),
            typeof(IndexPartialSeptupletRaster),
            typeof(BarycentricTripletRaster),
            typeof(BarycentricPartialTripletRaster),
            typeof(ChromaticBarycentricTripletRaster),
            typeof(ChromaticBarycentricPartialTripletRaster),
            typeof(ChromaticIndexTripletRaster),
            typeof(ChromaticIndexPartialTripletRaster),
        };

        foreach (Type gridType in gridTypes)
        {
            var constructors = gridType.GetConstructors();
            Type[][] signatures = constructors
                .Select(constructor => constructor.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray())
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(constructors, Has.Length.EqualTo(2), gridType.Name);
                Assert.That(
                    signatures.Any(signature => signature.SequenceEqual(new[] { typeof(HexMapGeometry) })),
                    Is.True,
                    gridType.Name);
                Assert.That(
                    signatures.Any(signature => signature.SequenceEqual(new[] { typeof(HexMapGeometry), typeof(RasterGeometry) })),
                    Is.True,
                    gridType.Name);
            });
        }
    }

    [Test]
    public void RasterTypes_GeometryConstructor_CoversWholeHexMap()
    {
        var hexMapGeometry = new HexMapGeometry(
            3,
            2,
            new VectorXY(10f, -20f),
            2f,
            Layout.EvenQ);
        RasterGeometry expected = hexMapGeometry.ToRasterGeometry(1f);
        Type[] gridTypes =
        {
            typeof(IndexTripletRaster),
            typeof(IndexPartialTripletRaster),
            typeof(IndexSeptupletRaster),
            typeof(IndexPartialSeptupletRaster),
            typeof(BarycentricTripletRaster),
            typeof(BarycentricPartialTripletRaster),
            typeof(ChromaticBarycentricTripletRaster),
            typeof(ChromaticBarycentricPartialTripletRaster),
            typeof(ChromaticIndexTripletRaster),
            typeof(ChromaticIndexPartialTripletRaster),
        };

        foreach (Type gridType in gridTypes)
        {
            object grid = Activator.CreateInstance(gridType, hexMapGeometry)!;
            var geometry = (RasterGeometry)gridType.GetProperty("Geometry")!.GetValue(grid)!;

            Assert.That(geometry, Is.EqualTo(expected), gridType.Name);
        }
    }

    [Test]
    public void BarycentricTripletRaster_UsesProvidedRasterGeometry()
    {
        var rasterGeometry = new RasterGeometry(
            new PointXY(10f, -20f),
            new VectorXY(8f, 6f),
            new VectorXYInt(4, 3));
        var hexMapGeometry = new HexMapGeometry(2, 2, 1f, Layout.OddR);

        var grid = new BarycentricTripletRaster(
            hexMapGeometry,
            rasterGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
            Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
            Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
            Assert.That(grid[11], Is.EqualTo(grid[new VectorXYInt(3, 2)]));
        });
    }

    [Test]
    public void BarycentricPartialTripletRaster_UsesProvidedGeometries()
    {
        var rasterGeometry = new RasterGeometry(
            new PointXY(10f, -20f),
            new VectorXY(8f, 6f),
            new VectorXYInt(4, 3));
        var hexMapGeometry = new HexMapGeometry(2, 2, 1f, Layout.OddR);

        var grid = new BarycentricPartialTripletRaster(hexMapGeometry, rasterGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
            Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
            Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
            Assert.That(grid[11], Is.EqualTo(grid[new VectorXYInt(3, 2)]));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ChromaticIndexTripletRasters_UseProvidedGeometries(bool partial)
    {
        var hexMapGeometry = new HexMapGeometry(2, 2, new VectorXY(10f, -20f), 2f, Layout.OddR);
        var rasterGeometry = new RasterGeometry(
            new PointXY(8f, -22f),
            new VectorXY(8f, 6f),
            new VectorXYInt(4, 3));

        if (partial)
        {
            var grid = new ChromaticIndexPartialTripletRaster(hexMapGeometry, rasterGeometry);
            Assert.Multiple(() =>
            {
                Assert.That(grid, Is.AssignableTo<ISpatialRaster<PartialTriplet<byte>>>());
                Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
                Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
                Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
            });
        }
        else
        {
            var grid = new ChromaticIndexTripletRaster(hexMapGeometry, rasterGeometry);
            Assert.Multiple(() =>
            {
                Assert.That(grid, Is.AssignableTo<ISpatialRaster<Triplet<byte>>>());
                Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
                Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
                Assert.That(grid.Resolution, Is.EqualTo(rasterGeometry.Resolution));
            });
        }
    }

    [TestCase(Layout.OddR)]
    [TestCase(Layout.EvenR)]
    [TestCase(Layout.OddQ)]
    [TestCase(Layout.EvenQ)]
    public void IndexTripletRaster_ExposesGeometryResolutionAndSampledValues(Layout layout)
    {
        var topology = new HexMapTopology(2, 1, layout);
        var hexMapGeometry = new HexMapGeometry(topology, 1f);
        var rasterGeometry = CreateDefaultRasterGeometry(hexMapGeometry, new VectorXYInt(4, 2));
        var grid = new IndexTripletRaster(hexMapGeometry, rasterGeometry);

        Assert.Multiple(() =>
        {
            Assert.That(grid, Is.AssignableTo<ISpatialRaster<Triplet<VectorXYInt>>>());
            Assert.That(grid.SourceHexMapGeometry, Is.EqualTo(hexMapGeometry));
            Assert.That(grid.Geometry, Is.EqualTo(rasterGeometry));
            Assert.That(grid.Topology, Is.EqualTo(topology));
            Assert.That(grid.Resolution, Is.EqualTo(new VectorXYInt(4, 2)));
            Assert.That(grid[0], Is.EqualTo(grid[VectorXYInt.Zero]));
        });
    }

    [Test]
    public void RasterTypes_DoNotExposeBackingCollections()
    {
        Assert.That(typeof(IndexTripletRaster).GetProperty("IndexTriplets"), Is.Null);
        Assert.That(typeof(IndexPartialTripletRaster).GetProperty("IndexTriplets"), Is.Null);
        Assert.That(typeof(BarycentricTripletRaster).GetProperty("BarycentricCoordinates"), Is.Null);
        Assert.That(typeof(BarycentricPartialTripletRaster).GetProperty("BarycentricCoordinates"), Is.Null);
        Assert.That(typeof(ChromaticIndexTripletRaster).GetProperty("ChromaticIndices"), Is.Null);
        Assert.That(typeof(ChromaticIndexPartialTripletRaster).GetProperty("ChromaticIndices"), Is.Null);
        Assert.That(typeof(ChromaticIndexTripletRaster).GetProperty("Layout"), Is.Null);
        Assert.That(typeof(ChromaticIndexPartialTripletRaster).GetProperty("Layout"), Is.Null);

        Type[] topologyRasterTypes =
        {
            typeof(IndexTripletRaster),
            typeof(IndexPartialTripletRaster),
            typeof(IndexSeptupletRaster),
            typeof(IndexPartialSeptupletRaster),
        };
        string[] redundantPropertyNames =
        {
            "Width",
            "Height",
            "ResolutionX",
            "ResolutionY",
            "HexResolution",
            "Layout",
        };

        foreach (Type rasterType in topologyRasterTypes)
        {
            foreach (string propertyName in redundantPropertyNames)
                Assert.That(rasterType.GetProperty(propertyName), Is.Null, $"{rasterType.Name}.{propertyName}");
        }
    }

    [Test]
    public void ChromaticIndexTripletRasters_HaveOnlyLayoutFillPrivateMethods()
    {
        string[] expectedMethodNames = { "Fill", "FillOddR", "FillEvenR", "FillOddQ", "FillEvenQ" };
        Type[] rasterTypes = { typeof(ChromaticIndexTripletRaster), typeof(ChromaticIndexPartialTripletRaster) };

        foreach (Type rasterType in rasterTypes)
        {
            string[] methodNames = rasterType
                .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .ToArray();

            Assert.That(methodNames, Is.EquivalentTo(expectedMethodNames), rasterType.Name);
        }
    }

    [Test]
    public void TopologyRasters_HaveOnlyLayoutFillPrivateMethods()
    {
        string[] expectedMethodNames = { "Fill", "FillOddR", "FillEvenR", "FillOddQ", "FillEvenQ" };
        Type[] rasterTypes =
        {
            typeof(IndexTripletRaster),
            typeof(IndexPartialTripletRaster),
            typeof(IndexSeptupletRaster),
            typeof(IndexPartialSeptupletRaster),
        };

        foreach (Type rasterType in rasterTypes)
        {
            string[] methodNames = rasterType
                .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .ToArray();

            Assert.That(methodNames, Is.EquivalentTo(expectedMethodNames), rasterType.Name);
        }
    }

    [Test]
    public void IndexTripletRaster_UsesNearestVertexAndLeftRightOrder()
    {
        var expected = new Triplet<VectorXYInt>(
            new VectorXYInt(0, 0),
            new VectorXYInt(0, 1),
            new VectorXYInt(1, 0));
        var hexMapGeometry = new HexMapGeometry(2, 2, 1f, Layout.OddR);
        var grid = new IndexTripletRaster(
            hexMapGeometry,
            CreateDefaultRasterGeometry(hexMapGeometry, new VectorXYInt(64, 64)));
        bool found = false;

        for (int y = 0; y < grid.Resolution.Y && !found; y++)
        {
            for (int x = 0; x < grid.Resolution.X; x++)
            {
                if (grid[x, y].Equals(expected))
                {
                    found = true;
                    break;
                }
            }
        }

        Assert.That(found, Is.True);
    }

    [TestCase(Layout.OddR, 0, 1, 1, 0)]
    [TestCase(Layout.EvenR, 1, 1, 1, 0)]
    [TestCase(Layout.OddQ, 1, 0, 1, -1)]
    [TestCase(Layout.EvenQ, 1, 1, 1, 0)]
    public void GetAdjacentTriplet_UsesLayoutSpecificVertexEdges(
        Layout layout,
        int expectedLeftX,
        int expectedLeftY,
        int expectedRightX,
        int expectedRightY)
    {
        Triplet<VectorXYInt> triplet = VectorXYInt.Zero.GetAdjacentTriplet(HexVertex.Vertex0, layout);

        AssertTriplet(
            triplet,
            new Triplet<VectorXYInt>(
                VectorXYInt.Zero,
                new VectorXYInt(expectedLeftX, expectedLeftY),
                new VectorXYInt(expectedRightX, expectedRightY)));
    }

    [Test]
    public void GetAdjacentEdges_WhenVertexIsInvalid_Throws()
    {
        var invalid = (HexVertex)6;

        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetAdjacentEdges());
            Assert.Throws<ArgumentOutOfRangeException>(() => invalid.GetAdjacentEdges(Layout.OddR));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = VectorXYInt.Zero.GetAdjacentPair(invalid, Layout.OddR));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = VectorXYInt.Zero.GetAdjacentTriplet(invalid, Layout.OddR));
        });
    }

    [Test]
    public void ChromaticIndexTripletRaster_UsesSameVertexTripletOrder()
    {
        var indexTriplet = new Triplet<VectorXYInt>(
            new VectorXYInt(0, 0),
            new VectorXYInt(0, 1),
            new VectorXYInt(1, 0));
        Triplet<byte> expected = indexTriplet.GetChromaticTriplet(Layout.OddR);
        var topology = new HexMapTopology(2, 2, Layout.OddR);
        var resolution = new VectorXYInt(64, 64);
        var hexMapGeometry = new HexMapGeometry(topology, 1f);
        var rasterGeometry = new RasterGeometry(new PointXY(0f, 0f), hexMapGeometry.GetBoundingBoxSize(), resolution);
        var indexGrid = new IndexTripletRaster(hexMapGeometry, rasterGeometry);
        var grid = new ChromaticIndexTripletRaster(hexMapGeometry, rasterGeometry);
        VectorXYInt? matchingIndex = null;

        for (int y = 0; y < resolution.Y && matchingIndex == null; y++)
        {
            for (int x = 0; x < resolution.X; x++)
            {
                var gridIndex = new VectorXYInt(x, y);
                if (indexGrid[gridIndex].Equals(indexTriplet))
                {
                    matchingIndex = gridIndex;
                    break;
                }
            }
        }

        Assert.That(matchingIndex, Is.Not.Null);
        VectorXYInt index = matchingIndex!.Value;

        Triplet<byte> actual = grid[index];

        AssertTriplet(actual, expected);
        Assert.That(grid.TryGetValue(index, out Triplet<byte> fromTry), Is.True);
        AssertTriplet(fromTry, expected);
    }

    [Test]
    public void TryGetMethods_WhenRasterIndexIsOutside_ReturnFalse()
    {
        var adjacency = new IndexSeptupletMap(new HexMapTopology(2, 2, Layout.OddR));
        var resolution = VectorXYInt.One;
        var hexMapGeometry = new HexMapGeometry(adjacency.Topology, 1f);
        var rasterGeometry = new RasterGeometry(new PointXY(-2f, -2f), new VectorXY(4f, 4f), resolution);
        var indexGrid = new IndexTripletRaster(hexMapGeometry, rasterGeometry);
        var partialIndexGrid = new IndexPartialTripletRaster(hexMapGeometry, rasterGeometry);
        var barycentricGrid = new BarycentricTripletRaster(
            hexMapGeometry,
            rasterGeometry);
        var partialBarycentricGrid = new BarycentricPartialTripletRaster(
            hexMapGeometry,
            rasterGeometry);
        var chromaticGrid = new ChromaticIndexTripletRaster(hexMapGeometry, rasterGeometry);
        var outsideIndex = new VectorXYInt(1, 0);

        Assert.That(indexGrid.TryGetValue(outsideIndex, out Triplet<VectorXYInt> indexTriplet), Is.False);
        Assert.That(indexTriplet, Is.EqualTo(default(Triplet<VectorXYInt>)));
        Assert.That(partialIndexGrid.TryGetValue(outsideIndex, out PartialTriplet<VectorXYInt> partialIndexTriplet), Is.False);
        Assert.That(partialIndexTriplet, Is.EqualTo(default(PartialTriplet<VectorXYInt>)));
        Assert.That(barycentricGrid.TryGetValue(outsideIndex, out Triplet<float> barycentricCoordinates), Is.False);
        Assert.That(barycentricCoordinates, Is.EqualTo(default(Triplet<float>)));
        Assert.That(partialBarycentricGrid.TryGetValue(outsideIndex, out PartialTriplet<float> partialBarycentricCoordinates), Is.False);
        Assert.That(partialBarycentricCoordinates, Is.EqualTo(default(PartialTriplet<float>)));
        Assert.That(chromaticGrid.TryGetValue(outsideIndex, out Triplet<byte> chromaticIndices), Is.False);
        Assert.That(chromaticIndices, Is.EqualTo(default(Triplet<byte>)));
    }

    [Test]
    public void Constructors_WhenArgumentsAreInvalid_Throw()
    {
        var emptyAdjacency = new IndexSeptupletMap(new HexMapTopology(0, 1, Layout.OddR));
        var emptyPartialAdjacency = new IndexPartialSeptupletMap(new HexMapTopology(0, 1, Layout.OddR));

        var emptyHexMapGeometry = new HexMapGeometry(emptyAdjacency.Topology, 1f);
        var validRasterGeometry = new RasterGeometry(new PointXY(0f, 0f), VectorXY.One, VectorXYInt.One);

        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexTripletRaster(emptyHexMapGeometry, validRasterGeometry));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletRaster(new HexMapGeometry(1, 1, 1f, Layout.OddR), default));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarycentricTripletRaster(new HexMapGeometry(1, 1, 1f, Layout.OddR), default));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSeptupletRaster(emptyHexMapGeometry, validRasterGeometry));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexSeptupletRaster(new HexMapGeometry(1, 1, 1f, Layout.OddR), default));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialSeptupletRaster(new HexMapGeometry(emptyPartialAdjacency.Topology, 1f), validRasterGeometry));
        Assert.Throws<ArgumentOutOfRangeException>(() => new IndexPartialTripletRaster(emptyHexMapGeometry, validRasterGeometry));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarycentricTripletRaster(
            new HexMapGeometry(emptyAdjacency.Topology, 1f),
            new RasterGeometry(new PointXY(-1f, -1f), new VectorXY(2f, 2f), VectorXYInt.One)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarycentricPartialTripletRaster(
            new HexMapGeometry(emptyAdjacency.Topology, 1f),
            new RasterGeometry(new PointXY(-1f, -1f), new VectorXY(2f, 2f), VectorXYInt.One)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexTripletRaster(new HexMapGeometry(emptyAdjacency.Topology, 1f), validRasterGeometry));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChromaticIndexPartialTripletRaster(new HexMapGeometry(emptyAdjacency.Topology, 1f), validRasterGeometry));
    }

    private static void AssertTriplet<T>(Triplet<T> actual, Triplet<T> expected)
    {
        Assert.That(actual.Main, Is.EqualTo(expected.Main));
        Assert.That(actual.Left, Is.EqualTo(expected.Left));
        Assert.That(actual.Right, Is.EqualTo(expected.Right));
    }

    private static RasterGeometry CreateDefaultRasterGeometry(
        HexMapGeometry hexMapGeometry,
        VectorXYInt resolution)
    {
        return new RasterGeometry(
            new PointXY(0f, 0f),
            hexMapGeometry.GetBoundingBoxSize(),
            resolution);
    }

}
