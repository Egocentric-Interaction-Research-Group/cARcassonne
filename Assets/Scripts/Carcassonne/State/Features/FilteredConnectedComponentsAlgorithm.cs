using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.ConnectedComponents
{
    /// <summary>
    /// Algorithm that computes connected components of a graph.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public sealed class FilteredConnectedComponentsAlgorithm<TVertex, TEdge>
        : AlgorithmBase<IUndirectedGraph<TVertex, TEdge>>
        , IConnectedComponentAlgorithm<TVertex, TEdge, IUndirectedGraph<TVertex, TEdge>>
        where TEdge : IEdge<TVertex>
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        public FilteredConnectedComponentsAlgorithm([NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            Func<IEnumerable<TEdge>, IEnumerable<TEdge>> adjacentEdgesFilter)
            : this(null, visitedGraph, new Dictionary<TVertex, int>(), adjacentEdgesFilter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="components">Graph components.</param>
        public FilteredConnectedComponentsAlgorithm(
            [NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IDictionary<TVertex, int> components,
            Func<IEnumerable<TEdge>, IEnumerable<TEdge>> adjacentEdgesFilter)
            : this(null, visitedGraph, components, adjacentEdgesFilter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedComponentsAlgorithm{TVertex,TEdge}"/> class.
        /// </summary>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="components">Graph components.</param>
        public FilteredConnectedComponentsAlgorithm(
            [CanBeNull] IAlgorithmComponent host,
            [NotNull] IUndirectedGraph<TVertex, TEdge> visitedGraph,
            [NotNull] IDictionary<TVertex, int> components,
            [NotNull] Func<IEnumerable<TEdge>, IEnumerable<TEdge>> adjacentEdgesFilter)
            : base(host, visitedGraph)
        {
            Components = components ?? throw new ArgumentNullException(nameof(components));
            AdjacentEdgesFilter = adjacentEdgesFilter ?? throw new ArgumentNullException(nameof(adjacentEdgesFilter));
        }

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            Components.Clear();
            ComponentCount = 0;
        }

        /// <inheritdoc />
        protected override void InternalCompute()
        {
            if (VisitedGraph.VertexCount == 0)
                return;

            ComponentCount = -1;
            UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge> dfs = null;
            try
            {
                dfs = new UndirectedDepthFirstSearchAlgorithm<TVertex, TEdge>(
                    this,
                    VisitedGraph,
                    new Dictionary<TVertex, GraphColor>(VisitedGraph.VertexCount),
                    AdjacentEdgesFilter);

                dfs.StartVertex += OnStartVertex;
                dfs.DiscoverVertex += OnVertexDiscovered;
                dfs.Compute();
                ++ComponentCount;
            }
            finally
            {
                if (dfs != null)
                {
                    dfs.StartVertex -= OnStartVertex;
                    dfs.DiscoverVertex -= OnVertexDiscovered;
                }
            }
        }

        #endregion

        #region IConnectedComponentAlgorithm<TVertex,TEdge,TGraph>

        /// <inheritdoc />
        public int ComponentCount { get; private set; }

        /// <inheritdoc />
        public IDictionary<TVertex, int> Components { get; }

        #endregion
        
        /// <inheritdoc />
        public Func<IEnumerable<TEdge>, IEnumerable<TEdge>> AdjacentEdgesFilter { get; }
        

        private void OnStartVertex([NotNull] TVertex vertex)
        {
            ++ComponentCount;
        }

        private void OnVertexDiscovered([NotNull] TVertex vertex)
        {
            Components[vertex] = ComponentCount;
        }

        private static Func<IEnumerable<TEdge>, IEnumerable<TEdge>> NullFilter = edges => edges;
    }
}