# Создать и заполнить HexMap

Используйте <xref:Akeldov.Math.Hexes.HexMap`1>, чтобы хранить одно изменяемое значение для каждой
ячейки конечной прямоугольной гексагональной карты. Топология задаёт размеры и раскладку, а
`HexMap<TValue>` хранит сами значения.

## Создание карты

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 4,
    height: 3,
    layout: Layout.OddR);

var map = new HexMap<int>(topology);
```

Конструктор создаёт `topology.Count` ячеек и заполняет их значением `default(TValue)`. Для
`HexMap<int>` начальное значение каждой ячейки равно `0`.

## Заполнение и чтение

Обойдите строки и столбцы топологии и обращайтесь к ячейкам по `VectorXYInt`:

```csharp
for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        map[new VectorXYInt(x, y)] = x + y * 10;
    }
}

var cell = new VectorXYInt(2, 1);
int flatIndex = cell.Y * topology.Resolution.X + cell.X;

Console.WriteLine($"По индексу XY: {map[cell]}");
Console.WriteLine($"По плоскому индексу {flatIndex}: {map[flatIndex]}");
```

Результат:

```text
По индексу XY: 12
По плоскому индексу 6: 12
```

Индексатор `VectorXYInt` удобен для работы со строками и столбцами. Индексатор `int` обращается к
тем же ячейкам в построчном порядке: сначала изменяется `X`, затем `Y`. Раскладка влияет на
гексагональный смысл индексов, но не меняет порядок хранения.

## Инициализация готовыми значениями

Если значения уже находятся в построчном порядке, передайте массив конструктору:

```csharp
var values = new[]
{
     0,  1,  2,  3, // y = 0
    10, 11, 12, 13, // y = 1
    20, 21, 22, 23, // y = 2
};

var initializedMap = new HexMap<int>(topology, values);
```

Длина массива должна быть равна `topology.Count`. Конструктор сохраняет массив без копирования,
поэтому изменения через карту видны через `values`, и наоборот. Если совместное изменяемое
хранилище не нужно, передайте копию: `(int[])values.Clone()`.

Далее можно [найти соседей гекса](find-hex-neighbors.md). Полный контракт хранения и индексаторов
описан в разделе [«Карты»](../../concepts/data-storage/maps.md).
