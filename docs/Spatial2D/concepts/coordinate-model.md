# Coordinate Model

Spatial2D uses a conventional Cartesian coordinate model.

`PointXY` represents a position. `VectorXY` represents an offset or direction. `VectorXYInt` represents integer coordinates, most commonly grid or raster coordinates.

```csharp
using Akeldov.Math.Spatial2D;

var position = new PointXY(3.5f, 8f);
var offset = new VectorXY(1f, 2f);
var cell = new VectorXYInt(3, 8);

PointXY moved = position + offset;
VectorXY displacement = moved - position;
```

Raster grids use world-space `PointXY` and `VectorXY` values for their bounds, and `VectorXYInt` for their cell resolution.

Most geometric distances and curve coordinates are measured in the same world coordinate units as the source points.
