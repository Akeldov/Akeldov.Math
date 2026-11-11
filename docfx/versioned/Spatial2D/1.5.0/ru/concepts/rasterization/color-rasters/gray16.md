# Raster&lt;Gray16BitColor&gt;

Используйте `Raster<Gray16BitColor>` для буфера изображения, а `SpatialRaster<Gray16BitColor>` —
для того же формата пикселей с мировыми границами. Тип ячейки — <xref:Akeldov.Math.Spatial2D.Imaging.Gray16BitColor>.
Он хранит `Value` в целочисленном диапазоне `0–65535`.

Нулевая интенсивность означает чёрный цвет, максимальная — белый. Альфа-канала нет;
чёрная ячейка не становится автоматически прозрачной.

Подходит для изображений карт высот и плавных градиентов. Повышенная точность уменьшает
квантование, но для физических величин всё равно нужен явный диапазон; возможна потеря данных.

## Создание, сохранение и загрузка

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = Gray16BitColor.FromNormalized(0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<Gray16BitColor>(resolution, new[] { pixel, Gray16BitColor.White });
image.SaveAsPng("gray16.png");
Raster<Gray16BitColor> loaded = RasterImageLoader.LoadGray16FromPng("gray16.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<Gray16BitColor>(grid, loaded.Values);
```

PNG сохраняет 16-битные каналы. Прямой перегрузки BMP нет; сначала преобразуйте значения в 8-битный цветовой тип.

## Операции и хранение

`FromNormalized` ограничивает диапазон и округляет нормализованные каналы.
`Blend(from, to, amount)` интерполирует значения каналов.

Конструкторы сохраняют массивы; `spatial` и `loaded` в примере используют общие пиксели.
Загрузка не восстанавливает мировые границы: в примере они заданы явно. Для независимого
хранения скопируйте `Values`. `MapValues` создаёт новый буфер с преобразованными цветами.

См. [цветовые растры](../color-rasters.md).
