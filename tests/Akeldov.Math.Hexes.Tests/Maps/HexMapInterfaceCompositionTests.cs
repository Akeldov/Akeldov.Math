using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests.Maps;

public class HexMapInterfaceCompositionTests
{
    [Test]
    public void HexMap_ProvidesMutableAndReadOnlyViews()
    {
        var topology = new HexMapTopology(2, 1, Layout.OddR);
        var mutableMap = new HexMap<int>(topology);
        IHexMap<int> readOnlyView = mutableMap;

        mutableMap[new VectorXYInt(0, 0)] = 10;
        mutableMap[1] = 20;

        Assert.Multiple(() =>
        {
            Assert.That(readOnlyView.Topology, Is.EqualTo(topology));
            Assert.That(readOnlyView[0], Is.EqualTo(10));
            Assert.That(readOnlyView[new VectorXYInt(1, 0)], Is.EqualTo(20));
        });
    }

    [Test]
    public void SpatialHexMap_ProvidesMutableSpatialViewAndMaintainsTopologyInvariant()
    {
        var geometry = new HexMapGeometry(
            2,
            1,
            new VectorXY(10f, -5f),
            2f,
            Layout.EvenQ);
        var mutableMap = new SpatialHexMap<int>(geometry);
        ISpatialHexMap<int> spatialView = mutableMap;

        mutableMap[0] = 7;
        mutableMap[new VectorXYInt(1, 0)] = 9;

        Assert.Multiple(() =>
        {
            Assert.That(spatialView.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialView.Topology, Is.EqualTo(spatialView.Geometry.Topology));
            Assert.That(spatialView[0], Is.EqualTo(7));
            Assert.That(spatialView[1], Is.EqualTo(9));
        });
    }

    [Test]
    public void SpatialNumericMaps_ImplementCombinedSpatialAndNumericContracts()
    {
        var geometry = new HexMapGeometry(2, 1, 1.5f, Layout.EvenR);
        var floatMap = new SpatialFloatHexMap(geometry, new[] { -2f, 4f });
        var intMap = new SpatialIntHexMap(geometry, new[] { -3, 5 });
        ISpatialFloatHexMap spatialFloatView = floatMap;
        ISpatialIntHexMap spatialIntView = intMap;
        IFloatHexMap floatView = spatialFloatView;
        IIntHexMap intView = spatialIntView;
        floatMap[0] = -6f;
        intMap[1] = 8;

        Assert.Multiple(() =>
        {
            Assert.That(spatialFloatView.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialIntView.Geometry, Is.EqualTo(geometry));
            Assert.That(spatialFloatView.Topology, Is.EqualTo(spatialFloatView.Geometry.Topology));
            Assert.That(spatialIntView.Topology, Is.EqualTo(spatialIntView.Geometry.Topology));
            Assert.That(floatView.Min, Is.EqualTo(-6f));
            Assert.That(floatView.Max, Is.EqualTo(4f));
            Assert.That(intView.Min, Is.EqualTo(-3));
            Assert.That(intView.Max, Is.EqualTo(8));
        });
    }

    [Test]
    public void SpatialSpecializations_AreSealedLeafTypes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(SpatialBoolHexMap).IsSealed, Is.True);
            Assert.That(typeof(SpatialFloatHexMap).IsSealed, Is.True);
            Assert.That(typeof(SpatialIntHexMap).IsSealed, Is.True);
        });
    }
}
