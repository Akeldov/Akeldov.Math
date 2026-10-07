# Пространственные координаты

Пространственные координаты размещают логическую гексагональную решётку в непрерывном декартовом
пространстве Akeldov.Math.Spatial2D. API гексов используют `VectorXY` для смещений, центров,
начала координат, размеров и вершин в мировом пространстве. `PointXY` применяется для точек, по
которым выполняется выборка или определяется содержащий их гекс.

Не смешивайте эти значения с индексами строк и столбцов `VectorXYInt`. Пара целочисленных
координат хранилища не задаёт физическое положение, пока неизвестны раскладка, радиус гекса и
начало координат.

## Мировые оси и единицы

В Spatial2D положительная ось X направлена вправо, а положительная ось Y — вверх.
Akeldov.Math.Hexes не задаёт конкретную единицу: координаты могут измеряться в пикселях, метрах,
игровых или любых других согласованных мировых единицах.

Радиус гекса — расстояние от центра до вершины. Апофема — расстояние от центра до стороны:

```text
апофема = радиус * sqrt(3) / 2
```

Расстояние между соседними центрами равно `sqrt(3) * радиус`. API, принимающие радиус, требуют
конечное значение больше нуля.

## Отображение QRS на декартовы оси

Пространственный базис QRS зависит от ориентации раскладки. Нечётный и чётный варианты одной
ориентации используют одинаковый непрерывный базис:

| Раскладки | Ориентация | Смещение X | Смещение Y |
|---|---|---|---|
| `OddR`, `EvenR` | Вершиной вверх | `sqrt(3) * радиус * (Q + R / 2)` | `3 * радиус * R / 2` |
| `OddQ`, `EvenQ` | Горизонтальной стороной вверх | `3 * радиус * Q / 2` | `sqrt(3) * радиус * (R + Q / 2)` |

Формулы дают смещение относительно центра нулевого гекса. Чтобы получить центр в мировом
пространстве, прибавьте выбранное начало координат.

Для целочисленной QRS-координаты используйте `GetHexOffset(hexRadius, layout)`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
float radius = 10f;
var qrsIndex = new VectorQRSInt(q: 2, r: -1);

VectorXY offset = qrsIndex.GetHexOffset(radius, layout);
var origin = new VectorXY(100f, 50f);
VectorXY center = origin + offset;
```

`GetHexOffset` не добавляет начало координат. Поэтому метод подходит для составления переносов
или размещения одной логической сетки в разных точках мирового пространства.

## Преобразование дробных QRS- и XY-векторов

`ToVectorXY(Layout)` отображает дробный
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> в `VectorXY` в базисе гексагональной сетки с
единичным радиусом. `ToVectorQRS(Layout)` выполняет обратное преобразование:

```csharp
Layout layout = Layout.OddR;
const float radius = 10f;
var fractionalQrs = new VectorQRS(q: 1.5f, r: -0.25f);

VectorXY unitRadiusOffset = fractionalQrs.ToVectorXY(layout);
VectorQRS restored = unitRadiusOffset.ToVectorQRS(layout);
VectorXY physicalOffset = unitRadiusOffset * radius;
```

Используйте одну ориентацию в обоих вызовах. После прямого и обратного преобразований результат
может отличаться из-за небольшой погрешности вычислений с плавающей точкой, поэтому сравнивайте
значения с подходящим для приложения допуском.

`ToNormalizedAxial(hexRadius)` решает другую задачу: он делит на радиус QRS-координаты, уже
выраженные в масштабированных радиусом единицах QRS. Метод не поворачивает оси и не преобразует
XY-вектор:

```csharp
var scaledQrs = new VectorQRS(15f, -2.5f);
VectorQRS normalized = scaledQrs.ToNormalizedAxial(10f);

// normalized равен (1.5, -0.25, -1.25)
```

## Получение центра по индексу хранилища

`VectorXYInt.GetHexCenter` объединяет раскладку со смещением, радиус и начало координат нулевого
гекса:

```csharp
Layout layout = Layout.OddR;
const float radius = 10f;
var origin = new VectorXY(100f, 50f);
var qrsIndex = new VectorQRSInt(q: 2, r: -1);

VectorXY center = origin + qrsIndex.GetHexOffset(radius, layout);
VectorXYInt index = qrsIndex.ToXYIndex(layout);
VectorXY sameCenter = index.GetHexCenter(
    radius,
    origin,
    layout);
```

`sameCenter` равен `center` с учётом возможной погрешности вычислений с плавающей точкой.
Параметр начала координат — центр индекса хранилища `(0, 0)`, который также является нулевым
QRS-гексом при использовании той же раскладки.

Перегрузка без начала координат использует стандартный центр нулевого гекса:

| Ориентация | Стандартное начало координат |
|---|---|
| Вершиной вверх | `(апофема, радиус)` |
| Горизонтальной стороной вверх | `(радиус, апофема)` |

Статический вспомогательный метод `GetHexCenter(q, r, hexRadius, layout)` на основе QRS использует
те же стандартные значения. Задавайте начало координат явно, если карту нужно совместить с
существующей мировой системой координат.

## Совместное хранение топологии и геометрии

<xref:Akeldov.Math.Hexes.Geometry.HexMapGeometry> объединяет
<xref:Akeldov.Math.Hexes.HexMapTopology> с началом координат, радиусом и вычисленной апофемой.
Используйте его, когда несколько операций должны согласованно применять размеры хранилища и
физическое размещение:

```csharp
var topology = new HexMapTopology(
    width: 8,
    height: 6,
    layout: Layout.OddR);
var geometry = new HexMapGeometry(
    topology,
    origin: new VectorXY(100f, 50f),
    radius: 10f);

VectorXY mapOrigin = geometry.Origin;
float mapRadius = geometry.Radius;
float mapApothem = geometry.Apothem;
```

Конструктор геометрии требует конечных компонентов начала координат и конечного положительного
радиуса. Один экземпляр `HexMapGeometry` предотвращает случайное сочетание топологии одной карты
с началом координат или радиусом другой.

Переход точки `PointXY` к содержащему её гексу описан на странице
[«Дискретизация координат»](../coordinate-discretization.md), а центры, вершины и границы карт —
на странице [«Геометрия гексагональной сетки»](../../hex-grid-model/geometry.md).
