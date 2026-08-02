# Растеризация

Растеризация дискретизирует непрерывную двумерную геометрию или поле на прямоугольной сетке и
сохраняет одно значение для каждой ячейки. Она связывает геометрическую модель Spatial2D с
изображениями, масками, картами высот, полями расстояний и другими регулярными данными.

В Spatial2D растеризация разделена на три независимых решения:

```text
Геометрия или поле
        |
        v
RasterGeometry: границы в мировых координатах, разрешение, центры ячеек
        |
        v
Растеризатор или selector: исходное значение -> значение ячейки
        |
        v
SpatialRaster<TValue> -> необязательное преобразование -> PNG или BMP
```

Типы сеток, растров и растеризаторов находятся в пространстве имён
<xref:Akeldov.Math.Spatial2D.Rasterization>. Цвета и методы экспорта изображений находятся в
<xref:Akeldov.Math.Spatial2D.Imaging>.

## Задание сетки выборки

<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> описывает выровненный по осям
прямоугольник в мировых координатах и его разрешение в ячейках:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var grid = new RasterGeometry(
    origin: new PointXY(-1f, -1f),
    size: new VectorXY(6f, 4f),
    resolution: new VectorXYInt(600, 400));

VectorXY cellSize = grid.CellSize;       // (0.01, 0.01) в мировых единицах
PointXY firstSample = grid.GetCellCenter(0, 0);
```

`Origin` — левый нижний угол. Ячейка `(0, 0)` тоже находится слева внизу, но источник
вычисляется в её центре, а не в углу. `CellSize` равен `Size / Resolution` независимо по каждой
оси.

Если удобнее задавать плотность пикселей, а не точное разрешение, передайте
`minimumPixelsPerUnit`. Разрешение будет округлено вверх независимо по каждой оси:

```csharp
var densityGrid = new RasterGeometry(
    cornerA: new PointXY(5f, 3f),
    cornerB: new PointXY(-1f, -1f),
    minimumPixelsPerUnit: 100);
```

Порядок углов не важен. Координаты границ должны быть конечными, ширина и высота —
положительными. Разрешение и плотность пикселей также должны быть положительными.

## Выбор типа растра

Тип значения растра является обобщённым: ячейка может хранить цвет, число, метку, логическую
маску или прикладной тип.

| Тип | Пространственные границы | Когда использовать |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Rasterization.IRaster`1> | Нет | Чтение прямоугольной сетки значений. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.Raster`1> | Нет | Изменяемые значения, когда важны только разрешение и индексы. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRaster`1> | Есть | Чтение растра, ячейки которого должны оставаться привязанными к мировым координатам. |
| <xref:Akeldov.Math.Spatial2D.Rasterization.SpatialRaster`1> | Есть | Изменяемый результат пространственных растеризаторов и полей. |

Конкретные растры сохраняют переданный конструктору массив и предоставляют его через `Values`.
В массиве должно быть ровно `Resolution.X * Resolution.Y` элементов. Значения хранятся по
строкам, поэтому плоский индекс ячейки `(x, y)` равен `y * Resolution.X + x`.

Используйте пространственный растр, пока последующим операциям нужны исходные границы или центры
ячеек. `SpatialRaster<TValue>` также является `Raster<TValue>`, поэтому его можно напрямую
передавать экспорту изображений и алгоритмам, которым достаточно `IRaster<TValue>`.

## Выбор способа растеризации

Начните с данных, которые предоставляет источник:

| Источник или задача | Способ |
|---|---|
| Любое <xref:Akeldov.Math.Spatial2D.Fields.IField`1> | Вычислить поле в центре каждой ячейки и преобразовать значение через selector. |
| Точка, кривая или другой источник обычного расстояния | Преобразовать неотрицательное ближайшее расстояние в оттенок серого. |
| [Контур](geometry-model/contours.md) или [регион](geometry-model/regions.md) | Преобразовать знаковое расстояние: отрицательное внутри, ноль на границе, положительное снаружи. |
| Параметризованная кривая | Использовать расстояние и координату на кривой, например для градиента вдоль пути. |
| Несколько цветных геометрических объектов | Собрать слои на основе расстояний в `GeometryScene<TColor>`. |
| Собственная пара источника и значения | Реализовать <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRasterizer`2>. |

<xref:Akeldov.Math.Spatial2D.Rasterization.IRasterizer`2> создаёт непространственный
`Raster<TValue>` по разрешению. <xref:Akeldov.Math.Spatial2D.Rasterization.ISpatialRasterizer`2>
принимает `RasterGeometry` и создаёт `SpatialRaster<TValue>`.

## Растеризация обводки кривой

Вспомогательные методы кривых преобразуют расстояние в обводку с настраиваемым затуханием за её
краем:

```csharp
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var segment = new Segment(
    new PointXY(0f, 0f),
    new PointXY(4f, 2f));

SpatialRaster<Gray8BitColor> stroke = segment.Rasterize(
    curveWidth: 0.12f,
    fadeDistance: 0.04f,
    curveColor: Gray8BitColor.White,
    backgroundColor: Gray8BitColor.Black,
    rasterGeometry: grid);

stroke.SaveAsPng("segment.png");
```

`curveWidth` — полная ширина линии в мировых единицах. `fadeDistance` — неотрицательное
расстояние за краем линии, на котором результат переходит в цвет фона. Для набора кривых в каждой
ячейке используется расстояние до ближайшей кривой.

Сетка делает одну выборку в центре каждой ячейки. Повышение разрешения сохраняет более мелкие
детали; полоса затухания визуально сглаживает края, но не изменяет исходную геометрию.

## Создание маски по знаковому расстоянию

Знаковое расстояние сохраняет различие между внутренней и внешней областями. Его можно
преобразовать в бинарную маску, мягкое покрытие или визуализацию расстояния:

```csharp
using Akeldov.Math.Spatial2D.Regions;

var disk = new Disk(new PointXY(2f, 1f), radius: 0.75f);

SpatialRaster<Gray8BitColor> mask = disk.Rasterize(
    signedDistance => signedDistance <= 0f
        ? Gray8BitColor.White
        : Gray8BitColor.Black,
    grid);

mask.SaveAsPng("disk-mask.png");
```

Для коллекции встроенный растеризатор использует минимальное знаковое расстояние. Точка находится
внутри объединения, если хотя бы один источник имеет отрицательное расстояние. Сохраняйте исходное
расстояние в `SpatialRaster<float>`, когда следующим операциям нужны смещения, пороги столкновений
или собственные функции преобразования, а не закодированное изображение.

Отдельный сценарий разобран в руководстве
[«Растеризация поля знаковых расстояний»](../how-to-guides/rasterization/rasterize-a-signed-distance-field.md).

## Растеризация полей и преобразование значений

Любое [поле](fields.md) можно вычислить на сетке. Selector преобразует значение предметной области
в нужный тип ячейки:

```csharp
SpatialRaster<Gray8BitColor> image = field.Rasterize(
    grid,
    value => Gray8BitColor.FromNormalized(
        (value - field.Min) / (field.Max - field.Min)));
```

Растеризация поля посещает центры ячеек по строкам и возвращает новый изменяемый растр,
принадлежащий вызывающему коду. Для диапазона нулевой ширины вместо показанной нормализации нужно
задать отдельное прикладное преобразование.

`MapValues` преобразует существующий растр, не меняя разрешение. При вызове для
`ISpatialRaster<TValue>` метод также сохраняет `Geometry` и возвращает новый
`SpatialRaster<TResult>`:

```csharp
SpatialRaster<bool> occupied = mask.MapValues(color => color.Value != 0);
```

Новый массив значений является изменяемым и принадлежит вызывающему коду.

## Сборка геометрической сцены

<xref:Akeldov.Math.Spatial2D.Rasterization.GeometryScene`1> объединяет несколько источников в один
буфер. Слои вычисляются в порядке добавления. У каждого слоя есть функция смешивания; слои,
созданные вспомогательными методами сцены, используют её функцию по умолчанию.

```csharp
using Akeldov.Math.Spatial2D.Imaging;

RGBA16BitColor background = RGBA16BitColor.FromNormalized(1f, 1f, 1f, 1f);
RGBA16BitColor fill = RGBA16BitColor.FromNormalized(0.1f, 0.4f, 0.9f, 0.35f);
RGBA16BitColor outline = RGBA16BitColor.FromNormalized(0.05f, 0.08f, 0.15f, 1f);

SpatialRaster<RGBA16BitColor> sceneRaster =
    new GeometryScene<RGBA16BitColor>(background, RGBA16BitColor.AlphaOver)
        .AddSignedPointDistanceBasedLayer(disk, fill, edgeFalloff: 0.02f)
        .AddPointDistanceBasedLayer(segment, outline, fillDistance: 0.06f, edgeFalloff: 0.02f)
        .Rasterize(grid);

sceneRaster.SaveAsPng("scene.png");
```

Слои обычного расстояния подходят для точек и открытых кривых. Слои знакового расстояния — для
замкнутых контуров и регионов с семантикой «внутри/снаружи». Слои параметризованной проекции нужны,
когда цвет или ширина меняются вдоль кривой. Сцена может работать и с нецветовыми типами ячеек,
если передать подходящие фон и функцию смешивания.

## Выбор цвета и экспорт

Spatial2D предоставляет четыре типа ячеек изображения:

| Тип ячейки | Каналы | Подходящие задачи |
|---|---|---|
| <xref:Akeldov.Math.Spatial2D.Imaging.Gray8BitColor> | 8-битный серый | Маски и компактные скалярные изображения. |
| <xref:Akeldov.Math.Spatial2D.Imaging.Gray16BitColor> | 16-битный серый | Высоты и расстояния, требующие большей точности. |
| <xref:Akeldov.Math.Spatial2D.Imaging.RGBA8BitColor> | 8 бит на канал RGBA | Обычные цветные изображения с прозрачностью. |
| <xref:Akeldov.Math.Spatial2D.Imaging.RGBA16BitColor> | 16 бит на канал RGBA | Высокоточная композиция и градиенты. |

`SaveAsPng` поддерживает все четыре типа цвета и записывает данные в файл или поток. `SaveAsBmp`
поддерживает 8-битный серый и 8-битный RGBA. Экспорт принимает `IRaster<TColor>`: в изображение
попадают разрешение и значения ячеек, а пространственная `Geometry` остаётся метаданными приложения.

## Практические правила

- Сначала задавайте границы в мировых единицах, затем выбирайте разрешение по минимальной детали,
  которую нужно сохранить.
- Считайте `RasterGeometry` частью контракта данных, если значения растра должны отображаться
  обратно в пространство.
- Используйте знаковое расстояние только для источников с содержательной семантикой
  «внутри/снаружи».
- Для композиции и сохранения точности скалярных значений предпочитайте 16 бит; переходите к 8
  битам, когда этого требует формат результата или ограничение памяти.
- Помните, что конструкторы сохраняют переданный массив. Сначала скопируйте его, если последующие
  изменения со стороны исходного владельца не должны менять растр.

Продолжите с концепциями [полей](fields.md), [кривых](geometry-model/curves.md),
[контуров](geometry-model/contours.md) и [регионов](geometry-model/regions.md) или перейдите к
[руководству по растеризации знакового расстояния](../how-to-guides/rasterization/rasterize-a-signed-distance-field.md).
