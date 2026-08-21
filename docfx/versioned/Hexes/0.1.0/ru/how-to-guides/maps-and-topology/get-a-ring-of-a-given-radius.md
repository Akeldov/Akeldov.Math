# Получить кольцо заданного радиуса

В пакете нет отдельного метода построения кольца. Для конечной топологии обойдите её
допустимые индексы и оставьте ячейки, QRS-расстояние которых от центра точно равно заданному
радиусу.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Vectors.QRS;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 5,
    height: 5,
    layout: Layout.OddR);
var center = new VectorXYInt(2, 2);

List<VectorXYInt> ring = GetRing(topology, center, radius: 2);

Console.WriteLine(string.Join(", ", ring));

static List<VectorXYInt> GetRing(
    HexMapTopology topology,
    VectorXYInt center,
    int radius)
{
    if (radius < 0)
        throw new ArgumentOutOfRangeException(nameof(radius));

    if (center.X < 0 || center.X >= topology.Resolution.X ||
        center.Y < 0 || center.Y >= topology.Resolution.Y)
    {
        throw new ArgumentOutOfRangeException(nameof(center));
    }

    var result = new List<VectorXYInt>();
    VectorQRSInt centerQrs = center.ToQRSIndex(topology.Layout);

    for (int y = 0; y < topology.Resolution.Y; y++)
    {
        for (int x = 0; x < topology.Resolution.X; x++)
        {
            var index = new VectorXYInt(x, y);
            VectorQRSInt indexQrs = index.ToQRSIndex(topology.Layout);

            long distance = Math.Max(
                Math.Abs((long)indexQrs.Q - centerQrs.Q),
                Math.Max(
                    Math.Abs((long)indexQrs.R - centerQrs.R),
                    Math.Abs((long)indexQrs.S - centerQrs.S)));

            if (distance == radius)
                result.Add(index);
        }
    }

    return result;
}
```

Метод возвращает новый изменяемый список в построчном порядке:

```text
(1, 0), (2, 0), (3, 0), (0, 1), (3, 1), (0, 2), (4, 2), (0, 3), (3, 3), (1, 4), (2, 4), (3, 4)
```

`radius` — расстояние по сетке в шагах между смежными по ребру ячейками, а не геометрический
размер гекса. При нулевом радиусе метод возвращает только центр. Полное кольцо положительного
радиуса содержит `6 * radius` ячеек, но перебор обрезает результат по границам топологии, поэтому
для центра у края или при слишком большом радиусе список может быть короче либо пуст.
Отрицательный радиус и центр вне топологии приводят к `ArgumentOutOfRangeException`.

Формула расстояния описана в разделе
[«QRS-координаты»](../../concepts/fundamentals/coordinate-systems/qrs-coordinates.md), границы
конечной карты — в разделе [«Топология»](../../concepts/hex-grid-model/topology.md), а смежность
на один шаг — в рецепте [«Найти соседей гекса»](find-hex-neighbors.md).
