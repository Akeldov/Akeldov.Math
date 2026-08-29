# Использование специализированных карт гексов

Используйте логические, целочисленные и вещественные специализации, когда карте нужны поячеечные
операции. Выбирайте пространственную специализацию, если результаты должны сохранять положение в
мировом пространстве.

## Создание карт

```csharp
using Akeldov.Math.Hexes;

var topology = new HexMapTopology(3, 2, Layout.OddR);

var blocked = new BoolHexMap(topology, new[]
{
    false, true,  false,
    false, false, true,
});

var movementCost = new IntHexMap(topology, new[]
{
    1, 4, 2,
    3, 1, 5,
});

var elevation = new FloatHexMap(topology, new[]
{
    0.1f, 0.4f, 0.2f,
    0.7f, 0.5f, 0.9f,
});
```

Как и `HexMap<TValue>`, каждый конструктор сохраняет переданный массив. Сначала клонируйте массив,
если источник и карта не должны совместно владеть изменяемым хранилищем.

## Преобразование числовых значений

```csharp
IntHexMap doubledCost = movementCost * 2;
IntHexMap pairwiseCost = movementCost * movementCost;
FloatHexMap weightedElevation = (elevation + movementCost) / 2f;

IntHexMap boundedCost = movementCost.Clamp(2, 4);
FloatHexMap normalizedElevation = elevation.Rescale(0f, 1f);
```

Операторы и методы диапазона создают новые карты. У карт-операндов должны совпадать топологии.
Смешанная операция целочисленной и вещественной карт возвращает вещественную карту. `Rescale`
переносит текущие экстремумы в запрошенный диапазон; постоянная карта заполняется новым минимумом.

## Построение масок сравнениями

```csharp
BoolHexMap high = elevation >= 0.5f;
BoolHexMap affordable = 3 >= movementCost;
BoolHexMap higherThanCost = elevation > movementCost;
BoolHexMap usable = !blocked & affordable;
```

`<`, `>`, `<=` и `>=` сравнивают ячейки и возвращают логическую карту. Сравнения с константой
работают в любом порядке операндов. `==` и `!=` не являются поячеечными операторами.

## Обработка связных областей

```csharp
BoolHexMap expanded = usable.Dilate();
BoolHexMap cleaned = usable.Open();
BoolHexMap boundary = usable.Outline();

BoolHexMap selected = usable.FloodFill(new VectorXYInt(0, 0));
(IntHexMap labels, int componentCount) = usable.ConnectedComponents();
IntHexMap distance = usable.DistanceTransform(targetValue: true);
```

Морфология и связность используют соседство по шести рёбрам. `FloodFill` следует логическому
значению начальной ячейки, метки компонент детерминированы, а недостижимое расстояние равно
`int.MaxValue`.

## Сохранение геометрии

```csharp
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 8f);

SpatialFloatHexMap spatialElevation = elevation.ToSpatialHexMap(geometry);
SpatialFloatHexMap spatialWeighted = spatialElevation + movementCost;
SpatialBoolHexMap spatialHigh = spatialWeighted > 2f;

FloatHexMap detachedCopy = spatialWeighted.ToHexMap();
```

Для пары пространственной и обычной карт должны совпадать топологии; результат будет
пространственным и сохранит геометрию пространственного операнда. У двух пространственных карт
должна совпадать вся геометрия. Методы преобразования копируют значения в независимое хранилище.

Далее [объедините маски и выберите значения](combine-and-select-hex-map-values.md).
