# Создать хроматическую карту

Используйте <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>, чтобы заранее вычислить
трёхцветный класс каждой ячейки конечной гексагональной топологии. Такая карта удобна для повторных
запросов, раздельных проходов обработки и последующей визуализации.

## Создать карту из топологии

Передайте исходную топологию конструктору:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var chromaticMap = new ChromaticIndexMap(topology);
```

При создании для каждой ячейки топологии вычисляется одно значение `byte`: `0`, `1` или `2`. Карта
сохраняет разрешение и раскладку в свойстве `Topology`. Она реализует доступный только для чтения
контракт `ISpatialHexMap<byte>`, поэтому классы можно читать, но нельзя заменять.

Конструктор с одной топологией добавляет стандартное пространственное размещение с единичным радиусом
гекса. Эта геометрия не влияет на классы, а лишь позволяет использовать результат в пространственных API.

## Прочитать сохранённые классы

Читайте класс по индексу `(X, Y)` или по плоскому индексу в порядке строк:

```csharp
var index = new VectorXYInt(2, 1);

byte byCoordinates = chromaticMap[index];
int flatIndex = index.Y * chromaticMap.Topology.Resolution.X + index.X;
byte byFlatIndex = chromaticMap[flatIndex];

Console.WriteLine(byCoordinates); // 1
Console.WriteLine(byCoordinates == byFlatIndex); // True
```

Координатный индексатор проверяет обе компоненты и выбрасывает `IndexOutOfRangeException` за пределами
конечной топологии. В отличие от `GetChromaticClass`, `ChromaticIndexMap` не предоставляет классы для
отрицательных или других индексов вне карты.

## Обработать по одному классу за проход

Выполните три прохода, если одновременно изменяемые ячейки не должны иметь общего ребра:

```csharp
for (byte classIndex = 0; classIndex < 3; classIndex++)
{
    for (int y = 0; y < topology.Resolution.Y; y++)
    {
        for (int x = 0; x < topology.Resolution.X; x++)
        {
            var index = new VectorXYInt(x, y);

            if (chromaticMap[index] != classIndex)
                continue;

            Console.WriteLine($"Проход {classIndex}: {index}");
        }
    }
}
```

В одном проходе никакие две выбранные ячейки не являются непосредственными соседями по ребру. Но
проход не становится независимым, если операция читает или изменяет окрестность глубже одного ребра.

## Сохранить существующую пространственную геометрию

Создайте карту из `HexMapGeometry`, если её радиус и начало мировых координат должны совпадать с другой
картой или растром:

```csharp
using Akeldov.Math.Hexes.Geometry;

var geometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var spatialChromaticMap = new ChromaticIndexMap(geometry);

Console.WriteLine(spatialChromaticMap.Geometry == geometry); // True
```

Классы по-прежнему зависят только от индекса и раскладки, а `Geometry` управляет последующей
пространственной выборкой. Перейдите к разделу
[«Создать хроматический растр»](create-a-chromatic-raster.md), чтобы перенести хроматические данные
на прямоугольную сетку, или к [«Получить хроматический индекс гекса»](get-a-hex-chromatic-index.md)
для разового вычисления. Полная модель описана в разделе
[«Хроматизация»](../../concepts/spatial-algorithms/chromatization.md).
