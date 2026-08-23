# Контуры

Контуры — это замкнутые границы конечной длины. Spatial2D предоставляет круговые и
прямоугольные контуры, а также позволяет соединять конечные пути в составную границу. Контуры
поддерживают все общие запросы [кривых](curves.md) и добавляют проверку охвата и знаковое
расстояние.

Типы контуров находятся в пространстве имён <xref:Akeldov.Math.Spatial2D.Contours>.

## Кривые, контуры и области

Эти абстракции описывают связанную, но различную геометрию:

| Абстракция | Значение | Типичная операция |
|---|---|---|
| [Кривая](curves.md) | Одномерная геометрия, которая может быть открытой или замкнутой, конечной или бесконечной. | Спроецировать точку или пересечь луч. |
| Контур | Конечная замкнутая граница. | Проверить охват или измерить знаковое расстояние до границы. |
| [Область](regions.md) | Заполненная двумерная площадь, которая может ограничиваться несколькими контурами. | Проверить принадлежность площади с учётом отверстий. |

Используйте контур, когда основным объектом является сама граница. Если важны правила
заполнения, несколько границ или отверстия, используйте область. Метод `Encloses` контура при
этом удобен для простой проверки принадлежности одной замкнутой границе.

## Интерфейсы контуров

Интерфейсы контуров дополняют модель кривых возможностями замкнутой границы:

| Интерфейс | Возможности |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Contours.IContour> | Конечная замкнутая кривая со свойствами `Length`, методом `Encloses`, беззнаковым `Distance` и знаковым `SignedDistance`. |
| <xref:Akeldov.Math.Spatial2D.Contours.IParameterizedContour> | Добавляет координату кривой, основанную на длине, и параметризованную проекцию. |
| <xref:Akeldov.Math.Spatial2D.Curves.IContourPath> | Конечный направленный путь с запросом числа пересечений для правила заполнения, необходимым составному контуру. |
| <xref:Akeldov.Math.Spatial2D.Contours.ICompositeContour> | Предоставляет пути контура как структурное представление только для чтения. |
| <xref:Akeldov.Math.Spatial2D.Contours.IParameterizedCompositeContour> | Объединяет составную границу с одной непрерывной координатой обхода. |

Все контуры реализуют <xref:Akeldov.Math.Spatial2D.Curves.ICurve> для измерения расстояния до
точки и проекции. Отдельно они реализуют
<xref:Akeldov.Math.Spatial2D.Curves.IRightwardCrossingProvider> для запросов правил заполнения.
`IContour` не предоставляет полиморфные пересечения с лучом; поддерживаемые несоставные типы
контуров выполняют эти операции через методы расширения. Составные типы их не предоставляют.

## Выбор конкретного контура

Тип определяется формой границы и необходимостью координаты обхода:

| Форма | Без параметризации | С параметризацией | Когда использовать |
|---|---|---|---|
| Окружность | <xref:Akeldov.Math.Spatial2D.Contours.Circle> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCircle> | Граница представляет полную окружность. |
| Прямоугольник по осям | <xref:Akeldov.Math.Spatial2D.Contours.RectangleContour> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedRectangleContour> | Стороны прямоугольника параллельны мировым осям. |
| Повёрнутый прямоугольник | <xref:Akeldov.Math.Spatial2D.Contours.OrientedRectangleContour> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedOrientedRectangleContour> | Прямоугольник повёрнут в мировом пространстве. |
| Пути или вершины многоугольника | <xref:Akeldov.Math.Spatial2D.Contours.CompositeContour> | <xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCompositeContour> | Граница собирается из отрезков, дуг, кривых Безье или других конечных путей. |

Типы без параметризации описывают только геометрию границы. Параметризованные типы также
задают положение координаты ноль и направление её увеличения.

## Создание стандартных контуров

Круговые и прямоугольные контуры напрямую предоставляют размеры своей границы:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 3f);

var rectangle = new RectangleContour(
    cornerA: new PointXY(-2f, -1f),
    cornerB: new PointXY(2f, 1f));

float circumference = circle.Length;
float perimeter = rectangle.Length; // 12
```

`RectangleContour` нормализует две противоположные вершины в `Min` и `Max`, поэтому порядок
аргументов не важен. `OrientedRectangleContour` вместо этого принимает центр, размер и угол
поворота в радианах. Направление поворота описано на странице
[«Углы и единицы измерения»](../fundamentals/angles-and-units.md).

## Построение составного контура

Для многоугольника передайте как минимум три вершины в порядке обхода границы. Конструктор
соединит соседние вершины параметризованными отрезками и замкнёт последнюю вершину на первую:

```csharp
var triangle = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f),
    new PointXY(2f, 3f));
```

Для смешанной границы передайте `IReadOnlyList<IContourPath>`. Каждый путь должен заканчиваться
там, где начинается следующий, а последний путь должен соединяться с первым:

```csharp
using System;
using Akeldov.Math.Spatial2D.Curves;

var curvedBoundary = new CompositeContour(new IContourPath[]
{
    new ParameterizedSegment(
        new PointXY(-2f, 0f),
        new PointXY(2f, 0f)),
    new ParameterizedArc(
        center: new PointXY(0f, 0f),
        radius: 2f,
        startAngle: 0f,
        endAngle: MathF.PI,
        angularDirection: AngularDirection.Counterclockwise)
});
```

Составной контур копирует ссылки на переданные пути в закрытое хранилище. Свойство `Curves`
предоставляет структурное представление этой копии только для чтения, поэтому после создания
нельзя изменить порядок или количество частей контура. Пути должны иметь конечную
неотрицательную длину, а их суммарная длина должна оставаться конечной.

## Параметризация замкнутой границы

`IParameterizedContour` сопоставляет координаты из диапазона `[0, Length]` с положениями на
границе. Координаты `0` и `Length` описывают одну геометрическую точку, поскольку контур замкнут:

```csharp
using Akeldov.Math.Spatial2D.Curves;

var directedCircle = new ParameterizedCircle(
    center: new PointXY(0f, 0f),
    radius: 2f,
    startAngle: MathF.PI / 2f,
    contourDirection: ContourDirection.Counterclockwise);

PointXY start = directedCircle.GetPoint(0f); // Приблизительно (0, 2)
PointXY halfway = directedCircle.GetPoint(directedCircle.Length * 0.5f);
```

Для параметризованных прямоугольников можно выбрать именованное начало границы или координату
на периметре. `ParameterizedCompositeContour` начинается в `StartPoint` первого пути и проходит
пути в порядке списка, предоставляя всей цепочке одну непрерывную координату длины.

## Охват и расстояние до границы

`Encloses` возвращает `true` для точки внутри или на замкнутой границе. `Distance` всегда
неотрицателен, а `SignedDistance` по принятому соглашению возвращает отрицательное значение
внутри:

```csharp
var sample = new PointXY(1f, 0f);

bool enclosed = circle.Encloses(sample);              // true
float distance = circle.Distance(sample);             // 2
float signedDistance = circle.SignedDistance(sample); // -2
CurveProjection projection = circle.Project(sample);
```

У `SignedDistance` нет параметра допуска. Знак определяется непосредственно вычислениями границы
и охвата контура.

Поддерживаемые несоставные типы контуров предоставляют `GetPointIntersections` через методы
расширения для пересечений; ни `IContour`, ни `IContourPath` эту операцию не объявляют. У
`CompositeContour` и `ParameterizedCompositeContour` нет перегрузок пересечения с лучом, потому
что их разнородные пути не имеют общего контракта бинарного пересечения.

## Сглаживание углов многоугольника

`FilletCorners(radius)` возвращает новый `CompositeContour`: соседние параметризованные отрезки
обрезаются, а между ними вставляются касательные дуги. Исходный контур не изменяется.

```csharp
var square = new CompositeContour(
    new PointXY(0f, 0f),
    new PointXY(4f, 0f),
    new PointXY(4f, 4f),
    new PointXY(0f, 4f));

CompositeContour rounded = square.FilletCorners(radius: 0.5f);
```

Радиус измеряется в мировых единицах координат. Углы, в которых участвуют пути, отличные от
`ParameterizedSegment`, сохраняются без изменений.

## Преобразование границы в область

Круговые и прямоугольные типы контуров предоставляют соответствующее значение заполненной
области через `ToRegion`. Для произвольной составной границы или нескольких контуров,
интерпретируемых по правилу заполнения, используйте
<xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion>.

Практические примеры:

- [Построить замкнутый контур](../../how-to-guides/curves-and-contours/build-a-closed-contour.md)
- [Создать область с отверстиями](../../how-to-guides/regions/create-a-region-with-holes.md)
- [Концепции растеризации](../rasterization.md)
