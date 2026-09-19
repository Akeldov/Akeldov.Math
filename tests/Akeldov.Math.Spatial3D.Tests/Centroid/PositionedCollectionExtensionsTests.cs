namespace Akeldov.Math.Spatial3D.Tests.Centroid;

public class PositionedCollectionExtensionsTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void GetCentroid_WithPoints_AveragesAllThreeCoordinates(bool useReadOnlyList)
    {
        var items = new[]
        {
            new PointXYZ(0f, 2f, 4f),
            new PointXYZ(10f, -2f, 8f),
            new PointXYZ(-4f, 6f, -6f)
        };
        IReadOnlyList<PointXYZ> list = new List<PointXYZ>(items).AsReadOnly();

        var centroid = useReadOnlyList ? list.GetCentroid() : items.GetCentroid();

        Assert.That(centroid, Is.EqualTo(new PointXYZ(2f, 2f, 2f)));
    }

    [TestCase(false, 0)]
    [TestCase(false, 1)]
    [TestCase(false, 2)]
    [TestCase(true, 0)]
    [TestCase(true, 1)]
    [TestCase(true, 2)]
    public void GetClosestToCentroid_WithPositionedObjects_ReturnsOriginalClosestItem(bool useReadOnlyList, int closestIndex)
    {
        var center = new PositionedItem(new PointXYZ(1f, 2f, 3f));
        var values = new List<PositionedItem>
        {
            new PositionedItem(new PointXYZ(1f, 2f, 11f)),
            new PositionedItem(new PointXYZ(1f, 2f, -5f))
        };
        values.Insert(closestIndex, center);
        var items = values.ToArray();
        IReadOnlyList<PositionedItem> list = values.AsReadOnly();

        var centroid = useReadOnlyList ? list.GetCentroid() : items.GetCentroid();
        var closest = useReadOnlyList ? list.GetClosestToCentroid() : items.GetClosestToCentroid();

        Assert.Multiple(() =>
        {
            Assert.That(centroid, Is.EqualTo(center.Position));
            Assert.That(closest, Is.SameAs(center));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GetClosestTo_WithPoints_UsesAllThreeCoordinates(bool useReadOnlyList)
    {
        var items = new[]
        {
            new PointXYZ(9f, 1f, -10f),
            new PointXYZ(10f, 1f, 10f),
            new PointXYZ(9f, 8f, 10f)
        };
        IReadOnlyList<PointXYZ> list = new List<PointXYZ>(items).AsReadOnly();
        var target = new PointXYZ(9f, 1f, 10f);

        var closest = useReadOnlyList ? list.GetClosestTo(target) : items.GetClosestTo(target);

        Assert.That(closest, Is.EqualTo(items[1]));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void NearestItemMethods_WhenDistancesAreEqual_ReturnFirstItem(bool useReadOnlyList)
    {
        var items = new[]
        {
            new PositionedItem(new PointXYZ(1f, 2f, -3f)),
            new PositionedItem(new PointXYZ(1f, 2f, 3f))
        };
        IReadOnlyList<PositionedItem> list = new List<PositionedItem>(items).AsReadOnly();
        var target = new PointXYZ(1f, 2f, 0f);

        var closestToPoint = useReadOnlyList ? list.GetClosestTo(target) : items.GetClosestTo(target);
        var closestToCentroid = useReadOnlyList ? list.GetClosestToCentroid() : items.GetClosestToCentroid();

        Assert.Multiple(() =>
        {
            Assert.That(closestToPoint, Is.SameAs(items[0]));
            Assert.That(closestToCentroid, Is.SameAs(items[0]));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Methods_WithSinglePoint_ReturnThatPoint(bool useReadOnlyList)
    {
        var point = new PointXYZ(1f, -2f, 3f);
        var items = new[] { point };
        IReadOnlyList<PointXYZ> list = new List<PointXYZ>(items).AsReadOnly();

        var centroid = useReadOnlyList ? list.GetCentroid() : items.GetCentroid();
        var closestToCentroid = useReadOnlyList ? list.GetClosestToCentroid() : items.GetClosestToCentroid();
        var closestToPoint = useReadOnlyList ? list.GetClosestTo(default) : items.GetClosestTo(default);

        Assert.Multiple(() =>
        {
            Assert.That(centroid, Is.EqualTo(point));
            Assert.That(closestToCentroid, Is.EqualTo(point));
            Assert.That(closestToPoint, Is.EqualTo(point));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GetCentroid_WithLargeFiniteCoordinates_DoesNotOverflow(bool useReadOnlyList)
    {
        var point = new PointXYZ(float.MaxValue, -float.MaxValue, float.MaxValue);
        var items = new[] { point, point };
        IReadOnlyList<PointXYZ> list = new List<PointXYZ>(items).AsReadOnly();

        var centroid = useReadOnlyList ? list.GetCentroid() : items.GetCentroid();

        Assert.That(centroid, Is.EqualTo(point));
    }

    [TestCase(false, float.MaxValue)]
    [TestCase(true, float.MaxValue)]
    [TestCase(false, float.Epsilon)]
    [TestCase(true, float.Epsilon)]
    public void GetClosestTo_WithExtremeFiniteCoordinates_ReturnsNearestItem(bool useReadOnlyList, float coordinate)
    {
        var items = new[]
        {
            new PointXYZ(-coordinate, -coordinate, -coordinate),
            new PointXYZ(0f, 0f, 0f)
        };
        IReadOnlyList<PointXYZ> list = new List<PointXYZ>(items).AsReadOnly();
        var target = new PointXYZ(coordinate, coordinate, coordinate);

        var closest = useReadOnlyList ? list.GetClosestTo(target) : items.GetClosestTo(target);

        Assert.That(closest, Is.EqualTo(items[1]));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GetClosestToCentroid_WithLargeFiniteCoordinates_ReturnsNearestItem(bool useReadOnlyList)
    {
        var items = new[]
        {
            new PointXYZ(float.MaxValue, float.MaxValue, float.MaxValue),
            new PointXYZ(0f, 0f, 0f),
            new PointXYZ(-float.MaxValue, -float.MaxValue, -float.MaxValue)
        };
        IReadOnlyList<PointXYZ> list = new List<PointXYZ>(items).AsReadOnly();

        var closest = useReadOnlyList ? list.GetClosestToCentroid() : items.GetClosestToCentroid();

        Assert.That(closest, Is.EqualTo(items[1]));
    }

    [Test]
    public void Methods_WhenItemsAreNull_ThrowArgumentNullException()
    {
        PositionedItem[] items = null!;
        IReadOnlyList<PositionedItem> list = null!;

        Assert.Multiple(() =>
        {
            Assert.That(() => items.GetCentroid(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetCentroid(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("items"));
            Assert.That(() => items.GetClosestToCentroid(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetClosestToCentroid(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("items"));
            Assert.That(() => items.GetClosestTo(default), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetClosestTo(default), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("items"));
        });
    }

    [Test]
    public void Methods_WhenItemsAreEmpty_ThrowArgumentException()
    {
        var items = Array.Empty<PositionedItem>();
        IReadOnlyList<PositionedItem> list = new List<PositionedItem>().AsReadOnly();

        Assert.Multiple(() =>
        {
            Assert.That(() => items.GetCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => items.GetClosestToCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetClosestToCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => items.GetClosestTo(default), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetClosestTo(default), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    public void Methods_WhenItemsContainNull_ThrowArgumentException(int nullIndex)
    {
        var items = new[] { new PositionedItem(default), new PositionedItem(default) };
        items[nullIndex] = null!;
        IReadOnlyList<PositionedItem> list = new List<PositionedItem>(items).AsReadOnly();

        Assert.Multiple(() =>
        {
            Assert.That(() => items.GetCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => items.GetClosestToCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetClosestToCentroid(), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => items.GetClosestTo(default), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
            Assert.That(() => list.GetClosestTo(default), Throws.ArgumentException.With.Property("ParamName").EqualTo("items"));
        });
    }

    private sealed class PositionedItem : IHasPosition3D
    {
        public PositionedItem(PointXYZ position)
        {
            Position = position;
        }

        public PointXYZ Position { get; }
    }
}
