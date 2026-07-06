using System;
using System.Collections.Generic;

namespace Akeldov.Math.Graphs
{
    /// <summary>
    /// Provides a read-only directed graph backed by copied outgoing-vertex collections.
    /// </summary>
    /// <typeparam name="TVertex">The directed graph vertex type.</typeparam>
    public sealed class DirectedGraph<TVertex> : IDirectedGraph<TVertex>
        where TVertex : notnull
    {
        private static readonly IReadOnlyList<TVertex> EmptyVertices = Array.AsReadOnly(Array.Empty<TVertex>());

        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> _incomingVertices;
        private readonly IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> _outgoingVertices;

        /// <summary>
        /// Initializes a new read-only directed graph from vertices and outgoing-vertex collections.
        /// </summary>
        /// <param name="vertices">The vertices in the graph.</param>
        /// <param name="outgoingVertices">The outgoing vertices keyed by graph vertex.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vertices"/>, <paramref name="outgoingVertices"/>, or one of their entries is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a vertex is duplicated, or an outgoing-vertex entry references a vertex outside the graph.
        /// </exception>
        public DirectedGraph(
            IReadOnlyCollection<TVertex> vertices,
            IReadOnlyDictionary<TVertex, IReadOnlyList<TVertex>> outgoingVertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            if (outgoingVertices == null)
                throw new ArgumentNullException(nameof(outgoingVertices));

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

            var incomingBuilders = new Dictionary<TVertex, List<TVertex>>(vertexSet.Count);
            var outgoingCopy = new Dictionary<TVertex, IReadOnlyList<TVertex>>(vertexSet.Count);
            foreach (TVertex vertex in vertexList)
            {
                incomingBuilders.Add(vertex, new List<TVertex>());
                outgoingCopy.Add(vertex, EmptyVertices);
            }

            foreach (KeyValuePair<TVertex, IReadOnlyList<TVertex>> pair in outgoingVertices)
            {
                if (pair.Key is null)
                    throw new ArgumentNullException(nameof(outgoingVertices));

                if (!vertexSet.Contains(pair.Key))
                    throw new ArgumentException("Outgoing-vertex keys must be vertices in the graph.", nameof(outgoingVertices));

                if (pair.Value == null)
                    throw new ArgumentNullException(nameof(outgoingVertices));

                var outgoingArray = new TVertex[pair.Value.Count];
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    TVertex outgoingVertex = pair.Value[i];
                    if (outgoingVertex is null)
                        throw new ArgumentNullException(nameof(outgoingVertices));

                    if (!vertexSet.Contains(outgoingVertex))
                        throw new ArgumentException("Outgoing vertices must be vertices in the graph.", nameof(outgoingVertices));

                    outgoingArray[i] = outgoingVertex;
                    incomingBuilders[outgoingVertex].Add(pair.Key);
                }

                outgoingCopy[pair.Key] = Array.AsReadOnly(outgoingArray);
            }

            var incomingCopy = new Dictionary<TVertex, IReadOnlyList<TVertex>>(vertexSet.Count);
            foreach (TVertex vertex in vertexList)
            {
                incomingCopy.Add(vertex, Array.AsReadOnly(incomingBuilders[vertex].ToArray()));
            }

            Vertices = Array.AsReadOnly(vertexList.ToArray());
            _incomingVertices = incomingCopy;
            _outgoingVertices = outgoingCopy;
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
    }
}
