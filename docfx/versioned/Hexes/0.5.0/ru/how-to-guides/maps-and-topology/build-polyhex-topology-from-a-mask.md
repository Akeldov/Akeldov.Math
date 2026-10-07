# Построить топологию полигекса из маски

Используйте `Polyhex`, чтобы преобразовать прямоугольную Q/R-маску в неизменяемое множество
занятых гексов. Значение `true` включает ячейку в фигуру.

```csharp
using Akeldov.Math.Hexes.Topology;

bool[,] mask =
{
    { false, true,  false }, // q = 0, r = 0..2
    { true,  true,  true  }, // q = 1, r = 0..2
    { false, true,  false }, // q = 2, r = 0..2
};

var polyhex = new Polyhex(mask);

int hexCount = polyhex.HexCount;         // 5
bool centerIsPresent = polyhex[1, 1];    // true
int qSize = polyhex.QRSResolution.Q;     // 3
int rSize = polyhex.QRSResolution.R;     // 3
```

Первое измерение массива соответствует Q, второе — R. Компонент S вычисляется как `-Q - R`,
поэтому третьего измерения у маски нет, а `Layout` не используется.

Конструктор копирует маску: последующие изменения `mask` не влияют на `polyhex`. Можно также
передать `int[,]`: ноль означает отсутствие ячейки, а любое ненулевое значение — её наличие.
Размеры маски должны быть положительными; пустые фигуры, отверстия и несвязные компоненты
допустимы.

Полная модель данных описана в разделе
[«Полигексы»](../../concepts/hex-grid-model/polyhexes.md). Чтобы добавить физический размер и
получить границу, переходите к рецепту
[«Преобразовать полигекс в контур Spatial2D»](../geometry-and-polyhexes/convert-a-polyhex-to-a-spatial2d-contour.md).
