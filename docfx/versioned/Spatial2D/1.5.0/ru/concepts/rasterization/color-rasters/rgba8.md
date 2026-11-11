# Raster&lt;RGBA8BitColor&gt;

Используйте `Raster<RGBA8BitColor>` для буфера изображения, а `SpatialRaster<RGBA8BitColor>` —
для того же формата пикселей с мировыми границами. Тип ячейки — <xref:Akeldov.Math.Spatial2D.Imaging.RGBA8BitColor>.
Он хранит `R`, `G`, `B` и `A` в целочисленном диапазоне `0–255`.

Нулевая альфа означает прозрачность, максимальная — непрозрачность. RGB хранит цвет независимо
от альфы (straight alpha).

Подходит для обычных цветных изображений и наложений. У `SpatialRaster<RGBA8BitColor>` методы
`Draw`, `Fill`, `DrawArrow`, `DrawGrid` и `DrawText` меняют существующий буфер в мировых координатах.
См. [рисование фигур и текста](../../../how-to-guides/rasterization/draw-on-a-raster.md).

## Создание, сохранение и загрузка

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = RGBA8BitColor.FromNormalized(0.2f, 0.5f, 0.9f, 0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<RGBA8BitColor>(resolution, new[] { pixel, RGBA8BitColor.White });
image.SaveAsPng("rgba8.png");
Raster<RGBA8BitColor> loaded = RasterImageLoader.LoadRgba8FromPng("rgba8.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<RGBA8BitColor>(grid, loaded.Values);
```

PNG сохраняет 8-битные каналы. Также доступны экспорт BMP и `LoadRgba8FromBmp`.

## Операции и хранение

`FromNormalized` ограничивает диапазон и округляет нормализованные каналы.
`Blend(from, to, amount)` интерполирует значения каналов, включая альфу; это не наложение source-over.

Конструкторы сохраняют массивы; `spatial` и `loaded` в примере используют общие пиксели.
Загрузка не восстанавливает мировые границы: в примере они заданы явно. Для независимого
хранения скопируйте `Values`. `MapValues` создаёт новый буфер с преобразованными цветами.

См. [цветовые растры](../color-rasters.md).
