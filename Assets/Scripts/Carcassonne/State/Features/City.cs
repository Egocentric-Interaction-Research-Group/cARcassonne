using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms.ConnectedComponents;

namespace Carcassonne.State.Features
{
    using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;
    using BoardGraphFilter = Func<IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>, IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>>;
    
    public class City : FeatureGraph
    {
        public override int Segments => Vertices.Count() - IntraTileFeatureConnections;
        public override int OpenSides => ComputeOpenSides();

        public int Shields => Vertices.Select(v=> v.tile).Distinct().Count(t=> t.Shield);
        public override bool Complete => OpenSides == 0;
        
        /// <summary>
        /// A count of the number of vertices representing a single feature on a single tile in the city.
        /// For example, if a city is split on a tile (two ports are disconnected) that tile would return 0
        /// for this measure. But if the NORTH and SOUTH of a tile were a connected city, it would return 1.
        /// </summary>
        private int IntraTileFeatureConnections =>
            Edges.Count(e => e.Tag == ConnectionType.Feature && e.Source.tile == e.Target.tile);

        private IEnumerable<CarcassonneEdge> FeatureEdges => Edges.Where(e => e.Tag == ConnectionType.Feature);
        private IEnumerable<CarcassonneEdge> BoardEdges => Edges.Where(e => e.Tag == ConnectionType.Board);

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

        public static BoardGraphFilter CityFilter = edges =>
            edges.Where(e =>
            {
                var featureEdge = e.Tag == ConnectionType.Feature;
                var cityVertices = e.Source.geography == Geography.City &&
                                   e.Target.geography == Geography.City;
                return featureEdge && cityVertices;
            });

        public static List<City> FromBoardGraph(BoardGraph bg)
        {
            var cities = new List<City>();
            
            FilteredConnectedComponentsAlgorithm<SubTile, CarcassonneEdge> connectedComponents =
                new FilteredConnectedComponentsAlgorithm<SubTile, CarcassonneEdge>(bg, CityFilter);
            
            // This should still contain non-city nodes, they should just be isolated.
            connectedComponents.Compute();
            // Filter non-City subtiles here.
            var components = connectedComponents.Components.Where(kvp => kvp.Key.geography == Geography.City);
            
            var cityVertices = components.ToLookup(p => p.Value, p=>p.Key);

            foreach (var cityVertexGroup in cityVertices)
            {
                var city = City.CityFromVertexList(bg, cityVertices[cityVertexGroup.Key]);
                cities.Add(city);
            }

            return cities;
        }
    
        public static City CityFromVertexList(BoardGraph bg, IEnumerable<SubTile> vertices)
        {
            City city = new City();
            
            Debug.Assert(vertices.All(v=> v.geography == Geography.City));

            city.AddVertexRange(vertices);

            // Edges that are between two vertices that are part of this city and are Feature/Board edges.
            var tileEdges = bg.Edges.Where(e => e.Tag != ConnectionType.Tile &&
                                city.Vertices.Contains(e.Source) && city.Vertices.Contains(e.Target));
            city.AddEdgeRange(tileEdges);

            return city;
        }
        
        public override int Points => getPoints();

        private int getPoints()
        {
            int segments = Vertices.Count() - IntraTileFeatureConnections;
            int points = segments + Shields;
            
            return Complete ? points * 2 : points;
        }

    }
}