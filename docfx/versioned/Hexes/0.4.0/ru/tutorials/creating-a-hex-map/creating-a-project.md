# Создание проекта

В этой части руководства вы создадите консольное приложение .NET 6 и подключите
Akeldov.Math.Hexes 0.4.0. Все последующие шаги будут дополнять один файл `Program.cs`.

## Создание консольного приложения

Выполните команды в каталоге, где хотите разместить проект:

```powershell
dotnet new console --framework net6.0 --name HexMap.Tutorial
cd HexMap.Tutorial
```

Команда создаёт проект с включёнными неявными пространствами имён и nullable-анализом.

## Установка Hexes

Добавьте пакет указанной версии:

```powershell
dotnet add package Akeldov.Math.Hexes --version 0.4.0
```

Пакет подтянет совместимую версию Akeldov.Math.Spatial2D, типы которой используются для
двумерных индексов и геометрии.

Замените содержимое `Program.cs` минимальной проверкой:

```csharp
using Akeldov.Math.Hexes;

Console.WriteLine($"Hexes layout: {Layout.OddR}");
```

Запустите приложение:

```powershell
dotnet run
```

Ожидаемый результат:

```text
Hexes layout: OddR
```

Если приложение собрано и вывело имя <xref:Akeldov.Math.Hexes.Layout>, пакет подключён правильно.
Переходите к разделу [«Выбор раскладки»](choosing-a-layout.md).
