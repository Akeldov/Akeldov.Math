# Найти соседей гекса

Используйте `GetAdjacents(layout)`, чтобы получить шесть индексов гексов, смежных по ребру с
выбранным `VectorXYInt`. Передавайте раскладку топологии: смещения соседей зависят от принятого в
ней соглашения о строках или столбцах.

## Получение шести соседей

```csharp
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);
var center = new VectorXYInt(0, 0);

VectorXYInt[] allNeighbors = center.GetAdjacents(topology.Layout);
```

`GetAdjacents` вычисляет смежность на бесконечной гексагональной сетке. Каждый вызов возвращает
новый изменяемый массив, принадлежащий вызывающему коду.

## Учёт границ карты

Соседи гекса у края могут лежать за пределами конечной карты. Отфильтруйте их перед
использованием в качестве индексов `HexMap<TValue>`:

```csharp
var inBoundsNeighbors = new List<VectorXYInt>();

foreach (VectorXYInt neighbor in allNeighbors)
{
    if (neighbor.X >= 0 && neighbor.X < topology.Resolution.X &&
        neighbor.Y >= 0 && neighbor.Y < topology.Resolution.Y)
    {
        inBoundsNeighbors.Add(neighbor);
    }
}
```

Для углового гекса `(0, 0)` в раскладке `OddR` список из примера содержит `(1, 0)` и `(0, 1)`.
Остальные четыре индекса существуют на бесконечной сетке, но лежат за пределами топологии.
`GetAdjacents` не обрезает и не переносит их циклически; обращение с таким индексом к карте
выбрасывает `IndexOutOfRangeException`.

Для большего расстояния переходите к рецепту
[«Получить кольцо заданного радиуса»](get-a-ring-of-a-given-radius.md). Зависимость смежности от
раскладки описана в разделе
[«Индексы строк и столбцов»](../../concepts/fundamentals/coordinate-systems/row-and-column-indices.md),
а границы конечной карты — в разделе [«Топология»](../../concepts/hex-grid-model/topology.md).
