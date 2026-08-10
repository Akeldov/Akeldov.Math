# Сгенерировать точки Пуассона

Выборка дисков Пуассона создаёт нерегулярный набор точек без тесных скоплений. Выборщик заполняет
прямоугольную область, сохраняя между принятыми точками не меньше заданного расстояния.

## Сгенерируйте точки с постоянным интервалом

Создайте <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler>, передав
генератор случайных чисел и максимальное количество кандидатов около каждой активной точки.
Используйте фиксированное зерно, когда результат должен воспроизводиться в контролируемой среде.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var sampler = new PoissonDiskPointSampler(
    random: new Random(12345),
    maxAttempts: 30);

var fieldSize = new VectorXY(120f, 80f);

List<PoissonDiskPointSample> samples = sampler.Sample(
    fieldSize,
    minimalDistance: 9f);
```

Каждый <xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSample> содержит
свойства `Point` и `MinimalDistance` — положение точки и интервал, использованный при её принятии.
Точки лежат в полуоткрытом прямоугольнике от `(0, 0)` включительно до `fieldSize` исключительно.

Возвращаемый `List<PoissonDiskPointSample>` является новым изменяемым списком и принадлежит
вызывающему коду. Его можно фильтровать, дополнять, преобразовывать и повторно использовать без
изменения состояния выборщика.

## Настройте плотность и стоимость

Два основных параметра влияют на результат по-разному:

| Параметр | Влияние |
| --- | --- |
| `minimalDistance` | Большее значение создаёт меньше точек с большими интервалами. Оно должно быть конечным и положительным. |
| `maxAttempts` | Большее значение может плотнее заполнить оставшиеся пробелы, но проверяет больше кандидатов и требует больше вычислений. Оно должно быть положительным. |

Передавайте `Random` с фиксированным зерном для тестов, сохраняемых процедурных миров и других
сценариев с контролируемой воспроизводимостью. Используйте принадлежащий приложению случайный
источник, если при каждом запуске нужна новая конфигурация.

## Изменяйте интервал по области

Передайте <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> вместо постоянного расстояния, если
одни части области должны быть плотнее других. В этом примере интервал изменяется от `5` около
левой стороны до `13` около правой:

```csharp
using Akeldov.Math.Spatial2D.Fields;

var spacingField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(0f, 0f),
            value: 5f),
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(fieldSize.X, 0f),
            value: 13f)
    });

List<PoissonDiskPointSample> adaptiveSamples =
    sampler.Sample(fieldSize, spacingField);
```

`Min`, `Max` поля и каждое вычисленное значение должны быть конечными и положительными. Каждая
принятая точка хранит расстояние, запрошенное в её положении. Для любой пары принятых точек
фактическое расстояние не меньше большего из двух сохранённых минимальных расстояний.

## Визуализируйте результат

Встроенный растеризатор колец рисует каждую принятую точку и окружность её минимального
расстояния. Используйте для геометрии растра тот же размер в мировых координатах, что и для
выборки:

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: fieldSize,
    resolution: new VectorXYInt(600, 400));

var rasterizer = new PoissonDiskPointSampleCollectionRingsGray16BitRasterizer(
    pointRadius: 1.2f,
    ringThickness: 0.2f,
    backgroundGrayLevel: Gray16BitColor.Black,
    ringGrayLevel: new Gray16BitColor(0x6000),
    pointGrayLevel: Gray16BitColor.White);

var raster = rasterizer.Rasterize(samples, geometry);
raster.SaveAsPng("poisson-disk-points.png");
```

`SaveAsPng` записывает изображение относительно рабочей директории приложения.

## Используйте другое начало координат

Выборщик всегда создаёт координаты относительно `(0, 0)` и не принимает начало области. Если
целевой прямоугольник начинается в другом месте, прибавляйте его смещение к каждой точке при
использовании или копировании результата. Не изменяйте `MinimalDistance`: перенос не влияет на
интервалы.

Инварианты алгоритма и его связь с другими пространственными алгоритмами описаны в разделе
[Пространственные алгоритмы](../../concepts/spatial-algorithms.md). Другие способы преобразования
пространственных данных в изображения см. в разделе
[Растеризация](../../concepts/rasterization.md).
