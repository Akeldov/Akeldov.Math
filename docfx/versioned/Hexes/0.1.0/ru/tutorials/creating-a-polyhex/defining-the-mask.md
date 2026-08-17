# Формирование маски

В этой части руководства вы создадите консольное приложение и опишете фигуру прямоугольной
логической маской. Значение `true` означает, что Q/R-ячейка входит в полигекс.

## Создание проекта

Выполните команды в каталоге, где должен находиться проект:

```powershell
dotnet new console --framework net6.0 --name Polyhex.Tutorial
cd Polyhex.Tutorial
dotnet add package Akeldov.Math.Hexes --version 0.1.0
```

## Добавление маски

Замените содержимое `Program.cs` следующим кодом:

```csharp
using Akeldov.Math.Hexes.Topology;

bool[,] mask =
{
    { false, true,  true,  false }, // q = 0, r = 0..3
    { true,  true,  true,  true  }, // q = 1, r = 0..3
    { true,  true,  false, true  }, // q = 2, r = 0..3
    { false, true,  true,  false }  // q = 3, r = 0..3
};

Console.WriteLine(
    $"Маска: {mask.GetLength(0)} x {mask.GetLength(1)}");
```

Запустите приложение:

```powershell
dotnet run
```

Ожидаемый результат:

```text
Маска: 4 x 4
```

Первое измерение массива соответствует Q, второе — R. Например, `mask[2, 2]` — внутренняя
незанятая ячейка фигуры. Её производная координата S равна `-2 - 2`, но отдельного третьего
измерения массива для S нет.

Локальная Q/R-маска не использует <xref:Akeldov.Math.Hexes.Layout>. Раскладка потребуется только
при размещении фигуры в двумерном координатном пространстве.

Переходите к разделу [Построение топологии полигекса](building-polyhex-topology.md).
