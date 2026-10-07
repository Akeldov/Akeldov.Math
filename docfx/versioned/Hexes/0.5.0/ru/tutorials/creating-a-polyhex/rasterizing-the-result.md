# Растеризация результата

В заключительной части руководства вы вычислите знаковое расстояние до региона на регулярной
сетке. Пиксели внутри полигекса станут белыми, а фон и отверстие останутся чёрными.

## Создание и сохранение растра

Добавьте в начало `Program.cs` пространства имён:

```csharp
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
```

Затем добавьте после создания `region` следующий код:

```csharp
static Gray8BitColor ToMaskColor(float signedDistance) =>
    signedDistance <= 0f ? Gray8BitColor.White : Gray8BitColor.Black;

var rasterGeometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(9f, 7f),
    resolution: new VectorXYInt(720, 560));

SpatialRaster<Gray8BitColor> raster =
    region.Rasterize(ToMaskColor, rasterGeometry);

string outputPath = Path.GetFullPath("polyhex.png");
raster.SaveAsPng(outputPath);

Console.WriteLine(
    $"Растр: {raster.Resolution.X} x {raster.Resolution.Y}");
Console.WriteLine($"Сохранено: {outputPath}");
```

В конце выводится:

```text
Растр: 720 x 560
Сохранено: <каталог проекта>\polyhex.png
```

<xref:Akeldov.Math.Spatial2D.Rasterization.RasterGeometry> отображает прямоугольник размером
`9` на `7` единиц координатного пространства в 720 на 560 пикселей. `Rasterize` передаёт знаковое
расстояние каждого пикселя в `ToMaskColor`: неположительное значение находится внутри региона,
а положительное — снаружи или в отверстии.

Откройте `polyhex.png`: на изображении будет белая фигура из 11 гексов с чёрным центральным
отверстием. Для масок произвольного размера вычисляйте сетку по границам созданных контуров, а не
используйте известные размеры этого примера.

Теперь у вас есть неизменяемый полигекс, геометрия отдельной ячейки, заполненный регион Spatial2D
и его PNG-представление. Подробнее о построителях, расширении и контурных масках, владении и
проверках читайте в разделе [Полигексы](../../concepts/hex-grid-model/polyhexes.md).
