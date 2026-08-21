# Найти путь между двумя гексами

Вызовите
<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMapExtensions.FindShortestPath*>
для <xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap>, чтобы найти маршрут с минимальной стоимостью
между двумя ячейками. Поиск переходит только между соседними по ребру гексами и
минимизирует суммарную стоимость, а не обязательно число шагов.

## Создать карту для поиска

В следующей карте `3 × 2` средняя ячейка верхнего ряда имеет высокую стоимость.
Каждый обычный шаг стоит `2`: `1` за выход из исходного гекса и `1` за вход в целевой:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 3,
    height: 2,
    layout: Layout.OddR);

var exitCosts = new HexMap<float>(topology, new[]
{
    1f, 100f, 1f,
    1f,   1f, 1f
});

var entryCosts = new HexMap<float>(topology, new[]
{
    1f, 100f, 1f,
    1f,   1f, 1f
});

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
```

Обе карты стоимости должны использовать одну топологию. Направленный шаг из `A` в `B`
стоит `exitCosts[A] + entryCosts[B]`.

## Найти и прочитать путь

Передайте индексы начального и конечного гексов. Оба индекса должны находиться внутри топологии:

```csharp
var source = new VectorXYInt(0, 0);
var destination = new VectorXYInt(2, 0);

HexPath? path = transferCosts.FindShortestPath(source, destination);

if (path is null)
{
    Console.WriteLine("Доступного пути нет.");
    return;
}

Console.WriteLine($"Общая стоимость: {path.TotalCost}");
foreach (VectorXYInt index in path.HexIndexes)
{
    Console.WriteLine($"({index.X}, {index.Y})");
}
```

Результат:

```text
Общая стоимость: 6
(0, 0)
(0, 1)
(1, 1)
(2, 0)
```

<xref:Akeldov.Math.Hexes.Pathfinding.HexPath.HexIndexes> — доступная только для чтения последовательность,
включающая начальный и конечный гексы. <xref:Akeldov.Math.Hexes.Pathfinding.HexPath.TotalCost> — сумма
направленных стоимостей перехода между последовательными индексами.

Прямой маршрут по верхнему ряду состоит всего из двух шагов, но вход в `(1, 0)` и выход из него
увеличивают стоимость до `202`. Поэтому алгоритм выбирает обход по нижнему ряду из трёх шагов
с общей стоимостью `6`.

## Учесть особые результаты

- Если ни один маршрут с конечной стоимостью не достигает целевого гекса, `FindShortestPath`
  возвращает `null`.
- Если начальный и конечный гексы совпадают, результат содержит этот единственный индекс и
  имеет нулевую общую стоимость.
- Если есть несколько маршрутов с одинаковой минимальной стоимостью, не полагайтесь на конкретную
  последовательность индексов: гарантируется только минимальная общая стоимость.

Карты стоимости читаются заново при каждом поиске. Можно изменить стоимость местности или препятствия,
а затем снова вызвать `FindShortestPath`, не пересоздавая `HexTransferCostMap`.

Настройка используемых выше карт описана в разделе [«Задать стоимость переходов»](set-transfer-costs.md).
Блокировка ячеек показана в разделе [«Исключить непроходимые гексы»](exclude-impassable-hexes.md). При
возврате `null` перейдите к разделу [«Обработать отсутствие доступного пути»](handle-no-available-path.md).
Полный контракт алгоритма и проверок описан в разделе
[«Поиск пути»](../../concepts/spatial-algorithms/pathfinding.md).
