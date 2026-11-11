# Raster&lt;RGBA16BitColor&gt;

Используйте `Raster<RGBA16BitColor>` для буфера изображения, а `SpatialRaster<RGBA16BitColor>` —
для того же формата пикселей с мировыми границами. Тип ячейки — <xref:Akeldov.Math.Spatial2D.Imaging.RGBA16BitColor>.
Он хранит `R`, `G`, `B` и `A` в целочисленном диапазоне `0–65535`.

Нулевая альфа означает прозрачность, максимальная — непрозрачность. RGB хранит цвет независимо
от альфы (straight alpha).

Подходит для плавных цветовых градиентов и многослойных сцен.
`RGBA16BitColor.AlphaOver(background, foreground)` накладывает передний план на фон,
а `ScaleAlpha(coverage)` масштабирует непрозрачность. `GeometryScene<RGBA16BitColor>`
может использовать `AlphaOver` как функцию смешивания.

## Создание, сохранение и загрузка

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Imaging;

var pixel = RGBA16BitColor.FromNormalized(0.2f, 0.5f, 0.9f, 0.5f);
var resolution = new VectorXYInt(2, 1);
var image = new Raster<RGBA16BitColor>(resolution, new[] { pixel, RGBA16BitColor.White });
image.SaveAsPng("rgba16.png");
Raster<RGBA16BitColor> loaded = RasterImageLoader.LoadRgba16FromPng("rgba16.png");

var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 1f), resolution);
var spatial = new SpatialRaster<RGBA16BitColor>(grid, loaded.Values);
```

PNG сохраняет 16-битные каналы. Прямой перегрузки BMP нет; сначала преобразуйте значения в 8-битный цветовой тип.

## Операции и хранение

`FromNormalized` ограничивает диапазон и округляет нормализованные каналы.
`Blend(from, to, amount)` интерполирует значения каналов, включая альфу; это не наложение source-over.

Конструкторы сохраняют массивы; `spatial` и `loaded` в примере используют общие пиксели.
Загрузка не восстанавливает мировые границы: в примере они заданы явно. Для независимого
хранения скопируйте `Values`. `MapValues` создаёт новый буфер с преобразованными цветами.

См. [цветовые растры](../color-rasters.md).
