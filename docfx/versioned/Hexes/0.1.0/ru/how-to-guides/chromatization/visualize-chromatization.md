# Визуализировать хроматизацию

Отобразите `ChromaticIndexMap` отдельным цветом для каждого класса, чтобы проверить трёхцветный узор.
Для проверки интерполяции около вершин гексов перенесите упорядоченные по классам барицентрические
веса в красный, зелёный и синий каналы.

## Отобразить карту классов

Создайте пространственную хроматическую карту, растеризуйте её трёхцветной палитрой и сохраните
результат в PNG:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;
using System.IO;

var geometry = new HexMapGeometry(
    width: 5,
    height: 4,
    origin: VectorXY.Zero,
    radius: 1f,
    layout: Layout.OddR);

var chromaticMap = new ChromaticIndexMap(geometry);

SpatialRaster<RGBA8BitColor> classImage = chromaticMap.Rasterize(
    pixelsPerApothem: 24f,
    margin: geometry.Radius * 0.5f,
    colorSelector: ToClassColor);

string classImagePath = Path.GetFullPath("chromatic-classes.png");
classImage.SaveAsPng(classImagePath);

static RGBA8BitColor ToClassColor(byte chromaticIndex)
{
    return chromaticIndex switch
    {
        0 => new RGBA8BitColor(239, 71, 71, byte.MaxValue),
        1 => new RGBA8BitColor(59, 201, 114, byte.MaxValue),
        2 => new RGBA8BitColor(71, 119, 232, byte.MaxValue),
        _ => new RGBA8BitColor(0, 0, 0, 0)
    };
}
```

`Rasterize` выбирает содержащий гекс по центру каждой выходной ячейки и передаёт его класс в
`ToClassColor`. Поэтому результат имеет резкие границы между гексами. Ячейки внешнего поля, которые
оказались вне конечной топологии, сохраняют `default(RGBA8BitColor)` и остаются прозрачными.

Повторите пример с `EvenR`, `OddQ` или `EvenQ`, чтобы увидеть влияние смещённой раскладки на узор.
Трёхцветный инвариант сохраняется: ячейки с общим ребром имеют разные цвета.

## Визуализировать веса в порядке классов

Используйте <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricPartialTripletRaster>, чтобы показать
плавные веса около вершин. Сопоставьте класс `0` красному, класс `1` зелёному, а класс `2` синему:

```csharp
RasterGeometry rasterGeometry = geometry.ToRasterGeometry(
    pixelsPerApothem: 24f,
    margin: geometry.Radius * 0.5f);

var weightRaster = new ChromaticBarycentricPartialTripletRaster(
    geometry,
    rasterGeometry);

SpatialRaster<RGBA8BitColor> weightImage =
    weightRaster.MapValues(ToWeightColor);

string weightImagePath = Path.GetFullPath("chromatic-weights.png");
weightImage.SaveAsPng(weightImagePath);

static RGBA8BitColor ToWeightColor(
    PartialChromaticTriplet<float> weights)
{
    float red = weights.HasIndex0 ? weights.Index0 : 0f;
    float green = weights.HasIndex1 ? weights.Index1 : 0f;
    float blue = weights.HasIndex2 ? weights.Index2 : 0f;
    float sum = red + green + blue;

    return sum > 0f
        ? RGBA8BitColor.FromNormalized(
            red / sum,
            green / sum,
            blue / sum,
            alpha: 1f)
        : new RGBA8BitColor(0, 0, 0, 0);
}
```

`Index0`, `Index1` и `Index2` уже переставлены по хроматическим классам, поэтому один цветовой канал
представляет один класс во всём изображении. Деление на `sum` повторно нормализует граничные выборки,
у которых присутствует только часть соседних гексов. Выборка без единого класса внутри карты становится
прозрачной.

`MapValues` создаёт новый цветной растр, сохраняя `rasterGeometry`. Изображение весов — это
диагностическое представление основы интерполяции, а не отображение значений, хранящихся в
гексагональной карте.

## Интерпретировать результат

- Используйте `chromatic-classes.png` для проверки назначения классов и раскладки.
- Используйте `chromatic-weights.png` для проверки плавных каналов классов и границ конечной карты.
- Увеличьте `pixelsPerApothem` для более детального результата без изменения исходной геометрии.
- Сохраняйте одну палитру при сравнении раскладок или версий.

Исходные данные описаны в разделах [«Создать хроматическую карту»](create-a-chromatic-map.md) и
[«Создать хроматический растр»](create-a-chromatic-raster.md). Общие варианты экспорта приведены в
разделе [«Преобразовать растр в изображение»](../rasters/convert-a-raster-to-an-image.md).
