# Полные и частичные индексные карты

Создадим две карты, которые хранят индексы ячеек, и визуализируем различия на границах карты.
<xref:Akeldov.Math.Hexes.Topology.IndexSeptupletMap> сохраняет всех шестерых логических соседей, а
<xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletMap> отмечает, какие из них действительно
принадлежат ограниченной карте.

Это гексагональные карты: каждое значение описывает окрестность **ячейки-гекса**, а не пикселя
растра. Обе карты заранее вычисляют основной индекс и шесть соседних в порядке, заданном
раскладкой. Собственная геометрия позволяет растеризовать их без отдельного задания размещения.

Запускайте каждый пример отдельно в консольном проекте со ссылкой на Akeldov.Math.Hexes.
При необходимости начните со страницы [«Создание проекта»](creating-a-hex-map/creating-a-project.md).

## Полная окрестность: IndexSeptupletMap

Каждая ячейка хранит `Septuplet<VectorXYInt>`. Поле `Main` содержит индекс самой ячейки, а
`Adjacent0`–`Adjacent5` — индексы шестерых логических соседей. На границе координаты соседей
сохраняются, даже если выходят за пределы карты. Это удобно для алгоритмов, которые самостоятельно
задают правила обработки границ.

Создадим карту 6 × 4 с раскладкой `OddR` и радиусом от центра до вершины в одну мировую единицу.
Красный канал — сумма X-координат соседей, делённая на `36f`; зелёный — сумма их Y-координат
с тем же делителем. Синий канал имеет максимальную интенсивность. Координаты основной ячейки
в расчёте не участвуют.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using System.IO;

var geometry = new HexMapGeometry(
    width: 6,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var map = new IndexSeptupletMap(geometry);
Septuplet<VectorXYInt> neighborhood = map[new VectorXYInt(0, 0)];

map
    .Rasterize(
        pixelsPerApothem: 36f,
        margin: 0f,
        colorSelector: ToIndexColor)
    .SaveAsPng(Path.GetFullPath("index-septuplet-map.png"));

static RGBA16BitColor ToIndexColor(Septuplet<VectorXYInt> septuplet)
{
    float r =
        septuplet.Adjacent0.X +
        septuplet.Adjacent1.X +
        septuplet.Adjacent2.X +
        septuplet.Adjacent3.X +
        septuplet.Adjacent4.X +
        septuplet.Adjacent5.X;
    r /= 36f;

    float g =
        septuplet.Adjacent0.Y +
        septuplet.Adjacent1.Y +
        septuplet.Adjacent2.Y +
        septuplet.Adjacent3.Y +
        septuplet.Adjacent4.Y +
        septuplet.Adjacent5.Y;
    g /= 36f;

    return RGBA16BitColor.FromNormalized(r, g, 1f);
}
```

![Полная индексная карта: цвет по индексам всех шестерых соседей](~/assets/hexes/topology/index-septuplet-map.png)

Делитель `36f` задаёт фиксированный масштаб цвета для этого примера, а не среднее по соседям.
`FromNormalized` ограничивает каналы диапазоном `0..1`, в том числе при отрицательных суммах
у границы. Отдельный параметр `pixelsPerApothem: 36f` управляет разрешением изображения, а не цветом.

## Частичная окрестность: IndexPartialSeptupletMap

Каждая ячейка хранит `PartialSeptuplet<VectorXYInt>`. Флаги `HasAdjacent0`–`HasAdjacent5`
показывают, какие соседи находятся внутри карты. Проверяйте соответствующий флаг, прежде чем
обращаться по индексу соседа к другой ограниченной карте.

В этом примере геометрия и масштаб цвета остаются прежними, но в суммы координат входят только
присутствующие соседи:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using System.IO;

var geometry = new HexMapGeometry(
    width: 6,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var map = new IndexPartialSeptupletMap(geometry);
PartialSeptuplet<VectorXYInt> neighborhood = map[new VectorXYInt(0, 0)];

map
    .Rasterize(
        pixelsPerApothem: 36f,
        margin: 0f,
        colorSelector: ToIndexColor)
    .SaveAsPng(Path.GetFullPath("index-partial-septuplet-map.png"));

static RGBA16BitColor ToIndexColor(PartialSeptuplet<VectorXYInt> septuplet)
{
    float r =
        (septuplet.HasAdjacent0 ? septuplet.Adjacent0.X : 0) +
        (septuplet.HasAdjacent1 ? septuplet.Adjacent1.X : 0) +
        (septuplet.HasAdjacent2 ? septuplet.Adjacent2.X : 0) +
        (septuplet.HasAdjacent3 ? septuplet.Adjacent3.X : 0) +
        (septuplet.HasAdjacent4 ? septuplet.Adjacent4.X : 0) +
        (septuplet.HasAdjacent5 ? septuplet.Adjacent5.X : 0);
    r /= 36f;

    float g =
        (septuplet.HasAdjacent0 ? septuplet.Adjacent0.Y : 0) +
        (septuplet.HasAdjacent1 ? septuplet.Adjacent1.Y : 0) +
        (septuplet.HasAdjacent2 ? septuplet.Adjacent2.Y : 0) +
        (septuplet.HasAdjacent3 ? septuplet.Adjacent3.Y : 0) +
        (septuplet.HasAdjacent4 ? septuplet.Adjacent4.Y : 0) +
        (septuplet.HasAdjacent5 ? septuplet.Adjacent5.Y : 0);
    g /= 36f;

    return RGBA16BitColor.FromNormalized(r, g, 1f);
}
```

![Частичная индексная карта: цвет по индексам только присутствующих соседей](~/assets/hexes/topology/index-partial-septuplet-map.png)

## Сравнение результатов

У внутренних ячеек есть все шесть соседей, поэтому оба примера дают там одинаковые цвета.
На границе полная карта учитывает в суммах координаты за пределами карты, а частичный пример
пропускает их. Делитель остаётся равным `36f`: повторной нормализации по оставшимся соседям нет.

Частичная карта не переносит отсутствующих соседей на противоположную сторону и не заменяет их
граничными ячейками. Флаги обозначают наличие, а не значение для подстановки. При чтении самой
карты обе реализации по-прежнему требуют индекс ячейки внутри её границ.

Устройство значений описано в разделе
[«Полные и частичные окрестности»](../concepts/data-storage/complete-and-partial-neighborhoods.md),
а индексные растры по пикселям и интерполяция — в учебнике
[«Растеризация гексагональной карты»](rasterizing-a-hex-map/index.md).
