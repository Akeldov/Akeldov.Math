# Преобразовать индекс строки и столбца в QRS

Используйте `ToQRSIndex(layout)`, когда индекс `VectorXYInt` из прямоугольной карты нужно
преобразовать в независимую от смещения строк и столбцов QRS-координату.

## Преобразование индекса хранилища

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

Layout layout = Layout.OddR;
var storageIndex = new VectorXYInt(4, 3);

VectorQRSInt qrsIndex = storageIndex.ToQRSIndex(layout);

Console.WriteLine(
    $"QRS: ({qrsIndex.Q}, {qrsIndex.R}, {qrsIndex.S})");
```

Результат:

```text
QRS: (3, 3, -6)
```

<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> удобен для вычисления расстояний, смещений и
поворотов, поскольку одна QRS-координата обозначает тот же гекс независимо от нечётного или
чётного правила смещения.

## Сохранение раскладки на границе хранилища

QRS-значение не запоминает исходную раскладку. Чтобы позднее получить исходный индекс, передайте
то же значение `layout`:

```csharp
VectorXYInt restored = qrsIndex.ToXYIndex(layout);

Console.WriteLine(restored == storageIndex); // True
```

Если преобразовать результат с другой раскладкой, QRS останется корректным, но вернётся другой
индекс строк и столбцов. Поэтому храните раскладку рядом с топологией, а не выбирайте её заново в
каждом вызове.

`ToQRSIndex` не проверяет границы конкретной карты: отрицательные и выходящие за разрешение
`VectorXYInt` допустимы на неограниченной координатной сетке.

Прямое преобразование описано в рецепте
[«Преобразовать QRS в индекс строки и столбца»](convert-qrs-to-row-and-column-indices.md).
