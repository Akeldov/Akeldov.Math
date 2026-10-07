# Растеризовать значения HexMap

Используйте `Rasterize`, чтобы выбрать одно значение гексагональной карты в центре каждой ячейки
прямоугольного растра. Селектор преобразует исходное значение в тип результата: цвет изображения,
число или другую структуру для последующей обработки.

## Растеризовать пространственную карту

Создайте <xref:Akeldov.Math.Hexes.SpatialHexMap`1>, если нужно сохранить радиус и положение карты в
мировых координатах. Следующий пример преобразует три класса местности в цвета RGBA:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

var terrain = new SpatialHexMap<int>(geometry, new[]
{
    0, 0, 1, 1,
    0, 1, 1, 2,
    1, 1, 2, 2
});

SpatialRaster<RGBA8BitColor> raster = terrain.Rasterize(
    pixelsPerApothem: 24f,
    margin: geometry.Radius,
    colorSelector: TerrainColor);

static RGBA8BitColor TerrainColor(int terrainType)
{
    return terrainType switch
    {
        0 => new RGBA8BitColor(40, 104, 216, byte.MaxValue),
        1 => new RGBA8BitColor(50, 160, 72, byte.MaxValue),
        2 => new RGBA8BitColor(136, 136, 136, byte.MaxValue),
        _ => new RGBA8BitColor(224, 48, 48, byte.MaxValue)
    };
}
```

`pixelsPerApothem` задаёт плотность выборки. `margin` добавляет указанное число мировых единиц с каждой
стороны ограничивающего прямоугольника карты. Здесь поле равно радиусу гекса, поэтому результат содержит
ячейки за пределами конечной топологии.

Для каждой выходной ячейки `Rasterize` берёт её центральную точку, находит содержащий её гекс, читает
значение гекса и вызывает `colorSelector`. Ячейки, центры которых находятся вне исходной топологии,
сохраняют `default(RGBA8BitColor)` с нулевым альфа-каналом, поэтому внешнее поле прозрачно. Для таких
ячеек селектор не вызывается.

## Повторно использовать точную сетку выборки

Передайте явную `RasterGeometry`, если результат должен совпадать с другим пространственным растром
ячейка в ячейку:

```csharp
RasterGeometry rasterGeometry = terrain.ToRasterGeometry(
    pixelsPerApothem: 24f,
    margin: geometry.Radius);

SpatialRaster<RGBA8BitColor> raster = terrain.Rasterize(
    rasterGeometry,
    TerrainColor);
```

Результат сохраняет всю переданную геометрию: начало координат, размер в пространстве и разрешение.
Пользовательская сетка может покрывать всю карту, вырезать её часть или выходить за её границы.
Используйте один экземпляр геометрии для всех слоёв, которым нужны общие координаты пикселей.

## Растеризовать логическую карту

<xref:Akeldov.Math.Hexes.IHexMap`1> содержит топологию и значения, но не геометрию в мировом пространстве.
Передайте его перегрузке требуемое разрешение в пикселях:

```csharp
var values = new HexMap<int>(
    new HexMapTopology(2, 2, Layout.OddR),
    new[]
    {
        10, 20,
        30, 40
    });

Raster<byte> preview = values.Rasterize(
    resolution: new VectorXYInt(320, 240),
    colorSelector: value => (byte)(value * 5));
```

Этот вариант временно размещает топологию со стандартным началом координат и единичным радиусом,
а затем вписывает её в заданное разрешение. Он возвращает `Raster<T>`, а не `SpatialRaster<T>`, поэтому
результат не имеет положения в мировом пространстве. Используйте его для логического предпросмотра;
если важны начало координат, радиус или размер ячейки, выберите пространственную карту.

## Выбрать прямую выборку или интерполяцию

При прямой растеризации каждой ячейке растра назначается ровно один содержащий её гекс. Метод не смешивает
соседние значения и не вычисляет площадь покрытия пикселя, поэтому на рёбрах гексов значения меняются
ступенчато.

Если выборка должна смешивать значения из трёх ближайших центров гексов, объедините индексный и
барицентрический растры. См. [«Создать барицентрический растр»](create-a-barycentric-raster.md).
Чтобы записать цветной растр в файл, перейдите к разделу
[«Преобразовать растр в изображение»](convert-a-raster-to-an-image.md). Полная модель выборки описана
в разделе [«Растеризация»](../../concepts/rasterization.md).
