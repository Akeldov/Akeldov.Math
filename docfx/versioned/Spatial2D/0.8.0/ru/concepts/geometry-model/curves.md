# Кривые

Кривые описывают одномерную геометрию в двумерном пространстве. Spatial2D предоставляет
бесконечные прямые, лучи, конечные отрезки, дуги и кривые Безье. Из кривых строятся границы
[контуров](contours.md); кроме того, кривые позволяют вычислять расстояния и проекции и могут
служить источниками для полей и растеризаторов.

Большинство типов кривых находится в пространстве имён
<xref:Akeldov.Math.Spatial2D.Curves>. Полные окружности и другие замкнутые границы относятся к
контурам, хотя и поддерживают общие операции над кривыми.

## Модель кривых

Интерфейсы кривых последовательно добавляют возможности, не навязывая единственное
представление геометрии:

| Интерфейс | Возможности |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Curves.ICurve> | Вычисляет расстояние до точки и её проекцию, считает пересечения с направленным вправо лучом и находит пересечения с произвольным лучом. |
| <xref:Akeldov.Math.Spatial2D.Curves.IFiniteCurve> | Предоставляет конечную длину `Length` в мировых единицах координат. |
| <xref:Akeldov.Math.Spatial2D.Curves.IOneEndpointCurve> | Имеет одну конечную точку, как луч. |
| <xref:Akeldov.Math.Spatial2D.Curves.ITwoEndpointCurve> | Имеет две конечные точки без заданного порядка обхода. |
| <xref:Akeldov.Math.Spatial2D.Curves.IParameterizedCurve> | Сопоставляет координату кривой с точкой и возвращает эту координату при проецировании. |
| <xref:Akeldov.Math.Spatial2D.Curves.IPath> | Добавляет к параметризованной кривой упорядоченные свойства `StartPoint` и `EndPoint`. |
| <xref:Akeldov.Math.Spatial2D.Curves.IFinitePath> | Объединяет конечную длину, упорядоченные концы и параметризацию; такие кривые используются при построении контуров. |

Выбирайте наиболее узкий интерфейс, достаточный для операции. Например, алгоритм вычисления
близости может принимать `ICurve`, а алгоритму обхода от одного конца к другому нужен
`IFinitePath`.

## Выбор конкретной кривой

Тип определяется протяжённостью кривой и тем, имеет ли значение направление обхода:

| Семейство | Тип | Вид | Протяжённость и область координаты | Когда использовать |
|---|---|---|---|---|
| Линейные | <xref:Akeldov.Math.Spatial2D.Curves.Line> | <img src="/Akeldov.Math/assets/spatial2d/curves/line.png" width="64" height="64" alt="Рендеринг расстояния до Line"> | Бесконечная; без параметризации | Важна только геометрия прямой. |
| Линейные | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedLine> | <img src="/Akeldov.Math/assets/spatial2d/curves/parameterized-line.png" width="64" height="64" alt="Рендеринг расстояния до ParameterizedLine с растущей толщиной"> | Бесконечная; `(-бесконечность, +бесконечность)` | Нужны начало отсчёта, направление и знаковая координата вдоль прямой. |
| Линейные | <xref:Akeldov.Math.Spatial2D.Curves.Ray> | <img src="/Akeldov.Math/assets/spatial2d/curves/ray.png" width="64" height="64" alt="Рендеринг расстояния до Ray с растущей толщиной"> | Полубесконечная; `[0, +бесконечность)` | Геометрию позади начала луча нужно исключить. |
| Линейные | <xref:Akeldov.Math.Spatial2D.Curves.Segment> | <img src="/Akeldov.Math/assets/spatial2d/curves/segment.png" width="64" height="64" alt="Рендеринг расстояния до Segment"> | Конечная; без параметризации | Порядок концов не должен влиять на геометрическую идентичность. |
| Линейные | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedSegment> | <img src="/Akeldov.Math/assets/spatial2d/curves/parameterized-segment.png" width="64" height="64" alt="Рендеринг расстояния до ParameterizedSegment с растущей толщиной"> | Конечная; `[0, Length]` | Важно направление или расстояние от начальной точки. |
| Линейные | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedSegmentChain> | <img src="/Akeldov.Math/assets/spatial2d/curves/parameterized-segment-chain.png" width="64" height="64" alt="Рендеринг расстояния до ParameterizedSegmentChain с растущей толщиной"> | Конечная открытая ломаная; `[0, Length]` | Последовательность отрезков должна вести себя как единый путь. |
| Круговые | <xref:Akeldov.Math.Spatial2D.Curves.Arc> | <img src="/Akeldov.Math/assets/spatial2d/curves/arc.png" width="64" height="64" alt="Рендеринг расстояния до Arc"> | Конечный угловой диапазон; без параметризации | Важна только геометрия дуги. |
| Круговые | <xref:Akeldov.Math.Spatial2D.Curves.ParameterizedArc> | <img src="/Akeldov.Math/assets/spatial2d/curves/parameterized-arc.png" width="64" height="64" alt="Рендеринг расстояния до ParameterizedArc с растущей толщиной"> | Направленный угловой диапазон; `[0, Length]` | Важен обход по часовой стрелке или против неё. |
| Безье | <xref:Akeldov.Math.Spatial2D.Curves.QuadraticBezier> | <img src="/Akeldov.Math/assets/spatial2d/curves/quadratic-bezier.png" width="64" height="64" alt="Рендеринг расстояния до QuadraticBezier с растущей толщиной"> | Конечный путь; `[0, Length]` | Достаточно одной управляющей точки. |
| Безье | <xref:Akeldov.Math.Spatial2D.Curves.CubicBezier> | <img src="/Akeldov.Math/assets/spatial2d/curves/cubic-bezier.png" width="64" height="64" alt="Рендеринг расстояния до CubicBezier с растущей толщиной"> | Конечный путь; `[0, Length]` | Нужны отдельные исходящая и входящая управляющие точки. |
| Безье | <xref:Akeldov.Math.Spatial2D.Curves.BezierCurve> | <img src="/Akeldov.Math/assets/spatial2d/curves/cubic-bezier.png" width="64" height="64" alt="Рендеринг расстояния до кубической BezierCurve с растущей толщиной"> | Конечный путь; `[0, Length]` | Степень и количество управляющих точек определяются данными. |

Каждый эскиз растеризован по полю расстояния до кривой. У параметризованных кривых толщина
линии растёт вместе с координатой кривой и тем самым показывает направление обхода.

<xref:Akeldov.Math.Spatial2D.Contours.Circle> представляет полную окружность. Если полной
окружности также нужны начало и направление обхода, используйте
<xref:Akeldov.Math.Spatial2D.Contours.ParameterizedCircle>.

Углы прямых, лучей и дуг задаются в радианах. Соглашения о координатах и направлении поворота
описаны на странице [«Углы и единицы измерения»](../fundamentals/angles-and-units.md).

## Координаты кривой

Координаты кривой — это расстояния в мировых единицах координат, а не нормализованные значения
из диапазона `[0, 1]`. Конечный путь начинается с координаты `0` и заканчивается координатой
`Length`:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

PointXY halfway = path.GetPoint(path.Length * 0.5f); // (5, 0)
```

Параметризованные бесконечные прямые используют знаковые координаты, а лучи принимают только
неотрицательные координаты. Направление пути определяет, какой из его концов имеет координату
ноль.

Кривые Безье также предоставляют метод `GetPointAt(t)`, где `t` — нормализованный параметр
Безье в диапазоне `[0, 1]`. Обычно он не пропорционален длине дуги. Используйте `GetPoint`, если
координата должна представлять расстояние вдоль аппроксимированного пути.

## Проекция точки и расстояние

Любая `ICurve` умеет находить ближайшую к образцу точку. `Project` возвращает проекцию и
расстояние, а `ProjectWithParameter` — ещё и координату проекции на кривой:

```csharp
var sample = new PointXY(4f, 3f);
ParameterizedCurveProjection projection = path.ProjectWithParameter(sample);

PointXY closest = projection.ProjectedPoint;   // (4, 0)
float coordinate = projection.CurveCoordinate; // 4
float distance = projection.Distance;           // 3
```

Если ближайшая позиция и координата кривой не нужны, достаточно вызвать `Distance(point)`.
Линейные и круговые типы вычисляют эти операции аналитически. Для длины, проекции, расстояния и
пересечений с лучом кривые Безье используют внутреннюю аппроксимацию ломаной.

## Пересечение кривой с лучом

`GetRayIntersections` возвращает точки, расположенные в прямом направлении переданного луча:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var ray = new Ray(
    origin: new PointXY(-10f, 0f),
    angle: 0f);

List<PointXY> intersections = circle.GetRayIntersections(ray);
```

Метод возвращает новый изменяемый список, принадлежащий вызывающему коду. Необязательный
аргумент `geometryEpsilon` измеряется в мировых единицах координат и управляет сравнениями около
касательных, конечных точек, коллинеарных наложений и почти параллельных кривых.

`CountRightwardCrossings` — специализированный запрос числа пересечений, используемый
алгоритмами проверки принадлежности. Для него действует полуоткрытое правило учёта концов,
благодаря которому общая вершина не считается дважды.

## Построение составной геометрии

`IFinitePath` содержит упорядоченные концы, необходимые для соединения кривых в контур. Конец
каждого пути должен совпадать с началом следующего, а в замкнутом контуре последний путь должен
возвращаться к началу первого. Полученный контур может задавать внешнюю границу или отверстие
[области](regions.md).

Практические примеры:

- [Проецирование точки на кривую](../../how-to-guides/curves-and-contours/project-a-point-onto-a-curve.md)
- [Поиск пересечений кривой](../../how-to-guides/curves-and-contours/find-curve-intersections.md)
- [Построение замкнутого контура](../../how-to-guides/curves-and-contours/build-a-closed-contour.md)
- [Учебник по кривым и преобразованиям](../../tutorials/2d-geometry-fundamentals/curves-and-transformations.md)
