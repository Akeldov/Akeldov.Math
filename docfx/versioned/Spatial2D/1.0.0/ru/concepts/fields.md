# Поля

Поле сопоставляет каждой точке двумерного пространства некоторое значение. Оно может описывать
высоту, тепло, стоимость перемещения, идентификатор материала, маску или другую изменяющуюся в
пространстве величину. Поле сэмплируется непосредственно в координатах `PointXY` и не требует
растра.

Типы полей находятся в пространстве имён <xref:Akeldov.Math.Spatial2D.Fields>.

## Контракт поля

| Интерфейс | Контракт |
|---|---|
| <xref:Akeldov.Math.Spatial2D.Fields.IField`1> | `Sample(point)` возвращает одно значение `TValue` в двумерной точке. |
| <xref:Akeldov.Math.Spatial2D.Fields.IFloatField> | Возвращает значения `float` во включительном диапазоне от `Min` до `Max`. |
| <xref:Akeldov.Math.Spatial2D.Fields.IIntField> | Возвращает значения `int` во включительном диапазоне от `Min` до `Max`. |

Реализуйте `IField<TValue>` напрямую, если значение вычисляется по формуле или приходит из
внешнего источника данных. Используйте поле влияния, когда дискретные точки или кривые вносят
значения в зависимости от расстояния до точки запроса.

## Конвейер поля влияния

Поле влияния разделяет геометрию источников, их выбор и объединение значений:

```text
Сохранённые источники влияния
        |
        v
Необязательный индекс выбирает непустое локальное окружение
        |
        v
Сэмплер объединяет значения выбранных источников
        |
        v
Ограниченное поле проверяет и ограничивает публичный результат
```

<xref:Akeldov.Math.Spatial2D.Fields.InfluenceField`2> реализует общий конвейер.
<xref:Akeldov.Math.Spatial2D.Fields.PointInfluenceField`2> и
<xref:Akeldov.Math.Spatial2D.Fields.CurveInfluenceField`2> задают контракты для точечных и
криволинейных источников.

## Выбор геометрии источника

Каждый типизированный источник возвращает
<xref:Akeldov.Math.Spatial2D.Fields.InfluenceSample`1>, содержащий значение, точку источника,
расстояние до неё и вес.

| Геометрия | Встроенные источники | Применение |
|---|---|---|
| Точка | <xref:Akeldov.Math.Spatial2D.Fields.FloatPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.IntPointInfluenceSource>, <xref:Akeldov.Math.Spatial2D.Fields.BoolPointInfluenceSource> | Датчики, поселения, управляющие точки, разрозненные измерения и категориальные маркеры |
| Параметризованная кривая | <xref:Akeldov.Math.Spatial2D.Fields.FloatCurveInfluenceSource> | Дороги, реки, береговые линии, пути и другие значения, привязанные вдоль [кривой](geometry-model/curves.md) |

Точечный источник имеет фиксированные `Position`, значение и вес. Источник на кривой проецирует
запрос на кривую; его значение и вес могут зависеть от координаты проекции.

## Создание точечного поля влияния

Создайте источники, выберите сэмплер и передайте их полю:

```csharp
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Fields;

var sources = new[]
{
    new FloatPointInfluenceSource(1f, new PointXY(0f, 0f), 0f),
    new FloatPointInfluenceSource(1f, new PointXY(10f, 0f), 100f),
    new FloatPointInfluenceSource(1f, new PointXY(5f, 8f), 50f)
};

var sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);

float value = field.Sample(new PointXY(4f, 3f));
float minimum = field.Min; // 0
float maximum = field.Max; // 100
```

Числовые точечные поля вычисляют `Min`, `Max` и `DistinctValues` из сохранённых источников.
`BoolPointInfluenceField` предоставляет различные логические значения без числового диапазона.

## Настройка обработки источников

Сэмплер обязателен, а индекс источников — нет. Они отвечают на разные вопросы:

| Решение | Подробное описание | Кратко |
|---|---|---|
| Как объединять выбранные значения? | [Стратегии сэмплирования](fields/sampling-strategies.md) | Выберите ближайший источник, обратно-взвешенное по расстоянию или барицентрическое поведение. |
| Какие источники должны участвовать в этой точке? | [Индексирование источников](fields/source-indexing.md) | Используйте геометрию полуплоскостей или Делоне для получения локального окружения. |

Индексированный выбор математически меняет поле и не является только оптимизацией. Передайте
полю сам индекс, чтобы он оставался единственным владельцем сохранённого снимка источников:

```csharp
var sourceIndex =
    new DelaunayInfluenceSourceIndex<FloatPointInfluenceSource>(sources);
var localField = new FloatPointInfluenceField(
    new BarycentricFloatSampler<FloatPointInfluenceSource>(),
    sourceIndex);
```

## Привязка влияния к кривой

`FloatCurveInfluenceSource` может вычислять значение по ближайшей координате кривой:

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

float value = curveField.Sample(new PointXY(7f, 3f)); // 70
```

## Владение и допустимые значения

При создании из коллекции поле влияния копирует ссылки на источники и предоставляет сохранённую
структуру через доступное только для чтения свойство `InfluenceSources`. Последующие структурные
изменения коллекции вызывающего кода не влияют на поле. При создании из индекса поле вместо этого
предоставляет снимок, принадлежащий индексу.

Коллекция источников должна быть непустой и не содержать `null`. Точки запросов и положения
точечных источников должны иметь конечные координаты. Веса должны быть неотрицательными и не
равными `NaN`; отдельные стратегии сэмплирования могут предъявлять более строгие требования.

## Растеризация поля

Поле не зависит от растра и допускает выборку в произвольных точках. Растеризация вычисляет его
в центрах ячеек `RasterGeometry`; вещественные точечные поля также предоставляют удобный метод
тепловой карты:

```csharp
using Akeldov.Math.Spatial2D.Rasterization;

var geometry = new RasterGeometry(
    origin: new PointXY(0f, 0f),
    size: new VectorXY(10f, 8f),
    resolution: new VectorXYInt(160, 128));

var heatMap = field.RasterizeHeatMap(geometry);
```

Следующие материалы:

- [Стратегии сэмплирования](fields/sampling-strategies.md)
- [Индексирование источников](fields/source-indexing.md)
- [Построить карту влияния](../how-to-guides/fields/build-an-influence-map.md)
- [Учебник по созданию карты влияния](../tutorials/building-an-influence-map/index.md)
- [Растеризация](rasterization.md)
