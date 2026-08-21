# Объединить карты и выбрать значения

Объединяйте маски операторами `BoolHexMap`, а затем применяйте `Select`, чтобы выбрать одно из
двух значений для каждой ячейки.

## Объединение масок

```csharp
using Akeldov.Math.Hexes;

var topology = new HexMapTopology(3, 2, Layout.OddR);

var land = new BoolHexMap(topology, new[]
{
    true,  true,  false,
    true,  false, false,
});

var visible = new BoolHexMap(topology, new[]
{
    true, false, true,
    true, true,  false,
});

BoolHexMap visibleLand = land & visible;
BoolHexMap eitherCondition = land | visible;
BoolHexMap exactlyOneCondition = land ^ visible;
```

Операторы выполняются поячеечно и создают новые карты. Топологии обоих операндов должны
совпадать.

## Выбор числовых значений

```csharp
var landCost = new IntHexMap(topology, new[] { 1, 1, 1, 2, 2, 2 });
var waterCost = new IntHexMap(topology, new[] { 8, 8, 8, 9, 9, 9 });

IntHexMap movementCost = land.Select(landCost, waterCost);
```

Если `land[index]` равно `true`, результат получает `landCost[index]`, иначе —
`waterCost[index]`. Специализированные перегрузки возвращают `BoolHexMap`, `IntHexMap` или
`FloatHexMap`.

Обобщённая перегрузка принимает две карты `HexMap<TValue>`:

```csharp
var landLabels = new HexMap<string>(topology, new[]
{
    "plain", "forest", "plain", "hill", "hill", "plain",
});
var waterLabels = new HexMap<string>(topology, new[]
{
    "sea", "sea", "lake", "sea", "lake", "sea",
});

HexMap<string> terrain = land.Select(landLabels, waterLabels);
```

Топологии всех трёх карт должны совпадать. `Select` возвращает новую непространственную карту и
не меняет исходные карты.
