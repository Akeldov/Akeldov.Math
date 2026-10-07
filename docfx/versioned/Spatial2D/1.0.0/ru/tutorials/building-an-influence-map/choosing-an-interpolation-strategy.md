# Выбор стратегии интерполяции

Стратегия выборки определяет, как выбранные источники превращаются в одно значение поля.
Spatial2D предоставляет несколько вариантов для чисел с плавающей точкой.

## Ближайший источник

Использованный на предыдущем шаге
<xref:Akeldov.Math.Spatial2D.Fields.NearestFloatInfluenceSampler`1> возвращает значение
ближайшего источника. Он подходит для дискретных зон, но создаёт скачки на границах между ними.

## Обратное расстояние

<xref:Akeldov.Math.Spatial2D.Fields.InverseDistanceWeightedFloatSampler`1> смешивает значения
всех переданных источников. Вклад источника пропорционален его весу и обратно пропорционален
расстоянию до точки:

```csharp
var sampler =
    new InverseDistanceWeightedFloatSampler<FloatPointInfluenceSource>();

var field = new FloatPointInfluenceField(sampler, sources);
```

Этот вариант даёт плавное поле, но без отсечения просматривает весь набор источников при каждом
вызове `Sample`.

## Барицентрическая интерполяция

Для учебной карты используйте
<xref:Akeldov.Math.Spatial2D.Fields.BarycentricFloatSampler`1>. Она интерполирует значение по
отрезку или треугольнику ближайших подходящих источников:

```csharp
var sampler = new BarycentricFloatSampler<FloatPointInfluenceSource>();
var field = new FloatPointInfluenceField(sampler, sources);
```

Один источник даёт постоянное значение, два образуют линейный переход, а три — плоскость значений
над треугольником. Для большего набора стратегия ищет подходящий треугольник среди ближайших
кандидатов. Снаружи выбранного треугольника она может экстраполировать значение, после чего
`FloatPointInfluenceField` ограничивает его диапазоном `Min`–`Max`.

Оставьте в `Program.cs` барицентрический вариант. Далее вы сделаете выбор источников локальным:
[«Отсечение удалённых источников»](culling-distant-sources.md).
