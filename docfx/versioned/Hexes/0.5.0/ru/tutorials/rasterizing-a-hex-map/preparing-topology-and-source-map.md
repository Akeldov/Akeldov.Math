# Подготовка топологии и исходной карты

Сначала создайте конечную гексагональную топологию, её геометрию в пространстве и по одному
нормализованному значению высоты для каждого гекса. На следующих шагах эти значения будут
интерполированы в пиксели.

## Создание карты

Замените содержимое `Program.cs` следующим кодом:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;
using Akeldov.Math.Spatial2D.Imaging;
using Akeldov.Math.Spatial2D.Rasterization;

const int Width = 9;
const int Height = 7;

var topology = new HexMapTopology(Width, Height, Layout.OddR);
var mapGeometry = new HexMapGeometry(topology, radius: 1f);
var elevationMap = new HexMap<float>(topology);

for (int y = 0; y < Height; y++)
{
    for (int x = 0; x < Width; x++)
    {
        float dx = (x - (Width - 1) / 2f) / (Width - 1);
        float dy = (y - (Height - 1) / 2f) / (Height - 1);
        float hill = 1f - 1.5f * MathF.Sqrt(dx * dx + dy * dy);
        float ridge = 0.15f * MathF.Sin(1.3f * x + 0.7f * y);

        elevationMap[new VectorXYInt(x, y)] = Math.Clamp(hill + ridge, 0f, 1f);
    }
}
```

`HexMapTopology` задаёт конечный диапазон индексов 9×7 и взаимное расположение строк `OddR`.
`HexMapGeometry` размещает эту топологию в непрерывном пространстве. Радиус равен одной единице
координат, а начало координат по умолчанию оставляет внешние гексы целиком внутри границ геометрии.

Процедурная формула нужна только для демонстрационных данных. Каждое значение ограничивается
диапазоном от `0` до `1`, что упрощает последующее назначение цветов. Вместо неё можно использовать
температуру, высоту, влияние или другое скалярное поле.

Перейдите к [созданию индексного растра](creating-an-index-raster.md).
