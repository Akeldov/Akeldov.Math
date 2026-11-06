# IndexPartialSeptupletMap

Предвычисляет Main и шесть соседних индексов вместе с флагами принадлежности конечной исходной топологии. Подходит для повторного обхода доступных соседей без повторного вычисления границ.

Тип значения: `PartialSeptuplet<VectorXYInt>`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var map = new IndexPartialSeptupletMap(topology);
PartialSeptuplet<VectorXYInt> neighbors = map[VectorXYInt.Zero];
if (neighbors.HasAdjacent0)
{
    VectorXYInt adjacent = neighbors.Adjacent0;
}
```

При любой допустимой выборке `HasMain` равен true. `HasAdjacent0`–`HasAdjacent5` отмечают доступных соседей в порядке рёбер. Отсутствующие позиции сохраняют логические индексы: проверяйте флаги, а не значения. Сам индекс выборки должен находиться внутри карты.

Карта владеет предвычисленным хранилищем и имеет индексаторы только для чтения. Возвращаемое значение — структура, а не изменяемое представление хранилища. См. [Значения окрестностей](../neighborhood-values.md) о флагах присутствия и непосредственной выборке значений соседей.

## Хранение и индексирование

Время создания и объём хранилища растут с `Topology.Count`. Переиспользуйте карту для запросов по одной исходной сетке. Векторный индексатор обращается к исходному гексу; плоский использует `y * Topology.Resolution.X + x`. Оба требуют допустимого индекса выборки.

[Карты](../maps.md) · API: <xref:Akeldov.Math.Hexes.Topology.IndexPartialSeptupletMap>.
