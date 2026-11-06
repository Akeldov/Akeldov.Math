# BarycentricPartialTripletRaster

Используется вместе с IndexPartialTripletRaster. HasMain, HasLeft и HasRight отмечают веса гексов исходной топологии. Отсутствующие позиции содержат ноль; оставшиеся веса не перенормируются. Их сумма может быть меньше единицы, поэтому перед интерполяцией выберите политику обработки границ.

Тип выборки: `PartialTriplet<float>`.
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
var raster = new BarycentricPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` возвращает `false` вне растра или при отсутствии всех позиций исходной карты.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.BarycentricPartialTripletRaster>.
