# Визуализация BarycentricTripletRaster

Покажем основной барицентрический вес в оттенках серого.
<xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster> хранит три веса интерполяции
в центре каждой ячейки растра. Пример воспроизводит визуализацию основного веса
из примеров геометрических растров.

## От индексов к весам

Как и [IndexTripletRaster](visualizing-index-triplet-raster.md), растр находит содержащий
гекс и его ближайшую вершину. Содержащий гекс и два соседа, встречающихся с ним у этой вершины,
задают три **центра гексов**, образующие треугольник интерполяции. Его вершины — не углы
отдельного гекса.

Индексный растр хранит `Triplet<VectorXYInt>`, а этот растр — `Triplet<float>`.
Компоненты `Main`, `Left` и `Right` содержат веса соответствующих центров гексов:

```text
P = Main * centerMain + Left * centerLeft + Right * centerRight
Main + Left + Right ≈ 1
```

Веса меняются вместе с точкой выборки, даже пока тройка выбранных индексов гексов остаётся
прежней. Порядок компонентов геометрический, а не хроматический: `Main` не привязан
к постоянному цветовому классу.

## Отрисовка основного веса

Запустите этот законченный пример в консольном проекте со ссылкой на Akeldov.Math.Hexes.
Подготовка описана на странице [«Создание проекта»](creating-a-hex-map/creating-a-project.md).

Исходная карта содержит 5 × 4 гекса с раскладкой `OddR`. Центр нулевого гекса находится
в мировой точке `(0, 0)`, а радиус от центра до вершины равен одной мировой единице.
`ToRasterGeometry` покрывает ограничивающий прямоугольник карты с плотностью 16 пикселей
на апофему без дополнительного отступа; начало растра не обязано совпадать с центром
нулевого гекса.

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

var sourceRaster = new BarycentricTripletRaster(
    hexGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("barycentric-triplet-raster-main-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(Triplet<float> barycentric)
{
    float main = barycentric.Main;
    return RGBA16BitColor.FromNormalized(main, main, main);
}
```

![BarycentricTripletRaster: вес Main показан в оттенках серого](~/assets/hexes/rasters/barycentric-triplet-raster-main-odd-r-rgba16.png)

## Как читать изображение

Одинаковые красный, зелёный и синий каналы дают оттенок серого. `FromNormalized` переводит
`Main` в 16-битные каналы и по умолчанию задаёт полную непрозрачность.
`MapValues` сохраняет геометрию выборки.

- В центре гекса теоретические веса равны `(1, 0, 0)`: основной вес даёт наибольшую яркость.
- При движении к границе гекса основной вес уменьшается.
- В вершине гекса три окружающих центра дают равный вклад: каждый вес равен `1/3`.

Выборки лежат в центрах пикселей, поэтому максимальный вес среди них не обязан быть ровно `1`.
Внутри каждого выбранного треугольника веса меняются линейно. Изображение показывает вес того
гекса, который сейчас занимает позицию `Main`, а не карту высот, размытие или влияние
одного фиксированного гекса.

## Границы и интерполяция

Это **полный** растр: он вычисляет веса на подразумеваемой бесконечной гексагональной сетке,
даже если один или несколько соответствующих индексов гексов выходят за пределы исходной
карты. Поэтому прямоугольное изображение остаётся непрозрачным, в том числе за зубчатым
контуром карты.

Для интерполяции значений карты создайте `IndexTripletRaster` с теми же
`HexMapGeometry` и `RasterGeometry`, а затем объединяйте соответствующие значения и веса.
Одного совпадения разрешений растров недостаточно для согласования выборок.
Не используйте непроверенные индексы за пределами карты для чтения ограниченной `HexMap<T>`.

Для границ конечной карты используйте пару `IndexPartialTripletRaster` и
`BarycentricPartialTripletRaster`, при необходимости нормализуя оставшиеся веса.
Законченный пример интерполяции приведён на странице
[«Создать барицентрический растр»](../how-to-guides/rasters/create-a-barycentric-raster.md).

Чтобы показать все три веса через устойчивые цветовые классы RGB вместо геометрических
позиций `Main/Left/Right`, перейдите к учебнику
[«Хроматизация и барицентрическая интерполяция»](chromatic-barycentric-interpolation.md).
