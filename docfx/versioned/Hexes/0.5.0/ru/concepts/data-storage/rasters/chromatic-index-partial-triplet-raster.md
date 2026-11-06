# ChromaticIndexPartialTripletRaster

Хроматические классы с флагами HasMain, HasLeft и HasRight. Позиции вне топологии отсутствуют и содержат ноль. Класс 0 допустим, поэтому определяйте отсутствие только по флагам. Подходит для выборки классов в пределах конечной карты.

Тип выборки: `PartialTriplet<byte>`.
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
var raster = new ChromaticIndexPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

Метода `TryGetValue` нет. Проверяйте границы растра перед индексированием.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexPartialTripletRaster>.
