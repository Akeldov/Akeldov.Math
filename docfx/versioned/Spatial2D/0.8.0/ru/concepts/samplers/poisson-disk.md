# Семплер Пуассона

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> создаёт
нерегулярный набор точек с управляемым минимальным расстоянием.

## Генерация точек дисками Пуассона

Выборка дисков Пуассона заполняет прямоугольное поле, соблюдая минимальное расстояние между
принятыми точками. Результат нерегулярен, но в нём нет тесных скоплений и больших случайных
пробелов, характерных для независимой равномерной случайной выборки.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var sampler = new PoissonDiskPointSampler(
    random: new Random(12345),
    maxAttempts: 30);

var fieldSize = new VectorXY(100f, 60f);

List<PoissonDiskPointSample> samples = sampler.Sample(
    fieldSize,
    minimalDistance: 6f);

PointXY firstPoint = samples[0].Point;
float firstSpacing = samples[0].MinimalDistance;
```

Точки лежат в полуоткрытом прямоугольнике от `(0, 0)` включительно до `fieldSize` исключительно.
У выборщика нет параметра начала координат; сместите возвращённые точки, если целевой мировой
прямоугольник начинается в другом месте.

`maxAttempts` ограничивает количество кандидатов, проверяемых около каждой активной точки.
Большее значение может дать более плотный результат ценой дополнительных вычислений. Передача
`Random` с фиксированным зерном делает запуски повторяемыми в контролируемом окружении, что
удобно для тестов и процедурной генерации.

Возвращаемый `List<PoissonDiskPointSample>` является новым изменяемым списком и принадлежит
вызывающему коду.

## Изменение расстояния с помощью поля

Минимальное расстояние может поступать из любого `IFloatField`. Это позволяет создавать области
с разной плотностью в одном наборе точек:

```csharp
using Akeldov.Math.Spatial2D.Fields;

var spacingField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(0f, 0f),
            value: 4f),
        new FloatPointInfluenceSource(
            weight: 1f,
            position: new PointXY(fieldSize.X, 0f),
            value: 12f)
    });

List<PoissonDiskPointSample> adaptiveSamples =
    sampler.Sample(fieldSize, spacingField);
```

`Min`, `Max` поля и каждое выбранное значение должны быть конечными и положительными. Каждая
принятая точка хранит расстояние, запрошенное в её положении. Для любой пары фактическое
расстояние не меньше большего из двух сохранённых минимальных расстояний, поэтому точку с
большим интервалом нельзя окружить соседями с малым интервалом.

## См. также

- [Семплеры](index.md) — обзор генерации наборов точек.
- [Сгенерировать точки Пуассона](../../how-to-guides/sampling/generate-poisson-disk-points.md) —
  практическое руководство, включая визуализацию и экспорт PNG.
- [Поля](../fields.md) — источники значений для адаптивного расстояния.
- [Разбиение элементов Вороного](../partitioning/voronoi.md) — назначение объектов сгенерированным сайтам.
- [Процедурное разбиение пространства](../../tutorials/procedural-space-partitioning/index.md) —
  совместное использование генерации сайтов, разбиения и визуализации.
