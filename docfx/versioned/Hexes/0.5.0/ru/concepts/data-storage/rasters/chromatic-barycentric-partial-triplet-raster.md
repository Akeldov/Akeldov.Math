# ChromaticBarycentricPartialTripletRaster

Барицентрические веса в порядке классов с флагами HasIndex0, HasIndex1 и HasIndex2. Веса и флаги переставляются совместно. Отсутствующие позиции содержат ноль; оставшиеся веса не перенормируются. Подходит для смешивания по классам с учётом исходной топологии.

Тип выборки: `PartialChromaticTriplet<float>`.
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
var raster = new ChromaticBarycentricPartialTripletRaster(source, sampling);
var sample = raster[new VectorXYInt(0, 0)];
```

`TryGetValue` проверяет границы растра; успех не гарантирует принадлежность конечной карте. Метод может вернуть `true` при `Presence == None`.

См. [Растры](../rasters.md): геометрия выборки, хранение и общие правила обработки границ.

API: <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster>.
