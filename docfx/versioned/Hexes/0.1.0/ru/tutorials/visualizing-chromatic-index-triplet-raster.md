# Визуализация ChromaticIndexTripletRaster

Закодируем хроматические классы трёх соседних гексов в каналах RGB.
<xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster> хранит `Triplet<byte>`
для каждой ячейки растра. Его компоненты — номера классов `0`, `1` и `2`,
а не координаты гексов или веса интерполяции.

Пример воспроизводит изображение троек классов из примеров хроматических растров.
Сама трёхцветная классификация описана в учебнике
[«Хроматизация гексагональной карты»](chromatizing-a-hex-map.md).

## Классы в геометрическом порядке

Для мировой точки в центре каждого пикселя растр находит содержащий гекс и его ближайшую
вершину. Он сохраняет классы этого гекса и двух соседей, встречающихся с ним у выбранной вершины:

- `Main` — класс содержащего гекса, кодируется красным каналом.
- `Left` — класс одного из соседей, кодируется зелёным.
- `Right` — класс второго соседа, кодируется синим.

Эти три гекса попарно соседствуют по ребру, поэтому их классы различны. Каждая тройка —
перестановка `(0, 1, 2)`. Порядок компонентов геометрический: `Main` не обязан иметь
класс `0`, а `Left` и `Right` не задают постоянные направления на экране.

В отличие от `ChromaticIndexMap`, который хранит один класс на гекс, этот растр хранит
три класса на прямоугольную ячейку растра.

## Отрисовка троек классов

Запустите этот законченный пример в консольном проекте со ссылкой на Akeldov.Math.Hexes.
Подготовка описана на странице [«Создание проекта»](creating-a-hex-map/creating-a-project.md).

Исходная карта содержит 5 × 4 гекса с раскладкой `OddR` и радиусом от центра до вершины
в одну мировую единицу. Конструктор без явного начала координат размещает нулевой гекс
в точке `(apothem, radius)` для этой раскладки. Растр начинается в мировой точке `(0, 0)`,
имеет размер ограничивающего прямоугольника карты и разрешение 192 × 192 пикселя.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System;
using System.IO;

var topology = new HexMapTopology(5, 4, Layout.OddR);
var hexMapGeometry = new HexMapGeometry(topology, radius: 1f);

var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));

var sourceRaster = new ChromaticIndexTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("chromatic-index-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(Triplet<byte> chromatic)
{
    return new RGBA16BitColor(
        ToChannel(0.18f + 0.34f * chromatic.Main),
        ToChannel(0.18f + 0.34f * chromatic.Left),
        ToChannel(0.18f + 0.34f * chromatic.Right),
        ushort.MaxValue);
}

static ushort ToChannel(float value)
{
    value = MathF.Min(MathF.Max(value, 0f), 1f);
    return (ushort)MathF.Round(value * ushort.MaxValue);
}
```

![ChromaticIndexTripletRaster: номера классов Main, Left и Right закодированы каналами RGB](~/assets/hexes/rasters/chromatic-index-triplet-raster-odd-r-rgba16.png)

## Как читать цвета

Формула `0.18f + 0.34f * classIndex` переводит классы `0`, `1` и `2` в нормализованные
интенсивности `0.18`, `0.52` и `0.86`. `ToChannel` ограничивает каждую интенсивность
и переводит её в 16-битный канал; альфа-канал всегда задаёт полную непрозрачность.

Поэтому каждый пиксель использует одну из шести перестановок этих трёх интенсивностей каналов.
Пока тройка классов не меняется, цвет постоянен. Повторяющиеся области с резкими границами
показывают порядок классов, а не плавную интерполяцию.

Здесь красный канал всегда кодирует класс `Main`; он **не** означает «вклад класса 0
в красный цвет». Аналогично зелёный и синий кодируют номера классов в позициях `Left`
и `Right`. `MapValues` только преобразует значения в цвета, сохраняя геометрию выборки.

## Границы и барицентрические веса

Это **полный** растр: он классифицирует подразумеваемую бесконечную сетку, включая гексы
за пределами исходной конечной топологии. Он не добавляет флаги наличия и не обрезает
изображение по контуру карты. Класс `0` — допустимый класс, а не признак отсутствия.

Если должны участвовать только гексы внутри карты, используйте `ChromaticIndexPartialTripletRaster`
и его флаги наличия. Чтение классов с учётом границ описано на странице
[«Создать хроматический растр»](../how-to-guides/chromatization/create-a-chromatic-raster.md).

Для смешивания значений одних номеров классов недостаточно: нужны также барицентрические веса.
`BarycentricTripletRaster` хранит веса в геометрическом порядке `Main/Left/Right`,
а `ChromaticBarycentricTripletRaster` переставляет их по классу в `Index0/Index1/Index2`.
Эти упорядоченные по классу компоненты — **веса**, а не номера классов, показанные здесь.

Плавная RGB-визуализация с постоянным цветовым каналом для каждого класса приведена в учебнике
[«Хроматизация и барицентрическая интерполяция»](chromatic-barycentric-interpolation.md).
