# Рисование фигур и текста на растре

Методы <xref:Akeldov.Math.Spatial2D.Imaging.SpatialRasterDrawingExtensions> изменяют существующий
SpatialRaster&lt;RGBA8BitColor&gt;. Координаты мировые, ось Y направлена вверх.
Толщина, радиусы, длины штрихов, шаг сетки, размеры наконечника и высота текста задаются
числами float в мировых единицах.

## Создание и рисование

```csharp
using System;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Curves;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    new PointXY(0, 0), new VectorXY(80, 50), new VectorXYInt(800, 500));
var pixels = new RGBA8BitColor[800 * 500];
Array.Fill(pixels, RGBA8BitColor.White);
var raster = new SpatialRaster<RGBA8BitColor>(geometry, pixels);
var blue = new RGBA8BitColor(37, 99, 235, 220);

raster.DrawGrid(new VectorXY(10, 10), width: 0.1f,
    color: new RGBA8BitColor(30, 41, 59, 35),
    majorEvery: 2, majorColor: new RGBA8BitColor(30, 41, 59, 90),
    labelHeight: 1f);
raster.FillCircle(new PointXY(20, 25), 8f, blue);
raster.FillRectangle(new PointXY(45, 15), new PointXY(65, 30), blue.Lighten(0.4f));
raster.DrawArrow(
    new ParameterizedSegment(new PointXY(10, 8), new PointXY(70, 8)),
    width: 0.3f, headLength: 3f, headWidth: 2f, dash: 2f,
    color: blue.Darken(0.3f));
raster.DrawText("Spatial2D 1.5", origin: new PointXY(10, 40), height: 3f);
raster.SaveAsPng("drawing.png");
```

Растр сохраняет переданный массив пикселей. Методы изменяют его на месте в порядке вызовов.
Рисование ограничено границами растра; прозрачность смешивается по правилу source-over
без гамма-коррекции.

## Выбор операции

| Операция | API |
| --- | --- |
| Сплошной отрезок | DrawSegment(start, end, width, color) или Draw(Segment, width, color) |
| Пунктирный отрезок | DrawDashedSegment(start, end, width, dash, color) или DrawDashed(Segment, width, dash, color) |
| Другие кривые | Draw(curve, width, color), включая параметризованные через ICurve |
| Замкнутый контур | Draw(contour, width, color) или DrawContour(points, width, color) |
| Пунктирный контур | DrawDashed(contour, width, color, dash) или DrawContour(points, width, color, dash) |
| Заливка круга или прямоугольника | Fill(shape, color), FillCircle(center, radius, color), FillRectangle(cornerA, cornerB, color) |
| Стрелка | DrawArrow(start, end, width, headLength, headWidth, color) или перегрузка с ParameterizedSegment |
| Пунктирный стержень стрелки | Параметр dash перед color в обеих перегрузках DrawArrow; ноль означает сплошной стержень |
| Координатная сетка | DrawGrid(step, width, color); перегрузка с VectorXY также принимает настройки основных линий, подписей и bounds |

Кривые, стрелки, сетки, круги и маски глифов используют покрытие по сетке 4×4.
Для заливки осевого прямоугольника вычисляется точная площадь пересечения с ячейкой.
Концы штрихов закруглены.

Номинальная длина штриха равна длине промежутка. Закругления выступают в промежутки, поэтому
толстые штрихи могут соединиться. У пунктирных отрезков каждый штрих компонуется отдельно;
стрелки и контуры объединяют покрытие перед наложением цвета. Фаза пунктира контура
продолжается через границы рёбер.

headLength — осевая длина наконечника от вершины до основания; headWidth — полный поперечный
размах без толщины линии. Наконечник всегда сплошной; на короткой стрелке размеры сохраняются.
При совпадающих концах ничего не рисуется.

Линии сетки проходят через кратные шагу мировые координаты, включая отрицательные.
Основные линии имеют приоритет на пересечениях. Подписи используют встроенный шрифт
и формат чисел, не зависящий от локали; пересекающиеся и не помещающиеся подписи пропускаются.
Прямоугольник bounds ограничивает и линии, и подписи.

## Выбор и повторное использование шрифта

DrawText(text, origin, height) использует чёрный цвет и заранее подготовленный
<xref:Akeldov.Math.Spatial2D.Imaging.GlyphSet.Default>. Перегрузка с color задаёт другой цвет.
origin — начало первой базовой линии, а не верхний левый угол.

Настраиваемый пиксельный шрифт и загрузка TrueType:

```csharp
var pixelGlyphs = new GlyphSet(new PixelFont(advance: 7, lineGap: 2), pixelHeight: 28);
raster.DrawText("Привет, Spatial2D!", pixelGlyphs, new PointXY(10, 35), 2f, blue);

var glyphs = new GlyphSet(TrueTypeFont.Load("MyFont.ttf"), pixelHeight: 48);
raster.DrawText("Cached text", glyphs, new PointXY(10, 30), 3f, blue);
var textBounds = glyphs.MeasureText("Cached text", height: 3f);
```

Файл TrueType нужно предоставить самостоятельно. Повторно используйте GlyphSet между
рисованиями, чтобы не готовить одни и те же глифы заново. Маски TrueType создаются лениво;
стандартный пиксельный набор уже подготовлен. Изменяемые кэши TrueType не предназначены
для одновременного доступа из нескольких потоков.

MeasureText возвращает логические границы относительно базовой линии, включая пробелы
и метрики строк, а не точные границы видимых штрихов. Поддерживаются переносы CR, LF и CRLF.
Нет сложного шейпинга, двунаправленной раскладки или разворачивания табуляции.
Неизвестные символы пиксельного шрифта заменяются на '?'.

## Изменение цвета

```csharp
var border = blue.Darken(0.3f);
var light = blue.Lighten(0.4f, ColorAdjustmentMode.LinearRgb);
var tint = blue.Lighten(0.4f, ColorAdjustmentMode.Oklch);
```

Методы возвращают новый RGBA8BitColor, сохраняя альфу. Входные и выходные RGB-каналы
интерпретируются как sRGB. amount должен быть конечным числом в [0, 1];
неверное значение или режим вызывает ArgumentOutOfRangeException.
Ноль сохраняет цвет; единица даёт чёрный или белый с исходной альфой.

Srgb изменяет закодированные каналы, LinearRgb — линейные каналы после декодирования.
Oklch изменяет светлоту, сохраняет тон и уменьшает цветность только для попадания в sRGB.
Одинаковый amount в разных режимах не означает одинаковое изменение яркости или светлоты.
