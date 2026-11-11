# Пространственные растры

Пространственный растр связывает прямоугольную сетку значений с
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>: началом `Origin`, размером `Size`
в мировых координатах и разрешением `Resolution`. `CellSize` и `GetCellCenter(x, y)` связывают
индексы ячеек с координатами. Такие растры подходят для полей, карт расстояний, масок и рисунков.

Общий интерфейс <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRaster`1> расширяет `IRaster<T>`
свойством `Geometry`. `SpatialRaster<T>` — общий контейнер; три специализированных типа
добавляют операторы, сохраняя геометрию и изменяемое построчное хранение.

## Геометрия и владение данными

Конструкторы сохраняют переданный массив. Операции между специализированными пространственными
растрами требуют совпадения начала, размера и разрешения. Смешанные операторы с обычным растром
проверяют разрешение и сохраняют геометрию пространственного операнда. Пересэмплирования нет.

`MapValues`, вызванный через `ISpatialRaster<T>`, сохраняет геометрию и создаёт новый изменяемый
`SpatialRaster<TResult>`, принадлежащий вызывающему коду. Сохраняйте пространственный тип при
выборе перегрузки: если видеть объект только как `IRaster<T>`, результат будет без привязки.

Цвет выбирается независимо: `SpatialRaster<RGBA8BitColor>` одновременно является пространственным
и [цветовым растром](color-rasters.md).

## Типы растров

- [`SpatialRaster<T>`](spatial-rasters/spatial-raster.md)
- [`SpatialBoolRaster`](spatial-rasters/spatial-bool-raster.md)
- [`SpatialIntRaster`](spatial-rasters/spatial-int-raster.md)
- [`SpatialFloatRaster`](spatial-rasters/spatial-float-raster.md)
