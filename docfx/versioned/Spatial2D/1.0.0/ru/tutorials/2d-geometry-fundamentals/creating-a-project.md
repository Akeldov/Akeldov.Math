# Создание проекта

В этом учебнике мы создадим небольшое консольное приложение с
`Akeldov.Math.Spatial2D`. Перед началом установите .NET SDK 6 или новее.

## Создание консольного проекта

Выполните команды в терминале:

```powershell
dotnet new console --name Spatial2D.Fundamentals
Set-Location Spatial2D.Fundamentals
dotnet add package Akeldov.Math.Spatial2D --version 1.0.0
```

Пакет поддерживает .NET 6 и .NET Standard 2.1, поэтому его можно использовать и в других
совместимых проектах .NET.

## Проверка настройки

Замените содержимое `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D;

var point = new PointXY(2f, 3f);

Console.WriteLine($"Spatial2D готов: ({point.X}, {point.Y})");
```

Запустите приложение:

```powershell
dotnet run
```

Ожидаемый вывод:

```text
Spatial2D готов: (2, 3)
```

Проект готов. Переходите к разделу [«Точки и векторы»](points-and-vectors.md).
