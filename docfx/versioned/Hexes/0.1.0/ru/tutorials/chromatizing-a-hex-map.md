# Хроматизация гексагональной карты

В этом учебнике каждой ячейке гексагональной карты назначается один из трёх устойчивых классов.
Гексы одного класса никогда не имеют общего ребра, поэтому классы позволяют организовать операцию
в три прохода, внутри которых непосредственно соседние ячейки не изменяются одновременно.

Готовый консольный пример выводит такой узор для карты `OddR` размером `5` на `4`:

```text
0 1 2 0 1
2 0 1 2 0
0 1 2 0 1
2 0 1 2 0
```

Сначала пройдите раздел [«Создание проекта»](creating-a-hex-map/creating-a-project.md) или
используйте существующий проект со ссылкой на `Akeldov.Math.Hexes`.

## Создание топологии и хроматической карты

Создайте топологию и передайте её в
<xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(
    width: 5,
    height: 4,
    layout: Layout.OddR);

var chromaticMap = new ChromaticIndexMap(topology);
```

При создании карты для каждой ячейки вычисляется один класс типа `byte`: `0`, `1` или `2`.
Класс зависит от индекса ячейки и раскладки, а не от значений, хранящихся в другой карте.

## Вывод узора

Прочитайте класс по каждому индексу строки и столбца:

```csharp
for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        byte chromaticClass = chromaticMap[new VectorXYInt(x, y)];
        Console.Write($"{chromaticClass} ");
    }

    Console.WriteLine();
}
```

Классы каждой горизонтальной пары различаются. Смещение соседних строк также обеспечивает разные
классы у соприкасающихся между ними ячеек. Этот инвариант сохраняется для `EvenR`, `OddQ` и
`EvenQ`, хотя выведенные узоры будут отличаться.

## Группировка ячеек в три прохода

Сгруппируйте индексы конечной карты по классам:

```csharp
var passes = new[]
{
    new List<VectorXYInt>(),
    new List<VectorXYInt>(),
    new List<VectorXYInt>()
};

for (int y = 0; y < topology.Resolution.Y; y++)
{
    for (int x = 0; x < topology.Resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        passes[chromaticMap[index]].Add(index);
    }
}

for (int classIndex = 0; classIndex < passes.Length; classIndex++)
{
    Console.WriteLine($"Проход {classIndex}");

    foreach (VectorXYInt index in passes[classIndex])
        Console.WriteLine($"  {index}");
}
```

Внутри одного прохода никакие два индекса не соответствуют соседним по ребру гексам. Это удобно
для локальных изменений на месте, когда нельзя одновременно записывать соседние ячейки.
Параллельное выполнение дополнительно требует, чтобы принадлежащее вызывающему коду хранилище
поддерживало одновременную запись в разные ячейки.

Гарантия относится только к непосредственным соседям по ребру. Если операция читает или изменяет
ячейки на расстоянии двух и более шагов, элементы одного класса всё равно могут влиять друг на
друга. Для таких операций используйте более широкое разбиение или отдельную выходную карту.

## Класс одного индекса без карты

Для редких запросов вычисляйте класс напрямую, не создавая конечную карту:

```csharp
var index = new VectorXYInt(3, 2);
int chromaticClass = index.GetChromaticClass(topology.Layout);

Console.WriteLine(chromaticClass); // 0
```

`GetChromaticClass` принимает также отрицательные индексы и индексы за пределами карты, потому что
классифицирует подразумеваемую бесконечную гексагональную решётку. Индексатор
`ChromaticIndexMap`, напротив, принимает только индексы внутри своей конечной топологии.

Теперь карта разбита на три воспроизводимых прохода. Математический инвариант и правила порядка в
тройках описаны в статье [«Хроматизация»](../concepts/spatial-algorithms/chromatization.md).
Если нужны упорядоченные по классам веса интерполяции на прямоугольной сетке выборки, перейдите к
разделу
[«Создание хроматического растра»](../how-to-guides/chromatization/create-a-chromatic-raster.md).

Законченный RGB-пример приведён в следующем учебнике:
[«Хроматизация и барицентрическая интерполяция»](chromatic-barycentric-interpolation.md).
