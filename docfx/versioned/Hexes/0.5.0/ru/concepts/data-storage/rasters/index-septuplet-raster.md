# IndexSeptupletRaster

Основной гекс и шесть соседей в порядке рёбер библиотеки. Подходит для фильтров по семи ячейкам и клеточных правил. Индексы вне конечной карты сохраняются. Эта окрестность не является треугольником интерполяции.

Тип выборки: `Septuplet<VectorXYInt>`.
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
var raster = new IndexSeptupletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

Метода `TryGetValue` нет. Проверяйте границы растра перед индексированием.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletRaster>.
