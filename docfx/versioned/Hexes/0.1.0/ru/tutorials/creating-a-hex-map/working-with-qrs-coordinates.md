# Работа с QRS-координатами

В этой части руководства вы преобразуете индекс строки и столбца в QRS-координату и обратно.
Такое разделение позволяет хранить прямоугольную карту по индексам `X/Y`, а операции на
гексагональной сетке выполнять в независимой от раскладки системе QRS.

## Две формы одного индекса

Добавьте пространства имён в начало `Program.cs`:

```csharp
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;
```

После объявления `layout` добавьте преобразование:

```csharp
var storageIndex = new VectorXYInt(3, 2);
VectorQRSInt qrsIndex = storageIndex.ToQRSIndex(layout);
VectorXYInt restoredIndex = qrsIndex.ToXYIndex(layout);

Console.WriteLine($"Индекс XY:  ({storageIndex.X}, {storageIndex.Y})");
Console.WriteLine($"Индекс QRS: ({qrsIndex.Q}, {qrsIndex.R}, {qrsIndex.S})");
Console.WriteLine($"Обратное преобразование: {restoredIndex == storageIndex}");
```

Для `OddR` результат будет таким:

```text
Индекс XY:  (3, 2)
Индекс QRS: (2, 2, -4)
Обратное преобразование: True
```

<xref:Akeldov.Math.Hexes.Vectors.QRS.VectorQRSInt> хранит `Q`, `R` и вычисляемую компоненту `S`,
причём всегда выполняется инвариант `Q + R + S = 0`. Конструктор с двумя аргументами вычисляет
`S` автоматически:

```csharp
var direction = new VectorQRSInt(1, 0);
VectorQRSInt adjacentQrsIndex = qrsIndex + direction;

Console.WriteLine(
    $"Смещение по Q: ({adjacentQrsIndex.Q}, {adjacentQrsIndex.R}, {adjacentQrsIndex.S})");
```

QRS-координата не зависит от правила смещения строк или столбцов. Значение `layout` требуется
только при переходе между QRS и индексом прямоугольного хранилища.

Переменные `storageIndex` и `qrsIndex` были нужны для демонстрации; дальнейшие шаги могут оставить
их в программе или удалить. Переходите к разделу
[«Создание топологии»](creating-the-topology.md).
