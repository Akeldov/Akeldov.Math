# Семплер Пуассона

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> генерирует точки
в прямоугольнике с заданным минимальным расстоянием между соседями. Вы задаёте расстояние,
а число точек зависит от того, сколько кандидатов примет семплер.

## Постоянное минимальное расстояние

В примере точки располагаются на расстоянии не менее 9 мировых единиц друг от друга
в прямоугольнике шириной 120 и высотой 80 единиц.

```csharp
using System;
using System.Collections.Generic;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var sampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);
List<PoissonDiskPointSample> samples = sampler.Sample(fieldSize, minimalDistance: 9f);
```

![Точки выборки Пуассона с окружностями одинакового радиуса вокруг каждой точки](~/assets/spatial2d/samplers/poisson-constant-distance.svg)

Точка на рисунке соответствует свойству `Point`, а радиус серой окружности вокруг неё равен
`MinimalDistance`. Окружности могут пересекаться, но расстояние между их центрами всегда
составляет не менее 9 единиц.

Оба размера `fieldSize` и минимальное расстояние должны быть конечными и положительными.
Координаты точек в примере лежат в пределах `0 ≤ X < 120` и `0 ≤ Y < 80`. Чтобы перенести
выборку в другую часть сцены, прибавьте нужное смещение к каждой точке. Каждый вызов возвращает
новый изменяемый список `List<PoissonDiskPointSample>`, принадлежащий вызывающему коду.

Параметр `maxAttempts` задаёт число попыток подобрать соседа вокруг активной точки.
Если все попытки неудачны, семплер исключает точку из дальнейшего поиска. Увеличение лимита
может заполнить больше промежутков, но требует дополнительных вычислений.

## Расстояние, заданное полем

Передайте <xref:Akeldov.Math.Spatial2D.Fields.IFloatField>, чтобы расстояние зависело от положения
точки. В примере оно растёт от 5 единиц у левого края до 13 у правого. Для сравнения двух
выборок создайте новый семплер с тем же начальным значением генератора.

```csharp
var spacingField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    new[]
    {
        new FloatPointInfluenceSource(weight: 1f, position: new PointXY(0f, 0f), value: 5f),
        new FloatPointInfluenceSource(weight: 1f, position: new PointXY(fieldSize.X, 0f), value: 13f)
    });

var adaptiveSampler = new PoissonDiskPointSampler(new Random(12345), maxAttempts: 30);
List<PoissonDiskPointSample> adaptiveSamples = adaptiveSampler.Sample(fieldSize, spacingField);
```

![Слева точки расположены плотнее, а к правому краю радиусы окружностей растут](~/assets/spatial2d/samplers/poisson-variable-distance.svg)

Для каждой пары точек минимальное расстояние между ними равно большему из двух значений
`MinimalDistance`. Границы поля должны быть конечными и положительными, причём `Min ≤ Max`.
Все значения поля должны лежать в этих пределах.

## См. также

- [Семплеры](index.md) — обзор генерации наборов точек.
- [Сгенерировать точки Пуассона](../../how-to-guides/sampling/generate-poisson-disk-points.md) —
  практическое руководство, включая визуализацию и экспорт PNG.
- [Поля](../fields.md) — источники значений для адаптивного расстояния.
- [Разбиение элементов Вороного](../partitioning/voronoi.md) — назначение объектов сгенерированным сайтам.
- [Процедурное разбиение пространства](../../tutorials/procedural-space-partitioning/index.md) —
  совместное использование генерации сайтов, разбиения и визуализации.
