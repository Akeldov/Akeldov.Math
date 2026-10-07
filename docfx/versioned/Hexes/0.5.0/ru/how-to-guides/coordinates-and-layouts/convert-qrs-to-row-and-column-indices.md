# Преобразовать QRS в индекс строки и столбца

Используйте `ToXYIndex(layout)`, когда целочисленную QRS-координату нужно применить как индекс
столбца и строки прямоугольного хранилища.

## Преобразование индекса

Подключите пространства имён Hexes, QRS и Spatial2D:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
```

Передайте ту же раскладку, которую использует карта:

```csharp
Layout layout = Layout.OddR;
var qrsIndex = new VectorQRSInt(q: 3, r: 3);

VectorXYInt storageIndex = qrsIndex.ToXYIndex(layout);

Console.WriteLine($"XY: ({storageIndex.X}, {storageIndex.Y})");
```

Результат:

```text
XY: (4, 3)
```

Для `OddR` нечётная строка `R = 3` смещена вправо, поэтому столбец `X` равен `4`. Компонента
`S = -6` следует из инварианта QRS и отдельно в преобразование не передаётся.

## Проверка обратным преобразованием

Если нужно убедиться, что параметры согласованы, преобразуйте результат обратно:

```csharp
VectorQRSInt restored = storageIndex.ToQRSIndex(layout);

Console.WriteLine(restored == qrsIndex); // True
```

Пара `ToXYIndex` и `ToQRSIndex` даёт точное обратное преобразование для положительных и
отрицательных индексов, если оба вызова используют одну раскладку.

Метод работает на неограниченной логической сетке. Перед обращением к `HexMap<T>` отдельно
проверьте, что `storageIndex.X` и `storageIndex.Y` входят в разрешение топологии.

Обратная задача описана в рецепте
[«Преобразовать индекс строки и столбца в QRS»](convert-row-and-column-indices-to-qrs.md).
