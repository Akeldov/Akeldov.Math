using System;
using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Provides a read-only edge graph backed by copied vertex and edge collections.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The edge type.</typeparam>
    public sealed class EdgeGraph<TVertex, TEdge> : IEdgeGraph<TVertex, TEdge>
        where TVertex : notnull
        where TEdge : IEdge<TVertex>
    {
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> _adjacentVertices;
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TEdge>> _incidentEdges;

        /// <summary>
        /// Initializes a new read-only edge graph from vertices and edges.
        /// </summary>
        /// <param name="vertices">The vertices in the graph.</param>
        /// <param name="edges">The edges in the graph.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/>, <paramref name="edges"/>, or one of their entries is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a vertex is duplicated, or an edge references a vertex outside the graph.
        /// </exception>
        public EdgeGraph(IReadOnlyCollection<TVertex> vertices, IReadOnlyCollection<TEdge> edges)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            var vertexSet = new HashSet<TVertex>();
            var vertexList = new List<TVertex>(vertices.Count);
            foreach (TVertex vertex in vertices)
            {
                if (vertex is null)
                    throw new ArgumentNullException(nameof(vertices));

                if (!vertexSet.Add(vertex))
                    throw new ArgumentException("The graph cannot contain duplicate vertices.", nameof(vertices));

                vertexList.Add(vertex);
            }

            var adjacentBuilders = new Dictionary<TVertex, List<TVertex>>(vertexSet.Count);
            var incidentBuilders = new Dictionary<TVertex, List<TEdge>>(vertexSet.Count);
            foreach (TVertex vertex in vertexList)
            {
                adjacentBuilders.Add(vertex, new List<TVertex>());
                incidentBuilders.Add(vertex, new List<TEdge>());
            }

            var edgeList = new List<TEdge>(edges.Count);
            var comparer = EqualityComparer<TVertex>.Default;
            foreach (TEdge edge in edges)
            {
                if (edge is null)
                    throw new ArgumentNullException(nameof(edges));

                TVertex firstVertex = edge.FirstVertex;
                TVertex secondVertex = edge.SecondVertex;

                if (firstVertex is null || secondVertex is null)
                    throw new ArgumentNullException(nameof(edges));

                if (!vertexSet.Contains(firstVertex) || !vertexSet.Contains(secondVertex))
                    throw new ArgumentException("Edges must reference vertices in the graph.", nameof(edges));

                edgeList.Add(edge);
                adjacentBuilders[firstVertex].Add(secondVertex);
                incidentBuilders[firstVertex].Add(edge);

                if (!comparer.Equals(firstVertex, secondVertex))
                {
                    adjacentBuilders[secondVertex].Add(firstVertex);
                    incidentBuilders[secondVertex].Add(edge);
                }
            }

            _adjacentVertices = CopyVertexLists(vertexList, adjacentBuilders);
            _incidentEdges = CopyEdgeLists(vertexList, incidentBuilders);
            Vertices = Array.AsReadOnly(vertexList.ToArray());
            Edges = Array.AsReadOnly(edgeList.ToArray());
        }

        /// <summary>
        /// Gets the read-only structural collection of vertices in the graph.
        /// </summary>
        public IReadOnlyCollection<TVertex> Vertices { get; }

        /// <summary>
        /// Gets the read-only structural collection of edges in the graph.
        /// </summary>
        public IReadOnlyCollection<TEdge> Edges { get; }

        /// <summary>
        /// Gets the read-only structural collection of vertices adjacent to the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose adjacent vertices should be returned.</param>
        /// <returns>The vertices adjacent to <paramref name="vertex"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> is not in this graph.</exception>
        public IReadOnlyList<TVertex> GetAdjacentVertices(TVertex vertex)
        {
            if (vertex is null)
                throw new ArgumentNullException(nameof(vertex));

            if (!_adjacentVertices.TryGetValue(vertex, out IReadOnlyList<TVertex>? adjacentVertices))
                throw new ArgumentException("The vertex is not in this graph.", nameof(vertex));

            return adjacentVertices;
        }

        /// <summary>
        /// Gets the read-only structural collection of edges incident to the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose incident edges should be returned.</param>
        /// <returns>The edges incident to <paramref name="vertex"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> is not in this graph.</exception>
        public IReadOnlyList<TEdge> GetIncidentEdges(TVertex vertex)
        {
            if (vertex is null)
                throw new ArgumentNullException(nameof(vertex));

            if (!_incidentEdges.TryGetValue(vertex, out IReadOnlyList<TEdge>? incidentEdges))
                throw new ArgumentException("The vertex is not in this graph.", nameof(vertex));

            return incidentEdges;
        }

        private static IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> CopyVertexLists(
            IReadOnlyList<TVertex> vertices,
            IReadOnlyDictionary<TVertex, List<TVertex>> lists)
        {
            var copy = new Dictionary<TVertex, IReadOnlyList<TVertex>>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                TVertex vertex = vertices[i];
                copy.Add(vertex, Array.AsReadOnly(lists[vertex].ToArray()));
            }

            return copy;
        }

        private static IReadOnlyDictionary<TVertex, IReadOnlyList<TEdge>> CopyEdgeLists(
            IReadOnlyList<TVertex> vertices,
            IReadOnlyDictionary<TVertex, List<TEdge>> lists)
        {
            var copy = new Dictionary<TVertex, IReadOnlyList<TEdge>>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                TVertex vertex = vertices[i];
                copy.Add(vertex, Array.AsReadOnly(lists[vertex].ToArray()));
            }

            return copy;
        }
    }
}
