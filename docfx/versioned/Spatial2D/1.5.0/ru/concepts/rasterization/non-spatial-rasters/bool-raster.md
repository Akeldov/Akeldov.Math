# BoolRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.BoolRaster> предназначен для булевых масок без привязки к мировым координатам.

## Операции

`!` инвертирует ячейки; `&`, `|` и `^` комбинируют соответствующие значения. `==` и `!=`
возвращают маски сравнения, в том числе при сравнении со скалярным `bool`, а не один `bool`.
Для проверки на null используйте `is null`, для идентичности объектов — `ReferenceEquals`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var resolution = new VectorXYInt(2, 2);
var raster = new BoolRaster(resolution, new[] { true, false, true, false });
BoolRaster inverted = !raster;
BoolRaster selected = raster & inverted;
BoolRaster same = raster == true;
```

## Хранение и совместимость

Конструктор сохраняет массив. `Values` и индексаторы предоставляют изменяемое состояние.
Операторы создают новые изменяемые результаты, принадлежащие вызывающему коду, и не меняют операнды.

Для двух растровых операндов требуется одинаковое разрешение; автоматического изменения размера нет.

См. [Растры без пространственной привязки](../non-spatial-rasters.md).
