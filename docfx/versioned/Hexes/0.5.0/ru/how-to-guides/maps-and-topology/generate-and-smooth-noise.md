# Создать и сгладить шум

Создайте детерминированные процедурные данные с помощью `CreatePerlinNoise`, а затем уберите
мелкие колебания методом `GaussianBlur`.

## Генерация поля

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(128, 96, Layout.OddR);

FloatHexMap height = topology.CreatePerlinNoise(
    seed: 12345,
    scale: 16f,
    octaves: 5,
    persistence: 0.5f,
    lacunarity: 2f,
    offset: new VectorXY(0f, 0f));
```

Результат — изменяемая `FloatHexMap` со значениями в диапазоне `[0, 1]`. Одинаковые аргументы
создают одинаковое поле. Увеличьте `scale`, чтобы получить более крупные элементы, и измените
`offset`, чтобы выбрать другой участок того же поля, например соседний фрагмент карты.

`scale`, `lacunarity` и компоненты `offset` должны быть конечными; scale и lacunarity —
положительными. `octaves` должен быть положительным, а `persistence` — находиться в `[0, 1]`.

## Сглаживание поля

```csharp
FloatHexMap smoothHeight = height.GaussianBlur(sigma: 1.25f);
```

`sigma` измеряется в расстояниях между центрами гексов, смежных по ребру, и должен быть конечным
и положительным. Эта перегрузка обрезает ядро на трёх стандартных отклонениях.

Чтобы явно ограничить объём работы, задайте радиус ядра:

```csharp
FloatHexMap compactBlur = height.GaussianBlur(sigma: 1.25f, radius: 2);
```

Нулевой радиус создаёт независимую копию. У границ карты метод нормализует веса по доступным
исходным ячейкам, поэтому дополнительное заполнение не требуется. Обе перегрузки оставляют
`height` без изменений.
