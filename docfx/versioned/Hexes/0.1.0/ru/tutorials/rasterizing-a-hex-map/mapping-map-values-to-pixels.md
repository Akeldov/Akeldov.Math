# Перенос значений карты в пиксели

Объедините каждую тройку индексов с её барицентрическими весами. Внутренние пиксели смешивают три
высоты, а граничные — только позиции, оставшиеся внутри конечной карты.

## Создание растра высот

Добавьте следующий код после создания обоих служебных растров:

```csharp
var elevationValues = new float[
    checked(rasterGeometry.Resolution.X * rasterGeometry.Resolution.Y)];

for (int i = 0; i < elevationValues.Length; i++)
{
    elevationValues[i] = InterpolateElevation(
        elevationMap,
        indexRaster[i],
        barycentricRaster[i]);
}

var elevationRaster = new SpatialRaster<float>(
    rasterGeometry,
    elevationValues);

static float InterpolateElevation(
    HexMap<float> map,
    PartialTriplet<VectorXYInt> cells,
    PartialTriplet<float> weights)
{
    float weightedValue = 0f;
    float weightSum = 0f;

    if (cells.HasMain)
    {
        weightedValue += map[cells.Main] * weights.Main;
        weightSum += weights.Main;
    }

    if (cells.HasLeft)
    {
        weightedValue += map[cells.Left] * weights.Left;
        weightSum += weights.Left;
    }

    if (cells.HasRight)
    {
        weightedValue += map[cells.Right] * weights.Right;
        weightSum += weights.Right;
    }

    return weightSum > 0f
        ? weightedValue / weightSum
        : float.NaN;
}
```

Деление на `weightSum` повторно нормализует частичную окрестность. Поэтому у внешнего края
значения остаются в исходном диапазоне, а не затухают к нулю. Выборка без присутствующих ячеек
становится `float.NaN`; здесь такой маркер безопасен, поскольку исходные высоты конечны и
нормализованы.

`SpatialRaster<float>` сохраняет `rasterGeometry`, поэтому последующие преобразования не меняют
его размещение в пространстве и разрешение. Массив значений создаётся заново и не зависит ни от
служебных растров, ни от исходной карты.

Перейдите к [преобразованию значений в цвета](mapping-values-to-colors.md).
