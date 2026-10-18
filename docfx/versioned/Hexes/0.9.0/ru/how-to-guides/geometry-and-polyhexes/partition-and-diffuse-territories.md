# Разбиение и диффузия территорий

`PartitionByPerlinNoise` распределяет территории по относительным долям площади,
а `DiffuseByCellularPotts` изменяет их границы. Обе операции возвращают новые карты.

## Создание территорий по шуму

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.PerlinNoise;
using Akeldov.Math.Hexes.Partitioning.CellularPotts;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(64, 48, VectorXY.Zero, 1f, Layout.OddR);
var targets = new[]
{
    new PerlinNoisePartitionTarget(7, 30),
    new PerlinNoisePartitionTarget(42, 50),
    new PerlinNoisePartitionTarget(99, 20),
};
var noise = new PerlinNoisePartitionParameters(scale: 18f);
var full = geometry.PartitionByPerlinNoise(targets, noise, seed: 42);
```

Порядок целей задаёт полосы от низкого шума к высокому. Доли нормализуются: 30/50/20
эквивалентны 0.3/0.5/0.2. Одинаковые значения шума не разделяются, поэтому фактические площади
могут отличаться от заданных пропорций. ID не обязаны идти подряд: ищите ячейку по `Id`,
не используйте произвольный ID территории как индекс `Cells`. Масштаб шума задан в мировых единицах.

## Исключение гексов и разбиение по регионам

```csharp
var participation = new BoolHexMap(geometry.Topology);
var regions = new IntHexMap(geometry.Topology);
for (int y = 0; y < 48; y++)
for (int x = 0; x < 64; x++)
{
    var index = new VectorXYInt(x, y);
    participation[index] = x > 0 && y > 0;
    regions[index] = x < 32 ? 0 : 1;
}

var partial = geometry.PartitionByPerlinNoise(participation, regions, targets, noise, seed: 42);
// Без регионов: geometry.PartitionByPerlinNoise(participation, targets, noise, seed: 42).
```

Пороги и доли вычисляются отдельно в каждом регионе. Ячейка одной цели собирает её гексы
из всех регионов. Вместо двух масок можно передать nullable-карту регионов: null исключает
гекс, любое целое число обозначает регион. Топологии масок должны совпадать с геометрией.

## Диффузия границ без перехода между регионами

```csharp
var diffusion = new CellularPottsParameters(
    stepCount: 20, temperature: 0.5f, boundaryEnergy: 1f, areaPenalty: 1f,
    connectivity: CellularPottsConnectivity.None);

var evolved = partial.DiffuseByCellularPotts(regions, diffusion, new Random(42));
var evolvedFull = full.DiffuseByCellularPotts(regions, diffusion, new Random(42));

var targetAreas = new Dictionary<int, float> { [7] = 800f };
var resized = partial.DiffuseByCellularPotts(regions, targetAreas, diffusion, new Random(42));
```

Территории по шуму могут быть исходно несвязными, поэтому пример явно задаёт `None`.
Политика `Preserve` по умолчанию требует исходной связности каждой непустой ячейки по шести
соседям и запрещает её распад и исчезновение. Пустые ячейки не могут получить гексы.

Один шаг выполняет по одной попытке копирования на участвующий гекс с выборкой с возвращением.
ID регионов ограничивают копирование между соседями, но не изолируют расчёт энергии, площади
и связности. Null-гексы остаются null и не вносят граничную энергию. Дополнительной маски
участия нет: участие определяется исходными назначениями, которые должны соответствовать `Cells`.

Целевые площади — мягкие предпочтения в количестве гексов. Для пропущенных ID берутся исходные
площади. Точное число гексов и минимум энергии не гарантируются. Для работы без региональных
ограничений опустите `regions` в полной или частичной перегрузке. Для воспроизводимости
передавайте `Random` с заданным seed.

## Разделение несвязных ячеек

```csharp
var connected = evolved.SplitDisconnectedCells();
var connectedFull = evolvedFull.SplitDisconnectedCells();
var preserved = connected.DiffuseByCellularPotts(
    new CellularPottsParameters(stepCount: 10), new Random(42));
```

Разделение создаёт отдельную `HexPartitionCell` для каждой компоненты связности по шести соседям,
назначает ID от нуля в порядке построчного обнаружения, пропускает пустые ячейки и сохраняет null.
Исходные ID территорий и специфичные метаданные не переносятся. Выполните разделение перед
диффузией, если каждая компонента должна стать отдельной сохраняемой ячейкой. Сама диффузия
сохраняет исходные ID и порядок ячеек, возвращает новые снимки и не меняет исходную карту.
