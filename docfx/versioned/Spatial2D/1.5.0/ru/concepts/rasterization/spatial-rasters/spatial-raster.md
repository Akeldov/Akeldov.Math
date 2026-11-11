# SpatialRaster&lt;T&gt;

<xref:Akeldov.Math.Spatial2D.Rasterization.SpatialRaster`1> расширяет `Raster<T>` свойством
`Geometry` и реализует `ISpatialRaster<T>`. Он хранит значения, выбранные в известном
прямоугольнике мировых координат.

## Создание пространственного буфера

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    new PointXY(0f, 0f), new VectorXY(4f, 2f), new VectorXYInt(2, 2));
var raster = new SpatialRaster<float>(grid, new[] { 1f, 2f, 3f, 4f });
PointXY center = raster.Geometry.GetCellCenter(0, 0);
SpatialRaster<bool> mask = raster.MapValues(value => value >= 3f);
```

Конструктор сохраняет переданный массив. Начало и размер должны быть конечными, размер и
разрешение — положительными, длина массива — равной числу ячеек. Унаследованные индексаторы
изменяют построчный буфер `Values`; `Geometry` остаётся фиксированной.

## Выборка и преобразование

`grid.Rasterize(point => ...)` выбирает значения функции в центрах ячеек. Растеризаторы полей
и геометрии также возвращают `SpatialRaster<T>`. Для входного `ISpatialRaster<T>` метод
`MapValues` сохраняет геометрию и создаёт новый изменяемый результат, принадлежащий вызывающему коду.

`SpatialRaster<float>` хранит числа, но сам не добавляет арифметические операторы. Для операторов
используйте [SpatialFloatRaster](spatial-float-raster.md), [SpatialIntRaster](spatial-int-raster.md)
или [SpatialBoolRaster](spatial-bool-raster.md). Эти классы реализуют `ISpatialRaster<T>`
и неявно преобразуются в общий пространственный контейнер: создаётся оболочка с тем же массивом.

См. [пространственные растры](../spatial-rasters.md) и [цветовые растры](../color-rasters.md).
