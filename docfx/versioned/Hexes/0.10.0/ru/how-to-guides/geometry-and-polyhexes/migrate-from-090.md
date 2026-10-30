# Переход с Hexes 0.9.0

Hexes 0.10.0 объединяет пространственные результаты разбиений. Пересоберите приложения:
изменения возвращаемых типов несовместимы с 0.9.0 на уровне исходного и бинарного API.

## Замена явно указанных типов

| 0.9.0 | 0.10.0 |
|---|---|
| `VoronoiHexPartitionMap` | `SpatialHexPartitionMap<VoronoiHexPartitionCell>` |
| `PerlinNoiseHexPartitionMap` | `SpatialHexPartitionMap<HexPartitionCell>` |
| `PartialPerlinNoiseHexPartitionMap` | `SpatialPartialHexPartitionMap<HexPartitionCell>` |

Новые типы находятся в `Akeldov.Math.Hexes`. `VoronoiHexPartitionCell` остаётся в
`Akeldov.Math.Hexes.Partitioning.Voronoi`. При использовании `var` обычно не требуется менять
объявление результата. `PartialVoronoiHexPartitionMap` сохранён и наследует пространственную
базовую карту частичного разбиения.

## Замена вспомогательных членов полного Voronoi

Сохраните исходную переменную `HexCenterMap`, если нужны именно центры, использованные при
разбиении. Полный результат больше не предоставляет `Centers`. Свойство `Geometry` сохранено;
новая карта центров по геометрии не воспроизводит вручную изменённые координаты центров.

Передавайте полное разбиение напрямую туда, где принимается `ISpatialIntHexMap`. Прежнее неявное
преобразование в `SpatialHexMap<int>` и `ToMutableHexMap()` удалены. Для независимой
пространственной карты назначений скопируйте идентификаторы:

```csharp
var assignments = new int[partition.Topology.Count];
for (int i = 0; i < assignments.Length; i++)
    assignments[i] = partition[i];
var independent = new SpatialHexMap<int>(partition.Geometry, assignments);
```

Частичный результат Voronoi сохраняет карту центров, преобразование в общий фасад, метод
копирования назначений и доступ к исходной маске участия.

## Сохранение геометрии при преобразованиях

У `SplitDisconnectedCells` и `DiffuseByCellularPotts` появились перегрузки для обоих
пространственных типов разбиений. Сохраняйте пространственный статический тип или используйте
`var`, чтобы результат сохранял начало координат, радиус и layout. Приведение к
`HexPartitionMap<T>` или `PartialHexPartitionMap<T>` выбирает перегрузки без геометрии.
Обе операции создают результат без изменения исходного разбиения.

Запись через индексатор по-прежнему меняет только назначения, без обновления `Cells` и
`HexIndexes`. Перед диффузией или разделением поддерживайте их согласованность.

## Другие дополнения

- `IHexMap<out TValue>` поддерживает ковариантные преобразования для ссылочных значений.
- `ToOffsetRegion(distance, layout)` расширяет регион полигекса на конечное неотрицательное
  расстояние в мировых единицах. Ноль даёт исходную форму; `ToApothemOffsetRegion` сохранён.
- Целевые платформы остаются `netstandard2.1` и `net6.0`, зависимость Spatial2D — `[1.1.0, 2.0.0)`.
