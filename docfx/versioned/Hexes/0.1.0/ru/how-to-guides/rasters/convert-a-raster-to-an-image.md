# Преобразовать растр в изображение

Преобразуйте числовой или категориальный растр в поддерживаемый цветовой тип Spatial2D, а затем
закодируйте его с помощью `SaveAsPng` или `SaveAsBmp`. Экспорт поддерживает как обычный `Raster<T>`,
так и содержащий геометрию `SpatialRaster<T>`.

## Преобразовать значения в цвета

Следующий пример преобразует нормализованные высоты в цветовую карту RGBA. Значение `float.NaN`
обозначает выборку за пределами конечной гексагональной карты и становится прозрачным пикселем:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var rasterGeometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(3f, 2f),
    resolution: new VectorXYInt(3, 2));

var elevationRaster = new SpatialRaster<float>(
    rasterGeometry,
    new[]
    {
        0f, 0.25f, 0.5f,
        float.NaN, 0.75f, 1f
    });

SpatialRaster<RGBA8BitColor> imageRaster =
    elevationRaster.MapValues(ToColor);

static RGBA8BitColor ToColor(float elevation)
{
    return float.IsNaN(elevation)
        ? new RGBA8BitColor(0, 0, 0, 0)
        : RGBA8BitColor.FromTemperature(elevation);
}
```

`MapValues` вызывает селектор один раз для каждой ячейки и создаёт новый массив значений. Если исходный
растр пространственный, результат сохраняет ту же `RasterGeometry`; обычный `Raster<T>` преобразуется
в другой обычный растр с прежним разрешением.

`FromTemperature` ожидает нормализованное значение и ограничивает конечные значения диапазоном от `0`
до `1`. Обрабатывайте маркеры вроде `NaN` отдельно до передачи в палитру. Используйте тип RGBA, если
выборки за пределами карты должны быть прозрачными: серые типы не содержат альфа-канала.

Если `HexMap.Rasterize` уже возвращает `Gray8BitColor`, `Gray16BitColor`, `RGBA8BitColor` или
`RGBA16BitColor`, пропустите `MapValues` и экспортируйте полученный растр напрямую.

## Сохранить файл PNG

Подключите пространство имён для работы с файлами, создайте каталог назначения и вызовите `SaveAsPng`:

```csharp
using System.IO;

string outputDirectory = Path.GetFullPath("output");
Directory.CreateDirectory(outputDirectory);

string outputPath = Path.Combine(outputDirectory, "hex-map.png");
imageRaster.SaveAsPng(outputPath);

Console.WriteLine(
    $"Сохранено {imageRaster.Resolution.X} x {imageRaster.Resolution.Y} пикселей: {outputPath}");
```

Перегрузка с путём создаёт или перезаписывает файл, но родительский каталог должен уже существовать.
При кодировании используются только разрешение растра и значения его ячеек. `SpatialRaster<T>` сохраняет
мировую `Geometry` в памяти, однако файлы PNG и BMP эту геометрию не содержат.

Кодировщик сам учитывает порядок строк растра при записи строк изображения; переворачивать их вручную
не нужно.

## Записать изображение в поток

Используйте перегрузку с потоком, если изображение нужно вернуть из веб-обработчика или добавить в архив:

```csharp
using var output = new MemoryStream();
imageRaster.SaveAsPng(output);

byte[] pngBytes = output.ToArray();
```

Поток принадлежит вызывающему коду, который решает, когда его освободить. Поток должен поддерживать
запись. Такая перегрузка доступна для всех поддерживаемых PNG-типов цвета.

## Выбрать формат и точность

| Тип ячейки растра | PNG | BMP | Типичная задача |
|---|---:|---:|---|
| `Gray8BitColor` | Да | Да | Маски и компактные серые предпросмотры |
| `Gray16BitColor` | Да | Нет | Высоты, расстояния и другие точные скалярные данные |
| `RGBA8BitColor` | Да | Да | Обычные цветные изображения с прозрачностью |
| `RGBA16BitColor` | Да | Нет | Высокоточные градиенты и композиция |

Вызовите `SaveAsBmp` вместо `SaveAsPng`, если нужен несжатый 8-битный BMP. Обобщённый числовой растр,
например `Raster<float>`, нельзя экспортировать напрямую: сначала задайте преобразование каждого значения
в один из поддерживаемых цветовых типов.

Создание исходного растра показано в разделе
[«Растеризовать значения HexMap»](rasterize-hex-map-values.md), а плавная интерполяция между центрами
гексов — в разделе [«Создать барицентрический растр»](create-a-barycentric-raster.md). Общая модель
растров описана в разделе [«Растеризация»](../../concepts/rasterization.md).
