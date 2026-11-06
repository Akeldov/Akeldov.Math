# IndexPartialTripletRaster

Тройка индексов с флагами HasMain, HasLeft и HasRight для принадлежности исходной топологии. Отсутствующие позиции содержат значения по умолчанию. Проверяйте флаги перед обращением к карте: (0, 0) также является допустимым индексом.

Тип выборки: `PartialTriplet<VectorXYInt>`.
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
var raster = new IndexPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` возвращает `false` вне растра или при отсутствии всех позиций исходной карты.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster>.
