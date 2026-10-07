# Получить геометрические границы карты

Используйте `GetBoundingBox()`, чтобы получить осевой прямоугольник, содержащий все гексы
конечной карты вместе с внешними рёбрами и вершинами граничных ячеек.

## Получить ограничивающий прямоугольник

Создайте <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> с топологией карты, центром нулевого
гекса и радиусом, затем вызовите `GetBoundingBox()`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

var geometry = new HexMapGeometry(
    width: 3,
    height: 2,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

Rectangle bounds = geometry.GetBoundingBox();

Console.WriteLine(FormattableString.Invariant(
    $"Min: ({bounds.Min.X:F3}, {bounds.Min.Y:F3})"));
Console.WriteLine(FormattableString.Invariant(
    $"Max: ({bounds.Max.X:F3}, {bounds.Max.Y:F3})"));
Console.WriteLine(FormattableString.Invariant(
    $"Размер: ({bounds.Size.X:F3}, {bounds.Size.Y:F3})"));
```

Результат:

```text
Min: (8.268, 18.000)
Max: (20.392, 25.000)
Размер: (12.124, 7.000)
```

`Origin` задаёт центр нулевого гекса, поэтому обычно не совпадает с `bounds.Min`. Ограничивающий
прямоугольник доходит до крайней вершины или крайнего ребра по каждой оси. Правило нечётного или
чётного смещения также может сдвинуть строку или столбец в сторону отрицательного направления
оси.

## Получить только размер

Если операции нужны размеры без положения прямоугольника, вызовите `GetBoundingBoxSize()`:

```csharp
VectorXY size = geometry.GetBoundingBoxSize();
```

Возвращаемое значение равно `geometry.GetBoundingBox().Size`. Оно подходит для выделения
выходной поверхности, начало которой задаётся отдельно. Если важно выравнивание в мировом
пространстве, используйте полный прямоугольник.

Если у вас уже есть <xref:Akeldov.Math.Hexes.HexMapTopology>, но сохранять `HexMapGeometry` не
нужно, передайте параметры размещения напрямую:

```csharp
Rectangle sameBounds = geometry.Topology.GetBoundingBox(
    geometry.Origin,
    geometry.Radius);
```

Оба размера карты должны быть больше нуля. Пустая топология допустима как хранилище, но не имеет
геометрических границ, поэтому `GetBoundingBox()` и `GetBoundingBoxSize()` выбрасывают
`ArgumentOutOfRangeException`, если ширина или высота равна нулю. Компоненты начала должны быть
конечными, радиус — конечным и положительным, а раскладка — поддерживаемой.

Получение центров ячеек и координат углов описано в рецепте
[«Получить центр и вершины гекса»](get-a-hex-center-and-vertices.md). Формулы границ карты
приведены в разделе
[«Геометрия гексагональной сетки»](../../concepts/hex-grid-model/geometry.md).
