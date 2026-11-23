# Разбиение прямоугольника на области Вороного

Метод `bounds.PartitionByVoronoi(sites)` строит ячейки Вороного внутри заданного прямоугольника. Для каждого сайта он возвращает <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.VoronoiCell>. Свойство `Region` задаёт всю занятую ячейкой область, а `Region.Contours` содержит её замкнутые границы. По этой геометрии можно проверить принадлежность точки, залить ячейку цветом или обвести её контуры.

## Как вес определяет границу

Если передать точки `PointXY`, у всех сайтов будет единичный вес. Каждая точка прямоугольника принадлежит ближайшему сайту, а общая граница двух ячеек проходит по серединному перпендикуляру к отрезку между сайтами. Стороны прямоугольника замыкают крайние ячейки. На рисунке точки обозначают сайты. Крайний левый сайт находится снаружи, но ближайшая к нему часть прямоугольника входит в его ячейку.

![Шесть сайтов и многоугольные ячейки; сайт слева от прямоугольника задаёт голубую ячейку у края.](~/assets/spatial2d/voronoi/bounded-exterior-site.png)

Для взвешенного разбиения передайте объекты `Site`. При конечных положительных весах точку получает сайт с наименьшим отношением расстояния к весу. Для двух разных весов линия равных отношений — окружность Аполлония. После отсечения соседними ячейками и прямоугольником от неё могут остаться дуги. На рисунке у центрального жёлтого сайта вес 1,5, у нижнего зелёного — 0,75.

![Прямые участки и дуги ограничивают цветные ячейки; у жёлтой ячейки видны изогнутые стороны.](~/assets/spatial2d/voronoi/weighted-strong-center.png)

## Ячейка с отверстием

В примере ниже два сайта лежат на одной горизонтали. Голубая ячейка с весом 1 занимает диск. Оранжевая ячейка с весом 2 занимает остальную часть прямоугольника. У неё два контура — внешний прямоугольник и окружность отверстия.

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Regions;

var bounds = new Rectangle(new PointXY(32, 16), new PointXY(336, 208));
var sites = new[]
{
    new Site(new PointXY(149, 112), weight: 1f),
    new Site(new PointXY(233, 112), weight: 2f)
};

var cells = bounds.PartitionByVoronoi(sites);
var disk = cells[0].Region!;
var surround = cells[1].Region!;

bool inDisk = disk.Contains(sites[0].Position);         // true
bool inSurround = surround.Contains(sites[0].Position); // false
int boundaryCount = surround.Contours.Count;            // 2
```

![Голубой диск целиком лежит внутри прямоугольника. Оранжевая область окружает диск.](~/assets/spatial2d/voronoi/weighted-circular-hole.png)

## Ячейка из нескольких частей

В том же прямоугольнике поместим сайты в `(249, 112)` и `(312, 112)` с весами 3 и 4. Теперь голубая ячейка пересекает и верхнюю, и нижнюю сторону прямоугольника. У оранжевой ячейки снова два контура, но на этот раз каждый ограничивает отдельную заполненную часть. Обе части принадлежат правому сайту. Метод `Region.Contains(point)` учитывает все контуры по правилу чётности.

![Два оранжевых участка у левого и правого краёв отделены друг от друга голубой ячейкой.](~/assets/spatial2d/voronoi/weighted-disconnected-cell.png)

## Как нарисовать контуры

Для примера с отверстием залейте каждый регион целиком. Затем обведите каждый контур и отметьте положения сайтов поверх заливки.

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var raster = new SpatialRaster<RGBA8BitColor>(
    new RasterGeometry(new PointXY(0, 0), new VectorXY(352, 224), new VectorXYInt(352, 224)),
    new RGBA8BitColor[352 * 224]);
var blue = new RGBA8BitColor(173, 204, 231, 255);
var orange = new RGBA8BitColor(247, 202, 157, 255);
var ink = new RGBA8BitColor(40, 53, 68, 255);

raster.Draw(disk, blue);
raster.Draw(surround, orange);
foreach (var cell in cells)
{
    foreach (var contour in cell.Region!.Contours)
        raster.Draw(contour, 1.5f, ink);
    raster.FillCircle(cell.Site, 3.5f, ink);
}
raster.SaveAsPng("voronoi.png");
```

## Пустые ячейки и релаксация

По умолчанию (`EmptyCellPolicy.LeaveAsIs`) порядок ячеек совпадает с порядком сайтов. Если внутри прямоугольника у ячейки нет площади, её `Region` равен `null`. При произвольных входных данных проверяйте его перед рисованием или вызовом `GetCentroid()`. Параметр `relaxationIterations` задаёт число шагов Ллойда. На каждом шаге сайт непустой ячейки перемещается в центроид её площади с учётом всех частей и отверстий. Вес сохраняется, а диаграмма строится заново.

Для распределения уже заданных точек или объектов по группам см. [разбиение элементов Вороного](voronoi.md).
