using System;
using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Provides a read-only graph backed by copied adjacency collections.
    /// </summary>
    /// <typeparam name="TVertex">The graph vertex type.</typeparam>
    public sealed class Graph<TVertex> : IGraph<TVertex>
        where TVertex : notnull
    {
        private static readonly IReadOnlyList<TVertex> EmptyVertices = Array.AsReadOnly(Array.Empty<TVertex>());

        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> _adjacentVertices;

        /// <summary>
        /// Initializes a new read-only graph from vertices and adjacent-vertex collections.
        /// </summary>
        /// <param name="vertices">The vertices in the graph.</param>
        /// <param name="adjacentVertices">The adjacent vertices keyed by graph vertex.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/>, <paramref name="adjacentVertices"/>, or one of their entries is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a vertex is duplicated, or an adjacency entry references a vertex outside the graph.
        /// </exception>
        public Graph(
            IReadOnlyCollection<TVertex> vertices,
            IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> adjacentVertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            if (adjacentVertices == null)
                throw new ArgumentNullException(nameof(adjacentVertices));

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

            var adjacentCopy = new Dictionary<TVertex, IReadOnlyList<TVertex>>(vertexSet.Count);
            foreach (TVertex vertex in vertexList)
            {
                adjacentCopy.Add(vertex, EmptyVertices);
            }

            foreach (KeyValuePair<TVertex, IReadOnlyList<TVertex>> pair in adjacentVertices)
            {
                if (pair.Key is null)
                    throw new ArgumentNullException(nameof(adjacentVertices));

                if (!vertexSet.Contains(pair.Key))
                    throw new ArgumentException("Adjacency keys must be vertices in the graph.", nameof(adjacentVertices));

                if (pair.Value == null)
                    throw new ArgumentNullException(nameof(adjacentVertices));

                var adjacentArray = new TVertex[pair.Value.Count];
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    TVertex adjacentVertex = pair.Value[i];
                    if (adjacentVertex is null)
                        throw new ArgumentNullException(nameof(adjacentVertices));

                    if (!vertexSet.Contains(adjacentVertex))
                        throw new ArgumentException("Adjacent vertices must be vertices in the graph.", nameof(adjacentVertices));

                    adjacentArray[i] = adjacentVertex;
                }

                adjacentCopy[pair.Key] = Array.AsReadOnly(adjacentArray);
            }

            Vertices = Array.AsReadOnly(vertexList.ToArray());
            _adjacentVertices = adjacentCopy;
        }

        /// <summary>
        /// Gets the read-only structural collection of vertices in the graph.
        /// </summary>
        public IReadOnlyCollection<TVertex> Vertices { get; }

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
    }
}
