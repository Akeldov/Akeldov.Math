# Найти ближайший гекс для точки

Используйте `PointXY.ToXYIndex`, чтобы определить индекс гекса, содержащего пространственную
точку. Передайте радиус, центр нулевого гекса и полную раскладку той же сетки.

## Определение индекса

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
const float hexRadius = 2f;
var zeroHexCenter = new VectorXY(10f, 20f);
var point = new PointXY(20.64f, 25.9f);

VectorXYInt index = point.ToXYIndex(
    hexRadius,
    zeroHexCenter,
    layout);

Console.WriteLine($"Index: ({index.X}, {index.Y})");
```

Результат:

```text
Index: (3, 2)
```

Метод сначала переводит точку в дробные QRS-координаты, выбирает ближайшую целочисленную ячейку,
а затем преобразует её в индекс указанной раскладки.

## Проверка границ карты

Результат относится к неограниченной гексагональной сетке. Если точка должна находиться в
конечной карте, проверьте индекс до обращения к `HexMap<T>`:

```csharp
var topology = new HexMapTopology(7, 5, layout);

bool isInside =
    index.X >= 0 && index.X < topology.Resolution.X &&
    index.Y >= 0 && index.Y < topology.Resolution.Y;
```

`hexRadius` должен быть конечным и положительным, а точка и центр — иметь конечные компоненты.
Если смешать начало, радиус или раскладку от другой сетки, метод вернёт правдоподобный, но
смыслово неверный индекс.

Точка точно на общем ребре или вершине назначается одной ячейке детерминированно. Рядом с такой
границей малое изменение `float` может выбрать соседний гекс, поэтому не используйте результат
как устойчивый идентификатор для геометрически неоднозначной точки.

См. также [«Преобразовать QRS в координаты Spatial2D»](convert-qrs-to-spatial2d-coordinates.md).
