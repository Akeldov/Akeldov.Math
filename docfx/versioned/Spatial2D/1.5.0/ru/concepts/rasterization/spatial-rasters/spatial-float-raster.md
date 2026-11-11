# SpatialFloatRaster

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialFloatRaster> предназначен для вещественных полей и расстояний с привязкой к мировым координатам.

## Операции

Поддерживает поэлементные `+`, бинарный `-` и унарный `-`. Сложение и вычитание со скаляром
работают в обоих порядках. Растровые операнды могут быть специализированными или общими
контейнерами с соответствующим типом значения.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(2f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialFloatRaster(grid, new[] { 1f, 2f, 3f, 4f });
SpatialFloatRaster shifted = raster + 0.5f;
SpatialFloatRaster difference = shifted - raster;
SpatialFloatRaster inverted = -raster;
```

## Хранение и совместимость

Конструктор сохраняет массив. `Values` и индексаторы предоставляют изменяемое состояние.
Операторы создают новые изменяемые результаты, принадлежащие вызывающему коду, и не меняют операнды.

Два пространственных операнда должны иметь одинаковую `Geometry`. Смешанные операторы с обычным
растром требуют одинакового разрешения и сохраняют геометрию пространственного операнда.
Пересэмплирования нет. Неявное преобразование в `SpatialRaster<float>` создаёт оболочку
с тем же массивом: изменения видны через оба объекта. Для проверки и сохранения геометрии
сохраняйте пространственные типы операндов.

## Фильтрация и генерация

`GaussianBlur(sigma)` и `GaussianBlur(sigma, radius)` возвращают новый `SpatialRaster<float>`.
Sigma и радиус измеряются в ячейках; на границах веса нормализуются по доступным ячейкам.
`grid.CreatePerlinNoise(seed: 42, scale: 10f)` создаёт новый `SpatialRaster<float>`
со значениями `[0, 1]`. Эти методы возвращают общие контейнеры; для арифметических операторов
оберните их `Values` в `SpatialFloatRaster`, учитывая совместное использование массива.

Масштаб шума измеряется в мировых единицах.

См. [Пространственные растры](../spatial-rasters.md).
