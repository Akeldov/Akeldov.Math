# Partition a rectangle into Voronoi regions

`bounds.PartitionByVoronoi(sites)` builds a Voronoi diagram clipped to a rectangle. Each site produces a <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.VoronoiCell>. Its `Region` describes the area occupied by the cell, and `Region.Contours` holds its closed boundaries. Use this geometry to test whether a point belongs to a cell, fill the cell, or draw its outlines.

## How weights shape the boundary

Pass `PointXY` sites to give every site unit weight. Each point in the rectangle belongs to its nearest site. Two neighboring cells meet along the perpendicular bisector of the line segment between their sites. The rectangle edges close the outer cells. The dots below mark the sites. The leftmost site lies outside the rectangle and still owns the part closest to it.

![Six sites and polygonal cells. The site to the left of the rectangle owns the blue cell along its edge.](~/assets/spatial2d/voronoi/bounded-exterior-site.png)

Pass `Site` objects to assign weights. For finite positive weights, the site with the smallest distance divided by weight wins. For two different weights, the locus of equal ratios is an Apollonius circle. Clipping against other cells and the rectangle can leave arcs of this circle on the boundary. Below, the central yellow site has weight 1.5 and the lower green site has weight 0.75.

![Straight edges and circular arcs bound the colored cells. The central yellow cell has curved sides.](~/assets/spatial2d/voronoi/weighted-strong-center.png)

## A cell with a hole

The two sites below lie on a horizontal line. The blue cell with weight 1 occupies a disk. The orange cell with weight 2 fills the rest of the rectangle. Its two contours are the outer rectangle and the circle around the hole.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

var bounds = new Rectangle(new PointXY(32, 16), new PointXY(336, 208));
var sites = new[]
{
    new Site(new PointXY(149, 112), weight: 1f),
    new Site(new PointXY(233, 112), weight: 2f)
};

var cells = bounds.PartitionByVoronoi(sites);
var disk = cells[0].Region!;
var surround = cells[1].Region!;

bool inDisk = disk.Contains(sites[0].Position);         // true
bool inSurround = surround.Contains(sites[0].Position); // false
int boundaryCount = surround.Contours.Count;            // 2
```

![A blue disk lies entirely inside the rectangle. The orange region surrounds the disk.](~/assets/spatial2d/voronoi/weighted-circular-hole.png)

## A cell with disconnected components

In the same rectangle, place sites at `(249, 112)` and `(312, 112)` with weights 3 and 4. The blue cell now reaches both the top and bottom edges. The orange cell again has two contours, this time enclosing separate filled components. Both belong to the site on the right. `Region.Contains(point)` accounts for every contour using the even–odd fill rule.

![The blue cell separates two orange components at the left and right edges of the rectangle.](~/assets/spatial2d/voronoi/weighted-disconnected-cell.png)

## Draw the contours

For the example with a hole, fill each region as a whole. Then stroke each contour and mark the sites on top of the fills.

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var raster = new SpatialRaster<RGBA8BitColor>(
    new RasterGeometry(new PointXY(0, 0), new VectorXY(352, 224), new VectorXYInt(352, 224)),
    new RGBA8BitColor[352 * 224]);
var blue = new RGBA8BitColor(173, 204, 231, 255);
var orange = new RGBA8BitColor(247, 202, 157, 255);
var ink = new RGBA8BitColor(40, 53, 68, 255);

raster.Draw(disk, blue);
raster.Draw(surround, orange);
foreach (var cell in cells)
{
    foreach (var contour in cell.Region!.Contours)
        raster.Draw(contour, 1.5f, ink);
    raster.FillCircle(cell.Site, 3.5f, ink);
}
raster.SaveAsPng("voronoi.png");
```

## Empty cells and relaxation

The default `EmptyCellPolicy.LeaveAsIs` keeps cells in site order. A cell with no area inside the rectangle has a null `Region`. For arbitrary inputs, check it before drawing or calling `GetCentroid()`. Set `relaxationIterations` to run Lloyd relaxation. Each step moves every nonempty cell's site to its area centroid, accounting for all components and holes, preserves its weight, and rebuilds the diagram.

To group an existing collection of points or objects by site, see [Voronoi item partitioning](voronoi.md).
