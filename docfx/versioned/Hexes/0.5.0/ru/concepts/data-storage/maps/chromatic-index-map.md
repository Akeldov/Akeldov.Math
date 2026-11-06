# ChromaticIndexMap

Предвычисляет класс трёхцветной раскраски для каждого гекса. Значения равны 0, 1 или 2; соседи по ребру имеют разные классы. Эти байты — метки классов, а не индексы карты и не цвета изображения.

Тип значения: `byte`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var map = new ChromaticIndexMap(topology);
byte chromaticClass = map[new VectorXYInt(1, 1)];
```

Используйте класс для выбора одного из трёх каналов или сопоставления данных гексов с хроматической интерполяцией. Классы зависят от топологии и раскладки. На границе нет отсутствующих позиций: каждому допустимому гексу соответствует один класс.

Конструктор однократно заполняет внутреннее хранилище; индексаторы доступны только для чтения. Класс 0 допустим и не означает отсутствие. См. [Хроматизация](../../spatial-algorithms/chromatization.md) о шаблоне и [Растры](../rasters.md) о пространственной выборке троек классов.

## Хранение и индексирование

Время создания и объём хранилища растут с `Topology.Count`. Переиспользуйте карту для запросов по одной исходной сетке. Векторный индексатор обращается к исходному гексу; плоский использует `y * Topology.Resolution.X + x`. Оба требуют допустимого индекса выборки.

[Карты](../maps.md) · API: <xref:Akeldov.Math.Hexes.Chromatization.ChromaticIndexMap>.
