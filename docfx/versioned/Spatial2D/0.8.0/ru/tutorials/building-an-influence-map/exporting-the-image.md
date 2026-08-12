# Экспорт изображения

Расширения из пространства имён `Akeldov.Math.Spatial2D.Imaging` сохраняют цветовые растры в
PNG. Добавьте после растеризации:

```csharp
const string outputPath = "influence-heatmap.png";
raster.SaveAsPng(outputPath);

Console.WriteLine($"Карта сохранена: {Path.GetFullPath(outputPath)}");
```

Запустите проект:

```powershell
dotnet run
```

Файл `influence-heatmap.png` появится в текущем рабочем каталоге. Он использует 16 бит на канал,
как и `RGBA16BitColor` в созданном растре.

## Полный код

Итоговый `Program.cs` выглядит так:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(12f, 12f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(88f, 14f), 25f),
    new FloatPointInfluenceSource(1f, new PointXY(18f, 58f), 50f),
    new FloatPointInfluenceSource(1f, new PointXY(83f, 54f), 75f),
    new FloatPointInfluenceSource(1f, new PointXY(50f, 34f), 100f)
};

var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var field = new FloatPointInfluenceField(sampler, sources, culler);

var grid = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(100f, 70f),
    resolution: new VectorXYInt(800, 560));

SpatialRaster<RGBA16BitColor> raster = field.RasterizeHeatMap(grid);

const string outputPath = "influence-heatmap.png";
raster.SaveAsPng(outputPath);

Console.WriteLine($"Карта сохранена: {Path.GetFullPath(outputPath)}");
```

Теперь у вас есть воспроизводимый конвейер: источники → выборка → локальное отсечение →
растеризация → PNG. Меняйте значения и положения источников, веса, стратегию выборки или
разрешение сетки, не затрагивая остальные этапы.

Подробнее о назначении полей и доступных типах источников см. в разделе
[«Поля влияния»](../../concepts/fields.md).
