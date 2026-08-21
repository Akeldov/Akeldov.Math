# Создать хроматический растр

Используйте хроматические растры, чтобы заранее вычислить трёхцветные классы или упорядоченные по
классам веса интерполяции вокруг каждой выборки прямоугольного растра. Они сохраняют устойчивую связь
с классами `0`, `1` и `2`, даже когда геометрический порядок `Main`, `Left`, `Right` меняется по сетке.

## Задать общую геометрию выборки

Создайте исходную геометрию гексов и прямоугольную сетку для её выборки:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Rasterization;

var mapGeometry = new HexMapGeometry(
    width: 4,
    height: 3,
    origin: new VectorXY(10f, 20f),
    radius: 2f,
    layout: Layout.OddR);

RasterGeometry rasterGeometry = mapGeometry.ToRasterGeometry(
    pixelsPerApothem: 16f,
    margin: mapGeometry.Radius);
```

Передавайте один и тот же экземпляр `RasterGeometry` всем связанным растрам. Совпадения разрешений
недостаточно, если различаются начало координат или размер области в пространстве.

## Прочитать классы в геометрическом порядке

Создайте <xref:Akeldov.Math.Hexes.Topology.ChromaticIndexTripletRaster>, чтобы классифицировать
содержащий гекс и двух соседей, которые встречаются с ним у ближайшей вершины:

```csharp
var classRaster = new ChromaticIndexTripletRaster(
    mapGeometry,
    rasterGeometry);

var sample = new VectorXYInt(
    classRaster.Resolution.X / 2,
    classRaster.Resolution.Y / 2);

if (classRaster.TryGetValue(sample, out Triplet<byte> classes))
{
    Console.WriteLine($"Main:  класс {classes.Main}");
    Console.WriteLine($"Left:  класс {classes.Left}");
    Console.WriteLine($"Right: класс {classes.Right}");
}
```

`Main`, `Left` и `Right` обозначают геометрические позиции, а не фиксированные номера классов. Для
полной тройки у вершины их значения являются перестановкой `0`, `1`, `2`; не считайте `Main` классом
`0`.

Полный растр классифицирует подразумеваемую бесконечную решётку. `TryGetValue` проверяет только
координату прямоугольного растра, поэтому возвращённый класс может принадлежать гексу за пределами
конечной исходной топологии.

## Прочитать барицентрические веса в порядке классов

Создайте <xref:Akeldov.Math.Hexes.Topology.ChromaticBarycentricTripletRaster>, если один канал должен
всегда представлять один и тот же хроматический класс:

```csharp
var weightRaster = new ChromaticBarycentricTripletRaster(
    mapGeometry,
    rasterGeometry);

if (weightRaster.TryGetValue(
        sample,
        out ChromaticTriplet<float> weights))
{
    Console.WriteLine($"Вес класса 0: {weights.Index0}");
    Console.WriteLine($"Вес класса 1: {weights.Index1}");
    Console.WriteLine($"Вес класса 2: {weights.Index2}");
}
```

Это обычные барицентрические веса `Main`, `Left`, `Right`, переставленные по классам из `classRaster`.
Поэтому `Index0`, `Index1` и `Index2` всегда соответствуют классам `0`, `1` и `2`, а их сумма для
полной тройки приблизительно равна `1`.

Два примера намеренно используют разный порядок: `classRaster.Main` сообщает класс главного
геометрического гекса, а `weightRaster.Index0` — вес того геометрического гекса, который имеет класс
`0`. Не объединяйте эти компоненты по одинаковой позиции.

## Обработать границу конечной карты

Используйте частичные варианты, когда допустимы только ячейки исходной карты:

```csharp
var partialClassRaster = new ChromaticIndexPartialTripletRaster(
    mapGeometry,
    rasterGeometry);

var partialWeightRaster = new ChromaticBarycentricPartialTripletRaster(
    mapGeometry,
    rasterGeometry);

PartialTriplet<byte> partialClasses = partialClassRaster[sample];
PartialChromaticTriplet<float> partialWeights = partialWeightRaster[sample];

if (partialClasses.HasMain)
    Console.WriteLine($"Класс Main: {partialClasses.Main}");
if (partialClasses.HasLeft)
    Console.WriteLine($"Класс Left: {partialClasses.Left}");
if (partialClasses.HasRight)
    Console.WriteLine($"Класс Right: {partialClasses.Right}");

if (partialWeights.HasIndex0)
    Console.WriteLine($"Присутствующий вес класса 0: {partialWeights.Index0}");
if (partialWeights.HasIndex1)
    Console.WriteLine($"Присутствующий вес класса 1: {partialWeights.Index1}");
if (partialWeights.HasIndex2)
    Console.WriteLine($"Присутствующий вес класса 2: {partialWeights.Index2}");
```

Растр классов хранит флаги в геометрическом порядке `HasMain`, `HasLeft`, `HasRight`.
Барицентрический растр переставляет и значения, и флаги в `HasIndex0`, `HasIndex1`, `HasIndex2`.
Отсутствующие веса не нормализуются повторно; при смешивании значений ограниченной карты делите
результат на сумму присутствующих весов.

В версии `0.1.0` у `ChromaticIndexPartialTripletRaster` нет `TryGetValue`: заранее проверяйте
координату или используйте проверяемый индексатор `[VectorXYInt]`.
`ChromaticBarycentricPartialTripletRaster.TryGetValue` возвращает `false` только для координаты за
пределами прямоугольного растра. Успешный вызов всё ещё может вернуть значение, у которого сняты все
три флага присутствия.

## Выбрать подходящий растр

| Требуемые данные | Полная исходная решётка | Конечная исходная карта |
|---|---|---|
| Классы в порядке `Main/Left/Right` | `ChromaticIndexTripletRaster` | `ChromaticIndexPartialTripletRaster` |
| Веса в порядке `Index0/Index1/Index2` | `ChromaticBarycentricTripletRaster` | `ChromaticBarycentricPartialTripletRaster` |

Конструкторы с одним аргументом автоматически покрывают карту с плотностью один пиксель на апофему
без внешнего поля. Используйте явную геометрию выборки, если несколько слоёв должны совпадать. Все
ячейки вычисляются при создании, поэтому используйте растры повторно, пока исходная геометрия и сетка
выборки не меняются.

Продолжите с разделом [«Визуализировать хроматизацию»](visualize-chromatization.md) или обратитесь к
[«Создать барицентрический растр»](../rasters/create-a-barycentric-raster.md) для интерполяции в
геометрическом порядке. Полные правила порядка описаны в разделе
[«Хроматизация»](../../concepts/spatial-algorithms/chromatization.md).
