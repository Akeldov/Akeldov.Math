# Преобразовать QRS в координаты Spatial2D

Используйте `GetHexOffset` для целочисленного QRS-индекса или `ToVectorXY` для дробного QRS,
когда логическую координату нужно разместить в непрерывном пространстве Spatial2D.

## Центр целочисленного гекса

Задайте радиус, раскладку и центр нулевого гекса:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
const float hexRadius = 2f;
var zeroHexCenter = new VectorXY(10f, 20f);
var qrsIndex = new VectorQRSInt(q: 2, r: 1);

VectorXY offset = qrsIndex.GetHexOffset(hexRadius, layout);
VectorXY center = zeroHexCenter + offset;
var centerPoint = new PointXY(center.X, center.Y);

Console.WriteLine(FormattableString.Invariant(
    $"Offset: ({offset.X:F3}, {offset.Y:F3})"));
Console.WriteLine(FormattableString.Invariant(
    $"Center: ({centerPoint.X:F3}, {centerPoint.Y:F3})"));
```

Результат приблизительно равен:

```text
Offset: (8.660, 3.000)
Center: (18.660, 23.000)
```

`GetHexOffset` возвращает `VectorXY`: смещение центра выбранного гекса относительно центра
нулевого гекса. Добавляйте его к своему `zeroHexCenter`; не интерпретируйте начало как угол карты.

## Дробная QRS-координата

<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS> преобразуется в базис сетки единичного радиуса.
Умножьте результат на фактический радиус:

```csharp
var fractional = new VectorQRS(q: 2.25f, r: 0.5f);

VectorXY fractionalOffset =
    fractional.ToVectorXY(layout) * hexRadius;
VectorXY fractionalPosition = zeroHexCenter + fractionalOffset;
```

`OddR` и `EvenR` используют один пространственный QRS-базис с гексами вершиной вверх; `OddQ` и
`EvenQ` — один базис с плоской стороной вверх. Правило чётного или нечётного смещения влияет на
индексы хранилища, но не меняет положение одной QRS-координаты.

Обратный выбор ячейки для пространственной точки описан в рецепте
[«Найти ближайший гекс для точки»](find-the-nearest-hex-to-a-point.md).
