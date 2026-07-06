using Akeldov.Math.Graphs;

namespace Akeldov.Math.Graphs.Tests;

public class GraphImplementationTests
{
    [Test]
    public void Graph_CopiesVerticesAndAdjacency()
    {
        var vertices = new List<string> { "a", "b", "c" };
        var aAdjacents = new List<string> { "b" };
        var adjacentVertices = new Dictionary<string, IReadOnlyList<string>>
        {
            ["a"] = aAdjacents
        };

        var graph = new Graph<string>(vertices, adjacentVertices);
        vertices.Add("d");
        aAdjacents.Add("c");

        Assert.Multiple(() =>
        {
            Assert.That(graph.Vertices, Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(graph.GetAdjacentVertices("a"), Is.EqualTo(new[] { "b" }));
            Assert.That(graph.GetAdjacentVertices("b"), Is.Empty);
            Assert.That(graph.GetAdjacentVertices("c"), Is.Empty);
        });
    }

    [Test]
    public void DirectedGraph_BuildsIncomingVerticesFromOutgoingVertices()
    {
        var graph = new DirectedGraph<string>(
            new[] { "a", "b", "c" },
            new Dictionary<string, IReadOnlyList<string>>
            {
                ["a"] = new[] { "b", "c" },
                ["c"] = new[] { "b" }
            });

        Assert.Multiple(() =>
        {
            Assert.That(graph.GetAdjacentVertices("a"), Is.EqualTo(new[] { "b", "c" }));
            Assert.That(graph.GetOutgoingVertices("a"), Is.EqualTo(new[] { "b", "c" }));
            Assert.That(graph.GetIncomingVertices("a"), Is.Empty);
            Assert.That(graph.GetIncomingVertices("b"), Is.EqualTo(new[] { "a", "c" }));
            Assert.That(graph.GetIncomingVertices("c"), Is.EqualTo(new[] { "a" }));
        });
    }

    [Test]
    public void EdgeGraph_BuildsAdjacencyAndIncidentEdgesFromEdges()
    {
        var ab = new Edge("a", "b");
        var bc = new Edge("b", "c");
        var cc = new Edge("c", "c");

        var graph = new EdgeGraph<string, Edge>(
            new[] { "a", "b", "c", "d" },
            new[] { ab, bc, cc });

        Assert.Multiple(() =>
        {
            Assert.That(graph.Vertices, Is.EqualTo(new[] { "a", "b", "c", "d" }));
            Assert.That(graph.Edges, Is.EqualTo(new[] { ab, bc, cc }));
            Assert.That(graph.GetAdjacentVertices("a"), Is.EqualTo(new[] { "b" }));
            Assert.That(graph.GetAdjacentVertices("b"), Is.EqualTo(new[] { "a", "c" }));
            Assert.That(graph.GetAdjacentVertices("c"), Is.EqualTo(new[] { "b", "c" }));
            Assert.That(graph.GetAdjacentVertices("d"), Is.Empty);
            Assert.That(graph.GetIncidentEdges("a"), Is.EqualTo(new[] { ab }));
            Assert.That(graph.GetIncidentEdges("b"), Is.EqualTo(new[] { ab, bc }));
            Assert.That(graph.GetIncidentEdges("c"), Is.EqualTo(new[] { bc, cc }));
            Assert.That(graph.GetIncidentEdges("d"), Is.Empty);
        });
    }

    [Test]
    public void DirectedEdgeGraph_BuildsDirectedAdjacencyAndEdges()
    {
        var ab = new DirectedEdge("a", "b");
        var cb = new DirectedEdge("c", "b");
        var bb = new DirectedEdge("b", "b");

        var graph = new DirectedEdgeGraph<string, DirectedEdge>(
            new[] { "a", "b", "c", "d" },
            new[] { ab, cb, bb });

        Assert.Multiple(() =>
        {
            Assert.That(graph.Vertices, Is.EqualTo(new[] { "a", "b", "c", "d" }));
            Assert.That(graph.Edges, Is.EqualTo(new[] { ab, cb, bb }));
            Assert.That(graph.GetAdjacentVertices("a"), Is.EqualTo(new[] { "b" }));
            Assert.That(graph.GetOutgoingVertices("a"), Is.EqualTo(new[] { "b" }));
            Assert.That(graph.GetIncomingVertices("b"), Is.EqualTo(new[] { "a", "c", "b" }));
            Assert.That(graph.GetOutgoingVertices("b"), Is.EqualTo(new[] { "b" }));
            Assert.That(graph.GetIncomingEdges("b"), Is.EqualTo(new[] { ab, cb, bb }));
            Assert.That(graph.GetOutgoingEdges("b"), Is.EqualTo(new[] { bb }));
            Assert.That(graph.GetIncidentEdges("b"), Is.EqualTo(new[] { ab, cb, bb }));
            Assert.That(graph.GetIncomingVertices("d"), Is.Empty);
            Assert.That(graph.GetOutgoingVertices("d"), Is.Empty);
        });
    }

    [Test]
    public void Graph_WhenAdjacentVertexIsOutsideGraph_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Graph<string>(
                new[] { "a" },
                new Dictionary<string, IReadOnlyList<string>>
                {
                    ["a"] = new[] { "b" }
                }));
    }

    [Test]
    public void EdgeGraph_WhenEdgeReferencesVertexOutsideGraph_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new EdgeGraph<string, Edge>(
                new[] { "a" },
                new[] { new Edge("a", "b") }));
    }

    private sealed class Edge : IEdge<string>
    {
        public Edge(string firstVertex, string secondVertex)
        {
            FirstVertex = firstVertex;
            SecondVertex = secondVertex;
        }

        public string FirstVertex { get; }

        public string SecondVertex { get; }
    }

    private sealed class DirectedEdge : IDirectedEdge<string>
    {
        public DirectedEdge(string fromVertex, string toVertex)
        {
            FromVertex = fromVertex;
            ToVertex = toVertex;
        }

        public string FirstVertex => FromVertex;

        public string SecondVertex => ToVertex;

        public string FromVertex { get; }

        public string ToVertex { get; }
    }
}
