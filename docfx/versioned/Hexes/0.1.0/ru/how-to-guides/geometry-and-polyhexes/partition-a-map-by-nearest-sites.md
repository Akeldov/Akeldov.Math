# Разделить карту по ближайшим центрам

Используйте `ToVoronoiHexPartitionMap()`, чтобы назначить каждый гекс ближайшему взвешенному
сайту. Сравнение выполняется по мировым координатам центров гексов, поэтому создайте
<xref:Akeldov.Math.Hexes.Geometry.HexCenterMap> из той же геометрии, которая задаёт размещение
карты.

## Создание разбиения

Следующий пример делит карту из одной строки между двумя сайтами одинакового веса:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var geometry = new HexMapGeometry(
    width: 3,
    height: 1,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var centers = new HexCenterMap(geometry);
var sites = new[]
{
    new Site(new PointXY(0f, 0f), weight: 1f),
    new Site(new PointXY(4f, 0f), weight: 1f)
};

VoronoiHexPartitionMap partition =
    centers.ToVoronoiHexPartitionMap(sites);
```

Позиции сайтов и центры гексов должны находиться в одной системе координат. Сайт не обязан
находиться внутри карты: он всё равно может получить гексы, центры которых расположены ближе
всего к нему.

## Чтение назначений и ячеек

Обратитесь к карте разбиения по индексу, чтобы найти ячейку, назначенную конкретному гексу.
`SiteIndex` указывает позицию сайта в исходном массиве `sites`:

```csharp
for (int x = 0; x < geometry.Topology.Resolution.X; x++)
{
    VoronoiCell cell = partition[new VectorXYInt(x, 0)];
    Console.Write($"{cell.SiteIndex} ");
}
```

Пример выводит:

```text
0 0 1
```

Используйте `Cells`, когда гексы нужны группами по исходным сайтам:

```csharp
foreach (VoronoiCell cell in partition.Cells)
{
    Console.WriteLine($"Сайт {cell.SiteIndex}: гексов — {cell.HexIndexes.Count}");
}
```

Результат:

```text
Сайт 0: гексов — 2
Сайт 1: гексов — 1
```

`Cells` содержит по одной ячейке для каждого исходного сайта и сохраняет исходный порядок. Если
сайт не получил ни одного гекса, он всё равно представлен ячейкой с пустым списком `HexIndexes`.

## Повторное использование набора сайтов

Метод расширения создаёт разделитель для одного вызова. Создайте
<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitioner> напрямую, если одни и те же
сайты нужно применить к нескольким геометриям карт:

```csharp
var partitioner = new VoronoiHexPartitioner(sites);

VoronoiHexPartitionMap firstPartition = partitioner.Partition(centers);
VoronoiHexPartitionMap secondPartition = partitioner.Partition(
    new HexCenterMap(new HexMapGeometry(6, 4, VectorXY.Zero, 1f, Layout.OddR)));
```

Конструктор копирует и проверяет сайты, поэтому последующие изменения исходного массива не
влияют на разделитель.

## Настройка влияния с помощью весов

Для конечных положительных весов при назначении сравнивается `distance / weight`. Поэтому
увеличение веса сайта расширяет область его влияния. Координаты должны быть конечными, веса —
неотрицательными и не `NaN`, список сайтов не должен быть пустым, а хотя бы один вес должен быть
ненулевым.

<xref:Akeldov.Math.Hexes.Partitioning.Voronoi.VoronoiHexPartitionMap> — семантический результат
только для чтения: назначения отдельных гексов остаются согласованными с `Cells` и списком
`HexIndexes` каждой ячейки. Вызовите `ToMutableHexMap()`, если нужна новая принадлежащая
вызывающему карта с изменяемыми назначениями.

Формула взвешенного расстояния и особое поведение нулевого и бесконечного весов описаны в разделе
[«Разбиение пространства»](../../concepts/spatial-algorithms/space-partitioning.md).
