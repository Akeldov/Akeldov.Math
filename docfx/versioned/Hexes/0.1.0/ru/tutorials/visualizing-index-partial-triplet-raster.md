# Визуализация IndexPartialTripletRaster

Покажем, как граница конечной гексагональной карты меняет изображение индексных троек.
<xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster> хранит
`PartialTriplet<VectorXYInt>` для каждой ячейки растра. Флаги `HasMain`, `HasLeft` и
`HasRight` указывают, какие из трёх выбранных индексов гексов принадлежат исходной карте.

В примере используются та же геометрия и формула цвета, что и в
[визуализации IndexTripletRaster](visualizing-index-triplet-raster.md).
Меняется только обработка отсутствующих индексов.

## Отрисовка присутствующих индексов

Запустите этот законченный пример отдельно в консольном проекте со ссылкой на Akeldov.Math.Hexes.
Подготовка описана на странице [«Создание проекта»](creating-a-hex-map/creating-a-project.md).

Исходная карта содержит 5 × 4 гекса с раскладкой `OddR` и радиусом от центра до вершины
в одну мировую единицу. По умолчанию центр нулевого гекса — `(apothem, radius)`.
Прямоугольник выборки начинается в мировой точке `(0, 0)`, имеет размер ограничивающего
прямоугольника карты и разрешение 192 × 192 пикселя.

Для каждого пикселя растр выбирает содержащий гекс (`Main`) и двух соседей (`Left` и
`Right`), встречающихся с ним у его ближайшей вершины. Красный, зелёный и синий каналы кодируют
эти три индекса соответственно. Каждый флаг независимо определяет, участвует ли его канал
в расчёте.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.IO;

var hexMapGeometry = new HexMapGeometry(
    width: 5,
    height: 4,
    radius: 1f,
    layout: Layout.OddR);

var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));

var sourceRaster = new IndexPartialTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("index-partial-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(PartialTriplet<VectorXYInt> triplet)
{
    return new RGBA16BitColor(
        ToPresenceChannel(triplet.Main, triplet.HasMain),
        ToPresenceChannel(triplet.Left, triplet.HasLeft),
        ToPresenceChannel(triplet.Right, triplet.HasRight),
        ushort.MaxValue);
}

static ushort ToPresenceChannel(VectorXYInt index, bool hasValue)
{
    return hasValue
        ? ToChannel(EncodeIndex(index))
        : (ushort)0;
}

static float EncodeIndex(VectorXYInt index)
{
    return 0.08f + 0.075f * (index.X + 1) + 0.12f * (index.Y + 1);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![IndexPartialTripletRaster: каналы RGB отсутствующих индексов обнулены](~/assets/hexes/rasters/index-partial-triplet-raster-odd-r-rgba16.png)

## Почему граница выглядит иначе

Там, где все три выбранных гекса принадлежат карте, цвета совпадают с полным растром.
У границы отсутствующий `Main`, `Left` или `Right` обнуляет только свой канал RGB.
Так появляются насыщенные граничные цвета на изображении.

В примере альфа-канал всегда равен `ushort.MaxValue`. Поэтому отсутствующие индексы **не**
делают пиксель прозрачным: если нет ни одного присутствующего индекса, пиксель непрозрачно-чёрный.
При отсутствующем `Main` ещё могут отображаться присутствующие `Left` или `Right`:
такой пиксель не отбрасывается целиком.

`EncodeIndex` использует ту же иллюстративную формулу интенсивности по X/Y, что и полный пример.
`ToChannel` ограничивает и квантует интенсивность, но не меняет наличие индексов.
Барицентрические веса и повторная нормализация здесь не используются.

## Проверка наличия при чтении карты

Координата ячейки растра и сохранённый индекс гекса относятся к разным сеткам. Допустимый пиксель
может содержать частичную или полностью отсутствующую окрестность гексов. Проверяйте
соответствующий флаг, прежде чем использовать компонент для чтения ограниченной исходной карты.

Не считайте индекс `(0, 0)` признаком отсутствия: это также допустимый индекс гекса.
Наличие определяется флагами `HasMain`, `HasLeft` и `HasRight`, а не сохранёнными
координатами. Растр отмечает позиции вне карты как отсутствующие, но не переносит их
на противоположную сторону и не заменяет граничными ячейками.

Безопасное чтение значений рассмотрено на странице
[«Обработать частичные окрестности»](../how-to-guides/rasters/handle-partial-neighborhoods.md).
Чтобы интерполировать значения, а не показывать индексы, перейдите к странице
[«Создать барицентрический растр»](../how-to-guides/rasters/create-a-barycentric-raster.md).
