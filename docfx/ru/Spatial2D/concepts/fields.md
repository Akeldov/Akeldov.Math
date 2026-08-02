# Поля

Поле сопоставляет точке двумерного пространства некоторое значение. Поля могут представлять
тепло, высоту, стоимость перемещения, идентификатор материала, маску или любую другую величину,
меняющуюся в пространстве. Для выборки поля не нужен растр: вызывающий код передаёт `PointXY` и
непосредственно получает значение.

Типы полей находятся в пространстве имён <xref:Akeldov.Math.Spatial2D.Fields>.

## Интерфейсы полей

Базовый интерфейс намеренно мал:

| Интерфейс | Контракт |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.IField`1> | `Sample(point)` возвращает одно значение `TValue` в двумерной точке. |
| <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> | Возвращает значения `float` во включительном диапазоне от `Min` до `Max`. |
| <xref:Akeldov.Math.Spatial2D.Fields.IIntField> | Возвращает значения `int` во включительном диапазоне от `Min` до `Max`. |

Реализуйте `IField<TValue>` напрямую, если значение задаётся замкнутой формулой или поступает из
внешнего источника данных:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

public sealed class HorizontalGradientField : IFloatField
{
    public float Min => 0f;
    public float Max => 100f;

    public float Sample(PointXY point)
    {
        float value = point.X;
        return value.Clamp(Min, Max);
    }
}
```

Поле влияния подходит, когда значения должны вычисляться по дискретным точечным или
криволинейным источникам.

## Конвейер поля влияния

Поле влияния разделяет четыре ответственности:

```text
Сохранённые источники влияния
        |
        v
Необязательное отсечение выбирает непустое локальное подмножество
        |
        v
Стратегия выборки объединяет вклады источников в исходное значение
        |
        v
Ограниченное поле проверяет и ограничивает публичный результат
```

Благодаря этому один набор источников может использовать выборку ближайшего соседа,
обратно-взвешенную по расстоянию или барицентрическую выборку — с геометрическим отсечением или
без него.

<xref:Akeldov.Math.Spatial2D.Fields.InfluenceField`2> реализует общий конвейер.
<xref:Akeldov.Math.Spatial2D.Fields.PointInfluenceField`2> и
<xref:Akeldov.Math.Spatial2D.Fields.CurveInfluenceField`2> ограничивают его точечными и
криволинейными источниками.

## Выбор источников влияния

Каждый <xref:Akeldov.Math.Spatial2D.Fields.IInfluenceSource> измеряет расстояние до точки выборки.
Типизированный источник возвращает <xref:Akeldov.Math.Spatial2D.Fields.InfluenceSample`1> со
следующими данными:

- вносимое значение;
- точка источника, использованная для вклада;
- неотрицательное расстояние до этой точки;
- вес источника для совместимых стратегий выборки.

Выбирайте источник по месту привязки значения:

| Геометрия источника | Встроенные источники | Поведение |
|---|---|---|
| Точка | <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceSource> | Точка источника равна фиксированному `Position`; значение и вес постоянны. |
| Параметризованная кривая | <xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceSource> | Точка источника является проекцией на кривую; значение и вес могут меняться по координате кривой. |

Точечные источники подходят для поселений, датчиков, управляющих маркеров или разрозненных
измерений. Криволинейные источники подходят для дорог, рек, береговых линий, путей и других
объектов, ближайшее положение которых может находиться в любом месте вдоль
[кривой](geometry-model/curves.md).

## Создание точечного поля влияния

Следующее поле возвращает вещественное значение, вычисленное по трём расположенным в
пространстве источникам:

```csharp
var sources = new[]
{
    new FloatPointInfluenceSource(
        weight: 1f,
        position: new PointXY(0f, 0f),
        value: 0f),
    new FloatPointInfluenceSource(
        weight: 1f,
        position: new PointXY(10f, 0f),
        value: 100f),
    new FloatPointInfluenceSource(
        weight: 1f,
        position: new PointXY(5f, 8f),
        value: 50f)
};

var sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();

var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(4f, 3f));
float minimum = field.Min; // 0
float maximum = field.Max; // 100
```

`FloatPointInfluenceField` вычисляет `Min`, `Max` и `DistinctValues` из скопированного набора
источников. `IntPointInfluenceField` делает то же для целочисленных источников.
`BoolPointInfluenceField` предоставляет различные логические значения, но не имеет числового
диапазона.

## Выбор стратегии выборки

Стратегия определяет, как выбранные источники влияют на результат:

| Стратегия | Встроенный тип | Результат |
|---|---|---|
| Ближайший источник | <xref:Akeldov.Math.Spatial2D.Fields.NearestInfluenceSampler`2> и числовые специализации | Возвращает значение источника с наименьшим геометрическим расстоянием. Работает с произвольными типами значений. |
| Обратно-взвешенная по расстоянию | <xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> | Смешивает вещественные значения с весом `вес источника / расстояние`; практически совпавший источник немедленно побеждает. |
| Барицентрическая | <xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1> и <xref:Akeldov.Math.Spatial2D.Fields.BarycentricIntSampler`1> | Интерполирует или экстраполирует вдоль отрезка либо внутри треугольника источников, используя запасные варианты для вырожденных конфигураций. |

Выборка ближайшего источника создаёт кусочно-постоянные территории. Обратно-взвешенная выборка
создаёт гладкое глобальное смешивание. Барицентрическая выборка создаёт кусочно-линейное
изменение, определяемое локальной геометрией источников.

Стратегия выборки является математической операцией и при экстраполяции может выйти за диапазон
значений источников. Ограниченные типы полей сохраняют публичный контракт, ограничивая исходный
результат. Общий `InfluenceField<TSource, TValue>` не задаёт диапазон и не ограничивает результат
стратегии.

Обратно-взвешенная выборка требует конечного положительного веса каждого выбранного вклада.
Выборка ближайшего источника игнорирует вес. Барицентрическая стратегия учитывает эффективное
расстояние с поправкой на вес при выборе кандидатов, а сама интерполяция использует положения и
значения источников.

## Отсечение источников перед выборкой

Отсекатель выбирает источники, относящиеся к каждой точке, до запуска стратегии выборки.
Отсечение меняет локальную окрестность интерполяции, поэтому это не только оптимизация
производительности.

Встроенные отсекатели работают с точечными источниками влияния:

| Отсекатель | Поведение выбора |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.HalfPlaneCuller`1> | Обходит источники от ближних к дальним и исключает скрытые за границами полуплоскостей, созданными более близкими источниками. |
| <xref:Akeldov.Math.Spatial2D.Fields.DelaunayCuller`1> | Возвращает содержащий точку треугольник Делоне; вне триангуляции возвращает ближайшую вершину или ребро выпуклой оболочки, а для коллинеарного случая применяет запасную стратегию. |

`DelaunayCuller` требует как минимум три источника с уникальными положениями. Неколлинеарные
источники триангулируются при создании отсекателя. Оба отсекателя всегда возвращают новый
изменяемый непустой список, принадлежащий вызывающему коду.

Передайте полю и отсекателю один логический набор источников:

```csharp
var culler = new DelaunayCuller<FloatPointInfluenceSource>(sources);
var barycentric = new BarycentricFloatSampler<FloatPointInfluenceSource>();

var localField = new FloatPointInfluenceField(
    barycentric,
    sources,
    culler);

float localValue = localField.Sample(new PointXY(4f, 3f));
```

Пользовательские реализации
<xref:Akeldov.Math.Spatial2D.Fields.IInfluenceSourceCuller`1> обязаны возвращать хотя бы один
источник. Если вернуть `null` или пустой список, поле завершится явной ошибкой вместо выборки из
неопределённой окрестности.

## Привязка влияния к кривой

`FloatCurveInfluenceSource` оборачивает `IParameterizedCurve`. Для каждой выборки он проецирует
точку на кривую и вычисляет поставщики значения и веса в координате проекции:

```csharp
using Akeldov.Math.Spatial2D.Curves;

var path = new ParameterizedSegment(
    startPoint: new PointXY(0f, 0f),
    endPoint: new PointXY(10f, 0f));

var pathSource = new FloatCurveInfluenceSource(
    weight: 1f,
    curve: path,
    valueProvider: curveCoordinate => curveCoordinate * 10f);

var curveField = new FloatCurveInfluenceField(
    new NearestFloatInfluenceSampler<ICurveInfluenceSource<float>>(),
    new ICurveInfluenceSource<float>[] { pathSource },
    min: 0f,
    max: 100f);

float curveValue = curveField.Sample(new PointXY(7f, 3f)); // 70
```

Доступны перегрузки с постоянными значением и весом. Поставщики, зависящие от координаты,
проверяются при выборке: вес должен быть неотрицательным и не равным `NaN`, значение не должно
быть `NaN`. `FloatCurveInfluenceField` использует явно переданные конструктору `min` и `max` и
ограничивает результат этим включительным диапазоном.

## Владение источниками и допустимые значения

Полю влияния нужна непустая коллекция источников без элементов `null`. Поле копирует ссылки на
источники в закрытое хранилище и предоставляет сохранённую структуру через доступное только для
чтения свойство `InfluenceSources`. Последующие изменения списка или массива вызывающего кода не
меняют порядок или количество источников поля.

Точки выборки и положения точечных источников должны быть конечными. Веса источников должны быть
неотрицательными и не равными `NaN`; отдельные стратегии могут предъявлять более строгие
требования. Вещественные значения источников и диапазоны ограниченных полей не должны содержать
`NaN`.

## Растеризация поля

Поле остаётся непрерывным и допускает выборку в произвольных точках. Растеризация вычисляет его
в центрах ячеек `RasterGeometry` и сохраняет преобразованные значения. Для
`FloatPointInfluenceField` также доступен удобный растеризатор тепловой карты:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(10f, 8f),
    resolution: new VectorXYInt(160, 128));

var heatMap = field.RasterizeHeatMap(geometry);
```

Сквозные примеры:

- [Построить карту влияния](../how-to-guides/fields/build-an-influence-map.md)
- [Учебник по созданию карты влияния](../tutorials/building-an-influence-map/index.md)
- [Растеризация](rasterization.md)
