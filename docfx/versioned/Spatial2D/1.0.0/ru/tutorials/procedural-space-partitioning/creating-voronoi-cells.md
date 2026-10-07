# Создание областей Вороного

`VoronoiItemPartitioner<TItem>` распределяет расположенные элементы, а не строит границы
многоугольников. В этой карте каждый элемент представляет центр одной квадратной ячейки сетки.

Добавьте список с сеткой размером 120 на 80:

```csharp
var mapCells = new List<MapCell>(120 * 80);

for (int y = 0; y < 80; y++)
{
    for (int x = 0; x < 120; x++)
        mapCells.Add(new MapCell(x, y));
}
```

После инструкций верхнего уровня в конце `Program.cs` добавьте тип `MapCell`:

```csharp
sealed class MapCell : IHasPosition2D
{
    public MapCell(int x, int y)
    {
        X = x;
        Y = y;
        Position = new PointXY(x + 0.5f, y + 0.5f);
    }

    public int X { get; }
    public int Y { get; }
    public PointXY Position { get; }
}
```

Теперь распределите ячейки между ближайшими центрами:

```csharp
var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions =
    partitioner.Partition(mapCells);

foreach (var partition in partitions)
{
    Console.WriteLine(
        $"Центр {partition.Site.Position}: {partition.Items.Count} ячеек");
}
```

`MapCell` реализует <xref:Akeldov.Math.Spatial2D.IHasPosition2D> — единственный пространственный
контракт, который требуется partitioner. При равных весах каждая ячейка достаётся ближайшему
центру.

`LeaveAsIs` сохраняет в результате одну область для каждого настроенного центра, даже если ей не
досталось ни одной ячейки. Возвращаемый список является семантическим результатом с устойчивыми
порядком и количеством элементов, а `Items` каждой области — структурное представление только для
чтения.

Переходите к разделу [«Добавление весов»](adding-weights.md).
