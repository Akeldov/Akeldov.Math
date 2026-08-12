# Проверка точек и пересечений

В заключительной части учебника вы выполните запросы к границе и заполненной области из
предыдущих шагов, спроецируете точку на контур и проведёте через фигуру луч. Продолжайте работу в
проекте `Spatial2D.Fundamentals`, где в `Program.cs` уже определены `contour`, `region`,
`insidePoint`, `outsidePoint` и функция `ToWorld`.

## Выберите запрос контура или региона

Контур представляет одну замкнутую границу, а регион применяет правило заполнения к одной или
нескольким границам. Сравните их запросы для фигуры учебника с одним контуром:

```csharp
bool contourEnclosesInside = contour.Encloses(insidePoint);
bool contourEnclosesOutside = contour.Encloses(outsidePoint);

bool regionContainsInside = region.Contains(insidePoint);
bool regionContainsOutside = region.Contains(outsidePoint);

Console.WriteLine($"Контур охватывает внутреннюю точку: {contourEnclosesInside}");
Console.WriteLine($"Контур охватывает внешнюю точку:   {contourEnclosesOutside}");
Console.WriteLine($"Регион содержит внутреннюю точку: {regionContainsInside}");
Console.WriteLine($"Регион содержит внешнюю точку:    {regionContainsOutside}");
```

Результат:

```text
Контур охватывает внутреннюю точку: True
Контур охватывает внешнюю точку:   False
Регион содержит внутреннюю точку: True
Регион содержит внешнюю точку:    False
```

Для одной границы <xref:Akeldov.Math.Spatial2D.Contours.IContour.Encloses(Akeldov.Math.Spatial2D.PointXY)>
и <xref:Akeldov.Math.Spatial2D.Regions.IRegion.Contains(Akeldov.Math.Spatial2D.PointXY)>
одинаково классифицируют эти точки, включая точку на границе. Используйте `Contains`, когда в
модели важны правила заполнения, несколько контуров или отверстия. Используйте `Encloses`, когда
основным объектом является одна граница.

## Спроецируйте внешнюю точку на границу

Используйте `Project`, когда нужны и ближайшее положение на границе, и расстояние до него:

```csharp
CurveProjection projection = contour.Project(outsidePoint);

Console.WriteLine($"Точка проекции:       {projection.ProjectedPoint}");
Console.WriteLine($"Расстояние проекции:  {projection.Distance}");
Console.WriteLine($"На границе:           {contour.Distance(projection.ProjectedPoint) == 0f}");
```

`projection.ProjectedPoint` лежит на левой стороне фигуры, а `projection.Distance` приблизительно
равно `1.5` мировой единицы. Оно совпадает с `contour.Distance(outsidePoint)` и
`region.Distance(outsidePoint)`, поскольку оба объекта используют одну границу.

<xref:Akeldov.Math.Spatial2D.Curves.CurveProjection> хранит точку проекции вместе с
неотрицательным расстоянием. Если само ближайшее положение не требуется, вызывайте `Distance`
напрямую.

## Проведите луч через контур

Если пространство имён обобщённых коллекций ещё не подключено, добавьте его в начало
`Program.cs`:

```csharp
using System.Collections.Generic;
```

Создайте луч в `outsidePoint`. Его угол совпадает с поворотом `PI / 6` в функции `ToWorld`,
поэтому в локальных координатах фигуры луч движется горизонтально слева направо:

```csharp
var probeRay = new Ray(
    origin: outsidePoint,
    angle: MathF.PI / 6f);

List<PointXY> intersections = contour.GetRayIntersections(probeRay);

Console.WriteLine($"Пересечения границы: {intersections.Count}");
```

Луч входит через прямую левую сторону и выходит через криволинейную правую:

```text
Пересечения границы: 2
```

Угол конструктора <xref:Akeldov.Math.Spatial2D.Curves.Ray> измеряется в радианах. Запрос
возвращает только пересечения в начале луча или впереди него; точки позади начала не включаются.

## Выберите первое пересечение

Не полагайтесь на порядок, возвращаемый произвольной кривой. Перед выбором точки входа
отсортируйте принадлежащий вызывающему коду изменяемый список по расстоянию от начала луча:

```csharp
intersections.Sort((left, right) =>
    outsidePoint.Distance(left).CompareTo(outsidePoint.Distance(right)));

PointXY entryPoint = intersections[0];
float distanceToEntry = outsidePoint.Distance(entryPoint);

Console.WriteLine($"Расстояние до входа: {distanceToEntry}");
Console.WriteLine($"Точка входа содержится в регионе: {region.Contains(entryPoint)}");
```

Точка входа лежит на границе, поэтому `region.Contains(entryPoint)` возвращает `true`.
Сортировка и фильтрация `intersections` не изменяют `contour`: `GetRayIntersections` возвращает
новый изменяемый список, принадлежащий вызывающему коду.

## Обработайте промахи и численный допуск

Если луч не пересекает контур, метод возвращает пустой список. Касание обычно даёт одну точку, а
луч изнутри фигуры — только выход впереди него. Всегда проверяйте `Count` перед обращением к
элементам результата.

Необязательный аргумент `geometryEpsilon` управляет сравнениями около концов и точек касания, а
также при коллинеарных наложениях и почти параллельных кривых. Это конечное неотрицательное
расстояние в мировых единицах. Начните со стандартного значения и увеличивайте его только с
учётом масштаба и ожидаемого шума входных данных. Составной контур удаляет дубликаты пересечений
в общих концах путей в пределах этого допуска.

Пересечения кривых Безье, как их длина и проекция, используют внутреннюю полилинейную
аппроксимацию. Учитывайте её, когда запрос проходит совсем близко к криволинейной стороне.

## Завершение учебника

Теперь вы умеете:

1. Создавать точки, векторы, прямолинейные пути и путь Безье.
2. Преобразовывать локальную геометрию в мировое пространство.
3. Строить и проверять замкнутый контур.
4. Превращать контур в заполненный регион.
5. Классифицировать и проецировать точки, измерять расстояния и находить пересечения.

Следующие практические примеры приведены в руководствах
[«Спроецировать точку на кривую»](../../how-to-guides/curves-and-contours/project-a-point-onto-a-curve.md)
и [«Найти пересечения кривых»](../../how-to-guides/curves-and-contours/find-curve-intersections.md).
Подробные контракты описаны на концептуальных страницах
[«Контуры»](../../concepts/geometry-model/contours.md) и
[«Регионы»](../../concepts/geometry-model/regions.md).
