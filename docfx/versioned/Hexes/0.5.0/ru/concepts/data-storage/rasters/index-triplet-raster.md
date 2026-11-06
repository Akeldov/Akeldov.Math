# IndexTripletRaster

Индексы основного гекса и двух гексов у ближайшей вершины. Подходят для получения значений карты при интерполяции. Результат описывает бесконечную сетку, включая индексы вне конечной карты.

Тип выборки: `Triplet<VectorXYInt>`.
Компоненты: `Main, Left, Right`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(topology, new VectorXY(0f, 0f), 1f);
var sampling = source.ToRasterGeometry(pixelsPerApothem: 4f);
var raster = new IndexTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` проверяет границы растра; успех не гарантирует принадлежность конечной карте.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster>.
