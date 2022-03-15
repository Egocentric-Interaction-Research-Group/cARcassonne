using System.Collections.Generic;
using System.Linq;
using QuikGraph;

namespace Carcassonne.State.Features
{
    public interface IFeature
    {
        public bool Complete { get; }
        
        public int Segments
        {
            get;
        }
        
        public int OpenSides
        {
            get;
        }
        
        public int Points
        {
            get;
        }
        
        public int PotentialPoints
        {
            get;
        }

    }
    
    public abstract class FeatureGraph : CarcassonneGraph, IFeature{
        public virtual int Segments => Vertices.Count() - IntraTileFeatureConnections;
        public abstract int Points { get; }
        
        /// <summary>
        /// How many points would the current tiles be worth, if the feature were complete? This captures the expanded
        /// value of city tiles, which double if the city is completed.
        /// </summary>
        public abstract int PotentialPoints { get; }
        public virtual int OpenSides => ComputeOpenSides();
        public virtual bool Complete => OpenSides == 0;

        /// <summary>
        /// A count of the number of vertices representing a single feature on a single tile in the city.
        /// For example, if a city is split on a tile (two ports are disconnected) that tile would return 0
        /// for this measure. But if the NORTH and SOUTH of a tile were a connected city, it would return 1.
        /// </summary>
        protected int IntraTileFeatureConnections =>
            Edges.Count(e => e.Tag == ConnectionType.Feature && e.Source.tile == e.Target.tile);

        private IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>> FeatureEdges => Edges.Where(e => e.Tag == ConnectionType.Feature);
        private IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>> BoardEdges => Edges.Where(e => e.Tag == ConnectionType.Board);

        /// <summary>
        /// Compute the number of open sides as the number of vertices that do not have Board Edges attached to them.
        /// </summary>
        /// <returns></returns>
        private int ComputeOpenSides()
        {
            var sourceBoardVertices = BoardEdges.Select(e => e.Source);
            var targetBoardVertices = BoardEdges.Select(e => e.Target);

            var boardVertices = Enumerable.Concat(sourceBoardVertices, targetBoardVertices);

            return Vertices.Count() - boardVertices.Distinct().Count();
        }
    }
    
}