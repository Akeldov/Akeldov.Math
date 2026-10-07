# Визуализация результата

Итоговые области уже содержат все элементы карты и релаксированный центр, которому они принадлежат.
Отрисуйте каждый элемент квадратом SVG размером в одну единицу, а затем нанесите поверх центры.

Добавьте в начало `Program.cs` пространства имён:

```csharp
using System.Globalization;
using System.Text;
```

После вызова `Partition(mapCells)` добавьте:

```csharp
string[] colors =
{
    "#2563eb", "#dc2626", "#16a34a", "#ca8a04",
    "#9333ea", "#0891b2", "#ea580c", "#4f46e5"
};

var svg = new StringBuilder();
svg.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 120 80\">");

for (int partitionIndex = 0; partitionIndex < partitions.Count; partitionIndex++)
{
    string color = colors[partitionIndex % colors.Length];

    foreach (MapCell cell in partitions[partitionIndex].Items)
    {
        int svgY = 79 - cell.Y;
        svg.AppendLine(
            $"  <rect x=\"{cell.X}\" y=\"{svgY}\" width=\"1\" height=\"1\" fill=\"{color}\" />");
    }
}

foreach (var partition in partitions)
{
    string x = partition.Site.Position.X.ToString(CultureInfo.InvariantCulture);
    string y = (80f - partition.Site.Position.Y).ToString(CultureInfo.InvariantCulture);
    svg.AppendLine(
        $"  <circle cx=\"{x}\" cy=\"{y}\" r=\"0.8\" fill=\"white\" stroke=\"#111827\" stroke-width=\"0.25\" />");
}

svg.AppendLine("</svg>");
const string outputPath = "weighted-voronoi.svg";
File.WriteAllText(outputPath, svg.ToString());

Console.WriteLine($"Карта сохранена: {Path.GetFullPath(outputPath)}");
```

Координата Y инвертируется, потому что в SVG она возрастает вниз, а карта Spatial2D использует
обычное направление Y вверх. Откройте `weighted-voronoi.svg` в браузере: цветом показаны области,
а белыми маркерами — их центры.

## Полный код

```csharp
using System.Globalization;
using System.Text;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var pointSampler = new PoissonDiskPointSampler(
    new Random(12345),
    maxAttempts: 30);

var samples = pointSampler.Sample(fieldSize, minimalDistance: 14f);
var sites = samples
    .Select((sample, index) => new Site(
        sample.Point,
        weight: index % 5 == 0 ? 1.8f : 1f))
    .ToArray();

var mapCells = new List<MapCell>(120 * 80);
for (int y = 0; y < 80; y++)
{
    for (int x = 0; x < 120; x++)
        mapCells.Add(new MapCell(x, y));
}

var partitioner = new VoronoiItemPartitioner<MapCell>(
    sites,
    relaxationIterations: 2,
    emptyCellPolicy: EmptyCellPolicy.LeaveAsIs);

IReadOnlyList<VoronoiItemPartition<MapCell>> partitions =
    partitioner.Partition(mapCells);

string[] colors =
{
    "#2563eb", "#dc2626", "#16a34a", "#ca8a04",
    "#9333ea", "#0891b2", "#ea580c", "#4f46e5"
};

var svg = new StringBuilder();
svg.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 120 80\">");

for (int partitionIndex = 0; partitionIndex < partitions.Count; partitionIndex++)
{
    string color = colors[partitionIndex % colors.Length];
    foreach (MapCell cell in partitions[partitionIndex].Items)
    {
        int svgY = 79 - cell.Y;
        svg.AppendLine(
            $"  <rect x=\"{cell.X}\" y=\"{svgY}\" width=\"1\" height=\"1\" fill=\"{color}\" />");
    }
}

foreach (var partition in partitions)
{
    string x = partition.Site.Position.X.ToString(CultureInfo.InvariantCulture);
    string y = (80f - partition.Site.Position.Y).ToString(CultureInfo.InvariantCulture);
    svg.AppendLine(
        $"  <circle cx=\"{x}\" cy=\"{y}\" r=\"0.8\" fill=\"white\" stroke=\"#111827\" stroke-width=\"0.25\" />");
}

svg.AppendLine("</svg>");
const string outputPath = "weighted-voronoi.svg";
File.WriteAllText(outputPath, svg.ToString());

Console.WriteLine($"Карта сохранена: {Path.GetFullPath(outputPath)}");

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

Теперь у вас есть воспроизводимый конвейер от равномерно распределённых случайных центров до
релаксированной взвешенной дискретной карты Вороного. Меняйте начальное значение генератора,
минимальное расстояние, веса, число проходов релаксации или разрешение сетки, чтобы получать карты
разного характера.
