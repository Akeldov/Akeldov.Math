# Получить рёбра и вершины

Используйте `HexEdge`, чтобы выбрать соседа через сторону, и `HexVertex`, чтобы выбрать три гекса,
сходящихся в углу. Всегда передавайте раскладку карты.

```csharp
using Akeldov.Math.Hexes;
using Akeldov.Math.Hexes.Topology;
using Akeldov.Math.Spatial2D;

var topology = new HexMapTopology(5, 5, Layout.OddR);
var hex = new VectorXYInt(2, 2);

// Ребро Edge0 разделяют hex и acrossEdge.
VectorXYInt acrossEdge = hex.GetAdjacent(
    HexEdge.Edge0,
    topology.Layout); // (3, 2)

// В вершине Vertex0 сходятся три гекса.
Triplet<VectorXYInt> atVertex = hex.GetAdjacentTriplet(
    HexVertex.Vertex0,
    topology.Layout);

VectorXYInt main = atVertex.Main;   // (2, 2)
VectorXYInt left = atVertex.Left;   // (2, 3)
VectorXYInt right = atVertex.Right; // (3, 2)
```

Если нужны только два соседних гекса, вызовите `GetAdjacentPair(vertex, layout)`. Чтобы получить
два значения `HexEdge`, сходящихся в вершине, используйте `vertex.GetAdjacentEdges(layout)`.

Рёбра и вершины нумеруются от `0` до `5` против часовой стрелки. Их физическая ориентация зависит
от раскладки. Вспомогательные методы работают с бесконечной сеткой, поэтому проверяйте полученные
индексы по границам конечной карты перед использованием в `HexMap<TValue>`.

Эти методы описывают топологические связи, а не точки или отрезки в мировом пространстве.
Пространственные координаты описаны в разделе
[«Геометрия»](../../concepts/hex-grid-model/geometry.md). Чтобы представить произвольную конечную
форму, переходите к рецепту
[«Построить топологию полигекса из маски»](build-polyhex-topology-from-a-mask.md).
