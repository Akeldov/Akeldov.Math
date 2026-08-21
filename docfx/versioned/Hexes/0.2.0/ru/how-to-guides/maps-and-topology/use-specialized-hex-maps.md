# Использовать специализированные карты

Используйте `BoolHexMap`, `IntHexMap` и `FloatHexMap` для масок и числовых полей. Эти типы
сохраняют правила индексирования `HexMap<TValue>` и добавляют операции для своего типа значений.

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

Как и `HexMap<TValue>`, карта сохраняет переданный массив как внутреннее хранилище. Если
последующие изменения исходного массива не должны влиять на карту, заранее клонируйте его.

## Чтение и преобразование числовых карт

```csharp
Console.WriteLine($"Costs: {movementCost.Min}..{movementCost.Max}");
Console.WriteLine($"Elevation: {elevation.Min}..{elevation.Max}");

IntHexMap doubledCost = movementCost * 2;
FloatHexMap shiftedElevation = elevation + 0.25f;
```

Каждый оператор возвращает новую изменяемую карту. Для операций над двумя картами их топологии
должны совпадать. `IntHexMap` использует проверяемую арифметику, поэтому переполнение вызывает
`OverflowException`.

## Копирование карты, заданной интерфейсом

```csharp
IHexMap<int> sourceCosts = movementCost;
IntHexMap editableCopy = sourceCosts.ToIntHexMap();

IHexMap<float> sourceElevation = elevation;
FloatHexMap editableFloatCopy = sourceElevation.ToFloatHexMap();
```

Методы преобразования копируют значения в независимое хранилище. Последующие изменения исходной
и результирующей карт не влияют друг на друга.

Далее [объедините маски и выберите значения](combine-and-select-hex-map-values.md).
