# Raster&lt;T&gt;

<xref:Akeldov.Math.Spatial2D.Rasterization.Raster`1> хранит произвольные значения ячеек без мировых
границ. Используйте его, когда достаточно индексов и разрешения. Он реализует `IRaster<T>`
и служит базой для `BoolRaster`, `IntRaster`, `FloatRaster` и `SpatialRaster<T>`.

## Создание и доступ к значениям

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var raster = new Raster<int>(new VectorXYInt(2, 2), new[] { 1, 2, 3, 4 });
raster[1, 0] = 10;
Raster<bool> mask = raster.MapValues(value => value >= 3);
```

Конструктор сохраняет массив. Все три индексатора (`[x, y]`, `[VectorXYInt]` и плоский `[index]`)
читают и меняют один построчный буфер, доступный через `Values`. Для независимого хранения
скопируйте массив перед созданием растра. Недопустимые индексы вызывают исключения.

## Преобразование значений

`MapValues(value => ...)` создаёт новый изменяемый `Raster<TResult>`, принадлежащий вызывающему
коду, с тем же разрешением. Общий контейнер не перегружает числовые и булевы операторы;
для них используйте соответствующий специализированный растр.

См. [растры без привязки](../non-spatial-rasters.md) и [SpatialRaster<T>](../spatial-rasters/spatial-raster.md).
