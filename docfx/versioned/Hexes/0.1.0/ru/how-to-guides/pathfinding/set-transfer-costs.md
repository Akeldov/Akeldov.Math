# Задать стоимость переходов

Используйте <xref:Akeldov.Math.Hexes.Pathfinding.HexTransferCostMap>, чтобы описать стоимость
каждого направленного шага по конечной гексагональной карте. Этот класс объединяет две карты
`IHexMap<float>`:

```text
cost(from → to) = ExitCosts[from] + EntryCosts[to]
```

Так одна ячейка может иметь разные стоимости входа и выхода.

## Создание карт стоимости входа и выхода

Обе карты должны использовать одну топологию. В этом примере вход в лес стоит `4`, вход на любую
другую местность — `1`, а начальная стоимость выхода из каждой ячейки равна `0`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 3,
    height: 2,
    layout: Layout.OddR);

var terrain = new[]
{
    '.', 'F', '.',
    '.', '.', '.'
};

var exitCosts = new HexMap<float>(topology);
var entryCosts = new HexMap<float>(topology);

for (int index = 0; index < topology.Count; index++)
{
    entryCosts[index] = terrain[index] == 'F' ? 4f : 1f;
}

var transferCosts = new HexTransferCostMap(exitCosts, entryCosts);
```

`HexMap<float>` заполняет ячейки значением `0`, поэтому в цикле достаточно инициализировать
только `entryCosts`. Если приложение по-разному оценивает вход и выход, заполните обе карты.

## Проверка направленной стоимости

Лес `(1, 0)` и равнина `(2, 0)` являются соседями. Рассчитайте стоимость движения в обоих
направлениях, чтобы увидеть, откуда берётся каждая часть стоимости:

```csharp
var forest = new VectorXYInt(1, 0);
var plain = new VectorXYInt(2, 0);

Console.WriteLine($"Из леса на равнину: {transferCosts.GetTransferCost(forest, plain)}");
Console.WriteLine($"С равнины в лес: {transferCosts.GetTransferCost(plain, forest)}");
```

Результат:

```text
Из леса на равнину: 1
С равнины в лес: 4
```

Первый шаг использует стоимость входа на равнину, а обратный — стоимость входа в лес. Маршрут не
учитывает стоимость входа в начальную ячейку и стоимость выхода из конечной, поскольку таких
переходов в нём нет.

`GetTransferCost(from, to)` только складывает два сохранённых значения. Метод принимает любые два
индекса внутри карты и не проверяет, являются ли ячейки соседними. Ограничение движения соседями
с общим ребром применяет `FindShortestPath`.

## Изменение стоимости во время работы

`HexTransferCostMap` сохраняет ссылки на две исходные карты. При изменении любой из них следующий
расчёт стоимости или поиск пути использует новое значение:

```csharp
exitCosts[forest] = 2f;

Console.WriteLine($"Из леса на равнину: {transferCosts.GetTransferCost(forest, plain)}");
```

Теперь результат выглядит так:

```text
Из леса на равнину: 3
```

Шаг складывает стоимость выхода из леса `2` и стоимость входа на равнину `1`.

Для обычных проходимых ячеек используйте только конечные неотрицательные значения.
`FindShortestPath` отклоняет отрицательные значения, `float.NaN` и отрицательную бесконечность.
Чтобы запретить движение с помощью положительной бесконечности, перейдите к разделу
[«Исключить непроходимые гексы»](exclude-impassable-hexes.md). Полный контракт стоимости и
направления переходов описан в разделе [«Поиск пути»](../../concepts/spatial-algorithms/pathfinding.md).
