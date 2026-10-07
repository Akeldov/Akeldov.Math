# Повернуть гексагональные координаты

Используйте `Rotate(SixfoldAngle)` для точного поворота по сетке с шагом 60 градусов или
`Rotate(float angleRad)` для произвольного угла с дробным результатом. Положительный угол означает
поворот против часовой стрелки.

## Точный поворот на 60 градусов

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;

var index = new VectorQRSInt(q: 2, r: -5);

VectorQRSInt rotated = index.Rotate(SixfoldAngle.Deg60);

Console.WriteLine(
    $"Rotated: ({rotated.Q}, {rotated.R}, {rotated.S})");
```

Результат:

```text
Rotated: (5, -3, -2)
```

<xref:Akeldov.Math.Hexes.SixfoldAngle> содержит шесть значений от `Deg0` до `Deg300`. Поворот
переставляет QRS-компоненты и меняет их знаки, поэтому целочисленный индекс и инвариант
`Q + R + S = 0` сохраняются без погрешности.

## Поворот относительно другого гекса

`Rotate` использует начало QRS как центр. Для другого центра перенесите индекс, поверните смещение
и верните его обратно:

```csharp
var pivot = new VectorQRSInt(q: 1, r: -1);
var point = new VectorQRSInt(q: 3, r: -2);

VectorQRSInt aroundPivot =
    (point - pivot).Rotate(SixfoldAngle.Deg120) + pivot;
```

## Произвольный угол

Числовая перегрузка принимает радианы и возвращает дробный
<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRS>:

```csharp
VectorQRS rotatedBy30Degrees = index.Rotate(MathF.PI / 6f);

VectorQRSInt nearestHex =
    rotatedBy30Degrees.ToQRSIndex(Layout.OddR);
```

Не передавайте `60f` для поворота на 60 градусов: это будет 60 радиан. Используйте
`SixfoldAngle.Deg60` или `MathF.PI / 3f`.

QRS-поворот не зависит от раскладки. `Layout` нужен в последнем примере только для выбора
ближайшего целочисленного гекса после дробного поворота.

Подробнее см. в разделе
[«Повороты и преобразования»](../../concepts/fundamentals/rotations-and-transformations.md).
