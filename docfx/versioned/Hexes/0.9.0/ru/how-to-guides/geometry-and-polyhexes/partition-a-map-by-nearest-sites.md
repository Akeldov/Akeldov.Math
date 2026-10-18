# Разбиение карты по ближайшим сайтам

Создайте `HexCenterMap` из геометрии карты, затем назначьте центры взвешенным сайтам:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;

var geometry = new HexMapGeometry(3, 1, VectorXY.Zero, 1f, Layout.OddR);
var centers = new HexCenterMap(geometry);
var sites = new[]
{
    new Site(new PointXY(0f, 0f), weight: 1f),
    new Site(new PointXY(4f, 0f), weight: 1f),
};

VoronoiHexPartitionMap partition = centers.PartitionByVoronoi(sites);
for (int x = 0; x < geometry.Topology.Resolution.X; x++)
    Console.Write($"{partition[new VectorXYInt(x, 0)]} ");
// 0 0 1

foreach (VoronoiHexPartitionCell cell in partition.Cells)
    Console.WriteLine($"Cell {cell.Id}: {cell.HexIndexes.Count} hexes");
```

Сайты и сохранённые центры должны находиться в одном координатном пространстве. Без маски
регионов сайт вне карты может получать гексы. Для конечных положительных весов минимизируется
`distance / weight`. Нулевые и бесконечные веса описаны в разделе
[«Разбиение пространства»](../../concepts/spatial-algorithms/space-partitioning.md).

## Исключение гексов маской участия

```csharp
var participationMask = new BoolHexMap(geometry.Topology, new[] { true, false, true });
PartialVoronoiHexPartitionMap partial =
    centers.PartitionByVoronoi(participationMask, sites);

int? cellId = partial[new VectorXYInt(1, 0)]; // null
bool participated = partial.Participates(new VectorXYInt(1, 0)); // false
```

Исключённые гексы изначально имеют `null` вместо идентификатора и отсутствуют в `HexIndexes`
ячеек. Результат копирует маску участия; изменение исходной маски не влияет на него.
`Participates` всегда описывает исходную маску, даже после изменения назначений.

## Ограничение назначений регионами

Маска `IHexMap<int>` ограничивает каждый гекс сайтами того же региона. Равные значения обозначают
один регион, включая несвязные области; ноль и отрицательные значения допустимы. Регион сайта
определяется гексом, содержащим его позицию. Сайт вне карты не получает гексов. Участвующий гекс
без подходящего сайта в своём регионе вызывает `InvalidOperationException`.

```csharp
var regions = new IntHexMap(geometry.Topology, new[] { 0, 0, 1 });
VoronoiHexPartitionMap regional =
    centers.PartitionByVoronoi(regions, sites,
        new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude));
PartialVoronoiHexPartitionMap selected = centers.PartitionByVoronoi(
    participationMask, regions, sites,
    new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude));

var nullableRegions = new HexMap<int?>(geometry.Topology, new int?[] { 0, null, 1 });
PartialVoronoiHexPartitionMap combined = centers.PartitionByVoronoi(
    nullableRegions, sites,
    new VoronoiHexPartitionParameters(emptyCellPolicy: EmptyCellPolicy.Exclude));
```

Топологии обеих масок должны совпадать с картой центров. С отдельной логической маской сайт
в исключённом гексе может получать участвующие гексы своего целочисленного региона.
В nullable-маске `null` исключает и гекс, и находящийся в нём сайт.

## Перераспределение эксклавов

```csharp
PartialVoronoiHexPartitionMap connected = centers.PartitionByVoronoi(
    participationMask, sites,
    new VoronoiHexPartitionParameters(
        emptyCellPolicy: EmptyCellPolicy.Exclude,
        exclavePolicy: ExclavePolicy.ReassignToClosestCell));
```

По умолчанию `ExclavePolicy.LeaveAsIs` сохраняет несвязные назначения. Перераспределение оставляет
компоненту с ближайшим к каждому сайту гексом, затем распространяется от оставленных компонент
по слоям. Переназначаемый гекс выбирает соседнюю растущую ячейку по невзвешенному расстоянию
до сайта. Равенство разрешается в пользу более раннего исходного сайта; границы регионов
и исключённые гексы сохраняются. Недостижимые компоненты становятся новыми ячейками после
исходных, в построчном порядке компонент; их сайты располагаются в гексе, ближайшем к исходному
сайту, и сохраняют его вес.

Политика пустых ячеек применяется после перераспределения. `LeaveAsIs` сохраняет пустые ячейки,
`Exclude` удаляет их и уплотняет идентификаторы, `ThrowException` отклоняет пустую ячейку.
Исходные `Id` и `SiteIndex` индексируют `Cells`; после удаления ячеек или создания ячеек эксклавов
они могут не соответствовать индексам исходных сайтов.

## Изменение назначений и копирование

Полное разбиение наследует `IntHexMap`, частичное — `HexMap<int?>`. Запись через индексатор
меняет только назначения. Сохранённые `Cells`, их `HexIndexes` и маска участия продолжают
описывать первоначальный результат.

```csharp
SpatialHexMap<int?> shared = partial; // O(1), общие геометрия и назначения.
HexMap<int?> independent = partial.ToMutableHexMap(); // Копия назначений.
BoolHexMap originalMask = partial.ToMutableParticipationMask(); // Копия исходной маски.
```

## Релаксация позиций сайтов

Передайте `new VoronoiHexPartitionParameters(relaxationIterations: 3)`, чтобы включить
релаксацию по центроидам. По умолчанию число итераций равно нулю. Один неизменяемый объект
параметров задаёт релаксацию, обработку пустых ячеек и эксклавов для полных и частичных перегрузок.

## Переход с 0.8.0

- Замените `ToVoronoiHexPartitionMap` и `ToPartialVoronoiHexPartitionMap` на `PartitionByVoronoi`.
- Передавайте маски участия и регионов перед `sites`.
- Замените отдельные аргументы политик объектом `VoronoiHexPartitionParameters` после `sites`.
- В коде соседств и растров замените `PairPresenceFlags`, `TripletPresenceFlags`,
  `SeptupletPresenceFlags` и `ChromaticTripletPresenceFlags` на `PairPresence`, `TripletPresence`,
  `SeptupletPresence` и `ChromaticTripletPresence`.
- Пересоберите потребителей: изменения API несовместимы с 0.8.0 на уровне исходников и сборок.

Генерация территорий по шуму и последующая диффузия описаны в руководстве
[«Разбиение и диффузия территорий»](partition-and-diffuse-territories.md).
