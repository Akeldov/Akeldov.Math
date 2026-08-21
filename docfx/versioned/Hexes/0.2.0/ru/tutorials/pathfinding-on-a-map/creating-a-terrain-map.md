# Создание карты местности

В этой части руководства вы создадите консольное приложение и сохраните карту местности размером
7 на 5 гексов. Точка обозначает равнину, `F` — лес, а `W` — воду.

## Создание проекта

Выполните команды в каталоге, где должен находиться проект:

```powershell
dotnet new console --framework net6.0 --name HexPathfinding.Tutorial
cd HexPathfinding.Tutorial
dotnet add package Akeldov.Math.Hexes --version 0.2.0
```

Вместе с Akeldov.Math.Hexes устанавливается совместимая версия Akeldov.Math.Spatial2D. Из неё
берётся используемый ниже тип индекса `VectorXYInt`.

## Хранение местности

Замените содержимое `Program.cs` следующим кодом:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Pathfinding;
using Akeldov.Math.Spatial2D;

string[] terrainRows =
{
    ".......",
    "..FFF..",
    "..FWF..",
    "..FFF..",
    "......."
};

var topology = new HexMapTopology(
    width: terrainRows[0].Length,
    height: terrainRows.Length,
    layout: Layout.OddR);

var terrain = new HexMap<char>(
    topology,
    string.Concat(terrainRows).ToCharArray());

var start = new VectorXYInt(0, 2);
var goal = new VectorXYInt(6, 2);

Console.WriteLine(
    $"Карта: {topology.Resolution.X} x {topology.Resolution.Y}; " +
    $"маршрут: {start} -> {goal}");
```

Запустите приложение:

```powershell
dotnet run
```

Ожидаемый результат:

```text
Карта: 7 x 5; маршрут: (0, 2) -> (6, 2)
```

`HexMap<char>` хранит объединённые строки в порядке строк: сначала меняется `X`, затем `Y`.
Топология задаёт размеры карты и правила соседства, а `start` и `goal` обозначают концы будущего
маршрута.

Оставьте эти объявления в `Program.cs` и переходите к разделу
[Назначение стоимостей переходов](assigning-transfer-costs.md).
