# Добавление непроходимых гексов

В этой части руководства вы запретите маршруту входить в гексы воды и выходить из них.
Akeldov.Math.Hexes обозначает заблокированный переход значением `float.PositiveInfinity`.

## Блокировка воды

Замените два диагностических вызова `Console.WriteLine` в конце `Program.cs` следующим циклом:

```csharp
for (int index = 0; index < topology.Count; index++)
{
    if (terrain[index] != 'W')
    {
        continue;
    }

    entryCosts[index] = float.PositiveInfinity;
    exitCosts[index] = float.PositiveInfinity;
}

float waterStep = transferCosts.GetTransferCost(
    new VectorXYInt(3, 1),
    new VectorXYInt(3, 2));

Console.WriteLine(
    $"Вода непроходима: {float.IsPositiveInfinity(waterStep)}");
```

Ожидаемый результат:

```text
Вода непроходима: True
```

Карты стоимости изменились уже после создания `transferCosts`. Результат всё равно обновился,
потому что `HexTransferCostMap` сохраняет карты, а не копирует их значения.

Бесконечная стоимость входа запрещает войти в гекс, а бесконечная стоимость выхода — покинуть
его. Установка обоих значений полностью изолирует воду. Положительная бесконечность поддерживается
алгоритмом; отрицательные значения, `float.NaN` и отрицательная бесконечность недопустимы.

Оставьте цикл и удалите диагностическое вычисление перед переходом к разделу
[Поиск пути](finding-a-path.md).
