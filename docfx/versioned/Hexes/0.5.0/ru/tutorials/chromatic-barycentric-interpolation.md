# Хроматизация и барицентрическая интерполяция

Объединим трёхцветную классификацию гексов с барицентрическими координатами, чтобы получить
непрерывное смешивание цветов между центрами ячеек. Хроматизация назначает каждому гексу класс,
а барицентрические координаты определяют вклад каждого из трёх окружающих гексов в точке выборки.

Этот учебник продолжает [«Хроматизацию гексагональной карты»](chromatizing-a-hex-map.md).
Он воспроизводит RGB-визуализацию весов из примеров растеризации Hexes и объясняет порядок
классов и обработку границ. Код выполняется в консольном проекте со ссылкой на Akeldov.Math.Hexes.

## От трёх центров гексов к трём весам

Для центра каждого пикселя растр находит содержащий его гекс и ближайшую вершину этого гекса.
Центры трёх гексов, сходящихся в этой вершине, образуют треугольник. Барицентрические координаты
представляют точку выборки как взвешенную сумму этих центров:

```text
P = wMain * centerMain + wLeft * centerLeft + wRight * centerRight
wMain + wLeft + wRight = 1
```

Вершинами треугольника здесь служат **центры гексов**, а не углы одного гекса.
<xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster> хранит веса в геометрическом порядке
`Main/Left/Right`. При перемещении точки выборки по сетке эти роли меняются.

У трёх гексов разные хроматические классы: `0`, `1` и `2`. Перестановка весов по классу
придаёт каждому выходному каналу постоянный смысл:

| Геометрический компонент | Пример класса | Пример веса | Хроматический компонент |
| --- | --- | --- | --- |
| `Main` | `2` | `0.6` | `Index2 = 0.6` |
| `Left` | `0` | `0.1` | `Index0 = 0.1` |
| `Right` | `1` | `0.3` | `Index1 = 0.3` |

Эту перестановку выполняет
<xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster>.
Внутри он сочетает `BarycentricTripletRaster` с `ChromaticIndexTripletRaster` на одной геометрии
выборки. Его значения имеют тип `ChromaticTriplet<float>`: `Index0`, `Index1` и `Index2` —
это **веса**, а не индексы ячеек или номера классов.

Чтобы увидеть номера классов до перестановки весов, сравните с
[визуализацией ChromaticIndexTripletRaster](visualizing-chromatic-index-triplet-raster.md).

## Цвета классов и смешивание весов

Назначим классу `0` красный цвет, классу `1` — зелёный, классу `2` — синий.
Тогда формула смешивания сводится к `RGB = (Index0, Index1, Index2)`.
Запустите этот законченный пример:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

RasterGeometry sampling = geometry.ToRasterGeometry(pixelsPerApothem: 32f);

var weights = new ChromaticBarycentricTripletRaster(geometry, sampling);
SpatialRaster<RGBA8BitColor> blendedImage = weights.MapValues(
    w => RGBA8BitColor.FromNormalized(w.Index0, w.Index1, w.Index2, alpha: 1f));

var classes = new ChromaticIndexMap(geometry);
SpatialRaster<RGBA8BitColor> classImage = classes.Rasterize(
    pixelsPerApothem: 32f,
    margin: 0f,
    colorSelector: c => RGBA8BitColor.FromNormalized(
        c == 0 ? 1f : 0f,
        c == 1 ? 1f : 0f,
        c == 2 ? 1f : 0f,
        alpha: 1f));

classImage.SaveAsPng(Path.GetFullPath("chromatic-classes.png"));
blendedImage.SaveAsPng(Path.GetFullPath("chromatic-barycentric.png"));
```

Первое изображение окрашивает каждый гекс конечной карты цветом его класса:

![Дискретная RGB-раскраска классов карты OddR размером пять на четыре гекса](~/assets/hexes/chromatization/chromatic-classes.png)

Второе изображение напрямую отображает три упорядоченных по классам веса в RGB:

![Барицентрическое RGB-смешивание с привязкой красного, зелёного и синего к хроматическим классам](~/assets/hexes/chromatization/chromatic-barycentric.png)

В центре гекса его собственный вес равен `1`, а два других — `0`. Посередине между центрами
двух соседних гексов их веса равны `0.5`. В общей вершине гексов все три веса равны `1/3`,
что даёт одинаковые RGB-каналы и серый цвет. Центры пикселей обычно лишь приближают эти точные
положения.

Переходы кусочно-линейны на треугольниках центров. Если напрямую отобразить
`Main/Left/Right` в RGB, каналы окажутся привязаны к меняющимся геометрическим ролям.
Хроматическая перестановка сохраняет вклад конкретного гекса в одном и том же канале.

## Другая палитра или значения

Для цветов `C0`, `C1` и `C2`, назначенных трём классам, смешивайте их компоненты по формуле:

```text
C(P) = Index0 * C0 + Index1 * C1 + Index2 * C2
```

Та же формула интерполирует числовые значения. Один номер класса не определяет конкретный гекс:
один класс встречается у множества ячеек. Если значения различаются по гексам, получите реальные
индексы окружения из `IndexTripletRaster`, прочитайте значения карты и также переставьте их
по классам. Другой вариант — совместить индексный и обычный барицентрический растры напрямую
в порядке `Main/Left/Right`, как в разделе
[«Перенос значений карты в пиксели»](rasterizing-a-hex-map/mapping-map-values-to-pixels.md).

## Граница конечной карты

Полный растр весов продолжает логическую гексагональную решётку за пределами конечной топологии.
Поэтому изображение смешивания заполняет всю прямоугольную область выборки, включая углы,
где дискретная карта классов прозрачна. Сумма полных весов по-прежнему приблизительно равна `1`.

Чтобы учитывать только гексы внутри карты, используйте
<xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster>.
Добавьте после предыдущего примера:

```csharp
var partialWeights = new ChromaticBarycentricPartialTripletRaster(
    geometry, sampling);

SpatialRaster<RGBA8BitColor> finiteImage = partialWeights.MapValues(ToFiniteRgb);
finiteImage.SaveAsPng(Path.GetFullPath("chromatic-barycentric-finite.png"));

static RGBA8BitColor ToFiniteRgb(PartialChromaticTriplet<float> w)
{
    float w0 = w.HasIndex0 ? w.Index0 : 0f;
    float w1 = w.HasIndex1 ? w.Index1 : 0f;
    float w2 = w.HasIndex2 ? w.Index2 : 0f;
    float sum = w0 + w1 + w2;

    return sum > 0f
        ? RGBA8BitColor.FromNormalized(w0 / sum, w1 / sum, w2 / sum, alpha: 1f)
        : default;
}
```

![RGB-смешивание присутствующих гексов с нормализацией весов у границы](~/assets/hexes/chromatization/chromatic-barycentric-finite.png)

Частичный растр переставляет по классам и веса, и флаги присутствия. Оставшиеся веса он не
нормализует. Деление на их сумму сохраняет интенсивность смешивания у границы; если просто
обнулить отсутствующие вклады, RGB-изображение в этих местах станет темнее.
При отсутствии положительного вклада пример возвращает прозрачный чёрный цвет.

Флаги присутствия описывают окружающие гексы, а не маску отсечения пикселя. У точки чуть за
границей карты всё ещё может быть сосед внутри неё. Если нужен точный контур из гексов,
дополнительно проверяйте, что содержащий точку гекс принадлежит исходной топологии.

Используйте одинаковые `HexMapGeometry` и `RasterGeometry` для связанных индексных,
хроматических и барицентрических растров: одинакового числа пикселей недостаточно для совпадения
точек выборки. API полных и частичных растров описан в разделе
[«Создание хроматического растра»](../how-to-guides/chromatization/create-a-chromatic-raster.md),
а трёхцветный инвариант — в статье
[«Хроматизация»](../concepts/spatial-algorithms/chromatization.md).
