# Raster&lt;Gray8BitColor&gt;

Используйте `Raster<Gray8BitColor>` для буфера изображения, а `SpatialRaster<Gray8BitColor>` —
для того же формата пикселей с мировыми границами. Тип ячейки — <xref:Akeldov.Math.Spatial2D.Imaging.Gray8BitColor>.
Он хранит `Value` в целочисленном диапазоне `0–255`.

Нулевая интенсивность означает чёрный цвет, максимальная — белый. Альфа-канала нет;
чёрная ячейка не становится автоматически прозрачной.

Подходит для компактных масок и изображений в оттенках серого. Если дальше нужны логические
операторы над маской, сохраняйте отдельный булев растр.

## Создание, сохранение и загрузка

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = Gray8BitColor.FromNormalized(0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<Gray8BitColor>(resolution, new[] { pixel, Gray8BitColor.White });
image.SaveAsPng("gray8.png");
Raster<Gray8BitColor> loaded = RasterImageLoader.LoadGray8FromPng("gray8.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<Gray8BitColor>(grid, loaded.Values);
```

PNG сохраняет 8-битные каналы. Также доступны экспорт BMP и `LoadGray8FromBmp`.

## Операции и хранение

`FromNormalized` ограничивает диапазон и округляет нормализованные каналы.
`Blend(from, to, amount)` интерполирует значения каналов.

Конструкторы сохраняют массивы; `spatial` и `loaded` в примере используют общие пиксели.
Загрузка не восстанавливает мировые границы: в примере они заданы явно. Для независимого
хранения скопируйте `Values`. `MapValues` создаёт новый буфер с преобразованными цветами.

См. [цветовые растры](../color-rasters.md).
