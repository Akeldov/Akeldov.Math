# Получение рёбер и вершин

В этой части руководства вы получите шесть вершин одной занятой ячейки в мировых координатах и
соедините соседние вершины отрезками рёбер. Это полезно, когда приложению нужна геометрия
отдельного гекса, а не только внешний контур полигекса.

## Построение геометрии одной ячейки

Добавьте в начало `Program.cs` пространства имён:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
```

Затем добавьте после вывода данных полигекса следующий код:

```csharp
const Layout layout = Layout.OddR;
const float hexRadius = 1f;
const int sampleQ = 0;
const int sampleR = 1;

VectorXY[] vertices =
    Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexVertices(
        sampleQ,
        sampleR,
        hexRadius,
        layout);
var edges = new Segment[vertices.Length];

for (int index = 0; index < vertices.Length; index++)
{
    edges[index] = new Segment(
        (PointXY)vertices[index],
        (PointXY)vertices[(index + 1) % vertices.Length]);
}

Console.WriteLine($"Вершин гекса: {vertices.Length}");
Console.WriteLine($"Рёбер гекса: {edges.Length}");
```

Последние строки результата:

```text
Вершин гекса: 6
Рёбер гекса: 6
```

`GetHexVertices` возвращает новый изменяемый массив, принадлежащий вызывающему коду. Выбранная
Q/R-ячейка `[0, 1]` занята, а `OddR` задаёт для неё ориентацию вершиной вверх в пространстве XY.
Последнее ребро соединяет вершину 5 с вершиной 0.

Это шесть рёбер одной ячейки. Если повторить операцию для всех занятых ячеек, общие внутренние
рёбра попадут в результат дважды. При последующем создании региона внутренние рёбра удаляются,
и остаётся только граница полигекса.

Переходите к разделу
[Преобразование в геометрию Spatial2D](converting-to-spatial2d-geometry.md).
