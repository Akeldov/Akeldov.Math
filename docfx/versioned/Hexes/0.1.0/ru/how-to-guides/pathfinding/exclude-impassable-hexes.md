# Исключить непроходимые гексы

Установите стоимость входа или выхода равной `float.PositiveInfinity`, чтобы запретить
алгоритму поиска пути соответствующий переход.
<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMapExtensions.FindShortestPath*> пропускает каждый
шаг между соседними гексами, суммарная стоимость которого равна положительной
бесконечности.

```text
cost(from → to) = ExitCosts[from] + EntryCosts[to]
```

## Полностью заблокировать гекс

Если в гекс нельзя ни входить, ни выходить из него, установите обе стоимости равными
положительной бесконечности. В следующем примере заблокирован `(1, 0)`, поэтому
кратчайший путь из `(0, 0)` в `(2, 0)` проходит по нижнему ряду:

```csharp
using System.Linq;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 3,
    height: 2,
    layout: Layout.OddR);

var exitCosts = new HexMap<float>(topology);
var entryCosts = new HexMap<float>(topology, new[]
{
    1f, 1f, 1f,
    1f, 1f, 1f
});

var blocked = new VectorXYInt(1, 0);
entryCosts[blocked] = float.PositiveInfinity;
exitCosts[blocked] = float.PositiveInfinity;

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
HexPath? path = transferCosts.FindShortestPath(
    new VectorXYInt(0, 0),
    new VectorXYInt(2, 0));

Console.WriteLine($"Путь найден: {path is not null}");
Console.WriteLine($"Путь входит в заблокированный гекс: {path!.HexIndexes.Contains(blocked)}");
```

Результат:

```text
Путь найден: True
Путь входит в заблокированный гекс: False
```

`HexTransferCostMap` сохраняет ссылки на обе карты стоимости, поэтому изменения после его
создания тоже влияют на следующий поиск пути. Когда временно заблокированный гекс снова
становится проходимым, верните обеим значениям конечную неотрицательную стоимость.

## Выбрать направление блокировки

Стоимости входа и выхода действуют по-разному:

- `entryCosts[index] = float.PositiveInfinity` запрещает любому маршруту входить в гекс.
  Если маршрут начинается в нём, он всё ещё может выйти при конечной стоимости выхода.
- `exitCosts[index] = float.PositiveInfinity` запрещает любому маршруту выходить из гекса. Маршрут
  всё ещё может войти в него при конечной стоимости входа, поэтому гекс всё ещё можно использовать
  как точку назначения.
- Установка обоих значений в положительную бесконечность изолирует гекс в обоих направлениях.

Это различие позволяет моделировать как односторонние конечные точки, так и полностью
непроходимую местность. Поскольку стоимость шага складывается из стоимости выхода из исходного
гекса и стоимости входа в целевой, для блокировки направленного шага достаточно одного
бесконечного слагаемого.

## Заблокировать тип местности

Если непроходимые ячейки определены картой местности, обновите обе карты стоимости за один проход:

```csharp
for (int index = 0; index < topology.Count; index++)
{
    if (terrain[index] != 'W')
    {
        continue;
    }

    entryCosts[index] = float.PositiveInfinity;
    exitCosts[index] = float.PositiveInfinity;
}
```

Если заблокированные ячейки отделяют исходный гекс от целевого, `FindShortestPath` возвращает
`null`. Обработка этого результа показана в разделе
[«Обработать отсутствие доступного пути»](handle-no-available-path.md).

Положительная бесконечность — единственное неконечное значение, которое можно использовать как барьер.
Отрицательные стоимости, `float.NaN` и отрицательная бесконечность приводят к исключению
`InvalidOperationException` в `FindShortestPath`. Обычная стоимость переходов настраивается по инструкции
[«Задать стоимость переходов»](set-transfer-costs.md). Полный контракт поиска пути описан в разделе
[«Поиск пути»](../../concepts/spatial-algorithms/pathfinding.md).
