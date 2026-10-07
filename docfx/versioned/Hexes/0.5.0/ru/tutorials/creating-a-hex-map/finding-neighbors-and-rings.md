# Поиск соседей и колец

В этой части руководства вы отметите шесть соседей центрального гекса и ячейки на расстоянии два.
Соседство зависит от раскладки, а расстояние удобнее вычислять после преобразования индексов в
QRS-координаты.

## Непосредственные соседи

Добавьте пространство имён топологии в начало `Program.cs`:

```csharp
using Akeldov.Math.Hexes.Topology;
```

После создания `map` и `center` получите соседние индексы:

```csharp
foreach (VectorXYInt neighbor in center.GetAdjacents(topology.Layout))
{
    if (IsInside(neighbor, topology))
    {
        map[neighbor] = '1';
    }
}
```

`GetAdjacents` возвращает шесть соседей на бесконечной сетке. Для гекса у границы некоторые
индексы могут лежать за пределами конечной карты, поэтому перед обращением к индексатору нужна
проверка:

```csharp
static bool IsInside(VectorXYInt index, HexMapTopology topology) =>
    index.X >= 0 &&
    index.X < topology.Resolution.X &&
    index.Y >= 0 &&
    index.Y < topology.Resolution.Y;
```

## Второе кольцо

В пакете нет отдельного метода построения кольца. Пройдите по конечной карте и выберите
индексы, QRS-расстояние которых от центра равно двум:

```csharp
VectorQRSInt centerQrs = center.ToQRSIndex(topology.Layout);

for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        VectorQRSInt indexQrs = index.ToQRSIndex(topology.Layout);

        if (GetHexDistance(centerQrs, indexQrs) == 2)
        {
            map[index] = '2';
        }
    }
}
```

Добавьте функцию расстояния рядом с `IsInside`:

```csharp
static int GetHexDistance(VectorQRSInt first, VectorQRSInt second)
{
    int deltaQ = Math.Abs(first.Q - second.Q);
    int deltaR = Math.Abs(first.R - second.R);
    int deltaS = Math.Abs(first.S - second.S);

    return Math.Max(deltaQ, Math.Max(deltaR, deltaS));
}
```

Максимум модулей разностей трёх компонент равен числу шагов между гексами. Такой расчёт не
зависит от `OddR`, `EvenR`, `OddQ` или `EvenQ`; раскладка участвует только в преобразовании исходных
`VectorXYInt`.

Переходите к разделу [«Визуализация карты»](visualizing-the-map.md), чтобы увидеть отмеченные
кольца.
