# Назначение стоимости переходов

В этой части руководства вы преобразуете тип местности в стоимость направленного перехода.
Цена одного шага равна сумме стоимости выхода из текущего гекса и стоимости входа в соседний.

## Создание карт стоимости

Замените `Console.WriteLine` в конце `Program.cs` следующим кодом:

```csharp
static float GetEntryCost(char terrain) => terrain switch
{
    '.' => 1f,
    'F' => 4f,
    'W' => 1f,
    _ => throw new ArgumentOutOfRangeException(nameof(terrain))
};

var exitCosts = new HexMap<float>(topology);
var entryCosts = new HexMap<float>(topology);

for (int index = 0; index < topology.Count; index++)
{
    exitCosts[index] = 0f;
    entryCosts[index] = GetEntryCost(terrain[index]);
}

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);

float plainStep = transferCosts.GetTransferCost(
    new VectorXYInt(0, 0),
    new VectorXYInt(1, 0));
float forestStep = transferCosts.GetTransferCost(
    new VectorXYInt(1, 1),
    new VectorXYInt(2, 1));

Console.WriteLine($"Вход на равнину: {plainStep}");
Console.WriteLine($"Вход в лес: {forestStep}");
```

Ожидаемый результат:

```text
Вход на равнину: 1
Вход в лес: 4
```

Все стоимости выхода равны нулю, поэтому цена перехода полностью определяется местностью, в
которую входит маршрут. Каждый посещённый после старта гекс равнины добавляет `1`, а леса — `4`.
Сам начальный гекс не оплачивается, потому что маршрут в него не входит.

<xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap> сохраняет ссылки на обе изменяемые карты.
Поэтому следующий поиск увидит изменения в `entryCosts` и `exitCosts`.

Пока вода получила временную конечную стоимость. В разделе
[Добавление непроходимых гексов](adding-impassable-hexes.md) вы превратите её в препятствие.
