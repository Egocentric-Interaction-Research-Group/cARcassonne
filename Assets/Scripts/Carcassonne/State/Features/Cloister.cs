using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using QuikGraph;
using UnityEngine;

namespace Carcassonne.State.Features
{
    
    using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;
    using BoardGraphFilter = Func<IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>, IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>>;
    
    public class Cloister : FeatureGraph
    {
        private const int NumSides = 9;
        
        public override int Segments => NumSides - _openSides;

        public override int Points => Segments;

        public override int PotentialPoints => Points;

        public override int OpenSides => _openSides;
        private int _openSides = NumSides;

        public static List<Cloister> FromBoardGraph(BoardGraph bg)
        {
            var cloisters = new List<Cloister>();
            
            // Get a list of cloister vertices
            var cloisterVertices = bg.Vertices.Where(v => v.geography == Geography.Cloister);

            foreach (var cl in cloisterVertices)
            {
                Cloister c = new Cloister();
                
                // Get the verticies within 3 spaces in the x and y directions. 
                var vertices = bg.Vertices.Where(v =>
                                       (v.location - cl.location).sqrMagnitude <= 13);
                var tiles = vertices.Select(v => v.tile).Distinct();
                var tileCount = tiles.Count();

                Debug.Assert( tileCount >= 2, $"There should be at least one connected tile and the original tile. {tileCount} tiles found.");
                Debug.Assert(tileCount <= 9, $"There should not be more than 9 vertices. {tileCount} found.");

                c.AddVertex(cl);
                c._openSides -= tileCount;
                
                cloisters.Add(c);
            }

            return cloisters;
        }
    }
}