# Построение замкнутого контура

На этом шаге учебника вы замкнёте цепочку из двух путей с предыдущего шага и превратите её в
контур. Продолжайте работу в проекте `Spatial2D.Fundamentals`, где в `Program.cs` уже объявлены
преобразованные пути `worldEdge` и `worldSide`.

## Добавление недостающих путей

Существующая цепочка начинается в преобразованной точке `start` и заканчивается в преобразованной
точке `end`. Добавьте верхнюю левую вершину в локальных координатах, преобразуйте её той же
функцией `ToWorld` и соедините конец цепочки с её началом:

```csharp
var upperLeft = new PointXY(1f, 4f);
PointXY worldUpperLeft = ToWorld(upperLeft);

var worldTopEdge = new ParameterizedSegment(
    worldSide.EndPoint,
    worldUpperLeft);

var worldLeftEdge = new ParameterizedSegment(
    worldUpperLeft,
    worldEdge.StartPoint);
```

Теперь четыре пути соединяются в следующем порядке:

```text
worldEdge -> worldSide -> worldTopEdge -> worldLeftEdge -> worldEdge
```

`EndPoint` каждого пути совпадает со `StartPoint` следующего. Последний отрезок заканчивается в
начале первого пути, поэтому цепочка замкнута.

## Создание контура

Добавьте пространство имён контуров в начало `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D.Contours;
```

Замените массив `openBoundary` с предыдущего шага замкнутой границей и передайте её конструктору
<xref:Akeldov.Math.Spatial2D.Contours.CompositeContour>:

```csharp
IFinitePath[] closedBoundary =
{
    worldEdge,
    worldSide,
    worldTopEdge,
    worldLeftEdge
};

var contour = new CompositeContour(closedBoundary);

Console.WriteLine($"Количество путей: {contour.Curves.Count}");
Console.WriteLine($"Длина границы:    {contour.Length}");
```

Свойство `Curves` сохраняет порядок обхода в виде структурного представления только для чтения.
`Length` равен сумме длин четырёх путей в мировых единицах; вклад кривой Безье основан на
используемом библиотекой приближении.

Конструктор сразу проверяет всю цепочку. Если пути отсутствуют, не соединяются или расположены в
неправильном порядке, он выбрасывает `ArgumentException`. При проверке соединений используется
стандартный геометрический допуск Spatial2D, поэтому концы, полученные обычными вычислениями с
плавающей точкой, также могут образовать допустимую цепочку.

## Проверка замыкающего соединения

Конструктор контура уже гарантировал замкнутость, но при знакомстве с API соединение можно
проверить явно:

```csharp
IFinitePath firstPath = contour.Curves[0];
IFinitePath lastPath = contour.Curves[contour.Curves.Count - 1];

bool closes = lastPath.EndPoint.AlmostEquals(firstPath.StartPoint);
Console.WriteLine($"Контур замкнут:    {closes}");
```

Последняя строка вывода:

```text
Контур замкнут:    True
```

Не добавляйте после `worldLeftEdge` ещё один отрезок нулевой длины. Для замкнутой цепочки должны
совпадать начальное и конечное положения, а повторный путь не требуется.

## Использование непрерывной координаты контура

`CompositeContour` описывает границу, но не задаёт для неё общую координату обхода. Если требуется
непрерывно перемещаться по всем четырём путям, создайте из того же массива
<xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCompositeContour>:

```csharp
var parameterizedContour = new ParameterizedCompositeContour(closedBoundary);

PointXY halfwayAround = parameterizedContour.GetPoint(
    parameterizedContour.Length * 0.5f);

Console.WriteLine($"Середина обхода:   {halfwayAround}");
```

Координата начинается в `worldEdge.StartPoint`, возрастает по путям в порядке элементов массива
и принимает значения от `0` до общей `Length`. Поскольку контур замкнут, координаты `0` и
`Length` задают одно положение.

## Создание многоугольника по вершинам

Если все стороны должны быть прямыми, используйте конструктор из точек. Он соединяет соседние
вершины параметризованными отрезками и автоматически замыкает последнюю вершину на первую:

```csharp
var polygon = new CompositeContour(
    ToWorld(start),
    ToWorld(corner),
    ToWorld(end),
    ToWorld(upperLeft));
```

Не используйте этот сокращённый вариант для смешанной границы из учебника: он заменит
`worldSide` прямым отрезком и потеряет форму кривой Безье.

Теперь у вас есть проверенная замкнутая граница. Переходите к разделу
[«Создание региона»](creating-a-region.md).
