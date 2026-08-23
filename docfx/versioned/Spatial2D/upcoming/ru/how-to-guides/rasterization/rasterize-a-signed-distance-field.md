# Растеризовать знаковое поле расстояний

Знаковое поле хранит кратчайшее расстояние до границы вместе с информацией о нахождении внутри
или снаружи. В Spatial2D значения внутри региона отрицательны, на границе равны нулю, а в
отверстиях и снаружи региона положительны.

В этом руководстве вы создадите квадратный регион с квадратным отверстием, преобразуете расстояния
от `-1` до `1` в 16-битные оттенки серого и сохраните результат в PNG.

## Создайте регион

Добавьте нужные пространства имён и определите два замкнутых контура:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

IRegion region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});
```

`ContourBasedRegion` применяет правило заполнения по чётности. Внешний контур создаёт заполненную
область, а вложенный снова переключает её в пустое состояние и становится отверстием.

После инструкций верхнего уровня добавьте вспомогательный метод:

```csharp
static IContour CreateSquareContour(
    float left,
    float bottom,
    float right,
    float top)
{
    return new CompositeContour(new IContourPath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```

Пути расположены последовательно, а последний заканчивается в начале первого. Именно такую
замкнутую цепочку требует `CompositeContour`. Каждый `IContourPath` поддерживает подсчёт
пересечений для правила заполнения и напрямую объявляет запрос пересечения с лучом, используемый
составным контуром.

## Задайте сетку выборки

Оставьте вокруг внешней границы отступ в половину мировой единицы:

```csharp
var grid = new RasterGeometry(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(320, 320));
```

Сетка покрывает диапазон `[-0.5, 4.5]` по обеим осям. Квадратные мировые границы и квадратное
разрешение сохраняют пропорции геометрии. Каждая выходная ячейка вычисляет регион в своём центре.

## Преобразуйте расстояние в оттенок серого

Преобразуйте `-1` в чёрный цвет, граничное значение `0` — в средний серый, а `1` — в белый.
Более далёкие значения ограничьте концами этого интервала:

```csharp
var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(
    signedDistance =>
    {
        float normalized = Math.Clamp(
            (signedDistance + 1f) / 2f,
            0f,
            1f);

        return new Gray16BitColor(
            (ushort)(normalized * ushort.MaxValue));
    });
```

Функция преобразования управляет только визуализацией. Она не изменяет регион или вычисление
расстояния. Выберите другой интервал, если полезный масштаб расстояний приложения больше или
меньше одной мировой единицы.

## Растеризуйте и экспортируйте

```csharp
SpatialRaster<Gray16BitColor> raster =
    region.Rasterize(grid, rasterizer);

raster.SaveAsPng("signed-distance.png");
```

Возвращаемый растр является новым, изменяемым и принадлежит вызывающему коду. Через свойство
`Geometry` он сохраняет `grid`, поэтому ячейки по-прежнему можно связать с мировыми координатами.
Экспорт PNG записывает разрешение и оттенки серого, но не встраивает мировые границы.

## Полный код

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using Akeldov.Math.Spatial2D.Regions;

IRegion region = new ContourBasedRegion(new IContour[]
{
    CreateSquareContour(0f, 0f, 4f, 4f),
    CreateSquareContour(1f, 1f, 3f, 3f)
});

var grid = new RasterGeometry(
    origin: new PointXY(-0.5f, -0.5f),
    size: new VectorXY(5f, 5f),
    resolution: new VectorXYInt(320, 320));

var rasterizer = new SignedPointDistanceProviderGray16BitRasterizer(
    signedDistance =>
    {
        float normalized = Math.Clamp(
            (signedDistance + 1f) / 2f,
            0f,
            1f);

        return new Gray16BitColor(
            (ushort)(normalized * ushort.MaxValue));
    });

SpatialRaster<Gray16BitColor> raster =
    region.Rasterize(grid, rasterizer);

raster.SaveAsPng("signed-distance.png");

static IContour CreateSquareContour(
    float left,
    float bottom,
    float right,
    float top)
{
    return new CompositeContour(new IContourPath[]
    {
        new ParameterizedSegment(new PointXY(left, bottom), new PointXY(right, bottom)),
        new ParameterizedSegment(new PointXY(right, bottom), new PointXY(right, top)),
        new ParameterizedSegment(new PointXY(right, top), new PointXY(left, top)),
        new ParameterizedSegment(new PointXY(left, top), new PointXY(left, bottom))
    });
}
```

Если нужна только маска, используйте бинарный selector вроде `distance <= 0f`. Сохраняйте или
создавайте значения расстояния с плавающей точкой, когда следующему этапу требуются пороги,
смещения, зазоры столкновений или другая функция преобразования.

Модель растров и сеток подробно описана в разделе
[«Растеризация»](../../concepts/rasterization.md).
