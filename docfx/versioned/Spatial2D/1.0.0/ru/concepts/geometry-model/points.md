# Точки

<xref:Akeldov.Math.Spatial2D.PointXY> задаёт положение в двумерном декартовом
пространстве. Координаты `X` и `Y` хранятся как числа с плавающей точкой одинарной точности и
измеряются в тех же мировых единицах, что и геометрия, в которой используется точка.

Используйте точки для положений: концов кривых, центров областей, мест выборки и результатов
пересечений. Направления, смещения и перемещения задавайте [векторами](vectors.md).

## Создание точки и чтение координат

Передайте координаты конструктору:

```csharp
using Akeldov.Math.Spatial2D;

var point = new PointXY(3.5f, 8f);

float x = point.X; // 3.5
float y = point.Y; // 8
```

`PointXY` — `readonly struct` со структурной семантикой значений. При присваивании переменной
или передаче в метод копируются координаты точки, а равенство определяется значениями координат,
не идентичностью объекта. После создания координаты нельзя изменить: перемещение или
преобразование точки возвращает новое значение.

Координаты также можно получить деконструкцией:

```csharp
var (x, y) = new PointXY(3.5f, 8f);
```

Конструктор отклоняет координаты `NaN`. Бесконечные значения разрешены, но геометрические API,
которым требуется конечное положение, проверяют это условие на своей публичной границе.

## Различие между точками и векторами

Точка обозначает положение. Вектор описывает перемещение между положениями или сдвиг,
применяемый к положению. Это различие отражено в операторах Spatial2D:

```csharp
var start = new PointXY(3f, 4f);
var offset = new VectorXY(1f, 2f);

PointXY end = start + offset;         // (4, 6)
PointXY movedBack = end - offset;     // (3, 4)
VectorXY displacement = end - start; // (1, 2)
```

Сложение двух точек намеренно не поддерживается. Сумма двух положений не имеет однозначного
геометрического смысла, тогда как перенос точки на вектор и разность двух точек определены
однозначно.

По той же причине преобразование точки в вектор координат и обратно выполняется явным
приведением:

```csharp
var point = new PointXY(3f, 2f);
VectorXY coordinates = (VectorXY)point;

var vector = new VectorXY(5f, 4f);
PointXY position = (PointXY)vector;
```

Числовые компоненты сохраняются, а явное приведение показывает изменение геометрического смысла
непосредственно в месте вызова.

## Расстояние между точками

<xref:Akeldov.Math.Spatial2D.PointXY.Distance(Akeldov.Math.Spatial2D.PointXY)> возвращает
евклидово расстояние между двумя точками:

```csharp
var a = new PointXY(0f, 0f);
var b = new PointXY(3f, 4f);

float distance = a.Distance(b); // 5
```

Если расстояния нужно только сравнить, используйте
<xref:Akeldov.Math.Spatial2D.PointXYExtensions.SquaredDistanceTo(Akeldov.Math.Spatial2D.PointXY,Akeldov.Math.Spatial2D.PointXY)>.
Этот метод не вычисляет квадратный корень:

```csharp
float squaredDistance = a.SquaredDistanceTo(b); // 25
```

`PointXY` также реализует <xref:Akeldov.Math.Spatial2D.IPointDistanceProvider>, поэтому точка
может выступать источником расстояния в API, принимающих эту абстракцию.

## Точное и приближённое равенство

`PointXY` реализует `IEquatable<PointXY>`. Методы `Equals` и операторы `==` и `!=` сравнивают
обе координаты точно:

```csharp
var a = new PointXY(1f, 2f);
var b = new PointXY(1f, 2f);

bool equal = a == b;      // true
bool different = a != b; // false
```

Точное равенство и соответствующий ему хеш-код позволяют использовать точки как ключи словаря.
Вычисления с плавающей точкой могут вносить небольшие погрешности округления, поэтому
геометрическую близость следует явно проверять методом
<xref:Akeldov.Math.Spatial2D.PointXYExtensions.AlmostEquals(Akeldov.Math.Spatial2D.PointXY,Akeldov.Math.Spatial2D.PointXY,System.Single)>:

```csharp
bool almostEqual = new PointXY(1f, 2f).AlmostEquals(
    new PointXY(1.000001f, 2f));

bool withinCustomTolerance = new PointXY(1f, 2f).AlmostEquals(
    new PointXY(1.01f, 2f),
    epsilon: 0.02f); // true
```

Допуск задаёт включительное евклидово расстояние. По умолчанию используется
`GeometryConstants.GeometryEpsilon`.

## Интерполяция и экстраполяция

<xref:Akeldov.Math.Spatial2D.PointXYExtensions.LerpTo(Akeldov.Math.Spatial2D.PointXY,Akeldov.Math.Spatial2D.PointXY,System.Single)>
перемещает исходную точку к целевой в соответствии с параметром `t`:

```csharp
var source = new PointXY(0f, 0f);
var target = new PointXY(10f, 4f);

PointXY start = source.LerpTo(target, 0f);     // (0, 0)
PointXY middle = source.LerpTo(target, 0.5f); // (5, 2)
PointXY end = source.LerpTo(target, 1f);       // (10, 4)
PointXY beyond = source.LerpTo(target, 1.5f);  // (15, 6)
```

Значения от `0` до `1` интерполируют положение вдоль отрезка. Значения за пределами этого
диапазона экстраполируют положение вдоль той же прямой. Параметр `t` должен быть конечным.

## Поворот и преобразование точек

Метод `Rotate` поворачивает точку вокруг заданного центра. По умолчанию углы в Spatial2D
выражаются в радианах:

```csharp
var point = new PointXY(2f, 0f);
var pivot = new PointXY(1f, 0f);

PointXY rotated = point.Rotate(pivot, MathF.PI / 2f);
// Приблизительно (1, 1)
```

Метод `Transform` выполняет преобразование относительно начала координат и добавляет перенос:

```csharp
var point = new PointXY(1f, 0f);

PointXY transformed = point.Transform(
    angle: MathF.PI / 2f,
    offset: new VectorXY(3f, 4f));
// Приблизительно (3, 5)
```

Перегрузка с масштабом выполняет операции в следующем порядке:

1. Равномерно масштабирует относительно начала координат.
2. Поворачивает вокруг начала координат на угол в радианах.
3. Применяет смещение.

```csharp
PointXY transformed = new PointXY(1f, 0f).Transform(
    scaleFactor: 2f,
    angle: MathF.PI / 2f,
    offset: new VectorXY(3f, 4f));
// Приблизительно (3, 6)
```

Для смещения и центра поворота также доступны перегрузки с `VectorXYInt`.

## Объекты с положением

`PointXY` реализует <xref:Akeldov.Math.Spatial2D.IHasPosition2D>. Свойство `Position` возвращает
саму точку:

```csharp
IHasPosition2D positioned = new PointXY(2f, 4f);
PointXY position = positioned.Position; // (2, 4)
```

Благодаря этому точку можно напрямую передавать алгоритмам, которые работают с объектами,
имеющими положение, например при пространственном разбиении и отсечении источников влияния.

Полный список членов приведён в справочнике API для
<xref:Akeldov.Math.Spatial2D.PointXY> и
<xref:Akeldov.Math.Spatial2D.PointXYExtensions>.
