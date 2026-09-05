# Кривые

Кривые описывают одномерную геометрию в двумерном пространстве. Spatial2D предоставляет
бесконечные прямые, лучи, конечные отрезки, дуги, кривые Безье, B-сплайны и NURBS. Из кривых строятся границы
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
| <xref:Akeldov.Math.Spatial2D.Curves.ICurve> | Вычисляет расстояние до точки и её проекцию. |
| <xref:Akeldov.Math.Spatial2D.Curves.IFiniteCurve> | Предоставляет конечную длину `Length` в мировых единицах координат. |
| <xref:Akeldov.Math.Spatial2D.Curves.IOneEndpointCurve> | Имеет одну конечную точку, как луч. |
| <xref:Akeldov.Math.Spatial2D.Curves.ITwoEndpointCurve> | Имеет две конечные точки без заданного порядка обхода. |
| <xref:Akeldov.Math.Spatial2D.Curves.IParameterizedCurve> | Сопоставляет координату кривой с точкой и возвращает эту координату при проецировании. |
| <xref:Akeldov.Math.Spatial2D.Curves.IPath> | Добавляет к параметризованной кривой упорядоченные свойства `StartPoint` и `EndPoint`. |
| <xref:Akeldov.Math.Spatial2D.Curves.IFinitePath> | Объединяет конечную длину, упорядоченные концы и параметризацию. |
| <xref:Akeldov.Math.Spatial2D.Curves.IRightwardCrossingProvider> | Считает пересечения с горизонтальным направленным вправо лучом для правил заливки. |
| <xref:Akeldov.Math.Spatial2D.Curves.IContourPath> | Объединяет `IFinitePath` с подсчётом пересечений для построения и проверки охвата контура. |

Выбирайте наиболее узкий интерфейс, достаточный для операции. Например, алгоритм вычисления
близости может принимать `ICurve`, алгоритм обхода от одного конца к другому — `IFinitePath`, а
построитель составного контура — `IContourPath`.

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
| Сплайн | <xref:Akeldov.Math.Spatial2D.Curves.BSpline> | <img src="/Akeldov.Math/assets/spatial2d/curves/b-spline.png" width="64" height="64" alt="Рендеринг расстояния до BSpline с растущей толщиной"> | Конечный путь; `[0, Length]` | Кривую определяют степень, управляющие точки и неравномерный вектор узлов. |
| Сплайн | <xref:Akeldov.Math.Spatial2D.Curves.Nurbs> | <img src="/Akeldov.Math/assets/spatial2d/curves/nurbs.png" width="64" height="64" alt="Рендеринг расстояния до Nurbs с растущей толщиной"> | Конечный путь; `[0, Length]` | В дополнение к данным B-сплайна нужны рациональные веса. |

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

Кривые Безье и сплайны также предоставляют метод `GetPointAt(t)`, где `t` — нормализованный
параметр в диапазоне `[0, 1]`. Сплайны дополнительно предоставляют `GetPointAtKnot(knot)` в
исходных единицах вектора узлов. Эти параметры обычно не пропорциональны длине дуги. Используйте
`GetPoint`, если координата должна представлять расстояние вдоль аппроксимированного пути. Примеры
работы с вектором узлов, весами и аппроксимацией приведены в руководствах
[«Создать B-сплайн»](../../how-to-guides/curves-and-contours/create-a-b-spline.md) и
[«Создать NURBS-кривую»](../../how-to-guides/curves-and-contours/create-a-nurbs-curve.md).

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
Линейные и круговые типы вычисляют эти операции аналитически. Кривые Безье и сплайны используют
внутреннюю аппроксимацию ломаной для длины, проекции и расстояния. Для пересечений кривые Безье
решают полином исходной кривой; сплайны не предоставляют перегрузки бинарных пересечений.

## Пересечение кривой с лучом

Методы расширения `GetPointIntersections` конкретных типов возвращают изолированные точки
пересечения, расположенные в прямом направлении переданного луча. Интерфейсы кривых не объявляют
эту бинарную операцию. Поддерживаемые несоставные типы контуров предоставляют её через методы
расширения; у `CompositeContour` и `ParameterizedCompositeContour` нет перегрузок пересечения с
лучом:

```csharp
using System.Collections.Generic;
using Akeldov.Math.Spatial2D.Contours;

var circle = new Circle(
    center: new PointXY(0f, 0f),
    radius: 5f);

var ray = new Ray(
    origin: new PointXY(-10f, 0f),
    angle: 0f);

List<PointXY> intersections = circle.GetPointIntersections(ray);
```

Метод возвращает новый изменяемый список, принадлежащий вызывающему коду. Точки, принадлежащие
непрерывному множеству пересечений, не возвращаются. Например, коллинеарное наложение линейной
кривой и луча не даёт характерной точки.

`IRightwardCrossingProvider.CountRightwardCrossings` — специализированный запрос числа
пересечений, используемый алгоритмами проверки принадлежности. Для него действует полуоткрытое
правило учёта концов, благодаря которому общая вершина не считается дважды.

## Построение составной геометрии

`IContourPath` содержит упорядоченные концы и запрос числа пересечений для правила заполнения,
необходимые для соединения кривых в контур. Конец каждого пути должен совпадать с началом
следующего, а в замкнутом контуре последний путь должен возвращаться к началу первого. Полученный
контур может задавать внешнюю границу или отверстие [области](regions.md).

Практические примеры:

- [Создать B-сплайн](../../how-to-guides/curves-and-contours/create-a-b-spline.md)
- [Создать NURBS-кривую](../../how-to-guides/curves-and-contours/create-a-nurbs-curve.md)
- [Проецирование точки на кривую](../../how-to-guides/curves-and-contours/project-a-point-onto-a-curve.md)
- [Поиск пересечений кривой](../../how-to-guides/curves-and-contours/find-curve-intersections.md)
- [Построение замкнутого контура](../../how-to-guides/curves-and-contours/build-a-closed-contour.md)
- [Учебник по кривым и преобразованиям](../../tutorials/2d-geometry-fundamentals/curves-and-transformations.md)
