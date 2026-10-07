# Preparing Topology and the Source Map

Begin with a finite hex topology, its world-space geometry, and one normalized elevation value per
hex. Later steps will interpolate these values into pixels.

## Create the map

Replace `Program.cs` with the following code:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

const int Width = 9;
const int Height = 7;

var topology = new HexMapTopology(Width, Height, Layout.OddR);
var mapGeometry = new HexMapGeometry(topology, radius: 1f);
var elevationMap = new HexMap<float>(topology);

for (int y = 0; y < Height; y++)
{
    for (int x = 0; x < Width; x++)
    {
        float dx = (x - (Width - 1) / 2f) / (Width - 1);
        float dy = (y - (Height - 1) / 2f) / (Height - 1);
        float hill = 1f - 1.5f * MathF.Sqrt(dx * dx + dy * dy);
        float ridge = 0.15f * MathF.Sin(1.3f * x + 0.7f * y);

        elevationMap[new VectorXYInt(x, y)] = Math.Clamp(hill + ridge, 0f, 1f);
    }
}
```

`HexMapTopology` defines the finite 9×7 index range and the `OddR` relationship between rows.
`HexMapGeometry` places that topology in continuous space. Its radius is one world unit, and its
default origin leaves the complete outer hexes inside the geometry bounds.

The procedural formula is only sample data. Every stored value is clamped to the range from `0`
to `1`, which will make color mapping straightforward. You can replace it with temperatures,
heights, influence values, or any other scalar field.

Continue with [Creating an Index Raster](creating-an-index-raster.md).
