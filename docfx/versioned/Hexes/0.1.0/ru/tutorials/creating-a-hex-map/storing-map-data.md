# Хранение данных в карте

В этой части руководства вы создадите изменяемую карту и запишете символ в каждую ячейку.
Символы удобны для консольной визуализации, но `HexMap<TValue>` может хранить значения любого
типа.

## Создание и заполнение карты

После создания `topology` добавьте карту:

```csharp
var map = new HexMap<char>(topology);

for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        map[new VectorXYInt(x, y)] = '.';
    }
}
```

Конструктор создаёт по одному значению для каждой ячейки. Вложенные циклы заполняют все 35
позиций точкой, которая будет обозначать обычный гекс.

## Два индексатора

Пометьте центральный гекс:

```csharp
var center = new VectorXYInt(3, 2);
map[center] = '@';

int centerFlatIndex = center.Y * topology.Resolution.X + center.X;

Console.WriteLine($"По индексу XY: {map[center]}");
Console.WriteLine($"По плоскому индексу {centerFlatIndex}: {map[centerFlatIndex]}");
```

Результат:

```text
По индексу XY: @
По плоскому индексу 17: @
```

<xref:Akeldov.Math.Hexes.HexMap`1> предоставляет индексатор `VectorXYInt` и плоский индексатор
`int`. Плоское хранилище использует построчный порядок: сначала меняется `X`, а позиция `(x, y)`
соответствует `y * width + x`. Для прикладного кода обычно понятнее индексатор `VectorXYInt`.

Оставьте переменные `map` и `center` в `Program.cs` и переходите к разделу
[«Поиск соседей и колец»](finding-neighbors-and-rings.md).
