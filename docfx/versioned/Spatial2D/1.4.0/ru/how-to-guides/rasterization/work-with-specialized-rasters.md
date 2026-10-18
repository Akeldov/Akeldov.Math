# Работа с числовыми и логическими растрами

Специализированные растры удобны для поэлементных операций. Они сохраняют построчное хранение
и изменяемые индексаторы `Raster<T>`; в массиве первым изменяется X.

## Сложение и вычитание числовых значений

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Rasterization;

var resolution = new VectorXYInt(2, 2);
var heights = new FloatRaster(resolution, new[] { 1f, 2f, 3f, 4f });
var offsets = new Raster<float>(resolution, new[] { 0.5f, 1f, 1.5f, 2f });
FloatRaster adjusted = heights + offsets - 0.5f;
FloatRaster inverted = -heights;

var counts = new IntRaster(resolution, new[] { 1, 2, 3, 4 });
IntRaster remaining = 10 - counts;
```

`FloatRaster` и `IntRaster` поддерживают `+`, бинарный `-` и унарный `-`. Скаляры допустимы
с обеих сторон; смешанные растровые операнды должны иметь одинаковые тип значений и разрешение.
Каждая операция создаёт новый изменяемый растр во владении вызывающего, сохраняя входы.
Целочисленная арифметика использует unchecked-семантику переполнения.

## Объединение логических масок

```csharp
var visible = new BoolRaster(resolution, new[] { true, false, true, false });
var blocked = new Raster<bool>(resolution, new[] { false, false, true, true });
BoolRaster available = visible & (visible != blocked);
BoolRaster hidden = !visible;
BoolRaster same = visible == blocked;
```

Логические растры поддерживают `!`, `&`, `|`, `^`, `==` и `!=`. Равенство и неравенство
сравнивают ячейки и возвращают маску, а не одно логическое значение или сравнение ссылок.
Для проверки null используйте `raster is null`, для сравнения объектов — `ReferenceEquals`.
Сравнения со скаляром, например `visible == true`, тоже возвращают маски.

## Сохранение геометрии и общая generic-обёртка

```csharp
var grid = new RasterGeometry(new PointXY(0f, 0f), new VectorXY(2f, 2f), resolution);
var spatialHeights = new SpatialFloatRaster(grid, new[] { 1f, 2f, 3f, 4f });
SpatialFloatRaster shifted = spatialHeights + offsets;
var spatialCounts = new SpatialIntRaster(grid, new[] { 1, 2, 3, 4 });
SpatialIntRaster increased = spatialCounts + counts;
var spatialMask = new SpatialBoolRaster(grid, new[] { true, true, false, false });
SpatialBoolRaster selected = spatialMask & visible;

SpatialRaster<float> shared = spatialHeights;
shared[0, 0] = 10f;
Console.WriteLine(spatialHeights[0, 0]); // 10
```

Операции между пространственными операндами требуют равенства `RasterGeometry`, включая начало,
размер и разрешение. Перегрузки с непространственным операндом проверяют разрешение и сохраняют
геометрию пространственного операнда. Повторной дискретизации входов нет.

Неявные преобразования специализированных пространственных типов в `SpatialRaster<T>` создают
обёртку с общим массивом значений. Конструкторы также сохраняют переданный массив. Если нужны
независимые данные, клонируйте массив: `Values` возвращает изменяемое состояние, а не копию.
Сохраняйте пространственные типы операндов, когда геометрию нужно проверять и переносить:
приведение к `Raster<T>` выбирает непространственные перегрузки.

## Растеризация расстояний в значения

```csharp
var segment = new Segment(new PointXY(0f, 0f), new PointXY(2f, 0f));
SpatialRaster<float> distances = segment.Rasterize(distance => distance, grid);
SpatialRaster<bool> nearSegment = segment.Rasterize(distance => distance < 1f, grid);
var distanceValues = new SpatialFloatRaster(grid, distances.Values);
SpatialFloatRaster offsetDistances = distanceValues - 0.25f;
```

Generic-перегрузка `Rasterize<T, TResult>` берёт выборки в центрах ячеек и отображает расстояния
в мировых единицах в выбранный тип значений. Если объявленный тип источника реализует
`ISignedPointDistanceProvider`, селектор получает отрицательные расстояния внутри и положительные
снаружи; иначе расстояния беззнаковые. Пример использует беззнаковое расстояние до отрезка.
Растеризация создаёт новый изменяемый результат во владении вызывающего; `distanceValues`
затем разделяет массив с `distances`.
