# ChromaticBarycentricTripletRaster

Барицентрические веса, упорядоченные по хроматическому классу. Компонента соответствует классу, а не Main, Left или Right. Подходит для смешивания трёх каналов по классам. Сумма весов приблизительно равна единице; границы конечной карты их не обрезают.

Тип выборки: `ChromaticTriplet<float>`.
Компоненты: `Index0, Index1, Index2`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(topology, new VectorXY(0f, 0f), 1f);
var sampling = source.ToRasterGeometry(pixelsPerApothem: 4f);
var raster = new ChromaticBarycentricTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` проверяет границы растра; успех не гарантирует принадлежность конечной карте.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster>.
