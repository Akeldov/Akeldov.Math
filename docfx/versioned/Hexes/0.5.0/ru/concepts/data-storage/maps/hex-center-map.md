# HexCenterMap

Предвычисляет пространственный центр каждого гекса. Используйте карту, когда несколько выборок полей, запросов расстояния или операций разбиения используют одни и те же центры. В отличие от растра, она содержит ровно одну точку на исходный гекс.

Тип значения: `PointXY`.

## Создание и доступ

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Hexes.Chromatization;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(4, 3, Layout.OddR);
var geometry = new HexMapGeometry(topology, new VectorXY(10f, 20f), 2f);
var map = new HexCenterMap(geometry);
PointXY center = map[new VectorXYInt(1, 1)];
```

Конструктор с геометрией учитывает раскладку, начало и радиус. Конструктор с топологией использует единичный радиус. Центры вычисляются при создании в порядке строк.

`HexCenterMap` наследует изменяемое хранилище `HexMap<PointXY>`. Изменение сохранённой точки не обновляет `Geometry`; при использовании геометрических алгоритмов поддерживайте соответствие точек геометрии.

## Хранение и индексирование

Время создания и объём хранилища растут с `Topology.Count`. Переиспользуйте карту для запросов по одной исходной сетке. Векторный индексатор обращается к исходному гексу; плоский использует `y * Topology.Resolution.X + x`. Оба требуют допустимого индекса выборки.

[Карты](../maps.md) · API: <xref:Akeldov.Math.Hexes.Geometry.HexCenterMap>.
