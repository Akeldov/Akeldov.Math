# IndexPartialSeptupletRaster

Окрестность из семи ячеек с флагами HasMain и HasAdjacent0–HasAdjacent5. Все логические индексы сохраняются, даже для отсутствующих позиций. Если Main вне топологии, все флаги сброшены, даже когда сосед находится внутри. Выбирайте допустимые входы фильтра по флагам.

Тип выборки: `PartialSeptuplet<VectorXYInt>`.
Компоненты: `Main, Adjacent0 … Adjacent5`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(8, 6, Layout.OddR);
var source = new HexMapGeometry(topology, new VectorXY(0f, 0f), 1f);
var sampling = source.ToRasterGeometry(pixelsPerApothem: 4f);
var raster = new IndexPartialSeptupletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

Метода `TryGetValue` нет. Проверяйте границы растра перед индексированием.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster>.
