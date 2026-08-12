# Генерация точек Пуассона

Случайно выбранные точки часто образуют скопления. Сэмплер дисков Пуассона предотвращает их,
сохраняя минимальное расстояние между принятыми точками. Такой набор хорошо подходит для
начального расположения процедурных областей.

Добавьте в `Program.cs` следующий код:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Partitioning.Voronoi;
using Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk;

var fieldSize = new VectorXY(120f, 80f);
var pointSampler = new PoissonDiskPointSampler(
    new Random(12345),
    maxAttempts: 30);

var samples = pointSampler.Sample(fieldSize, minimalDistance: 14f);
var sites = samples
    .Select(sample => new Site(sample.Point, weight: 1f))
    .ToArray();

Console.WriteLine($"Создано центров: {sites.Length}");
```

<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSampler> начинает со
случайной точки и проверяет до `maxAttempts` кандидатов вокруг каждого активного образца. Большее
число попыток позволяет тщательнее заполнять промежутки, но требует дополнительных вычислений.

Фиксированное начальное значение `Random` делает расположение воспроизводимым. Меняйте его, когда
нужна другая карта, и сохраняйте прежним при отладке или тестировании генерации.

`Sample` возвращает новый изменяемый список, принадлежащий вызывающему коду. Каждый
<xref:Akeldov.Math.Spatial2D.Sampling.Point.PoissonDisk.PoissonDiskPointSample> хранит точку и
минимальное расстояние, с которым она была принята. Здесь каждый образец превращается в
равновзвешенный <xref:Akeldov.Math.Spatial2D.Partitioning.Voronoi.Site>.

Переходите к разделу [«Создание областей Вороного»](creating-voronoi-cells.md).
