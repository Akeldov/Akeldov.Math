# Дискретные индексы

Дискретный индекс обозначает элемент двумерной целочисленной сетки. В
Akeldov.Math.Spatial2D тип <xref:Akeldov.Math.Spatial2D.VectorXYInt> хранит два целочисленных
компонента, используемых для ячеек растра, разрешений, размеров и дискретных смещений.

Сам тип хранит числа, но не назначает им роль. Допустимые значения и смысл `VectorXYInt` —
индекс, размер или смещение — определяет принимающий API.

## Явное обозначение роли

Один тип значений служит нескольким связанным задачам:

| Роль | Смысл | Типичные ограничения |
|---|---|---|
| Индекс | Адрес одного элемента сетки | Начинается с нуля и входит в разрешение |
| Разрешение | Количество столбцов и строк | Оба компонента положительны |
| Смещение | Относительный переход между индексами | Компоненты могут быть отрицательными |
| Размеры | Целочисленные ширина и высота | Ограничения задаёт принимающий API |

Используйте имена `index`, `resolution` и `offset`, чтобы роль оставалась понятной:

```csharp
using Akeldov.Math.Spatial2D;

var index = new VectorXYInt(3, 2);
var resolution = new VectorXYInt(5, 4);
var offset = new VectorXYInt(-1, 1);

VectorXYInt neighbor = index + offset; // (2, 3)
```

Выражение `index + index` корректно численно, но обычно не имеет полезного смысла для адресации.
Считайте роль, заданную окружающим API, частью контракта значения.

## Нулевая индексация растров

Двумерные индексы растров Spatial2D начинаются с нуля. `X` выбирает столбец, а `Y` — строку.
Для разрешения `(width, height)` допустимы полуоткрытые диапазоны:

```text
0 <= X < width
0 <= Y < height
```

Например, у растра с разрешением `(5, 4)` первый индекс равен `(0, 0)`, а последний — `(4, 3)`.
Само разрешение не является допустимым индексом.

```csharp
var resolution = new VectorXYInt(5, 4);
var index = new VectorXYInt(4, 3);

bool isInside =
    index.X >= 0 && index.X < resolution.X &&
    index.Y >= 0 && index.Y < resolution.Y; // true
```

Встроенный `Raster<TValue>` проверяет индексы при обращении. Отрицательный компонент или
компонент, равный соответствующему компоненту разрешения либо превышающий его, приводит к
`ArgumentOutOfRangeException`.

## Обращение к ячейкам растра

Контракты растров принимают как индекс `VectorXYInt`, так и отдельные компоненты `x` и `y`:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var raster = new Raster<char>(
    resolution: new VectorXYInt(3, 2),
    values: new[]
    {
        'a', 'b', 'c',
        'd', 'e', 'f'
    });

char first = raster[new VectorXYInt(0, 0)]; // 'a'
char secondRowFirst = raster[0, 1];         // 'd'
char last = raster[new VectorXYInt(2, 1)];  // 'f'
```

`Raster<TValue>` хранит значения построчно. Его двумерный индекс `(x, y)` преобразуется так:

```text
flatIndex = y * resolution.X + x
```

Поэтому в сохранённом массиве быстрее всего изменяется индекс `X`. Однако порядок плоского
индекса `int` в общем контракте `IRaster<TValue>` определяется реализацией. При работе с
произвольной реализацией через интерфейс нельзя полагаться на построчный порядок плоских
индексов.

## Перебор внутри разрешения

Чтобы обойти конкретный `Raster<TValue>` в порядке хранения, перебирайте сначала `Y`, затем `X`:

```csharp
VectorXYInt resolution = raster.Resolution;

for (int y = 0; y < resolution.Y; y++)
{
    for (int x = 0; x < resolution.X; x++)
    {
        var index = new VectorXYInt(x, y);
        char value = raster[index];
    }
}
```

Компоненты разрешения растра должны быть положительными. `Raster<TValue>` также требует, чтобы
число ячеек помещалось в одномерный массив, а переданный массив значений содержал ровно
`resolution.X * resolution.Y` элементов.

## Соседние индексы и смещения

Целочисленные базисные векторы удобно использовать как смещения на одну ячейку вдоль декартовых
осей:

```csharp
var index = new VectorXYInt(2, 2);

VectorXYInt right = index + VectorXYInt.BasisX; // (3, 2)
VectorXYInt left = index - VectorXYInt.BasisX;  // (1, 2)
VectorXYInt up = index + VectorXYInt.BasisY;    // (2, 3)
VectorXYInt down = index - VectorXYInt.BasisY;  // (2, 1)
```

Арифметические операции не знают разрешение целевой сетки. Проверяйте каждый полученный индекс
перед использованием: смещение от граничной ячейки может дать отрицательный индекс или индекс
на верхней границе.

## Преобразование ячеек в мировое пространство

Индекс не является положением в мировом пространстве.
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> связывает дискретное разрешение растра
с непрерывными границами. Начало сетки находится в левом нижнем углу, а `GetCellCenter`
возвращает центр ячейки в мировых координатах:

```csharp
var geometry = new RasterGeometry(
    origin: new PointXY(1f, 2f),
    size: new VectorXY(10f, 6f),
    resolution: new VectorXYInt(5, 3));

VectorXY cellSize = geometry.CellSize; // (2, 2)
PointXY first = geometry.GetCellCenter(0, 0); // (2, 3)
PointXY last = geometry.GetCellCenter(new VectorXYInt(4, 2)); // (10, 7)
```

Центр отстоит от границ на половину ячейки. Для каждой оси преобразование имеет вид:

```text
center = origin + (index + 0.5) * cellSize
```

Увеличение `X` перемещает ячейку вдоль положительной мировой оси X, а увеличение `Y` — вдоль
положительной мировой оси Y. Соглашения о порядке строк в файле изображения относятся к
кодированию и не изменяют это определение сетки в мировом пространстве.

## Явное определение перехода из мирового пространства

Преобразование мировой точки обратно в ячейку требует правила для границ. Точка может лежать за
пределами растра, на его внешней границе или точно между ячейками. `RasterGeometry` предоставляет
однозначное преобразование индекса в центр; код обратного преобразования должен самостоятельно
определить обработку перечисленных случаев.

Распространённое правило полуоткрытых ячеек сначала выражает точку в координатах ячеек,
отклоняет значения за пределами `[0, resolution)`, а затем применяет `MathF.Floor`:

```csharp
PointXY point = new PointXY(4.5f, 5.5f);

float cellX = (point.X - geometry.Origin.X) / geometry.CellSize.X;
float cellY = (point.Y - geometry.Origin.Y) / geometry.CellSize.Y;

if (cellX < 0f || cellX >= geometry.Resolution.X ||
    cellY < 0f || cellY >= geometry.Resolution.Y)
{
    throw new ArgumentOutOfRangeException(nameof(point));
}

var index = new VectorXYInt(
    (int)MathF.Floor(cellX),
    (int)MathF.Floor(cellY));
```

Не заменяйте `Floor` прямым приведением, если возможны отрицательные координаты: приведение к
целому усекает к нулю и может ошибочно поместить точку чуть ниже начала сетки в индекс ноль.
Округление применяйте только тогда, когда требуется ближайшая координата сетки, а не содержащая
точку ячейка. Семантика преобразования и округления описана в разделе
[«Векторы»](vectors.md).

## Точное сравнение индексов

Дискретные индексы используют структурное точное равенство. Поэтому `VectorXYInt` подходит для
ключей словаря и элементов множества:

```csharp
using System.Collections.Generic;

var visited = new HashSet<VectorXYInt>();
visited.Add(new VectorXYInt(3, 2));

bool alreadyVisited = visited.Contains(new VectorXYInt(3, 2)); // true
```

Сравнение с допуском предназначено для непрерывных точек и векторов, а не для дискретных
индексов.

Полные списки членов приведены в справочнике API для
<xref:Akeldov.Math.Spatial2D.VectorXYInt> и
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>.
