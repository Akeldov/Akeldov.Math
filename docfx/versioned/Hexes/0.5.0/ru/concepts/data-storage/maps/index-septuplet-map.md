# IndexSeptupletMap

Предвычисляет индекс каждого гекса как Main и индексы шести соседей по рёбрам как Adjacent0–Adjacent5. Подходит для повторных запросов окрестности, когда вызывающий код обрабатывает границы карты или использует дополненные данные.

Тип значения: `Septuplet<VectorXYInt>`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var map = new IndexSeptupletMap(topology);
Septuplet<VectorXYInt> neighbors = map[new VectorXYInt(1, 1)];
VectorXYInt main = neighbors.Main;
VectorXYInt adjacent = neighbors.Adjacent0;
```

Позиции соседей соответствуют `HexEdge.Edge0`–`HexEdge.Edge5`; смещения зависят от раскладки и чётности строки или столбца. Полный результат сохраняет индексы бесконечной сетки, включая отрицательные координаты и индексы за пределами топологии. Проверяйте соседа перед обращением к конечной карте значений.

Карта владеет предвычисленным массивом и предоставляет индексаторы только для чтения. Возвращённые структуры не позволяют заменить сохранённые окрестности. Используйте [IndexPartialSeptupletMap](index-partial-septuplet-map.md), чтобы также предвычислить принадлежность границам.

## Хранение и индексирование

Время создания и объём хранилища растут с `Topology.Count`. Переиспользуйте карту для запросов по одной исходной сетке. Векторный индексатор обращается к исходному гексу; плоский использует `y * Topology.Resolution.X + x`. Оба требуют допустимого индекса выборки.

[Карты](../maps.md) · API: <xref:Akeldov.Math.Hexes.Topology.IndexSeptupletMap>.
