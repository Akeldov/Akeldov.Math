# Визуализация IndexTripletRaster

Преобразуем три индекса гексов в каждой ячейке растра в каналы RGB.
<xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster> описывает полные окрестности вершины,
включая индексы за пределами конечной исходной карты. Пример воспроизводит изображение
с раскраской индексов из примеров топологических растров.

## Что представляет каждый пиксель

Растр берёт мировую точку в центре каждого пикселя. Для неё он находит содержащий гекс и его
ближайшую вершину, затем сохраняет `Triplet<VectorXYInt>`:

- `Main` — индекс содержащего гекса, кодируется красным каналом.
- `Left` — индекс одного из соседей у выбранной вершины, кодируется зелёным.
- `Right` — индекс второго соседа у той же вершины, кодируется синим.

Порядок `Left` и `Right` зависит от вершины и раскладки, а не задаёт постоянные направления
на экране. Это **индексы ячеек**, а не барицентрические веса или хроматические классы.
В отличие от индексной карты, растр хранит окрестность для каждой прямоугольной ячейки растра,
а не для каждого гекса.

## Отрисовка полных троек

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

var hexMapGeometry = new HexMapGeometry(
    width: 5,
    height: 4,
    radius: 1f,
    layout: Layout.OddR);

var rasterGeometry = new RasterGeometry(
    new PointXY(0f, 0f),
    hexMapGeometry.GetBoundingBoxSize(),
    new VectorXYInt(192, 192));

var sourceRaster = new IndexTripletRaster(
    hexMapGeometry,
    rasterGeometry);

SpatialRaster<RGBA16BitColor> colorRaster = sourceRaster.MapValues(ToColor);

colorRaster.SaveAsPng(
    Path.GetFullPath("index-triplet-raster-odd-r-rgba16.png"));

static RGBA16BitColor ToColor(Triplet<VectorXYInt> triplet)
{
    return new RGBA16BitColor(
        ToChannel(EncodeIndex(triplet.Main)),
        ToChannel(EncodeIndex(triplet.Left)),
        ToChannel(EncodeIndex(triplet.Right)),
        ushort.MaxValue);
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

![IndexTripletRaster: индексы Main, Left и Right закодированы каналами RGB](~/assets/hexes/rasters/index-triplet-raster-odd-r-rgba16.png)

## Как читать результат

`EncodeIndex` преобразует X- и Y-координаты индекса в одну интенсивность. `ToChannel`
ограничивает её диапазоном `0..1` и переводит в 16-битный канал. Это диагностическая раскраска,
а не уникальное или обратимое кодирование индекса.

Цвет меняется при смене содержащего гекса или его ближайшей вершины. Пока выбранная тройка
остаётся прежней, цвет постоянен: это не плавная барицентрическая интерполяция.
`MapValues` преобразует сохранённые тройки в цвета, сохраняя геометрию выборки.

Полный растр продолжает логическую гексагональную сетку за пределы конечной карты. Индекс вне
исходной топологии по-прежнему участвует в расчёте своего канала. Ограничение цвета **не**
прижимает индекс к границе карты: не используйте непроверенную тройку для чтения ограниченной
`HexMap<T>`.

Сравните результат с
[визуализацией IndexPartialTripletRaster](visualizing-index-partial-triplet-raster.md):
геометрия выборки и формула цвета те же, но каналы отсутствующих индексов обнуляются.
Создание и чтение растра подробнее описаны на странице
[«Создать индексный Triplet-растр»](../how-to-guides/rasters/create-an-index-triplet-raster.md).
