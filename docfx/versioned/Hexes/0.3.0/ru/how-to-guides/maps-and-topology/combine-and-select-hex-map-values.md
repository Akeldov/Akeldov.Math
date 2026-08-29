# Объединение карт и выбор значений

Объединяйте маски операторами логических карт, а затем применяйте `Select`, чтобы выбрать одно из
двух значений для каждой ячейки. Обычные перегрузки возвращают обычные карты, а специализированные
пространственные перегрузки сохраняют геометрию.

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
BoolHexMap hidden = !visible;
```

Операторы выполняются поячеечно и создают новые карты. У обычных операндов должны совпадать
топологии.

## Выбор обычных значений

```csharp
var landCost = new IntHexMap(topology, new[] { 1, 1, 1, 2, 2, 2 });
var waterCost = new IntHexMap(topology, new[] { 8, 8, 8, 9, 9, 9 });

IntHexMap movementCost = land.Select(landCost, waterCost);
```

Если `land[index]` равно `true`, результат получает `landCost[index]`, иначе —
`waterCost[index]`. Специализированные перегрузки возвращают `BoolHexMap`, `IntHexMap` или
`FloatHexMap`.

Обобщённая обычная перегрузка принимает две карты `HexMap<TValue>`:

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

Топологии всех трёх карт должны совпадать. Эти перегрузки возвращают новую обычную карту.

## Выбор пространственных значений

Используйте условие `SpatialBoolHexMap` и соответствующие специализированные пространственные
ветви, если результат должен сохранять положение в мировом пространстве:

```csharp
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 8f);

SpatialBoolHexMap spatialLand = land.ToSpatialHexMap(geometry);
SpatialIntHexMap spatialLandCost = landCost.ToSpatialHexMap(geometry);
SpatialIntHexMap spatialWaterCost = waterCost.ToSpatialHexMap(geometry);

SpatialIntHexMap spatialMovementCost = spatialLand.Select(
    spatialLandCost,
    spatialWaterCost);
```

Специализированные пространственные перегрузки доступны для логических, целочисленных и
вещественных ветвей. У условия, `whenTrue` и `whenFalse` должна совпадать вся геометрия: топология,
начало координат и радиус. Результат будет пространственным и сохранит эту геометрию. Обобщённой
перегрузки выбора для `SpatialHexMap<TValue>` нет.

Каждая перегрузка `Select` создаёт независимое хранилище результата и не меняет исходные карты.
