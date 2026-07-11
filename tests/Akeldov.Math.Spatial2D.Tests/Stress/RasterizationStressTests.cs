using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

namespace Akeldov.Math.Spatial2D.Tests.Stress;

[Explicit("Stress tests are excluded from normal test runs.")]
[Category("Stress")]
public class RasterizationStressTests
{
    [Test]
    public void RasterizeRegionSignedDistance_WithLargeGrid_ProducesFiniteMappedValues()
    {
        IContourBasedRegion region = new ContourBasedRegion(new IContour[]
        {
            new Rectangle(new PointXY(0f, 0f), new PointXY(200f, 200f)).ToContour(),
            new Rectangle(new PointXY(75f, 75f), new PointXY(125f, 125f)).ToContour()
        });
        var grid = new SpatialRasterGrid(
            new PointXY(-25f, -25f),
            new VectorXY(250f, 250f),
            new VectorXYInt(512, 512));
        int mappedValueCount = 0;
        bool sawInsideDistance = false;
        bool sawOutsideDistance = false;
        var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(distance =>
        {
            Assert.That(float.IsNaN(distance), Is.False);
            Assert.That(float.IsInfinity(distance), Is.False);

            mappedValueCount++;
            sawInsideDistance |= distance < 0f;
            sawOutsideDistance |= distance > 0f;

            return distance <= 0f ? ushort.MaxValue : ushort.MinValue;
        });

        SpatialRaster<Gray16BitColor> raster = rasterizer.Rasterize(region, grid);

        Assert.That(raster.Width, Is.EqualTo(grid.Resolution.X));
        Assert.That(raster.Height, Is.EqualTo(grid.Resolution.Y));
        Assert.That(raster.Values, Has.Length.EqualTo(grid.Resolution.X * grid.Resolution.Y));
        Assert.That(mappedValueCount, Is.EqualTo(raster.Values.Length));
        Assert.That(sawInsideDistance, Is.True);
        Assert.That(sawOutsideDistance, Is.True);
    }
}
