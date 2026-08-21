# Создать барицентрический растр

Используйте <xref:Akeldov.Math.Hexes.Topology.BarycentricTripletRaster>, чтобы заранее вычислить три веса
интерполяции в центре каждой ячейки растра. Веса `Main`, `Left` и `Right` соответствуют тем же позициям
в `IndexTripletRaster`; объедините два растра для интерполяции значений, заданных в центрах гексов.

## Создать согласованные растры поиска

Задайте конечную исходную карту и общую геометрию выборки. Для ограниченной карты создайте частичные
варианты, чтобы выборки на её границе не содержали индексы гексов за пределами карты:

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

var values = new HexMap<float>(mapGeometry.Topology, new[]
{
    10f, 20f, 30f,
    40f, 50f, 60f
});

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 8f,
    margin: mapGeometry.Radius);

var indexRaster = new IndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);

var weightRaster = new BarycentricPartialTripletRaster(
    mapGeometry,
    rasterGeometry);
```

Оба растра поиска должны получать одинаковые `HexMapGeometry` и `RasterGeometry`. Совпадения одного
разрешения недостаточно: начало координат и размеры в пространстве определяют точку выборки каждой
ячейки. При общей геометрии позиции и веса `Main`, `Left` и `Right` точно соответствуют друг другу.

Сокращённый вариант `new BarycentricPartialTripletRaster(mapGeometry)` создаёт сетку с плотностью один
пиксель на апофему без внешнего поля. Используйте явную геометрию, если растр должен совпадать с другим
растром или выходным изображением.

## Интерполировать одну выборку

Прочитайте триплеты индексов и весов по одной координате растра, учтите только присутствующие позиции
и нормализуйте оставшиеся веса:

```csharp
var sample = new VectorXYInt(
    weightRaster.Resolution.X / 2,
    weightRaster.Resolution.Y / 2);

if (indexRaster.TryGetValue(sample, out PartialTriplet<VectorXYInt> indices) &&
    weightRaster.TryGetValue(sample, out PartialTriplet<float> weights))
{
    float interpolated = Interpolate(values, indices, weights);
    Console.WriteLine($"Интерполированное значение: {interpolated}");
}

static float Interpolate(
    HexMap<float> map,
    PartialTriplet<VectorXYInt> indices,
    PartialTriplet<float> weights)
{
    float weightedValue = 0f;
    float weightSum = 0f;

    if (indices.HasMain)
    {
        weightedValue += map[indices.Main] * weights.Main;
        weightSum += weights.Main;
    }

    if (indices.HasLeft)
    {
        weightedValue += map[indices.Left] * weights.Left;
        weightSum += weights.Left;
    }

    if (indices.HasRight)
    {
        weightedValue += map[indices.Right] * weights.Right;
        weightSum += weights.Right;
    }

    return weightSum > 0f
        ? weightedValue / weightSum
        : float.NaN;
}
```

Во внутренней выборке присутствуют все три позиции, а сумма их весов приблизительно равна `1`. На
границе конечной карты `BarycentricPartialTripletRaster` очищает отсутствующие позиции, но сохраняет
исходные веса присутствующих. Деление на `weightSum` повторно нормализует частичную окрестность и не
даёт значениям затухать к нулю у края.

Согласованные частичные растры индексов и весов вычисляют одинаковые флаги наличия, поэтому достаточно
проверять свойства `HasMain`, `HasLeft` и `HasRight` триплета индексов. Успешный `TryGetValue` означает,
что координата растра существует и в исходной карте присутствует хотя бы одна позиция. Метод возвращает
`false`, если координата находится вне растра или в выборке нет ни одного веса внутри карты.

## Когда использовать полный вариант

Создавайте `BarycentricTripletRaster` вместе с `IndexTripletRaster`, если потребителю нужны все три
позиции подразумеваемой бесконечной гексагональной сетки:

```csharp
var completeIndices = new IndexTripletRaster(mapGeometry, rasterGeometry);
var completeWeights = new BarycentricTripletRaster(mapGeometry, rasterGeometry);

Triplet<VectorXYInt> indices = completeIndices[sample];
Triplet<float> weights = completeWeights[sample];
```

Для существующей ячейки растра полный вариант всегда возвращает три веса, даже если соответствующий
индекс гекса лежит за пределами конечной исходной топологии. Его `TryGetValue` проверяет только координату
растра. Не используйте непроверенные полные индексы для чтения ограниченного `HexMap<T>`.

При создании растров вся пространственная классификация выполняется заранее. Используйте готовые растры
повторно, если значения карты меняются, а обе геометрии остаются прежними. Семантика индексов описана в
разделе [«Создать индексный Triplet-растр»](create-an-index-triplet-raster.md), а работа с границей — в
разделе [«Обработать частичные окрестности»](handle-partial-neighborhoods.md). Модель хранения приведена
в разделе [«Растры»](../../concepts/data-storage/rasters.md).
