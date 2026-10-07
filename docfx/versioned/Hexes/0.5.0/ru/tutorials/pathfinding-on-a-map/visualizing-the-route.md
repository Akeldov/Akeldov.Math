# Визуализация маршрута

В заключительной части руководства вы наложите успешный маршрут на карту местности. Перед
нечётными строками добавляется один пробел, чтобы приблизительно показать раскладку `OddR` в
терминале.

## Вывод результата

Добавьте этот код после восстановления стоимости входа в цель:

```csharp
var route = new HashSet<VectorXYInt>(path.HexIndexes);

Console.WriteLine();
Console.WriteLine("Обозначения: S = старт, G = цель, * = путь, F = лес, # = вода");

for (int y = 0; y < topology.Resolution.Y; y++)
{
    if ((y & 1) == 1)
    {
        Console.Write(' ');
    }

    for (int x = 0; x < topology.Resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        char symbol = terrain[index] == 'W' ? '#' : terrain[index];

        if (route.Contains(index))
        {
            symbol = '*';
        }

        if (index == start)
        {
            symbol = 'S';
        }
        else if (index == goal)
        {
            symbol = 'G';
        }

        Console.Write($"{symbol} ");
    }

    Console.WriteLine();
}

Console.WriteLine($"Общая стоимость: {path.TotalCost}");
```

Заключительная часть вывода:

```text
Обозначения: S = старт, G = цель, * = путь, F = лес, # = вода
. . . . . . .
 . . F F F . .
S * F # F . G
 . * F F F * .
. . * * * * .
Общая стоимость: 8
```

Маршрут обходит и непроходимую воду, и более дорогой лес. Начальные пробелы нужны только для
вывода в терминал; настоящее соседство определяется топологией.

Теперь у вас есть полный пример поиска пути с учётом местности. Подробнее о модели стоимости,
проверке значений и односторонних ограничениях читайте в разделе
[Поиск пути](../../concepts/spatial-algorithms/pathfinding.md).
