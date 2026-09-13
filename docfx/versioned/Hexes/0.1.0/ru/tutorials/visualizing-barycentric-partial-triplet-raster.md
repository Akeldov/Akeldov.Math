# Визуализация BarycentricPartialTripletRaster

Покажем основной барицентрический вес только там, где его гекс принадлежит конечной исходной
карте. <xref:Akeldov.Math.Hexes.Topology.BarycentricPartialTripletRaster> хранит
`PartialTriplet<float>` для каждой ячейки растра: три геометрических веса и флаги их наличия.

В примере используются та же геометрия и раскраска в оттенках серого, что и в
[визуализации BarycentricTripletRaster](visualizing-barycentric-triplet-raster.md).
Он воспроизводит изображение частичного основного веса из примеров геометрических растров.

## Веса и наличие

Растр выбирает содержащий гекс (`Main`) и двух соседей (`Left` и `Right`), встречающихся
с ним у его ближайшей вершины. Три **центра гексов** образуют треугольник интерполяции,
как и в полном растре.

Флаги `HasMain`, `HasLeft` и `HasRight` показывают, какие из этих гексов принадлежат
исходной топологии. Присутствующие позиции сохраняют исходные барицентрические веса;
отсутствующие содержат ноль. Оставшиеся веса **не нормализуются автоматически**, поэтому
их сумма у границы может быть меньше единицы.

Компоненты содержат веса, а не индексы. Если нужны также значения из соответствующих гексов,
используйте согласованный `IndexPartialTripletRaster`.

## Отрисовка присутствующего основного веса

Запустите этот законченный пример в консольном проекте со ссылкой на Akeldov.Math.Hexes.
Подготовка описана на странице [«Создание проекта»](creating-a-hex-map/creating-a-project.md).

Исходная карта содержит 5 × 4 гекса с раскладкой `OddR`. Центр нулевого гекса находится
в мировой точке `(0, 0)`, а радиус от центра до вершины равен одной мировой единице.
Сетка выборки покрывает ограничивающий прямоугольник карты с плотностью 16 пикселей на апофему
без дополнительного отступа. Внутри этого прямоугольника всё равно есть точки за зубчатым
контуром карты.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

var hexGeometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

RasterGeometry rasterGeometry = hexGeometry.ToRasterGeometry(
    pixelsPerApothem: 16f,
    margin: 0f);

var sourceRaster = new BarycentricPartialTripletRaster(
    hexGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("barycentric-partial-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(PartialTriplet<float> barycentric)
{
    float main = barycentric.HasMain ? barycentric.Main : 0f;
    return RGBA16BitColor.FromNormalized(main, main, main);
}
```

![BarycentricPartialTripletRaster: присутствующий вес Main показан серым, отсутствующий Main — чёрным](~/assets/hexes/rasters/barycentric-partial-triplet-raster-odd-r-rgba16.png)

## Сравнение с полным растром

Там, где `HasMain` равен `true`, яркость совпадает с полным изображением основного веса,
даже если `Left` или `Right` отсутствует. Пример показывает исходный вес `Main`,
не перераспределяя вклад отсутствующих соседей.

Там, где `HasMain` равен `false`, функция `ToColor` использует нулевую интенсивность.
По умолчанию `FromNormalized` задаёт полную непрозрачность, поэтому такие пиксели
**непрозрачно-чёрные**, а не прозрачные. Присутствующий `Left` или `Right` не делает
такой пиксель светлее: эта визуализация намеренно не учитывает оба этих веса.

Так изображение показывает контур конечной карты, сохраняя яркие центры гексов и более тёмные
края из полного примера. `MapValues` сохраняет геометрию выборки: отсечение отображаемого веса
не обрезает прямоугольный растр.

## Нормализация, когда она нужна операции

При интерполяции по конечной карте учитывайте только присутствующие исходные значения
и делите результат на сумму их весов, если она положительна:

```text
weightSum = сумма присутствующих весов
value = сумма (присутствующий вес * соответствующее значение карты) / weightSum
```

Это сохраняет постоянное исходное поле у границы. Если `weightSum` равна нулю, явно выберите
результат для отсутствующих данных. Пример в оттенках серого выше намеренно не выполняет
эту нормализацию: он показывает сам сохранённый основной вес.

Создавайте индексный растр и растр весов с теми же `HexMapGeometry` и `RasterGeometry`.
Перед чтением ограниченной карты проверяйте флаги наличия: численный вес, равный нулю,
не заменяет флаг присутствия.

Законченный пример интерполяции с безопасной обработкой границ приведён на странице
[«Создать барицентрический растр»](../how-to-guides/rasters/create-a-barycentric-raster.md).
Чтобы показать соответствующие индексы вместо весов, см.
[«Визуализация IndexPartialTripletRaster»](visualizing-index-partial-triplet-raster.md).
