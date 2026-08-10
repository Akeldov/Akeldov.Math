# Построить карту влияния

Поле влияния с вещественными значениями подходит для интерполяции значений, привязанных к
точкам двумерного пространства. В этом руководстве мы построим тепловую карту по трём источникам
и сохраним её в PNG.

## Создайте источники влияния

У каждого <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource> есть вес, положение в
мировых координатах и значение. Положения должны быть конечными и различными. Конечный положительный вес
подходит для любого семплера вещественных значений.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
};
```

Последний аргумент — скалярное значение источника. Диапазон поля определяется этими значениями,
поэтому в примере он равен от `0` до `100`.

## Постройте поле

Выберите семплер в зависимости от требуемого перехода между источниками:

| Семплер | Когда использовать |
| --- | --- |
| <xref:Akeldov.Math.Spatial2D.Fields.NearestFloatInfluenceSampler`1> | Каждая точка должна принимать значение ближайшего источника, образуя резкие границы. |
| <xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> | Все выбранные источники должны давать плавную смесь с весом, обратно пропорциональным расстоянию. Веса источников должны быть положительными. |
| <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1> | Значения должны изменяться линейно внутри локальных треугольников источников. |

Для барицентрической интерполяции <xref:Akeldov.Math.Spatial2D.Fields.DelaunayCuller`1> перед
каждым вычислением выбирает содержащий точку треугольник Делоне:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);
```

`DelaunayCuller` требует не менее трёх источников с уникальными положениями. Вне области
триангуляции он выбирает ближайшую вершину или ребро выпуклой оболочки. Не передавайте culler в
конструктор поля, если семплер должен учитывать все источники.

Поле можно вычислять напрямую в мировых координатах:

```csharp
float value = field.Sample(new PointXY(50f, 32f));
```

<xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceField> ограничивает вычисленное значение
включительным диапазоном от `field.Min` до `field.Max`, полученным из значений источников.

## Растеризуйте и сохраните карту

Задайте прямоугольник в мировых координатах и разрешение в пикселях с помощью
<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry>. При растеризации поле вычисляется в
центре каждой ячейки.

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    new PointXY(0f, 0f),
    new VectorXY(100f, 64f),
    new VectorXYInt(160, 96));

SpatialRaster<RGBA16BitColor> heatMap = field.RasterizeHeatMap(geometry);
heatMap.SaveAsPng("influence-heatmap.png");
```

Растеризатор тепловой карты сопоставляет `field.Min` холодному краю цветовой шкалы, а
`field.Max` — горячему. `SaveAsPng` записывает `influence-heatmap.png` относительно рабочей
директории приложения.

## Полный пример

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(18f, 14f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(82f, 16f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 52f), 50f)
};

var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);

var geometry = new RasterGeometry(
    new PointXY(0f, 0f),
    new VectorXY(100f, 64f),
    new VectorXYInt(160, 96));

SpatialRaster<RGBA16BitColor> heatMap = field.RasterizeHeatMap(geometry);
heatMap.SaveAsPng("influence-heatmap.png");
```

Описание устройства полей и других вариантов растеризации см. в разделах
[Поля](../../concepts/fields.md) и [Растеризация](../../concepts/rasterization.md).
