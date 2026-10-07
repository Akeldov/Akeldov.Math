# Кривые и преобразования

На этом шаге учебника вы соедините точки прямолинейным и криволинейным путями, а затем разместите
полученную геометрию в мировом пространстве. Продолжайте работу в проекте
`Spatial2D.Fundamentals` с предыдущих шагов.

## Создание направленного отрезка

Замените содержимое `Program.cs`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var start = new PointXY(1f, 1f);
var corner = new PointXY(5f, 1f);

var lowerEdge = new ParameterizedSegment(start, corner);
PointXY lowerEdgeMidpoint = lowerEdge.GetPoint(lowerEdge.Length * 0.5f);

Console.WriteLine($"Длина:    {lowerEdge.Length}");
Console.WriteLine($"Середина: {lowerEdgeMidpoint}");
```

Запустите приложение:

```powershell
dotnet run
```

Результат:

```text
Длина:    4
Середина: (3, 1)
```

У <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedSegment> есть упорядоченные `StartPoint` и
`EndPoint`. Координата этой кривой представляет расстояние в мировых единицах: ноль соответствует
началу, а `Length` — концу. Направление станет существенным при объединении путей в контур.

Используйте <xref:Akeldov.Math.Spatial2D.Curves.Segment>, когда порядок концов не имеет значения и
нужен только геометрический отрезок.

## Добавление пути Безье

Добавьте после `lowerEdge` квадратичный путь Безье:

```csharp
var end = new PointXY(5f, 4f);
var control = new PointXY(7f, 2.5f);
var roundedSide = new QuadraticBezier(corner, control, end);

PointXY halfwayByParameter = roundedSide.GetPointAt(0.5f);
Console.WriteLine($"Середина кривой Безье: {halfwayByParameter}");
```

Результат:

```text
Середина кривой Безье: (6, 2.5)
```

Управляющая точка притягивает кривую к `(7, 2.5)`, но сама не является точкой пути.
`GetPointAt(t)` принимает нормализованный параметр Безье от `0` до `1`. Чтобы получить положение
на заданном расстоянии вдоль приближённой кривой, вызовите `GetPoint(curveCoordinate)` со значением
от `0` до `Length`.

И `ParameterizedSegment`, и <xref:Akeldov.Math.Spatial2D.Curves.QuadraticBezier> реализуют
<xref:Akeldov.Math.Spatial2D.Curves.IFinitePath>. Этот общий интерфейс предоставляет упорядоченные
концы, конечную длину и операции с координатой кривой, необходимые составным контурам.

## Перенос путей

Типы кривых неизменяемы. Сложение с вектором создаёт перенесённую копию, не изменяя исходного
значения:

```csharp
var translation = new VectorXY(2f, -1f);

ParameterizedSegment movedEdge = lowerEdge + translation;
QuadraticBezier movedSide = roundedSide + translation;

Console.WriteLine($"Перенесённый отрезок: {movedEdge.StartPoint} -> {movedEdge.EndPoint}");
Console.WriteLine($"Перенесённая кривая:  {movedSide.StartPoint} -> {movedSide.EndPoint}");
```

Пути остаются соединёнными, поскольку к их общему концу применяется одинаковый перенос:

```text
Перенесённый отрезок: (3, 0) -> (7, 0)
Перенесённая кривая:  (7, 0) -> (7, 3)
```

## Переход от локальной геометрии к мировой

Для масштабирования с поворотом преобразуйте все определяющие точки и создайте новые пути.
Добавьте следующую локальную функцию и преобразованные пути:

```csharp
PointXY ToWorld(PointXY point) => point.Transform(
    scaleFactor: 1.5f,
    angle: MathF.PI / 6f,
    offset: new VectorXY(10f, 2f));

var worldEdge = new ParameterizedSegment(
    ToWorld(lowerEdge.StartPoint),
    ToWorld(lowerEdge.EndPoint));

var worldSide = new QuadraticBezier(
    ToWorld(roundedSide.StartPoint),
    ToWorld(roundedSide.ControlPoint),
    ToWorld(roundedSide.EndPoint));
```

`Transform` выполняет операции в следующем порядке:

1. Равномерно масштабирует относительно начала координат.
2. Поворачивает против часовой стрелки вокруг начала координат на `PI / 6` радиан (30 градусов).
3. Добавляет вектор переноса.

Применение одной функции ко всем определяющим точкам сохраняет соединение между путями. Чтобы
повернуть геометрию вокруг заданной точки, используйте `point.Rotate(pivot, angle)`; угол этого
метода также выражается в радианах.

## Сохранение порядка обхода

Сохраните преобразованные пути через общий интерфейс в том порядке, в котором их следует
обходить:

```csharp
IFinitePath[] openBoundary =
{
    worldEdge,
    worldSide
};

foreach (IFinitePath path in openBoundary)
    Console.WriteLine($"{path.StartPoint} -> {path.EndPoint}");
```

Граница всё ещё открыта: `worldSide.EndPoint` не соединяется обратно с
`worldEdge.StartPoint`. На следующем шаге вы добавите недостающие пути и убедитесь, что цепочка
замыкается.

Переходите к разделу
[«Построение замкнутого контура»](building-a-closed-contour.md).
