# SpatialBoolRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialBoolRaster> предназначен для булевых масок с привязкой к мировым координатам.

## Операции

`!` инвертирует ячейки; `&`, `|` и `^` комбинируют соответствующие значения. `==` и `!=`
возвращают маски сравнения, в том числе при сравнении со скалярным `bool`, а не один `bool`.
Для проверки на null используйте `is null`, для идентичности объектов — `ReferenceEquals`.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(2f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialBoolRaster(grid, new[] { true, false, true, false });
SpatialBoolRaster inverted = !raster;
SpatialBoolRaster selected = raster & inverted;
SpatialBoolRaster same = raster == true;
```

## Хранение и совместимость

Конструктор сохраняет массив. `Values` и индексаторы предоставляют изменяемое состояние.
Операторы создают новые изменяемые результаты, принадлежащие вызывающему коду, и не меняют операнды.

Два пространственных операнда должны иметь одинаковую `Geometry`. Смешанные операторы с обычным
растром требуют одинакового разрешения и сохраняют геометрию пространственного операнда.
Пересэмплирования нет. Неявное преобразование в `SpatialRaster<bool>` создаёт оболочку
с тем же массивом: изменения видны через оба объекта. Для проверки и сохранения геометрии
сохраняйте пространственные типы операндов.

См. [Пространственные растры](../spatial-rasters.md).
