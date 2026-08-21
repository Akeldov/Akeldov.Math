# Создать индексный Triplet-растр

Используйте <xref:Akeldov.Math.Hexes.Topology.IndexTripletRaster>, чтобы заранее вычислить три индекса гексов,
окружающих точку в центре каждой ячейки растра. Результат хранит данные для интерполяции и других
пространственных операций, но сам по себе не является изображением.

## Задать исходную геометрию и сетку выборки

Создайте `HexMapGeometry` для конечной исходной карты, а затем получите покрывающую её `RasterGeometry`:

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

var indexRaster = new IndexTripletRaster(
    mapGeometry,
    rasterGeometry);
```

`pixelsPerApothem` задаёт плотность выборки. Сохраняйте полученную `rasterGeometry`, если несколько
специализированных растров должны точно совпадать по ячейкам. Сокращённый вариант
`new IndexTripletRaster(mapGeometry)` создаёт сетку с плотностью один пиксель на апофему без внешнего поля.

Растр предоставляет переданные геометрии через `SourceHexMapGeometry` и `Geometry`. `Resolution` — это разрешение
ячеек растра, а `Topology` — топология исходной карты; это разные системы координат.

## Прочитать триплет

Выберите индекс ячейки растра и вызовите `TryGetValue` перед чтением результата:

```csharp
var sample = new VectorXYInt(
    indexRaster.Resolution.X / 2,
    indexRaster.Resolution.Y / 2);

if (indexRaster.TryGetValue(
        sample,
        out Triplet<VectorXYInt> hexIndices))
{
    Console.WriteLine($"Main:  {hexIndices.Main}");
    Console.WriteLine($"Left:  {hexIndices.Left}");
    Console.WriteLine($"Right: {hexIndices.Right}");
}
```

Для каждой точки выборки:

- `Main` — содержащий или ближайший гекс на подразумеваемой бесконечной сетке.
- `Left` и `Right` — два соседа, которые встречаются с `Main` у ближайшей вершины.
- Порядок левого и правого соседа зависит от этой вершины и выбранной раскладки.

Те же значения доступны через индексаторы `[x, y]`, `[VectorXYInt]` и плоский целочисленный индекс в порядке строк.
`TryGetValue` удобен, когда координаты ячейки растра могут выходить за пределы `Resolution`.

## Обработать границу конечной карты

`IndexTripletRaster` моделирует полную бесконечную гексагональную сетку. Поэтому рядом с границей конечной исходной
карты любой из `Main`, `Left` или `Right` может оказаться за пределами `Topology.Resolution`. `TryGetValue` проверяет только наличие
запрошенной ячейки растра и не гарантирует принадлежность трёх возвращённых индексов конечной карте.

Не используйте непроверенный триплет для индексации ограниченного `HexMap<T>`. Если можно использовать только индексы внутри
карты, создайте <xref:Akeldov.Math.Hexes.Topology.IndexPartialTripletRaster> с теми же геометриями и проверяйте его флаги наличия:

```csharp
var partialIndexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Гранично-безопасное чтение показано в разделе [«Обработать частичные окрестности»](handle-partial-neighborhoods.md).
Чтобы вычислить веса интерполяции для тех же трёх позиций, передайте `rasterGeometry` в [«Создать барицентрический растр»](create-a-barycentric-raster.md).
Полная модель растров описана в разделе [«Растры»](../../concepts/data-storage/rasters.md).
