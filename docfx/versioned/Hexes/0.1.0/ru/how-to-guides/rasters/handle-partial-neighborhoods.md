# Обработать частичные окрестности

Используйте частичные индексные растры, когда точки выборки касаются конечной гексагональной карты или выходят за её границу.
Флаги наличия отличают доступные индексы внутри карты от логических соседей на окружающей бесконечной сетке. Всегда проверяйте свойство `Has...` перед использованием
сохранённого индекса с ограниченной картой.

## Создать частичный Triplet-растр

Следующая сетка выборки включает внешнее поле, поэтому некоторые ячейки растра могут ссылаться на позиции за пределами исходной топологии `3 × 2`:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var mapGeometry = new HexMapGeometry(
    width: 3,
    height: 2,
    radius: 1f,
    layout: Layout.OddR);

var values = new HexMap<int>(mapGeometry.Topology, new[]
{
    10, 20, 30,
    40, 50, 60
});

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 8f,
    margin: mapGeometry.Radius);

var indexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Каждая ячейка растра хранит `PartialTriplet<VectorXYInt>` с позициями `Main`, `Left` и `Right`. Их свойства `HasMain`, `HasLeft` и
`HasRight` независимы: у выборки с `Main` за пределами карты левый или правый сосед всё ещё может находиться внутри карты.

## Использовать только присутствующие позиции триплета

`TryGetValue` возвращает `true`, только если координаты растра допустимы и хотя бы одна из трёх позиций гексов принадлежит исходной топологии. Это удобно для пропуска пустых выборок:

```csharp
var sample = new VectorXYInt(
    indexRaster.Resolution.X / 2,
    indexRaster.Resolution.Y / 2);

if (!indexRaster.TryGetValue(
        sample,
        out PartialTriplet<VectorXYInt> indices))
{
    Console.WriteLine("У выборки нет значений исходной карты.");
    return;
}

int total = 0;
int count = 0;

if (indices.HasMain)
{
    total += values[indices.Main];
    count++;
}

if (indices.HasLeft)
{
    total += values[indices.Left];
    count++;
}

if (indices.HasRight)
{
    total += values[indices.Right];
    count++;
}

float average = (float)total / count;
Console.WriteLine($"Среднее присутствующих значений: {average}");
```

Поскольку успешный `TryGetValue` гарантирует наличие хотя бы одной позиции, `count` будет положительным. Если код уже проверил координаты растра и должен отличать пустую выборку, прочитайте индексатор и сравните `Presence` с `TripletPresenceFlags.None`.

## Обработать частичную окрестность из семи индексов

Используйте <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletRaster>, когда для операции нужны `Main` и все шесть соседних по ребру позиций:

```csharp
var septupletRaster = new IndexPartialSeptupletRaster(
    mapGeometry,
    rasterGeometry);

PartialSeptuplet<VectorXYInt> neighborhood = septupletRaster[sample];

if (!neighborhood.HasMain)
{
    Console.WriteLine("Выборка находится за пределами исходной карты.");
    return;
}

int neighborhoodTotal = values[neighborhood.Main];

if (neighborhood.HasAdjacent0)
    neighborhoodTotal += values[neighborhood.Adjacent0];
if (neighborhood.HasAdjacent1)
    neighborhoodTotal += values[neighborhood.Adjacent1];
if (neighborhood.HasAdjacent2)
    neighborhoodTotal += values[neighborhood.Adjacent2];
if (neighborhood.HasAdjacent3)
    neighborhoodTotal += values[neighborhood.Adjacent3];
if (neighborhood.HasAdjacent4)
    neighborhoodTotal += values[neighborhood.Adjacent4];
if (neighborhood.HasAdjacent5)
    neighborhoodTotal += values[neighborhood.Adjacent5];

Console.WriteLine($"Сумма присутствующей окрестности: {neighborhoodTotal}");
```

Для частичного септуплета `HasMain == false` означает, что все семь флагов сброшены. Когда `Main` присутствует, каждый `HasAdjacentN` независимо сообщает, находится ли соответствующий
сосед по ребру внутри топологии. У Septuplet-растров нет `TryGetValue`, поэтому проверяйте координаты растра или используйте проверяемый индексатор `[VectorXYInt]`, как выше.

## Не определять наличие по сохранённым значениям

Два семейства частичных растров по-разному хранят данные в отсутствующих позициях:

| Растр | Поведение флагов | Данные в отсутствующей позиции |
|---|---|---|
| `IndexPartialTripletRaster` | Проверяет `Main`, `Left` и `Right` независимо | `default(VectorXYInt)` |
| `IndexPartialSeptupletRaster` | Сбрасывает все флаги при отсутствующем `Main`; иначе проверяет каждого соседа | Сохраняет вычисленный логический индекс |

Для обоих типов истину определяет флаг. Никогда не сравнивайте индекс с `VectorXYInt.Zero`: `(0, 0)` — допустимая ячейка исходной карты. `ToTriplet()` и `ToSeptuplet()`
сохраняют записанные значения, но отбрасывают всю информацию о наличии, поэтому вызывайте их только после обработки отсутствующих позиций. Частичные растры не прижимают отсутствующие индексы к ближайшей ячейке карты.

Полные варианты описаны в разделах [«Создать индексный Triplet-растр»](create-an-index-triplet-raster.md) и
[«Создать индексный Septuplet-растр»](create-an-index-septuplet-raster.md). Для частичных весов интерполяции перейдите к разделу
[«Создать барицентрический растр»](create-a-barycentric-raster.md). Базовые типы данных описаны в разделе
[«Полные и частичные окрестности»](../../concepts/data-storage/complete-and-partial-neighborhoods.md).
