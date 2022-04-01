using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using UnityEngine;
using QuikGraph;

namespace Carcassonne.State.Features
{
    // using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;
    using BoardGraphFilter = Func<IEnumerable<CarcassonneEdge>, IEnumerable<CarcassonneEdge>>;
    
    public class Road : FeatureGraph
    {
        public static BoardGraphFilter RoadFilter = edges =>
            edges.Where(e =>
            {
                var featureEdge = e.Tag == ConnectionType.Feature;
                var roadVertices = e.Source.geography == Geography.Road &&
                                   e.Target.geography == Geography.Road;
                return featureEdge && roadVertices;
            });

        public static List<Road> FromBoardGraph(BoardGraph bg)
        {
            var roads = new List<Road>();
            
            FilteredConnectedComponentsAlgorithm<SubTile, CarcassonneEdge> connectedComponents =
                new FilteredConnectedComponentsAlgorithm<SubTile, CarcassonneEdge>(bg, RoadFilter);
            
            // This should still contain non-road nodes, they should just be isolated.
            connectedComponents.Compute();
            // Filter non-Road subtiles here.
            var components = connectedComponents.Components.Where(kvp => kvp.Key.geography == Geography.Road);
            
            var roadVertices = components.ToLookup(p => p.Value, p=>p.Key);

            foreach (var roadVertexGroup in roadVertices)
            {
                var road = Road.RoadFromVertexList(bg, roadVertices[roadVertexGroup.Key]);
                roads.Add(road);
            }

            return roads;
        }
    
        public static Road RoadFromVertexList(BoardGraph bg, IEnumerable<SubTile> vertices)
        {
            Road road = new Road();
            
            Debug.Assert(vertices.All(v=> v.geography == Geography.Road));

            road.AddVertexRange(vertices);

            // Edges that are between two vertices that are part of this road and are Feature/Board edges.
            var tileEdges = bg.Edges.Where(e => e.Tag != ConnectionType.Tile &&
                                road.Vertices.Contains(e.Source) && road.Vertices.Contains(e.Target));
            road.AddEdgeRange(tileEdges);

            return road;
        }
        
        public override int Points => Vertices.Count() - IntraTileFeatureConnections;

        public override int PotentialPoints => Points;

    }
}