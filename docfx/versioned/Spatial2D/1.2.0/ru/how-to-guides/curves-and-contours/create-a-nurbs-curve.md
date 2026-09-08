# Создать NURBS-кривую

Используйте <xref:Akeldov.Math.Spatial2D.Curves.Nurbs>, когда рациональные веса должны управлять
силой влияния каждой управляющей точки на сплайн. Полученная конечная направленная кривая
реализует <xref:Akeldov.Math.Spatial2D.Curves.IContourPath>, поэтому поддерживает обход, проекцию,
измерение расстояния и использование в составном контуре.

## Задать взвешенный сплайн

Передайте по одному конечному строго положительному весу для каждой управляющей точки. Количество
узлов должно быть равно количеству управляющих точек плюс степень плюс один; для вектора узлов
действуют те же правила, что и у <xref:Akeldov.Math.Spatial2D.Curves.BSpline>.

Эта рациональная квадратичная кривая точно представляет четверть окружности при вычислении через
`GetPointAt` или `GetPointAtKnot`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

float diagonalWeight = MathF.Sqrt(0.5f);

PointXY[] controlPoints =
{
    new PointXY(1f, 0f),
    new PointXY(1f, 1f),
    new PointXY(0f, 1f)
};

float[] weights = { 1f, diagonalWeight, 1f };
float[] knots = { 0f, 0f, 0f, 1f, 1f, 1f };

var quarterCircle = new Nurbs(
    degree: 2,
    controlPoints: controlPoints,
    weights: weights,
    knots: knots);

PointXY pointOnArc = quarterCircle.GetPointAt(0.5f);
```

При равных весах форма совпадает с B-сплайном, построенным по той же степени, управляющим точкам и
узлам. Увеличьте вес, чтобы притянуть кривую к соответствующей управляющей точке.

## Выбрать систему координат

Используйте `GetPointAt(t)` с нормализованным параметром в `[0, 1]` или
`GetPointAtKnot(knot)` со значением в `[KnotStart, KnotEnd]`, чтобы вычислить рациональный сплайн
напрямую. Используйте `GetPoint(curveCoordinate)` с координатой в `[0, Length]`, когда движение
или размещение должно основываться на приближённом расстоянии от `StartPoint`.

Нормализованный параметр и параметр узла обычно не пропорциональны длине дуги. В
[руководстве по B-сплайну](create-a-b-spline.md#выбрать-систему-координат) три системы координат
сопоставлены в таблице.

## Настроить качество аппроксимации

Прямое вычисление через `GetPointAt` и `GetPointAtKnot` использует алгоритм де Бура и сохраняет
точную рациональную форму. `Length`, `GetPoint`, `Distance`, `Project`, `ProjectWithParameter`,
`CountRightwardCrossings` и `Flatten` используют кэшированную аппроксимацию ломаной.
`GetPointIntersections` решает уравнения исходных рациональных участков, поэтому эта
аппроксимация и `segmentsPerKnotSpan` не влияют на результат.

Задайте `segmentsPerKnotSpan`, если приближённым операциям требуется больше подразделений.
Значение по умолчанию — `64`; увеличивайте его для резких изгибов, сильно различающихся интервалов
или экстремальных весов:

```csharp
var detailedQuarterCircle = new Nurbs(
    degree: 2,
    controlPoints: controlPoints,
    weights: weights,
    knots: knots,
    segmentsPerKnotSpan: 128);

ParameterizedCurveProjection projection =
    detailedQuarterCircle.ProjectWithParameter(new PointXY(0.7f, 0.7f));

float distance = projection.Distance;
float distanceFromStart = projection.CurveCoordinate;
```

Количество подразделений управляет разрешением, но не задаёт геометрическую границу ошибки.
`Flatten` возвращает новый изменяемый список направленных отрезков, принадлежащий вызывающему
коду. `ControlPoints`, `Weights` и `Knots` являются представлениями только для чтения над
состоянием, скопированным при создании, поэтому последующие изменения входных массивов не меняют
кривую.

Правила кратности узлов приведены в руководстве [«Создать B-сплайн»](create-a-b-spline.md), общие
контракты — на странице [«Кривые»](../../concepts/geometry-model/curves.md), а
[построение замкнутого контура](build-a-closed-contour.md) показывает использование NURBS-кривой
как части границы.
