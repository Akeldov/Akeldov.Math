# Карты

Карта гексов связывает одно значение с каждой ячейкой прямоугольной
<xref:Akeldov.Math.Hexes.HexMapTopology>. Топология задаёт допустимые индексы X/Y, раскладку и
число ячеек, а карта хранит данные. Карты подходят для ландшафта, масок, стоимостей, меток и
любого другого состояния ячеек.

## Выбор типа карты

| Тип | Назначение |
|---|---|
| `IHexMap<TValue>` | Доступ только для чтения к любой карте с топологией |
| `HexMap<TValue>` | Изменяемое хранилище произвольных значений |
| `ISpatialHexMap<TValue>` | Доступ только для чтения к значениям с геометрией мирового пространства |
| `SpatialHexMap<TValue>` | Изменяемые значения с сохранённой геометрией |
| `BoolHexMap` | Булева маска с операторами `&`, `\|`, `^` и методом `Select` |
| `IntHexMap` | Целочисленные данные со свойствами `Min`, `Max` и арифметикой |
| `FloatHexMap` | Вещественные данные с арифметикой, шумом и размытием |

Специализированные карты появились в Hexes 0.2.0 и наследуют `HexMap<TValue>`. Поэтому у них те
же правила индексирования и хранения, а передавать их можно в API, принимающие `IHexMap<bool>`,
`IHexMap<int>` или `IHexMap<float>`.

## Создание и индексирование карты

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var terrain = new HexMap<string?>(topology);
var cell = new VectorXYInt(2, 1);

terrain[cell] = "forest";
string? value = terrain[cell];
```

Конструктор с одной топологией создаёт `Topology.Count` ячеек со значением `default(TValue)`.
К одному построчному хранилищу обращаются двумя способами:

- `map[new VectorXYInt(x, y)]` проверяет координаты X/Y;
- `map[flatIndex]` использует `flatIndex = y * width + x`.

Недопустимый индекс вызывает `IndexOutOfRangeException`. Раскладка меняет интерпретацию X/Y в
гексагональной сетке, но не порядок хранения.

## Инициализация массивом

```csharp
var elevation = new IntHexMap(topology, new[]
{
    10, 11, 12, 13,
    20, 21, 22, 23,
    30, 31, 32, 33,
});
```

Длина массива должна равняться `topology.Count`. Конструктор сохраняет переданный массив без
копирования, поэтому вызывающий код и карта разделяют изменяемое хранилище. Если нужна независимая
копия, клонируйте массив. Методы `ToIntHexMap()` и `ToFloatHexMap()` создают независимые копии из
любой совместимой `IHexMap<TValue>`.

## Работа с числовыми картами

`IntHexMap` и `FloatHexMap` предоставляют `Min` и `Max`. Для пустой карты оба свойства вызывают
`InvalidOperationException`.

Операторы вычисляют каждую ячейку, возвращают новую карту и не меняют исходные:

```csharp
var baseCost = new IntHexMap(topology, new int[topology.Count]);
IntHexMap adjustedCost = (baseCost + 3) * 2;

var height = new FloatHexMap(topology, new float[topology.Count]);
FloatHexMap normalized = (height - height.Min) / (height.Max - height.Min);
```

Оба числовых типа поддерживают `+` и `-` для двух карт, скалярные `+`, `-`, `*`, `/`, а также
сложение, вычитание и умножение со скаляром слева. У карт-операндов должны совпадать топологии.
Целочисленные операции проверяют переполнение, а деление следует обычным правилам C#.

Пример нормализации предполагает, что `height.Max != height.Min`; проверяйте это условие для
реальных данных.

## Объединение масок и выбор значений

`BoolHexMap` предоставляет поячеечные операторы И, ИЛИ и исключающего ИЛИ. Метод `Select`
выбирает значения между двумя картами:

```csharp
var land = new BoolHexMap(topology, new bool[topology.Count]);
var visible = new BoolHexMap(topology, new bool[topology.Count]);
BoolHexMap visibleLand = land & visible;

var landCost = new IntHexMap(topology, new int[topology.Count]);
var waterCost = new IntHexMap(topology, new int[topology.Count]);
IntHexMap movementCost = land.Select(landCost, waterCost);
```

`Select` работает с булевыми, целочисленными, вещественными и обобщёнными ветвями
`HexMap<TValue>`. Топологии всех трёх карт должны совпадать. Метод возвращает новую
непространственную карту и не меняет исходные.

Прежние расширения `And` и `Or` доступны для интерфейсных и пространственных булевых карт. Их
пространственные перегрузки сохраняют геометрию, а операторы `BoolHexMap` возвращают
непространственные карты.

## Генерация и сглаживание вещественных полей

Детерминированный фрактальный шум Перлина создаётся непосредственно из топологии:

```csharp
FloatHexMap noise = topology.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f);

FloatHexMap smoothNoise = noise.GaussianBlur(sigma: 1.25f);
```

`CreatePerlinNoise` берёт выборки в центрах гексов единичного радиуса и возвращает значения в
диапазоне `[0, 1]`. Чем больше `scale`, тем крупнее элементы поля; `offset` выбирает другой участок
того же детерминированного поля.

`GaussianBlur` возвращает новую карту и нормализует ядро у границ. Перегрузка по умолчанию
обрезает ядро на трёх стандартных отклонениях, а перегрузка с `radius` позволяет явно задать
неотрицательный радиус в шагах гексагональной сетки.

## Сохранение геометрии мирового пространства

Используйте `SpatialHexMap<TValue>`, если значения должны хранить
<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry>. Свойство `Topology` берётся из геометрии, а
`Geometry` дополнительно сохраняет начало координат и радиус гекса:

```csharp
using Akeldov.Math.Hexes.Geometry;

var geometry = new HexMapGeometry(topology, new VectorXY(100f, 50f), radius: 8f);
var moisture = new SpatialHexMap<float>(geometry);
moisture[new VectorXYInt(0, 0)] = 0.75f;
```

Специализированные карты хранят топологию, но не геометрию. Если преобразование должно сохранить
размещение в мировом пространстве, явно создайте `SpatialHexMap<TValue>` с исходной геометрией.

## Связанные разделы

- [«Растры»](rasters.md) описывают регулярные и частичные хранилища окрестностей.
- [«Полные и частичные окрестности»](complete-and-partial-neighborhoods.md) объясняют обработку
  границ.
- [«Растеризация»](../rasterization.md) преобразует значения карты в регулярные пиксельные растры.
- [«Пространственные алгоритмы»](../spatial-algorithms/index.md) описывают поиск пути,
  хроматизацию и разбиение.
