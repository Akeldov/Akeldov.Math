# Экспорт

Гексагональные карты поддерживают бинарный экспорт и импорт через
<xref:Akeldov.Math.Hexes.HexMapFile> и методы расширения для `BinaryWriter` и `BinaryReader`.
Запись сохраняет размеры карты, раскладку и значения ячеек; пространственная запись также
сохраняет начало координат и радиус гекса. Чтение создаёт новую изменяемую карту с независимым
хранилищем, принадлежащую вызывающему коду.

## Поддерживаемые типы карт

Все перегрузки экспорта называются `Write`. Метод импорта должен соответствовать типу значений
и наличию пространственной геометрии в записи:

| Значения | Импорт карты без геометрии | Импорт пространственной карты |
|---|---|---|
| `bool` | `ReadBoolHexMap` | `ReadSpatialBoolHexMap` |
| `int` | `ReadIntHexMap` | `ReadSpatialIntHexMap` |
| `float` | `ReadFloatHexMap` | `ReadSpatialFloatHexMap` |

Эти методы доступны и у `HexMapFile`, и у `BinaryReader`. Произвольные типы значений `HexMap<T>`
не поддерживаются. Импорт не преобразует типы значений и не отбрасывает пространственную
геометрию: метод чтения другого вида записи выбрасывает `InvalidDataException`.

## Экспорт и импорт файла

```csharp
using Akeldov.Math.Hexes;

var mask = new BoolHexMap(
    new HexMapTopology(3, 2, Layout.OddR),
    new[] { true, false, true, false, true, false });

HexMapFile.Write("mask.hmap", mask);
BoolHexMap restored = HexMapFile.ReadBoolHexMap(
    "mask.hmap", maxCellCount: 1_000_000);
```

`Write` создаёт или перезаписывает файл. Родительский каталог должен существовать, а относительные
пути отсчитываются от текущего рабочего каталога процесса. Файловые методы закрывают файл и при
успехе, и при ошибке. Метод чтения файла получает первую запись и игнорирует оставшиеся байты.
Запись не атомарна: ошибка после открытия может оставить частично записанный файл.

## Экспорт и импорт потока

Используйте <xref:Akeldov.Math.Hexes.HexMapBinaryWriterExtensions> и
<xref:Akeldov.Math.Hexes.HexMapBinaryReaderExtensions> для работы с записями в существующем
бинарном потоке. Пример записывает и читает булеву карту в памяти:

```csharp
using System.IO;
using System.Text;
using Akeldov.Math.Hexes;

var mask = new BoolHexMap(
    new HexMapTopology(2, 1, Layout.OddR), new[] { true, false });

using var stream = new MemoryStream();
using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
writer.Write(mask);
writer.Flush();

stream.Position = 0;
using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
BoolHexMap restored = reader.ReadBoolHexMap(maxCellCount: 100);
```

Методы расширения оставляют reader, writer и сам поток открытыми. Они не сбрасывают буферы
и не меняют позицию отдельно от чтения или записи; временем жизни и позиционированием управляет
вызывающий код. Потоки без возможности перемещения также поддерживаются. Успешное чтение
потребляет ровно одну запись, поэтому поток может содержать несколько карт и другие данные.
При ошибке прежняя позиция не восстанавливается. Не изменяйте исходную карту во время записи.

## Сохранение пространственной геометрии

Выбор перегрузки зависит от типа источника на этапе компиляции. Конкретная пространственная
карта или `ISpatialHexMap<T>` выбирает пространственную перегрузку `Write`, сохраняющую начало
координат и радиус:

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Geometry;
using Akeldov.Math.Spatial2D;

var geometry = new HexMapGeometry(
    new HexMapTopology(2, 2, Layout.EvenQ), new VectorXY(-10f, 20f), radius: 2f);
var heights = new SpatialFloatHexMap(geometry, new[] { 1f, 2f, 3f, 4f });

HexMapFile.Write("heights.hmap", heights);
SpatialFloatHexMap restored = HexMapFile.ReadSpatialFloatHexMap(
    "heights.hmap", maxCellCount: 1_000_000);
```

Если передать тот же объект как `IHexMap<float>`, экспорт сохранит только топологию и значения,
без геометрии. Такую запись нужно читать через `ReadFloatHexMap`. Это правило действует и для
`BinaryWriter.Write`.

## Формат и проверка данных

Запись начинается с сигнатуры `HMAP` и версии формата `1`, независимой от версии пакета.
Значения следуют за заголовком построчно: сначала изменяется X. Булевы значения занимают один
байт на ячейку, целые и вещественные — четыре байта в порядке little-endian. Вещественные
значения сохраняют битовое представление. Пространственные записи содержат координаты начала
и радиус перед значениями. Формат не использует сжатие или контрольную сумму.

`maxCellCount` ограничивает выделение памяти и по умолчанию равен `int.MaxValue`; ноль разрешает
только пустые карты. До выделения массива значений проверяются размеры, раскладка, вид записи
и пространственная геометрия. Некорректная запись или превышение лимита вызывает
`InvalidDataException`, неполная запись — `EndOfStreamException`, отрицательный лимит —
`ArgumentOutOfRangeException`. Исключения файловой системы передаются вызывающему коду.

Побайтовое описание формата приведено в разделе
[Сохранение и загрузка гексагональных карт](../../../how-to-guides/maps-and-topology/save-and-load-hex-maps.md).
Вернуться к разделу [Карты](../maps.md).
