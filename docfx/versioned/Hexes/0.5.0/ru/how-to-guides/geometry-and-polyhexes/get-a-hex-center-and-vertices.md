# Получить центр и вершины гекса

Используйте начало, радиус и раскладку геометрии карты, чтобы преобразовать индекс хранилища в
центр гекса в мировом пространстве. Затем разверните центр в шесть вершин гекса.

## Получить центр

Создайте <xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> и выберите ячейку по её индексу
хранилища <xref:Akeldov.Math.Spatial2D.VectorXYInt>:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var index = new VectorXYInt(2, 1);

VectorXY center = index.GetHexCenter(
    geometry.Radius,
    geometry.Origin,
    geometry.Topology.Layout);
```

Для этой геометрии `center` приблизительно равен `(18.660, 23.000)`. `Origin` задаёт центр
нулевого гекса, а не минимальный угол карты. Используйте радиус, начало и полную раскладку одной
геометрии: смешивание параметров разных сеток даст правдоподобную, но неверную позицию.

## Получить шесть вершин

Вызовите `GetHexVertices` для центра и передайте те же радиус и раскладку:

```csharp
VectorXY[] vertices = center.GetHexVertices(
    geometry.Radius,
    geometry.Topology.Layout);

for (int i = 0; i < vertices.Length; i++)
{
    VectorXY vertex = vertices[i];
    Console.WriteLine(FormattableString.Invariant(
        $"Вершина {i}: ({vertex.X:F3}, {vertex.Y:F3})"));
}
```

Результат:

```text
Вершина 0: (20.392, 24.000)
Вершина 1: (18.660, 25.000)
Вершина 2: (16.928, 24.000)
Вершина 3: (16.928, 22.000)
Вершина 4: (18.660, 21.000)
Вершина 5: (20.392, 22.000)
```

Метод возвращает новый изменяемый массив, принадлежащий вызывающему коду. Шесть вершин
упорядочены против часовой стрелки. Для `OddR` и `EvenR` вершина `0` находится под углом 30
градусов к положительной оси X; для `OddQ` и `EvenQ` она лежит на положительной оси X.

Радиус должен быть конечным и больше нуля. Компоненты начала и центра должны быть конечными, а
раскладка — одной из `OddR`, `EvenR`, `OddQ` или `EvenQ`.

Работа с QRS-координатами вместо индексов хранилища описана в рецепте
[«Преобразовать QRS в координаты Spatial2D»](../coordinates-and-layouts/convert-qrs-to-spatial2d-coordinates.md).
Формулы и правила ориентации приведены в разделе
[«Геометрия гексагональной сетки»](../../concepts/hex-grid-model/geometry.md).
