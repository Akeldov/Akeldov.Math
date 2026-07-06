using System;
using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Provides a read-only directed edge graph backed by copied vertex and edge collections.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    /// <typeparam name="TEdge">The directed edge type.</typeparam>
    public sealed class DirectedEdgeGraph<TVertex, TEdge> : IDirectedEdgeGraph<TVertex, TEdge>
        where TVertex : notnull
        where TEdge : IDirectedEdge<TVertex>
    {
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TEdge>> _incidentEdges;
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TEdge>> _incomingEdges;
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> _incomingVertices;
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TEdge>> _outgoingEdges;
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> _outgoingVertices;

        /// <summary>
        /// Initializes a new read-only directed edge graph from vertices and directed edges.
        /// </summary>
        /// <param name="vertices">The vertices in the graph.</param>
        /// <param name="edges">The directed edges in the graph.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/>, <paramref name="edges"/>, or one of their entries is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a vertex is duplicated, or an edge references a vertex outside the graph.
        /// </exception>
        public DirectedEdgeGraph(IReadOnlyCollection<TVertex> vertices, IReadOnlyCollection<TEdge> edges)
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

            var incidentEdgeBuilders = new Dictionary<TVertex, List<TEdge>>(vertexSet.Count);
            var incomingEdgeBuilders = new Dictionary<TVertex, List<TEdge>>(vertexSet.Count);
            var incomingVertexBuilders = new Dictionary<TVertex, List<TVertex>>(vertexSet.Count);
            var outgoingEdgeBuilders = new Dictionary<TVertex, List<TEdge>>(vertexSet.Count);
            var outgoingVertexBuilders = new Dictionary<TVertex, List<TVertex>>(vertexSet.Count);
            foreach (TVertex vertex in vertexList)
            {
                incidentEdgeBuilders.Add(vertex, new List<TEdge>());
                incomingEdgeBuilders.Add(vertex, new List<TEdge>());
                incomingVertexBuilders.Add(vertex, new List<TVertex>());
                outgoingEdgeBuilders.Add(vertex, new List<TEdge>());
                outgoingVertexBuilders.Add(vertex, new List<TVertex>());
            }

            var edgeList = new List<TEdge>(edges.Count);
            var comparer = EqualityComparer<TVertex>.Default;
            foreach (TEdge edge in edges)
            {
                if (edge is null)
                    throw new ArgumentNullException(nameof(edges));

                TVertex fromVertex = edge.FromVertex;
                TVertex toVertex = edge.ToVertex;

                if (fromVertex is null || toVertex is null)
                    throw new ArgumentNullException(nameof(edges));

                if (!vertexSet.Contains(fromVertex) || !vertexSet.Contains(toVertex))
                    throw new ArgumentException("Edges must reference vertices in the graph.", nameof(edges));

                edgeList.Add(edge);
                outgoingEdgeBuilders[fromVertex].Add(edge);
                outgoingVertexBuilders[fromVertex].Add(toVertex);
                incomingEdgeBuilders[toVertex].Add(edge);
                incomingVertexBuilders[toVertex].Add(fromVertex);
                incidentEdgeBuilders[fromVertex].Add(edge);

                if (!comparer.Equals(fromVertex, toVertex))
                {
                    incidentEdgeBuilders[toVertex].Add(edge);
                }
            }

            _incidentEdges = CopyEdgeLists(vertexList, incidentEdgeBuilders);
            _incomingEdges = CopyEdgeLists(vertexList, incomingEdgeBuilders);
            _incomingVertices = CopyVertexLists(vertexList, incomingVertexBuilders);
            _outgoingEdges = CopyEdgeLists(vertexList, outgoingEdgeBuilders);
            _outgoingVertices = CopyVertexLists(vertexList, outgoingVertexBuilders);
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
        public IReadOnlyList<TVertex> GetAdjacentVertices(TVertex vertex) => GetOutgoingVertices(vertex);

        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose incoming vertices should be returned.</param>
        /// <returns>The vertices with edges directed into <paramref name="vertex"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> is not in this graph.</exception>
        public IReadOnlyList<TVertex> GetIncomingVertices(TVertex vertex)
        {
            if (vertex is null)
                throw new ArgumentNullException(nameof(vertex));

            if (!_incomingVertices.TryGetValue(vertex, out IReadOnlyList<TVertex>? incomingVertices))
                throw new ArgumentException("The vertex is not in this graph.", nameof(vertex));

            return incomingVertices;
        }

        /// <summary>
        /// Gets the read-only structural collection of vertices with edges directed out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose outgoing vertices should be returned.</param>
        /// <returns>The vertices with edges directed out of <paramref name="vertex"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> is not in this graph.</exception>
        public IReadOnlyList<TVertex> GetOutgoingVertices(TVertex vertex)
        {
            if (vertex is null)
                throw new ArgumentNullException(nameof(vertex));

            if (!_outgoingVertices.TryGetValue(vertex, out IReadOnlyList<TVertex>? outgoingVertices))
                throw new ArgumentException("The vertex is not in this graph.", nameof(vertex));

            return outgoingVertices;
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

        /// <summary>
        /// Gets the read-only structural collection of edges directed into the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose incoming edges should be returned.</param>
        /// <returns>The edges directed into <paramref name="vertex"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> is not in this graph.</exception>
        public IReadOnlyList<TEdge> GetIncomingEdges(TVertex vertex)
        {
            if (vertex is null)
                throw new ArgumentNullException(nameof(vertex));

            if (!_incomingEdges.TryGetValue(vertex, out IReadOnlyList<TEdge>? incomingEdges))
                throw new ArgumentException("The vertex is not in this graph.", nameof(vertex));

            return incomingEdges;
        }

        /// <summary>
        /// Gets the read-only structural collection of edges directed out of the specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex whose outgoing edges should be returned.</param>
        /// <returns>The edges directed out of <paramref name="vertex"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> is not in this graph.</exception>
        public IReadOnlyList<TEdge> GetOutgoingEdges(TVertex vertex)
        {
            if (vertex is null)
                throw new ArgumentNullException(nameof(vertex));

            if (!_outgoingEdges.TryGetValue(vertex, out IReadOnlyList<TEdge>? outgoingEdges))
                throw new ArgumentException("The vertex is not in this graph.", nameof(vertex));

            return outgoingEdges;
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
