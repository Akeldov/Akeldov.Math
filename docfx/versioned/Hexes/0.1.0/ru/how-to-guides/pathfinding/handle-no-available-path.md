# Обработать отсутствие доступного пути

<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMapExtensions.FindShortestPath*> возвращает
`null`, если исходный и целевой гексы не соединяет маршрут с конечной стоимостью. Это штатный
результат поиска: проверяйте его до чтения `HexIndexes` или `TotalCost`.

## Обнаружить недостижимую цель

Средняя ячейка этой карты из одного столбца имеет бесконечную стоимость входа. Любой маршрут из
`(0, 0)` в `(0, 2)` должен был бы войти в эту ячейку, поэтому доступного маршрута нет:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 1,
    height: 3,
    layout: Layout.OddR);

var exitCosts = new HexMap<float>(topology, new[]
{
    1f,
    1f,
    1f
});

var entryCosts = new HexMap<float>(topology, new[]
{
    1f,
    float.PositiveInfinity,
    1f
});

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
var source = new VectorXYInt(0, 0);
var destination = new VectorXYInt(0, 2);

HexPath? path = transferCosts.FindShortestPath(source, destination);

if (path is null)
{
    Console.WriteLine("Маршрут до цели недоступен.");
}
else
{
    Console.WriteLine($"Стоимость маршрута: {path.TotalCost}");
}
```

Результат:

```text
Маршрут до цели недоступен.
```

Располагайте проверку на `null` рядом с поиском. В приложении эта ветвь также может очистить ранее
показанный маршрут, отключить команду перемещения или предложить выбрать другую цель.

## Повторить поиск после изменения карты

`HexTransferCostMap` сохраняет карты стоимости входа и выхода. Если временное препятствие исчезло,
измените исходную карту и повторите поиск. Пересоздавать объект стоимости переходов не нужно:

```csharp
var blocked = new VectorXYInt(0, 1);
entryCosts[blocked] = 1f;

HexPath? reopenedPath = transferCosts.FindShortestPath(source, destination);

Console.WriteLine($"Маршрут найден после открытия прохода: {reopenedPath is not null}");
Console.WriteLine($"Стоимость маршрута: {reopenedPath!.TotalCost}");
```

Теперь результат выглядит так:

```text
Маршрут найден после открытия прохода: True
Стоимость маршрута: 4
```

Каждый из двух шагов стоит `1` за выход из исходного гекса и `1` за вход в целевой.

## Отличить отсутствие маршрута от ошибки входных данных

`null` означает, что запрос и карты стоимости допустимы, но поиск не смог достичь цели. Недопустимые
входные данные приводят к исключению:

| Условие | Результат |
|---|---|
| Маршрут с конечной стоимостью не достигает цели | `null` |
| `from` или `to` находится за пределами топологии | `ArgumentOutOfRangeException` |
| Стоимость отрицательна, равна `float.NaN` или отрицательной бесконечности | `InvalidOperationException` |
| Карты входа и выхода имеют разные топологии | `ArgumentException` в конструкторе `HexTransferCostMap` |

Не перехватывайте эти исключения как признак недоступного маршрута: исправьте недопустимый индекс, стоимость
или топологию. Положительная бесконечность допустима и намеренно обозначает непроходимый вход или выход.

Если исходный и целевой гексы совпадают, результат не равен `null`: он содержит единственный индекс и имеет
нулевую стоимость при условии, что все хранимые стоимости допустимы.

Создание исходного поиска показано в разделе
[«Найти путь между двумя гексами»](find-a-path-between-two-hexes.md). Управление недоступными переходами описано в разделе
[«Исключить непроходимые гексы»](exclude-impassable-hexes.md). Полное поведение описано в разделе
[«Поиск пути»](../../concepts/spatial-algorithms/pathfinding.md).
