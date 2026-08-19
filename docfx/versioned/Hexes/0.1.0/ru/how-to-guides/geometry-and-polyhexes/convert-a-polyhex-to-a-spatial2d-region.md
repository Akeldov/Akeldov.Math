# Преобразовать полигекс в регион Spatial2D

Используйте `ToRegion()`, когда занятые ячейки нужно превратить в единую фигуру Spatial2D для
геометрических запросов. Преобразование строит точное объединение правильных гексов, удаляет общие
внутренние рёбра и сохраняет отверстия и несвязные компоненты с помощью правила заливки
«чёт-нечет».

## Создание региона с отверстием

У топологического `Polyhex` нет физического размера ячейки. Оберните его маску в
<xref:Akeldov.Math.Hexes.Geometry.PolyhexGeometry>, подключите расширения для контуров и передайте
раскладку, задающую ориентацию гексов:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Geometry.Contours;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Regions;

const float HexRadius = 2f;
const Layout layout = Layout.OddR;

var geometry = new PolyhexGeometry(
    new bool[,]
    {
        { true, true,  true },
        { true, false, true },
        { true, true,  true }
    },
    radius: HexRadius);

ContourBasedRegion region = geometry.ToRegion(layout);

Console.WriteLine($"Контуров: {region.Contours.Count}");
Console.WriteLine($"Правило заливки: {region.FillRule}");
```

Первое измерение массива соответствует Q, второе — R. Незанятая ячейка `[1, 1]` окружена занятыми,
поэтому результат содержит внешний контур и контур отверстия:

```text
Контуров: 2
Правило заливки: EvenOdd
```

`ToRegion()` создаёт новый <xref:Akeldov.Math.Spatial2D.Regions.ContourBasedRegion> и не изменяет
`geometry` или лежащую в её основе неизменяемую маску полигекса.

## Геометрические запросы к заполненной фигуре

Если отверстия и несколько компонентов нужно интерпретировать вместе, используйте регион, а не
отдельный контур. Добавьте следующий код после создания `region`:

```csharp
PointXY occupiedCenter = (PointXY)Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(
    q: 0,
    r: 0,
    hexRadius: HexRadius,
    layout: layout);
PointXY holeCenter = (PointXY)Akeldov.Math.Hexes.Geometry.VectorXYExtensions.GetHexCenter(
    q: 1,
    r: 1,
    hexRadius: HexRadius,
    layout: layout);
var outside = new PointXY(-1f, -1f);

Console.WriteLine($"Центр занятого гекса: {region.Contains(occupiedCenter)}");
Console.WriteLine($"Центр отверстия: {region.Contains(holeCenter)}");
Console.WriteLine($"Снаружи: {region.Contains(outside)}");
```

Результат:

```text
Центр занятого гекса: True
Центр отверстия: False
Снаружи: False
```

`Contains(point)` включает границу региона. `Distance(point)` возвращает неотрицательное расстояние
до ближайшей границы. `SignedDistance(point)` отрицателен внутри заполненной области, равен нулю на
границе и положителен как снаружи внешнего контура, так и внутри отверстия.

## Согласованная раскладка и размещение

Перегрузка без аргумента раскладки использует `Layout.OddR`. R-раскладки создают гексы вершиной
вверх, а Q-раскладки — гексы горизонтальной стороной вверх. `PolyhexGeometry` хранит радиус, но не
раскладку, поэтому при преобразовании маски и вычислении центров её ячеек передавайте одну и ту же
раскладку.

Преобразование использует стандартное размещение нулевого гекса и не принимает пользовательское
начало координат. В R-раскладках центр ячейки маски `[0, 0]` равен
`(HexApothem, HexRadius)`, а в Q-раскладках — `(HexRadius, HexApothem)`. Если приложению нужно другое
начало координат, преобразуйте или объедините полученную геометрию отдельно.

## Обработка некорректных входных данных

Маска должна содержать хотя бы одну занятую ячейку. Для пустого полигекса `ToRegion()` выбрасывает
`InvalidOperationException`, поскольку замкнутую границу построить нельзя. Значение `null` вместо
геометрии приводит к `ArgumentNullException`, а неподдерживаемое значение раскладки — к
`ArgumentOutOfRangeException`.

Если заполненную границу нужно расширить наружу на одну апофему гекса, используйте
`ToApothemOffsetRegion()`. Для работы с отдельными замкнутыми границами вместо заполненной фигуры
смотрите [«Преобразовать полигекс в контур Spatial2D»](convert-a-polyhex-to-a-spatial2d-contour.md).
В разделе [«Полигексы»](../../concepts/hex-grid-model/polyhexes.md) подробно описаны владение
маской, размещение, отверстия и несвязные компоненты.
