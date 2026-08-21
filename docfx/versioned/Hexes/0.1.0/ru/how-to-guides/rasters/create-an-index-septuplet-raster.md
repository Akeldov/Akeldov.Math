# Создать индексный Septuplet-растр

Используйте <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletRaster>, чтобы заранее вычислить центральный индекс гекса и
индексы всех шести его соседей по рёбрам для каждой точки выборки растра. Такая окрестность из семи индексов
подходит для фильтров, локальных симуляций и других операций над всеми ближайшими соседями содержащего гекса.

## Создать растр

Задайте конечную гексагональную карту в мировом пространстве, получите сетку выборки и передайте обе геометрии в растр:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var mapGeometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 16f);

var indexRaster = new IndexSeptupletRaster(
    mapGeometry,
    rasterGeometry);
```

`pixelsPerApothem` задаёт плотность выборки. Используйте ту же `rasterGeometry`, если эта таблица поиска должна совпадать
с другим растром. Сокращённый вариант `new IndexSeptupletRaster(mapGeometry)` создаёт сетку с плотностью один пиксель на апофему без внешнего поля.

`SourceHexMapGeometry` предоставляет исходную геометрию карты, `Geometry` — сетку выборки, а `Resolution` — число ячеек растра.
Исходная топология доступна через `indexRaster.SourceHexMapGeometry.Topology`.

## Прочитать окрестность из семи индексов

Центральная ячейка растра гарантированно находится внутри созданного выше положительного разрешения:

```csharp
var sample = new VectorXYInt(
    indexRaster.Resolution.X / 2,
    indexRaster.Resolution.Y / 2);

Septuplet<VectorXYInt> neighborhood = indexRaster[sample];

Console.WriteLine($"Main:      {neighborhood.Main}");
Console.WriteLine($"Adjacent0: {neighborhood.Adjacent0}");
Console.WriteLine($"Adjacent1: {neighborhood.Adjacent1}");
Console.WriteLine($"Adjacent2: {neighborhood.Adjacent2}");
Console.WriteLine($"Adjacent3: {neighborhood.Adjacent3}");
Console.WriteLine($"Adjacent4: {neighborhood.Adjacent4}");
Console.WriteLine($"Adjacent5: {neighborhood.Adjacent5}");
```

`Main` — содержащий или ближайший гекс на подразумеваемой бесконечной сетке. `Adjacent0`–`Adjacent5` — его шесть соседей по рёбрам,
соответствующие `HexEdge.Edge0`–`HexEdge.Edge5`. Их физические направления зависят от раскладки, но порядок номеров рёбер остаётся стабильным.

То же значение можно прочитать через `[x, y]` или плоский целочисленный индекс в порядке строк. В версии `0.1.0` у Septuplet-растра нет
метода `TryGetValue`. Если индексы не являются константами времени компиляции, предпочитайте индексатор `[VectorXYInt]`: он проверяет обе координаты и вызывает
`IndexOutOfRangeException`, если они выходят за пределы `Resolution`.

## Выбрать Septuplet- или Triplet-растр

`IndexSeptupletRaster` всегда выбирает `Main` и всех шестерых соседей по рёбрам. `IndexTripletRaster` вместо этого выбирает `Main` и двух соседей,
которые встречаются с ним у ближайшей вершины. Используйте септуплет для полной окрестности радиуса 1, а триплет — для операций, зависящих от одной вершины и трёх
окружающих её гексов, например для барицентрической интерполяции.

## Обработать границу конечной карты

Полный септуплет описывает бесконечную гексагональную сетку. Рядом с границей исходной карты некоторые соседние индексы могут
оказаться за пределами конечной топологии, а при наличии внешнего поля вне неё может оказаться даже `Main`. Не используйте эти индексы напрямую с ограниченным `HexMap<T>`.

Для значений, учитывающих границу, создайте <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster> с теми же геометриями:

```csharp
var partialIndexRaster = new IndexPartialSeptupletRaster(
    mapGeometry,
    rasterGeometry);
```

Его флаги наличия определяют, какие позиции принадлежат исходной топологии. Перед индексацией конечной карты перейдите к разделу
[«Обработать частичные окрестности»](handle-partial-neighborhoods.md). Таблица поиска по трём индексам описана в разделе
[«Создать индексный Triplet-растр»](create-an-index-triplet-raster.md). Полная модель растров описана в разделе
[«Растры»](../../concepts/data-storage/rasters.md).
