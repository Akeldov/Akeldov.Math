# Найти пересечения кривых

Используйте `ICurve.GetPointIntersections`, чтобы найти изолированные точки пересечения кривой
или контура с направленным лучом. Метод возвращает только пересечения в начале луча или дальше
по его направлению.

## Провести луч через кривую

В следующем примере луч начинается слева от окружности и направлен вдоль положительной оси X,
поэтому он дважды пересекает границу:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Contours;
using Akeldov.Math.Spatial2D.Curves;

ICurve boundary = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var ray = new Ray(
    origin: new PointXY(-10f, 0f),
    angle: 0f);

List<PointXY> intersections = boundary.GetPointIntersections(ray);

// intersections содержит (-5, 0) и (5, 0).
```

Угол в конструкторе `Ray` задаётся в радианах. Запись `new Ray(origin)` — это сокращённый способ
создать луч, направленный вдоль положительной оси X.

Если луч проходит мимо кривой или кривая целиком находится позади него, результат будет пустым.
Касание обычно даёт одну точку. Если луч начинается внутри замкнутого контура, метод возвращает
только точку выхода перед ним. Не полагайтесь на порядок элементов, если конкретный тип кривой
явно не гарантирует его.

Метод возвращает новый изменяемый `List<PointXY>`, принадлежащий вызывающему коду. Список можно
сортировать, фильтровать и переиспользовать, не изменяя кривую.

## Использовать общий интерфейс кривых

Один и тот же вызов подходит для прямых, лучей, отрезков, дуг, кривых Безье, окружностей и
составных контуров, поскольку все они реализуют `ICurve`:

```csharp
static List<PointXY> FindIntersections(ICurve curve, Ray ray)
{
    return curve.GetPointIntersections(ray);
}
```

Линейные и круговые кривые (`Line`, `Ray`, `Segment`, `ParameterizedLine`,
`ParameterizedSegment`, `ParameterizedSegmentChain`, `Arc` и `ParameterizedArc`), а также
`QuadraticBezier` и `CubicBezier` предоставляют перегрузки `GetPointIntersections` для `Line`,
`ParameterizedLine`, `Segment`, `ParameterizedSegment` и `ParameterizedSegmentChain`.
Линейные и круговые кривые дополнительно предоставляют точные перегрузки для `Arc` и `ParameterizedArc`:

```csharp
var segment = new Segment(new PointXY(-2f, 1f), new PointXY(2f, 1f));
var probeLine = new Line(new PointXY(0f, -2f), new PointXY(0f, 2f));

List<PointXY> lineIntersections = segment.GetPointIntersections(probeLine);
```

Несколько пересечений упорядочиваются по каноническому направлению `Line` или по
параметризованному направлению `ParameterizedLine`. Для `Segment` они идут от `EndpointA` к
`EndpointB`, а для `ParameterizedSegment` — от `StartPoint` к `EndPoint`. Для
`ParameterizedSegmentChain` уникальные пересечения упорядочиваются от `StartPoint` цепочки к её
`EndPoint`. Для `Arc` пересечения упорядочиваются против часовой стрелки от `StartAngle`, а для
`ParameterizedArc` — от `StartPoint` к `EndPoint` с учётом `AngularDirection`. Оба типа отрезков
ограничивают результаты с учётом включения концов.

В Spatial2D нет единого метода для пересечения двух произвольных кривых.

Пересечения кривых Безье с лучом вычисляются по внутренней полилинейной аппроксимации.
Пересечения `QuadraticBezier` и `CubicBezier` с прямой вместо этого находятся решением полинома
исходной кривой. Общий `BezierCurve` не предоставляет эти перегрузки.

`GetPointIntersections(Ray)` использует стандартный геометрический допуск библиотеки. Пересечения
с `Line`, `ParameterizedLine`, `Segment`, `ParameterizedSegment` или
`ParameterizedSegmentChain`, `Arc` или `ParameterizedArc` используют точные сравнения. Для кривых
Безье круговые перегрузки отсутствуют, потому что пересечение общей кубической кривой с окружностью
не имеет точного алгебраического решения.

## Учесть наложения и концы

`GetPointIntersections` возвращает только изолированные точки. Наложение может содержать
бесконечно много точек, поэтому принадлежащие этому непрерывному множеству точки пропускаются.
Изолированное пересечение во включённом конце отрезка возвращается, а в исключённом — нет.

Составные кривые и контуры объединяют результаты своих частей и удаляют общие точки в пределах
стандартного геометрического допуска библиотеки. Благодаря этому луч, проходящий через вершину
контура, обычно не возвращает одно и то же положение отдельно для каждого соседнего участка.

Описание модели геометрии приведено на странице [«Кривые»](../../concepts/geometry-model/curves.md).
Далее можно перейти к [построению замкнутого контура](build-a-closed-contour.md).
