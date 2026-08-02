# Спроецировать точку на кривую

Используйте `Project`, чтобы найти ближайшую точку на любой `ICurve` и расстояние до неё.

## Проекция на любую кривую

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

ICurve curve = new Segment(
    new PointXY(0f, 0f),
    new PointXY(10f, 0f));

var sample = new PointXY(4f, 3f);
CurveProjection projection = curve.Project(sample);

PointXY closestPoint = projection.ProjectedPoint; // (4, 0)
float distance = projection.Distance;              // 3
```

Если нужно только расстояние, вызовите `curve.Distance(sample)`.

## Сохранение координаты кривой

Для `IParameterizedCurve` используйте `ProjectWithParameter`, чтобы дополнительно получить
координату проекции вдоль кривой:

```csharp
var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

ParameterizedCurveProjection pathProjection =
    path.ProjectWithParameter(sample);

float curveCoordinate = pathProjection.CurveCoordinate; // 4
PointXY samePoint = path.GetPoint(curveCoordinate);       // (4, 0)
```

Координаты кривой измеряются в мировых единицах, а не нормализуются в диапазон `[0, 1]`. Для
этого отрезка координата равна расстоянию от `StartPoint` и лежит в `[0, Length]`.

Ограниченные отрезки и дуги проецируют точку на ближайший конец, если неограниченная проекция
лежит за их пределами. Координаты исходной точки должны быть конечными.

Подробнее рассказано на странице [«Кривые»](../../concepts/geometry-model/curves.md). Далее можно
перейти к [поиску пересечений кривой](find-curve-intersections.md).
